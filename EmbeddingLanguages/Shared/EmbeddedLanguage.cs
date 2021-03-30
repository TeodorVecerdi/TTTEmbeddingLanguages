using System;
using System.Collections.Generic;

namespace Shared {
    public abstract class EmbeddedLanguage<T> : IDisposable where T : class, IDisposable {
        private readonly T state;
        protected T State => state;


        protected EmbeddedLanguage() {
            state = AcquireState();
        }

        protected abstract T AcquireState();
        protected abstract TRet GetVal<TRet>(string key) where TRet : class;
        protected abstract void SetVal<TVal>(string key, TVal value);
        protected abstract object[] CallFunc(string funcKey, object[] parameters);

        public TRet Get<TRet>(string key) where TRet : class {
            return GetVal<TRet>(key);
        }

        public void Set<TVal>(string key, TVal value) {
            SetVal<TVal>(key, value);
        }

        public TRet Call<TRet>(string key, params object[] parameters) where TRet : class {
            return CallFunc(key, parameters)[0] as TRet;
        }

        public (TA, TB) Call<TA, TB>(string key, params object[] parameters) where TA : class where TB : class {
            var res = CallFunc(key, parameters);
            return (res[0] as TA, res[1] as TB);
        }

        public (TA, TB, TC) Call<TA, TB, TC>(string key, params object[] parameters)
            where TA : class where TB : class where TC : class {
            var res = CallFunc(key, parameters);
            return (res[0] as TA, res[1] as TB, res[2] as TC);
        }

        public object[] Call(string key, params object[] parameters) => CallFunc(key, parameters);

        public void Dispose() {
            state.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}