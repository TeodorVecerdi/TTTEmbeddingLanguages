using System;
using System.Collections.Generic;
using Python;

namespace Measurements {
    class Program {
        static void Main(string[] args) {
            var memoryCleanup = new Action(() => {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
            
            PythonProgram.Initialize();
            
            var actions = 16384;
            var py_writeNormalMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_writeCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup), memoryCleanup, memoryCleanup, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_readNormalMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_readCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, memoryCleanup, GC.GetAllocatedBytesForCurrentThread)[0];
            
            var sampleCount = 512;
            var py_writeNormalTime = Measure(sampleCount, (func, action) => PythonProgram.WriteTime(actions, func, action), memoryCleanup, null, () => DateTime.Now);
            var py_readNormalTime = Measure(sampleCount, (func, action) => PythonProgram.ReadTime(actions, func, action), memoryCleanup, null, () => DateTime.Now);
            
        }
        
        internal static List<TR> Measure<TM, TR>(int sampleCount, Func<Func<TM>, Action, TR> function, Action cleanupFunctionSelf, Action cleanupFunction, Func<TM> measureFunction) {
            var samples = new List<TR>();
            for (var i = 0; i < sampleCount; i++) {
                cleanupFunctionSelf();
                samples.Add(function(measureFunction, cleanupFunction));
            }

            return samples;
        }
    }
}