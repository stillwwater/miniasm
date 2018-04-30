using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniASM.Builtin
{
    /// <summary>
    /// Data type used to store a reference to
    /// a C# object
    /// </summary>
    public class Obj
    {
        public string Identifier {
            get { return id; }
        }

        public object Reference {
            get { return obj; }
        }

        string id;
        object obj;

        public Obj(string id, object obj) {
            this.id = id;
            this.obj = obj;
        }

        public override string ToString() {
            return string.Format("{0} {1}", id, obj);
        }
    }
}
