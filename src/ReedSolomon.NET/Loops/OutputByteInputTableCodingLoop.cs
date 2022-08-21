// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class OutputByteInputTableCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs, in int inputCount,
            byte[][] outputs, in int outputCount, in int offset,
            in int byteCount)
        {
            var table = Galois.MultiplicationTable;
            for (var iOutput = 0; iOutput < outputCount; iOutput++)
            {
                var outputShard = outputs[iOutput];
                var matrixRow = matrixRows[iOutput];
                for (var iByte = offset; iByte < offset + byteCount; iByte++)
                {
                    var value = 0;
                    for (var iInput = 0; iInput < inputCount; iInput++)
                    {
                        var inputShard = inputs[iInput];
                        var multTableRow = table[matrixRow[iInput] & 0xFF];
                        value ^= multTableRow[inputShard[iByte] & 0xFF];
                    }

                    outputShard[iByte] = (byte)value;
                }
            }
        }
    }
}
