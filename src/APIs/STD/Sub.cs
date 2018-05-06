using MiniASM.APIs;
using MiniASM;
using System.IO;
using MiniASM.Builtin;

#if UNITY_EDITOR

using UnityEngine;

#endif

namespace MiniASM.APIs.STD
{
    public partial class Math : MiniAPI
    {
        public void Sub(Symbol<int> ptr, Symbol<float> b) {
            var a = mini.GetRegister<float>(ptr.Value, Tokens.NUM);
            a.Value -= b.Value;
            mini.SetAddress(ptr.Value, a);
        }

        public void Sub(Symbol<int> ptr, Symbol<Vector3> b) {
            var a = mini.GetRegister<Vector3>(ptr.Value, Tokens.VEC);
            a.Value -= b.Value;
            mini.SetAddress(ptr.Value, a);
        }

        public Symbol<int> Sub(Symbol<int> a, Symbol<int> b) {
            return new Symbol<int>(Tokens.INT, a.Value - b.Value);
        }

        public Symbol<float> Sub(Symbol<float> a, Symbol<float> b) {
            return new Symbol<float>(Tokens.NUM, a.Value - b.Value);
        }

        public Symbol<Vector3> Sub(Symbol<Vector3> a, Symbol<Vector3> b) {
            return new Symbol<Vector3>(a.Type, a.Value - b.Value);
        }
    }
}
