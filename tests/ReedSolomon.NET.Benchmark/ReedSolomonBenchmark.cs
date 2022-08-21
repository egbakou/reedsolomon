// Benchmark of Reed-Solomon encoding.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

using System.Text;
using ReedSolomon.NET.Loops;

namespace ReedSolomon.NET.Benchmark;

internal sealed class ReedSolomonBenchmark
{
    private const int DataCount = 17;
    private const int ParityCount = 3;
    private const int TotalCount = DataCount + ParityCount;
    private const int BufferSize = 200 * 1000;
    private const int ProcessorCacheSize = 10 * 1024 * 1024;
    private const int TwiceProcessorCacheSize = 2 * ProcessorCacheSize;
    private const int NumberOfBufferSets = TwiceProcessorCacheSize / DataCount / BufferSize + 1;
    private const long MeasurementDuration = 2 * 1000;

    private static readonly Random _random = new();

    private int _nextBuffer;

    internal void Run()
    {
        Console.WriteLine("preparing...");
        var bufferSets = new BufferSet[NumberOfBufferSets];

        for (var iBufferSet = 0; iBufferSet < NumberOfBufferSets; iBufferSet++)
            bufferSets[iBufferSet] = new BufferSet();

        var tempBuffer = new byte[BufferSize];

        var summaryLines = new List<string>();
        var csv = new StringBuilder();
        csv.Append("Outer,Middle,Inner,Multiply,Encode,Check\n");

        foreach (var codingLoop in CodingLoopHelpers.AllCodingLoops)
        {
            var encodeAverage = new Measurement();

            var testNameEcAvg = codingLoop.GetType().Name + " encodeParity";
            Console.WriteLine("\nTEST: " + testNameEcAvg);
            var codecEcAvg = new ReedSolomon(DataCount, ParityCount, codingLoop);
            Console.WriteLine("    warm up...");
            DoOneEncodeMeasurement(codecEcAvg, bufferSets);
            DoOneEncodeMeasurement(codecEcAvg, bufferSets);
            Console.WriteLine("    testing...");

            for (var iMeasurement = 0; iMeasurement < 10; iMeasurement++)
                encodeAverage.Add(DoOneEncodeMeasurement(codecEcAvg, bufferSets));

            Console.WriteLine("\nAVERAGE: {0}", encodeAverage);
            summaryLines.Add($"    {testNameEcAvg,-45} {encodeAverage}");

            // The encoding test should have filled all of the buffers with
            // correct parity, so we can benchmark parity checking.
            var checkAverage = new Measurement();

            var testNameCheckAvg = codingLoop.GetType().Name + " isParityCorrect";
            Console.WriteLine("\nTEST: " + testNameCheckAvg);
            var codecCheckAvg = new ReedSolomon(DataCount, ParityCount, codingLoop);
            Console.WriteLine("    warm up...");
            DoOneEncodeMeasurement(codecCheckAvg, bufferSets);
            DoOneEncodeMeasurement(codecCheckAvg, bufferSets);
            Console.WriteLine("    testing...");

            for (var iMeasurement = 0; iMeasurement < 10; iMeasurement++)
                checkAverage.Add(DoOneCheckMeasurement(codecCheckAvg, bufferSets, tempBuffer));

            Console.WriteLine("\nAVERAGE: {0}", checkAverage);
            summaryLines.Add($"    {testNameCheckAvg,-45} {checkAverage}");

            csv.Append(CodingLoopNameToCsvPrefix(codingLoop.GetType().Name));
            csv.Append(encodeAverage.GetRate());
            csv.Append(',');
            csv.Append(checkAverage.GetRate());
            csv.Append('\n');
        }

        Console.WriteLine("\n");
        Console.WriteLine(csv.ToString());

        Console.WriteLine("\nSummary:\n");

        foreach (var line in summaryLines)
            Console.WriteLine(line);
    }

    private Measurement DoOneEncodeMeasurement(ReedSolomon codec, IReadOnlyList<BufferSet> bufferSets)
    {
        long passesCompleted = 0;
        long bytesEncoded = 0;
        long encodingTime = 0;
        while (encodingTime < MeasurementDuration)
        {
            var bufferSet = bufferSets[_nextBuffer];
            _nextBuffer = (_nextBuffer + 1) % bufferSets.Count;
            var shards = bufferSet.Buffers;
            var startTime = DateTime.UtcNow;
            codec.EncodeParity(shards, 0, BufferSize);
            var endTime = DateTime.UtcNow;

            encodingTime += (long)(endTime - startTime).TotalMilliseconds;
            bytesEncoded += BufferSize * DataCount;
            passesCompleted += 1;
        }

        var seconds = encodingTime / 1000.0;
        var megabytes = bytesEncoded / 1000000.0;
        var result = new Measurement(megabytes, seconds);
        Console.WriteLine("        {0} passes, {1}", passesCompleted, result);

        return result;
    }

    private Measurement DoOneCheckMeasurement(ReedSolomon codec, IReadOnlyList<BufferSet> bufferSets, byte[] tempBuffer)
    {
        long passesCompleted = 0;
        long bytesChecked = 0;
        long checkingTime = 0;

        while (checkingTime < MeasurementDuration)
        {
            var bufferSet = bufferSets[_nextBuffer];
            _nextBuffer = (_nextBuffer + 1) % bufferSets.Count;
            var shards = bufferSet.Buffers;
            var startTime = DateTime.UtcNow;

            if (!codec.IsParityCorrect(shards, 0, BufferSize, tempBuffer))
                throw new Exception("parity not correct");

            var endTime = DateTime.UtcNow;
            checkingTime += (long)(endTime - startTime).TotalMilliseconds;
            bytesChecked += BufferSize * DataCount;
            passesCompleted += 1;
        }

        var seconds = checkingTime / 1000.0;
        var megabytes = bytesChecked / 1000000.0;
        var result = new Measurement(megabytes, seconds);
        Console.WriteLine("        {0} passes, {1}", passesCompleted, result);

        return result;
    }

    /// <summary>
    /// Converts a name like "OutputByteInputTableCodingLoop" to "output,byte,input,table,".
    /// </summary>
    private static string CodingLoopNameToCsvPrefix(string className)
    {
        var names = SplitCamelCase(className);
        return names[0] + "," + names[1] + "," + names[2] + "," + names[3] + ",";
    }

    /// <summary>
    /// Converts a name like "OutputByteInputTableCodingLoop"
    /// to a List of words: { "output", "byte", "input", "table", "coding", "loop" }
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    private static List<string> SplitCamelCase(string className)
    {
        var remaining = className;
        var result = new List<string>();

        while (!string.IsNullOrEmpty(remaining))
        {
            var found = false;

            for (var i = 1; i < remaining.Length; i++)
            {
                if (!char.IsUpper(remaining[i]))
                    continue;
                result.Add(remaining[..i]);
                remaining = remaining[i..];
                found = true;
                break;
            }

            if (found)
                continue;

            result.Add(remaining);
            remaining = "";
        }

        return result;
    }

    private sealed class BufferSet
    {
        public readonly byte[][] Buffers;

        public BufferSet()
        {
            Buffers = new byte[TotalCount][];

            for (var iBuffer = 0; iBuffer < TotalCount; iBuffer++)
            {
                Buffers[iBuffer] = new byte[BufferSize];
                var buffer = Buffers[iBuffer];

                for (var iByte = 0; iByte < BufferSize; iByte++)
                    buffer[iByte] = (byte)_random.Next(256);
            }

            var bigBuffer = new byte[TotalCount * BufferSize];

            for (var i = 0; i < TotalCount * BufferSize; i++)
                bigBuffer[i] = (byte)_random.Next(256);
        }
    }

    private sealed class Measurement
    {
        public double Megabytes;
        public double Seconds;

        public Measurement()
        {
            Megabytes = 0.0;
            Seconds = 0.0;
        }

        public Measurement(double megabytes, double seconds)
        {
            Megabytes = megabytes;
            Seconds = seconds;
        }

        public void Add(Measurement other)
        {
            Megabytes += other.Megabytes;
            Seconds += other.Seconds;
        }

        public double GetRate() => Megabytes / Seconds;

        public override string ToString() => $"{GetRate():F1} MB/s";
    }
}
