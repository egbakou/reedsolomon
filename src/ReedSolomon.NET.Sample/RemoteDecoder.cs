// Remote Decoder.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

using Downloader;

namespace ReedSolomon.NET.Sample;

public static class RemoteDecoder
{
    private const int DataShards = 4;
    private const int ParityShards = 2;
    private const int TotalShards = 6;
    private const int BytesInInt = 4;

   public static void EntryPoint(string[] args)
    {
        const string filePath = "path_where_original_file_will_be_reconstructed/1GB.bin";
        var remoteFilePaths = new[]
        {
            "https://download1490.mediafire.com/hxz9tskcvfvg/xrxfst8lr2xypx5/1GB.bin.0",
            "https://download1350.mediafire.com/fi3q567mo9hg/rmo1lpsiumh8ftr/1GB.bin.1",
            "https://download1350.mediafire.com/55xuv5bgpcdg/cykxpo0h0zeqk7p/1GB.bin.2",
            "https://download1350.mediafire.com/lykstfg6gswg/849o4a1rer61o5u/1GB.bin.3"
        };

        var downloadOpt = new DownloadConfiguration()
        {
            ChunkCount = 8, // file parts to download, default value is 1
            OnTheFlyDownload = true, // caching in-memory or not? default values is true
            ParallelDownload = true // download parts of file as parallel or not. Default value is false
        };

        var startTime = DateTime.Now;
        Console.WriteLine("Start time: " + DateTime.Now);
        Parallel.ForEach(remoteFilePaths, (remoteFilePath) =>
        {
            var downloader = new DownloadService(downloadOpt);
            var fileName = remoteFilePath[(remoteFilePath.LastIndexOf('/') + 1)..];
            downloader.DownloadFileTaskAsync(remoteFilePath, $"location_to_store_chunks_file/{fileName}")
                .GetAwaiter().GetResult();
        });
        var endTime = DateTime.Now;
        var duration = (endTime - startTime).TotalSeconds;
        Console.WriteLine("Download duration: {0}", duration);

        // count the number of file that name end by .0 .1 .2 .3
        // var shardsList = Directory.GetFiles(@"location_to_store_chunks_file/", "1GB.bin.*");
        // Exclude the file that name end by .bin
        var shardsList = Directory.GetFiles(@"location_to_store_chunks_file/", "1GB.bin.*")
            .Where(file => !file.EndsWith(".bin")).ToList();
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
