// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

using System;

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class ByteInputOutputExpCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount,
            byte[][] outputs,
            in int outputCount, in int offset, in int byteCount)
        {
            for (var iByte = offset; iByte < offset + byteCount; iByte++)
            {
                const int iInputStart = 0;
                var inputShardStart = inputs[iInputStart];
                var inputByteStart = inputShardStart[iByte];
                for (var iOutput = 0; iOutput < outputCount; iOutput++)
                {
                    var outputShard = outputs[iOutput];
                    var matrixRow = matrixRows[iOutput];
                    outputShard[iByte] = Galois.Multiply(matrixRow[iInputStart], inputByteStart);
                }

                for (var iInput = 1; iInput < inputCount; iInput++)
                {
                    var inputShard = inputs[iInput];
                    var inputByte = inputShard[iByte];
                    for (var iOutput = 0; iOutput < outputCount; iOutput++)
                    {
                        var outputShard = outputs[iOutput];
                        var matrixRow = matrixRows[iOutput];
                        outputShard[iByte] ^= Galois.Multiply(matrixRow[iInput], inputByte);
                    }
                }
            }
        }
    }
}
