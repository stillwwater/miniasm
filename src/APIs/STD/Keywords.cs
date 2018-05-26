using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;

namespace MiniASM.APIs.STD
{
    public class Keywords : MiniAPI
    {
        public void Mov(Symbol<int> ptr, MetaSymbol value) {
            mini.SetAddress(ptr.Value, value);
        }

        public MetaSymbol Mov(MetaSymbol value) {
            return value;
        }

        public void Call(Symbol<int> ptr) {
            var label = mini.GetAddress<Instruction>(ptr.Value, Tokens.LBL, Tokens.FUN);
            mini.Run(label.Value.Identifier);
        }

        public void For(Symbol<int> ptr, Symbol<float> max, Symbol<int> label) {
            var fun = mini.GetAddress<Instruction>(label.Value, Tokens.LBL, Tokens.FUN).Value;

            object o = mini.FindObject(fun.ObjectIdentifier, noerror: true);
            o = o ?? mini;

            int itreg = ptr.Value;
            var args = new MetaSymbol[0];

            for (int i = 0; i < max.Value; i++) {
                mini.SetAddress(itreg, new Symbol<float>(Tokens.NUM, i));
                fun.Call(args, o);
            }
        }

        public void For(Symbol<int> ptr, Symbol<int> max, Symbol<int> label) {
            For(ptr, mini.GetRegister<float>(max.Value, Tokens.NUM), label);
        }

        public void Loop(Symbol<int> ptr) {
            var fun = mini.GetAddress<Instruction>(ptr.Value, Tokens.LBL).Value;

            int itreg = ptr.Value;
            var args = new MetaSymbol[0];

            while (mini.GetRegister<float>(1, Tokens.NUM).Value != 0) {
                fun.Call(args, mini);
            }
        }

        public static void CreateReference() {
            CreateReference("kw", new Keywords());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;
            AddFun("Mov", MetaSymbol.Ptr, MetaSymbol.Any);
            AddFun("Mov", MetaSymbol.Any);
            AddFun("Call", MetaSymbol.Ptr);
            AddFun("For", MetaSymbol.Ptr, MetaSymbol.Num, MetaSymbol.Ptr);
            AddFun("For", MetaSymbol.Ptr, MetaSymbol.Ptr, MetaSymbol.Ptr);
            AddFun("Loop", MetaSymbol.Ptr);
        }
    }
}
