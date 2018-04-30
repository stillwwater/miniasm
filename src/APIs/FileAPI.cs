using MiniASM.APIs;
using MiniASM;
using System.IO;
using MiniASM.Builtin;

namespace MiniASM.APIs
{
    public class FileAPI : MiniAPI
    {
        public void Remove(Symbol<string> path) {
            File.Delete(path.Value);
        }
        
        public void Write(Symbol<string> path, Symbol<string> content) {
            File.WriteAllText(path.Value, content.Value);
        }

        public void Append(Symbol<string> path, Symbol<string> content) {
            File.AppendAllText(path.Value, content.Value);
        }

        public Symbol<string> Read(Symbol<string> path) {
            return new Symbol<string>(Tokens.STR, File.ReadAllText(path.Value));
        }

        public static void CreateReference() {
            CreateReference("file", new FileAPI());
        }

        public override void Init(Interpreter mini) {
            base.Init(mini);
            AddFun("Remove", MetaSymbol.Str);
            AddFun("Write", MetaSymbol.Str, MetaSymbol.Str);
            AddFun("Append", MetaSymbol.Str, MetaSymbol.Str);
            AddFun("Read", MetaSymbol.Str);
        }
    }
}
