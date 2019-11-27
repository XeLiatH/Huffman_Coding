using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Huffman
{
    public static class Huffman
    {
        public const int BLOCK_LENGTH = 4096;
        public const char SEPARATOR = ';';

        public static KeyValuePair<Dictionary<char, BitArray>, bool[]> Encode(string input)
        {
            // TODO: návratová hodnota? vrácí kodovaci tabulku a data
            // asi vracet Slovník něco jako Dictionary<List<bool>, bool[4096]> ?

            Console.WriteLine();
            Console.Write("Vstup: ");
            Console.WriteLine("\"" + input + "\"");
            Console.WriteLine();
            Console.WriteLine();

            Dictionary<char, int> frequencies = new Dictionary<char, int>();
            foreach (char c in input)
            {
                if (!frequencies.ContainsKey(c))
                {
                    frequencies[c] = 0;
                }

                frequencies[c]++;
            }

            Console.WriteLine("Frekvence jednotlivých znaků:");
            Console.WriteLine("----------");
            foreach (KeyValuePair<char, int> freq in frequencies)
            {
                Console.WriteLine("\"" + freq.Key + "\": " + freq.Value);
            }
            Console.WriteLine("----------");
            Console.WriteLine();

            List<Node> pomocnyList = new List<Node>();

            Console.WriteLine("Seřazení znaků od nejmenší frekvence po nejvyšší:");
            Console.WriteLine("----------");
            foreach (KeyValuePair<char, int> freq in frequencies.OrderBy(p => p.Value))
            {
                Console.WriteLine("\"" + freq.Key + "\": " + freq.Value);
                pomocnyList.Add(new Node(freq.Key, freq.Value, null, null));
            }
            Console.WriteLine("----------");
            Console.WriteLine();

            // sestavení stromu
            while (pomocnyList.Count > 1)
            {
                pomocnyList = pomocnyList.OrderBy(n => n.Frequency).ToList();

                Node left = pomocnyList[0];
                pomocnyList.RemoveAt(0);

                Node right = pomocnyList[0];
                pomocnyList.RemoveAt(0);

                Node parent = new Node('\0', left.Frequency + right.Frequency, left, right);
                pomocnyList.Add(parent);
            }

            Node root = pomocnyList.First();
            Console.WriteLine(root.Character);

            Dictionary<char, BitArray> lookupTable = CreateLookupTable(root);

            Console.WriteLine("Zakódování jednotlivých znaků:");
            Console.WriteLine("----------");
            foreach (KeyValuePair<char, BitArray> pair in lookupTable)
            {
                string word = "";
                foreach (bool bit in pair.Value)
                {
                    word += (bit) ? '1' : '0';
                }

                Console.WriteLine("\"" + pair.Key + "\": " + word);
            }
            Console.WriteLine("----------");
            Console.WriteLine();

            // BinaryWriter fout = new BinaryWriter(new FileStream("test_simple.dat", FileMode.Create));

            // todo: uložit kodovaci tabulku
            // znak pocet_bitu data
            // char int BitArray

            // fout.Write(SEPARATOR);

            string output = "";

            List<bool> outputBits = new List<bool>();
            int bitCount = 0;
            foreach (char c in input)
            {
                for (int i = 0; i < lookupTable[c].Length; i++)
                {
                    bool bit = lookupTable[c][i];

                    outputBits.Add(bit);

                    output += bit ? '1' : '0';
                    // fout.Write(bit);
                    // fout.Flush();
                    bitCount++;
                }
            }

            Console.WriteLine();
            Console.Write("Výstup: ");
            Console.WriteLine("\"" + output + "\"");
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("Velikost původního textu [bit]: ");
            Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetByteCount(input) * 8);

            Console.Write("Velikost textu po zakódování [bit]: ");
            Console.WriteLine(bitCount);

            return new KeyValuePair<Dictionary<char, BitArray>, bool[]>(lookupTable, outputBits.ToArray());
        }

        private static Dictionary<char, BitArray> CreateLookupTable(Node tree)
        {
            Dictionary<char, BitArray> lookupTable = new Dictionary<char, BitArray>();
            BuildCode(tree, new List<bool>(), ref lookupTable);

            return lookupTable;
        }

        private static void BuildCode(Node node, List<bool> ba, ref Dictionary<char, BitArray> lookupTable)
        {
            if (node.IsLeaf())
            {
                lookupTable.Add(node.Character, new BitArray(ba.ToArray()));
                return;
            }

            // BitArray bitArray = new BitArray()

            List<bool> bl = new List<bool>(ba);
            List<bool> br = new List<bool>(ba);

            bl.Add(false);
            BuildCode(node.Left, bl, ref lookupTable);

            br.Add(true);
            BuildCode(node.Right, br, ref lookupTable);
        }

        public static string Decode(Dictionary<char, BitArray> lookupTable, bool[] data)
        {
            string result = "";

            List<bool> buffer = new List<bool>();
            foreach (bool bit in data)
            {
                buffer.Add(bit);
                foreach (KeyValuePair<char, BitArray> pair in lookupTable)
                {
                    bool isCorrect = false;
                    if (buffer.Count == pair.Value.Length)
                    {
                        for (int i = 0; i < pair.Value.Length; i++)
                        {
                            if (pair.Value[i] != buffer[i])
                            {
                                isCorrect = false;
                                break;
                            }

                            isCorrect = true;
                        }

                    }

                    if (isCorrect)
                    {
                        result += pair.Key;
                        buffer.Clear();
                        break;
                    }
                }
            }

            return result;
        }
    }
}
