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

        string filePath = Path.Combine(threeUp, "day9.txt");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found at: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        var knots = new (int x, int y)[10];  // 10 knots, all start at (0, 0)
        var visited = new HashSet<string>();
        visited.Add("0,0");

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

            for (int s = 0; s < steps; s++)
            {
                // Move head
                knots[0].x += dx;
                knots[0].y += dy;

                // Move all other knots
                for (int i = 1; i < knots.Length; i++)
                {
                    int diffX = knots[i - 1].x - knots[i].x;
                    int diffY = knots[i - 1].y - knots[i].y;

                    if (Math.Abs(diffX) > 1 || Math.Abs(diffY) > 1)
                    {
                        knots[i].x += Math.Sign(diffX);
                        knots[i].y += Math.Sign(diffY);
                    }
                }

                // Track the tail
                var tail = knots[9];
                visited.Add($"{tail.x},{tail.y}");
            }
        }

        Console.WriteLine($"Part 2: {visited.Count}");
    }
}
