using System;
using System.Linq;
using System.Threading;

namespace RustFreeVM {
    partial class Cpu {
        public const ushort PC_START = 0x0000;
        public const ushort SP_START = 0xFFFF;

        private Bus bus;
        private byte a, b, flags;
        private ushort x, y, pc, sp;

        public Cpu() {
            a = b = flags = 0;
            x = y = 0;
            pc = PC_START;
            sp = SP_START;

            bus = Bus.getInstance();
        }

        public void Cycle() {
            Instruction instruction = fetch();
            execute(instruction);
        }

        public void Start() {
            Thread t = new Thread(() => {
                while (true) Cycle();
            });
            t.Start();
        }

        /// <summary>
        /// Fetch an instruction from memory
        /// </summary>
        /// <returns>The instruction</returns>
        public Instruction fetch() {
            Instruction instruction = new Instruction();

            // Read opcode
            instruction.Opcode = bus.WaitOnCommand(new Bus.Command(
                Memory.DEVICE_ID, (byte)Memory.Commands.ReadByte,
                pc, null
            )).Byte();

            // Read operands
            ushort address = (ushort) (pc + 1);
            foreach (var n in Enumerable.Range(0, instruction.OperandsRequired())) {
                Operand operand = new Operand();
                // Read operand type
                operand.Type = bus.WaitOnCommand(new Bus.Command(
                    Memory.DEVICE_ID, (byte)Memory.Commands.ReadByte,
                    address, null
                )).Byte();

                // For direct referencing
                ushort direct_address = 0;
                if (operand.Type == (byte)Operand.Types.Direct || operand.Type == (byte)Operand.Types.DirectW || operand.Type == (byte)Operand.Types.Indirect || operand.Type == (byte)Operand.Types.IndirectW) {
                    direct_address = bus.WaitOnCommand(new Bus.Command(
                        Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                        (ushort)(address + 1), null
                    )).Word();
                }

                // For indirect referencing
                ushort indirect_address = 0;
                if (operand.Type == (byte)Operand.Types.Indirect || operand.Type == (byte)Operand.Types.IndirectW) {
                    indirect_address = bus.WaitOnCommand(new Bus.Command(
                        Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                        direct_address, null
                    )).Word();
                }
                
                switch (operand.Type) {
                    // Single byte
                    case (byte)Operand.Types.Register:
                    case (byte)Operand.Types.Static:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadByte,
                            (ushort)(address + 1), null
                        ));
                        address += 2;
                        break;
                    
                    // Single word
                    case (byte)Operand.Types.StaticW:
                    case (byte)Operand.Types.Direct:
                    case (byte)Operand.Types.DirectW:
                    case (byte)Operand.Types.Indirect:
                    case (byte)Operand.Types.IndirectW:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                            (ushort)(address + 1), null
                        ));
                        address += 3;
                        break;

                    default:
                        panic("Unknown operand type encountered");
                        break;
                }

                if (operand.Type == (byte)Operand.Types.Indirect || operand.Type == (byte)Operand.Types.IndirectW) {
                    operand.Value = bus.WaitOnCommand(new Bus.Command(
                        Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                        operand.Value.Word(), null
                    ));
                }

                instruction.Operands.Add(operand);
            }

            pc = address;
            return instruction;
        }

        /// <summary>
        /// Execute an instruction
        /// </summary>
        /// <param name="instruction">Instruction to execute</param>
        public void execute(Instruction instruction) {
            switch (instruction.Opcode) {
                case (byte)Instruction.Operators.NOP:
                    break;

                case (byte)Instruction.Operators.HLT:
                    HLT();
                    break;

                case (byte)Instruction.Operators.MOV:
                    MOV(instruction.Operands[0], instruction.Operands[1]);
                    break;

                default:
                    panic("Unknown operator encountered");
                    break;
            }
        }

        /// <summary>
        /// Get a register's value
        /// </summary>
        /// <param name="register">Registr ID</param>
        /// <returns>The current register value</returns>
        public Value getRegister(byte register) {
            switch (register) {
                case (byte)Registers.A:
                    return new Value(a);
                case (byte)Registers.B:
                    return new Value(b);
                case (byte)Registers.X:
                    return new Value(x);
                case (byte)Registers.Y:
                    return new Value(y);
                case (byte)Registers.PC:
                    return new Value(pc);
                case (byte)Registers.SP:
                    return new Value(sp);

                default:
                    panic("Unknown register encountered");
                    return null;
            }
        }

        /// <summary>
        /// Set the value of a register
        /// </summary>
        /// <param name="register">Register ID</param>
        /// <param name="value">Value to set</param>
        public void setRegister(byte register, Value value) {
            switch (register) {
                case (byte)Registers.A:
                    a = value.Byte();
                    break;
                case (byte)Registers.B:
                    b = value.Byte();
                    break;
                case (byte)Registers.X:
                    x = value.Byte();
                    break;
                case (byte)Registers.Y:
                    y = value.Byte();
                    break;
                case (byte)Registers.PC:
                    pc = value.Byte();
                    break;
                case (byte)Registers.SP:
                    sp = value.Byte();
                    break;

                default:
                    panic("Unknown register encountered");
                    break;
            }
        }

        /// <summary>
        /// Resolve an instruction to value mode
        /// </summary>
        /// <param name="operand">The raw operand</param>
        public void resolve(Operand operand) {
            if (operand.Type == (byte)Operand.Types.Register) {
                // Correct for register sources
                operand.Value = getRegister((byte)operand.Type);
            } else if (operand.Type == (byte)Operand.Types.Direct || operand.Type == (byte)Operand.Types.DirectW || operand.Type == (byte)Operand.Types.Indirect || operand.Type == (byte)Operand.Types.IndirectW) {
                // Memory based operands
                operand.Value = bus.WaitOnCommand(new Bus.Command(
                        Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                        operand.Value.Word(), null
                    ));
            }
        }

        /// <summary>
        /// Stop execution on an error
        /// </summary>
        /// <param name="msg"></param>
        public void panic(string msg) {
            Console.WriteLine("SYSTEM ERROR: " + msg);
            System.Environment.Exit(1);
        }

        /// <summary>
        /// Represents the identifiers for registers
        /// </summary>
        public enum Registers {
            A=0x00, B=0x01, X=0x02, Y=0x03,
            PC=0x04, SP=0x05
        }

        /// <summary>
        /// Masks for the various state flags
        /// </summary>
        public enum Flags {
            Carry = 0x01,
            Parity = 0x02,
            Zero = 0x04,
            Sign = 0x08,
            Interrupt = 0x10,
            Overflow = 0x20
        }
    }
}
