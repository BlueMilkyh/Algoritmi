using System;
using System.Collections.Generic;
using System.IO;

class Day18_Part1
{
    static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;

        string filePath = Path.Combine(threeUp, "day18.txt");
        var lines = File.ReadAllLines(filePath);
        var cubes = new HashSet<(int x, int y, int z)>();

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            cubes.Add((int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])));
        }

        int surfaceArea = 0;
        foreach (var (x, y, z) in cubes)
        {
            int exposed = 6;
            if (cubes.Contains((x + 1, y, z))) exposed--;
            if (cubes.Contains((x - 1, y, z))) exposed--;
            if (cubes.Contains((x, y + 1, z))) exposed--;
            if (cubes.Contains((x, y - 1, z))) exposed--;
            if (cubes.Contains((x, y, z + 1))) exposed--;
            if (cubes.Contains((x, y, z - 1))) exposed--;
            surfaceArea += exposed;
        }

        Console.WriteLine("Part 1: " + surfaceArea);
    }
}
