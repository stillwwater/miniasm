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
        public Symbol<float> Abs(Symbol<float> num) {
            return new Symbol<float>(Tokens.NUM, Mathf.Abs(num.Value));
        }

        public Symbol<float> Floor(Symbol<float> num) {
            return new Symbol<float>(Tokens.NUM, Mathf.Floor(num.Value));
        }

        public Symbol<float> Ceil(Symbol<float> num) {
            return new Symbol<float>(Tokens.NUM, Mathf.Ceil(num.Value));
        }

        public Symbol<float> Round(Symbol<float> num) {
            return new Symbol<float>(Tokens.NUM, Mathf.Round(num.Value));
        }
    }
}

#endif