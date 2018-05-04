using MiniASM.Builtin;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MiniASM.APIs;

#if UNITY_EDITOR

using UnityEngine;

#endif

namespace MiniASM
{
    public class Interpreter
    {
        public enum SpecialRegisters
        {
            PSR = 0,
            AX = 1,
            IPR = 2,
            CMX = 3,
            OUT = 7,
        }

        public static Action<object> errorOut = Console.WriteLine;
        public static Action<object> debugOut = Console.WriteLine;

        struct Expression
        {
            internal int returnRegister;
            internal Symbol<Instruction> instruction;
            internal MetaSymbol[] args;

            public override string ToString() {
                var sb = new StringBuilder();
                sb.Append("Exp { ");
                sb.Append(returnRegister.ToString().PadLeft(4, '0'));

                sb.Append(instruction);
                sb.Append(" (");

                foreach (var arg in args) {
                    sb.Append(arg);
                    sb.Append(Tokens.SEPARATOR);
                }

                sb.AppendLine(")}");
                return sb.ToString();
            }
        }

        struct LabelDef
        {
            internal string identifier;
            internal string[] signature;
            internal string[] args;
            internal bool inline;

            public override string ToString() {
                var sb = new StringBuilder();
                sb.Append("\nDef { ");
                sb.Append(identifier);

                sb.Append(" ( ");

                foreach (var type in signature) {
                    sb.Append(type);
                    sb.Append(Tokens.SEPARATOR);
                }

                sb.AppendLine(")}");
                return sb.ToString();
            }
        }

        static Interpreter instance;
        internal bool debugParser;
        internal bool debugPreprocessor;

        List<MetaSymbol> symbolTable;
        Preprocessor preprocessor;
        bool ready = false;
        int registers;
        string line;
        string[] words;

        int lineNum;
        int tokenNum;
        StringBuilder multiLineBuffer = new StringBuilder();

        LabelDef openLabel;
        List<string> labelBodyBuffer = new List<string>();
        List<string> labelDocBuffer = new List<string>();
        bool isLabelOpen = false;

        public static Interpreter Instance {
            get { return instance; }
        }

        public bool Ready {
            get { return ready; }
        }

        public int LineNum {
            get { return lineNum; }
        }

        public int TokenNum {
            get { return tokenNum; }
        }

        public bool IsLabelDefinition {
            get { return isLabelOpen; }
        }

        private Interpreter(int registers) {
            this.registers = registers;
            symbolTable = new List<MetaSymbol>();
            preprocessor = new Preprocessor();
            InitRegisters();
            ready = true;
        }

        public static void Init(int registers) {
            if (instance != null) {
                throw new InterpreterError("Multiple interpreter instances");
            }
            instance = new Interpreter(registers);
        }

        public void Run(string line) {
            try {
                RunUnchecked(line);
            } catch (InterpreterError e) {
                errorOut(e.Message + " at " + StackTrace());
            } catch (TargetInvocationException e) {
                errorOut(e.InnerException.Message + " at " + StackTrace());
            }
        }

        public void RunUnchecked(string line) {
            if (!ready) {
                throw new InterpreterError("Error: Interpreter not initiated");
            }

            if (line == null) return;

            Parse(line);

            CheckCrucialRegisters();
        }

        public string StackTrace() {
            string st = string.Format("line #{0}", LineNum);
            return tokenNum == 0 ? st : string.Format("{0}, token #{1}", st, tokenNum);
        }

        public string FormatAddress(int address) {
            return string.Format("[{0}] {1}", address.ToString().PadLeft(5, '0'), GetAddress(address));
        }

        void CheckCrucialRegisters() {
            float R0 = MetaSymbol.Cast<float>(Tokens.NUM, symbolTable[0]).Value;

            switch ((int)R0) {
                case 1:
                    // continue
                    return;
                case 2:
                    StandardAPI.output(line);
                    return;
                case 0:
                    // exit
                    ready = false;
                    instance = null;
                    return;
                case -1:
                default:
                    // throw error
                    ready = false;
                    throw new InterpreterError("Error: (-1) abrupt shutdown");
            }
        }

        /// <summary>
        /// Adds data to the end of the Symbol Table
        /// </summary>
        internal void Push(MetaSymbol sym) {
            symbolTable.Add(sym);
        }

        internal void Push(string variable, MetaSymbol sym) {
            // push variable
            //Push(new Symbol<Var>(Tokens.VAR, new Var(preprocessor.AddModule(varId), arg)));
            preprocessor.ParsePreprocessorExp(string.Format("#define {0} R{1}", preprocessor.AddModule(variable), TableSize()));

            Push(sym);
        }

        /// <summary>
        /// Removes data from the end of the Symbol Table
        /// </summary>
        internal void Pop(int size) {
            int start = symbolTable.Count - size;
            symbolTable.RemoveRange(start, size);
        }

        #region Parser

        void Parse(string line) {
            lineNum++;

            line = line.Trim();

            // empty line
            if (line == "") return;

            // remove inline comment
            this.line = line[0] == Tokens.COMMENT ? line : line.Split(Tokens.COMMENT)[0].Trim();

            if (preprocessor.ParsePreprocessorExp(line)) return;

            // if this line is split accross multiple lines
            // handle it in ParseMultiline()
            if (ParseMultiline()) return;

            // if this statemnt is not an expression
            // handle it in ParseNonExecutable()
            if (ParseNonExecutable()) return;

            // we received a statement we can execute
            var exp = ParseExpression();

            if (debugParser) {
                debugOut(exp);
            }

            if (exp.instruction.Type == Tokens.FUN) {
                // the instruction is a C# method
                var fun = exp.instruction;
                // find the object that has this function defined
                object csobj = GetAddress<Obj>(fun.Value.ObjectAddr, Tokens.OBJ).Value.Reference;
                symbolTable[2] = fun;
                // call function and store it's return value in the return register
                var res = fun.Value.Call(exp.args, csobj);
                if (res != MetaSymbol.Nil) symbolTable[exp.returnRegister] = res;

            } else if (exp.instruction.Type == Tokens.LBL) {
                // the instruction is a user defined label
                var lbl = exp.instruction;
                symbolTable[2] = lbl;
                // call label
                lbl.Value.Call(exp.args, this);

                if (exp.returnRegister != 1) {
                    // assign whatever is in the accumulator to the returnregister
                    symbolTable[exp.returnRegister] = symbolTable[1];
                }
            }
        }

        /// <summary>
        /// Handles label definitions and comments
        /// </summary>
        /// <returns></returns>
        bool ParseNonExecutable() {
            // split line into words
            words = line.Split(Tokens.SEPARATOR);
            string first = words[0].Trim();

            // statement is documentation for the next open block
            if (first.Length > 1 && first.Substring(0, 2) == Tokens.DOC) {
                labelDocBuffer.Add(line.Substring(2).Trim());
                return true;
            }

            // line is just a comment
            if (first[0] == Tokens.COMMENT) return true;

            if (first == Tokens.SECTION_DEF) {
                // this defines a section
                // 'section' <identifier> ':'
                preprocessor.ParsePreprocessorExp(Tokens.PREPROCESSOR_DEF + line.Split(Tokens.DEF_MARKER)[0]);
                return true;
            }

            if (first == Tokens.SECTION_END) {
                preprocessor.ParsePreprocessorExp(Tokens.PREPROCESSOR_DEF + first);
                return true;
            }

            if (!isLabelOpen && line.Contains(Tokens.DEF) && Tokens.ValidType(first)) {
                // this defines a variable
                // <type> <identifier> ':' <arg>
                string[] tokens = line.Split(Tokens.DEF_MARKER);

                if (tokens.Length < 2) {
                    throw new SyntaxError("Variable definition", "<nil>");
                }

                // force type unless type is <var>
                string typeCast = first == Tokens.VAR ? "" : first;

                // get variable data
                MetaSymbol arg = ParseArguments(tokens[1].Split(Tokens.SEPARATOR), typeCast, strictType: true)[0];

                string varId = words[1];

                // remove ':' from variable name if it exists
                varId = varId[varId.Length - 1] == Tokens.DEF_MARKER ? varId.Substring(0, varId.Length - 1) : varId;
                Push(varId, arg);
                return true;
            }

            if (!isLabelOpen && line.Contains(Tokens.DEF)) {
                // this is a label definition
                if (isLabelOpen) {
                    // there was another label opened, close it
                    CloseBlock();
                }

                // parse the label and keep a reference to the result
                isLabelOpen = true;
                openLabel = ParseLabel();

                if (openLabel.inline) {
                    // this is an inline block and should be closed
                    CloseBlock();
                    isLabelOpen = false;
                }

                return true;
            }

            if (first == Tokens.END) {
                // close this label
                CloseBlock();
                isLabelOpen = false;
                return true;
            }

            if (isLabelOpen) {
                // label is currently open, append new lines to its buffer
                //labelBodyBuffer.Add(preprocessor.Compile(words));
                labelBodyBuffer.Add(line);
                return true;
            }

            // this statement is not a special block
            return false;
        }

        /// <summary>
        /// Handles label definitions
        /// [LBL, ARG*]
        /// assumes first word is a label
        /// </summary>
        LabelDef ParseLabel() {
            tokenNum = 1;
            var def = new LabelDef();
            // get name without ':', [label:] -> [label]
            def.identifier = words[0];
            var signature = new List<string>();
            var args = new List<string>();

            if (words.Length == 1) {
                // this label has no parameters (signature)
                def.signature = new string[0];
                def.identifier = def.identifier.Substring(0, words[0].Length - 1);
                return def;
            }

            bool open = false;
            bool variable = false;
            var expressionBuffer = new StringBuilder();

            for (int i = 1; i < words.Length; i++) {
                tokenNum++;
                string word = words[i].Trim();

                if (word[word.Length - 1] == Tokens.DEF_MARKER) {
                    // remove ':'
                    word = word.Substring(0, word.Length - 1);
                }

                if (word[0] == Tokens.OPEN_PAREN) {
                    // open parenthesis [(arg]
                    word = word.Substring(1);
                    open = true;
                }

                if (word[word.Length - 1] == Tokens.CLOSE_PAREN) {
                    // close parenthesis and store literal [arg)]
                    word = word.Substring(0, word.Length - 1);
                    args.Add(word);
                    open = false;
                    continue;
                }

                if (open) {
                    string tmp = word[word.Length - 1] == ',' ? word.Substring(0, word.Length - 1) : word;
                    // parentheses are open store value
                    if (variable) {
                        args.Add(tmp);
                    } else {
                        signature.Add(tmp);
                    }
                    variable = !variable;
                } else {
                    // if we have reached this point with no parenthesis this must
                    // be a label with an inline definition
                    // [label: (<arg>*) <expression>] | [label: <expression>]
                    expressionBuffer.Append(word);
                    expressionBuffer.Append(Tokens.SEPARATOR);
                }
            }
            def.signature = signature.ToArray();
            def.args = args.ToArray();
            tokenNum = 0;

            if (expressionBuffer.Length > 0) {
                // this label has an inline definition
                labelBodyBuffer.Add(expressionBuffer.ToString());
                // this will trigger the NonExpressionParser to close the block
                def.inline = true;
            }

            return def;
        }

        /// <summary>
        /// Handles multiline
        /// [line] { [line*] }
        /// </summary>
        bool ParseMultiline() {
            int index;

            if ((index = line.IndexOf(Tokens.OPEN_BRACE)) >= 0) {
                multiLineBuffer.Append(line.Remove(index, 1));
                multiLineBuffer.Append(Tokens.SEPARATOR);
                return true;
            }

            if ((index = line.IndexOf(Tokens.CLOSE_BRACE)) >= 0) {
                multiLineBuffer.Append(line.Remove(index, 1));

                string expression = multiLineBuffer.ToString();
                multiLineBuffer.Length = 0;

                Run(expression);
                return true;
            }

            if (multiLineBuffer.Length > 0) {
                multiLineBuffer.Append(line);
                multiLineBuffer.Append(Tokens.SEPARATOR);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles expressions
        /// [RX, INS, ARG*] | [INS, ARG*]
        /// </summary>
        Expression ParseExpression() {
            tokenNum = 1;
            var exp = new Expression();
            int start = 0;

            words[0] = preprocessor.ReplaceDefined(words[0].Trim());

            if (words[0][0] == Tokens.REGISTER_MARKER) {
                // first character in first word is 'R'
                // this must be a return register
                exp.returnRegister = ParseRegister(words[0]);
                // skip the first word in the loop since it's already
                // been parsed
                start = 1;
            } else {
                // we did not receive a return register
                // assign it to the accumulator
                exp.returnRegister = 1;
            }

            tokenNum += start;

            if (words.Length < start + 1) {
                // this expression does not have an instruction
                throw new SyntaxError("<instruction>", "<nil>");
            }

            string[] args = new string[words.Length - start - 1];
            Array.Copy(words, start + 1, args, 0, args.Length);

            exp.args = ParseArguments(args);
            exp.instruction = GetInstruction(words[start], exp.args);
            tokenNum = 0;
            return exp;
        }

        /// <summary>
        /// Parses arguments into Symbols
        /// </summary>
        /// <param name="strictType">all tokens must be of the same type</param>
        /// <returns></returns>
        MetaSymbol[] ParseArguments(string[] args, string expectedType = "", bool strictType = false) {
            var tokens = new List<MetaSymbol>();
            int tmpI;

            var strbuffer = new StringBuilder();
            var feed = new List<string>();
            bool openLiteral = false;
            bool innerExpression = false;

            for (int i = 0; i < args.Length; i++) {
                tokenNum++;
                string word = args[i].Trim();

                if (word == "") continue;

                //
                // STEP 1:
                // checking for lists and vectors (recursive)
                //

                if (!innerExpression && word[0] == Tokens.OPEN_PAREN) {
                    // the start of another expression
                    innerExpression = true;
                    feed.Add(word.Substring(1));
                    continue;
                }

                if (innerExpression && word[word.Length - 1] == Tokens.CLOSE_PAREN) {
                    // the end of an inner expression
                    innerExpression = false;
                    feed.Add(word.Substring(0, word.Length - 1));

                    var inner = ParseArguments(feed.ToArray(), expectedType, strictType: true);

                    if (expectedType == "" || expectedType == Tokens.VEC) {
                        tokens.Add(new Symbol<Vector3>(Tokens.VEC, ParseVector(inner)));
                    } else {
                        var list = new List<MetaSymbol>();
                        list.AddRange(inner);
                        tokens.Add(new Symbol<List<MetaSymbol>>(Tokens.LIST, list));
                    }

                    feed = new List<string>();
                    expectedType = "";
                    continue;
                }

                if (innerExpression) {
                    // we're in the middle of an inner expression, keep appending
                    feed.Add(word);
                    continue;
                }

                //
                // STEP 2:
                // checking for string literals
                //

                if (word[0] == Tokens.LITERAL_MARKER) {
                    // [$string] -> [string]
                    tokens.Add(new Symbol<string>(Tokens.STR, word.Substring(1)));
                    continue;
                }

                if (!openLiteral && Tokens.Quote(word[0])) {
                    // the start of a string literal ["str]

                    if (word.Length > 1 && Tokens.Quote(word[word.Length - 1])) {
                        // quote is only a single word ["str"] append [str]
                        tokens.Add(new Symbol<string>(Tokens.STR, word.Substring(1, word.Length - 2)));
                        continue;
                    }

                    // remove quote and append [str]
                    strbuffer.Append(word.Substring(1));

                    openLiteral = true;
                    continue;
                }

                if (openLiteral && Tokens.Quote(word[word.Length - 1])) {
                    // the end of a string literal [str"] append [ str]
                    strbuffer.Append(Tokens.SEPARATOR);
                    strbuffer.Append(word.Substring(0, word.Length - 1));

                    // store buffer
                    tokens.Add(new Symbol<string>(Tokens.STR, strbuffer.ToString()));
                    strbuffer = new StringBuilder();

                    openLiteral = false;
                    continue;
                }

                if (openLiteral) {
                    // we're in the middle of a literal, keep appending
                    strbuffer.Append(Tokens.SEPARATOR);
                    strbuffer.Append(word);
                    continue;
                }

                word = preprocessor.ReplaceDefined(word);

                //
                // STEP 3:
                // checking for explicit types [type <str|int|num>] (casting)
                //

                if (Tokens.ValidCastTypes(word)) {
                    expectedType = word;
                    continue;
                }

                // next word
                if (expectedType != "") {
                    switch (expectedType) {
                        case Tokens.STR:
                            tokens.Add(new Symbol<string>(Tokens.STR, word));
                            break;
                        case Tokens.INT:
                            tmpI = (word[0] == Tokens.REGISTER_MARKER) ? ParseRegister(word) : int.Parse(word);
                            tokens.Add(new Symbol<int>(Tokens.INT, tmpI));
                            break;
                        case Tokens.NUM:
                            tokens.Add(ParseArguments(new string[] { word })[0]);
                            break;
                    }
                    if (!strictType) expectedType = "";
                    continue;
                }

                //
                // STEP 4:
                // check if this is a reference to a register 'R' or a dereference symbol '&'
                //

                if (word[0] == Tokens.REGISTER_MARKER) {
                    // this is a reference to a register, add a pointer to it
                    tokens.Add(new Symbol<int>(Tokens.INT, ParseRegister(word)));
                    continue;
                }

                if (word[0] == Tokens.VALUE_MARKER) {
                    // token is a '&', this requires a token after it, otherwise it's nil
                    if (word.Length < 2) {
                        tokens.Add(MetaSymbol.Nil);
                        continue;
                    }

                    // remove '&'
                    word = preprocessor.ReplaceDefined(word.Substring(1));

                    if (word[0] == Tokens.REGISTER_MARKER) {
                        // the following token is a register marker
                        // '&R12' -> 'R12' -> 12
                        int ptr = ParseRegister(word);
                        // get the value stored in the register
                        tokens.Add(GetRegister(ptr));
                        continue;
                    }

                    // check if the word is an instruction
                    // having a '&' precede an instruction label means we should execute it
                    if ((tmpI = GetInstructionAddress(word)) > -1) {
                        // this marker is an instruction, get its value
                        // note that only labels can be executed in line and have their
                        // return value replace its token.
                        // no parameters can be passed since it is expected that this label
                        // is part of the current block and therefore should use the same parameters
                        // as its parent.
                        var instruction = GetAddress<Instruction>(tmpI, Tokens.LBL);
                        // execute the instruction
                        if (instruction.Type == Tokens.LBL) {
                            instruction.Value.Call(new MetaSymbol[0], this);
                            // replace this token with the return value of the label
                            tokens.Add(GetRegister(1));
                            continue;
                        }
                    }

                    // dereference marker used to get the value of a single word string literal
                    // obsolete: use '$' instead
                    tokens.Add(new Symbol<string>(Tokens.STR, word));
                    //throw new SyntaxError("<pointer>", word);
                    continue;
                }

                //
                // STEP 5:
                // check if this is a number
                //
                float value;
                if (float.TryParse(word, out value)) {
                    tokens.Add(new Symbol<float>(Tokens.NUM, value));
                    continue;
                }

                //
                // STEP 6:
                // check if this is an instruction pointer
                //

                if ((tmpI = GetInstructionAddress(word)) > -1) {
                    // this instruction exists, add a token with its address
                    tokens.Add(new Symbol<int>(Tokens.INT, tmpI));
                    continue;
                }

                // reached bottom of loop and we still don't recognize this token
                throw new UndefinedError(word);
            }

            return tokens.ToArray();
        }

        /// <summary>
        /// Closes an open label block,
        /// storing it in the symbol table
        /// </summary>
        void CloseBlock() {
            openLabel.identifier = preprocessor.AddModule(openLabel.identifier);
            var label = new Label(
                openLabel.identifier,
                openLabel.signature,
                openLabel.args,
                labelBodyBuffer.ToArray(),
                labelDocBuffer.ToArray()
            );

            var ins = new Symbol<Instruction>(Tokens.LBL, label);

            if (preprocessor.Module != null) {
                ins.Value.ObjectIdentifier = preprocessor.Module;
            }

            symbolTable.Add(ins);
            labelDocBuffer.Clear();

            if (debugParser) {
                debugOut(openLabel);
            }

            if (debugPreprocessor) {
                foreach (string ln in labelBodyBuffer) {
                    debugOut(ln);
                }
            }

            labelBodyBuffer.Clear();
        }

        /// <summary>
        /// Gets the address of a register beased on its name
        /// "R12" -> 12
        /// </summary>
        int ParseRegister(string word) {
            if (word.Length < 2) {
                // this word does not have a number assigned to it
                throw new SyntaxError("<ptr>", "<nil>");
            }

            string regname = word.Substring(1);

            int reg;
            if (!int.TryParse(regname, out reg)) {
                throw new SyntaxError("<ptr>", regname);
            }

            return reg;
        }

        List<T> ParseList<T>(MetaSymbol[] items, string expectedType) {
            var list = new List<T>();

            foreach (var item in items) {
                list.Add(((Symbol<T>)item).Value);
            }

            return list;
        }

        Vector3 ParseVector(MetaSymbol[] items) {
            Vector3 vec = new Vector3();

            if (items.Length >= 1) {
                vec.x = ((Symbol<float>)items[0]).Value;
            }

            if (items.Length >= 2) {
                vec.y = ((Symbol<float>)items[1]).Value;
            }

            if (items.Length >= 3) {
                vec.z = ((Symbol<float>)items[2]).Value;
            }

            return vec;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Initialize registers with null value
        /// </summary>
        void InitRegisters() {
            symbolTable.Add(new Symbol<float>(Tokens.NUM, 1));

            for (int i = 1; i < registers; i++) {
                symbolTable.Add(MetaSymbol.Nil);
            }
        }

        /// <summary>
        /// Resets the line number;
        /// </summary>
        public void ResetLineNum() {
            lineNum = 0;
        }

        public object FindObject(string name, bool noerror = false) {
            for (int i = registers; i < symbolTable.Count; i++) {
                var sym = symbolTable[i];

                if (sym.Type == Tokens.OBJ) {
                    var obj = ((Symbol<Obj>)sym).Value;

                    if (obj.Identifier == name) {
                        return obj.Reference;
                    }
                }
            }

            if (!noerror) throw new UndefinedError(name);

            return null;
        }

        /// <summary>
        /// Finds address of C# object stored in the symbol table
        /// </summary>
        public int FindObjectAddress(string name, bool noerror = false) {
            for (int i = registers; i < symbolTable.Count; i++) {
                var sym = symbolTable[i];

                if (sym.Type == Tokens.OBJ) {
                    var obj = ((Symbol<Obj>)sym).Value;

                    if (obj.Identifier == name) {
                        return i;
                    }
                }
            }

            if (!noerror) throw new UndefinedError(name);

            return -1;
        }

        /// <summary>
        /// Finds and returns an instruction
        /// </summary>
        public Symbol<Instruction> GetInstruction(string label, MetaSymbol[] signature) {
            // loop starting from after the last register as this
            // is where instructions are stored
            for (int i = symbolTable.Count - 1; i >= 0; i--) {
                var sym = symbolTable[i];

                if (Tokens.Instruction(sym)) {
                    // this address is an instruction
                    var ins = (Symbol<Instruction>)sym;

                    if ((ins.Value.Identifier == label
                        || ins.Value.Identifier == preprocessor.AppendObjectId(ins.Value.ObjectIdentifier, label))
                        && ins.Value.SignatureMatch(signature)) {
                        // instruction matches label and signature
                        // return instruction
                        return ins;
                    }
                }
            }

            throw new UndefinedError(label);
        }

        /// <summary>
        /// Gets the address of an instruction
        /// Note that this does not account for overloaded methods
        /// and will simply get the first match, use FindInstruction()
        /// to match both the identifier and signature of instruction
        /// </summary>
        public int GetInstructionAddress(string label) {
            for (int i = symbolTable.Count - 1; i >= 0; i--) {
                var sym = symbolTable[i];

                if (Tokens.Instruction(sym)) {
                    // this address is an instruction
                    var ins = (Symbol<Instruction>)sym;

                    if (ins.Value.Identifier == label
                        || ins.Value.Identifier == preprocessor.AppendObjectId(ins.Value.ObjectIdentifier, label)) {
                        // instruction matches label
                        // return address of instruction
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Sets a register to a value (from R0 to registers-1)
        /// </summary>
        public void SetRegister(int register, MetaSymbol value) {
            if (register > registers - 1) {
                throw new PermissionError("modify address", register);
            }

            symbolTable[register] = value;
        }

        /// <summary>
        /// Sets parameter registers (R10+) to values
        /// </summary>
        internal void SetParamRegisters(MetaSymbol[] values) {
            for (int i = 0; i < values.Length; i++) {
                SetRegister(i + 10, values[i]);
            }
        }

        internal void SetAddress(int addr, MetaSymbol value) {
            symbolTable[addr] = value;
        }

        /// <summary>
        /// Gets the symbol in a register
        /// </summary>
        public MetaSymbol GetRegister(int register) {
            if (register > registers - 1) {
                // uncomment to not allow access to other addresses
                //throw new PermissionError("access address", register);
            }

            return symbolTable[register];
        }

        /// <summary>
        /// Gets a register and casts the value to a desired type
        /// </summary>
        public Symbol<T> GetRegister<T>(int register, string type) {
            MetaSymbol value = GetRegister(register);

            if (value.Type != type && type != Tokens.ANY) {
                throw new SyntaxError(type, value.Type);
            }

            return (Symbol<T>)value;
        }

        /// <summary>
        /// Gets the symbol stored at address
        /// </summary>
        public MetaSymbol GetAddress(int address) {
            return symbolTable[address];
        }

        /// <summary>
        /// Gets a register and casts the value to a desired type
        /// </summary>
        public Symbol<T> GetAddress<T>(int addr, params string[] types) {
            MetaSymbol value = GetRegister(addr);

            foreach (string type in types) {
                if (value.Type == type) {
                    return (Symbol<T>)value;
                }
            }

            throw new SyntaxError(value.Type, types[0]);
        }

        /// <summary>
        /// Gets the size of the SymbolTable
        /// </summary>
        public int TableSize() {
            return symbolTable.Count;
        }

        /// <summary>
        /// Adds a native C# object to the table
        /// </summary>
        public void AddObj(Obj obj) {
            symbolTable.Add(new Symbol<Obj>(Tokens.OBJ, obj));
        }

        /// <summary>
        /// Adds a native C# method to the table
        /// </summary>
        public void AddFun(Instruction fun) {
            var ins = new Symbol<Instruction>(Tokens.FUN, fun);
            ins.Value.ObjectAddr = FindObjectAddress(ins.Value.ObjectIdentifier);
            symbolTable.Add(ins);
        }

        #endregion

        public override string ToString() {
            var sb = new StringBuilder();

            for (int i = 0; i < symbolTable.Count; i++) {
                sb.Append("[");
                sb.Append(i.ToString().PadLeft(5, '0'));
                sb.Append("] ");
                sb.Append(symbolTable[i]);
                sb.AppendLine();

                if (i == registers - 1) {
                    sb.AppendLine("==========");
                    continue;
                }

                if ((i + 1) % 10 == 0 || i == 3) {
                    sb.AppendLine("----------");
                }
            }

            return sb.ToString() + preprocessor.ToString();
        }
    }
}
