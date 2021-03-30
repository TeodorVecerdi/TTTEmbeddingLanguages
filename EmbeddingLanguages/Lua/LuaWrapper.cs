using System.Collections.Generic;
using NLua;
using Shared;

namespace Lua {
    public class LuaWrapper : EmbeddedLanguage<NLua.Lua> {
        public LuaWrapper(Dictionary<string, dynamic> args) {
            if(args.ContainsKey("Load .NET Types") && args["Load .NET Types"]) State.LoadCLRPackage();
            if(args.ContainsKey("Load File")) State.DoFile(args["Load File"]);
        }

        protected override NLua.Lua AcquireState() {
            return new();
        }

        protected override TRet GetVal<TRet>(string key) where TRet : class {
            return State[key] as TRet;
        }

        protected override void SetVal<TVal>(string key, TVal value) {
            State[key] = value;
        }

        protected override object[] CallFunc(string funcKey, object[] parameters) {
            var func = State[funcKey] as LuaFunction;
            return func.Call(parameters);
        }
    }
}