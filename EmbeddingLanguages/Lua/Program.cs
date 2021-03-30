using System;
using System.IO;
using Embedded.Data;
using NLua;

namespace Lua {
    public static class Program {
        private const string resourcesDirectoryName = "resources";
        private static readonly string currentDirectory = Environment.CurrentDirectory;
        private static readonly string resources = Path.Combine(currentDirectory, resourcesDirectoryName);

        public static void Main() {
            using var lua = new NLua.Lua();
            lua.LoadCLRPackage();
            lua.DoFile($"{resources}/hello.lua");
            var hello = (Vector3)(lua["CreateVector"] as LuaFunction).Call(0.5f, 2.345f, -6.9f)[0];
            Console.WriteLine(hello);
        }
    }
}