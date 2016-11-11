using System.Collections.Generic;

namespace RustFreeVM {
    public class Bus {
        private static Bus instance;
        private Queue<Command> command_queue;

        private Bus() {
            command_queue = new Queue<Command>();
        }

        /// <summary>
        /// Get the singleton instance of the system bus
        /// </summary>
        /// <returns></returns>
        public static Bus getInstance() {
            if (instance == null)
                instance = new Bus();
            return instance;
        }

        /// <summary>
        /// Synchonously send a command to a device
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Value WaitOnCommand(Command c) {

            // Set it and forget it
            command_queue.Enqueue(c);
            while (c.Result == null) ;

            // Send back result
            return c.Result;
        }

        /// <summary>
        /// Synchronously receive a command
        /// </summary>
        /// <returns></returns>
        public Command WaitForCommand(byte target) {
            while (command_queue.Count == 0 || command_queue.Peek().Target != target) ;
            return command_queue.Dequeue();
        }

        /// <summary>
        /// Represents a command that can be sent to a device
        /// </summary>
        public class Command {
            public byte Target { get; private set; }
            public byte Opcode { get; private set; }

            public ushort Address { get; private set; }
            public Value Data { get; private set; }

            public Value Result { get; set; }

            public Command(byte target, byte opcode, ushort address, Value data) {
                Target = target;
                Opcode = opcode;

                Address = address;
                Data = data;

                Result = null;
            }

            public Command(byte target, byte opcode) {
                Target = target;
                Opcode = opcode;

                Address = 0;
                Data = null;

                Result = null;
            }

            public void Respond() {
                Result = new Value();
            }
        }
    }

    public class Value {
        private readonly byte? _byte;
        private readonly ushort? _word;

        public Value() { }
        public Value(Value _source) {
            _byte = _source._byte;
            _word = _source._word;
        }

        public Value(byte v) {
            _byte = v;
        }

        public Value(ushort v) {
            _word = v;
        }

        public byte Byte() {
            return (_byte.HasValue) ? _byte.Value :
                (byte)_word.GetValueOrDefault();
        }

        public ushort Word() {
            return (_word.HasValue) ? _word.Value :
                (ushort)_byte.GetValueOrDefault();
        }
    }
}
