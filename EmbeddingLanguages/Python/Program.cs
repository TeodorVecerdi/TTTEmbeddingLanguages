using System;
using System.Collections.Generic;
using Python.Runtime;

namespace Python {
    internal static class Program {
        internal static void Main() {
            // Set python dll (part of path variable)
            Runtime.Runtime.PythonDLL = "python39.dll";
            int sampleCount = 1, writes = 16384;
            var samplesNormal_Total = Measure(sampleCount, func => RunMeasuredNormal(writes, func), () => GC.GetTotalAllocatedBytes(true));
            var samplesGC_Total = Measure(sampleCount, func => RunMeasuredGC(writes, func), () => GC.GetTotalAllocatedBytes(true));
            var samplesNormal_TotalProc = Measure(sampleCount, func => RunMeasuredNormal(writes, func), GC.GetAllocatedBytesForCurrentThread);
            var samplesGC_TotalProc = Measure(sampleCount, func => RunMeasuredGC(writes, func), GC.GetAllocatedBytesForCurrentThread);
            
            long sumNT = 0, sumGCT = 0, sumNTP = 0, sumGCTP = 0;
            for (var i = 0; i < sampleCount; i++) {
                sumNT += samplesNormal_Total[i];
                sumGCT += samplesGC_Total[i];
                sumNTP += samplesNormal_TotalProc[i];
                sumGCTP += samplesGC_TotalProc[i];
            }
            double avgNT = sumNT / (double)sampleCount, avgGCT = sumGCT / (double)sampleCount, avgNTP = sumNTP / (double)sampleCount, avgGCTP = sumGCTP / (double)sampleCount;

            Console.WriteLine($"samplesNormal_Total average over {sampleCount} samples => {avgNT} bytes total, {avgNT/writes} bytes/write");
            Console.WriteLine($"samplesGC_Total average over {sampleCount} samples => {avgGCT} bytes total, {avgGCT/writes} bytes/write");
            Console.WriteLine($"samplesNormal_TotalProc average over {sampleCount} samples => {avgNTP} bytes total, {avgNTP/writes} bytes/write");
            Console.WriteLine($"samplesGC_TotalProc average over {sampleCount} samples => {avgGCTP} bytes total, {avgGCTP/writes} bytes/write");
        }

        internal static List<TM> Measure<TM>(int sampleCount, Func<Func<TM>, TM> function, Func<TM> measureFunction) {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var samples = new List<TM>();
            for (var i = 0; i < sampleCount; i++) {
                samples.Add(function(measureFunction));
            }

            return samples;
        }

        internal static long RunMeasuredNormal(int writes, Func<long> measureFunction) {
            long start, end;
            using (var state = Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    start = measureFunction();
                    for (int i = 0; i < writes; i++) {
                        scope.Exec($"a = {i}");
                    }

                    end = measureFunction();
                }
            }

            return end - start;
        }

        internal static long RunMeasuredGC(int writes, Func<long> measureFunction) {
            long start, end;
            using (var state = Py.GIL()) {
                using (var scope = Py.CreateScope()) {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    start = measureFunction();
                    for (int i = 0; i < writes; i++) {
                        scope.Exec($"a = {i}");
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    end = measureFunction();
                }
            }
            return end - start;
        }
    }
}