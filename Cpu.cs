using System;
using System.Collections.Generic;
using System.Text;

namespace Interp
{

    public class OP
    {
        public OpType type;
        public int value;
    }

    [Flags]
    public enum OpType : int
    {
        ADD      = 0x0000,
        SUB      = 0x0001,
        INC      = 0x0002,
        DEC      = 0x0004,
        JMP      = 0x0008,
        MOV      = 0x0010,
        PRT      = 0x0020,
        NUMBER   = 0x0040,
        REGISTER = 0x0080,
        MEMORY   = 0x0100,
        EOP      = 0x0200,
        JE       = 0x0400,
        JL       = 0x0800,
        JG       = 0x1000,
        JNE      = 0x2000,
        PSH      = 0x4000,
        POP      = 0x8000,
    }

    public class Cpu
    {
        public Cpu(OP[] program, int ram_size = 128)
        {
            prog = program;
            ram = new int[ram_size];
            for (int i = 0; i < ram_size; i++)
            {
                ram[i] = 0xcccc;
            }
        }

        public OP[] prog;
        bool running;
        public int ip;
        public int[] registers = new int[2];
        public int[] ram;
        public int[] stack = new int[16];
        public int sp = -1;

        public bool debug_output;
        public Action<string> print_char_func;
        public Action debug_after_step_callback;

        public OP cop { get => prog[ip]; }

        int GetArgValue(int arg_index)
        {
            OP arg = prog[arg_index];

            switch (arg.type)
            {
                case OpType.NUMBER:
                    return arg.value;
                case OpType.REGISTER:
                    return registers[arg.value];
                case OpType.MEMORY:
                    return ram[arg.value];
                default:
                    return 0;
            }
        }

        void Set(OP arg, int value)
        {
            switch (arg.type)
            {
                case OpType.REGISTER:
                    registers[arg.value] = value;
                    break;
                case OpType.MEMORY:
                    ram[arg.value] = value;
                    break;
            }
        }

        int Get(OP arg)
        {
            switch (arg.type)
            {
                case OpType.NUMBER:
                    return arg.value;
                case OpType.REGISTER:
                    return registers[arg.value];
                case OpType.MEMORY:
                    return ram[arg.value];
                default:
                    return 0;
            }
        }

        public void Run()
        {
            running = true;

            while (running)
            {
                if (debug_output)
                {
                    debug_after_step_callback?.Invoke();
                }

                NextStep();
            }
        }

        void NextStep()
        {
            switch (cop.type)
            {
                case OpType.INC:
                    registers[0]++;
                    ip++;
                    break;
                case OpType.DEC:
                    registers[0]--;
                    ip++;
                    break;
                case OpType.ADD:
                    ip++;
                    registers[0] += Get(cop);
                    ip++;
                    break;
                case OpType.SUB:
                    ip++;
                    registers[0] -= Get(cop);
                    ip++;
                    break;
                case OpType.JMP:
                    ip++;
                    ip = Get(cop);
                    break;
                case OpType.MOV:
                    ip++;
                    OP arg0 = cop;
                    ip++;
                    OP arg1 = cop;
                    Set(arg1, Get(arg0));
                    ip++;
                    break;
                case OpType.PRT:
                    print_char_func($"{registers[0]}");
                    ip++;
                    break;
                case OpType.EOP:
                    running = false;
                    break;
                case OpType.JE:
                    if (registers[0] == 0)
                    {
                        ip++;
                        ip = Get(cop);
                    }
                    else
                    {
                        ip += 2;
                    }
                    break;
                case OpType.JNE:
                    if (registers[0] != 0)
                    {
                        ip++;
                        ip = Get(cop);
                    }
                    else
                    {
                        ip += 2;
                    }
                    break;
                case OpType.JL:
                    if (registers[0] < 0)
                    {
                        ip++;
                        ip = Get(cop);
                    }
                    else
                    {
                        ip += 2;
                    }
                    break;
                case OpType.JG:
                    if (registers[0] > 0)
                    {
                        ip++;
                        ip = Get(cop);
                    }
                    else
                    {
                        ip += 2;
                    }
                    break;
                case OpType.PSH:
                    if(sp < 15)
                    {
                        sp++;
                        stack[sp] = registers[0];
                        ip++;
                    } else {
                        throw new Exception("Stack size excided");
                    }
                    break;
                case OpType.POP:
                    if(sp >= 0)
                    {
                        registers[0] = stack[sp];
                        sp--;
                        ip++;
                    } else
                    {
                        throw new Exception("Stack is empty");
                    }
                    break;
                default:
                    ip++;
                    break;
            }


        }
    }
}
