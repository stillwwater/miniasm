#if UNITY_EDITOR

using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;
using UnityEngine;


namespace MiniASM.APIs.STD
{
    public partial class Math : MiniAPI
    {
        public Symbol<float> Rand() {
            return new Symbol<float>(Tokens.NUM, Random.value);
        }
            
        public Symbol<float> Perlin(Symbol<Vector3> pos) {
            float noise = Mathf.PerlinNoise(pos.Value.x, pos.Value.y);
            return new Symbol<float>(Tokens.NUM, noise);
        }
    }
}

#endif
