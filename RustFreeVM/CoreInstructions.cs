using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustFreeVM {
    partial class Cpu {
        /// <summary>
        /// Stop execution
        /// </summary>
        public void HLT() {
            System.Environment.Exit(0);
        }

        /// <summary>
        /// Move data from place to place
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="src">Source</param>
        public void MOV(Operand dst, Operand src) {
            resolve(src);

            if (dst.Type == (byte)Operand.Types.Register) {
                // Destination is a register, width doesn't matter
                setRegister(dst.Value.Byte(), src.Value);
            } else if (src.isWide()) {
                // Source is a word
                bus.WaitOnCommand(new Bus.Command(
                    Memory.DEVICE_ID, (byte)Memory.Commands.WriteWord,
                    dst.Value.Word(), src.Value
                ));
            } else {
                // Source is a byte
                bus.WaitOnCommand(new Bus.Command(
                    Memory.DEVICE_ID, (byte)Memory.Commands.WriteByte,
                    dst.Value.Word(), src.Value
                ));
            }

            // Set 0 flag
            if (src.Value.Word() == 0)
                STZ();
        }

        /// <summary>
        /// Clear all flags
        /// </summary>
        public void CLF() {
            flags = 0;
        }

        /// <summary>
        /// Clear the carry flag
        /// </summary>
        public void CLC() {
            // The ~ operator cooerces to int, for some reason
            flags &= (byte)(0xFF & ~( (uint)(Flags.Carry) ));
        }

        /// <summary>
        /// Clear the parity flag
        /// </summary>
        public void CLP() {
            // The ~ operator cooerces to int, for some reason
            flags &= (byte)(0xFF & ~((uint)(Flags.Parity)));
        }

        /// <summary>
        /// Clear the zero flag
        /// </summary>
        public void CLZ() {
            // The ~ operator cooerces to int, for some reason
            flags &= (byte)(0xFF & ~((uint)(Flags.Zero)));
        }

        /// <summary>
        /// Clear the sign flag
        /// </summary>
        public void CLS() {
            // The ~ operator cooerces to int, for some reason
            flags &= (byte)(0xFF & ~((uint)(Flags.Sign)));
        }

        /// <summary>
        /// Clear the interupt flag
        /// </summary>
        public void CLI() {
            // The ~ operator cooerces to int, for some reason
            flags &= (byte)(0xFF & ~((uint)(Flags.Interrupt)));
        }

        /// <summary>
        /// Clear the overflow flag
        /// </summary>
        public void CLO() {
            // The ~ operator cooerces to int, for some reason
            flags &= (byte)(0xFF & ~((uint)(Flags.Overflow)));
        }

        /// <summary>
        /// Set the carry flag
        /// </summary>
        public void STC() {
            flags |= (byte)Flags.Carry;
        }

        /// <summary>
        /// Set the carry flag
        /// </summary>
        public void STP() {
            flags |= (byte)Flags.Parity;
        }

        /// <summary>
        /// Set the carry flag
        /// </summary>
        public void STZ() {
            flags |= (byte)Flags.Zero;
        }

        /// <summary>
        /// Set the sign flag
        /// </summary>
        public void STS() {
            flags |= (byte)Flags.Sign;
        }

        /// <summary>
        /// Set the interupt flag
        /// </summary>
        public void STI() {
            flags |= (byte)Flags.Interrupt;
        }

        /// <summary>
        /// Set the overflow flag
        /// </summary>
        public void STO() {
            flags |= (byte)Flags.Overflow;
        }
    }
}
