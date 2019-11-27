using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Huffman
{
    class HuffmanHandler
    {
        private FileStream _inputFile;
        private FileStream _outputFile;
        private bool _verbose;

        public HuffmanHandler(FileStream inputFile, FileStream outputFile = null)
        {
            this._inputFile = inputFile;
            this._outputFile = outputFile;
            this._verbose = outputFile == null;
        }

        public void Encode()
        {
            StreamReader sr = new StreamReader(this._inputFile);

            // produkuje prázdný znaky na konci

            BinaryWriter fout = new BinaryWriter(this._outputFile);

            char[] buffer = new char[Huffman.BLOCK_LENGTH];
            while (sr.Read(buffer, 0, Huffman.BLOCK_LENGTH) > 0)
            {
                string input = new string(buffer);

                KeyValuePair<Dictionary<char, BitArray>, bool[]> result = Huffman.Encode(input);

                foreach (KeyValuePair<char, BitArray> pair in result.Key)
                {
                    fout.Write(pair.Key);
                    fout.Write(pair.Value.Length);
                    foreach (bool bit in pair.Value)
                    {
                        fout.Write(bit);
                    }

                    fout.Flush();
                }

                fout.Write(Huffman.SEPARATOR);
                fout.Flush();

                fout.Write(result.Value.Length);
                fout.Flush();
                foreach (bool bit in result.Value)
                {
                    fout.Write(bit);
                    fout.Flush();
                }
            }

            fout.Close();
        }

        public void Decode()
        {
            BinaryReader fin = new BinaryReader(this._inputFile);

            Dictionary<char, BitArray> lookupTable = new Dictionary<char, BitArray>();
            List<bool> data = new List<bool>();

            string decoded = "";

            int dataLen = 0;
            bool readingLookupTable = true;
            while (fin.BaseStream.Position != fin.BaseStream.Length)
            {
                if (readingLookupTable)
                {
                    char letter = fin.ReadChar();

                    if (letter == Huffman.SEPARATOR)
                    {
                        readingLookupTable = false;
                        dataLen = fin.ReadInt32();
                        continue;
                    }

                    int len = fin.ReadInt32();
                    bool[] bits = new bool[len];
                    for (int i = 0; i < len; i++)
                    {
                        bits[i] = fin.ReadBoolean();
                    }

                    lookupTable.Add(letter, new BitArray(bits));
                }
                else
                {
                    data.Add(fin.ReadBoolean());

                    if (dataLen == data.Count)
                    {
                        string dec = Huffman.Decode(lookupTable, data.ToArray());
                        // Console.WriteLine(dec);
                        decoded += dec;
                        readingLookupTable = true;
                        dataLen = 0;
                        lookupTable.Clear();
                        data.Clear();
                    }
                }
            }

            Console.WriteLine(decoded);
        }
    }
}