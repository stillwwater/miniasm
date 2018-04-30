using System;
using MiniASM;
using MiniASM.APIs;

public class MyAPI : MiniAPI
{
    public void SayHello(Symbol<string> name) {
        StandardAPI.output("Hello {0}", name.Value);
    }

    public static void CreateReference() {
        CreateReference("myapi", new MyAPI());
    }

    public override void Init(Interpreter mini) {
        base.Init(mini)
        AddFun("SayHello", MetaSymbol.Str);
    }
}
