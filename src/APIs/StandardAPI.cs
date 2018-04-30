using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MiniASM.Builtin;
using MiniASM.APIs.STD;

#if UNITY_EDITOR

using UnityEngine;

#endif

namespace MiniASM.APIs
{
    public class StandardAPI
    {
        public static Action<object> output = Console.WriteLine;
        public static Func<string> input = Console.ReadLine;

        public static void Load(Interpreter mini) {
            STD.InputOutput.CreateReference();
            STD.Keywords.CreateReference();
            STD.Comparisons.CreateReference();
            STD.String.CreateReference();
            STD.Vector.CreateReference();
            STD.List.CreateReference();
            STD.Math.CreateReference();
            STD.Tools.CreateReference();

            // load standard section
            mini.Run("require $stdio");
            mini.Run("stdio.import $std");
        }
    }
}
