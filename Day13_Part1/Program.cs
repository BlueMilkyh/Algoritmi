using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Day13Part1
{
    static int ComparePackets(JsonElement left, JsonElement right)
    {
        if (left.ValueKind == JsonValueKind.Number && right.ValueKind == JsonValueKind.Number)
            return left.GetInt32().CompareTo(right.GetInt32());

        if (left.ValueKind == JsonValueKind.Number)
            left = JsonDocument.Parse($"[{left.GetRawText()}]").RootElement;
        if (right.ValueKind == JsonValueKind.Number)
            right = JsonDocument.Parse($"[{right.GetRawText()}]").RootElement;

        var leftArr = left.EnumerateArray().ToArray();
        var rightArr = right.EnumerateArray().ToArray();
        for (int i = 0; i < Math.Min(leftArr.Length, rightArr.Length); i++)
        {
            int cmp = ComparePackets(leftArr[i], rightArr[i]);
            if (cmp != 0) return cmp;
        }
        return leftArr.Length.CompareTo(rightArr.Length);
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

        var lines = File.ReadAllLines(filePath);
        int indexSum = 0;
        int pairIndex = 1;

        for (int i = 0; i < lines.Length; i += 3)
        {
            var left = JsonDocument.Parse(lines[i]).RootElement;
            var right = JsonDocument.Parse(lines[i + 1]).RootElement;
            if (ComparePackets(left, right) < 0)
                indexSum += pairIndex;
            pairIndex++;
        }

        Console.WriteLine($"Part 1: {indexSum}");
    }
}
