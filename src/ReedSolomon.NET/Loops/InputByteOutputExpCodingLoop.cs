// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class InputByteOutputExpCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount, byte[][] outputs,
            in int outputCount, in int offset, in int byteCount)
        {
            const int iInputStart = 0;
            var inputShardStart = inputs[iInputStart];
            for (var iByte = offset; iByte < offset + byteCount; iByte++)
            {
                var inputByte = inputShardStart[iByte];
                for (var iOutput = 0; iOutput < outputCount; iOutput++)
                {
                    var outputShardStart = outputs[iOutput];
                    var matrixRowStart = matrixRows[iOutput];
                    outputShardStart[iByte] = Galois.Multiply(matrixRowStart[iInputStart], inputByte);
                }
            }

            for (var iInput = 1; iInput < inputCount; iInput++) {
                var inputShard = inputs[iInput];
                for (var iByte = offset; iByte < offset + byteCount; iByte++) {
                    var inputByte = inputShard[iByte];
                    for (var iOutput = 0; iOutput < outputCount; iOutput++) {
                        var outputShard = outputs[iOutput];
                        var matrixRow = matrixRows[iOutput];
                        outputShard[iByte] ^= Galois.Multiply(matrixRow[iInput], inputByte);
                    }
                }
            }
        }
    }
}
