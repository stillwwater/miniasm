using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniASM
{
    public class Symbol<T> : MetaSymbol
    {
        string type;
        T val;

        public override string Type {
            get { return type; }
        }

        public T Value {
            get { return val; }
            set { val = value; }
        }

        public Symbol(string internalType, T value) {
            type = internalType;
            Value = value;
        }

        public override string ToString() {
            return string.Format("<{0}> {1}", Type, Value);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
