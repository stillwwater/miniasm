#if UNITY_EDITOR

using MiniASM.APIs;
using MiniASM;
using System;
using MiniASM.Builtin;
using UnityEngine;

namespace MiniASM.APIs.STD
{
    public partial class Math : MiniAPI
    {
        public Symbol<float> Sin(Symbol<float> num) {
            return new Symbol<float>(Tokens.NUM, Mathf.Sin(num.Value));
        }

        public Symbol<float> Cos(Symbol<float> num) {
            return new Symbol<float>(Tokens.NUM, Mathf.Cos(num.Value));
        }

        public Symbol<float> Tan(Symbol<float> num) {
            return new Symbol<float>(Tokens.NUM, Mathf.Tan(num.Value));
        }
    }
}

#endif