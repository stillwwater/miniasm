using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;

namespace MiniASM.APIs.STD
{
    public class Comparisons : MiniAPI
    {

        public Symbol<float> Cmp(MetaSymbol a, MetaSymbol b) {
            if (Equal(a, b)) {
                return new Symbol<float>(Tokens.NUM, 0);
            }

            if (a.Type == b.Type && (a.Type == Tokens.NUM || a.Type == Tokens.INT)) {
                // must be int or num to compare greater than
                if (GreaterThan(a, b)) {
                    return new Symbol<float>(Tokens.NUM, 1);
                }
            }

            // not equal or less than
            return new Symbol<float>(Tokens.NUM, -1);
        }

        public Symbol<float> CType(Symbol<int> ptr, Symbol<string> a) {
            if (mini.GetRegister(ptr.Value).Type == a.Value) {
                return new Symbol<float>(Tokens.NUM, 0);
            }
            return new Symbol<float>(Tokens.NUM, -1);
        }

        public Symbol<float> CType(MetaSymbol a, MetaSymbol b) {
            if (a.Type == b.Type) {
                return new Symbol<float>(Tokens.NUM, 0);
            }
            return new Symbol<float>(Tokens.NUM, -1);
        }

        public void Jmp(Symbol<int> ptr) {
            var label = mini.GetAddress<Instruction>(ptr.Value, Tokens.LBL, Tokens.FUN);
            mini.Run(label.Value.Identifier);
            Ret();
        }

        public void Jmp(Symbol<int> ptr, MetaSymbol arg) {
            var fun = mini.GetAddress<Instruction>(ptr.Value, Tokens.LBL, Tokens.FUN).Value;
            object o = mini.FindObject(fun.ObjectIdentifier, noerror: true);
            o = o ?? mini;

            fun.Call(new MetaSymbol[] { arg }, o);
            Ret();
        }

        public void Ret() {
            if (Preprocessor.localDefinitionScope) {
                mini.SetRegister(3, new Symbol<int>(Tokens.INT, 1));
            }
        }

        public MetaSymbol Ret(MetaSymbol value) {
            Ret();
            return value;
        }

        public void Je(Symbol<int> label) {
            if (mini.GetRegister<float>(1, Tokens.NUM).Value == 0) {
                // cmp <any> <any> == 0
                mini.Run(GetLabel(label));
                Ret();
            }
        }

        public void Jne(Symbol<int> label) {
            if (mini.GetRegister<float>(1, Tokens.NUM).Value != 0) {
                // cmp <any> <any> != 0
                mini.Run(GetLabel(label));
                Ret();
            }
        }

        public void Jl(Symbol<int> label) {
            if (mini.GetRegister<float>(1, Tokens.NUM).Value < 0) {
                mini.Run(GetLabel(label));
                Ret();
            }
        }

        public void Jg(Symbol<int> label) {
            if (mini.GetRegister<float>(1, Tokens.NUM).Value > 0) {
                mini.Run(GetLabel(label));
                Ret();
            }
        }

        public static void CreateReference() {
            CreateReference("comp", new Comparisons());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;
            AddFun("Cmp", MetaSymbol.Any, MetaSymbol.Any);
            AddFun("CType", MetaSymbol.Any, MetaSymbol.Any);
            AddFun("CType", MetaSymbol.Ptr, MetaSymbol.Str);
            AddFun("Je", MetaSymbol.Ptr);
            AddFun("Jmp", MetaSymbol.Ptr);
            AddFun("Jmp", MetaSymbol.Ptr, MetaSymbol.Any);
            AddFun("Jne", MetaSymbol.Ptr);
            AddFun("Jl", MetaSymbol.Ptr);
            AddFun("Jg", MetaSymbol.Ptr);
            AddFun("Ret");
            AddFun("Ret", MetaSymbol.Any);
        }

        string GetLabel(Symbol<int> ptr) {
            return mini.GetAddress<Instruction>(ptr.Value, Tokens.LBL, Tokens.FUN).Value.Identifier;
        }

        bool Equal(MetaSymbol a, MetaSymbol b) {
            if (a.Type != b.Type) {
                return false;
            }

            switch (a.Type) {
                case Tokens.NUM:
                    return MetaSymbol.Cast<float>(Tokens.NUM, a).Value == MetaSymbol.Cast<float>(Tokens.NUM, b).Value;
                case Tokens.STR:
                    return MetaSymbol.Cast<string>(Tokens.STR, a).Value == MetaSymbol.Cast<string>(Tokens.STR, b).Value;
                case Tokens.INT:
                    return MetaSymbol.Cast<int>(Tokens.INT, a).Value == MetaSymbol.Cast<int>(Tokens.INT, b).Value;
                default:
                    throw new SyntaxError("<num>|<str>|<int>", a.Type);
            }
        }

        bool GreaterThan(MetaSymbol a, MetaSymbol b) {
            if (a.Type != b.Type) {
                return false;
            }

            switch (a.Type) {
                case Tokens.NUM:
                    return MetaSymbol.Cast<float>(Tokens.NUM, a).Value > MetaSymbol.Cast<float>(Tokens.NUM, b).Value;
                case Tokens.INT:
                    return MetaSymbol.Cast<int>(Tokens.INT, a).Value < MetaSymbol.Cast<int>(Tokens.INT, b).Value;
                default:
                    throw new SyntaxError("<num>|<int>", a.Type);
            }
        }
    }
}
