namespace RustFreeVM {
    class Program {
        static void Main(string[] args) {
            Memory m = new Memory();
            Bus b = Bus.getInstance();

            m.Start();

            b.WaitOnCommand(new Bus.Command(
                Memory.DEVICE_ID, (byte)Memory.Commands.WriteByte,
                0x01, (byte)0x02));

            while (true) ;
        }
    }
}
