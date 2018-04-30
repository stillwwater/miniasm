using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniASM.Builtin
{
    public class Instruction
    {
        protected string id;
        protected string[] sign;
        protected string[] doc;
        protected int calls = 0;
        protected int objectAddr;

        public string Identifier {
            get { return id; }
        }

        public string[] Signature {
            get { return sign; }
        }

        public string[] Documentation {
            get { return doc; }
        }

        public int ObjectAddr {
            get { return objectAddr; }
            set { objectAddr = value; }
        }

        public virtual string ObjectIdentifier {
            get { return "native"; }
            set { }
        }

        public virtual MetaSymbol Call(MetaSymbol[] args, object obj) {
            SignatureMatch(args);
            calls++;

            return MetaSymbol.Nil;
        }

        public virtual MetaSymbol Call() {
            return MetaSymbol.Nil;
        }

        /// <summary>
        /// Checks if symbol arguments match the label's parameters
        /// </summary>
        public bool SignatureMatch(MetaSymbol[] symbols) {
            if (symbols.Length != sign.Length) {
                return false;
            }

            for (int i = 0; i < sign.Length; i++) {
                if (symbols[i].Type == "any" || sign[i] == "any") {
                    continue;
                }
                if (symbols[i].Type != sign[i]) {
                    return false;
                }
            }
            return true;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append(Identifier);

            foreach (string item in sign) {
                sb.Append(" <");
                sb.Append(item);
                sb.Append(">");
            }

            sb.Append(" <");
            sb.Append(ObjectIdentifier);
            sb.Append(">");

            sb.Append(" [");
            sb.Append(calls);
            sb.Append("]");

            return sb.ToString();
        }
    }
}
