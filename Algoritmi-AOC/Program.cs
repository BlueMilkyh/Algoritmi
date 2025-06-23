using System;
using System.Collections.Generic;
using System.IO;

class Program
{
   public static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                        Directory.GetParent(
                            Directory.GetParent(current).FullName
                        ).FullName
                     ).FullName;
        Console.WriteLine(threeUp);
        string filePath = Path.Combine(threeUp, "day9.txt");
        string[] lines = File.ReadAllLines(filePath);

        var head = new int[] { 0, 0 };
        var tail = new int[] { 0, 0 };

        var visited = new HashSet<string>();
        visited.Add($"{tail[0]},{tail[1]}");

        var directions = new Dictionary<string, (int dx, int dy)>
        {
            { "U", (0, 1) },
            { "D", (0, -1) },
            { "L", (-1, 0) },
            { "R", (1, 0) }
        };

        foreach (var line in lines)
        {
            var parts = line.Split(' ');
            string dir = parts[0];
            int steps = int.Parse(parts[1]);

            var (dx, dy) = directions[dir];

            for (int i = 0; i < steps; i++)
            {
                // Move head
                head[0] += dx;
                head[1] += dy;

                // Check distance between head and tail
                int diffX = head[0] - tail[0];
                int diffY = head[1] - tail[1];

                if (Math.Abs(diffX) > 1 || Math.Abs(diffY) > 1)
                {
                    tail[0] += Math.Sign(diffX);
                    tail[1] += Math.Sign(diffY);

                }

                visited.Add($"{tail[0]},{tail[1]}");
            }
        }

        Console.WriteLine($"Part 1: {visited.Count}");
    }
}
