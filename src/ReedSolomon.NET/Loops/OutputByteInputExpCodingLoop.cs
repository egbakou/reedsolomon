// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class OutputByteInputExpCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount, byte[][] outputs,
            in int outputCount, in int offset, in int byteCount)
        {
            for (var iOutput = 0; iOutput < outputCount; iOutput++) {
                var outputShard = outputs[iOutput];
                var matrixRow = matrixRows[iOutput];
                for (var iByte = offset; iByte < offset + byteCount; iByte++) {
                    var value = 0;
                    for (var iInput = 0; iInput < inputCount; iInput++) {
                        var inputShard = inputs[iInput];
                        value ^= Galois.Multiply(matrixRow[iInput], inputShard[iByte]);
                    }
                    outputShard[iByte] = (byte) value;
                }
            }
        }
    }
}
