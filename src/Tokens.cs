using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniASM
{
    public static class Tokens
    {
        // Symbol types
        public const string NIL = "nil";
        public const string STR = "str";
        public const string NUM = "num";
        public const string INT = "int";
        public const string VEC = "vec";
        public const string LBL = "lbl";
        public const string OBJ = "obj";
        public const string FUN = "fun";
        public const string ANY = "any";
        public const string VAR = "var";
        public const string LIST = "list";

        // Keywords
        public const string END = "end";
        public const string DOC = ";;";
        public const string DEF = ":";

        // Errors
        public const string TYPE_ERR   = "TypeError";
        public const string SYNTAX_ERR = "SyntaxError";
        public const string PARAM_ERR  = "ParameterMismatch";

        // Chars
        public const char SEPARATOR        = ' ';
        public const char DEF_MARKER       = ':';
        public const char SCOPE_OP         = '.';
        public const char OPEN_PAREN       = '(';
        public const char CLOSE_PAREN      = ')';
        public const char OPEN_BRACKET     = '[';
        public const char CLOSE_BRACKET    = ']';
        public const char OPEN_BRACE       = '{';
        public const char CLOSE_BRACE      = '}';
        public const char VALUE_MARKER     = '&';
        public const char LITERAL_MARKER   = '$';
        public const char PREPROCESSOR_DEF = '#';
        public const char REGISTER_MARKER  = 'R';
        public const char ADDRESS_MARKER   = '#';
        public const char HEX_DELIMITER    = 'x';
        public const char COMMENT          = ';';
        public const char DOUBLE_QUOTE     = '"';
        public const char TAB              = '\t';
        public const char BACKSLASH        = '\\';
        public const char SINGLE_QUOTE     = '\'';

        // Preprocessor definitions
        public const string DEFINE      = "define";
        public const string INCLUDE     = "include";
        public const string SECTION_DEF = "section";
        public const string SECTION_END = "ends";
        public const string UNDEF       = "undefine";

        internal static bool Whitespace(char c) {
            return c == SEPARATOR || c == TAB;
        }

        internal static bool Quote(char c) {
            return c == SINGLE_QUOTE || c == DOUBLE_QUOTE;
        }

        internal static bool Address(string word) {
            if (word.Length < 2) {
                return false;
            }

            return (word[0] == REGISTER_MARKER)
                || (word[0] == ADDRESS_MARKER)
                || (word[1] == HEX_DELIMITER);
        }

        internal static bool Instruction(MetaSymbol sym) {
            return sym.Type == LBL || sym.Type == FUN;
        }


        internal static bool Numerical(string type) {
            switch (type) {
                case NUM:
                case VEC:
                case INT:
                    return true;
            }
            return false;
        }

        public static bool ValidType(string type) {
            switch (type) {
                case NIL:
                case NUM:
                case STR:
                case VEC:
                case LBL:
                case OBJ:
                case FUN:
                case INT:
                case VAR:
                    return true;
            }
            return false;
        }

        public static bool ValidCastTypes(string type) {
            switch (type) {
                case NUM:
                case STR:
                case INT:
                case VEC:
                    return true;
            }
            return false;
        }
    }
}
