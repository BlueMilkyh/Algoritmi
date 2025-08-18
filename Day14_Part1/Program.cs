using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;

        string filePath = Path.Combine(threeUp, "day14.txt");

        var input = File.ReadAllText(filePath);

        var lines = input
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray();

        // 2) Parsiranje kamnin in meja
        int maxY;
        var rocks = ParseRocks(lines, out maxY);

        // 3) 1. del: do trenutka padca v brezno
        int part1 = SimulatePart1(rocks, maxY, sourceX: 500, sourceY: 0);

        // 4) 2. del: z "tlom" na maxY + 2, do blokade izvora
        int part2 = SimulatePart2(rocks, maxY, sourceX: 500, sourceY: 0);

        Console.WriteLine($"Part 1: {part1}");
        Console.WriteLine($"Part 2: {part2}");
    }

    /// <summary>
    /// Parsira poti segmentov "x1,y1 -> x2,y2 -> ..." v množico zasedenih točk (kamnin).
    /// Vrne tudi največji y (maxY) kamnin, ki definira brezno (1. del) oz. višino tal (2. del).
    /// </summary>
    static HashSet<(int x, int y)> ParseRocks(string[] lines, out int maxY)
    {
        var rocks = new HashSet<(int x, int y)>();
        maxY = int.MinValue;

        foreach (var line in lines)
        {
            var parts = line.Split("->", StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim())
                            .ToArray();

            var points = parts.Select(p =>
            {
                var xy = p.Split(',', StringSplitOptions.RemoveEmptyEntries);
                int x = int.Parse(xy[0]);
                int y = int.Parse(xy[1]);
                return (x, y);
            }).ToArray();

            for (int i = 0; i + 1 < points.Length; i++)
            {
                var (x1, y1) = points[i];
                var (x2, y2) = points[i + 1];

                if (x1 == x2)
                {
                    // navpični segment
                    int from = Math.Min(y1, y2);
                    int to = Math.Max(y1, y2);
                    for (int y = from; y <= to; y++) rocks.Add((x1, y));
                }
                else if (y1 == y2)
                {
                    // vodoravni segment
                    int from = Math.Min(x1, x2);
                    int to = Math.Max(x1, x2);
                    for (int x = from; x <= to; x++) rocks.Add((x, y1));
                }
                else
                {
                    throw new InvalidOperationException("Vhod vsebuje nediagonalne in neortogonalne segmente, kar ni podprto.");
                }
            }
        }

        if (rocks.Count > 0) maxY = rocks.Max(p => p.y);
        if (maxY == int.MinValue) maxY = 0;

        return rocks;
    }

    /// <summary>
    /// 1. del: sipanje peska dokler zrno ne pade za maxY (v brezno). Vrne število mirovanih zrn.
    /// </summary>
    static int SimulatePart1(HashSet<(int x, int y)> rocks, int maxY, int sourceX, int sourceY)
    {
        var occupied = new HashSet<(int x, int y)>(rocks);
        int rested = 0;

        while (true)
        {
            int x = sourceX, y = sourceY;

            while (true)
            {
                // Če zrna zdrsnejo čez maxY, pade v brezno -> konec
                if (y > maxY)
                    return rested;

                // poskusi dol
                if (!occupied.Contains((x, y + 1)))
                {
                    y += 1;
                    continue;
                }
                // poskusi dol-levo
                if (!occupied.Contains((x - 1, y + 1)))
                {
                    x -= 1; y += 1;
                    continue;
                }
                // poskusi dol-desno
                if (!occupied.Contains((x + 1, y + 1)))
                {
                    x += 1; y += 1;
                    continue;
                }

                // ne more se premakniti -> se umiri
                occupied.Add((x, y));
                rested++;
                break;
            }
        }
    }

    /// <summary>
    /// 2. del: "tla" na višini floorY = maxY + 2. Simuliramo, dokler ni izvor (source) blokiran.
    /// </summary>
    static int SimulatePart2(HashSet<(int x, int y)> rocks, int maxY, int sourceX, int sourceY)
    {
        var occupied = new HashSet<(int x, int y)>(rocks);
        int floorY = maxY + 2;
        int rested = 0;

        // dokler izvor ni blokiran
        while (!occupied.Contains((sourceX, sourceY)))
        {
            int x = sourceX, y = sourceY;

            while (true)
            {
                // "Tla" so na floorY, zato se ne smemo pomakniti na y == floorY
                // Če bi y+1 == floorY, spodnja celica je blokirana.
                // poskusi dol
                if (y + 1 < floorY && !occupied.Contains((x, y + 1)))
                {
                    y += 1;
                    continue;
                }
                // poskusi dol-levo
                if (y + 1 < floorY && !occupied.Contains((x - 1, y + 1)))
                {
                    x -= 1; y += 1;
                    continue;
                }
                // poskusi dol-desno
                if (y + 1 < floorY && !occupied.Contains((x + 1, y + 1)))
                {
                    x += 1; y += 1;
                    continue;
                }

                // ne more se premakniti -> se umiri
                occupied.Add((x, y));
                rested++;
                break;
            }
        }

        return rested;
    }
}
