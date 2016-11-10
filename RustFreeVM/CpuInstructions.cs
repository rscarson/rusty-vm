using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustFreeVM {
    partial class Cpu {
        public void MOV(List<Operand> operands) {
            Operand dst = operands[0];
            Operand src = operands[1];

            if (dst.Type == (byte)Operand.Types.Register) {
                setRegister(dst.Value.Byte(), src.Value);
            } else if (src.isWide()) {
                bus.WaitOnCommand(new Bus.Command(
                    Memory.DEVICE_ID, (byte)Memory.Commands.WriteWord,
                    dst.Value.Word(), src.Value
                ));
            } else {
                bus.WaitOnCommand(new Bus.Command(
                    Memory.DEVICE_ID, (byte)Memory.Commands.WriteByte,
                    dst.Value.Word(), src.Value
                ));
            }

            if (src.Value.Word() == 0)
                flag |= (byte)Flags.Zero;
        }
    }
}
