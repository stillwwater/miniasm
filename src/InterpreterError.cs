using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniASM
{
    class InterpreterError : Exception
    {
        public InterpreterError(string message)
            : base(message) { }

        public InterpreterError(string error, string message, object item)
            : base(string.Format("{0}Error: {1} {2}", error, message, item)) { }

        public InterpreterError(string error, object message)
            : base(string.Format("{0}Error: {1}", error, message)) { }
    }

    class SyntaxError : InterpreterError
    {
        public SyntaxError(object expected, object found)
            : base(string.Format("SyntaxError: expected {0}, found {1}", expected, found)) { }
    }

    class UndefinedError : InterpreterError
    {
        public UndefinedError(object undefined)
            : base(string.Format("Error: {0} is undefined or <nil>", undefined)) { }
    }

    class ParameterError : InterpreterError
    {
        public ParameterError(object expected, object found)
            : base(string.Format("ParameterError: expected {0}, found {1}", expected, found)) { }
    }

    class PermissionError : InterpreterError
    {
        public PermissionError(string action, object obj)
            : base(string.Format("PermissionError: cannot {0} {1}", action, obj)) { }
    }

    class TypeError : InterpreterError
    {
        public TypeError(string errorType, object expected, object found)
            : base(string.Format("TypeError: invalid {0} type: expected {1}, found {2}", errorType, expected, found)) { }
    }
}
