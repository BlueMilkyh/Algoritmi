using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Day22Part2
{
    static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;

        string filePath = Path.Combine(threeUp, "day22.txt");
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        var input = File.ReadAllText(filePath).Split("\n\n");
        var map = input[0].Split('\n').Select(line => line.ToCharArray()).ToArray();
        var path = ParsePath(input[1].Trim());

        (int x, int y) pos = (map[0].ToList().FindIndex(c => c == '.'), 0);
        var facing = 0; // 0: right, 1: down, 2: left, 3: up

        foreach (var move in path)
        {
            if (move == "L")
            {
                facing = (facing + 3) % 4;
            }
            else if (move == "R")
            {
                facing = (facing + 1) % 4;
            }
            else
            {
                int steps = int.Parse(move.ToString());
                for (int i = 0; i < steps; i++)
                {
                    var (next, newFacing) = GetNextPositionCube(map, pos, facing);
                    if (map[next.y][next.x] == '#') break;
                    pos = next;
                    facing = newFacing;
                }
            }
        }

        Console.WriteLine($"Part 2: {1000 * (pos.y + 1) + 4 * (pos.x + 1) + facing}");
    }

    static ((int x, int y) pos, int facing) GetNextPositionCube(char[][] map, (int x, int y) pos, int facing)
    {
        var (dx, dy) = facing switch
        {
            0 => (1, 0),
            1 => (0, 1),
            2 => (-1, 0),
            3 => (0, -1),
            _ => throw new Exception("Invalid facing")
        };

        var next = (x: pos.x + dx, y: pos.y + dy);
        int newFacing = facing;

        // Handle cube transitions (hardcoded for sample input)
        if (next.y < 0 || next.y >= map.Length || next.x < 0 || next.x >= map[next.y].Length || map[next.y][next.x] == ' ')
        {
            // This is where cube folding logic would go
            // For actual solution, you'd need to implement the specific cube transitions
            // for your input's cube net configuration
            throw new NotImplementedException("Cube folding logic not implemented");
        }

        return (next, newFacing);
    }

    static List<string> ParsePath(string path)
    {
        var result = new List<string>();
        int i = 0;
        while (i < path.Length)
        {
            if (path[i] == 'L' || path[i] == 'R')
            {
                result.Add(path[i].ToString());
                i++;
            }
            else
            {
                int start = i;
                while (i < path.Length && char.IsDigit(path[i])) i++;
                result.Add(path.Substring(start, i - start));
            }
        }
        return result;
    }
}