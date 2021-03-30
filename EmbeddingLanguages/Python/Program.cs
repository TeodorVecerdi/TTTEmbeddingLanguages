﻿using System;
using System.Diagnostics;
using Python.Runtime;
using Embedded.Data;
using Embedded.GameObject;

namespace Python {
    internal static class Program {
        internal static void Main() {
            // Set python dll (part of path variable)
            Runtime.Runtime.PythonDLL = "python39.dll";

            using (Py.GIL()) {
                // Load main module
                using (var scope = Py.Import("resources.main") as PyScope) {
                    Debug.Assert(scope != null);
                    // Experiment1(hello);
                    Experiment_CreateObject(scope);
                }
            }
        }

        private static void Experiment_CreateObject(PyScope scope) {
            var method = scope.Get("CreateGameObject");
            var transform = new Transform(
                new Vector3(1.234f, 2.345f, 3.456f),
                new Vector3(4.567f, 5.678f, 6.789f),
                new Vector3(7.890f, 8.901f, 9.012f)
            );
            var gameObject = method.Invoke(transform.ToPython()).AsManagedObject(typeof(GameObject));
            Console.WriteLine(gameObject);
        }

        private static void Experiment1(PyScope scope) {
            var testComponent = scope.Get("TestComponent");
            var testSecondComponent = scope.Get("TestSecondComponent");

            scope.Set("first", testComponent.Invoke());
            scope.Set("second", testSecondComponent.Invoke());
            scope.Get("run").Invoke();
        }
    }
}