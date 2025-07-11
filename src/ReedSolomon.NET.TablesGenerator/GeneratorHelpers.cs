﻿namespace ReedSolomon.NET.TablesGenerator;

public static class Helpers
{
    private const int FieldSize = 256;

    /// <summary>
    /// The polynomial used to generate the logarithm table.
    /// There are a number of polynomials that work to generate
    /// a Galois field of 256 elements.  The choice is arbitrary, and we just use the first one.
    /// The possibilities are: 29, 43, 45, 77, 95, 99, 101, 105, 113, 135, 141, 169, 195, 207, 231, and 245.
    /// </summary>
    private const int GeneratingPolynomial = 29;

    private static readonly short[] _logTable =
    [
        -1, 0, 1, 25, 2, 50, 26, 198,
        3, 223, 51, 238, 27, 104, 199, 75,
        4, 100, 224, 14, 52, 141, 239, 129,
        28, 193, 105, 248, 200, 8, 76, 113,
        5, 138, 101, 47, 225, 36, 15, 33,
        53, 147, 142, 218, 240, 18, 130, 69,
        29, 181, 194, 125, 106, 39, 249, 185,
        201, 154, 9, 120, 77, 228, 114, 166,
        6, 191, 139, 98, 102, 221, 48, 253,
        226, 152, 37, 179, 16, 145, 34, 136,
        54, 208, 148, 206, 143, 150, 219, 189,
        241, 210, 19, 92, 131, 56, 70, 64,
        30, 66, 182, 163, 195, 72, 126, 110,
        107, 58, 40, 84, 250, 133, 186, 61,
        202, 94, 155, 159, 10, 21, 121, 43,
        78, 212, 229, 172, 115, 243, 167, 87,
        7, 112, 192, 247, 140, 128, 99, 13,
        103, 74, 222, 237, 49, 197, 254, 24,
        227, 165, 153, 119, 38, 184, 180, 124,
        17, 68, 146, 217, 35, 32, 137, 46,
        55, 63, 209, 91, 149, 188, 207, 205,
        144, 135, 151, 178, 220, 252, 190, 97,
        242, 86, 211, 171, 20, 42, 93, 158,
        132, 60, 57, 83, 71, 109, 65, 162,
        31, 45, 67, 216, 183, 123, 164, 118,
        196, 23, 73, 236, 127, 12, 111, 246,
        108, 161, 59, 82, 41, 157, 85, 170,
        251, 96, 134, 177, 187, 204, 62, 90,
        203, 89, 95, 176, 156, 169, 160, 81,
        11, 245, 22, 235, 122, 117, 44, 215,
        79, 174, 213, 233, 230, 231, 173, 232,
        116, 214, 244, 234, 168, 80, 88, 175
    ];


    /// <summary>
    /// Generates a logarithm table given a starting polynomial.
    /// </summary>
    /// <param name="polynomial">The starting polynomial</param>
    /// <returns>The logarithm table</returns>
    /// <exception cref="Exception">Duplication logarithm exception</exception>
    public static short[] GenerateLogTable(int polynomial)
    {
        var result = new short[FieldSize];
        for (var i = 0; i < FieldSize; i++)
        {
            result[i] = -1; // -1 means "not set"
        }

        var b = 1;
        for (var log = 0; log < FieldSize - 1; log++)
        {
            if (result[b] != -1) {
                throw new Exception("BUG: duplicate logarithm (bad polynomial?)");
            }
            result[b] = (short) log;
            b <<= 1;
            if (FieldSize <= b) {
                b = (b - FieldSize) ^ polynomial;
            }
        }
        return result;
    }

    /// <summary>
    /// Generates the inverse log table.
    /// </summary>
    /// <returns>The inverse of the log table</returns>
    public static byte[] GenerateExpTable()
    {
        var result = new byte [FieldSize * 2 - 2];
        for (var i = 1; i < FieldSize; i++) {
            int log = _logTable[i];

            result[log] = (byte)i;
            result[log + FieldSize -1] = (byte)i;
        }

        return result;
    }


    /// <summary>
    /// Generates a multiplication table as an array of byte arrays. To get the result of multiplying a and b: MULTIPLICATION_TABLE[a][b]
    /// </summary>
    /// <param name="expTable">The inverse of the log table.</param>
    /// <returns>The multiplication table.</returns>
    public static byte[][] GenerateMultiplicationTableSplit(byte[] expTable) {
        var result = new byte[FieldSize][];
        for(var i = 0; i < FieldSize; i++)
            result[i] = new byte[FieldSize];

        for(var a = 0; a < FieldSize; a++) {
            for(var b = 0; b < FieldSize; b++) {
                if(a == 0 || b == 0) {
                    result[a][b] = 0;
                    continue;
                }
                var logA = _logTable[a];
                var logB = _logTable[b];
                result[a][b] = expTable[logA + logB];
            }
        }
        return result;
    }

    /// <summary>
    /// Generates a multiplication table as an array of byte arrays. To get the result of multiplying a and b: MULTIPLICATION_TABLE[a][b]
    /// </summary>
    /// <param name="expTable">The inverse of the log table.</param>
    /// <returns>The multiplication table in one dimensional array..</returns>
    public static byte[] GenerateMultiplicationTable(byte[] expTable)
    {
        var result = new byte[FieldSize * FieldSize];
        for (var v = 0; v < FieldSize * FieldSize; v++)
        {
            var a = (byte)(v & 0xff);
            var b = (byte)(v >> 8);
            if(a == 0 || b == 0)
            {
                result[v] = 0;
                continue;
            }
            var logA = _logTable[a];
            var logB = _logTable[b];
            result[v]  = expTable[logA + logB];
        }

        return result;
    }


    /// <summary>
    /// Generates the inverse log table.
    /// </summary>
    /// <returns>The inverse of the log table</returns>
    public static ReadOnlySpan<byte> GenerateExpTableV2()
    {
        var result = new byte [FieldSize * 2 - 2];
        var resultSpan = result.AsSpan();
        for (var i = 1; i < FieldSize; i++) {
            int log = _logTable[i];

            resultSpan[log] = (byte)i;
            resultSpan[log + FieldSize -1] = (byte)i;
        }
        return resultSpan;
    }
}
