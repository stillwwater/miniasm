using System.Collections;
using System.Collections.Generic;
using MiniASM;
using MiniASM.Builtin;

public class MiniAPI {
    protected Interpreter mini;
    protected string apiName;

    public virtual void Init(Interpreter mini) {
        this.mini = mini;
    }

    public static void CreateReference(string name, MiniAPI api) {
        Interpreter.Instance.AddObj(new Obj(name, api));
        api.apiName = name;
    }

    public void AddFun(string method, params MetaSymbol[] args) {
        string fullname = apiName + "." + method.ToLower();
        mini.AddFun(new Fun(apiName, fullname, method, args));
    }
}
