using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    class Valve
    {
        public string Name = "";
        public int Rate;
        public List<string> Leads = new();
    }

    static void Main(string[] args)
    {
        // ===== 0) BRANJE VHODA PREK "threeUp" ALI PODANE POTI =====
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
                path = Path.Combine(threeUp, "day16.txt"); // po želji spremenite ime
            }

            lines = File.ReadAllLines(path);
            if (lines.Length == 0)
                throw new Exception($"Datoteka je prazna: {path}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Napaka pri branju vhodne datoteke: {ex.Message}");
            return;
        }

        // ===== 1) PARSING =====
        // Primer vrstice:
        // "Valve AA has flow rate=0; tunnels lead to valves DD, II, BB"
        var re = new Regex(@"^Valve\s+([A-Z]{2})\s+has\s+flow\s+rate=(\d+);.*valves?\s+(.+)$", RegexOptions.Compiled);
        var valves = new Dictionary<string, Valve>();
        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var line = raw.Trim();
            var m = re.Match(line);
            if (!m.Success) throw new Exception($"Nepričakovana vrstica: {line}");
            var name = m.Groups[1].Value;
            var rate = int.Parse(m.Groups[2].Value);
            var leads = m.Groups[3].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
            valves[name] = new Valve { Name = name, Rate = rate, Leads = leads };
        }

        // ===== 2) GRAFI IN RAZDALJE (Floyd–Warshall) =====
        var names = valves.Keys.OrderBy(s => s).ToList();
        var idxByName = names.Select((n, i) => (n, i)).ToDictionary(t => t.n, t => t.i);
        int n = names.Count;
        const int INF = 1_000_000;
        var dist = new int[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                dist[i, j] = (i == j) ? 0 : INF;

        for (int i = 0; i < n; i++)
            foreach (var nb in valves[names[i]].Leads)
                dist[i, idxByName[nb]] = Math.Min(dist[i, idxByName[nb]], 1);

        for (int k = 0; k < n; k++)
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (dist[i, k] + dist[k, j] < dist[i, j])
                        dist[i, j] = dist[i, k] + dist[k, j];

        // ===== 3) REDUKCIJA NA KORISTNE VENTILE =====
        string startName = "AA";
        int startIdx = idxByName[startName];

        var usefulNames = valves.Values.Where(v => v.Rate > 0).Select(v => v.Name).OrderBy(s => s).ToList();
        int mUseful = usefulNames.Count;
        var usefulRates = usefulNames.Select(nm => valves[nm].Rate).ToArray();

        var distStartToUseful = new int[mUseful];
        var distUsefulToUseful = new int[mUseful, mUseful];
        for (int i = 0; i < mUseful; i++)
        {
            distStartToUseful[i] = dist[startIdx, idxByName[usefulNames[i]]];
            for (int j = 0; j < mUseful; j++)
                distUsefulToUseful[i, j] = dist[idxByName[usefulNames[i]], idxByName[usefulNames[j]]];
        }

        // ===== 4) DFS + MEMO (BREZ out PARAMETROV) =====
        (int bestOverall, Dictionary<int, int> bestPerMask) SolveSingleActor(int timeLimit)
        {
            int bestOverall = 0;
            var bestPerMask = new Dictionary<int, int>();
            var seen = new Dictionary<(int pos, int time, int mask), int>();

            void UpdateBest(int mask, int val)
            {
                if (!bestPerMask.TryGetValue(mask, out var cur) || val > cur)
                    bestPerMask[mask] = val;
            }

            void FromStart(int timeLeft, int mask, int acc)
            {
                UpdateBest(mask, acc);
                for (int next = 0; next < mUseful; next++)
                {
                    if (((mask >> next) & 1) != 0) continue;
                    int t2 = timeLeft - distStartToUseful[next] - 1;
                    if (t2 <= 0) continue;
                    int gain = usefulRates[next] * t2;
                    FromUseful(next, t2, mask | (1 << next), acc + gain);
                }
            }

            void FromUseful(int pos, int timeLeft, int mask, int acc)
            {
                UpdateBest(mask, acc);
                var key = (pos, timeLeft, mask);
                if (seen.TryGetValue(key, out var best) && best >= acc) return;
                seen[key] = acc;

                for (int next = 0; next < mUseful; next++)
                {
                    if (((mask >> next) & 1) != 0) continue;
                    int t2 = timeLeft - distUsefulToUseful[pos, next] - 1;
                    if (t2 <= 0) continue;
                    int gain = usefulRates[next] * t2;
                    FromUseful(next, t2, mask | (1 << next), acc + gain);
                }
                if (acc > bestOverall) bestOverall = acc;
            }

            FromStart(timeLimit, 0, 0);
            return (bestOverall, bestPerMask);
        }

        // ===== 5) IZRAČUN OBEH DELOV =====
        var (part1, _) = SolveSingleActor(30);
        var (_, bestPerMask26) = SolveSingleActor(26);

        // Pripravi DP tabelo velikosti 2^mUseful.
        // dp[mask] = najboljši tlak za masko ALI katerikoli njen podnabor (po transformaciji spodaj).
        int fullMask = (1 << mUseful) - 1;
        int size = fullMask + 1;

        var dp = new int[size];
        foreach (var kv in bestPerMask26)
        {
            int mask = kv.Key;
            int val = kv.Value;
            if (val > dp[mask]) dp[mask] = val;
        }

        // SOS: "superset-max transform" – za vsak bit propagiramo maksimum iz podmask
        for (int bit = 0; bit < mUseful; bit++)
        {
            for (int mask = 0; mask < size; mask++)
            {
                if ((mask & (1 << bit)) != 0)
                    dp[mask] = Math.Max(dp[mask], dp[mask ^ (1 << bit)]);
            }
        }

        // Zdaj dp[comp] že vsebuje najboljši rezultat ZA KATERIKOLI PODNABOR od "comp".
        // Tako lahko varno kombiniramo vsako masko z njenim komplementom.
        int part2 = 0;
        for (int mask = 0; mask < size; mask++)
        {
            int comp = fullMask ^ mask;
            int candidate = dp[mask] + dp[comp];
            if (candidate > part2) part2 = candidate;
        }

        Console.WriteLine($"Part 1 (30 min, en akter): {part1}");
        Console.WriteLine($"Part 2 (26 min, dva akterja): {part2}");

    }
}
