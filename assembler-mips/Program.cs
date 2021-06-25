using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static assembler_mips.BaseConverter;

namespace assembler_mips
{
    public class Line
    {
        public string LineContent;
        public int ProgramCounter;
        public bool LineHasLabel;

        public Line(string cleanLine, int pc, bool hasLabel = false)
        {
            LineContent = cleanLine;
            ProgramCounter = pc;
            LineHasLabel = hasLabel;
        }
    }

    public class BaseConverter
    {
        // Dicionário com todos os caracteres da base 36 (e, consequentemente, de todas as anteriores)
        static char[] dictionary = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        // Verifica se um caractere é válido para a base de entrada
        static int CharValid(char c, int basein)
        {
            for (int i = 0; i < basein; i++)
                if (c == dictionary[i])
                    return i;
            return -1;
        }

        // Verifica se uma base é valida
        static bool BaseValid(int b)
        {
            if (b > 1 && b < dictionary.Length + 1)
                return true;
            else
                return false;
        }

        // Função potência com valores inteiros
        static int IntPow(int nBase, int nExpo)
        {
            if (nExpo == 0) return 1; if (nExpo == 1) return nBase;
            if (nBase == 0) return 0; if (nBase == 1) return 1;

            int y = nBase;
            for (int i = 2; i <= nExpo; i++)
            {
                y *= nBase;
            }
            return y;
        }

        // Função de conversão
        public static string Converter(int basein, string input, int baseout)
        {
            // Definições
            if (!BaseValid(basein))
                return null;
            if (!BaseValid(baseout))
                return null;
            string output = "", inputFormatted = input.ToUpper();
            long dec = 0;
            int expo = inputFormatted.Length - 1, valueNum;

            // Da base de entrada pra decimal
            foreach (char c in inputFormatted)
            {
                valueNum = CharValid(c, basein);
                if (valueNum < 0)
                    return null;
                else
                    dec += valueNum * IntPow(basein, expo);
                expo--;
            }

            // De decimal pra base de saída
            do
            {
                if (dec % baseout == 0)
                    output = "0" + output;
                else
                    output = dictionary[dec % baseout] + output;
                dec /= baseout;
            } while (dec != 0);

            return output;
        }
    }

    class Program
    {
        public static string ComplementoDeDois(string bin)
        {
            string c1 = "";

            for (int i = bin.Length - 1; i >= 0; i--)
                c1 = bin[i] == '0' ? "1" + c1 : "0" + c1;

            int c1Dec = Convert.ToInt32(Converter(2, c1, 10)) + 1;
            string output = Converter(10, c1Dec.ToString(), 2);
            return output;
        }
        static void Main(string[] args)
        {
            /// DEFINIÇÕES GERAIS
            string directory = @"C:\assembler\";

            try
            {
                if (Directory.Exists(directory)) { }
                else { DirectoryInfo newDir = Directory.CreateDirectory(directory); }

            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }

            /// Dicionário de labels <address, label>
            Dictionary<int, string> labelList = new Dictionary<int, string>();

            /// Arrays de registradores
            /// O índice de cada item é o número de seu registrador
            string[] registerList = new string[32] { "$zero", "$at", "$v0", "$v1", "$a0", "$a1", "$a2", "$a3", "$t0", "$t1", "$t2", "$t3", "$t4", "$t5", "$t6", "$t7", "$s0", "$s1", "$s2", "$s3", "$s4", "$s5", "$s6", "$s7", "$t8", "$t9", "$k0", "$k1", "$gp", "$sp", "$fp", "$ra" };

            /// Arquivo de entrada
            string[] asmContent;
            try
            {
                asmContent = File.ReadAllLines(directory + "test_assembler.asm");
            }
            catch (Exception e)
            {
                Console.WriteLine("A pasta C:\\assembler\\ não contém o arquivo .asm definido para ser lido.\nConfira os detalhes do erro:\n\n{0}", e.ToString());
                Console.ReadKey();
                return;
            }

            /// Program Counter
            int programCounter = 0, maxProgramCounter;

            /// Lista de linhas do arquivo
            List<Line> lines = new List<Line>();

            /// Lista de instruções binárias pro arquivo .mif
            List<string> binaryInstructions = new List<string>();

            /// Leitura das linhas e labels e atribuição de endereços
            foreach (string line in asmContent)
            {
                string cleanLine;
                string label;

                /// Apagando comentários
                int indexHash = line.IndexOf('#');
                if (indexHash == -1) cleanLine = line;
                else cleanLine = line.Substring(0, indexHash);

                if (cleanLine.Length > 0)
                {
                    /// Associando programCounter às linhas e labels
                    int indexTwoDots = cleanLine.IndexOf(':');
                    if (indexTwoDots > -1)
                    {
                        label = cleanLine.Substring(0, indexTwoDots);
                        labelList.Add(programCounter, label);
                    }
                    lines.Add(new Line(cleanLine, programCounter));

                    programCounter += 4;
                }
            }
            maxProgramCounter = programCounter - 4;




            // AJEITAR DAQUI PRA CIMA




            /// Processamento das instruções
            for (programCounter = 0; programCounter <= maxProgramCounter; programCounter += 4)
            {
                foreach (Line line in lines)
                {
                    if (line.ProgramCounter == programCounter)
                    {
                        Regex rxR_Type = new Regex("add |sub |and |or |xor |nor |slt |sll |srl |mul |mult |div |jr");
                        bool R_Type = rxR_Type.IsMatch(line.LineContent);

                        Regex rxI_Type = new Regex("beq |bne |addi |andi |ori |xori |slti |lui |lw |sw ");
                        bool I_Type = rxI_Type.IsMatch(line.LineContent);

                        Regex rxJ_Type = new Regex("j |jal ");
                        bool J_Type = rxJ_Type.IsMatch(line.LineContent);

                        string[] splittedLine = line.LineContent.Split(' ', '	', ',', '(', ')');
                        splittedLine = splittedLine.Where(w => w.Length > 0).ToArray();

                        int functPosition = splittedLine[0].IndexOf(':') > 0 ? 1 : 0;
                        string binaryLine = "";

                        if (R_Type)
                        {
                            // imprimindo entrada
                            foreach (string s in splittedLine)
                                Console.Write("{0} ", s);
                            Console.WriteLine();

                            string opcode = "000000", rs = "00000", rt = "00000", rd = "00000", shamt = "00000", funct = "000000";

                            if (!(new Regex("sll|srl|div|mult|jr").IsMatch(splittedLine[functPosition])))
                            {
                                int rdInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                int rsInt = Array.IndexOf(registerList, splittedLine[functPosition + 2]);
                                int rtInt = Array.IndexOf(registerList, splittedLine[functPosition + 3]);
                                rd = Converter(10, rdInt.ToString(), 2).PadLeft(5, '0');
                                rs = Converter(10, rsInt.ToString(), 2).PadLeft(5, '0');
                                rt = Converter(10, rtInt.ToString(), 2).PadLeft(5, '0');

                                switch (splittedLine[functPosition])
                                {
                                    case "add":
                                        funct = "100000";
                                        break;
                                    case "sub":
                                        funct = "100010";
                                        break;
                                    case "and":
                                        funct = "100100";
                                        break;
                                    case "or":
                                        funct = "100101";
                                        break;
                                    case "xor":
                                        funct = "100110";
                                        break;
                                    case "nor":
                                        funct = "100111";
                                        break;
                                    case "slt":
                                        funct = "101010";
                                        break;
                                    case "mul":
                                        funct = "000010";
                                        opcode = "011100";
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (new Regex("sll|srl").IsMatch(splittedLine[functPosition]))
                            {
                                // tratar esse Exit
                                if (Convert.ToInt32(splittedLine[functPosition + 3]) > 31) Environment.Exit(1);
                                shamt = splittedLine[functPosition + 3];
                                shamt = Converter(10, shamt, 2).PadLeft(5, '0');

                                int rdInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                int rtInt = Array.IndexOf(registerList, splittedLine[functPosition + 2]);
                                rd = Converter(10, rdInt.ToString(), 2).PadLeft(5, '0');
                                rt = Converter(10, rtInt.ToString(), 2).PadLeft(5, '0');

                                if (splittedLine[functPosition] == "srl") funct = "000010";
                            }
                            else if (new Regex("div|mult").IsMatch(splittedLine[functPosition]))
                            {
                                int rsInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                int rtInt = Array.IndexOf(registerList, splittedLine[functPosition + 2]);
                                rs = Converter(10, rsInt.ToString(), 2).PadLeft(5, '0');
                                rt = Converter(10, rtInt.ToString(), 2).PadLeft(5, '0');
                                funct = splittedLine[functPosition] == "div" ? "011010" : "011000";
                            }
                            else
                            { // jr
                                int rsInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                rs = Converter(10, rsInt.ToString(), 2).PadLeft(5, '0');
                                funct = "001000";
                            }

                            binaryLine = string.Format("{0} {1} {2} {3} {4} {5}",
                                opcode, rs, rt, rd, shamt, funct);
                        }

                        else if (I_Type)
                        {
                            string opcode = "000000", rs = "00000", rt = "00000", immediate = "0000000000000000";

                            if (new Regex("addi|andi|ori|xori|slti").IsMatch(splittedLine[functPosition]))
                            {
                                int rtInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                int rsInt = Array.IndexOf(registerList, splittedLine[functPosition + 2]);
                                rs = Converter(10, rsInt.ToString(), 2).PadLeft(5, '0');
                                rt = Converter(10, rtInt.ToString(), 2).PadLeft(5, '0');

                                string immDec = splittedLine[functPosition + 3];
                                if (immDec.Substring(0, 1) != "-")
                                    immediate = Converter(10, immDec, 2).PadLeft(16, '0');
                                else
                                {
                                    string immNoSignal = Converter(10, immDec[1..], 2).PadLeft(16, '0');
                                    immediate = ComplementoDeDois(immNoSignal);
                                }

                                switch (splittedLine[functPosition])
                                {
                                    case "addi":
                                        opcode = "001000";
                                        break;
                                    case "andi":
                                        opcode = "001100";
                                        break;
                                    case "ori":
                                        opcode = "001101";
                                        break;
                                    case "xori":
                                        opcode = "001110";
                                        break;
                                    case "slti":
                                        opcode = "001010";
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (splittedLine[functPosition] == "lui")
                            {
                                opcode = "001111";

                                int rtInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                rt = Converter(10, rtInt.ToString(), 2).PadLeft(5, '0');

                                string immDec = splittedLine[functPosition + 2];
                                if (immDec.Substring(0, 1) != "-")
                                    immediate = Converter(10, immDec, 2).PadLeft(16, '0');
                                else
                                {
                                    string immNoSignal = Converter(10, immDec[1..], 2).PadLeft(16, '0');
                                    immediate = ComplementoDeDois(immNoSignal);
                                }
                            }
                            else if (splittedLine[functPosition] == "lw" || splittedLine[functPosition] == "sw")
                            {
                                opcode = splittedLine[functPosition] == "lw" ? "100011" : "101011";

                                int rtInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                int baseInt = Array.IndexOf(registerList, splittedLine[functPosition + 3]);
                                rs = Converter(10, baseInt.ToString(), 2).PadLeft(5, '0');
                                rt = Converter(10, rtInt.ToString(), 2).PadLeft(5, '0');

                                string offsetDec = splittedLine[functPosition + 2];
                                if (offsetDec.Substring(0, 1) != "-")
                                    immediate = Converter(10, offsetDec, 2).PadLeft(16, '0');
                                else
                                {
                                    string offsetNoSignal = Converter(10, offsetDec[1..], 2).PadLeft(16, '0');
                                    immediate = ComplementoDeDois(offsetNoSignal);
                                }
                            }

                            // parei aqui
                            else if (new Regex("beq|bne").IsMatch(splittedLine[functPosition]))
                            {
                                /*int rsInt = Array.IndexOf(registerList, splittedLine[functPosition + 1]);
                                int rtInt = Array.IndexOf(registerList, splittedLine[functPosition + 2]);
                                // immediate = splittedLine[functPosition + 3];

                                opcode = splittedLine[functPosition] == "beq" ? "000100" : "000101";

                                rs = Converter(10, rsInt.ToString(), 2).PadLeft(5, '0');
                                rt = Converter(10, rtInt.ToString(), 2).PadLeft(5, '0');*/
                            }

                            binaryLine = string.Format("{0} {1} {2} {3}",
                                opcode, rs, rt, immediate);
                        }

                        else if (J_Type)
                        {
                            string opcode = "000000", address = "00000000000000000000000000";

                            if (splittedLine[functPosition] == "j")
                            {
                                opcode = "000010";

                                // string label = splittedLine[functPosition + 1];

                                // ???
                            }

                            if (splittedLine[functPosition] == "jal")
                            {
                                opcode = "000011";

                                // string label = splittedLine[functPosition + 1];

                                // ???
                            }

                            //binaryLine = string.Format("{0} {1}", opcode, address);
                        }

                        else
                        {
                            // label
                        }

                        if (binaryLine.Length > 0)
                            binaryInstructions.Add(binaryLine);
                    }
                }
            }




            foreach (string i in binaryInstructions)
            {
                Console.WriteLine(i);
            }





            /// Escrita no arquivo memoria.mif
            /*
            List<string> partsOfInstructions = new List<string>();
            foreach (string i in binaryInstructions)
            {
                string mostSignificant = i.Substring(0, 8);
                string part2 = i.Substring(8, 8);
                string part3 = i.Substring(16, 8);
                string lessSignificant = i.Substring(24, 8);

                partsOfInstructions.Add(lessSignificant);
                partsOfInstructions.Add(part3);
                partsOfInstructions.Add(part2);
                partsOfInstructions.Add(mostSignificant);
            }

            List<string> header = new List<string>() { 
                "WIDTH = 8;",
                "DEPTH = 512;\n",
                "ADDRESS_RADIX = DEC;",
                "DATA_RADIX = BIN;\n",
                "CONTENT\n",
                "BEGIN" 
            };

            File.WriteAllLines(directory + "memoria.mif", header);
            //foreach (string h in header)
                //Console.WriteLine(h);

            using (StreamWriter file =
                new StreamWriter(directory + "memoria.mif", true))
            {
                int address = 0;
                foreach (string i in partsOfInstructions)
                {
                    //Console.WriteLine(String.Format("{0}: {1}", address.ToString().PadLeft(3, '0'), i));
                    file.WriteLine(String.Format("{0}: {1}", address.ToString().PadLeft(3, '0'), i));
                    address++;
                }
            }

            using (StreamWriter file =
                new StreamWriter(directory + "memoria.mif", true))
            {
                //Console.WriteLine("END;");
                file.WriteLine("END;");
            }

            Console.WriteLine("Arquivo memoria.mif criado na pasta C:\\assembler\\"); */
            Console.ReadKey();
        }
    }
}
