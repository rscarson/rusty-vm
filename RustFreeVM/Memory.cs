using System.Threading;

namespace RustFreeVM {
    class Memory {
        // Identifier for bus communications
        public const byte DEVICE_ID = 0x00;
        public enum Commands {
            ReadByte=0x00, ReadWord=0x01,
            WriteByte = 0x02, WriteWord=0x03,
        }

        public const uint MEMORY_SIZE = 0xFFFF;
        private byte[] memory;

        public Memory() {
            memory = new byte[MEMORY_SIZE];
        }

        public void Cycle() {
            Bus.Command c = Bus.getInstance().WaitForCommand(DEVICE_ID);

            switch (c.Opcode) {
                case (byte)Commands.ReadByte:
                    c.Result = new Value(memory[c.Address]);
                    break;

                case (byte)Commands.ReadWord:
                    c.Result = new Value((ushort) (( (ushort)memory[c.Address] << 8 ) + memory[c.Address + 1]));
                    break;

                case (byte)Commands.WriteByte:
                    byte bdata = c.Data.Byte();
                    memory[c.Address] = bdata;
                    c.Respond();
                    break;

                case (byte)Commands.WriteWord:
                    ushort wdata = c.Data.Word();
                    memory[c.Address] = (byte)(wdata >> 8);
                    memory[c.Address + 1] = (byte)(wdata & 0x00FF);
                    break;
            }
        }

        public void Start() {
            Thread t = new Thread(() => {
                while (true) Cycle();
            });
            t.Start();
        }
    }
}
