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
        public void Mul(Symbol<int> ptr, Symbol<float> b) {
            var a = mini.GetRegister<float>(ptr.Value, Tokens.NUM);
            a.Value *= b.Value;
            mini.SetAddress(ptr.Value, a);
        }

        public Symbol<float> Mul(Symbol<float> a, Symbol<float> b) {
            return new Symbol<float>(Tokens.NUM, a.Value * b.Value);
        }

        public Symbol<Vector3> Mul(Symbol<Vector3> a, Symbol<float> b) {
            return new Symbol<Vector3>(Tokens.VEC, a.Value * b.Value);
        }
    }
}
