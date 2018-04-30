using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;

namespace MiniASM.APIs.STD
{
    public class Tools : MiniAPI
    {
        public void Define(Symbol<string> a, Symbol<int> constant) {
            mini.Run(string.Format("#define {0} R{1}", a.Value, constant.Value));
        }

        public Symbol<int> Allocate() {
            mini.Push(MetaSymbol.Nil);
            return new Symbol<int>(Tokens.INT, mini.TableSize() - 1);
        }

        public static void CreateReference() {
            CreateReference("tools", new Tools());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;
            AddFun("Define", MetaSymbol.Str, MetaSymbol.Ptr);
            AddFun("Allocate");
        }
    }
}
