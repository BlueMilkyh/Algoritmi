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

        var instructions = File.ReadAllLines(filePath);
        var cpu = new CPU();
        cpu.ExecuteProgram(instructions);

        Console.WriteLine($"Part 1: {cpu.SignalStrengthSum}");
        Console.WriteLine("Part 2:");
        cpu.RenderScreen();

    }

    class CPU
    {
        private int _x = 1;
        private int _cycle = 0;
        private readonly List<int> _signalStrengths = new List<int>();
        private readonly char[,] _crt = new char[6, 40];

        public int SignalStrengthSum => _signalStrengths.Sum();

        public void ExecuteProgram(string[] instructions)
        {
            foreach (var instruction in instructions)
            {
                var parts = instruction.Split(' ');
                switch (parts[0])
                {
                    case "noop":
                        Tick();
                        break;
                    case "addx":
                        Tick();
                        Tick();
                        _x += int.Parse(parts[1]);
                        break;
                }
            }
        }

        private void Tick()
        {
            _cycle++;

            // Part 1: Record signal strength at specific cycles
            if ((_cycle - 20) % 40 == 0 && _cycle <= 220)
            {
                _signalStrengths.Add(_cycle * _x);
            }

            // Part 2: Draw pixel
            var row = (_cycle - 1) / 40;
            var col = (_cycle - 1) % 40;

            if (Math.Abs(col - _x) <= 1)
            {
                _crt[row, col] = '#';
            }
            else
            {
                _crt[row, col] = '.';
            }
        }

        public void RenderScreen()
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 40; col++)
                {
                    Console.Write(_crt[row, col]);
                }
                Console.WriteLine();
            }
        }
    }
}
