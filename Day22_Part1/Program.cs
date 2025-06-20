using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Day22Part1
{
    static void Main()
    {
        try
        {
            // 1. Get input file path
            string filePath = GetInputFilePath();
            if (filePath == null) return;

            // 2. Read and parse input
            var (map, path) = ParseInput(filePath);
            if (map == null || path == null) return;

            // 3. Find starting position
            var startPos = FindStartPosition(map);
            if (startPos == (-1, -1))
            {
                Console.WriteLine("Error: No starting position (.) found on first row");
                return;
            }

            // 4. Navigate the map
            var (finalPos, finalFacing) = NavigateMap(map, path, startPos);

            // 5. Calculate password
            int password = 1000 * (finalPos.y + 1) + 4 * (finalPos.x + 1) + finalFacing;
            Console.WriteLine($"Part 1: {password}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static string GetInputFilePath()
    {
        try
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
                Console.WriteLine("Error: Input file not found at " + filePath);
                return null;
            }
            return filePath;
        }
        catch
        {
            Console.WriteLine("Error: Could not determine input file path");
            return null;
        }
    }

    static (char[][] map, List<string> path) ParseInput(string filePath)
    {
        string[] sections = File.ReadAllText(filePath).Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        if (sections.Length < 2)
        {
            Console.WriteLine("Error: Input must contain map and path sections separated by blank line");
            return (null, null);
        }

        // Parse map
        var mapLines = sections[0].Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (mapLines.Length == 0)
        {
            Console.WriteLine("Error: Map section is empty");
            return (null, null);
        }

        var map = mapLines.Select(line => line.PadRight(mapLines.Max(l => l.Length), ' ').ToCharArray()).ToArray();

        // Parse path
        var path = new List<string>();
        string pathStr = sections[1].Trim();
        int i = 0;
        while (i < pathStr.Length)
        {
            if (pathStr[i] == 'L' || pathStr[i] == 'R')
            {
                path.Add(pathStr[i].ToString());
                i++;
            }
            else if (char.IsDigit(pathStr[i]))
            {
                int start = i;
                while (i < pathStr.Length && char.IsDigit(pathStr[i])) i++;
                path.Add(pathStr.Substring(start, i - start));
            }
            else
            {
                i++;
            }
        }

        if (path.Count == 0)
        {
            Console.WriteLine("Error: No valid path instructions found");
            return (null, null);
        }

        return (map, path);
    }

    static (int x, int y) FindStartPosition(char[][] map)
    {
        for (int x = 0; x < map[0].Length; x++)
        {
            if (map[0][x] == '.') return (x, 0);
        }
        return (-1, -1);
    }

    static ((int x, int y) pos, int facing) NavigateMap(char[][] map, List<string> path, (int x, int y) startPos)
    {
        var pos = startPos;
        int facing = 0; // 0: right, 1: down, 2: left, 3: up
        var moves = new (int dx, int dy)[] { (1, 0), (0, 1), (-1, 0), (0, -1) };

        foreach (var move in path)
        {
            if (move == "L") facing = (facing + 3) % 4;
            else if (move == "R") facing = (facing + 1) % 4;
            else
            {
                int steps = int.Parse(move);
                for (int i = 0; i < steps; i++)
                {
                    var nextPos = GetNextPosition(map, pos, moves[facing]);
                    if (map[nextPos.y][nextPos.x] == '#') break;
                    pos = nextPos;
                }
            }
        }

        return (pos, facing);
    }

    static (int x, int y) GetNextPosition(char[][] map, (int x, int y) pos, (int dx, int dy) move)
    {
        int newX = pos.x + move.dx;
        int newY = pos.y + move.dy;

        // Handle wrapping
        if (newY < 0 || newY >= map.Length || newX < 0 || newX >= map[newY].Length || map[newY][newX] == ' ')
        {
            // Move in opposite direction until we find the other edge
            int wrapX = pos.x;
            int wrapY = pos.y;
            while (true)
            {
                wrapX -= move.dx;
                wrapY -= move.dy;
                if (wrapY < 0 || wrapY >= map.Length || wrapX < 0 || wrapX >= map[wrapY].Length || map[wrapY][wrapX] == ' ')
                {
                    wrapX += move.dx;
                    wrapY += move.dy;
                    break;
                }
            }
            return (wrapX, wrapY);
        }

        return (newX, newY);
    }
}