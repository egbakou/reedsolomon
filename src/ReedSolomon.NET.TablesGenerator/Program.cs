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
