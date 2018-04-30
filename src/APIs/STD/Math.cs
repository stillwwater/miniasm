using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;

namespace MiniASM.APIs.STD
{
    public partial class Math : MiniAPI
    {
        public Symbol<float> Num(Symbol<string> s) {
            return new Symbol<float>(Tokens.NUM, float.Parse(s.Value));
        }

        public Symbol<float> Num(Symbol<int> ptr) {
            return new Symbol<float>(Tokens.NUM, (float)ptr.Value);
        }

        public Symbol<int> Int(Symbol<string> s) {
            return new Symbol<int>(Tokens.INT, int.Parse(s.Value));
        }

        public Symbol<int> Int(Symbol<float> num) {
            return new Symbol<int>(Tokens.INT, (int)num.Value);
        }
        
        public static void CreateReference() {
            CreateReference("math", new Math());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;

            AddFun("Num", MetaSymbol.Str);
            AddFun("Num", MetaSymbol.Ptr);
            AddFun("Int", MetaSymbol.Str);
            AddFun("Int", MetaSymbol.Num);

            AddFun("Add", MetaSymbol.Ptr, MetaSymbol.Num);
            AddFun("Add", MetaSymbol.Ptr, MetaSymbol.Ptr);
            AddFun("Add", MetaSymbol.Ptr, MetaSymbol.Vec3);
            AddFun("Add", MetaSymbol.Num, MetaSymbol.Num);
            AddFun("Add", MetaSymbol.Vec3, MetaSymbol.Vec3);

            AddFun("Sub", MetaSymbol.Ptr, MetaSymbol.Num);
            AddFun("Sub", MetaSymbol.Ptr, MetaSymbol.Vec3);
            AddFun("Sub", MetaSymbol.Ptr, MetaSymbol.Ptr);
            AddFun("Sub", MetaSymbol.Num, MetaSymbol.Num);
            AddFun("Sub", MetaSymbol.Vec3, MetaSymbol.Vec3);

            AddFun("Mul", MetaSymbol.Ptr, MetaSymbol.Num);
            AddFun("Mul", MetaSymbol.Num, MetaSymbol.Num);
            AddFun("Mul", MetaSymbol.Vec3, MetaSymbol.Num);

            AddFun("Div", MetaSymbol.Ptr, MetaSymbol.Num);
            AddFun("Div", MetaSymbol.Num, MetaSymbol.Num);
            AddFun("Div", MetaSymbol.Vec3, MetaSymbol.Num);

            AddFun("Mod", MetaSymbol.Ptr, MetaSymbol.Num);
            AddFun("Mod", MetaSymbol.Num, MetaSymbol.Num);

            AddFun("Pow", MetaSymbol.Ptr, MetaSymbol.Num);
            AddFun("Pow", MetaSymbol.Num, MetaSymbol.Num);

#if UNITY_EDITOR
            AddFun("Abs", MetaSymbol.Num);
            AddFun("Floor", MetaSymbol.Num);
            AddFun("Ceil", MetaSymbol.Num);
            AddFun("Round", MetaSymbol.Num);
            AddFun("Sin", MetaSymbol.Num);
            AddFun("Cos", MetaSymbol.Num);
            AddFun("Tan", MetaSymbol.Num);
            AddFun("Rand");
            AddFun("Perlin", MetaSymbol.Vec3);
#endif
        }
    }
}
