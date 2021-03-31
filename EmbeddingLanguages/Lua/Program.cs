using System;
using System.IO;

namespace Lua {
    public static class Program {
        public static void Main() {
            RunGC();
        }

        internal static void RunNormal() {
            int a;
            using (var lua = new NLua.Lua()) {
                for (int i = 0; i < 10000; i++) {
                    lua.DoString($"a{i} = {i}");
                }

                a = lua.GetInteger("a9999");
            }

            Console.WriteLine(a);
        }

        internal static void RunGC() {
            int a;
            using (var lua = new NLua.Lua()) {
                GC.Collect();
                for (int i = 0; i < 10000; i++) {
                    lua.DoString($"a{i} = {i}");
                }

                GC.Collect();
                a = lua.GetInteger("a9999");
            }

            GC.Collect();
            Console.WriteLine(a);
        }
    }
}