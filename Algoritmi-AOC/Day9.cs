using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static int SolvePart1(string inputData)
    {
        var headPos = new int[] { 0, 0 };
        var tailPos = new int[] { 0, 0 };
        var visitedPositions = new HashSet<(int, int)>();
        visitedPositions.Add((tailPos[0], tailPos[1]));

        foreach (var line in inputData.Trim().Split("\n"))
        {
            var parts = line.Split(" ");
            var direction = parts[0];
            var steps = int.Parse(parts[1]);

            for (int i = 0; i < steps; i++)
            {
                // Move head
                switch (direction)
                {
                    case "R": headPos[0]++; break;
                    case "L": headPos[0]--; break;
                    case "U": headPos[1]++; break;
                    case "D": headPos[1]--; break;
                }

                // Calculate distance between head and tail
                int dx = headPos[0] - tailPos[0];
                int dy = headPos[1] - tailPos[1];

                // Move tail if necessary
                if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)
                {
                    if (dx == 0) // Same column, move vertically
                    {
                        tailPos[1] += dy > 0 ? 1 : -1;
                    }
                    else if (dy == 0) // Same row, move horizontally
                    {
                        tailPos[0] += dx > 0 ? 1 : -1;
                    }
                    else // Diagonal movement
                    {
                        tailPos[0] += dx > 0 ? 1 : -1;
                        tailPos[1] += dy > 0 ? 1 : -1;
                    }
                }
                visitedPositions.Add((tailPos[0], tailPos[1]));
            }
        }

        return visitedPositions.Count;
    }

    public static int SolvePart2(string inputData)
    {
        var knots = new List<int[]>();
        for (int i = 0; i < 10; i++)
        {
            knots.Add(new int[] { 0, 0 });
        }

        var visitedPositions = new HashSet<(int, int)>();
        visitedPositions.Add((knots[9][0], knots[9][1])); // Tail of the 9th knot

        foreach (var line in inputData.Trim().Split("\n"))
        {
            var parts = line.Split(" ");
            var direction = parts[0];
            var steps = int.Parse(parts[1]);

            for (int i = 0; i < steps; i++)
            {
                // Move head (first knot)
                switch (direction)
                {
                    case "R": knots[0][0]++; break;
                    case "L": knots[0][0]--; break;
                    case "U": knots[0][1]++; break;
                    case "D": knots[0][1]--; break;
                }

                // Move subsequent knots
                for (int j = 1; j < 10; j++)
                {
                    var head = knots[j - 1];
                    var tail = knots[j];

                    int dx = head[0] - tail[0];
                    int dy = head[1] - tail[1];

                    if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)
                    {
                        if (dx == 0) // Same column, move vertically
                        {
                            tail[1] += dy > 0 ? 1 : -1;
                        }
                        else if (dy == 0) // Same row, move horizontally
                        {
                            tail[0] += dx > 0 ? 1 : -1;
                        }
                        else // Diagonal movement
                        {
                            tail[0] += dx > 0 ? 1 : -1;
                            tail[1] += dy > 0 ? 1 : -1;
                        }
                    }
                }
                visitedPositions.Add((knots[9][0], knots[9][1]));
            }
        }

        return visitedPositions.Count;
    }

    public static void Main(string[] args)
    {
        string exampleInputPart1 = @"R 4
U 4
L 3
D 1
R 4
D 1
L 5
R 2";
        Console.WriteLine($"Part 1 Example Result: {SolvePart1(exampleInputPart1)}");

        string exampleInputPart2Small = @"R 4
U 4
L 3
D 1
R 4
D 1
L 5
R 2";
        Console.WriteLine($"Part 2 Small Example Result: {SolvePart2(exampleInputPart2Small)}");

        string exampleInputPart2Large = @"R 5
U 8
L 8
D 3
R 17
D 10
L 25
U 20";
        Console.WriteLine($"Part 2 Large Example Result: {SolvePart2(exampleInputPart2Large)}");
    }
}


