using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Day16Part2
{
    static Dictionary<string, int> flow = new();
    static Dictionary<string, List<string>> tunnels = new();
    static List<string> usefulValves = new();
    static Dictionary<(string, string), int> dist = new();
    static Dictionary<(string, int, int, bool), int> memo = new();

    static void Main()
    {
        var input = LoadInput();
        BuildGraph(input);
        PrecomputeDistances();

        var result = DFS("AA", 26, 0, true);
        Console.WriteLine("Part 2: " + result);
    }

    static List<string> LoadInput()
    {
        string current = Directory.GetCurrentDirectory();
        string inputPath = Path.Combine(
            Directory.GetParent(Directory.GetParent(Directory.GetParent(current).FullName).FullName).FullName,
            "day16.txt"
        );

        return File.ReadAllLines(inputPath).ToList();
    }

    static void BuildGraph(List<string> lines)
    {
        foreach (var line in lines)
        {
            var parts = line.Split(new[] { "Valve ", " has flow rate=", "; tunnel", "s lead to valve", "s lead to valves ", "s lead to valve " }, StringSplitOptions.RemoveEmptyEntries);
            var name = parts[0];
            var rate = int.Parse(parts[1]);
            var leadsTo = parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            flow[name] = rate;
            tunnels[name] = leadsTo;
        }

        usefulValves = flow.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).ToList();
    }

    static void PrecomputeDistances()
    {
        foreach (var from in tunnels.Keys)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<(string, int)>();
            queue.Enqueue((from, 0));
            visited.Add(from);

            while (queue.Any())
            {
                var (curr, d) = queue.Dequeue();
                dist[(from, curr)] = d;

                foreach (var next in tunnels[curr])
                {
                    if (!visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue((next, d + 1));
                    }
                }
            }
        }
    }

    static int DFS(string valve, int time, int openedBitmask, bool elephant)
    {
        if (memo.TryGetValue((valve, time, openedBitmask, elephant), out var cached))
            return cached;

        int max = 0;

        for (int i = 0; i < usefulValves.Count; i++)
        {
            if ((openedBitmask & (1 << i)) != 0) continue;

            string next = usefulValves[i];
            int cost = dist[(valve, next)] + 1;

            if (time - cost <= 0) continue;

            int pressure = flow[next] * (time - cost);
            int total = DFS(next, time - cost, openedBitmask | (1 << i), elephant) + pressure;

            if (total > max) max = total;
        }

        // Allow elephant to start only once (same as a second person starting)
        if (elephant)
        {
            int result = DFS("AA", 26, openedBitmask, false);
            if (result > max) max = result;
        }

        return memo[(valve, time, openedBitmask, elephant)] = max;
    }
}
