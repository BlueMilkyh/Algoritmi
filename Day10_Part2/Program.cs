using System;
using System.Collections.Generic;
using System.IO;

class Day10Part2
{
    static void Main()
    {
        string current = Directory.GetCurrentDirectory();
        string threeUp = Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(current).FullName
                            ).FullName
                        ).FullName;

        string filePath = Path.Combine(threeUp, "day10.txt");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found at: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        int X = 1;
        int cycle = 0;
        List<char> crtPixels = new List<char>();

        void RunCycle()
        {
            cycle++;
            int pos = (cycle - 1) % 40;
            crtPixels.Add(Math.Abs(pos - X) <= 1 ? '#' : '.');
        }

        foreach (var line in lines)
        {
            if (line == "noop")
            {
                RunCycle();
            }
            else if (line.StartsWith("addx"))
            {
                int value = int.Parse(line.Split(' ')[1]);
                RunCycle();
                RunCycle();
                X += value;
            }
        }

        Console.WriteLine("Part 2:");
        for (int i = 0; i < crtPixels.Count; i++)
        {
            Console.Write(crtPixels[i]);
            if ((i + 1) % 40 == 0)
                Console.WriteLine();
        }
    }
}
