using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;

        string filePath = Path.Combine(threeUp, "day13.txt");

        var input = File.ReadAllText(filePath);
        var packets = ParseInput(input);

        // Part 1
        var sum = 0;
        for (int i = 0; i < packets.Count; i += 2)
        {
            var pairIndex = (i / 2) + 1;
            if (Compare(packets[i], packets[i + 1]) <= 0)
            {
                sum += pairIndex;
            }
        }
        Console.WriteLine($"Part 1: {sum}");

        // Part 2
        var divider1 = ParsePacket("[[2]]");
        var divider2 = ParsePacket("[[6]]");

        packets.Add(divider1);
        packets.Add(divider2);

        packets.Sort((a, b) => Compare(a, b));

        var index1 = packets.IndexOf(divider1) + 1;
        var index2 = packets.IndexOf(divider2) + 1;

        Console.WriteLine($"Part 2: {index1 * index2}");
    }

    static List<JsonElement> ParseInput(string input)
    {
        return input.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(ParsePacket)
                    .ToList();
    }

    static JsonElement ParsePacket(string line)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            return doc.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing line: '{line}'");
            throw;
        }
    }

    static int Compare(JsonElement left, JsonElement right)
    {
        if (left.ValueKind == JsonValueKind.Number && right.ValueKind == JsonValueKind.Number)
        {
            return left.GetInt32().CompareTo(right.GetInt32());
        }

        var leftArray = left.ValueKind == JsonValueKind.Array ? left : WrapInArray(left);
        var rightArray = right.ValueKind == JsonValueKind.Array ? right : WrapInArray(right);

        for (int i = 0; i < Math.Min(leftArray.GetArrayLength(), rightArray.GetArrayLength()); i++)
        {
            var comparison = Compare(leftArray[i], rightArray[i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return leftArray.GetArrayLength().CompareTo(rightArray.GetArrayLength());
    }

    static JsonElement WrapInArray(JsonElement element)
    {
        using var doc = JsonDocument.Parse($"[{element}]");
        return doc.RootElement.Clone();
    }
}