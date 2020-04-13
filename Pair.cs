using System;
using System.Collections.Generic;
using System.Text;

namespace Crypto.Bot
{
    public class Pair
    {
        public string pair;
        public override int GetHashCode()
        {
            char[] arr = pair.ToCharArray();
            int sum = 0;
            Array.ForEach(arr, x => sum += (int)x);
            return sum;
        }
        public override bool Equals(object other)
        {
            return this.pair == ((Pair)other).pair;
        }
        public Pair()
        {

        }
        public Pair(string p)
        {
            pair = p;
        }
    }
}
