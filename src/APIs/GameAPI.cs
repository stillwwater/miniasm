using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniASM;
using MiniASM.Builtin;

namespace MiniASM.APIs
{
    public class GameAPI : MonoBehaviour
    {
        protected Interpreter gelii;
        Stack<Instruction> updateStack = new Stack<Instruction>();

        public void _Update(Symbol<int> ptr) {
            var label = gelii.GetAddress<Instruction>(ptr.Value, Tokens.LBL, Tokens.FUN);
            updateStack.Push(label.Value);
        }

        #region Helpers

        void AddFun(string method, params MetaSymbol[] args) {
            string fullname = "game." + method.ToLower();
            gelii.AddFun(new Fun("game", fullname, method, args));
        }

        public void AddAll(Interpreter gelii) {
            this.gelii = gelii;
            gelii.AddObj(new Obj("game", this));

            AddFun("_Update", MetaSymbol.Ptr);
        }

        #endregion

        void Update() {
            foreach (var instruction in updateStack) {
                instruction.Call();
            }
        }
    }
}
