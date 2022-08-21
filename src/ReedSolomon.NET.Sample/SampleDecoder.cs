// Sample Decoder
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Sample;

public static class SampleDecoder
{
    private const int DataShards = 4;
    private const int ParityShards = 2;
    private const int TotalShards = 6;
    private const int BytesInInt = 4;

    // Rename this method to Main to run the sample.
    public static void EntryPoint(string[] args)
    {
        const string filePath = "D:/docker-volumes/uploads/contract.pptx";

        // count the number of file that name end by .0 .1 .2 .3 .4 .5 .6 .7 .8 .9 in the directory /uploads
        // var shardsList = Directory.GetFiles(@"D:/docker-volumes/uploads/", "contract.pptx.*");
        // Exclude the file that name end by .pptx
        var shardsList = Directory.GetFiles(@"D:/docker-volumes/uploads/", "contract.pptx.*")
            .Where(file => !file.EndsWith(".pptx")).ToList();
        var shardsCount = shardsList.Count;
        var shardPresent = new bool[TotalShards];

        for(var i=0; i<TotalShards; i++)
        {
            if (!shardsList.Any(file => file.EndsWith("." + i))) continue;
            shardPresent[i] = true;
        }

        if (shardsCount < DataShards)
        {
            Console.WriteLine("Not enough shards to reconstruct the file. Expected {0} but got {1}.", DataShards, shardsCount);
            return;
        }

        Console.WriteLine("Found {0} shards.", shardsCount);
        for(var i = 0; i < shardsCount; i++)
        {
            Console.WriteLine("Shard {0} located at {1}", i, shardsList[i]);
        }


        var shards = new byte [TotalShards] [];
        var shardSize = 0;
        for(var i = 0; i < TotalShards; i++)
        {
            if (!shardPresent[i]) continue;
            using var shardFile = new FileStream(shardsList[i], FileMode.Open, FileAccess.Read, FileShare.Read);
            shardSize = (int)shardFile.Length;
            shards[i] = new byte[shardSize];
            shardFile.Read(shards[i], 0, shardSize);
            Console.WriteLine("Read {0} bytes from {1}", shardSize, shardsList[i]);
            shardFile.Close();
        }

        // Make empty buffers for the missing shards.
        for (var i = 0; i < TotalShards; i++)
        {
            if (shardPresent[i]) continue;
            shards[i] = new byte [shardSize];
        }

        // Use Reed-Solomon to fill in the missing shards
        var reedSolomon = ReedSolomon.Create(DataShards, ParityShards);
        reedSolomon.DecodeMissing(shards, shardPresent, 0, shardSize);

        // Combine the data shards into one buffer for convenience.
        // (This is not efficient, but it is convenient.)
        var data = new byte[shardSize * DataShards];
        for (var i = 0; i < DataShards; i++)
        {
            Buffer.BlockCopy(shards[i], 0, data, i * shardSize, shardSize);
        }

        // Extract the file length
        var fileLength = BitConverter.ToInt32(data, 0);
        Console.WriteLine("File length: {0}", fileLength);

        // Write the decoded file
        using var outputFile = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        outputFile.Write(data, BytesInInt, fileLength);
        outputFile.Close();
        Console.WriteLine("Wrote {0} bytes to {1}", fileLength, filePath);

    }
}
