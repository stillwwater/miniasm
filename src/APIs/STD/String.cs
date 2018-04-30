using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;
using System.Collections.Generic;
using System.Text;

namespace MiniASM.APIs.STD
{
    public class String : MiniAPI
    {
        public Symbol<float> Size(Symbol<string> str) {
            return new Symbol<float>(Tokens.NUM, str.Value.Length);
        }

        public void Add(Symbol<int> ptr, Symbol<string> b) {
            var a = mini.GetRegister<string>(ptr.Value, Tokens.STR);
            a.Value += b.Value;
            mini.SetAddress(ptr.Value, a);
        }

        public Symbol<string> Add(Symbol<string> a, Symbol<string> b) {
            return new Symbol<string>(a.Type, a.Value + b.Value);
        }

        public Symbol<List<MetaSymbol>> Split(Symbol<string> s) {
            var chars = new List<MetaSymbol>(s.Value.Length);

            foreach (char c in s.Value) {
                chars.Add(new Symbol<string>(Tokens.STR, c.ToString()));
            }

            return new Symbol<List<MetaSymbol>>(Tokens.LIST, chars);
        }

        public Symbol<string> Str(Symbol<float> num) {
            return new Symbol<string>(Tokens.STR, num.Value.ToString());
        }

        public Symbol<string> Str(Symbol<List<MetaSymbol>> list) {
            var sb = new StringBuilder();

            foreach (var item in list.Value) {
                sb.AppendLine(item.ToString());
            }

            return new Symbol<string>(Tokens.STR, sb.ToString());
        }

        public static void CreateReference() {
            CreateReference("string", new String());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;
            AddFun("Size", MetaSymbol.Str);
            AddFun("Add", MetaSymbol.Ptr, MetaSymbol.Str);
            AddFun("Add", MetaSymbol.Str, MetaSymbol.Str);
            AddFun("Split", MetaSymbol.Str);
            AddFun("Str", MetaSymbol.Num);
            AddFun("Str", MetaSymbol.List);
        }
    }
}
