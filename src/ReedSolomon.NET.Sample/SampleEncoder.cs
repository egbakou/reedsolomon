// Sample Encoder
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Sample;

public static class SampleEncoder
{
    private const int DataShards = 4;
    private const int ParityShards = 2;
    private const int TotalShards = 6;
    private const int BytesInInt = 4;

    public static void Main(string[] args)
    {
        const string filePath = "D:/docker-volumes/uploads/data.bin";
        Console.WriteLine("Byte array max");
        // get file size
        var fileSize = new FileInfo(filePath).Length;
        Console.WriteLine("File size: {0}", fileSize);

        // Figure out how big each shard will be.  The total size stored
        // will be the file size (8 bytes) plus the file.
        var storedSize = fileSize + BytesInInt;
        Console.WriteLine("Stored size: {0}", storedSize);
        var shardSize = (storedSize + DataShards - 1) / DataShards;
        Console.WriteLine("Shard size: {0}", shardSize);

        // Create a buffer holding the file size, followed by
        // the contents of the file.
        // The maximum index in any single dimension is 2,147,483,591 (0x7FFFFFC7) for byte arrays ~=1GB
        var buffer = new byte[shardSize * TotalShards];
        using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var fileSizeBytes = BitConverter.GetBytes(fileSize);
        Array.Copy(fileSizeBytes, 0, buffer, 0, BytesInInt);
        var read = file.Read(buffer, BytesInInt, (int)fileSize);
        Console.WriteLine("Read {0} bytes from file.", read);
        file.Close();

        // Make the buffers to hold the shards.
        var shards = new byte[TotalShards][];
        // Fill in the data shards
        for (var i = 0; i < DataShards; i++)
        {
            shards[i] = new byte[shardSize];
            Array.Copy(buffer, i * shardSize, shards[i], 0, shardSize);
        }
        // Fill the parity shards
        for (var i = DataShards; i < TotalShards; i++)
        {
            shards[i] = new byte[shardSize];
        }


        // Use Reed-Solomon to calculate the parity.
        Console.WriteLine("Start time: " + DateTime.Now);
        var reedSolomon = ReedSolomon.Create(DataShards, ParityShards);
        reedSolomon.EncodeParity(shards, 0, (int)shardSize);
        Console.WriteLine("End time: " + DateTime.Now);


        // // Write out the resulting files.
        for (var i = 0; i < TotalShards; i++)
        {
            var outputFile = $"{filePath}.{i}";
            // get file name from path
            var fileName = Path.GetFileName(outputFile);
            using var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.Write(shards[i], 0, (int)shardSize);
            outputStream.Close();
            Console.WriteLine("Wrote {0}", fileName);
        }
    }
}
