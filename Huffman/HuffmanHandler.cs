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

        public HuffmanHandler(Stream input, Stream output = null)
        {
            if (output == null)
            {
                output = Console.OpenStandardOutput();
            }

            this._input = input;
            this._output = output;
        }

        public void Encode()
        {
            BinaryReader reader = new BinaryReader(this._input);
            BinaryWriter writer = new BinaryWriter(this._output);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                char[] buffer = new char[Huffman.BLOCK_LENGTH];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = reader.ReadChar();
                    if (reader.BaseStream.Position == reader.BaseStream.Length)
                    {
                        break;
                    }
                }

                string inputString = new string(buffer);

                KeyValuePair<Dictionary<char, BitArray>, BitArray> encodedChunk = Huffman.Encode(inputString);

                var lookupTable = encodedChunk.Key;
                var encodedData = encodedChunk.Value;

                // TODO: zapsat kodovaci tabulku na vystup

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

                byte[] encodedDateBytes = IOHelper.BitsToBytes(encodedData);

                writer.Write(encodedData.Length);
                writer.Write(encodedDateBytes);

                writer.Flush();
            }

            reader.Close();
            writer.Close();
        }

        public void Decode()
        {
            BinaryReader reader = new BinaryReader(this._input);
            StreamWriter writer = new StreamWriter(this._output);

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
                        BitArray encodedDataByteBits = new BitArray(data.ToArray());

                        decodedString += Huffman.Decode(encodedDataByteBits, lookupTable);

                        readingLookupTable = true;
                        dataByteLength = 0;
                        lookupTable.Clear();
                        data.Clear();
                    }
                }
            }

            writer.Write(decodedString.Trim(Huffman.EMPTY_CHAR));

            reader.Close();
            writer.Close();
        }
    }
}