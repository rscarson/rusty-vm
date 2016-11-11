using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustFreeVM {
    partial class Cpu {
        /// <summary>
        /// Binary rotate left
        /// </summary>
        /// <param name="operand">Source AND destination</param>
        public void ROTL(Operand operand) {
            Operand src = new Operand(operand);
            resolve(operand);

            if (operand.isWide())
                operand.Value = new Value((ushort)((operand.Value.Word() << 1) | (operand.Value.Word() >> (16 - 1))));
            else
                operand.Value = new Value((byte)((operand.Value.Byte() << 1) | (operand.Value.Byte() >> (8 - 1))));

            MOV(src, operand);
        }

        /// <summary>
        /// Binary rotate right
        /// </summary>
        /// <param name="operand">Source AND destination</param>
        public void ROTR(Operand operand) {
            Operand src = new Operand(operand);
            resolve(operand);

            if (operand.isWide())
                operand.Value = new Value((ushort)((operand.Value.Word() >> 1) | (operand.Value.Word() << (16 - 1))));
            else
                operand.Value = new Value((byte)((operand.Value.Byte() >> 1) | (operand.Value.Byte() << (8 - 1))));

            MOV(src, operand);
        }

        /// <summary>
        /// Logical left shift
        /// </summary>
        /// <param name="operand">Source AND destination</param>
        public void SHFL(Operand operand) {
            Operand src = new Operand(operand);
            resolve(operand);

            if (operand.isWide()) {
                if ((operand.Value.Word() & 0x8000) > 0)
                    STC();

                operand.Value = new Value((ushort) (operand.Value.Word() << 1));
            } else {
                if ((operand.Value.Byte() & 0x80) > 0)
                    STC();

                operand.Value = new Value((byte)(operand.Value.Byte() << 1));
            }

            if (operand.Value.Word() == 0)
                STZ();

            MOV(src, operand);
        }

        /// <summary>
        /// Logical right shift
        /// </summary>
        /// <param name="operand">Source AND destination</param>
        public void SHFR(Operand operand) {
            Operand src = new Operand(operand);
            resolve(operand);

            if ((operand.Value.Word() & 0x1) > 0)
                STC();

            if (operand.isWide())
                operand.Value = new Value((ushort)(operand.Value.Word() >> 1));
            else
                operand.Value = new Value((byte)(operand.Value.Byte() >> 1));

            if (operand.Value.Word() == 0)
                STZ();

            MOV(src, operand);
        }

        /// <summary>
        /// Arithmetic left shift
        /// </summary>
        /// <param name="operand">Source AND destination</param>
        public void ASHFL(Operand operand) {
            SHFL(operand);
        }

        /// <summary>
        /// Arithmetic right shift
        /// </summary>
        /// <param name="operand">Source AND destination</param>
        public void ASHFR(Operand operand) {
            Operand src = new Operand(operand);
            resolve(operand);

            if ((operand.Value.Word() & 0x1) > 0)
                STC();

            if (operand.isWide())
                operand.Value = new Value((ushort)((operand.Value.Word() >> 1) | (operand.Value.Word() & 0x8000)));
            else
                operand.Value = new Value((ushort)((operand.Value.Byte() >> 1) | (operand.Value.Byte() & 0x80)));

            if (operand.Value.Word() == 0)
                STZ();

            MOV(src, operand);
        }
    }
}
