using ReedSolomon.NET.TablesGenerator;

var logTable = Helpers.GenerateLogTable(29);
ShowSyntaxRepresentation(logTable, "private static readonly short[] LogTable", false);

var expTable = Helpers.GenerateExpTable();
ShowSyntaxRepresentation(expTable, "private static readonly byte[] ExpTable", false);


var multiTable = Helpers.GenerateMultiplicationTable(expTable);
ShowSyntaxRepresentation(multiTable.ToArray(), "private static readonly byte[] MultiplicationTable", false);

var multiTableSplit = Helpers.GenerateMultiplicationTableSplit(expTable);
Show2DSyntaxRepresentation<byte[][]>(multiTableSplit, "private static readonly byte[][] MultiplicationTableSplit", true);

void ShowSyntaxRepresentation<T>(IReadOnlyList<T> array, string declaration, bool useHex = false)
{
    Console.Write($"{declaration} = \n{{\n");
    var columnNumber = 0;
    for (var index = 0; index < array.Count; index++)
    {
        var b = array[index];
        columnNumber++;
        var value = useHex ? $"0x{b:x2}" : b?.ToString();

        Console.Write(index != array.Count - 1 ? $"{value}, " : value);

        if (columnNumber != 26)
            continue;
        Console.WriteLine();
        columnNumber = 0;
    }

    Console.WriteLine("\n};");
}

void Show2DSyntaxRepresentation<T>(IReadOnlyList<byte[]> array, string declaration, bool useHex = false)
{
    Console.Write($"{declaration} = \n{{\n");
    foreach (var row in array)
    {
        Console.Write($"{{");
        for (var i = 0; i < row.Length; i++)
        {
            var value = useHex ? $"0x{row[i]:x2}" : row[i].ToString();
            Console.Write(i != row.Length - 1 ? $"{value}, " : value);
        }
        var endOfRow = !row.Equals(array[^1]) ? "}," : "}";
        Console.WriteLine(endOfRow);

    }
    Console.WriteLine("\n};");
}



/*var m1 = new Matrix(new[]
{
    new byte[] {1, 2, 3},
    new byte[] {4, 5, 6},
    new byte[] {7, 8, 9}
});
Console.WriteLine("m1:");
m1.Print();
Console.WriteLine("Get row 0:");
var row0 = m1.GetRow(0);
foreach (var b in row0)
{
    Console.Write($"{b} ");
}
Console.WriteLine("\nModified row0:");
row0[0] = 0;
foreach (var b in row0)
{
    Console.Write($"{b} ");
}
Console.WriteLine("\nm1:");
m1.Print();



var data = new[]
{
    new byte[] {1, 2, 3},
    new byte[] {4, 5, 6},
    new byte[] {7, 8, 9}
};
var m2 = new Matrix(data.AsSpan());
Console.WriteLine("m2:");
m2.Print();

class Matrix
{
    readonly int _columns;
    readonly int _rows;
    readonly byte[][] _data;

    public Matrix(Span<byte[]> data)
    {
        _rows = data.Length;
        _columns = data[0].Length;
        _data = new byte[_rows][];
        for (var r = 0; r < _rows; r++)
        {
            if (data[r].Length != _columns)
                throw new ArgumentException("All rows must have the same number of columns.");

            _data[r] = new byte[_columns];
            //data[r].CopyTo(_data[r], 0);
            Buffer.BlockCopy(data[r], 0, _data[r], 0, _columns);
        }
    }

    public Matrix(byte[][] data)
    {
        _rows = data.Length;
        _columns = data[0].Length;
        _data = new byte[_rows][];
        for (var r = 0; r < _rows; r++)
        {
            if (data[r].Length != _columns)
                throw new ArgumentException("All rows must have the same number of columns.");

            _data[r] = new byte[_columns];
            for (var c = 0; c < _columns; c++) {
                _data[r][c] = data[r][c];
            }
        }
    }


    public byte[] GetRow(int row) => _data[row].AsSpan().ToArray();

    public void Print()
    {
        for (var r = 0; r < _rows; r++)
        {
            for (var c = 0; c < _columns; c++)
            {
                Console.Write(_data[r][c] + " ");
            }
            Console.WriteLine();
        }
    }
}
*/

/*var data = new[]
{
    new byte[] {1, 2, 3},
    new byte[] {4, 5, 6},
    new byte[] {7, 8, 9}
};
Print2DArray(data.AsSpan());

void Print2DArray(ReadOnlySpan<byte[]> array)
{
    foreach (var t in array)
    {
        foreach (var t1 in t)
        {
            Console.Write($"{t1} ");
        }

        Console.WriteLine();
    }
}*/
