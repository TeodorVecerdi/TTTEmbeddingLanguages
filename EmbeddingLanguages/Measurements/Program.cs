using System;
using System.Collections.Generic;
using System.Linq;
using Embedded;
using Embedded.Data;
using Lua;
using Python;
using Python.Runtime;
using UnityCommons;
using UnityEngine;

namespace Measurements {
    class Program {
        static IEnumerable<int> Range(int start, int end) {
            for (var i = start; i < end; i++) {
                yield return i;
            }
        }

        private static readonly Func<List<(DateTime start, DateTime end)>, (TimeSpan min, TimeSpan max, TimeSpan avg)> processPerformanceSamples = samples => {
            TimeSpan min = TimeSpan.MaxValue, max = TimeSpan.MinValue, sum = TimeSpan.Zero;
            foreach (var (start, end) in samples) {
                var delta = end - start;
                if (delta < min) min = delta;
                if (delta > max) max = delta;
                sum += delta;
            }

            return (min, max, sum / samples.Count);
        };

        private static readonly Func<List<(long start, long end)>, (long min, long max, double avg)> processMemorySamples = samples => {
            long min = long.MaxValue, max = long.MinValue;
            var sum = 0ul;
            foreach (var (start, end) in samples) {
                var delta = end - start;
                if (delta < min) min = delta;
                if (delta > max) max = delta;
                sum += (ulong)delta;
            }

            return (min, max, sum / (double)samples.Count);
        };

        private static readonly Func<DateTime> timeMeasurement = () => DateTime.Now;
        private static readonly Func<long> memoryMeasurement = GC.GetAllocatedBytesForCurrentThread;
        private static readonly Action memoryCleanup = () => {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        };

        static void Main(string[] args) {
            PythonProgram.Initialize();
            // CompareComplexMemory();
            // CompareComplexPerformance();
            CompareFunctionCalls();
        }

        internal static void CompareFunctionCalls() {
            using (Py.GIL()) {
                using var pythonScope = Py.CreateScope();
                pythonScope.Exec(@"
def add(a, b):
    return a + b
");
                PythonFuncCalls(pythonScope);
            }

            using (var luaState = new NLua.Lua()) {
                luaState.DoString(@"
function add(a, b)
    return a + b
end
");
                LuaFuncCalls(luaState);
            }
        }

        internal static void LuaFuncCalls(NLua.Lua state) {
            var func = state.GetFunction("add");
            
            const int sampleCount = 64, actionCount = 1000000;
            var samples = Measure(sampleCount, (measure, _) => {
                var start = measure();
                for (var i = 0; i < actionCount; i++) {
                    var result = (long)func.Call(Rand.Int, Rand.Int)[0];
                }
                var end = measure();
                return (end.Item1 - start.Item1, end.Item2 - start.Item2);
            }, memoryCleanup, null, () => (timeMeasurement(), memoryMeasurement()));
            
            var sumTime = TimeSpan.Zero;
            var sumMemory = 0ul;
            for (var i = 0; i < sampleCount; i++) {
                sumTime += samples[i].Item1;
                sumMemory += (ulong) samples[i].Item2;
            }

            var avgMemory = sumMemory / (double) sampleCount;
            var avgTime = sumTime / sampleCount;
            Console.WriteLine($"Lua Function Calls:\n{avgMemory} bytes for {actionCount} calls\n{avgMemory / actionCount} bytes per call\n{avgTime.TotalMilliseconds} ms for {actionCount} calls\n{(avgTime / actionCount).TotalMilliseconds} ms per call");

        }

        internal static void PythonFuncCalls(PyScope scope) {
            var func = scope.Get("add");
            
            const int sampleCount = 64, actionCount = 1000000;
            var samples = Measure(sampleCount, (measure, _) => {
                var start = measure();
                for (var i = 0; i < actionCount; i++) {
                    var result = func.Invoke(Rand.Int.ToPython(), Rand.Int.ToPython()).As<long>();
                }
                var end = measure();
                return (end.Item1 - start.Item1, end.Item2 - start.Item2);
            }, memoryCleanup, null, () => ( timeMeasurement(), memoryMeasurement()));
            
            var sumTime = TimeSpan.Zero;
            var sumMemory = 0ul;
            for (var i = 0; i < sampleCount; i++) {
                sumTime += samples[i].Item1;
                sumMemory += (ulong) samples[i].Item2;
            }

            var avgMemory = sumMemory / (double) sampleCount;
            var avgTime = sumTime / sampleCount;
            Console.WriteLine($"Python Function Calls:\n{avgMemory} bytes for {actionCount} calls\n{avgMemory / actionCount} bytes per call\n{avgTime.TotalMilliseconds} ms for {actionCount} calls\n{(avgTime / actionCount).TotalMilliseconds} ms per call");
        }

        internal static void CompareComplexPerformance() {
            const int actions = 8192;
            const int sampleCount = 256;
            var vector = new Vector3(1.0f, 2.0f, 3.0f);
            var transform = new Transform(new Vector3(1.0f, 2.0f, 3.0f),
                                          new Vector3(11.0f, 22.0f, 33.0f),
                                          new Vector3(111.0f, 222.0f, 333.0f));
            var bigObject = new BigObject();
            var biggerObject = new BiggerObject();

            var keysA = Range(0, actions).Select(i => $"a{i}").ToList();
            var keysB = Range(0, actions).Select(i => $"b{i}").ToList();
            var keysC = Range(0, actions).Select(i => $"c{i}").ToList();
            var keysD = Range(0, actions).Select(i => $"d{i}").ToList();
            var keysE = Range(0, actions).Select(i => $"e{i}").ToList();

            var pythonWriteInt = new Action<PyScope, int>((scope, idx) => scope.Set(keysA[idx], idx));
            var pythonWriteVector3 = new Action<PyScope, int>((scope, idx) => scope.Set(keysB[idx], vector));
            var pythonWriteTransform = new Action<PyScope, int>((scope, idx) => scope.Set(keysC[idx], transform));
            var pythonWriteBigObj = new Action<PyScope, int>((scope, idx) => scope.Set(keysD[idx], bigObject));
            var pythonWriteBiggerObj = new Action<PyScope, int>((scope, idx) => scope.Set(keysE[idx], biggerObject));
            var luaWriteInt = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(keysA[idx], idx));
            var luaWriteVector3 = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(keysB[idx], vector));
            var luaWriteTransform = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(keysC[idx], transform));
            var luaWriteBigObj = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(keysD[idx], bigObject));
            var luaWriteBiggerObj = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(keysE[idx], biggerObject));

            var py_writeIntTime = Process(Measure(sampleCount, (measureFunction, cleanup) => PythonProgram.WriteTime(actions, measureFunction, cleanup, pythonWriteInt), memoryCleanup, null, timeMeasurement, "py_writeIntTime"));
            Console.WriteLine("py_writeIntTime done");
            var py_writeVector3Time = Process(Measure(sampleCount, (measureFunction, cleanup) => PythonProgram.WriteTime(actions, measureFunction, cleanup, pythonWriteVector3), memoryCleanup, null, timeMeasurement, "py_writeVector3Time"));
            Console.WriteLine("py_writeVector3Time done");
            var py_writeTransformTime = Process(Measure(sampleCount, (measureFunction, cleanup) => PythonProgram.WriteTime(actions, measureFunction, cleanup, pythonWriteTransform), memoryCleanup, null, timeMeasurement, "py_writeTransformTime"));
            Console.WriteLine("py_writeTransformTime done");
            var py_writeBigObjTime = Process(Measure(sampleCount, (measureFunction, cleanup) => PythonProgram.WriteTime(actions, measureFunction, cleanup, pythonWriteBigObj), memoryCleanup, null, timeMeasurement, "py_writeBigObjTime"));
            Console.WriteLine("py_writeBigObjTime done");
            var py_writeBiggerObjTime = Process(Measure(sampleCount, (measureFunction, cleanup) => PythonProgram.WriteTime(actions, measureFunction, cleanup, pythonWriteBiggerObj), memoryCleanup, null, timeMeasurement, "py_writeBiggerObjTime"));
            Console.WriteLine("py_writeBiggerObjTime done");

            var lua_writeIntTime = Process(Measure(sampleCount, (measureFunction, cleanup) => LuaProgram.WriteTime(actions, measureFunction, cleanup, luaWriteInt), memoryCleanup, null, timeMeasurement, "lua_writeIntTime"));
            Console.WriteLine("lua_writeIntTime done");
            var lua_writeVector3Time = Process(Measure(sampleCount, (measureFunction, cleanup) => LuaProgram.WriteTime(actions, measureFunction, cleanup, luaWriteVector3), memoryCleanup, null, timeMeasurement, "lua_writeVector3Time"));
            Console.WriteLine("lua_writeVector3Time done");
            var lua_writeTransformTime = Process(Measure(sampleCount, (measureFunction, cleanup) => LuaProgram.WriteTime(actions, measureFunction, cleanup, luaWriteTransform), memoryCleanup, null, timeMeasurement, "lua_writeTransformTime"));
            Console.WriteLine("lua_writeTransformTime done");
            var lua_writeBigObjTime = Process(Measure(sampleCount, (measureFunction, cleanup) => LuaProgram.WriteTime(actions, measureFunction, cleanup, luaWriteBigObj), memoryCleanup, null, timeMeasurement, "lua_writeBigObjTime"));
            Console.WriteLine("lua_writeBigObjTime done");
            var lua_writeBiggerObjTime = Process(Measure(sampleCount, (measureFunction, cleanup) => LuaProgram.WriteTime(actions, measureFunction, cleanup, luaWriteBiggerObj), memoryCleanup, null, timeMeasurement, "lua_writeBiggerObjTime"));
            Console.WriteLine("lua_writeBiggerObjTime done");

            Console.WriteLine(
                $"py_writeIntTime for {actions} writes => \n\tMin: {py_writeIntTime.min.TotalMilliseconds}ms\n\tMax: {py_writeIntTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_writeIntTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"py_writeVector3Time for {actions} writes => \n\tMin: {py_writeVector3Time.min.TotalMilliseconds}ms\n\tMax: {py_writeVector3Time.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_writeVector3Time.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"py_writeTransformTime for {actions} writes => \n\tMin: {py_writeTransformTime.min.TotalMilliseconds}ms\n\tMax: {py_writeTransformTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_writeTransformTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"py_writeBigObjTime for {actions} writes => \n\tMin: {py_writeBigObjTime.min.TotalMilliseconds}ms\n\tMax: {py_writeBigObjTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_writeBigObjTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"py_writeBiggerObjTime for {actions} writes => \n\tMin: {py_writeBiggerObjTime.min.TotalMilliseconds}ms\n\tMax: {py_writeBiggerObjTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_writeBiggerObjTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"lua_writeIntTime for {actions} writes => \n\tMin: {lua_writeIntTime.min.TotalMilliseconds}ms\n\tMax: {lua_writeIntTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_writeIntTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"lua_writeVector3Time for {actions} writes => \n\tMin: {lua_writeVector3Time.min.TotalMilliseconds}ms\n\tMax: {lua_writeVector3Time.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_writeVector3Time.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"lua_writeTransformTime for {actions} writes => \n\tMin: {lua_writeTransformTime.min.TotalMilliseconds}ms\n\tMax: {lua_writeTransformTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_writeTransformTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"lua_writeBigObjTime for {actions} writes => \n\tMin: {lua_writeBigObjTime.min.TotalMilliseconds}ms\n\tMax: {lua_writeBigObjTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_writeBigObjTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"lua_writeBiggerObjTime for {actions} writes => \n\tMin: {lua_writeBiggerObjTime.min.TotalMilliseconds}ms\n\tMax: {lua_writeBiggerObjTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_writeBiggerObjTime.avg.TotalMilliseconds}ms");
            Console.ReadKey();
        }

        internal static void CompareComplexMemory() {
            // const int actions = 1048576; // 1<<20
            const int actions = 16384;
            var vector = new Vector3(1.0f, 2.0f, 3.0f);
            var transform = new Transform(new Vector3(1.0f, 2.0f, 3.0f), new Vector3(11.0f, 22.0f, 33.0f), new Vector3(111.0f, 222.0f, 333.0f));
            var bigObject = new BigObject();
            var biggerObject = new BiggerObject();

            var a = Range(0, actions).Select(i => $"a{i}").ToList();
            var b = Range(0, actions).Select(i => $"b{i}").ToList();
            var c = Range(0, actions).Select(i => $"c{i}").ToList();
            var d = Range(0, actions).Select(i => $"d{i}").ToList();
            var e = Range(0, actions).Select(i => $"e{i}").ToList();

            var pythonWriteInt = new Action<PyScope, int>((scope, idx) => scope.Set(a[idx], idx));
            var pythonWriteVector3 = new Action<PyScope, int>((scope, idx) => scope.Set(b[idx], vector));
            var pythonWriteTransform = new Action<PyScope, int>((scope, idx) => scope.Set(c[idx], transform));
            var pythonWriteBigObj = new Action<PyScope, int>((scope, idx) => scope.Set(d[idx], bigObject));
            var pythonWriteBiggerObj = new Action<PyScope, int>((scope, idx) => scope.Set(e[idx], biggerObject));

            var luaWriteInt = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(a[idx], idx));
            var luaWriteVector3 = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(b[idx], vector));
            var luaWriteTransform = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(c[idx], transform));
            var luaWriteBigObj = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(d[idx], bigObject));
            var luaWriteBiggerObj = new Action<NLua.Lua, int>((scope, idx) => scope.SetObjectToPath(e[idx], biggerObject));

            var py_writeIntBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, pythonWriteInt), memoryCleanup, null, memoryMeasurement)[0];
            var py_writeVector3Bytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, pythonWriteVector3), memoryCleanup, null, memoryMeasurement)[0];
            var py_writeTransformBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, pythonWriteTransform), memoryCleanup, null, memoryMeasurement)[0];
            var py_writeBigObjBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, pythonWriteBigObj), memoryCleanup, null, memoryMeasurement)[0];
            var py_writeBiggerObjBytes = Measure(1, (measureFunction, cleanup) => PythonProgram.WriteMem(actions, measureFunction, cleanup, pythonWriteBiggerObj), memoryCleanup, null, memoryMeasurement)[0];

            var lua_writeIntBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteInt), memoryCleanup, null, memoryMeasurement)[0];
            var lua_writeVector3Bytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteVector3), memoryCleanup, null, memoryMeasurement)[0];
            var lua_writeTransformBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteTransform), memoryCleanup, null, memoryMeasurement)[0];
            var lua_writeBigObjBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteBigObj), memoryCleanup, null, memoryMeasurement)[0];
            var lua_writeBiggerObjBytes = Measure(1, (measureFunction, cleanup) => LuaProgram.WriteMem(actions, measureFunction, cleanup, luaWriteBiggerObj), memoryCleanup, null, memoryMeasurement)[0];

            Console.WriteLine($"py_writeIntBytes =>\n\t{py_writeIntBytes} total bytes for {actions} writes,\n\t{py_writeIntBytes / (double) actions} bytes/write");
            Console.WriteLine($"py_writeVector3Bytes =>\n\t{py_writeVector3Bytes} total bytes for {actions} writes,\n\t{py_writeVector3Bytes / (double) actions} bytes/write");
            Console.WriteLine(
                $"py_writeTransformBytes =>\n\t{py_writeTransformBytes} total bytes for {actions} writes,\n\t{py_writeTransformBytes / (double) actions} bytes/write");
            Console.WriteLine($"py_writeBigObjBytes =>\n\t{py_writeBigObjBytes} total bytes for {actions} writes,\n\t{py_writeBigObjBytes / (double) actions} bytes/write");
            Console.WriteLine(
                $"py_writeBiggerObjBytes =>\n\t{py_writeBiggerObjBytes} total bytes for {actions} writes,\n\t{py_writeBiggerObjBytes / (double) actions} bytes/write");

            Console.WriteLine($"lua_writeIntBytes =>\n\t{lua_writeIntBytes} total bytes for {actions} writes,\n\t{lua_writeIntBytes / (double) actions} bytes/write");
            Console.WriteLine($"lua_writeVector3Bytes =>\n\t{lua_writeVector3Bytes} total bytes for {actions} writes,\n\t{lua_writeVector3Bytes / (double) actions} bytes/write");
            Console.WriteLine(
                $"lua_writeTransformBytes =>\n\t{lua_writeTransformBytes} total bytes for {actions} writes,\n\t{lua_writeTransformBytes / (double) actions} bytes/write");
            Console.WriteLine($"lua_writeBigObjBytes =>\n\t{lua_writeBigObjBytes} total bytes for {actions} writes,\n\t{lua_writeBigObjBytes / (double) actions} bytes/write");
            Console.WriteLine(
                $"lua_writeBiggerObjBytes =>\n\t{lua_writeBiggerObjBytes} total bytes for {actions} writes,\n\t{lua_writeBiggerObjBytes / (double) actions} bytes/write");
        }

        internal static void MeasureBasic() {
            var pythonWrite = new Action<PyScope, int>((scope, idx) => { scope.Set("a", idx); });

            var luaWrite = new Action<NLua.Lua, int>((scope, idx) => { scope.SetObjectToPath("a", idx); });

            const int actions = 65536;
            const int sampleCount = 512;
            var py_writeNormalMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup, pythonWrite), memoryCleanup, null, memoryMeasurement)[0];
            var py_writeCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.WriteMem(actions, func, cleanup, pythonWrite), memoryCleanup, memoryCleanup, memoryMeasurement)[0];
            var py_readNormalMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, null, memoryMeasurement)[0];
            var py_readCleanupMemory = Measure(1, (func, cleanup) => PythonProgram.ReadMem(actions, func, cleanup), memoryCleanup, memoryCleanup, memoryMeasurement)[0];
            var lua_writeNormalMemory = Measure(1, (func, cleanup) => LuaProgram.WriteMem(actions, func, cleanup, luaWrite), memoryCleanup, null, memoryMeasurement)[0];
            var lua_writeCleanupMemory = Measure(1, (func, cleanup) => LuaProgram.WriteMem(actions, func, cleanup, luaWrite), memoryCleanup, memoryCleanup, memoryMeasurement)[0];
            var lua_readNormalMemory = Measure(1, (func, cleanup) => LuaProgram.ReadMem(actions, func, cleanup), memoryCleanup, null, memoryMeasurement)[0];
            var lua_readCleanupMemory = Measure(1, (func, cleanup) => LuaProgram.ReadMem(actions, func, cleanup), memoryCleanup, memoryCleanup, memoryMeasurement)[0];

            var py_writeNormalTimeSamples = Measure(sampleCount, (func, action) => PythonProgram.WriteTime(actions, func, action, pythonWrite), memoryCleanup, null, timeMeasurement);
            var py_readNormalTimeSamples = Measure(sampleCount, (func, action) => PythonProgram.ReadTime(actions, func, action), memoryCleanup, null, timeMeasurement);
            var lua_writeNormalTimeSamples = Measure(sampleCount, (func, action) => LuaProgram.WriteTime(actions, func, action, luaWrite), memoryCleanup, null, timeMeasurement);
            var lua_readNormalTimeSamples = Measure(sampleCount, (func, action) => LuaProgram.ReadTime(actions, func, action), memoryCleanup, null, timeMeasurement);

            var py_writeNormalTime = Process(py_writeNormalTimeSamples);
            var py_readNormalTime = Process(py_readNormalTimeSamples);
            var lua_writeNormalTime = Process(lua_writeNormalTimeSamples);
            var lua_readNormalTime = Process(lua_readNormalTimeSamples);

            Console.WriteLine($"py_writeNormalMemory =>\n\t{py_writeNormalMemory} total bytes for {actions} writes,\n\t{py_writeNormalMemory / (double) actions} bytes/write");
            Console.WriteLine($"py_writeCleanupMemory =>\n\t{py_writeCleanupMemory} total bytes for {actions} writes,\n\t{py_writeCleanupMemory / (double) actions} bytes/write");
            Console.WriteLine($"py_readNormalMemory =>\n\t{py_readNormalMemory} total bytes for {actions} reads,\n\t{py_readNormalMemory / (double) actions} bytes/read");
            Console.WriteLine($"py_readCleanupMemory =>\n\t{py_readCleanupMemory} total bytes for {actions} reads,\n\t{py_readCleanupMemory / (double) actions} bytes/read");
            Console.WriteLine($"lua_writeNormalMemory =>\n\t{lua_writeNormalMemory} total bytes for {actions} writes,\n\t{lua_writeNormalMemory / (double) actions} bytes/write");
            Console.WriteLine(
                $"lua_writeCleanupMemory =>\n\t{lua_writeCleanupMemory} total bytes for {actions} writes,\n\t{lua_writeCleanupMemory / (double) actions} bytes/write");
            Console.WriteLine($"lua_readNormalMemory =>\n\t{lua_readNormalMemory} total bytes for {actions} reads,\n\t{lua_readNormalMemory / (double) actions} bytes/read");
            Console.WriteLine($"lua_readCleanupMemory =>\n\t{lua_readCleanupMemory} total bytes for {actions} reads,\n\t{lua_readCleanupMemory / (double) actions} bytes/read");

            Console.WriteLine(
                $"py_writeNormalTime => \n\tMin: {py_writeNormalTime.min.TotalMilliseconds}ms\n\tMax: {py_writeNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_writeNormalTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"py_readNormalTime => \n\tMin: {py_readNormalTime.min.TotalMilliseconds}ms\n\tMax: {py_readNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {py_readNormalTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"lua_writeNormalTime => \n\tMin: {lua_writeNormalTime.min.TotalMilliseconds}ms\n\tMax: {lua_writeNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_writeNormalTime.avg.TotalMilliseconds}ms");
            Console.WriteLine(
                $"lua_readNormalTime => \n\tMin: {lua_readNormalTime.min.TotalMilliseconds}ms\n\tMax: {lua_readNormalTime.max.TotalMilliseconds}ms\n\tAvg (over {sampleCount} samples): {lua_readNormalTime.avg.TotalMilliseconds}ms");
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
                                                 Func<TM> measureFunction, string funcName = "") {
            var samples = new List<TR>();
            var start = DateTime.Now;
            for (var i = 0; i < sampleCount; i++) {
                cleanupFunctionSelf();
                samples.Add(function(measureFunction, cleanupFunction));
                var time = DateTime.Now - start;
                var percentCompleted = ((i + 1) / (double) sampleCount);
                var remaining = ((1 - percentCompleted) / percentCompleted) * time;
                Console.Title =
                    $"{funcName}: {100 * i / (double) sampleCount:F2}% complete - elapsed: {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}; remaining: {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            } 

            return samples;
        }

        private static (TMeasurement start, TMeasurement end) Measure<TMeasurement>(Func<TMeasurement> measureFunction, int count, Action action) {
            var start = measureFunction!();
            for (var i = 0; i < count; i++) {
                action!();
            }
            var end = measureFunction!();
            return (start, end);
        }

        private static List<(TMeasurement start, TMeasurement end)> Collect<TMeasurement>(int sampleCount, Func<TMeasurement> measureFunction, int count, Action action) {
            var samples = new List<(TMeasurement start, TMeasurement end)>();
            for (var i = 0; i < sampleCount; i++) {
                samples.Add(Measure(measureFunction!, count, action!));
            }

            return samples;
        }

        private static TProcessed Process<TProcessed, TMeasurement>(List<(TMeasurement start, TMeasurement end)> measurements, Func<List<(TMeasurement start, TMeasurement end)>, TProcessed> processFunction) {
            return processFunction!(measurements!);
        }
    }
}