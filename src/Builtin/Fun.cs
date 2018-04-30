using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniASM.Builtin
{
    /// <summary>
    /// used to store a reference to a native
    /// C# method
    /// </summary>
    public class Fun : Instruction
    {
        string objId;
        string methodName;
        string knownObj = null;
        Type[] internalSign;

        public override string ObjectIdentifier {
            get { return objId; }
        }

        public Fun(string objId, string id, string methodName, params MetaSymbol[] signature) {
            this.id = id;
            this.objId = objId;
            this.methodName = methodName;
            SetSignature(signature);
            //objId = GetObjId();
        }

        /// <summary>
        /// Invokes a method in C#
        /// </summary>
        public override MetaSymbol Call(MetaSymbol[] args, object csobj) {
            SignatureMatch(args);
            Type type = csobj.GetType();
            knownObj = type.ToString();

            var info = type.GetMethod(methodName, internalSign);
            MetaSymbol result = (MetaSymbol)info.Invoke(csobj, args);
            calls++;

            return result ?? MetaSymbol.Nil;
        }

        public override string ToString() {
            string obj = knownObj ?? "?" ;
            return base.ToString() + string.Format(" {0} {1}", obj, methodName);
        }

        /// <summary>
        /// MetaSym[] -> string[], Type[]
        /// </summary>
        void SetSignature(MetaSymbol[] sign) {
            this.sign = new string[sign.Length];
            internalSign = new Type[sign.Length];

            for (int i = 0; i < sign.Length; i++) {
                this.sign[i] = sign[i].Type;
                internalSign[i] = sign[i].GetType();
            }
        }

        /// <summary>
        /// object.label -> object
        /// </summary>
        string GetObjId() {
            return id.Split(Tokens.SCOPE_OP)[0];
        }
    }
}
