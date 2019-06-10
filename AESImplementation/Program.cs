using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AESImplementation
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initializing AES with key
           AesCipher aes = new AesCipher(new byte[]{ 0x2b,0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c });

            //I used this test to test different layers during development
            //Test(aes);

            //This is my test vector from https://nvlpubs.nist.gov/nistpubs/fips/nist.fips.197.pdf, Appendix B
            byte[] input = new byte[]
                {0x32, 0x43, 0xf6, 0xa8, 0x88, 0x5a, 0x30, 0x8d, 0x31, 0x31, 0x98, 0xa2, 0xe0, 0x37, 0x07, 0x34};

            //This is the method that tests the input
            EncryptTest(aes, input);
        }

        private static void EncryptTest(AesCipher aes, byte[] input)
        {

            Console.WriteLine("Encrypting:");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("input: ");
            foreach (var b in input)
            {
                Console.Write(b.ToString("X2") + " ");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            aes.Encrypt(input);
            stopwatch.Stop();
            ;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("output: ");
            foreach (var b in input)
            {
                Console.Write(b.ToString("X2") + " ");
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Time it took {stopwatch.ElapsedMilliseconds}ms or {stopwatch.ElapsedTicks} ticks");
            Console.ResetColor();
            
        }

        private static void Test(AesCipher aes)
        {
            //Testing MixColumns
            byte[] bytes = new byte[] {219, 19, 83, 69, 242, 10, 34, 92, 1, 1, 1, 1, 198, 198, 198, 198};
            aes.MixColumn(bytes);
            foreach (var b in bytes)
            {
                Console.WriteLine(b);
            }

            //Key expansion test

            Console.WriteLine("Keys test");
            foreach (var aesKey in aes.Keys)
            {
                Console.WriteLine();
                foreach (var b in aesKey.GetBytes())
                {
                    Console.Write(b.ToString("X") + " ");
                }
            }
        }
    }
}
