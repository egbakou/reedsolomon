// ReedSolomon Tests
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

using ReedSolomon.NET.Loops;
using Random = System.Random;

namespace ReedSolomon.NET.Tests;

public class ReedSolomonTests
{
    [Fact]
    public void OneEncode_Parity_Should_Be_Correct()
    {
        foreach (var codingLoop in CodingLoopHelpers.AllCodingLoops)
        {
            var codec = new ReedSolomon(5, 5, codingLoop);
            var shards = new byte[10][];
            shards[0] = [0, 1];
            shards[1] = [4, 5];
            shards[2] = [2, 3];
            shards[3] = [6, 7];
            shards[4] = [8, 9];
            shards[5] = new byte[2];
            shards[6] = new byte[2];
            shards[7] = new byte[2];
            shards[8] = new byte[2];
            shards[9] = new byte[2];

            codec.EncodeParity(shards, 0, 2);

            shards[5].ShouldBe([12, 13]);
            shards[6].ShouldBe([10, 11]);
            shards[7].ShouldBe([14, 15]);
            shards[8].ShouldBe([90, 91]);
            shards[9].ShouldBe([94, 95]);

            codec.IsParityCorrect(shards, 0, 2).ShouldBeTrue();
            shards[8][0] += 1;
            codec.IsParityCorrect(shards, 0, 2).ShouldBeFalse();
        }
    }

    [Fact]
    public void SimpleEncodeDecode()
    {
        var dataShards = new[]
        {
            new byte[] {0, 1}, new byte[] {1, 2}, new byte[] {1, 3}, new byte[] {2, 4}, new byte[] {3, 5}
        };
        RunEncodeDecode(5, 5, dataShards);
    }

    [Fact]
    public void BigEncodeDecode()
    {
        var random = new Random(0);
        const int dataCount = 64;
        const int parityCount = 64;
        const int shardSize = 200;
        var dataShards = new byte [dataCount][];
        for (var i = 0; i < dataCount; i++)
        {
            dataShards[i] = new byte[shardSize];
        }

        foreach (var shard in dataShards)
        {
            for (var i = 0; i < shard.Length; i++)
            {
                shard[i] = (byte)random.Next(256);
            }
        }

        RunEncodeDecode(dataCount, parityCount, dataShards);
    }

    /**
     * Encodes a set of data shards and then tries decoding
     * using all possible subsets of the encoded shards.
     *
     * Uses 5+5 coding, so there must be 5 input data shards.
     */
    private static void RunEncodeDecode(int dataCount, int parityCount, byte[][] dataShards)
    {
        var totalCount = dataCount + parityCount;

        // Make the list of data and parity shards.
        dataCount.ShouldBe(dataShards.Length);
        var dataLength = dataShards[0].Length;
        var allShards = new byte [totalCount][];
        for(var i = 0; i < dataCount; i++)
        {
            allShards[i] = new byte [dataLength];
        }
        for (var i = 0; i < dataCount; i++)
        {
            Array.Copy(dataShards[i], 0, allShards[i], 0, dataLength);
        }

        for (var i = dataCount; i < totalCount; i++)
        {
            allShards[i] = new byte [dataLength];
        }

        // Encode.
        var codec = ReedSolomon.Create(dataCount, parityCount);
        codec.EncodeParity(allShards, 0, dataLength);

        // Make a copy to decode with.
        var testShards = new byte [totalCount][];
        var shardPresent = new bool [totalCount];

        for (var i = 0; i < totalCount; i++)
        {
            testShards[i] = new byte [dataLength];
            Array.Copy(allShards[i], 0, testShards[i], 0, dataLength);
            shardPresent[i] = true;
        }

        // Decode with 0, 1, ..., 5 shards missing.
        for (var numberMissing = 0; numberMissing < parityCount + 1; numberMissing++)
        {
            TryAllSubsetsMissing(codec, allShards, testShards, shardPresent, numberMissing);
        }
    }

    private static void TryAllSubsetsMissing(ReedSolomon codec, byte[][] allShards, byte[][] testShards,
        bool[] shardPresent, int numberMissing)
    {
        var shardLength = allShards[0].Length;
        var subsets = AllSubsets(numberMissing, 0, 10);
        foreach (var subset in subsets)
        {
            // Get rid of the shards specified by this subset.
            foreach (var missingShard in subset)
            {
                ClearBytes(testShards[missingShard]);
                shardPresent[missingShard] = false;
            }

            // Reconstruct the missing shards
            codec.DecodeMissing(testShards, shardPresent, 0, shardLength);

            // Check the results.  After checking, the contents of testShards
            // is ready for the next test, the next time through the loop.
            CheckShards(allShards, testShards);

            // Put the "present" flags back
            for (var i = 0; i < codec.GetTotalShardCount(); i++)
            {
                shardPresent[i] = true;
            }
        }
    }

    [Fact]
    public void CodingLoops_Should_Produce_The_Same_Answers()
    {
        const int dataCount = 5;
        const int parityCount = 5;
        const int shardSize = 2000;

        var random = new Random(0);

        // Make a set of input data shards
        var dataShards = new byte [dataCount][];
        for (var i = 0; i < dataCount; i++)
        {
            dataShards[i] = new byte [shardSize];
        }

        foreach (var shard in dataShards)
        {
            for (var iByte = 0; iByte < shard.Length; iByte++)
            {
                shard[iByte] = (byte)random.Next(256);
            }
        }

        // Make a reference set of parity shards using an arbitrary coding loop.
        var expectedParityShards = ComputeParityShards(dataShards, ReedSolomon.Create(dataCount, parityCount));

        // Check that all coding loops produce the same set of parity shards.
        foreach (var codingLoop in CodingLoopHelpers.AllCodingLoops)
        {
            var codec = new ReedSolomon(dataCount, parityCount, codingLoop);
            var actualParityShards = ComputeParityShards(dataShards, codec);
            for (var i = 0; i < parityCount; i++)
            {
                expectedParityShards[i].ShouldBe(actualParityShards[i]);
            }
        }
    }

    /**
     * Given an array of data shards, computes parity and returns an array
     * of the resulting parity shards.
     */
    private static byte[][] ComputeParityShards(byte[][] dataShards, ReedSolomon codec)
    {
        var shardSize = dataShards[0].Length;
        var totalShardCount = codec.GetTotalShardCount();
        var dataShardCount = codec.GetDataShardCount();
        var parityShardCount = codec.GetParityShardCount();

        var parityShards = new byte [parityShardCount][];
        for (var i = 0; i < parityShardCount; i++)
        {
            parityShards[i] = new byte [shardSize];
        }

        var allShards = new byte [totalShardCount][];
        for (var iShard = 0; iShard < totalShardCount; iShard++)
        {
            if (iShard < dataShardCount)
            {
                allShards[iShard] = dataShards[iShard];
            }
            else
            {
                allShards[iShard] = parityShards[iShard - dataShardCount];
            }
        }

        codec.EncodeParity(allShards, 0, shardSize);

        var tempBuffer = new byte [shardSize];
        allShards[parityShardCount - 1][0] += 1;
        codec.IsParityCorrect(allShards, 0, shardSize).ShouldBeFalse();
        codec.IsParityCorrect(allShards, 0, shardSize, tempBuffer).ShouldBeFalse();
        allShards[parityShardCount - 1][0] -= 1;
        codec.IsParityCorrect(allShards, 0, shardSize).ShouldBeTrue();
        codec.IsParityCorrect(allShards, 0, shardSize, tempBuffer).ShouldBeTrue();

        return parityShards;
    }

    private static void ClearBytes(byte[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }
    }

    private static void CheckShards(byte[][] expectedShards, byte[][] actualShards)
    {
        expectedShards.Length.ShouldBe(actualShards.Length);
        for (var i = 0; i < expectedShards.Length; i++)
        {
            expectedShards[i].ShouldBe(actualShards[i]);
        }
    }

    private static List<int[]> AllSubsets(int n, int min, int max)
    {
        var result = new List<int[]>();
        if (n == 0)
        {
            result.Add([]);
        }
        else
        {
            for (var i = min; i < max - n; i++)
            {
                int[] prefix = [i];
                result.AddRange(AllSubsets(n - 1, i + 1, max)
                    .Select(suffix => AppendIntArrays(prefix, suffix)));
            }
        }

        return result;
    }

    private static int[] AppendIntArrays(int[] a, int[] b)
    {
        var result = new int[a.Length + b.Length];
        Array.Copy(a, 0, result, 0, a.Length);
        Array.Copy(b, 0, result, a.Length, b.Length);
        return result;
    }
}
