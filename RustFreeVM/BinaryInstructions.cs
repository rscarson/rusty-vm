using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustFreeVM {
    partial class Cpu {
        /// <summary>
        /// Binary not
        /// </summary>
        /// <param name="operand">Source AND destination</param>
        public void NOT(Operand operand) {
            Operand src = new Operand(operand);
            resolve(operand);

            // The ~ operator cooerces to int, for some reason
            if (operand.isWide())
                operand.Value = new Value((ushort)~((uint)operand.Value.Word()));
            else
                operand.Value = new Value((byte)~((uint)operand.Value.Byte()));

            MOV(src, operand);
        }

        /// <summary>
        /// Binary OR
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="src">Source</param>
        public void OR(Operand dst, Operand src) {
            Operand operand = new Operand(dst);
            resolve(operand);
            resolve(src);

            if (operand.isWide())
                operand.Value = new Value((ushort)(operand.Value.Word() | src.Value.Word()));
            else
                operand.Value = new Value((byte)(operand.Value.Byte() | src.Value.Byte()));

            MOV(dst, operand);
        }

        /// <summary>
        /// Binary AND
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="src">Source</param>
        public void AND(Operand dst, Operand src) {
            Operand operand = new Operand(dst);
            resolve(operand);
            resolve(src);

            if (operand.isWide())
                operand.Value = new Value((ushort)(operand.Value.Word() & src.Value.Word()));
            else
                operand.Value = new Value((byte)(operand.Value.Byte() & src.Value.Byte()));

            MOV(dst, operand);
        }

        /// <summary>
        /// Binary XOR
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="src">Source</param>
        public void XOR(Operand dst, Operand src) {
            Operand operand = new Operand(dst);
            resolve(operand);
            resolve(src);

            if (operand.isWide())
                operand.Value = new Value((ushort)(operand.Value.Word() ^ src.Value.Word()));
            else
                operand.Value = new Value((byte)(operand.Value.Byte() ^ src.Value.Byte()));

            MOV(dst, operand);
        }
    }
}
