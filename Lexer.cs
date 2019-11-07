using System;
using System.Collections.Generic;
using System.Text;

namespace Interp
{

    public struct OpDef
    {
        public OpDef(string name, OpType type , OpType[] args)
        {
            this.name = name;
            this.type = type;
            this.args = args;
        }

        public string name;
        public OpType type;
        public OpType[] args;
    }

    public struct Label
    {
        public Label(string name, int memory_location)
        {
            this.name = name;
            this.memory_location = memory_location;
        }

        public string name;
        public int memory_location;
    }

    public struct ResolveName
    {
        public ResolveName(string name, int op_idx)
        {
            this.name = name;
            this.op_idx = op_idx;
        }

        public string name;
        public int op_idx;
    }

    public static class Lexer
    {

        static OpDef[] op_defs = new OpDef[] {
            new OpDef("inc", OpType.INC, null),
            new OpDef("dec", OpType.DEC, null),
            new OpDef("add", OpType.ADD, new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY }),
            new OpDef("sub", OpType.SUB, new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY }),
            new OpDef("mov", OpType.MOV, new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY, OpType.REGISTER |OpType.MEMORY }),
            new OpDef("jmp", OpType.JMP, new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY }),
            new OpDef("je", OpType.JE,   new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY }),
            new OpDef("jne", OpType.JNE, new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY }),
            new OpDef("jg", OpType.JG,   new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY }),
            new OpDef("jl", OpType.JL,   new OpType[]{ OpType.NUMBER | OpType.REGISTER | OpType.MEMORY }),
            new OpDef("prt", OpType.PRT, null),
            new OpDef("psh", OpType.PSH, null),
            new OpDef("pop", OpType.POP, null),
        };

        public static OP[] TestProj()
        {
            string src = @"
                mov 0  $0
                mov 10 $1

                .pp
                mov $1 r0
                je end
                dec
                mov r0 $1
                mov $0 r0
                add 10
                mov r0 $0
                jmp pp

                .end
                mov $0 r0
                prt
            ";

            MakeProg(src, out var ops);
            return ops;
        }

        static List<Label> labels = new List<Label>();

        static Action<string> PrintError = Console.WriteLine;

        public static bool MakeProg(string src, out OP[] ops_arr)
        {
            ops_arr = null;
            List<OP> ops = new List<OP>();
            List<ResolveName> resolve_name = new List<ResolveName>();
            labels.Clear();

            int char_ptr = 0;

            while (char_ptr < src.Length)
            {
                switch (src[char_ptr])
                {
                    case '\r':
                    case '\t':
                    case '\n':
                    case ' ':
                        char_ptr++;
                        break;
                    case '.':
                        {
                            char_ptr++;
                            int start = char_ptr;

                            while (char.IsLetter(src[char_ptr]))
                            {
                                char_ptr++;
                            }

                            string name = src.Substring(start, char_ptr - start);
                            labels.Add(new Label(name, ops.Count));
                        }
                        break;
                    case '#':
                        while(src[char_ptr] != '\n')
                        {
                            char_ptr++;
                        }
                        break;
                    case '$':
                        {
                            char_ptr++;
                            int start = char_ptr;

                            while (char.IsDigit(src[char_ptr]))
                            {
                                char_ptr++;
                            }

                            if (char_ptr < start)
                            {
                                PrintError("Bad Memory Location");
                                return false;
                            }
                            else
                            {
                                int memory_location = int.Parse(src.Substring(start, char_ptr - start));
                                ops.Add(new OP() { type = OpType.MEMORY, value = memory_location });
                            }

                        }
                        break;
                    default:

                        while (char.IsWhiteSpace(src[char_ptr]))
                        {
                            char_ptr++;
                        }

                        if (char.IsDigit(src[char_ptr]))
                        {
                            int start = char_ptr;

                            while (char.IsDigit(src[char_ptr]))
                            {
                                char_ptr++;
                            }

                            int number = int.Parse(src.Substring(start, char_ptr - start));
                            ops.Add(new OP() { type = OpType.NUMBER, value = number });

                        }
                        else
                        {

                            if (src[char_ptr] == 'r' && char.IsDigit(src[char_ptr + 1]))
                            {
                                char_ptr++;
                                int register = int.Parse($"{src[char_ptr]}");
                                ops.Add(new OP() { type = OpType.REGISTER, value = register });
                            }
                            else
                            {

                                int start = char_ptr;

                                while (char_ptr < src.Length && char.IsLetter(src[char_ptr]) )
                                {
                                    char_ptr++;
                                }

                                if (char_ptr == start)
                                {
                                    PrintError("Bad Op");
                                    return false;
                                }
                                else
                                {
                                    string op = src.Substring(start, char_ptr - start);

                                    if (TryGetOpType(op, out var type))
                                    {
                                        ops.Add(new OP() { type = type, value = 0 });
                                    } else if (IsLabel(op, out var memory_location)) {
                                        ops.Add(new OP() { type = OpType.NUMBER, value = memory_location });
                                    } else {
                                        ops.Add(new OP() { type = OpType.NUMBER, value = 0 });
                                        resolve_name.Add(new ResolveName(op, ops.Count - 1));
                                    }
                                }

                            }

                        }

                        char_ptr++;

                        break;
                }
            }

            for (int i = 0; i < resolve_name.Count; i++)
            {
                if (IsLabel(resolve_name[i].name, out var memory_location))
                {
                    ops[resolve_name[i].op_idx].value = memory_location;
                }
                else
                {
                    PrintError($"Unknow Op: {resolve_name[i].name}");
                    return false;
                }
            }

            ops.Add(new OP() { type = OpType.EOP, value = 0 });
            OP[] res = ops.ToArray();

            if (ProjIsValide(res))
            {
                ops_arr = res;
                return true;
            }

            return false;
        }

        static bool IsLabel(string name, out int memory_location)
        {
            int length = labels.Count;

            for (int i = 0; i < length; i++)
            {
                if (labels[i].name == name)
                {
                    memory_location = labels[i].memory_location;
                    return true;
                }
            }

            memory_location = 0;
            return false;
        }

        static bool TryGetOpType(string op, out OpType type)
        {
            for (int i = 0; i < op_defs.Length; i++)
            {
                if (op_defs[i].name == op)
                {
                    type = op_defs[i].type;
                    return true;
                }
            }

            type = OpType.EOP;

            return false;
        }

        static OpDef GetDefByType(OpType type)
        {
            for (int i = 0; i < op_defs.Length; i++)
            {
                if (op_defs[i].type == type)
                    return op_defs[i];
            }

            return new OpDef("", 0, null);
        }

        static bool ProjIsValide(OP[] ops)
        {

            for (int i = 0; i < ops.Length - 1; i++)
            {

                OpDef def = GetDefByType(ops[i].type);
                
                if(def.name == "")
                {
                    PrintError($"Invalide Op/Arg location (ip:{i}) {OpToString(ops[i])}");
                    return false;
                }

                if(def.args != null)
                {
                    for (int arg_idx = 0; arg_idx < def.args.Length; arg_idx++)
                    {
                        i++;
                        if (!def.args[arg_idx].HasFlag(ops[i].type))
                        {
                            PrintError($"Invalide Arg type (ip:{i}) {OpToString(ops[i])}");
                            return false;
                        }
                    }

                }

            }

            return true;
        }

        static string OpToString(OP op)
        {
            return $"(op: { op.type} | { op.value})";
        }

    }
}
