using MiniASM;
using MiniASM.APIs;
using System;

namespace InterpreterConsole
{
    class Program
    {
        static void Main(string[] args) {
            Interpreter.Init(registers: 32);

            var mini = Interpreter.Instance;

            StandardAPI.output = Console.WriteLine; // change output method
            StandardAPI.input = Console.ReadLine; // change input method

            Bootloader.CreateReference();
            FileAPI.CreateReference();
            StandardAPI.Load(mini);

            while (mini.Ready) {
                if (mini.IsLabelDefinition) {
                    Console.Write(">> ");
                } else {
                    Console.Write("\n> ");
                }

                mini.Run(Console.ReadLine());
                mini.ResetLineNum();
            }

            Console.WriteLine("bye");
        }
    }
}
