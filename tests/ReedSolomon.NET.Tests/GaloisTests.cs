// Galois Tests
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.  All rights reserved.

namespace ReedSolomon.NET.Tests;

public class GaloisTests
{
    [Fact]
    public void Closure_Should_Never_Be_Tested()
    {
        // Unlike the Python implementation, there is no need to test
        // for closure.  Because add(), subtract(), multiply(), and
        // divide() all return bytes, there's no way they could
        // possibly return something outside the field.
    }

    [Fact]
    public void Add_And_Multiply_Associativity()
    {
        for (var i = 0; i <= 255; i++)
        {
            var a = Convert.ToByte(i);
            for (var j = 0; j <= 255; j++)
            {
                var b = Convert.ToByte(j);
                for (var k = 0; k <= 255; k++)
                {
                    var c = Convert.ToByte(k);
                    Galois.Add(a, Galois.Add(b, c))
                        .ShouldBe(Galois.Add(Galois.Add(a, b), c));
                    Galois.Multiply(a, Galois.Multiply(b, c))
                        .ShouldBe(Galois.Multiply(Galois.Multiply(a, b), c));
                }
            }
        }
    }

    [Fact]
    public void Add_And_Multiply_Identity()
    {
        for (var i = 0; i < 255; i++)
        {
            var a = (byte)i;
            Galois.Add(a, 0).ShouldBe(a);
            Galois.Multiply(a, 1).ShouldBe(a);
        }
    }

    [Fact]
    public void Add_And_Multiply_Inverse()
    {
        for (var i = 0; i < 255; i++)
        {
            var a = Convert.ToByte(i);

            var subtractResult = Galois.Subtract(0, a);
            Galois.Add(a, subtractResult).ShouldBe((byte)0);
            if (a == 0)
                continue;
            var b = Galois.Divide(1, a);
            Galois.Multiply(a, b).ShouldBe((byte)1);
        }
    }

    [Fact]
    public void Add_And_Multiply_Commutativity()
    {
        for (var i = 0; i < 255; i++)
        {
            for (var j = 0; j < 255; j++)
            {
                var a = Convert.ToByte(i);
                var b = Convert.ToByte(j);
                Galois.Add(a, b).ShouldBe(Galois.Add(b, a));
                Galois.Multiply(a, b).ShouldBe(Galois.Multiply(b, a));
            }
        }
    }

    [Fact]
    public void Add_And_Multiply_Distributivity()
    {
        for (var i = 0; i < 255; i++)
        {
            var a = Convert.ToByte(i);
            for (var j = 0; j < 255; j++)
            {
                var b = Convert.ToByte(j);
                for (var k = 0; k < 255; k++)
                {
                    var c = Convert.ToByte(k);
                    Galois.Multiply(a, Galois.Add(b, c))
                        .ShouldBe(Galois.Add(Galois.Multiply(a, b), Galois.Multiply(a, c)));
                }
            }
        }
    }

    [Fact]
    public void Exp_And_Power_Via_Multiply_Should_Be_The_Same()
    {
            for (var i = 0; i <= 255; i++)
            {
                var a = Convert.ToByte(i);
                byte power = 1;
                for (var j = 0; j <= 255; j++)
                {
                    Galois.Exp(a, j).ShouldBe(power);
                    power = Galois.Multiply(power, a);
                }
            }
    }

    [Fact]
    public void GeneratedTableMethods_Should_Return_the_Same_Result_As_The_Const()
    {
        var logTable = Galois.GenerateLogTable();
        logTable.ShouldBe(Galois.LogTable);

        var expTable = Galois.GenerateExpTable();
        expTable.ShouldBe(Galois.ExpTable);

        var multiplicationTable = Galois.GenerateMultiplicationTable();
        multiplicationTable.ShouldBe(Galois.MultiplicationTable);
    }
}
