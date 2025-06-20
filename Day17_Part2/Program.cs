using System;
using System.Collections.Generic;
using System.IO;

class Day17Part2
{
    static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;

        string filePath = Path.Combine(threeUp, "day17.txt");
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        string jetPattern = File.ReadAllText(filePath).Trim();
        Console.WriteLine($"Part 2: {SimulateRocks(jetPattern, 1000000000000)}");
    }

    static long SimulateRocks(string jetPattern, long totalRocks)
    {
        var chamber = new HashSet<(int x, int y)>();
        int highestRock = -1;
        int jetIndex = 0;
        RockType[] rockOrder = {
            RockType.Horizontal,
            RockType.Plus,
            RockType.Corner,
            RockType.Vertical,
            RockType.Square
        };
        int rockIndex = 0;

        var stateCache = new Dictionary<string, (long rocks, long height)>();
        long heightAddedByCycle = 0;

        for (long rocksDropped = 0; rocksDropped < totalRocks; rocksDropped++)
        {
            var rockType = rockOrder[rockIndex];
            var rock = GetRockShape(rockType, highestRock + 4);
            rockIndex = (rockIndex + 1) % rockOrder.Length;

            while (true)
            {
                // Jet push
                char jet = jetPattern[jetIndex];
                jetIndex = (jetIndex + 1) % jetPattern.Length;
                var (dx, dy) = jet == '<' ? (-1, 0) : (1, 0);

                if (CanMove(rock, chamber, dx, dy))
                {
                    rock = MoveRock(rock, dx, dy);
                }

                // Fall down
                if (CanMove(rock, chamber, 0, -1))
                {
                    rock = MoveRock(rock, 0, -1);
                }
                else
                {
                    foreach (var point in rock)
                    {
                        chamber.Add(point);
                        if (point.y > highestRock) highestRock = point.y;
                    }
                    break;
                }
            }

            // Cycle detection
            if (heightAddedByCycle == 0 && rocksDropped > 0)
            {
                string state = $"{rockIndex},{jetIndex}";
                for (int y = highestRock; y >= Math.Max(0, highestRock - 20); y--)
                {
                    for (int x = 0; x < 7; x++)
                        state += chamber.Contains((x, y)) ? '#' : '.';
                }

                if (stateCache.TryGetValue(state, out var cached))
                {
                    long cycleLength = rocksDropped - cached.rocks;
                    long cycleHeight = highestRock - cached.height;
                    long cyclesRemaining = (totalRocks - rocksDropped) / cycleLength;

                    heightAddedByCycle = cyclesRemaining * cycleHeight;
                    rocksDropped += cyclesRemaining * cycleLength;
                }
                else
                {
                    stateCache[state] = (rocksDropped, highestRock);
                }
            }
        }

        return highestRock + 1 + heightAddedByCycle;
    }

    static HashSet<(int x, int y)> GetRockShape(RockType type, int bottomY)
    {
        return type switch
        {
            RockType.Horizontal => new HashSet<(int, int)> { (2, bottomY), (3, bottomY), (4, bottomY), (5, bottomY) },
            RockType.Plus => new HashSet<(int, int)> {
                (3, bottomY + 2),
                (2, bottomY + 1), (3, bottomY + 1), (4, bottomY + 1),
                (3, bottomY) },
            RockType.Corner => new HashSet<(int, int)> {
                (4, bottomY + 2),
                (4, bottomY + 1),
                (2, bottomY), (3, bottomY), (4, bottomY) },
            RockType.Vertical => new HashSet<(int, int)> { (2, bottomY + 3), (2, bottomY + 2), (2, bottomY + 1), (2, bottomY) },
            RockType.Square => new HashSet<(int, int)> { (2, bottomY + 1), (3, bottomY + 1), (2, bottomY), (3, bottomY) },
            _ => throw new ArgumentException("Invalid rock type")
        };
    }

    static bool CanMove(HashSet<(int x, int y)> rock, HashSet<(int x, int y)> chamber, int dx, int dy)
    {
        foreach (var (x, y) in rock)
        {
            int newX = x + dx;
            int newY = y + dy;
            if (newX < 0 || newX >= 7 || newY < 0 || chamber.Contains((newX, newY)))
                return false;
        }
        return true;
    }

    static HashSet<(int x, int y)> MoveRock(HashSet<(int x, int y)> rock, int dx, int dy)
    {
        var newRock = new HashSet<(int x, int y)>();
        foreach (var (x, y) in rock)
            newRock.Add((x + dx, y + dy));
        return newRock;
    }
}

enum RockType { Horizontal, Plus, Corner, Vertical, Square }