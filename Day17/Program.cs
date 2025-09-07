using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    // ------------------------------------------------------------
    // 0) Konstante in podatkovne strukture
    // ------------------------------------------------------------

    // Komora je široka 7 polj. Vsaka "vrstica" komore je byte z uporabljenimi biti 6..0:
    //   bit 6 = skrajno levo polje, bit 0 = skrajno desno polje.
    //
    // Vsaka skala (oblika) je podana kot polje byte-ov od spodaj navzgor.
    // Za preglednost oblike pišemo kot ASCII nize dolžine 7 ('.' = prazno, '#' = polno),
    // nato jih s funkcijo Row() pretvorimo v bitne maske.
    //
    // Pomembno: naloga zahteva, da nova skala začne 2 polji od levega roba.
    // To dosežemo tako, da vzorce vnaprej narišemo z dvema točkama na levi ("..").
    //
    // Primer: vodoravna črta "####" z 2-levim odmikom -> "..####." (skupaj 7 znakov)

    static byte Row(string pattern)
    {
        if (pattern.Length != 7)
            throw new ArgumentException("Vzorec vrstice mora biti dolg natanko 7 znakov ('.' ali '#').");

        // Levi znak (pattern[0]) predstavlja levi rob komore (bit 6).
        // Gremo z leve proti desni in gradimo 7-bitno masko.
        byte mask = 0;
        for (int i = 0; i < 7; i++)
        {
            mask <<= 1;                  // prostor za naslednji bit
            if (pattern[i] == '#') mask |= 1;
            else if (pattern[i] != '.') throw new ArgumentException("Dovoljena znaka sta '.' in '#'.");
        }
        return mask;
    }

    // Pet standardnih AoC oblik, zapisane OD SPODAJ NAVZGOR.
    // (Tako jih kasneje neposredno "zlijemo" v komoro na koordinati y.)
    static readonly byte[][] SHAPES = new byte[][]
    {
        // 1) Vodoravna črta: ####
        new byte[]
        {
            Row("..####.")  // spodnja in edina vrstica
        },

        // 2) Plus:
        //   .#.
        //   ###
        //   .#.
        new byte[]
        {
            Row("...#..."),
            Row("..###.."),
            Row("...#...")
        },

        // 3) Obrnjeni L:
        //   ..#
        //   ..#
        //   ###
        new byte[]
        {
            Row("..###.."),
            Row("....#.."),
            Row("....#..")
        },

        // 4) Navpična črta:
        //   #
        //   #
        //   #
        //   #
        new byte[]
        {
            Row("..#...."),
            Row("..#...."),
            Row("..#...."),
            Row("..#....")
        },

        // 5) Kvadrat:
        //   ##
        //   ##
        new byte[]
        {
            Row("..##..."),
            Row("..##...")
        }
    };

    // Ključ stanja za zaznavanje ciklov:
    // - indeks curka (jet)
    // - indeks oblike (shape)
    // - normaliziran "profil vrha" komore (višinski podpis 7 stolpcev)
    readonly record struct StateKey(int JetIndex, int ShapeIndex, sbyte[] Profile)
    {
        public override int GetHashCode()
        {
            unchecked
            {
                int h = JetIndex * 31 + ShapeIndex;
                for (int i = 0; i < Profile.Length; i++)
                    h = h * 31 + Profile[i];
                return h;
            }
        }
        public bool Equals(StateKey other)
        {
            if (JetIndex != other.JetIndex || ShapeIndex != other.ShapeIndex) return false;
            if (Profile.Length != other.Profile.Length) return false;
            for (int i = 0; i < Profile.Length; i++)
                if (Profile[i] != other.Profile[i]) return false;
            return true;
        }
    }

    // ------------------------------------------------------------
    // 1) Vstopna točka
    // ------------------------------------------------------------
    static void Main(string[] args)
    {
        // --- Branje vzorca curkov ('<' ali '>') ---
        string jets;
        try
        {
            string path;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                path = args[0]; // argument ukazne vrstice ima prednost
            }
            else
            {
                // vaš vzorec "threeUp": datoteka day17.txt tri mape nad CWD
                string current = Directory.GetCurrentDirectory();
                string threeUp =
                    Directory.GetParent(
                        Directory.GetParent(
                            Directory.GetParent(current)!.FullName
                        )!.FullName
                    )!.FullName;

                path = Path.Combine(threeUp, "day17.txt");
            }

            jets = File.ReadAllText(path).Trim();
            if (jets.Length == 0)
                throw new InvalidOperationException("Vhodna datoteka je prazna ali brez veljavnih znakov '<' in '>'.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Napaka pri branju vhodne datoteke: {ex.Message}");
            return;
        }

        // --- Izračun obeh delov ---
        long part1 = Simulate(jets, 2022);                         // neposredna simulacija
        long part2 = SimulateWithCycles(jets, 1_000_000_000_000);  // simulacija + preskok ciklov

        Console.WriteLine($"Part 1: {part1}");
        Console.WriteLine($"Part 2: {part2}");
    }

    // ------------------------------------------------------------
    // 2) Simulacija za manjše število skal (brez ciklov)
    // ------------------------------------------------------------
    static long Simulate(string jets, long rocksToDrop)
    {
        var chamber = new List<byte>(); // dno = indeks 0
        int jetIdx = 0;

        for (long r = 0; r < rocksToDrop; r++)
        {
            int shapeIdx = (int)(r % SHAPES.Length);
            DropOne(chamber, SHAPES[shapeIdx], jets, ref jetIdx);
        }

        return chamber.Count; // višina je število zapolnjenih vrstic
    }

    // ------------------------------------------------------------
    // 3) Simulacija z zaznavanjem ciklov (za 10^12 skal)
    // ------------------------------------------------------------
    static long SimulateWithCycles(string jets, long rocksToDrop)
    {
        var chamber = new List<byte>();
        int jetIdx = 0;

        // Pomnimo: za dano (jetIdx, shapeIdx, profil) smo že videli višino in katero skalo (r) smo spustili.
        var seen = new Dictionary<StateKey, (long rockNum, long height)>();

        long r = 0;                    // koliko skal je (ali bo) padlo
        long addedHeightFromCycles = 0; // višina, dodana zaradi preskoka ciklov

        while (r < rocksToDrop)
        {
            int shapeIdx = (int)(r % SHAPES.Length);

            // Poskus zaznave cikla: potrebujemo stabilen podpis vrha
            var key = new StateKey(jetIdx, shapeIdx, TopProfile(chamber));
            if (seen.TryGetValue(key, out var prev))
            {
                long cycleLenRocks = r - prev.rockNum;
                long cycleHeight = chamber.Count - prev.height;

                if (cycleLenRocks > 0)
                {
                    long remainingRocks = rocksToDrop - r;
                    long cycles = remainingRocks / cycleLenRocks;
                    if (cycles > 0)
                    {
                        // Preskok: "odigramo" cele cikle brez podrobne simulacije
                        r += cycles * cycleLenRocks;
                        addedHeightFromCycles += cycles * cycleHeight;
                    }
                }
            }
            else
            {
                seen[key] = (r, chamber.Count);
            }

            if (r >= rocksToDrop) break;

            DropOne(chamber, SHAPES[shapeIdx], jets, ref jetIdx);
            r++;
        }

        return chamber.Count + addedHeightFromCycles;
    }

    // ------------------------------------------------------------
    // 4) Fizika spusta ene skale
    // ------------------------------------------------------------
    static void DropOne(List<byte> chamber, byte[] shape, string jets, ref int jetIdx)
    {
        // Začetna vertikalna lega: dno skale 3 vrstice nad trenutnim vrhom
        int y = chamber.Count + 3;
        int h = shape.Length;

        // Poskrbimo, da komora vsebuje dovolj praznih vrstic
        while (chamber.Count < y + h) chamber.Add(0);

        // Delovna kopija skale (oblike ne spreminjamo globalno)
        var rock = (byte[])shape.Clone();

        while (true)
        {
            // 1) Potisk s curkom (levo/desno) – če je možno
            char jet = jets[jetIdx];
            jetIdx = (jetIdx + 1) % jets.Length;

            if (jet == '<')
            {
                if (CanShift(rock, chamber, y, -1))
                    Shift(rock, -1);
            }
            else // jet == '>'
            {
                if (CanShift(rock, chamber, y, +1))
                    Shift(rock, +1);
            }

            // 2) Gravitacija – poskus padca za 1
            if (CanFall(rock, chamber, y))
            {
                y--;
            }
            else
            {
                // Trk: "zlijemo" skalo v komoro (bitni OR z obstoječo vsebino)
                for (int i = 0; i < h; i++)
                    chamber[y + i] = (byte)(chamber[y + i] | rock[i]);

                // Odrežemo morebitne vrhnje prazne vrstice (estetsko; ni nujno)
                while (chamber.Count > 0 && chamber[^1] == 0)
                    chamber.RemoveAt(chamber.Count - 1);

                return;
            }
        }
    }

    // Ali lahko skalo premaknemo vodoravno za dir (-1 levo, +1 desno)?
    static bool CanShift(byte[] rock, List<byte> chamber, int y, int dir)
    {
        for (int i = 0; i < rock.Length; i++)
        {
            byte row = rock[i];

            if (dir < 0) // levo
            {
                // Če je že na levem robu (bit 6), premik levo ni mogoč
                if ((row & 0b_0100_0000) != 0) return false;

                byte shifted = (byte)(row << 1);
                if (y + i < chamber.Count && (shifted & chamber[y + i]) != 0) return false;
            }
            else // desno
            {
                // Če je že na desnem robu (bit 0), premik desno ni mogoč
                if ((row & 0b_0000_0001) != 0) return false;

                byte shifted = (byte)(row >> 1);
                if (y + i < chamber.Count && (shifted & chamber[y + i]) != 0) return false;
            }
        }
        return true;
    }

    static void Shift(byte[] rock, int dir)
    {
        for (int i = 0; i < rock.Length; i++)
            rock[i] = (dir < 0) ? (byte)(rock[i] << 1) : (byte)(rock[i] >> 1);
    }

    // Ali lahko skala pade navzdol za 1 (brez trka)?
    static bool CanFall(byte[] rock, List<byte> chamber, int y)
    {
        if (y == 0) return false; // dosegli dno komore

        for (int i = 0; i < rock.Length; i++)
        {
            byte row = rock[i];
            byte below = (y - 1 + i < chamber.Count) ? chamber[y - 1 + i] : (byte)0;

            // Če imajo skala in "below" skupne 1-bite -> trk
            if ((row & below) != 0) return false;
        }
        return true;
    }

    // Profil vrha komore:
    // Za vsak od 7 stolpcev poiščemo višino najvišjega polnega polja (ali -1, če stolpec prazen),
    // nato profil normaliziramo kot (maxH - height[c]) -> podpis je translacijsko neodvisen.
    static sbyte[] TopProfile(List<byte> chamber)
    {
        int top = chamber.Count;
        var heights = new int[7];

        for (int c = 0; c < 7; c++)
        {
            int h = -1;
            for (int y = top - 1; y >= 0; y--)
            {
                // Preverimo bit za stolpec c: c=0 levo -> bit (6 - c)
                if ((chamber[y] & (1 << (6 - c))) != 0)
                {
                    h = y + 1; // "višina" stolpca nad dnom (štejemo vrstice)
                    break;
                }
            }
            heights[c] = h;
        }

        int maxH = heights.Max();
        var prof = new sbyte[7];
        for (int c = 0; c < 7; c++)
            prof[c] = (sbyte)(maxH - heights[c]);

        return prof;
    }
}
