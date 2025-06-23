using System;
using System.Collections.Generic;
using System.IO;

class Day18_Part2
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

        int min = 0, max = 0;
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            var cube = (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
            cubes.Add(cube);
            max = Math.Max(max, Math.Max(cube.Item1, Math.Max(cube.Item2, cube.Item3)));
        }

        max += 2; // for boundary

        var visited = new HashSet<(int, int, int)>();
        var queue = new Queue<(int, int, int)>();
        queue.Enqueue((max, max, max));
        visited.Add((max, max, max));

        int area = 0;
        int[] dirs = { 1, -1 };

        while (queue.Count > 0)
        {
            var (x, y, z) = queue.Dequeue();

            foreach (var dx in dirs)
            {
                var nx = x + dx;
                if (IsValid(nx)) TryVisit(nx, y, z);
            }

            foreach (var dy in dirs)
            {
                var ny = y + dy;
                if (IsValid(ny)) TryVisit(x, ny, z);
            }

            foreach (var dz in dirs)
            {
                var nz = z + dz;
                if (IsValid(nz)) TryVisit(x, y, nz);
            }
        }

        Console.WriteLine("Part 2: " + area);

        void TryVisit(int x, int y, int z)
        {
            var pos = (x, y, z);
            if (cubes.Contains(pos))
            {
                area++;
                return;
            }

            if (visited.Contains(pos)) return;
            if (x < -1 || y < -1 || z < -1 || x > max + 1 || y > max + 1 || z > max + 1) return;

            visited.Add(pos);
            queue.Enqueue(pos);
        }

        bool IsValid(int v) => v >= -1 && v <= max + 1;
    }
}
