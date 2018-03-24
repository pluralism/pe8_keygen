using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pe8_keygen
{
    class NumberUtils
    {
        static Random random = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            return random.Next(2, 10);
        }
    }
}
