using MiniASM.APIs;
using MiniASM;
using System.IO;
using MiniASM.Builtin;

namespace MiniASM.APIs.STD
{
    public partial class Math : MiniAPI
    {
        public void Pow(Symbol<int> ptr, Symbol<float> b) {
            var a = mini.GetRegister<float>(ptr.Value, Tokens.NUM);
            mini.SetAddress(ptr.Value, Pow(a, b));
        }

        public Symbol<float> Pow(Symbol<float> a, Symbol<float> b) {
            var result = new Symbol<float>(Tokens.NUM, a.Value);

            for (int i = 1; i < b.Value; i++) {
                result.Value *= a.Value;
            }

            return result;
        }
    }
}
