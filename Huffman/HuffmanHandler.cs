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
            BinaryWriter writer = new BinaryWriter(this._outputFile);

            char[] buffer = new char[Huffman.BLOCK_LENGTH];
            while (sr.Read(buffer, 0, Huffman.BLOCK_LENGTH) > 0)
            {
                string input = new string(buffer);

                KeyValuePair<Dictionary<char, BitArray>, BitArray> result = Huffman.Encode(input);

                foreach (KeyValuePair<char, BitArray> lookupTableItem in result.Key)
                {
                    var letter = lookupTableItem.Key;
                    var code = lookupTableItem.Value;

                    // align code to bytes
                    int codeByteCnt = code.Length / 8 + (code.Length % 8 == 0 ? 0 : 1);

                    writer.Write(letter);
                    writer.Write(code.Length);

                    byte[] codeBytes = new byte[codeByteCnt];
                    code.CopyTo(codeBytes, 0);
                    writer.Write(codeBytes);

                    // writer.Write(code.Length);
                    // foreach (bool bit in code)
                    // {
                    //     writer.Write(bit);
                    // }
                }

                writer.Write(Huffman.SEPARATOR);

                int dateByteCnt = result.Value.Length / 8 + (result.Value.Length % 8 == 0 ? 0 : 1);
                // Console.WriteLine(byteLength);

                writer.Write(result.Value.Length);
                // foreach (bool bit in result.Value)
                // {
                //     writer.Write(bit);
                // }

                byte[] dateBytes = new byte[dateByteCnt];
                result.Value.CopyTo(dateBytes, 0);

                writer.Write(dateBytes);

                writer.Flush();
            }

            sr.Close();
            writer.Close();
        }

        public void Decode()
        {
            BinaryReader reader = new BinaryReader(this._inputFile);

            Dictionary<char, BitArray> lookupTable = new Dictionary<char, BitArray>();
            List<byte> data = new List<byte>();

            string decoded = string.Empty;

            int dataByteCnt = 0;
            bool readingLookupTable = true;
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                if (readingLookupTable)
                {
                    char letter = reader.ReadChar();

                    if (letter == Huffman.SEPARATOR)
                    {
                        readingLookupTable = false;
                        int dataBitLength = reader.ReadInt32();

                        dataByteCnt = dataBitLength / 8 + (dataBitLength % 8 == 0 ? 0 : 1);

                        continue;
                    }

                    int len = reader.ReadInt32();

                    // reconstruct code byte length
                    int codeByteCnt = len / 8 + (len % 8 == 0 ? 0 : 1);

                    byte[] items = new byte[codeByteCnt];
                    for (int i = 0; i < codeByteCnt; i++)
                    {
                        items[i] = reader.ReadByte();
                    }

                    BitArray codeByteBits = new BitArray(items);
                    bool[] codeBits = new bool[len];
                    for (int i = 0; i < len; i++)
                    {
                        codeBits[i] = codeByteBits[i];
                    }

                    lookupTable.Add(letter, new BitArray(codeBits));
                }
                else
                {
                    data.Add(reader.ReadByte());

                    if (dataByteCnt == data.Count)
                    {
                        string dec = Huffman.Decode(lookupTable, new BitArray(data.ToArray()));
                        Console.WriteLine(dec);
                        decoded += dec;
                        readingLookupTable = true;
                        dataByteCnt = 0;
                        lookupTable.Clear();
                        data.Clear();
                    }
                }
            }

            reader.Close();

            Console.WriteLine(decoded);
        }
    }
}