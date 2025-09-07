using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    // Preprost in berljiv zapis 3D točke
    readonly record struct P3(int X, int Y, int Z);

    static void Main(string[] args)
    {
        // ------------------------------------------------------------
        // 1) Branje vhoda (vrstice oblike "x,y,z")
        // ------------------------------------------------------------
        string[] lines;
        try
        {
            string path;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                path = args[0]; // eksplicitno podana pot ima prednost
            }
            else
            {
                // vaš vzorec "threeUp": datoteka day18.txt tri mape nad CWD
                string current = Directory.GetCurrentDirectory();
                string threeUp =
                    Directory.GetParent(
                        Directory.GetParent(
                            Directory.GetParent(current)!.FullName
                        )!.FullName
                    )!.FullName;
                path = Path.Combine(threeUp, "day18.txt");
            }

            lines = File.ReadAllLines(path)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

            if (lines.Length == 0)
                throw new InvalidOperationException("Vhodna datoteka je prazna.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Napaka pri branju vhodne datoteke: {ex.Message}");
            return;
        }

        // ------------------------------------------------------------
        // 2) Parsanje kock v množico (HashSet)
        // ------------------------------------------------------------
        var lava = new HashSet<P3>();
        foreach (var raw in lines)
        {
            var parts = raw.Trim().Split(',');
            if (parts.Length != 3)
                throw new FormatException($"Neveljavna vrstica (pričakovano 'x,y,z'): {raw}");

            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            int z = int.Parse(parts[2]);
            lava.Add(new P3(x, y, z));
        }

        // Sosednje smeri (6-sosednost)
        var DIRS = new P3[]
        {
            new(+1, 0, 0), new(-1, 0, 0),
            new(0, +1, 0), new(0, -1, 0),
            new(0, 0, +1), new(0, 0, -1)
        };

        // ------------------------------------------------------------
        // 3) 1. del – skupna površina (tudi proti notranjim zračnim žepom)
        //    Za vsako kocko štejemo ploskve, ki niso pokrite s sosednjo kocko.
        // ------------------------------------------------------------
        long part1 = 0;
        foreach (var p in lava)
        {
            foreach (var d in DIRS)
            {
                var q = new P3(p.X + d.X, p.Y + d.Y, p.Z + d.Z);
                if (!lava.Contains(q)) part1++; // ploskev je izpostavljena
            }
        }

        // ------------------------------------------------------------
        // 4) 2. del – zunanja površina
        //    Ideja: omejimo prostorsko območje na "bounding box" + 1 rob v vse smeri
        //    in izvedemo BFS po zraku, ki je dosegljiv od zunaj. Zatem štejemo le
        //    tiste ploskve, ki mejijo na ta zunanji zrak.
        // ------------------------------------------------------------
        int minX = lava.Min(p => p.X) - 1;
        int maxX = lava.Max(p => p.X) + 1;
        int minY = lava.Min(p => p.Y) - 1;
        int maxY = lava.Max(p => p.Y) + 1;
        int minZ = lava.Min(p => p.Z) - 1;
        int maxZ = lava.Max(p => p.Z) + 1;

        bool InBounds(P3 p) =>
            p.X >= minX && p.X <= maxX &&
            p.Y >= minY && p.Y <= maxY &&
            p.Z >= minZ && p.Z <= maxZ;

        // BFS od "zunanjega" zraka (začnemo v vogalu izven lave)
        var start = new P3(minX, minY, minZ);
        var qBfs = new Queue<P3>();
        var outsideAir = new HashSet<P3>();
        qBfs.Enqueue(start);
        outsideAir.Add(start);

        while (qBfs.Count > 0)
        {
            var cur = qBfs.Dequeue();
            foreach (var d in DIRS)
            {
                var nxt = new P3(cur.X + d.X, cur.Y + d.Y, cur.Z + d.Z);
                if (!InBounds(nxt)) continue;
                if (lava.Contains(nxt)) continue;        // lava ni zrak
                if (outsideAir.Contains(nxt)) continue;  // že obiskano
                outsideAir.Add(nxt);
                qBfs.Enqueue(nxt);
            }
        }

        // Zunanja površina: ploskve, ki mejijo na "outsideAir"
        long part2 = 0;
        foreach (var p in lava)
        {
            foreach (var d in DIRS)
            {
                var q = new P3(p.X + d.X, p.Y + d.Y, p.Z + d.Z);
                if (outsideAir.Contains(q)) part2++;
            }
        }

        // ------------------------------------------------------------
        // 5) Izpis rezultatov
        // ------------------------------------------------------------
        Console.WriteLine($"Part 1: {part1}");
        Console.WriteLine($"Part 2: {part2}");
    }
}
