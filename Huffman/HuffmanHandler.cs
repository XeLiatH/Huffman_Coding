using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Huffman
{
    class HuffmanHandler
    {
        private Stream _input;
        private Stream _output;

        public HuffmanHandler(Stream inputFile, Stream outputFile = null)
        {
            if (outputFile == null)
            {
                outputFile = Console.OpenStandardOutput();
            }

            this._input = inputFile;
            this._output = outputFile;
        }

        public void Encode()
        {
            StreamReader reader = new StreamReader(this._input);
            BinaryWriter writer = new BinaryWriter(this._output);

            char[] buffer = new char[Huffman.BLOCK_LENGTH];
            while (reader.Read(buffer, 0, Huffman.BLOCK_LENGTH) > 0)
            {
                string inputString = new string(buffer);

                KeyValuePair<Dictionary<char, BitArray>, BitArray> encodedChunk = Huffman.Encode(inputString);

                var lookupTable = encodedChunk.Key;
                var data = encodedChunk.Value;

                foreach (KeyValuePair<char, BitArray> lookupTableItem in lookupTable)
                {
                    var symbol = lookupTableItem.Key;
                    var code = lookupTableItem.Value;

                    byte[] codeBytes = IOHelper.BitsToBytes(code);

                    writer.Write(symbol);
                    writer.Write(code.Length);
                    writer.Write(codeBytes);
                }

                writer.Write(Huffman.SEPARATOR);

                byte[] dateBytes = IOHelper.BitsToBytes(data);

                writer.Write(data.Length);
                writer.Write(dateBytes);

                writer.Flush();
            }

            reader.Close();
            writer.Close();
        }

        public string Decode()
        {
            BinaryReader reader = new BinaryReader(this._input);

            Dictionary<char, BitArray> lookupTable = new Dictionary<char, BitArray>();
            List<byte> data = new List<byte>();

            string decodedString = string.Empty;

            int dataByteLength = 0;
            bool readingLookupTable = true;
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                if (readingLookupTable)
                {
                    char symbol = reader.ReadChar();

                    if (symbol == Huffman.SEPARATOR)
                    {
                        int dataBitLength = reader.ReadInt32();

                        dataByteLength = IOHelper.ByteLengthFromBitLength(dataBitLength);
                        readingLookupTable = false;

                        continue;
                    }

                    int encodedSymbolBitLength = reader.ReadInt32();
                    int encodedSymbolByteLength = IOHelper.ByteLengthFromBitLength(encodedSymbolBitLength);

                    byte[] encodedSymbolParts = new byte[encodedSymbolByteLength];
                    for (int i = 0; i < encodedSymbolByteLength; i++)
                    {
                        encodedSymbolParts[i] = reader.ReadByte();
                    }

                    BitArray encodedSymbolByteBits = new BitArray(encodedSymbolParts);
                    BitArray encodedSymbolBits = new BitArray(encodedSymbolBitLength);
                    for (int i = 0; i < encodedSymbolBits.Length; i++)
                    {
                        encodedSymbolBits[i] = encodedSymbolByteBits[i];
                    }

                    lookupTable.Add(symbol, encodedSymbolBits);
                }
                else
                {
                    data.Add(reader.ReadByte());

                    if (dataByteLength == data.Count)
                    {
                        decodedString += Huffman.Decode(new BitArray(data.ToArray()), lookupTable);

                        readingLookupTable = true;
                        dataByteLength = 0;
                        lookupTable.Clear();
                        data.Clear();
                    }
                }
            }

            reader.Close();

            return decodedString.Trim(Huffman.EMPTY_CHAR);
        }
    }
}