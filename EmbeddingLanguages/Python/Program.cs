using Python.Runtime;

namespace Python {
    internal static class Program {
        internal static void Main() {
            // Set python dll (part of path variable)
            Runtime.Runtime.PythonDLL = "python39.dll";
            using var state = Py.GIL();
            using var baseScope = Py.CreateScope();
            baseScope.Exec("print('Hello, World!')");
        }
    }
}