// Reed-Solomon Coding over 8-bit values.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

using System;
using ReedSolomon.NET.Loops;

namespace ReedSolomon.NET
{
    /// <summary>
    /// Reed-Solomon Coding over 8-bit values.
    /// </summary>
    public class ReedSolomon
    {
        private readonly int _dataShardCount;
        private readonly int _parityShardCount;
        private readonly int _totalShardCount;
        private readonly Matrix _matrix;
        private readonly ICodingLoop _codingLoop;

        /// <summary>
        /// Rows from the matrix for encoding parity, each one as its own
        /// byte array to allow for efficient access while encoding.
        /// </summary>
        /// <returns></returns>
        private readonly byte[][] _parityRows;

        /// <summary>
        /// Initializes a new encoder/decoder, with a chosen coding loop.
        /// </summary>
        public ReedSolomon(int dataShardCount, int parityShardCount, ICodingLoop codingLoop)
        {
            // We can have at most 256 shards total, as any more would
            // lead to duplicate rows in the Vandermonde matrix, which
            // would then lead to duplicate rows in the built matrix
            // below. Then any subset of the rows containing the duplicate
            // rows would be singular.
            if (256 < dataShardCount + parityShardCount)
                throw new ArgumentException("too many shards - max is 256");

            _dataShardCount = dataShardCount;
            _parityShardCount = parityShardCount;
            _codingLoop = codingLoop;
            _totalShardCount = dataShardCount + parityShardCount;
            _matrix = BuildMatrix(dataShardCount, _totalShardCount);
            _parityRows = new byte[parityShardCount][];

            for (var i = 0; i < parityShardCount; i++)
                _parityRows[i] = _matrix.GetRow(dataShardCount + i);
        }

        /// <summary>
        /// Creates a ReedSolomon codec with the default coding loop.
        /// </summary>
        public static ReedSolomon Create(int dataShardCount, int parityShardCount) =>
            new ReedSolomon(dataShardCount, parityShardCount, new InputOutputByteTableCodingLoop());

        /// <summary>
        /// Returns the number of data shards.
        /// </summary>
        /// <returns>The number of data shards.</returns>
        public int GetDataShardCount() => _dataShardCount;

        /// <summary>
        /// Returns the number of parity shards.
        /// </summary>
        /// <returns>The number of parity shards.</returns>
        public int GetParityShardCount() => _parityShardCount;

        /// <summary>
        /// Returns the total number of shards.
        /// </summary>
        /// <returns>The total number of shards.</returns>
        public int GetTotalShardCount() => _totalShardCount;

        /// <summary>Encodes parity for a set of data shards.</summary>
        /// <param name="shards"> An array containing data shards followed by parity shards. Each shard is a byte array,
        /// and they must all be the same size.
        /// </param>
        /// <param name="offset">The index of the first byte in each shard to encode.</param>
        /// <param name="byteCount">The number of bytes to encode in each shard.</param>
        public void EncodeParity(byte[][] shards, int offset, int byteCount)
        {
            // Check arguments.
            CheckBuffersAndSizes(shards, offset, byteCount);

            // Build the array of output buffers.
            var outputs = new byte[_parityShardCount][];
            Array.Copy(shards,
                _dataShardCount,
                outputs,
                0,
                _parityShardCount);
            // or Buffer.BlockCopy(shards, _dataShardCount, outputs, 0, _parityShardCount);


            // Do the coding.
            _codingLoop.CodeSomeShards(_parityRows,
                shards,
                _dataShardCount,
                outputs, _parityShardCount,
                offset, byteCount);
        }

        /// <summary>
        /// Returns true if the parity shards contain the right data.
        /// </summary>
        /// <param name="shards"> An array containing data shards followed by parity shards.
        /// Each shard is a byte array, and they must all be the same size.
        /// </param>
        /// <param name="firstByte">The index of the first byte in each shard to check.</param>
        /// <param name="byteCount">The number of bytes to check in each shard.</param>
        /// <returns>True if the parity shards contain the right data, otherwise false.</returns>
        public bool IsParityCorrect(byte[][] shards, int firstByte, int byteCount)
        {
            // Check arguments.
            CheckBuffersAndSizes(shards, firstByte, byteCount);

            // Build the array of buffers being checked.
            var toCheck = new byte[_parityShardCount][];
            Array.Copy(shards,
                _dataShardCount,
                toCheck,
                0,
                _parityShardCount);

            // Do the checking.
            return _codingLoop.CheckSomeShards(
                _parityRows,
                shards, _dataShardCount,
                toCheck, _parityShardCount,
                firstByte, byteCount,
                null);
        }

        /// <summary>
        /// Returns true if the parity shards contain the right data.
        /// This method may be significantly faster than the one above that does not use a temporary buffer.
        /// </summary>
        /// <param name="shards">An array containing data shards followed by parity shards.
        /// Each shard is a byte array, and they must all be the same size.
        /// </param>
        /// <param name="firstByte">The index of the first byte in each shard to check.</param>
        /// <param name="byteCount">The number of bytes to check in each shard.</param>
        /// <param name="tempBuffer">A temporary buffer (the same size as each of the shards) to use when computing parity.</param>
        public bool IsParityCorrect(byte[][] shards, int firstByte, int byteCount, byte[] tempBuffer)
        {
            // Check arguments.
            CheckBuffersAndSizes(shards, firstByte, byteCount);

            if (tempBuffer.Length < firstByte + byteCount)
                throw new ArgumentException("tempBuffer is not big enough");

            // Build the array of buffers being checked.
            var toCheck = new byte[_parityShardCount][];
            Array.Copy(shards, _dataShardCount, toCheck, 0, _parityShardCount);

            // Do the checking.
            return _codingLoop.CheckSomeShards(
                _parityRows,
                shards,
                _dataShardCount,
                toCheck, _parityShardCount,
                firstByte,
                byteCount,
                tempBuffer);
        }

        /// <summary>
        /// Given a list of shards, some of which contain data, fills in the ones that don't have data. Quickly does
        /// nothing if all of the shards are present. If any shards are missing (based on the flags in shardsPresent),
        /// the data in those shards is recomputed and filled in.
        /// </summary>
        public void DecodeMissing(byte[][] shards, bool[] shardPresent, in int offset, in int byteCount)
        {
            // Check arguments.
            CheckBuffersAndSizes(shards, offset, byteCount);

            // Quick check: are all of the shards present?  If so, there's
            // nothing to do.
            var numberPresent = 0;
            for (var i = 0; i < _totalShardCount; i++)
            {
                numberPresent += shardPresent[i] ? 1 : 0;
            }

            // Cool.  All of the shards data data.  We don't
            // need to do anything.
            if (numberPresent == _totalShardCount)
                return;

            // More complete sanity check
            if (numberPresent < _dataShardCount)
                throw new ArgumentException("Not enough shards present");

            // Pull out the rows of the matrix that correspond to the
            // shards that we have and build a square matrix.  This
            // matrix could be used to generate the shards that we have
            // from the original data.
            //
            // Also, pull out an array holding just the shards that
            // correspond to the rows of the sub-matrix.  These shards
            // will be the input to the decoding process that re-creates
            // the missing data shards.
            var subMatrix = new Matrix(_dataShardCount, _dataShardCount);
            var subShards = new byte[_dataShardCount][];

            var subMatrixRowStart = 0;
            for (var matrixRow = 0; matrixRow < _totalShardCount && subMatrixRowStart < _dataShardCount; matrixRow++)
            {
                if (!shardPresent[matrixRow])
                    continue;
                for (var c = 0; c < _dataShardCount; c++)
                {
                    subMatrix.Set(subMatrixRowStart, c, _matrix.Get(matrixRow, c));
                }

                subShards[subMatrixRowStart] = shards[matrixRow];
                subMatrixRowStart += 1;
            }

            // Invert the matrix, so we can go from the encoded shards
            // back to the original data.  Then pull out the row that
            // generates the shard that we want to decode.  Note that
            // since this matrix maps back to the original data, it can
            // be used to create a data shard, but not a parity shard.
            var dataDecodeMatrix = subMatrix.Invert();

            // Re-create any data shards that were missing.
            // The input to the coding is all of the shards we actually
            // have, and the output is the missing data shards.  The computation
            // is done using the special decode matrix we just built.
            var outputs = new byte[_parityShardCount][];
            var matrixRows = new byte[_parityShardCount][];
            var outputCount = 0;
            for (var iShard = 0; iShard < _dataShardCount; iShard++)
            {
                if (shardPresent[iShard])
                    continue;
                outputs[outputCount] = shards[iShard];
                matrixRows[outputCount] = dataDecodeMatrix.GetRow(iShard);
                outputCount += 1;
            }

            _codingLoop.CodeSomeShards(
                matrixRows,
                subShards, _dataShardCount,
                outputs, outputCount,
                offset, byteCount);

            // Now that we have all of the data shards intact, we can
            // compute any of the parity that is missing.
            // The input to the coding is ALL of the data shards, including
            // any that we just calculated.  The output is whichever of the
            // data shards were missing.
            outputCount = 0;
            for (var iShard = _dataShardCount; iShard < _totalShardCount; iShard++)
            {
                if (shardPresent[iShard])
                    continue;
                outputs[outputCount] = shards[iShard];
                matrixRows[outputCount] = _parityRows[iShard - _dataShardCount];
                outputCount += 1;
            }

            _codingLoop.CodeSomeShards(
                matrixRows,
                shards, _dataShardCount,
                outputs, outputCount,
                offset, byteCount);
        }

        // Checks the consistency of arguments passed to public methods.
        private void CheckBuffersAndSizes(byte[][] shards, int offset, int byteCount)
        {
            // The number of buffers should be equal to the number of
            // data shards plus the number of parity shards.
            if (shards.Length != _totalShardCount)
            {
                throw new ArgumentException("wrong number of shards: " + shards.Length);
            }

            // All of the shard buffers should be the same length.
            var shardLength = shards[0].Length;
            for (var i = 1; i < shards.Length; i++)
            {
                if (shards[i].Length != shardLength)
                    throw new ArgumentException("Shards are different sizes");
            }

            // The offset and byteCount must be non-negative and fit in the buffers.
            if (offset < 0)
                throw new ArgumentException("offset is negative: " + offset);

            if (byteCount < 0)
                throw new ArgumentException("byteCount is negative: " + byteCount);

            if (shardLength < offset + byteCount)
                throw new ArgumentException("buffers to small: " + byteCount + offset);
        }

        /// <summary>
        /// Create the matrix to use for encoding, given the number of data shards and the number of total shards.
        /// The top square of the matrix is guaranteed to be an identity matrix,
        /// which means that the data shards are unchanged after encoding.
        /// </summary>
        private static Matrix BuildMatrix(int dataShards, int totalShards)
        {
            // Start with a Vandermonde matrix.  This matrix would work,
            // in theory, but doesn't have the property that the data
            // shards are unchanged after encoding.
            var vandermonde = Matrix.Vandermonde(totalShards, dataShards);

            // Multiple by the inverse of the top square of the matrix.
            // This will make the top square be the identity matrix, but
            // preserve the property that any square subset of rows is
            // invertible.
            var top = vandermonde.SubMatrix(0, 0, dataShards, dataShards);

            return vandermonde.Multiply(top.Invert());
        }
    }
}
