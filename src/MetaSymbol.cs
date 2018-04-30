using MiniASM.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MiniASM
{
    public abstract class MetaSymbol
    {
        #region default types

        static readonly Symbol<bool?> nil              = new Symbol<bool?>(Tokens.NIL, null);
        static readonly Symbol<float> num              = new Symbol<float>(Tokens.NUM, 0);
        static readonly Symbol<string> str             = new Symbol<string>(Tokens.STR, null);
        static readonly Symbol<int> ptr                = new Symbol<int>(Tokens.INT, 0);
        static readonly Symbol<Label> lbl              = new Symbol<Label>(Tokens.LBL, null);
        static readonly Symbol<Vector3> vec3           = new Symbol<Vector3>(Tokens.VEC, Vector3.zero);
        static readonly Symbol<MetaSymbol> any         = new Symbol<MetaSymbol>(Tokens.ANY, null);
        static readonly Symbol<List<MetaSymbol>> list  = new Symbol<List<MetaSymbol>>(Tokens.LIST, null);

        public static Symbol<bool?> Nil {
            get { return nil; }
        }

        public static Symbol<float> Num {
            get { return num; }
        }

        public static Symbol<string> Str {
            get { return str; }
        }

        public static Symbol<int> Ptr {
            get { return ptr; }
        }

        public static Symbol<Label> Lbl {
            get { return lbl; }
        }

        public static Symbol<List<MetaSymbol>> List {
            get { return list; }
        }

        public static Symbol<Vector3> Vec3 {
            get { return vec3; }
        }

        public static Symbol<MetaSymbol> Any {
            get { return any; }
        }

        #endregion

        public abstract string Type { get; }
        public static Symbol<T> Cast<T>(string expectedType, MetaSymbol sym) {
            if (sym.Type != expectedType) {
                throw new InterpreterError(Tokens.TYPE_ERR, expectedType, sym.Type);
            }

            return (Symbol<T>)sym;
        }
    }
}
