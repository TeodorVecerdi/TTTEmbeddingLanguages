using System;
using System.Collections.Generic;
using Lua;
using Python;

namespace Measurements {
    class Program {
        static void Main(string[] args) {
            var memoryCleanup = new Action(() => {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });

            PythonProgram.Initialize();

            var actions = 65536;
            var py_writeNormalMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_writeCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup), memoryCleanup, memoryCleanup,
                                                GC.GetAllocatedBytesForCurrentThread)[0];
            var py_readNormalMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_readCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, memoryCleanup,
                                               GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_writeNormalMemory = Measure(1, (func, cleanup) => LuaProgram.WriteMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_writeCleanupMemory = Measure(1, (func, cleanup) => LuaProgram.WriteMem(actions, func, cleanup), memoryCleanup, memoryCleanup,
                                                 GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_readNormalMemory = Measure(1, (func, cleanup) => LuaProgram.ReadMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_readCleanupMemory = Measure(1, (func, cleanup) => LuaProgram.ReadMem(actions, func, cleanup), memoryCleanup, memoryCleanup,
                                                GC.GetAllocatedBytesForCurrentThread)[0];

            var sampleCount = 512;
            var py_writeNormalTimeSamples = Measure(sampleCount, (func, action) => PythonProgram.WriteTime(actions, func, action), memoryCleanup, null, () => DateTime.Now);
            var py_readNormalTimeSamples = Measure(sampleCount, (func, action) => PythonProgram.ReadTime(actions, func, action), memoryCleanup, null, () => DateTime.Now);
            var lua_writeNormalTimeSamples = Measure(sampleCount, (func, action) => LuaProgram.WriteTime(actions, func, action), memoryCleanup, null, () => DateTime.Now);
            var lua_readNormalTimeSamples = Measure(sampleCount, (func, action) => LuaProgram.ReadTime(actions, func, action), memoryCleanup, null, () => DateTime.Now);

            var py_writeNormalTime = Process(py_writeNormalTimeSamples);
            var py_readNormalTime = Process(py_readNormalTimeSamples);
            var lua_writeNormalTime = Process(lua_writeNormalTimeSamples);
            var lua_readNormalTime = Process(lua_readNormalTimeSamples);

            Console.WriteLine($"py_writeNormalMemory =>\n\t{py_writeNormalMemory} total bytes for {actions} writes,\n\t{py_writeNormalMemory / (double) actions} bytes/write");
            Console.WriteLine($"py_writeCleanupMemory =>\n\t{py_writeCleanupMemory} total bytes for {actions} writes,\n\t{py_writeCleanupMemory / (double) actions} bytes/write");
            Console.WriteLine($"py_readNormalMemory =>\n\t{py_readNormalMemory} total bytes for {actions} reads,\n\t{py_readNormalMemory / (double) actions} bytes/read");
            Console.WriteLine($"py_readCleanupMemory =>\n\t{py_readCleanupMemory} total bytes for {actions} reads,\n\t{py_readCleanupMemory / (double) actions} bytes/read");
            Console.WriteLine($"lua_writeNormalMemory =>\n\t{lua_writeNormalMemory} total bytes for {actions} writes,\n\t{lua_writeNormalMemory / (double) actions} bytes/write");
            Console.WriteLine($"lua_writeCleanupMemory =>\n\t{lua_writeCleanupMemory} total bytes for {actions} writes,\n\t{lua_writeCleanupMemory / (double) actions} bytes/write");
            Console.WriteLine($"lua_readNormalMemory =>\n\t{lua_readNormalMemory} total bytes for {actions} reads,\n\t{lua_readNormalMemory / (double) actions} bytes/read");
            Console.WriteLine($"lua_readCleanupMemory =>\n\t{lua_readCleanupMemory} total bytes for {actions} reads,\n\t{lua_readCleanupMemory / (double) actions} bytes/read");
            
            Console.WriteLine($"py_writeNormalTime => \n\tMin: {py_writeNormalTime.min.TotalMilliseconds}ms\n\tMax: {py_writeNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_writeNormalTime.avg.TotalMilliseconds}ms");
            Console.WriteLine($"py_readNormalTime => \n\tMin: {py_readNormalTime.min.TotalMilliseconds}ms\n\tMax: {py_readNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_readNormalTime.avg.TotalMilliseconds}ms");
            Console.WriteLine($"lua_writeNormalTime => \n\tMin: {lua_writeNormalTime.min.TotalMilliseconds}ms\n\tMax: {lua_writeNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_writeNormalTime.avg.TotalMilliseconds}ms");
            Console.WriteLine($"lua_readNormalTime => \n\tMin: {lua_readNormalTime.min.TotalMilliseconds}ms\n\tMax: {lua_readNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_readNormalTime.avg.TotalMilliseconds}ms");
        }

        internal static (TimeSpan min, TimeSpan max, TimeSpan avg) Process(List<TimeSpan> samples) {
            TimeSpan min, max, sum;
            min = max = samples[0];
            sum = TimeSpan.Zero;
            foreach (var timeSpan in samples) {
                sum += timeSpan;
                if (timeSpan < min) min = timeSpan;
                if (timeSpan > max) max = timeSpan;
            }

            return (min, max, sum / samples.Count);
        }

        internal static List<TR> Measure<TM, TR>(int sampleCount, Func<Func<TM>, Action, TR> function, Action cleanupFunctionSelf, Action cleanupFunction,
                                                 Func<TM> measureFunction) {
            var samples = new List<TR>();
            for (var i = 0; i < sampleCount; i++) {
                cleanupFunctionSelf();
                samples.Add(function(measureFunction, cleanupFunction));
            }

            return samples;
        }
    }
}