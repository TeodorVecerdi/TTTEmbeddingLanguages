using System;
using System.IO;

namespace Lua {
    public static class Program {
        private const string resourcesDirectoryName = "resources";
        private static readonly string currentDirectory = Environment.CurrentDirectory;
        private static readonly string resources = Path.Combine(currentDirectory, resourcesDirectoryName);

        public static void Main() {
            using var lua = new NLua.Lua();
            
            lua.LoadCLRPackage();
            lua.DoFile($"{resources}/hello.lua");
        }
    }
}