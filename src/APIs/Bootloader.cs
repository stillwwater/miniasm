using System.Collections;
using System.Collections.Generic;
using MiniASM;
using MiniASM.APIs;
using MiniASM.Builtin;

namespace MiniASM.APIs
{
    public class Bootloader : MiniAPI
    {
        public void Require(Symbol<string> apiName) {
            var api = mini.FindObject(apiName.Value) as MiniAPI;
            api.Init(mini);
        }

        public static void CreateReference() {
            var bootloader = new Bootloader();
            CreateReference("bootloader", bootloader);
            bootloader.Init(Interpreter.Instance);
        }

        public override void Init(Interpreter mini) {
            base.Init(mini);
            mini.AddFun(new Fun("bootloader", "require", "Require", MetaSymbol.Str));
        }
    }
}