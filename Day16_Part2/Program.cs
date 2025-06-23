using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Day16_Part2
{
    static Dictionary<string, int> flow = new();
    static Dictionary<string, List<string>> graph = new();
    static Dictionary<(string, string), int> dist = new();
    static List<string> usefulValves;
    static Dictionary<(string, int, int, bool), int> memo = new();

    static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;
        string filePath = Path.Combine(threeUp, "day16.txt");
        var input = File.ReadAllLines(filePath);
        ParseInput(input);
        PrecomputeDistances();

        int result = DFS("AA", 26, 0, true);
        Console.WriteLine("Part 2: " + result);
    }

    static void ParseInput(string[] lines)
    {
        string pattern = @"^Valve (\w+) has flow rate=(\d+); tunnels? leads? to valves? (.+)$";
        var regex = new Regex(pattern);

        foreach (var line in lines)
        {
            var match = regex.Match(line);
            if (!match.Success)
            {
                Console.WriteLine("Failed to parse line: " + line);
                continue;
            }

            string name = match.Groups[1].Value;
            int rate = int.Parse(match.Groups[2].Value);
            var connections = match.Groups[3].Value.Split(',')
                .Select(s => s.Trim())
                .ToList();

            flow[name] = rate;
            graph[name] = connections;
        }

        usefulValves = flow.Where(kvp => kvp.Value > 0)
                           .Select(kvp => kvp.Key)
                           .ToList();
    }

    static void PrecomputeDistances()
    {
        foreach (var from in graph.Keys)
        {
            var queue = new Queue<(string node, int steps)>();
            var visited = new HashSet<string>();
            queue.Enqueue((from, 0));
            visited.Add(from);

            while (queue.Count > 0)
            {
                var (curr, steps) = queue.Dequeue();
                dist[(from, curr)] = steps;

                foreach (var next in graph[curr])
                {
                    if (!visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue((next, steps + 1));
                    }
                }
            }
        }
    }

    static int DFS(string pos, int time, int opened, bool elephant)
    {
        if (memo.TryGetValue((pos, time, opened, elephant), out int result))
            return result;

        int best = 0;

        for (int i = 0; i < usefulValves.Count; i++)
        {
            if ((opened & (1 << i)) != 0)
                continue;

            string next = usefulValves[i];
            int cost = dist[(pos, next)] + 1;
            int timeLeft = time - cost;

            if (timeLeft <= 0)
                continue;

            int pressure = flow[next] * timeLeft;
            int total = DFS(next, timeLeft, opened | (1 << i), elephant) + pressure;

            if (total > best)
                best = total;
        }

        // Allow the elephant to start its own DFS if not already done.
        if (elephant)
        {
            int withElephant = DFS("AA", 26, opened, false);
            if (withElephant > best)
                best = withElephant;
        }

        memo[(pos, time, opened, elephant)] = best;
        return best;
    }
}
