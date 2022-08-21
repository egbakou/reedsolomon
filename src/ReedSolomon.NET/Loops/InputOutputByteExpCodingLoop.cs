// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class InputOutputByteExpCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount, byte[][] outputs,
            in int outputCount, in int offset, in int byteCount)
        {
            const int iInputStart = 0;
            var inputShardStart = inputs[iInputStart];
            for (var iOutput = 0; iOutput < outputCount; iOutput++)
            {
                var outputShard = outputs[iOutput];
                var matrixRow = matrixRows[iOutput];
                var matrixByte = matrixRow[iInputStart];
                for (var iByte = offset; iByte < offset + byteCount; iByte++)
                {
                    outputShard[iByte] = Galois.Multiply(matrixByte, inputShardStart[iByte]);
                }
            }

            for (var iInput = 1; iInput < inputCount; iInput++) {
                var inputShard = inputs[iInput];
                for (var iOutput = 0; iOutput < outputCount; iOutput++) {
                    var outputShard = outputs[iOutput];
                    var matrixRow = matrixRows[iOutput];
                    var matrixByte = matrixRow[iInput];
                    for (var iByte = offset; iByte < offset + byteCount; iByte++) {
                        outputShard[iByte] ^= Galois.Multiply(matrixByte, inputShard[iByte]);
                    }
                }
            }
        }
    }
}
