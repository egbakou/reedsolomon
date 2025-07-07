// Matrix Tests
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Tests;

public class MatrixTests
{
    [Fact]
    public void Identity_Should_Return_The_Right_Matrix() =>
        Matrix.Identity(3).ToString().ShouldBe("[1, 0, 0]\n[0, 1, 0]\n[0, 0, 1]\n");

    [Fact]
    public void ToOneLineString_Should_Return_The_Right_String() =>
        Matrix.Identity(3).ToOneLineString().ShouldBe("[[1, 0, 0], [0, 1, 0], [0, 0, 1]]");

    [Fact]
    public void Multiply_Two_Matrices_Should_Return_The_Right_Matrix()
    {
        var m1 = new Matrix([
            [1, 2],
            [3, 4]
        ]);

        var m2 = new Matrix([
            [5, 6],
            [7, 8]
        ]);
        var result = m1.Multiply(m2);
        result.ToString().ShouldBe("[11, 22]\n[19, 42]\n");
    }

    [Fact]
    public void Inverse_Matrix_Should_Return_The_Right_Matrix()
    {
        Matrix m = new ([
            [56, 23, 98],
            [3, 100, 200],
            [45, 201, 123]
        ]);
        m.Invert().ToString().ShouldBe("[175, 133, 33]\n[130, 13, 245]\n[112, 35, 126]\n");
        Matrix.Identity(3).ShouldBe(m.Multiply(m.Invert()));
    }
}
