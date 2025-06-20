using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Day13Part2
{
    static int ComparePackets(JsonElement left, JsonElement right)
    {
        if (left.ValueKind == JsonValueKind.Number && right.ValueKind == JsonValueKind.Number)
            return left.GetInt32().CompareTo(right.GetInt32());

        if (left.ValueKind == JsonValueKind.Number)
            left = JsonDocument.Parse($"[{left.GetRawText()}]").RootElement;
        if (right.ValueKind == JsonValueKind.Number)
            right = JsonDocument.Parse($"[{right.GetRawText()}]").RootElement;

        var a = left.EnumerateArray().ToArray();
        var b = right.EnumerateArray().ToArray();
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            int cmp = ComparePackets(a[i], b[i]);
            if (cmp != 0) return cmp;
        }
        return a.Length.CompareTo(b.Length);
    }

    static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;
        string filePath = Path.Combine(threeUp, "day13.txt");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        var lines = File.ReadAllLines(filePath)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();

        var packets = lines
            .Select(line => JsonDocument.Parse(line).RootElement)
            .ToList();

        var dividerA = JsonDocument.Parse("[[2]]").RootElement;
        var dividerB = JsonDocument.Parse("[[6]]").RootElement;

        packets.Add(dividerA);
        packets.Add(dividerB);

        packets.Sort(ComparePackets);

        int posA = packets.FindIndex(p => p.ToString() == dividerA.ToString()) + 1;
        int posB = packets.FindIndex(p => p.ToString() == dividerB.ToString()) + 1;

        Console.WriteLine($"Part 2: {posA * posB}");
    }
}
