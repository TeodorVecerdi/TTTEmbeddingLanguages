using System;
using System.IO;

namespace Lua {
    public static class Program {
        private static string GetResourcePath(string filePath) {
            return $"{Path.Combine(Environment.CurrentDirectory, "resources", filePath)}";
        }

        public static void Main() {
            using var lua = new NLua.Lua();
            lua.DoString("print('Hello, World!')");
        }
    }
}