using System;
using System.Collections.Generic;

namespace AESImplementation
{
    public class AesCipher
    {
      

        #region Inner use helper classes
        public class Key
        {
            public Key(byte[] keyValue)
            {
                KeyValue = new Word[4];
                int currentWord = 0;
                List<byte> currentBytes = new List<byte>();
                for (int i = 0; i < keyValue.Length; i++)
                {
                    currentBytes.Add(keyValue[i]);
                    if (currentBytes.Count == 4)
                    {
                        KeyValue[currentWord] = new Word(currentBytes.ToArray());
                        currentWord++;
                        currentBytes.Clear();
                    }
                }
            }

            public Key(Word[] keyValue)
            {
                KeyValue = keyValue;
            }

            public Word[] KeyValue { get; set; }


            public byte[] GetBytes()
            {
                List<byte> bytes=new List<byte>();
                foreach (var word in KeyValue)
                {
                    bytes.AddRange(word.ByteValue);
                }

                return bytes.ToArray();
            }

        }

        public class Word
        {
            public byte[] ByteValue { get; set; }

            public Word(byte[] byteValue)
            {
                ByteValue = byteValue;
            }

            public Word Xor(Word other)
            {
                List<byte> result = new List<byte>();
                for (var i = 0; i < ByteValue.Length; i++)
                {
                    result.Add((byte)(ByteValue[i]^other.ByteValue[i]));
                }
                return new Word(result.ToArray());
            }
        }

        public static class WordHelper
        {
            public static Word WordRotate(Word word)
            {
                Word result = new Word(new byte[4]);
                result.ByteValue[3] = word.ByteValue[0];
                result.ByteValue[2] = word.ByteValue[3];
                result.ByteValue[1] = word.ByteValue[2];
                result.ByteValue[0] = word.ByteValue[1];
                return result;
            }

            public static Word SubWord(Word word, byte[] sBox)
            {
                Word result = new Word(new byte[4]);
                result.ByteValue[0] = sBox[word.ByteValue[0]];
                result.ByteValue[1] = sBox[word.ByteValue[1]];
                result.ByteValue[2] = sBox[word.ByteValue[2]];
                result.ByteValue[3] = sBox[word.ByteValue[3]];
                return result;
            }
        }


        public class LookupMatrix
        {
            private byte[,] _lookupBytes;

            public Func<byte, byte, byte> Multiplication { get; }

            public LookupMatrix(byte[] first, byte[] second, Func<byte, byte, byte> multiplication)
            {
                Multiplication = multiplication;
                _lookupBytes = new byte[first.Length,second.Length];

                for (int i = 0; i < first.Length; i++)
                {
                    for (int j = 0; j < second.Length; j++)
                    {
                        var currentFirst = first[i];
                        var currentSecond = second[j];
                        if (currentFirst != 1 && currentSecond != 1)
                        {
                            _lookupBytes[i, j] = Multiplication(currentFirst, currentSecond);
                        }

                        if (currentFirst == 1)
                        {
                            _lookupBytes[i, j] = currentSecond;
                        }
                        if (currentSecond == 1)
                        {
                            _lookupBytes[i, j] = currentFirst;
                        }

                    }
                }
            }

            public static byte[] ByteRange(byte to)
            {
                byte from = 0;
                List<byte> byteValues= new List<byte>();
                while (from<=to)
                {

                    if (byteValues.Count == 256)
                        break;
                    byteValues.Add(from);
                    from++;
                }
                return byteValues.ToArray();
            }

            public byte FastMult(byte first, byte second)
            {
                if (_lookupBytes == null)
                    throw new Exception("not initialized");

                
                // i * j = j * i
                if (first < _lookupBytes.GetLength(0) && second < _lookupBytes.GetLength(1))
                {
                    return _lookupBytes[first, second];
                }

                if (first < _lookupBytes.GetLength(1) && second < _lookupBytes.GetLength(0))
                {
                    return _lookupBytes[second, first];
                }

                //value is not found
                throw new Exception($"Value pair {first},{second} not existent");
            }
        }
        #endregion


        // forward SBox
        private readonly byte[] _sBox = new byte[256] {
            //0     1    2      3     4    5     6     7      8    9     A      B    C     D     E     F
            0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76, //0
            0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0, //1
            0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15, //2
            0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75, //3
            0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84, //4
            0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf, //5
            0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8, //6
            0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2, //7
            0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73, //8
            0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb, //9
            0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79, //A
            0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08, //B
            0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a, //C
            0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e, //D
            0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf, //E
            0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16 }; //F

        #region Rounds
        private readonly int[,] _nRounds = {{10, 12, 14}, {12, 12, 14}, {14, 14, 14}}; 
        private int GetRounds(int numberOfBlock, int numberOfKey)
        {
            return _nRounds[(numberOfBlock - 4) / 2, (numberOfKey - 4) / 2];
        }
        #endregion



        private readonly int _rounds;

        public Key[] Keys { get; }

        private static readonly LookupMatrix _multMatrix;

        public AesCipher(byte[] key)
        {
            _rounds = GetRounds(4, 4);
            Keys = new Key[_rounds+1];
            Keys[0] = new Key(key);
            GenerateKeyExpansions();

        }


        static AesCipher()
        {
            _multMatrix = new LookupMatrix(LookupMatrix.ByteRange(byte.MaxValue), LookupMatrix.ByteRange(3), GMul);
        }


        #region KeyExpansion Generation
        private void GenerateKeyExpansions()
        {
            List<Word> wordsSoFar = new List<Word>();
            wordsSoFar.AddRange(Keys[0].KeyValue);

            for (int i = 4; i < 4*_rounds+4; i++)
            {
                Word lastWord = wordsSoFar[i - 1];

                if (i % 4 == 0)
                {

                    lastWord = WordHelper.SubWord(WordHelper.WordRotate(lastWord), _sBox)
                        .Xor(GetRoundCoefficientWord(i / 4 - 1));
                }

                var current = wordsSoFar[i - 4].Xor(lastWord); 

                wordsSoFar.Add(current);
            }

            var currentWords = new List<Word>();
            int currentKeyIndex = 1;

            for (int i = 4; i < wordsSoFar.Count; i++)
            {
                currentWords.Add(wordsSoFar[i]);
                if (currentWords.Count == 4)
                {
                    Keys[currentKeyIndex] = new Key(currentWords.ToArray());
                    currentWords.Clear();
                    currentKeyIndex++;
                }
            }
        }
        private byte[] _roundCoefficients = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1B, 0x36 };

        private Word GetRoundCoefficientWord(int roundCoeffIndex)
        {
            return new Word(new byte[] { _roundCoefficients[roundCoeffIndex], 0, 0, 0 });
        }
        #endregion

        #region Encryption

        public void Encrypt(byte[] data)
        {
            AddRoundKey(data, Keys[0].GetBytes());
            for (int i = 0; i < _rounds - 1; i++)
            {
                Round(data, Keys[i + 1].GetBytes());
            }

            FinalRound(data, Keys[10].GetBytes());
        }

        private void Round(byte[] state, byte[] roundKey)
        {
            ByteSub(state);
            ShiftRow(state);
            MixColumn(state);
            AddRoundKey(state, roundKey);
        }

        private void FinalRound(byte[] state, byte[] roundKey)
        {
            ByteSub(state);
            ShiftRow(state);
            AddRoundKey(state, roundKey);
        }

        #endregion


        #region Layers
        private void AddRoundKey(byte[] state, byte[] roundKey)
        {
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = (byte)(state[i] ^ roundKey[i]);
            }
        }

        public void MixColumn(byte[] state)
        {
            byte[] stateCopy = new byte[state.Length];
            Array.Clear(stateCopy,0,stateCopy.Length);
            //Matrix used for multiplication is
            // 0x02 0x03 0x01 0x01
            // 0x01 0x02 0x03 0x01
            // 0x01 0x01 0x02 0x03
            // 0x03 0x01 0x01 0x02

            for (int column = 0; column < 4; column++)
            {
                //stateCopy[MatrixMap(0,column)] = (byte)(GMul(state[MatrixMap(0,column)],0x02) ^  GMul(state[MatrixMap(1,column)],0x003) ^ state[MatrixMap(2,column)] ^ state[MatrixMap(3,column)]);
                //stateCopy[MatrixMap(1, column)] = (byte)(state[MatrixMap(0, column)] ^ GMul(state[MatrixMap(1, column)], 0x02) ^ GMul(state[MatrixMap(2, column)],0x03) ^ state[MatrixMap(3, column)]);

                //stateCopy[MatrixMap(2, column)] = (byte)(state[MatrixMap(0, column)] ^ state[MatrixMap(1, column)] ^ GMul(state[MatrixMap(2, column)], 0x02) ^ GMul(state[MatrixMap(3, column)],0x03));
                //stateCopy[MatrixMap(3, column)] = (byte)(GMul(state[MatrixMap(0, column)],0x03) ^ state[MatrixMap(1, column)] ^ state[MatrixMap(2, column)] ^ GMul(state[MatrixMap(3, column)], 0x02));


                ////Changed to use lookup table, after discussing performance issues with Doctor G. Khachatryan
                stateCopy[MatrixMap(0, column)] = (byte)(_multMatrix.FastMult(state[MatrixMap(0, column)], 0x02) ^ _multMatrix.FastMult(state[MatrixMap(1, column)], 0x003) ^ state[MatrixMap(2, column)] ^ state[MatrixMap(3, column)]);
                stateCopy[MatrixMap(1, column)] = (byte)(state[MatrixMap(0, column)] ^ _multMatrix.FastMult(state[MatrixMap(1, column)], 0x02) ^ _multMatrix.FastMult(state[MatrixMap(2, column)], 0x03) ^ state[MatrixMap(3, column)]);

                stateCopy[MatrixMap(2, column)] = (byte)(state[MatrixMap(0, column)] ^ state[MatrixMap(1, column)] ^ _multMatrix.FastMult(state[MatrixMap(2, column)], 0x02) ^ _multMatrix.FastMult(state[MatrixMap(3, column)], 0x03));
                stateCopy[MatrixMap(3, column)] = (byte)(_multMatrix.FastMult(state[MatrixMap(0, column)], 0x03) ^ state[MatrixMap(1, column)] ^ state[MatrixMap(2, column)] ^ _multMatrix.FastMult(state[MatrixMap(3, column)], 0x02));
            }

            Array.Copy(stateCopy,state,stateCopy.Length);

        }

        private void ShiftRow(byte[] state)
        {
            //shifting second row by 1 to left
            byte firstValue = state[MatrixMap(1, 0)];
            state[MatrixMap(1, 0)] = state[MatrixMap(1, 1)];
            state[MatrixMap(1, 1)] = state[MatrixMap(1, 2)];
            state[MatrixMap(1, 2)] = state[MatrixMap(1, 3)];
            state[MatrixMap(1, 3)] = firstValue;

            //shifting third row by two to left
            firstValue = state[MatrixMap(2, 0)];
            state[MatrixMap(2, 0)] = state[MatrixMap(2, 2)];
            state[MatrixMap(2, 2)] = firstValue;

            firstValue = state[MatrixMap(2, 1)];
            state[MatrixMap(2, 1)] = state[MatrixMap(2, 3)];
            state[MatrixMap(2, 3)] = firstValue;

            //Shifting the last row by 3 to left

            byte[] oldBytes = {state[MatrixMap(3, 0)],state[MatrixMap(3,1)],state[MatrixMap(3,2)],state[MatrixMap(3,3)]};
            state[MatrixMap(3, 0)] = oldBytes[3];
            state[MatrixMap(3, 1)] = oldBytes[0];
            state[MatrixMap(3, 2)] = oldBytes[1];
            state[MatrixMap(3, 3)] = oldBytes[2];


        }

        private void ByteSub(byte[] state)
        {
            for (var i = 0; i < state.Length; i++)
            {
                state[i] = _sBox[state[i]];
            }
        }
        #endregion

        #region Helpers

        private int MatrixMap(int row, int col)
        {
            return row + col * 4;
        }

        private static byte GMul(byte a, byte b)
        { // Galois Field (256) Multiplication of two Bytes
            byte p = 0;

            for (int counter = 0; counter < 8; counter++)
            {
                if ((b & 1) != 0)
                {
                    p ^= a;
                }

                bool hi_bit_set = (a & 0x80) != 0;
                a <<= 1;
                if (hi_bit_set)
                {
                    a ^= 0x1B; /* x^8 + x^4 + x^3 + x + 1 */
                }
                b >>= 1;
            }

            return p;
        }

        void PrintDebug(byte[] values, string state = "")
        {
            Console.Write(state + ": ");
            foreach (var value in values)
            {
                Console.Write(value.ToString("X"));
            }
            Console.WriteLine();
        }


        #endregion

    }
}