using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;
using System.Collections.Generic;

namespace MiniASM.APIs.STD
{
    public class List : MiniAPI
    {
        public Symbol<float> Size(Symbol<List<MetaSymbol>> list) {
            return new Symbol<float>(Tokens.NUM, list.Value.Count);
        }

        public void Mov(Symbol<int> ptr, Symbol<float> index, MetaSymbol value) {
            var list = mini.GetRegister<List<MetaSymbol>>(ptr.Value, Tokens.LIST).Value;

            if (list.Count > 0 && list[0].Type != value.Type) {
                throw new SyntaxError(list[0].Type, value.Type);
            }

            int i = (int)index.Value;
            i = i < 0 ? list.Count + i : i;
            list[i] = value;
        }

        public void Mov(Symbol<int> ptr, Symbol<List<MetaSymbol>> list, Symbol<float> index) {
            int i = (int)index.Value;
            i = i < 0 ? list.Value.Count + i : i;
            mini.SetAddress(ptr.Value, list.Value[i]);
        }

        public void Push(Symbol<int> ptr, MetaSymbol value) {
            var list = mini.GetRegister<List<MetaSymbol>>(ptr.Value, Tokens.LIST).Value;

            if (list.Count > 0 && list[0].Type != value.Type) {
                throw new SyntaxError(list[0].Type, value.Type);
            }

            list.Add(value);
        }

        public MetaSymbol Pop(Symbol<int> ptr) {
            return Pop(ptr, new Symbol<float>(Tokens.NUM, -1));
        }

        public MetaSymbol Pop(Symbol<int> ptr, Symbol<float> index) {
            var list = mini.GetRegister<List<MetaSymbol>>(ptr.Value, Tokens.LIST).Value;

            int i = (int)index.Value;
            i = i < 0 ? list.Count + i : i;

            var value = list[i];
            list.RemoveAt(i);

            return value;
        }

        public void For(Symbol<int> ptr, Symbol<List<MetaSymbol>> list, Symbol<int> label) {
            var fun = mini.GetAddress<Instruction>(label.Value, Tokens.LBL, Tokens.FUN).Value;

            object o = mini.FindObject(fun.ObjectIdentifier, noerror: true);
            o = o ?? mini;

            int itreg = ptr.Value;
            var args = new MetaSymbol[0];

            foreach (var item in list.Value) {
                mini.SetAddress(itreg, item);
                fun.Call(args, o);
            }
        }

        public static void CreateReference() {
            CreateReference("list", new List());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;
            AddFun("Size", MetaSymbol.List);
            AddFun("Mov", MetaSymbol.Ptr, MetaSymbol.Num, MetaSymbol.Any);
            AddFun("Mov", MetaSymbol.Ptr, MetaSymbol.List, MetaSymbol.Num);
            AddFun("Push", MetaSymbol.Ptr, MetaSymbol.Any);
            AddFun("Pop", MetaSymbol.Ptr);
            AddFun("Pop", MetaSymbol.Ptr, MetaSymbol.Num);
            AddFun("For", MetaSymbol.Ptr, MetaSymbol.List, MetaSymbol.Ptr);
        }
    }
}
