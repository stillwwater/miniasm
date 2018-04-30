using MiniASM.APIs;
using MiniASM;
using MiniASM.Builtin;

#if UNITY_EDITOR

using UnityEngine;

#endif

namespace MiniASM.APIs.STD
{
    public class Vector : MiniAPI
    {
        public void For(Symbol<int> ptr, Symbol<Vector3> max, Symbol<int> label) {
            var calladdr = mini.GetAddress<Instruction>(label.Value, Tokens.LBL, Tokens.FUN).Value;

            object o = mini.FindObject(calladdr.ObjectIdentifier, noerror: true);
            o = o ?? mini;

            int itreg = ptr.Value;
            var args = new MetaSymbol[0];

            for (int x = 0; x < max.Value.x; x++) {
                for (int y = 0; y < max.Value.y; y++) {
                    for (int z = 0; z < max.Value.z; z++) {
                        mini.SetAddress(itreg, new Symbol<Vector3>(Tokens.VEC, new Vector3(x, y, z)));
                        calladdr.Call(args, o);
                    }
                }
            }
        }

        public void Mov(Symbol<int> ptr, Symbol<Vector3> vec, Symbol<string> axis) {
            float num;

            switch (axis.Value.ToLower()) {
                case "x":
                    num = vec.Value.x;
                    break;
                case "y":
                    num = vec.Value.y;
                    break;
                case "z":
                    num = vec.Value.z;
                    break;
                default:
                    throw new SyntaxError("valid axis", axis);
            }

            mini.SetAddress(ptr.Value, new Symbol<float>(Tokens.NUM, num));
        }

        public void Mov(Symbol<int> ptr, Symbol<string> axis, Symbol<float> num) {
            var vec = mini.GetRegister<Vector3>(ptr.Value, Tokens.VEC);

            switch (axis.Value.ToLower()) {
                case "x":
                    vec = new Symbol<Vector3>(
                        Tokens.VEC, new Vector3(num.Value, vec.Value.y, vec.Value.z)
                    );
                    break;
                case "y":
                    vec = new Symbol<Vector3>(
                        Tokens.VEC, new Vector3(vec.Value.x, num.Value, vec.Value.z)
                    );
                    break;
                case "z":
                    vec = new Symbol<Vector3>(
                        Tokens.VEC, new Vector3(vec.Value.x, vec.Value.y, num.Value)
                    );
                    break;
                default:
                    throw new SyntaxError("valid axis", axis);
            }

            mini.SetAddress(ptr.Value, vec);
        }

        public static void CreateReference() {
            CreateReference("vector", new Vector());
        }

        public override void Init(Interpreter mini) {
            this.mini = mini;
            AddFun("For", MetaSymbol.Ptr, MetaSymbol.Vec3, MetaSymbol.Ptr);
            AddFun("Mov", MetaSymbol.Ptr, MetaSymbol.Vec3, MetaSymbol.Str);
            AddFun("Mov", MetaSymbol.Ptr, MetaSymbol.Str, MetaSymbol.Num);
        }
    }
}
