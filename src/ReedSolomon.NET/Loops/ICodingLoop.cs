// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// Interface for a method of looping over inputs and encoding them.
    /// </summary>
    public interface ICodingLoop
    {
        /// <summary>
        /// Multiplies a subset of rows from a coding matrix by a full set of
        /// input shards to produce some output shards.
        /// </summary>
        /// <param name="matrixRows">The rows from the matrix to use.</param>
        /// <param name="inputs">
        /// An array of byte arrays, each of which is one input shard.
        /// The inputs array may have extra buffers after the ones that are used.
        /// They will be ignored.  The number of inputs used is determined by the length of the each matrix row.
        /// </param>
        /// <param name="inputCount">The number of input byte arrays.</param>
        /// <param name="outputs">Byte arrays where the computed shards are stored.  The outputs array may also have
        /// extra, unused, elements at the end.  The number of outputs computed, and the number of matrix rows used,
        /// is determined by outputCount.
        /// </param>
        /// <param name="outputCount">The number of outputs to compute.</param>
        /// <param name="offset">The index in the inputs and output of the first byte to process.</param>
        /// <param name="byteCount">The number of bytes to process.</param>
        void CodeSomeShards(byte[][] matrixRows,
            byte[][] inputs,
            in int inputCount,
            byte[][] outputs,
            in int outputCount,
            in int offset,
            in int byteCount);

        /// <summary>
        /// Multiplies a subset of rows from a coding matrix by a full set of input shards to produce
        /// some output shards, and checks that the the data is those shards matches what's expected.
        /// </summary>
        /// <param name="matrixRows">The rows from the matrix to use.</param>
        /// <param name="inputs">
        /// An array of byte arrays, each of which is one input shard. The inputs array may have
        /// extra buffers after the ones that are used.  They will be ignored.
        /// The number of inputs used is determined by the length of the each matrix row.
        /// </param>
        /// <param name="inputCount">THe number of input byte arrays.</param>
        /// <param name="toCheck">
        /// Byte arrays where the computed shards are stored.  The outputs array may also have
        /// extra, unused, elements at the end.  The number of outputs computed, and the number of matrix rows used,
        /// is determined by outputCount.
        /// </param>
        /// <param name="checkCount">The number of outputs to compute.</param>
        /// <param name="offset">The index in the inputs and output of the first byte to process.</param>
        /// <param name="byteCount">The number of bytes to process.</param>
        /// <param name="tempBuffer">A place to store temporary results.  May be null.</param>
        /// <returns></returns>
        bool CheckSomeShards(byte[][] matrixRows,
            byte[][] inputs,
            in int inputCount,
            byte[][] toCheck,
            in int checkCount,
            in int offset,
            in int byteCount,
            in byte[]? tempBuffer);
    }
}
