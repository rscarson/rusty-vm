using System.Collections.Generic;
using RustFreeVM;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace Assembler {
    class Program {
        static void Main(string[] args) {
            List<Instruction> instructions = new List<RustFreeVM.Instruction>();
            Dictionary<string, Value> variables = new Dictionary<string, Value>();
            Dictionary<string, int> labels = new Dictionary<string, int>();

            string filename = "../../example.va";
            StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            string line;
            while ((line = reader.ReadLine()) != null) {
                Instruction instruction = new Instruction();
                States current_state = States.Idle;


                // Search for labels
                line.TrimStart();
                var token_search = Regex.Match(line, "([a-ZA-Z0-9]+):");
                if (token_search.Success) {
                    line = line.Substring(token_search.Length);
                    labels.Add(token_search.Groups[0].Value, instructions.Count);
                }

                // Look for an operator
                line.TrimStart();
                foreach (var operator_value in Enum.GetValues(typeof(Instruction.Operators))) {
                    string operator_name = operator_value.ToString();
                    if (line.StartsWith(operator_name)) {
                        line = line.Substring(operator_name.Length);

                        current_state = States.Instruction;
                        instruction.Opcode = (byte)operator_value;
                    }
                }
            }

            while (true) ;
        }

        public enum States {
            Idle, Instruction, Variable
        }
    }
}
