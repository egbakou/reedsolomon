// Common implementations for coding loops.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// Common implementations for coding loops.
    /// Many of the coding loops do not have custom checkSomeShards() methods.
    /// The benchmark doesn't measure that method.
    /// </summary>
    public abstract class CodingLoopBase : ICodingLoop
    {
        /// <inheritdoc />
        public abstract void CodeSomeShards(byte[][] matrixRows, byte[][] inputs, in int inputCount,
            byte[][] outputs,
            in int outputCount, in int offset, in int byteCount);

        /// <inheritdoc />
        public virtual bool CheckSomeShards(byte[][] matrixRows,
            byte[][] inputs,
            in int inputCount,
            byte[][] toCheck,
            in int checkCount, in int offset, in int byteCount, in byte[]? tempBuffer)
        {
            // This is the loop structure for ByteOutputInput, which does not
            // require temporary buffers for checking.
            var table = Galois.MultiplicationTable;
            for (var iByte = offset; iByte < offset + byteCount; iByte++)
            {
                for (var iOutput = 0; iOutput < checkCount; iOutput++)
                {
                    var matrixRow = matrixRows[iOutput];
                    var value = 0;
                    for (var iInput = 0; iInput < inputCount; iInput++)
                    {
                        value ^= table[matrixRow[iInput] & 0xFF][inputs[iInput][iByte] & 0xFF];
                    }

                    if (toCheck[iOutput][iByte] != (byte)value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
