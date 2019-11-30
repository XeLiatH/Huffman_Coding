using System;
using System.Collections;
using System.Collections.Generic;

namespace Huffman
{
    public class Printer
    {
        public static void PrintLookupTable(Dictionary<char, BitArray> lookupTable)
        {
            Console.WriteLine();
            Console.WriteLine("Kódovací tabulka");
            PrintLineSeparator();
            foreach (KeyValuePair<char, BitArray> lookupTableItem in lookupTable)
            {
                Console.WriteLine(" * \"{0}\": \"{1}\"", lookupTableItem.Key, BitsToString(lookupTableItem.Value));
            }
            PrintLineSeparator();
            Console.WriteLine();
        }

        public static void PrintEncodedData(byte[] bytes)
        {
            BitArray bytesAsBits = new BitArray(bytes);

            Console.WriteLine();
            Console.WriteLine("Zakódovaná data");
            PrintLineSeparator();
            Console.WriteLine(BitsToString(bytesAsBits));
            PrintLineSeparator();
            Console.WriteLine();
        }

        public static string BitsToString(BitArray binary)
        {
            string binaryString = string.Empty;
            foreach (bool bit in binary)
            {
                binaryString += bit ? '1' : '0';
            }

            return binaryString;
        }

        private static void PrintLineSeparator()
        {
            Console.WriteLine("=======");
        }
    }
}