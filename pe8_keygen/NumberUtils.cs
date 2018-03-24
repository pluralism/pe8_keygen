using System;

namespace pe8_keygen
{
    class NumberUtils
    {
        static Random random = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            return random.Next(min, max + 1);
        }
    }
}
