using System;
using System.Collections.Generic;
using System.IO;

class Day10Part1
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
        int sumSignalStrengths = 0;
        int[] interestingCycles = { 20, 60, 100, 140, 180, 220 };

        void RunCycle()
        {
            cycle++;
            if (Array.Exists(interestingCycles, c => c == cycle))
            {
                sumSignalStrengths += cycle * X;
            }
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

        Console.WriteLine($"Part 1: {sumSignalStrengths}");
    }
}
