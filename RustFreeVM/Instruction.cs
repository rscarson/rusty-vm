using System.Collections.Generic;

namespace RustFreeVM {
    class Instruction {
        public byte Opcode { get; set; }
        public List<Operand> Operands;

        public Instruction() {
            Operands = new List<Operand>();
        }

        public enum Operators {
            NOP=0x00, MOV=0x01, 
            ADD=0x10, SUB=0x11, MUL=0x12, DIV=0x13, IMUL=0x14, IDIV=0x15, MOD=0x16, AVG=0x17, DEC=0x18, INC=0x19, NEG=0x1A, 
            ROTL=0x20, ROTR=0x21, SHFL=0x22, SHFR=0x23, ASHFL=0x24, ASHFR=0x25, 
            NOT=0x30, OR=0x31, AND=0x32, XOR=0x33, 
            CLF=0x40, CLC=0x41, CLP=0x42, CLZ=0x43, CLS=0x44, CLI=0x45, CLO=0x46, STC=0x47, STP=0x48, STZ=0x49, STS=0x4A, STI=0x4B, STO=0x4C, 
            CALL=0x50, RET=0x51, PUSH=0x52, POP=0x53, PUSHF=0x54, POPF=0x55, 
            CMP=0x60, JMP=0x61, JZ=0x62, JN=0x63, JE=0x64, JNE=0x65, 
        }

        public int OperandsRequired() {
            switch (Opcode) {
                case (byte)Operators.DEC:     case (byte)Operators.INC:
                case (byte)Operators.NEG:     case (byte)Operators.ROTL:
                case (byte)Operators.ROTR:    case (byte)Operators.SHFL:
                case (byte)Operators.SHFR:    case (byte)Operators.ASHFL:
                case (byte)Operators.ASHFR:   case (byte)Operators.NOT:
                case (byte)Operators.STC:     case (byte)Operators.STP:
                case (byte)Operators.STZ:     case (byte)Operators.STS:
                case (byte)Operators.STI:     case (byte)Operators.STO:
                case (byte)Operators.JMP:     case (byte)Operators.JZ:
                case (byte)Operators.JN:      case (byte)Operators.CALL:
                case (byte)Operators.PUSH:    case (byte)Operators.POP:
                case (byte)Operators.PUSHF:   case (byte)Operators.POPF:
                    return 1;

                case (byte)Operators.ADD:     case (byte)Operators.SUB:
                case (byte)Operators.MUL:     case (byte)Operators.DIV:
                case (byte)Operators.IMUL:    case (byte)Operators.IDIV:
                case (byte)Operators.MOD:     case (byte)Operators.AVG:
                case (byte)Operators.OR:      case (byte)Operators.AND:
                case (byte)Operators.XOR:     case (byte)Operators.MOV:
                case (byte)Operators.CMP:     case (byte)Operators.JE:
                case (byte)Operators.JNE:
                    return 2;

                default:
                    return 0;
            }
        }
    }

    class Operand {
        public byte Type { get; set; }
        public Value Value { get; set; }

        public enum Types {
            Register = 0x0,
            Static = 0x01, StaticW = 0x02,
            Direct = 0x03, Indirect = 0x04,
            DirectW = 0x05, IndirectW = 0x06
        }

        public bool isWide() {
            return (Type == (byte)Types.StaticW || Type == (byte)Types.DirectW || Type == (byte)Types.IndirectW);
        }
    }
}
