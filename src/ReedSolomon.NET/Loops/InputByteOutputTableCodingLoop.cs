// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class InputByteOutputTableCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount, byte[][] outputs,
            in int outputCount, in int offset, in int byteCount)
        {
            var table = Galois.MultiplicationTable;

            const int iInputStart = 0;
            var inputShardStart = inputs[iInputStart];
            for (var iByte = offset; iByte < offset + byteCount; iByte++)
            {
                var inputByte = inputShardStart[iByte];
                var multTableRow = table[inputByte & 0xFF];
                for (var iOutput = 0; iOutput < outputCount; iOutput++)
                {
                    var outputShard = outputs[iOutput];
                    var matrixRow = matrixRows[iOutput];
                    outputShard[iByte] = multTableRow[matrixRow[iInputStart] & 0xFF];
                }
            }

            for (var iInput = 1; iInput < inputCount; iInput++) {
                var inputShard = inputs[iInput];
                for (var iByte = offset; iByte < offset + byteCount; iByte++) {
                    var inputByte = inputShard[iByte];
                    var multTableRow = table[inputByte & 0xFF];
                    for (var iOutput = 0; iOutput < outputCount; iOutput++) {
                        var outputShard = outputs[iOutput];
                        var matrixRow = matrixRows[iOutput];
                        outputShard[iByte] ^= multTableRow[matrixRow[iInput] & 0xFF];
                    }
                }
            }
        }
    }
}
