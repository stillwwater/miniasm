## Extending the interpreter

Functionality can be added to the interpreter in the form of APIs defined in C#.

To define a custom API, inherit from the `MiniAPI`class, add a static `CreateReference()` and a `Init(Interpreter)` method. Then call the `CreateReference()` method to push a reference to the custom API to the interpreter.

`MyAPI.cs`

```c#
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
```

`Program.cs`

```c#
public class Program
{
    public static void Main(string[] args) {
        Interpreter.Init(registers: 32);

        var mini = Interpreter.Instance;

        Bootloader.CreateReference();
        MyAPI.CreateReference();
        StandardAPI.Load(mini);

        while (mini.Ready) {
            Console.Write("> ")
            gelii.Run(Console.ReadLine());
        }
    }
}
```

To use the custom API in the interpreter, initilize it using `require (str api)`

```assembly
require $myapi
myapi.sayhello "gelii"	; hello gelii

#include myapi
sayhello "gelii"	; hello gelii

mov R0 0		; halt
```
