// One specific ordering/nesting of the coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// One specific ordering/nesting of the coding loops.
    /// </summary>
    public class OutputInputByteTableCodingLoop : CodingLoopBase
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

                const int iInputStart = 0;
                var inputShardStart = inputs[iInputStart];
                var multTableRowStart = table[matrixRow[iInputStart] & 0xFF];
                for (var iByte = offset; iByte < offset + byteCount; iByte++)
                {
                    outputShard[iByte] = multTableRowStart[inputShardStart[iByte] & 0xFF];
                }

                for (var iInput = 1; iInput < inputCount; iInput++)
                {
                    var inputShard = inputs[iInput];
                    var multTableRow = table[matrixRow[iInput] & 0xFF];
                    for (var iByte = offset; iByte < offset + byteCount; iByte++)
                    {
                        outputShard[iByte] ^= multTableRow[inputShard[iByte] & 0xFF];
                    }
                }
            }
        }

        /// <inheritdoc />
        public override bool CheckSomeShards(byte[][] matrixRows, byte[][] inputs,
            in int inputCount, byte[][] toCheck,
            in int checkCount, in int offset, in int byteCount, in byte[]? tempBuffer)
        {
            if (tempBuffer == null)
            {
                return base.CheckSomeShards(matrixRows, inputs, inputCount, toCheck, checkCount, offset, byteCount,
                    null);
            }

            var table = Galois.MultiplicationTable;
            for (var iOutput = 0; iOutput < checkCount; iOutput++)
            {
                var outputShard = toCheck[iOutput];
                var matrixRow = matrixRows[iOutput];

                const int iInputStart = 0;
                var inputShardStart = inputs[iInputStart];
                var multTableRowStart = table[matrixRow[iInputStart] & 0xFF];
                for (var iByte = offset; iByte < offset + byteCount; iByte++)
                {
                    tempBuffer[iByte] = multTableRowStart[inputShardStart[iByte] & 0xFF];
                }

                for (var iInput = 1; iInput < inputCount; iInput++)
                {
                    var inputShard = inputs[iInput];
                    var multTableRow = table[matrixRow[iInput] & 0xFF];
                    for (var iByte = offset; iByte < offset + byteCount; iByte++)
                    {
                        tempBuffer[iByte] ^= multTableRow[inputShard[iByte] & 0xFF];
                    }
                }

                for (var iByte = offset; iByte < offset + byteCount; iByte++)
                {
                    if (tempBuffer[iByte] != outputShard[iByte])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
