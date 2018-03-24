using System;

namespace pe8_keygen
{
    class Program
    {
        static void Main(string[] args)
        {
            Keygen keygen = new Keygen();
            keygen.CreateKey();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
