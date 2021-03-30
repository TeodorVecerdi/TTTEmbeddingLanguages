using System;
using System.IO;
using Embedded;

namespace Lua {
    public static class Program {
        private const string resourcesDirectoryName = "resources";
        private static readonly string currentDirectory = Environment.CurrentDirectory;
        private static readonly string resources = Path.Combine(currentDirectory, resourcesDirectoryName);

        public static void Main() {
            using var lua = new LuaWrapper(new () {{"Load .NET Types", true},{"Load File", $"{resources}/hello.lua"}});
            var hello = lua.Call<HelloWorld>("CreateObject", "TEODOR");
            hello.Say();
        }
    }
}