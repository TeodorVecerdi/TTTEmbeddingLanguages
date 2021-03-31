using System;
using System.Collections.Generic;
using Python.Runtime;

namespace Python {
    public static class PythonProgram {
        internal static void Main() {
        }

        public static void Initialize() {
            Runtime.Runtime.PythonDLL = "python39.dll";
        }
        
        public static long WriteMem(int writes, Func<long> measureFunction, Action cleanupFunction) {
            long start, end;
            using (var state = Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    scope.Exec("a = None");
                    cleanupFunction?.Invoke();
                    start = measureFunction();
                    Write(writes, scope);
                    cleanupFunction?.Invoke();
                    end = measureFunction();
                }
            }
            return end - start;
        }
        
        public static TimeSpan WriteTime(int writes, Func<DateTime> measureFunction, Action cleanupFunction) {
            DateTime start, end;
            using (var state = Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    scope.Exec("a = None");
                    cleanupFunction?.Invoke();
                    start = measureFunction();
                    Write(writes, scope);
                    cleanupFunction?.Invoke();
                    end = measureFunction();
                }
            }
            return end - start;
        }

        private static void Write(int writes, PyScope scope) {
            for (int i = 0; i < writes; i++) {
                scope.Set("a", i);
            }
        }
        
        public static long ReadMem(int reads, Func<long> measureFunction, Action cleanupFunction) {
            long start, end;
            var a = 0;
            using (var state = Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    scope.Exec("a = 1");
                    cleanupFunction?.Invoke();
                    start = measureFunction();
                    Read(reads, ref a, scope);
                    
                    cleanupFunction?.Invoke();
                    end = measureFunction();
                    
                    scope.Set("a", a);
                }
            }

            return end - start;
        }

        public static TimeSpan ReadTime(int reads, Func<DateTime> measureFunction, Action cleanupFunction) {
            DateTime start, end;
            var a = 0;
            using (var state = Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    scope.Exec("a = 1");
                    cleanupFunction?.Invoke();
                    start = measureFunction();
                    Read(reads, ref a, scope);
                    
                    cleanupFunction?.Invoke();
                    end = measureFunction();
                    
                    scope.Set("a", a);
                }
            }

            return end - start;
        }

        private static void Read(int reads, ref int a, PyScope scope) {
            for (int i = 0; i < reads; i++) {
                a = scope.Get<int>("a");
            }
        }
    }
}