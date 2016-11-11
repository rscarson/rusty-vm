namespace RustFreeVM {
    class Program {
        static void Main(string[] args) {
            Memory memory = new Memory();
            Cpu cpu = new Cpu();

            memory.Start();
            cpu.Start();

            while (true) ;
        }
    }
}
