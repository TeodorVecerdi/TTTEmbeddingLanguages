using System;

namespace Lua {
    public static class LuaProgram {
        public static void Main() {
        }

        public static long WriteMem(int writes, Func<long> measureFunction, Action cleanupFunction) {
            long start, end;
            using (var lua = new NLua.Lua()) {
                cleanupFunction?.Invoke();
                start = measureFunction();
                for (int i = 0; i < writes; i++) {
                    lua["a"] = i;
                }
                cleanupFunction?.Invoke();
                end = measureFunction();
            }

            return end - start;
        }
        
        public static TimeSpan WriteTime(int writes, Func<DateTime> measureFunction, Action cleanupFunction) {
            DateTime start, end;
            using (var lua = new NLua.Lua()) {
                cleanupFunction?.Invoke();
                start = measureFunction();
                for (int i = 0; i < writes; i++) {
                    lua["a"] = i;
                }
                cleanupFunction?.Invoke();
                end = measureFunction();
            }

            return end - start;
        }
        
        public static long ReadMem(int reads, Func<long> measureFunction, Action cleanupFunction) {
            long start, end;
            var a = 0;
            using (var lua = new NLua.Lua()) {
                lua["a"] = a;
                cleanupFunction?.Invoke();
                start = measureFunction();
                for (int i = 0; i < reads; i++) {
                    a = lua.GetInteger("a");
                }
                cleanupFunction?.Invoke();
                end = measureFunction();
                lua["a"] = a;
            }

            return end - start;
        }
        
        public static TimeSpan ReadTime(int reads, Func<DateTime> measureFunction, Action cleanupFunction) {
            DateTime start, end;
            var a = 0;
            using (var lua = new NLua.Lua()) {
                lua["a"] = a;
                cleanupFunction?.Invoke();
                start = measureFunction();
                for (int i = 0; i < reads; i++) {
                    a = lua.GetInteger("a");
                }
                cleanupFunction?.Invoke();
                end = measureFunction();
                lua["a"] = a;
            }

            return end - start;
        }
    }
}