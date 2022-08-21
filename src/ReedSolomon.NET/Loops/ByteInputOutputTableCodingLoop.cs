// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class ByteInputOutputTableCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount, byte[][] outputs,
            in int outputCount, in int offset, in int byteCount)
        {
            var table = Galois.MultiplicationTable;

            for (var iByte = offset; iByte < offset + byteCount; iByte++) {
                const int iInputStart = 0;
                var inputShardStart = inputs[iInputStart];
                var inputByteStart = inputShardStart[iByte];
                for (var iOutput = 0; iOutput < outputCount; iOutput++)
                {
                    var outputShardStart = outputs[iOutput];
                    var matrixRowStart = matrixRows[iOutput];
                    var multTableRowStart = table[matrixRowStart[iInputStart] & 0xFF];
                    outputShardStart[iByte] = multTableRowStart[inputByteStart & 0xFF];
                }

                for (var iInput = 1; iInput < inputCount; iInput++) {
                    var inputShard = inputs[iInput];
                    var inputByte = inputShard[iByte];
                    for (var iOutput = 0; iOutput < outputCount; iOutput++) {
                        var outputShard = outputs[iOutput];
                        var matrixRow = matrixRows[iOutput];
                        var multTableRow = table[matrixRow[iInput] & 0xFF];
                        outputShard[iByte] ^= multTableRow[inputByte & 0xFF];
                    }
                }
            }
        }
    }
}
