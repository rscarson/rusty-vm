using System;
using System.Linq;
using System.Threading;

namespace RustFreeVM {
    partial class Cpu {
        public const ushort PC_START = 0x0000;
        public const ushort SP_START = 0xFFFF;

        private Bus bus;
        private byte a, b, flag;
        private ushort x, y, pc, sp;

        public Cpu() {
            a = b = flag = 0;
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
                    // Single byte in memory
                    case (byte)Operand.Types.Register:
                    case (byte)Operand.Types.Static:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadByte,
                            (ushort)(address + 1), null
                        ));
                        address += 2;
                        break;
                    
                    // Single word in memory
                    case (byte)Operand.Types.StaticW:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                            (ushort)(address + 1), null
                        ));
                        address += 3;
                        break;

                    // Single byte at an address
                    case (byte)Operand.Types.Direct:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadByte,
                            direct_address, null
                        ));
                        address += 3;
                        break;

                    // Single word at an address
                    case (byte)Operand.Types.DirectW:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                            direct_address, null
                        ));
                        address += 3;
                        break;

                    // Single byte at an indirect address
                    case (byte)Operand.Types.Indirect:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadByte,
                            direct_address, null
                        ));
                        address += 3;
                        break;

                    // Single word at an indirect address
                    case (byte)Operand.Types.IndirectW:
                        operand.Value = bus.WaitOnCommand(new Bus.Command(
                            Memory.DEVICE_ID, (byte)Memory.Commands.ReadWord,
                            direct_address, null
                        ));
                        address += 3;
                        break;

                    default:
                        panic("Unknown operand type encountered");
                        break;
                }

                instruction.Operands.Add(operand);
            }

            pc = address;
            return instruction;
        }

        public void execute(Instruction instruction) {
            switch (instruction.Opcode) {
                case (byte)Instruction.Operators.NOP:
                    break;

                case (byte)Instruction.Operators.MOV:
                    MOV(instruction.Operands);
                    break;

                default:
                    panic("Unknown operator encountered");
                    break;
            }
        }

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

        public void panic(string msg) {
            Console.WriteLine("SYSTEM ERROR: " + msg);
            System.Environment.Exit(1);
        }

        public enum Registers {
            A=0x00, B=0x01, X=0x02, Y=0x03,
            PC=0x04, SP=0x05
        }

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
