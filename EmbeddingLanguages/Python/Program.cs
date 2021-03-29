using System;
using System.IO;
using System.Net.Sockets;
using Python.Runtime;

namespace Python {
    internal static class Program {
        private const string resourcesDirectoryName = "resources";
        private static readonly string outDirectory = Environment.CurrentDirectory;
        private static readonly string resourcesDirectory = Path.Combine(outDirectory, resourcesDirectoryName);
         
        internal static void Main() {
            // Adding resources/ folder to process path variable so that I
            // can use `Import` on the python scripts in resources folder
            var path = Environment.GetEnvironmentVariable("Path");
            path = resourcesDirectory + Path.PathSeparator + path;
            Environment.SetEnvironmentVariable("Path", path);
            
            // Set python dll (part of path variable)
            Runtime.Runtime.PythonDLL = "python39.dll";
            
            using (Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    // Importing a module
                    var hello = scope.Import("resources.hello");
                    // Calling a function
                    hello.say(1, 2, 3, "Hello", 4.245f);
                }
            }
        }
    }
}