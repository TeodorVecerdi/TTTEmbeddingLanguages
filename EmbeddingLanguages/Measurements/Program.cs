using System;
using System.Collections.Generic;
using Embedded;
using Embedded.Components;
using Lua;
using Python;
using Python.Runtime;

namespace Measurements {
    class Program {
        static void Main(string[] args) {
            PythonProgram.Initialize();
            var gameManager = new GameManager();
            using var gil = Py.GIL();
            using var scope = Py.Import("pyresources.main");
            // scope.Exec("import clr");
            // scope.Exec("clr.AddReference('Shared')");
            // scope.Set("test", new GameObject().ToPython());
//             scope.Exec(@"
// print(test.HasComponent.__class__)
// ");
            return;

            // int enemyCount = Rand.Range(64, 128), walkerCount = Rand.Range(64, 128), targetFollowCount = Rand.Range(64, 128);
            int enemyCount = 118, walkerCount = 121, targetFollowCount = 86;
            Console.WriteLine($"Starting test with {enemyCount} enemies, {walkerCount} walkers, {targetFollowCount} followers");

            /*var gameManager = new GameManager();
            var gameObjects = new List<GameObject>();
            for (var i = 0; i < enemyCount; i++) {
                var enemy = new GameObject();
                enemy.Tag = "Enemy";
                enemy.AddComponent(new RandomWalk(Rand.Range(1f, 4f), 100));
                enemy.AddComponent(new TargetFollow(null, Rand.Range(1f, 2f)));
                enemy.AddComponent(new SphereCollider(Rand.Range(2f, 5f)));
                enemy.AddComponent(new EnemyAI());
                gameObjects.Add(enemy);
            }
            
            for (var i = 0; i < walkerCount; i++) {
                var walker = new GameObject();
                walker.Tag = "Walker";
                walker.AddComponent(new RandomWalk(Rand.Range(3f, 5f), 100));
                walker.AddComponent(new SphereCollider(Rand.Range(1f, 2f)));
                gameObjects.Add(walker);
            }
            
            for (var i = 0; i < targetFollowCount; i++) {
                var follower = new GameObject();
                follower.AddComponent(new TargetFollow(Rand.ListItem(gameObjects), Rand.Range(1f, 2f)));
            }
            gameObjects.Clear();
            
            gameManager.StartGame();
            while (true) {
                gameManager.UpdateGame();
            }*/
            PythonProgram.Test1(enemyCount, walkerCount, targetFollowCount);
            // PythonProgram.Test0(gameObjects);
        }

        internal static void MeasureBasic() {
            var memoryCleanup = new Action(() => {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });

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