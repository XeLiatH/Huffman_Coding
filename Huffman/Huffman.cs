using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Huffman
{
    public static class Huffman
    {
        public const int BLOCK_LENGTH = 4096;
        public const char EMPTY_CHAR = '\0';
        public const char SEPARATOR = '\\';

        public static KeyValuePair<Dictionary<char, BitArray>, BitArray> Encode(string stringInput)
        {
            Console.WriteLine(stringInput);

            Dictionary<char, int> frequencies = new Dictionary<char, int>();
            foreach (char symbol in stringInput)
            {
                if (!frequencies.ContainsKey(symbol))
                {
                    frequencies[symbol] = 0;
                }

                frequencies[symbol]++;
            }

            List<Node> frequencyNodes = new List<Node>();
            foreach (KeyValuePair<char, int> freq in frequencies.OrderBy(p => p.Value))
            {
                frequencyNodes.Add(new Node(freq.Key, freq.Value, null, null));
            }

            while (frequencyNodes.Count > 1)
            {
                frequencyNodes = frequencyNodes.OrderBy(n => n.Frequency).ToList();

                Node left = frequencyNodes[0];
                frequencyNodes.RemoveAt(0);

                Node right = frequencyNodes[0];
                frequencyNodes.RemoveAt(0);

                Node parent = new Node(EMPTY_CHAR, left.Frequency + right.Frequency, left, right);
                frequencyNodes.Add(parent);
            }

            Node root = frequencyNodes.First();

            Dictionary<char, BitArray> lookupTable = CreateLookupTable(root);

            List<bool> outputBits = new List<bool>();
            foreach (char symbol in stringInput)
            {
                BitArray encodedSymbol = lookupTable[symbol];
                for (int i = 0; i < encodedSymbol.Length; i++)
                {
                    outputBits.Add(encodedSymbol[i]);
                }
            }

            return new KeyValuePair<Dictionary<char, BitArray>, BitArray>(lookupTable, new BitArray(outputBits.ToArray()));
        }

        private static Dictionary<char, BitArray> CreateLookupTable(Node root)
        {
            Dictionary<char, BitArray> lookupTable = new Dictionary<char, BitArray>();
            BuildCode(root, new List<bool>(), ref lookupTable);

            return lookupTable;
        }

        private static void BuildCode(Node node, List<bool> bits, ref Dictionary<char, BitArray> lookupTable)
        {
            if (node.IsLeaf())
            {
                lookupTable.Add(node.Character, new BitArray(bits.ToArray()));
                return;
            }

            List<bool> leftBits = new List<bool>(bits);
            List<bool> rightBits = new List<bool>(bits);

            leftBits.Add(false);
            BuildCode(node.Left, leftBits, ref lookupTable);

            rightBits.Add(true);
            BuildCode(node.Right, rightBits, ref lookupTable);
        }

        public static string Decode(BitArray encodedData, Dictionary<char, BitArray> lookupTable)
        {
            string decodedString = string.Empty;
            Dictionary<string, char> reverseLookupTable = CreateReverseLookupTable(lookupTable);

            List<bool> buffer = new List<bool>();
            foreach (bool bit in encodedData)
            {
                buffer.Add(bit);

                string codeKey = BitsToString(new BitArray(buffer.ToArray()));
                if (reverseLookupTable.ContainsKey(codeKey))
                {
                    decodedString += reverseLookupTable[codeKey];
                    buffer.Clear();
                }
            }

            return decodedString;
        }

        private static Dictionary<string, char> CreateReverseLookupTable(Dictionary<char, BitArray> lookupTable)
        {
            Dictionary<string, char> reverseLookupTable = new Dictionary<string, char>();
            foreach (KeyValuePair<char, BitArray> lookupTableItem in lookupTable)
            {
                reverseLookupTable.Add(BitsToString(lookupTableItem.Value), lookupTableItem.Key);
            }

            return reverseLookupTable;
        }

        private static string BitsToString(BitArray binary)
        {
            string binaryString = string.Empty;
            foreach (bool bit in binary)
            {
                binaryString += bit ? '1' : '0';
            }

            return binaryString;
        }
    }
}
