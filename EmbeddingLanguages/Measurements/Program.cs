using System;
using System.Collections.Generic;
using System.Linq;
using Embedded;
using Embedded.Data;
using Lua;
using Python;
using Python.Runtime;
using UnityEngine;

namespace Measurements {
    class Program {
        static IEnumerable<int> Range(int start, int end) {
            for (var i = start; i < end; i++) {
                yield return i;
            }
        }
        
        static void Main(string[] args) {
            // var size = sizeof(float) * (9 + 3 * 3) * 3;
            // Console.WriteLine(size);
            PythonProgram.Initialize();
            CompareComplex();
        }

        internal static void CompareComplex() {
            var memoryCleanup = new Action(() => {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
            
            // const int actions = 1048576; // 1<<20
            const int actions = 16384; // 1<<14
            var vector = new Vector3(1.0f, 2.0f, 3.0f);
            var transform = new Transform(new Vector3(1.0f, 2.0f, 3.0f), new Vector3(11.0f, 22.0f, 33.0f), new Vector3(111.0f, 222.0f, 333.0f));
            var bigObject = new BigObject();
            var biggerObject = new BiggerObject();
            
            var a = Range(0, actions).Select(i => $"a{i}").ToList();
            var b = Range(0, actions).Select(i => $"b{i}").ToList();
            var c = Range(0, actions).Select(i => $"c{i}").ToList();
            var d = Range(0, actions).Select(i => $"d{i}").ToList();
            var e = Range(0, actions).Select(i => $"e{i}").ToList();

            var pythonWriteInt = new Action<PyScope, int>((scope, idx) => scope.Set($"a", idx));
            var pythonWriteVector3 = new Action<PyScope, int>((scope, idx) => scope.Set($"b", vector));
            var pythonWriteTransform = new Action<PyScope, int>((scope, idx) => scope.Set($"c", transform));
            var pythonWriteBigObj = new Action<PyScope, int>((scope, idx) => scope.Set($"d", bigObject));
            var pythonWriteBiggerObj = new Action<PyScope, int>((scope, idx) => scope.Set($"e", biggerObject));
            
            var luaWriteInt = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(a[idx], idx));
            var luaWriteVector3 = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(b[idx], vector));
            var luaWriteTransform = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(c[idx], transform));
            var luaWriteBigObj = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(d[idx], bigObject));
            var luaWriteBiggerObj = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(e[idx], biggerObject));
/*
            var py_writeIntBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, null, pythonWriteInt),
                                           memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_writeVector3Bytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, null, pythonWriteVector3),
                                           memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_writeTransformBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, null, pythonWriteTransform),
                                           memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_writeBigObjBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, null, pythonWriteBigObj),
                                                 memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_writeBiggerObjBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, null, pythonWriteBiggerObj),
                                              memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            */
            var lua_writeIntBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteInt),
                                           memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            Console.WriteLine("lua_writeIntBytes done");
            var lua_writeVector3Bytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteVector3),
                                               memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            Console.WriteLine("lua_writeVector3Bytes done");
            var lua_writeTransformBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteTransform),
                                                 memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            Console.WriteLine("lua_writeTransformBytes done");
            var lua_writeBigObjBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteBigObj),
                                              memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            Console.WriteLine("lua_writeBigObjBytes done");
            var lua_writeBiggerObjBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteBiggerObj),
                                                 memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            
            //Console.WriteLine($"py_writeIntBytes =>\n\t{py_writeIntBytes} total bytes for {actions} writes,\n\t{py_writeIntBytes / (double) actions} bytes/write");
            //Console.WriteLine($"py_writeVector3Bytes =>\n\t{py_writeVector3Bytes} total bytes for {actions} writes,\n\t{py_writeVector3Bytes / (double) actions} bytes/write");
            //Console.WriteLine($"py_writeTransformBytes =>\n\t{py_writeTransformBytes} total bytes for {actions} writes,\n\t{py_writeTransformBytes / (double) actions} bytes/write");
            //Console.WriteLine($"py_writeBigObjBytes =>\n\t{py_writeBigObjBytes} total bytes for {actions} writes,\n\t{py_writeBigObjBytes / (double) actions} bytes/write");
            //Console.WriteLine($"py_writeBiggerObjBytes =>\n\t{py_writeBiggerObjBytes} total bytes for {actions} writes,\n\t{py_writeBiggerObjBytes / (double) actions} bytes/write");
            
            Console.WriteLine($"lua_writeIntBytes =>\n\t{lua_writeIntBytes} total bytes for {actions} writes,\n\t{lua_writeIntBytes / (double) actions} bytes/write");
            Console.WriteLine($"lua_writeVector3Bytes =>\n\t{lua_writeVector3Bytes} total bytes for {actions} writes,\n\t{lua_writeVector3Bytes / (double) actions} bytes/write");
            Console.WriteLine($"lua_writeTransformBytes =>\n\t{lua_writeTransformBytes} total bytes for {actions} writes,\n\t{lua_writeTransformBytes / (double) actions} bytes/write");
            Console.WriteLine($"lua_writeBigObjBytes =>\n\t{lua_writeBigObjBytes} total bytes for {actions} writes,\n\t{lua_writeBigObjBytes / (double) actions} bytes/write");
            Console.WriteLine($"lua_writeBiggerObjBytes =>\n\t{lua_writeBiggerObjBytes} total bytes for {actions} writes,\n\t{lua_writeBiggerObjBytes / (double) actions} bytes/write");
        }

        internal static void MeasureBasic() {
            var memoryCleanup = new Action(() => {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
            var pythonInitialize = new Action<PyScope>(scope => {
                scope.Exec("a = None");
            });
            var pythonWrite = new Action<PyScope, int>((scope, idx) => {
                scope.Set("a", idx);
            });
            
            var luaWrite = new Action<NLua.Lua, int>((scope, idx) => {
                scope.SetObjectToPath("a", idx);
            });

            var actions = 65536;
            var py_writeNormalMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup, pythonInitialize, pythonWrite), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_writeCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup, pythonInitialize, pythonWrite), memoryCleanup, memoryCleanup,
                                                GC.GetAllocatedBytesForCurrentThread)[0];
            var py_readNormalMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var py_readCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, memoryCleanup,
                                               GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_writeNormalMemory = Measure(1, (func, cleanup) => LuaProgram.WriteMem(actions, func, cleanup, luaWrite), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_writeCleanupMemory = Measure(1, (func, cleanup) => LuaProgram.WriteMem(actions, func, cleanup, luaWrite), memoryCleanup, memoryCleanup,
                                                 GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_readNormalMemory = Measure(1, (func, cleanup) => LuaProgram.ReadMem(actions, func, cleanup), memoryCleanup, null, GC.GetAllocatedBytesForCurrentThread)[0];
            var lua_readCleanupMemory = Measure(1, (func, cleanup) => LuaProgram.ReadMem(actions, func, cleanup), memoryCleanup, memoryCleanup,
                                                GC.GetAllocatedBytesForCurrentThread)[0];

            var sampleCount = 512;
            var py_writeNormalTimeSamples = Measure(sampleCount, (func, action) => PythonProgram.WriteTime(actions, func, action, pythonInitialize, pythonWrite), memoryCleanup, null, () => DateTime.Now);
            var py_readNormalTimeSamples = Measure(sampleCount, (func, action) => PythonProgram.ReadTime(actions, func, action), memoryCleanup, null, () => DateTime.Now);
            var lua_writeNormalTimeSamples = Measure(sampleCount, (func, action) => LuaProgram.WriteTime(actions, func, action, luaWrite), memoryCleanup, null, () => DateTime.Now);
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