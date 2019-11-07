using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Interp
{
    class Startup
    {
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Enter source file path");
                return;
            }

            string src = "";

            try
            {
                src = File.ReadAllText(args[0]);
            }
            catch (Exception) { Console.WriteLine($"Couldn't file load: {src}"); }

            var res = Lexer.MakeProg(src, out var ops_from_src);

            if(!res)
            {
                return;
            }

            Cpu cpu = new Cpu(ops_from_src);
            List<string> prints = new List<string>();
            cpu.print_char_func = Console.WriteLine;

            if (args.Any((s) => s == "-d"))
            {
                cpu.debug_output = true;
                cpu.print_char_func = (str) =>
                {
                    prints.Add(str);
                };
            }

            Console.BufferWidth = 120;
            Console.BufferHeight = 80;

            cpu.debug_after_step_callback += () =>
            {
                PrintCpu(cpu, prints);
                while (Console.ReadKey().Key != ConsoleKey.W)
                {
                    if (Console.ReadKey().Key == ConsoleKey.R)
                    {
                        PrintCpu(cpu, prints);
                    }
                }

            };

            cpu.Run();
        }

        static void PrintCpu(Cpu cpu, List<string> prints)
        {
            Console.Clear();
            Console.CursorLeft = 0;
            Console.CursorTop = 0;

            Console.WriteLine($"IP:{cpu.ip}\tR0: {cpu.registers[0]}\tR1: {cpu.registers[1]}");

            for (int i = 0; i < cpu.prog.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;

                if (cpu.ip == i)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("->");
                }

                Console.CursorLeft = 2;
                Console.Write($"{cpu.prog[i].type} ");
                Console.ForegroundColor = ConsoleColor.White;

                OpType prev_op_type = cpu.prog[i].type;
                i++;
                if (i < cpu.prog.Length)
                switch (prev_op_type)
                {
                    case OpType.ADD:
                        PrintValue(cpu.prog[i]);
                        break;
                    case OpType.SUB:
                        PrintValue(cpu.prog[i]);
                        break;
                    case OpType.JMP:
                        PrintValue(cpu.prog[i]);
                        break;
                    case OpType.MOV:
                        PrintValue(cpu.prog[i]);
                        i++;
                        PrintValue(cpu.prog[i]);
                        break;
                    case OpType.JE:
                        PrintValue(cpu.prog[i]);
                        break;
                    case OpType.JL:
                        PrintValue(cpu.prog[i]);
                        break;
                    case OpType.JG:
                        PrintValue(cpu.prog[i]);
                        break;
                    case OpType.JNE:
                        PrintValue(cpu.prog[i]);
                        break;
                    default:
                        i--;
                        break;
                }

                Console.WriteLine();
            }

            for (int i = 0; i < cpu.stack.Length; i++)
            {
                Console.CursorLeft = 16;
                Console.CursorTop = i + 1;

                if (i == cpu.sp)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.Write($"|{cpu.stack[i]:X8}|");
            }

            int ram_p = 0;
            int cell = 0;
            int row = 0;

            int left_offset = 32;

            while (ram_p < cpu.ram.Length)
            {

                Console.CursorTop = row + 1;
                Console.CursorLeft = left_offset + (cell * 9);

                Console.Write($"{cpu.ram[ram_p]:X8} ");

                ram_p++;
                cell = ram_p % 8;
                if (cell == 0) { row++; }
            }

            row += 2;

            for (int i = 0; i < prints.Count; i++)
            {
                Console.CursorLeft = left_offset;
                Console.CursorTop = row;
                Console.Write(prints[i]);
                row++;
            }

        }

        static void PrintValue(OP op)
        {
            switch (op.type)
            {
                case OpType.NUMBER:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{op.value:x} ");
                    break;
                case OpType.REGISTER:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"r{op.value} ");
                    break;
                case OpType.MEMORY:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"${op.value:x} ");
                    break;
                default:
                    break;
            }
        }
    }

}
