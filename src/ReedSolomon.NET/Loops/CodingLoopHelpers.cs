// Coding Loop Helpers.
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Loops
{
    /// <summary>
    /// All of the available coding loop algorithms.
    /// </summary>
    public static class CodingLoopHelpers
    {
        /// <summary>
        /// <para>All of the available coding loop algorithms.
        /// The different choices nest the three loops in different orders,
        /// and either use the log/exponents tables, or use the multiplication table.</para>
        /// The naming of the three loops is (with number of loops in benchmark): <br/>
        /// "byte"   - Index of byte within shard.  (200,000 bytes in each shard) <br/>
        /// "input"  - Which input shard is being read.  (17 data shards) <br/>
        /// "output"  - Which output shard is being computed.  (3 parity shards) <br/>
        /// And the naming for multiplication method is: <br/>
        /// "table"  - Use the multiplication table. <br/>
        /// "exp"    - Use the logarithm/exponent table. <br/>
        /// <para>The ReedSolomonBenchmark class compares the performance of the different
        /// loops, which will depend on the specific processor you're running on.</para>
        /// <para>This is the inner loop.  It needs to be fast.  Be careful if you change it.</para>
        /// <para>I have tried inlining Galois.multiply(), but it doesn't
        /// make things any faster.  The JIT compiler is known to inline methods,
        /// so it's probably already doing so.</para>
        /// </summary>
        public static readonly ICodingLoop[] AllCodingLoops =
        {
            new ByteInputOutputExpCodingLoop(), new ByteInputOutputTableCodingLoop(),
            new ByteOutputInputExpCodingLoop(), new ByteOutputInputTableCodingLoop(),
            new InputByteOutputExpCodingLoop(), new InputByteOutputTableCodingLoop(),
            new InputOutputByteExpCodingLoop(), new InputOutputByteTableCodingLoop(),
            new OutputByteInputExpCodingLoop(), new OutputByteInputTableCodingLoop(),
            new OutputInputByteExpCodingLoop(), new OutputInputByteTableCodingLoop()
        };
    }
}
