using System;
using System.Collections.Generic;
using System.Text;

namespace MiniASM
{
    class Preprocessor
    {
        public static bool localDefinitionScope = false;

        Dictionary<string, string> definitions;
        string module;

        public string Module {
            get { return module; }
        }

        public Preprocessor() {
            definitions = new Dictionary<string, string>();
        }

        public bool ParsePreprocessorExp(string line) {
            if (line[0] != Tokens.PREPROCESSOR_DEF) return false;

            string[] words = line.Substring(1).Split(Tokens.SEPARATOR);
            int len = words.Length;

            if (words.Length < 1) {
                return true;
            }

            string instruction = words[0];

            switch (instruction) {
                case Tokens.INCLUDE:
                    Require(len, 1);
                    Include(words[1]);
                    break;
                case Tokens.DEFINE:
                    Require(len, 2);
                    Define(words[1], words[2]);
                    break;
                case Tokens.SECTION_DEF:
                    Require(len, 1);
                    module = words[1];
                    break;
                case Tokens.SECTION_END:
                    module = null;
                    break;
                case Tokens.UNDEF:
                    Require(len, 1);
                    UnDefine(words[1]);
                    break;
            }

            return true;
        }

        internal string AppendObjectId(string obj, string identifier) {
            if (definitions.ContainsKey(obj)) {
                return string.Format("{0}.{1}", obj, identifier);
            }

            return identifier;
        }

        internal string ReplaceDefined(string word) {
            if (definitions.ContainsKey(word)) {
                string rpl = definitions[word];
                return rpl == "" ? word : rpl;
            }

            return word;
        }

        internal string AddModule(string word) {
            if (!localDefinitionScope && module != null) {
                return string.Format("{0}.{1}", module, word);
            }

            return word;
        }

        internal string Compile(string[] words) {
            for (int i = 0; i < words.Length; i++) {

                if (words[i].Length == 0) continue;

                if (words[i][0] == Tokens.VALUE_MARKER) {
                    words[i] = Tokens.VALUE_MARKER + ReplaceDefined(words[i].Substring(1));
                    continue;
                }

                words[i] = ReplaceDefined(words[i]);
            }

            var buffer = new StringBuilder();

            foreach (string word in words) {
                buffer.Append(word);
                buffer.Append(Tokens.SEPARATOR);
            }

            return buffer.ToString();
        }

        #region Instructions

        void Define(string find, string replace) {
            if (definitions.ContainsKey(find)) {
                definitions[find] = replace;
                return;
            }

            definitions.Add(find, replace);
        }


        void UnDefine(string keyword) {
            if (definitions.ContainsKey(keyword)) {
                definitions.Remove(keyword);
            }
        }

        void Include(string objId) {
            if (definitions.ContainsKey(objId)) {
                return;
            }

            definitions.Add(objId, null);
        }

        #endregion

        void Require(int words, int count) {
            if (words < count + 1) {
                throw new SyntaxError(count, words - 1 + " parameters [PREPROCESSOR]");
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();

            foreach (var kv in definitions) {
                sb.Append("[ PRE] ");
                sb.Append(kv.Key);
                sb.Append(": ");
                sb.AppendLine(kv.Value ?? "<include>");
            }

            return sb.ToString();
        }
    }
}
