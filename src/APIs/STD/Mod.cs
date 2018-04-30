using MiniASM.APIs;
using MiniASM;
using System.IO;
using MiniASM.Builtin;

namespace MiniASM.APIs.STD
{
    public partial class Math : MiniAPI
    {
        public void Mod(Symbol<int> ptr, Symbol<float> b) {
            var a = mini.GetRegister<float>(ptr.Value, Tokens.NUM);
            a.Value %= b.Value;
            mini.SetAddress(ptr.Value, a);
        }

        public Symbol<float> Mod(Symbol<float> a, Symbol<float> b) {
            return new Symbol<float>(Tokens.NUM, a.Value % b.Value);
        }
    }
}
