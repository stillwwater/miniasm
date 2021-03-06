﻿using MiniASM.APIs;
using MiniASM;
using System.IO;
using System.Text;
using System;
using MiniASM.Builtin;

namespace MiniASM.APIs.STD
{
    public class InputOutput : MiniAPI
    {
        public void Peek() {
            StandardAPI.output(mini);
        }

        public void Peek(Symbol<float> batchSize) {
            var block = new StringBuilder();

            for (int i = 0; i < mini.TableSize(); i++) {
                block.AppendLine(mini.FormatAddress(i));

                if (i % batchSize.Value == 0) {
                    StandardAPI.output(block);
                    block.Length = 0;
                }
            }

            StandardAPI.output(block);
        }

        public void Peek(Symbol<int> start, Symbol<int> end) {
            for (int i = start.Value; i < end.Value; i++) {
                StandardAPI.output(mini.FormatAddress(i));
            }
        }

        public void Help(Symbol<int> ptr) {
            var instruction = mini.GetAddress<Instruction>(ptr.Value, Tokens.LBL);

            StandardAPI.output("\n" + instruction + "\n");

            foreach (string line in instruction.Value.Documentation) {
                StandardAPI.output("        " + line);
            }
        }

        public void Dump() {
            File.WriteAllText("mini.dmp", mini.ToString());
        }

        public void Dump(Symbol<string> path) {
            File.WriteAllText(path.Value, mini.ToString());
        }

        public void Debug(Symbol<string> flag) {
            switch (flag.Value) {
                case "parser":
                    mini.debugParser = !mini.debugParser;
                    break;
                case "preprocessor":
                    mini.debugPreprocessor = !mini.debugPreprocessor;
                    break;
                case "all":
                    mini.debugParser = true;
                    mini.debugPreprocessor = true;
                    break;
                case "none":
                    mini.debugParser = false;
                    mini.debugPreprocessor = false;
                    break;
                default:
                    StandardAPI.output("Unknown flag " + flag.Value);
                    break;
            }
        }

        public void IO(Symbol<int> ptr) {
            int addr = ptr.Value;

            switch (addr) {
                case 6:
                    if (StandardAPI.input == null) {
                        throw new InterpreterError("IO", "Input not supported");
                    }
                    mini.SetAddress(addr, new Symbol<string>(Tokens.STR, StandardAPI.input()));
                    break;
                case 7:
                    if (StandardAPI.output == null) {
                        throw new InterpreterError("IO", "Output not supported");
                    }

                    var output = mini.GetRegister(addr);

                    if (output.Type == Tokens.STR) {
                        StandardAPI.output(((Symbol<string>)output).Value);
                    } else {
                        StandardAPI.output(output.ToString());
                    }
                    mini.SetRegister(addr, new Symbol<string>(Tokens.STR, ""));
                    break;
                default:
                    throw new InterpreterError("IO", "Not io register");
            }
        }

        public void SetLD(Symbol<string> path) {
            StandardAPI.libraryDirectory = path.Value;
        }

        public Symbol<string> GetLD() {
            return new Symbol<string>(Tokens.STR, StandardAPI.libraryDirectory);
        }

        public void Load(Symbol<string> file) {
            if (!File.Exists(file.Value)) {
                throw new InterpreterError("IO", "cannot find file:", file.Value);
            }

            foreach (string line in File.ReadAllLines(file.Value)) {
                mini.Run(line);
            }

            mini.ResetLineNum();
        }

        public void Import(Symbol<string> module) {
            if (StandardAPI.libraryDirectory == null) {
                throw new InterpreterError("IO", "No library folder defined, define it with `stdio.setld`");
            }

            if (!module.Value.Contains(".")) {
                module.Value += ".mini";
            }

            module.Value = Path.Combine(StandardAPI.libraryDirectory, module.Value);
            Load(module);
        }

        public static void CreateReference() {
            CreateReference("stdio", new InputOutput());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;
            AddFun("Peek");
            AddFun("Peek", MetaSymbol.Num);
            AddFun("Peek", MetaSymbol.Ptr, MetaSymbol.Ptr);
            AddFun("Help", MetaSymbol.Ptr);
            AddFun("Dump");
            AddFun("Dump", MetaSymbol.Str);
            AddFun("Debug", MetaSymbol.Str);
            AddFun("IO", MetaSymbol.Ptr);
            AddFun("SetLD", MetaSymbol.Str);
            AddFun("GetLD");
            AddFun("Load", MetaSymbol.Str);
            AddFun("Import", MetaSymbol.Str);
        }
    }
}
