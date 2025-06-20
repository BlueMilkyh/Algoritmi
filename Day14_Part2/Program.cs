using System;
using System.Collections.Generic;
using System.IO;

class Day14Part2
{
    static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;

        string filePath = Path.Combine(threeUp, "day14.txt");
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        var rocks = new HashSet<(int x, int y)>();
        int maxY = 0;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var points = line.Split(" -> ");
            for (int i = 0; i < points.Length - 1; i++)
            {
                var (x1, y1) = ParsePoint(points[i]);
                var (x2, y2) = ParsePoint(points[i + 1]);

                if (x1 == x2)
                {
                    for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
                        rocks.Add((x1, y));
                }
                else
                {
                    for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
                        rocks.Add((x, y1));
                }

                maxY = Math.Max(maxY, Math.Max(y1, y2));
            }
        }

        int floorY = maxY + 2;
        var occupied = new HashSet<(int x, int y)>(rocks);
        int sandCount = 0;

        while (true)
        {
            int x = 500, y = 0;

            if (occupied.Contains((x, y)))
            {
                Console.WriteLine($"Part 2: {sandCount}");
                return;
            }

            while (true)
            {
                if (y + 1 == floorY)
                {
                    break;
                }
                else if (!occupied.Contains((x, y + 1))) y++;
                else if (!occupied.Contains((x - 1, y + 1))) { x--; y++; }
                else if (!occupied.Contains((x + 1, y + 1))) { x++; y++; }
                else break;
            }

            occupied.Add((x, y));
            sandCount++;
        }
    }

    static (int x, int y) ParsePoint(string point)
    {
        var parts = point.Split(',');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
