using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // 1) Branje vhoda (mapa day23.txt)
        string[] lines;
        try
        {
            string path;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                path = args[0];
            }
            else
            {
                string current = Directory.GetCurrentDirectory();
                string threeUp = Directory.GetParent(
                                    Directory.GetParent(
                                        Directory.GetParent(current)!.FullName
                                    )!.FullName
                                 )!.FullName;
                path = Path.Combine(threeUp, "day23.txt");
            }

            lines = File.ReadAllLines(path);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Napaka pri branju datoteke: {ex.Message}");
            return;
        }

        // 2) Zapišemo viline kot koordinate v HashSet
        var elves = new HashSet<(int x, int y)>();
        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[y].Length; x++)
            {
                if (lines[y][x] == '#')
                    elves.Add((x, y));
            }
        }

        // Definicija 8 sosedov
        var allDirs = new (int dx, int dy)[]
        {
            (-1,-1),(0,-1),(1,-1), // zgoraj levo, gor, zgoraj desno
            (-1, 0),        (1, 0), // levo, desno
            (-1, 1),(0, 1),(1, 1)   // spodaj levo, dol, spodaj desno
        };

        // Definicija smeri za preverjanje
        var checks = new List<((int dx, int dy) move, (int dx, int dy)[] required)>
        {
            ((0,-1), new[]{(-1,-1),(0,-1),(1,-1)}), // North
            ((0, 1), new[]{(-1, 1),(0, 1),(1, 1)}), // South
            ((-1,0), new[]{(-1,-1),(-1,0),(-1,1)}), // West
            (( 1,0), new[]{( 1,-1),( 1,0),( 1,1)})  // East
        };

        int round = 0;
        int part1 = 0;
        while (true)
        {
            round++;
            var proposals = new Dictionary<(int x, int y), (int px, int py)>(); // cilj -> kdo
            var counts = new Dictionary<(int x, int y), int>(); // št. kandidatov za cilj

            foreach (var elf in elves)
            {
                // 1. če je sam (noben sosed), ostane
                bool alone = true;
                foreach (var d in allDirs)
                {
                    if (elves.Contains((elf.x + d.dx, elf.y + d.dy)))
                    {
                        alone = false;
                        break;
                    }
                }
                if (alone)
                {
                    // predlog = ostane
                    continue;
                }

                // 2. drugače preveri 4 smeri v trenutnem vrstnem redu
                for (int i = 0; i < 4; i++)
                {
                    var (move, req) = checks[(i + (round - 1)) % 4];
                    bool free = true;
                    foreach (var d in req)
                    {
                        if (elves.Contains((elf.x + d.dx, elf.y + d.dy)))
                        {
                            free = false;
                            break;
                        }
                    }
                    if (free)
                    {
                        var dest = (elf.x + move.dx, elf.y + move.dy);
                        proposals[elf] = dest;
                        if (!counts.ContainsKey(dest)) counts[dest] = 0;
                        counts[dest]++;
                        break;
                    }
                }
            }

            // Če ni predlogov → konec (Part 2)
            if (proposals.Count == 0)
            {
                Console.WriteLine($"Part 2: {round}");
                break;
            }

            // Izvedemo poteze
            var newElves = new HashSet<(int x, int y)>(elves);
            bool moved = false;
            foreach (var kv in proposals)
            {
                var elf = kv.Key;
                var dest = kv.Value;
                if (counts[dest] == 1)
                {
                    newElves.Remove(elf);
                    newElves.Add(dest);
                    moved = true;
                }
            }
            elves = newElves;

            // Po 10 rundah izračunamo Part 1
            if (round == 10)
            {
                int minX = elves.Min(e => e.x);
                int maxX = elves.Max(e => e.x);
                int minY = elves.Min(e => e.y);
                int maxY = elves.Max(e => e.y);
                int area = (maxX - minX + 1) * (maxY - minY + 1);
                part1 = area - elves.Count;
                Console.WriteLine($"Part 1: {part1}");
            }
        }
    }
}
