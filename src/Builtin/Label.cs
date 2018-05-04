using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MiniASM.Builtin
{
    /// <summary>
    /// Data type used to store custom instructions
    /// and their documentation
    /// </summary>
    public class Label : Instruction
    {
        string[] body;
        string[] argVariables;
        string objId;

        public string[] Body {
            get { return body; }
        }

        public override string ObjectIdentifier {
            get { return objId; }
            set { objId = value; }
        }

        public Label(string id, string[] sign, string[] args, string[] body, string[] doc) {
            this.id = id;
            this.sign = sign;
            this.body = body;
            this.doc = doc;
            argVariables = args;
            objId = base.ObjectIdentifier;
        }

        public Label(string id) : this(id, new string[0], new string[0], new string[0], new string[0]) { }

        public override MetaSymbol Call(MetaSymbol[] args, object interpreter = null) {
            var ins = Interpreter.Instance;

            bool cachedScope = Preprocessor.localDefinitionScope;

            // turn off global section scope, change to local scope
            Preprocessor.localDefinitionScope = true;

            int snapshot = ins.TableSize();

            // set argument registers
            ins.SetParamRegisters(args);

            // declare arguments
            DeclareArguments(args);

            int defs = 0;

            // first pass, define inner labels
            for (int i = 0; i < body.Length; i++) {
                string line = body[i].Trim();

                if (line[line.Length - 1] == Tokens.DEF_MARKER) {
                    if (defs > 0) {
                        ins.Run(Tokens.END);
                    }
                    defs++;
                }
                if (defs > 0) {
                    ins.Run(line);
                }
            }

            if (defs > 0) {
                ins.Run(Tokens.END);
            }

            // second pass, execute main body
            for (int i = 0; i < body.Length; i++) {
                string line = body[i].Trim();

                if (line[line.Length - 1] == Tokens.DEF_MARKER) {
                    break;
                }

                ins.Run(line);

                if (ins.GetRegister<int>(3, Tokens.INT).Value == 1) {
                    // this is a jusmp instruction to another label
                    ins.SetRegister(3, new Symbol<int>(Tokens.INT, 0));
                    break;
                }
            }

            // destroy local data
            ins.Pop(ins.TableSize() - snapshot);

            // exit scope
            Preprocessor.localDefinitionScope = cachedScope;

            calls++;
            return MetaSymbol.Nil;
        }

        void DeclareArguments(MetaSymbol[] args) {
            for (int i = 0; i < args.Length; i++) {
                Interpreter.Instance.Push(argVariables[i], args[i]);
            }
        }

        public override MetaSymbol Call() {
            var ins = Interpreter.Instance;

            // execute body
            foreach (var line in body) {
                ins.Run(line);
            }
            calls++;
            return MetaSymbol.Nil;
        }
    }
}
