// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class ByteOutputInputTableCodingLoop : CodingLoopBase
    {
        /// <inheritdoc />
        public override void CodeSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount, byte[][] outputs,
            in int outputCount, in int offset, in int byteCount)
        {
            var table = Galois.MultiplicationTable;
            for (var iByte = offset; iByte < offset + byteCount; iByte++) {
                for (var iOutput = 0; iOutput < outputCount; iOutput++) {
                    var matrixRow = matrixRows[iOutput];
                    var value = 0;
                    for (var iInput = 0; iInput < inputCount; iInput++) {
                        value ^= table[matrixRow[iInput] & 0xFF][inputs[iInput][iByte] & 0xFF];
                    }
                    outputs[iOutput][iByte] = (byte) value;
                }
            }
        }
    }
}
