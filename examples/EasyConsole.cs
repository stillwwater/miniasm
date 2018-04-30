using MiniASM;
using MiniASM.APIs;
using System;

namespace EasyConsole
{
    public class Program
    {
        public static void Main(string[] args) {
            Interpreter.Init(registers: 32);

            var mini = Interpreter.Instance;

            Bootloader.CreateReference();
            StandardAPI.Load(mini);

            while (mini.Ready) {
                Console.Write("> ")
                gelii.Run(Console.ReadLine());
            }
        }
    }
}
