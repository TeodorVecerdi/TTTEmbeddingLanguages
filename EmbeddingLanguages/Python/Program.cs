using System.Diagnostics;
using Python.Runtime;

namespace Python {
    internal static class Program {
        internal static void Main() {
            // Set python dll (part of path variable)
            Runtime.Runtime.PythonDLL = "python39.dll";
            
            using (Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    var hello = scope.Import("resources.hello") as PyScope;
                    Debug.Assert(hello != null);
                    // Get types
                    var testComponent = hello.Get("TestComponent");
                    var testSecondComponent = hello.Get("TestSecondComponent");

                    hello.Set("first", testComponent.Invoke());
                    hello.Set("second", testSecondComponent.Invoke());
                    hello.Get("run").Invoke();
                }
            }
        }
    }
}