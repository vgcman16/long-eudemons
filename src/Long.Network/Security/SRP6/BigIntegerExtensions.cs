using System.Numerics;

namespace Long.Network.Security.SRP6
{
    /// <summary>
    /// This class adds additional support for .NET BigInteger.
    /// </summary>
    public static class BigIntegerExtensions
    {
        #region Constants
        public static readonly BigInteger Two = new BigInteger(2);
        public static readonly uint[] SmallPrimes = {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
            73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151,
            157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233,
            239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317,
            331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419,
            421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503,
            509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607,
            613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701,
            709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811,
            821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911,
            919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997,

            1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087,
            1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181,
            1187, 1193, 1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279,
            1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373,
            1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471,
            1481, 1483, 1487, 1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553, 1559,
            1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637,
            1657, 1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747,
            1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811, 1823, 1831, 1847, 1861, 1867,
            1871, 1873, 1877, 1879, 1889, 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973,
            1979, 1987, 1993, 1997, 1999,

            2003, 2011, 2017, 2027, 2029, 2039, 2053, 2063, 2069, 2081, 2083, 2087, 2089,
            2099, 2111, 2113, 2129, 2131, 2137, 2141, 2143, 2153, 2161, 2179, 2203, 2207,
            2213, 2221, 2237, 2239, 2243, 2251, 2267, 2269, 2273, 2281, 2287, 2293, 2297,
            2309, 2311, 2333, 2339, 2341, 2347, 2351, 2357, 2371, 2377, 2381, 2383, 2389,
            2393, 2399, 2411, 2417, 2423, 2437, 2441, 2447, 2459, 2467, 2473, 2477, 2503,
            2521, 2531, 2539, 2543, 2549, 2551, 2557, 2579, 2591, 2593, 2609, 2617, 2621,
            2633, 2647, 2657, 2659, 2663, 2671, 2677, 2683, 2687, 2689, 2693, 2699, 2707,
            2711, 2713, 2719, 2729, 2731, 2741, 2749, 2753, 2767, 2777, 2789, 2791, 2797,
            2801, 2803, 2819, 2833, 2837, 2843, 2851, 2857, 2861, 2879, 2887, 2897, 2903,
            2909, 2917, 2927, 2939, 2953, 2957, 2963, 2969, 2971, 2999,

            3001, 3011, 3019, 3023, 3037, 3041, 3049, 3061, 3067, 3079, 3083, 3089, 3109,
            3119, 3121, 3137, 3163, 3167, 3169, 3181, 3187, 3191, 3203, 3209, 3217, 3221,
            3229, 3251, 3253, 3257, 3259, 3271, 3299, 3301, 3307, 3313, 3319, 3323, 3329,
            3331, 3343, 3347, 3359, 3361, 3371, 3373, 3389, 3391, 3407, 3413, 3433, 3449,
            3457, 3461, 3463, 3467, 3469, 3491, 3499, 3511, 3517, 3527, 3529, 3533, 3539,
            3541, 3547, 3557, 3559, 3571, 3581, 3583, 3593, 3607, 3613, 3617, 3623, 3631,
            3637, 3643, 3659, 3671, 3673, 3677, 3691, 3697, 3701, 3709, 3719, 3727, 3733,
            3739, 3761, 3767, 3769, 3779, 3793, 3797, 3803, 3821, 3823, 3833, 3847, 3851,
            3853, 3863, 3877, 3881, 3889, 3907, 3911, 3917, 3919, 3923, 3929, 3931, 3943,
            3947, 3967, 3989,

            4001, 4003, 4007, 4013, 4019, 4021, 4027, 4049, 4051, 4057, 4073, 4079, 4091,
            4093, 4099, 4111, 4127, 4129, 4133, 4139, 4153, 4157, 4159, 4177, 4201, 4211,
            4217, 4219, 4229, 4231, 4241, 4243, 4253, 4259, 4261, 4271, 4273, 4283, 4289,
            4297, 4327, 4337, 4339, 4349, 4357, 4363, 4373, 4391, 4397, 4409, 4421, 4423,
            4441, 4447, 4451, 4457, 4463, 4481, 4483, 4493, 4507, 4513, 4517, 4519, 4523,
            4547, 4549, 4561, 4567, 4583, 4591, 4597, 4603, 4621, 4637, 4639, 4643, 4649,
            4651, 4657, 4663, 4673, 4679, 4691, 4703, 4721, 4723, 4729, 4733, 4751, 4759,
            4783, 4787, 4789, 4793, 4799, 4801, 4813, 4817, 4831, 4861, 4871, 4877, 4889,
            4903, 4909, 4919, 4931, 4933, 4937, 4943, 4951, 4957, 4967, 4969, 4973, 4987,
            4993, 4999,

            5003, 5009, 5011, 5021, 5023, 5039, 5051, 5059, 5077, 5081, 5087, 5099, 5101,
            5107, 5113, 5119, 5147, 5153, 5167, 5171, 5179, 5189, 5197, 5209, 5227, 5231,
            5233, 5237, 5261, 5273, 5279, 5281, 5297, 5303, 5309, 5323, 5333, 5347, 5351,
            5381, 5387, 5393, 5399, 5407, 5413, 5417, 5419, 5431, 5437, 5441, 5443, 5449,
            5471, 5477, 5479, 5483, 5501, 5503, 5507, 5519, 5521, 5527, 5531, 5557, 5563,
            5569, 5573, 5581, 5591, 5623, 5639, 5641, 5647, 5651, 5653, 5657, 5659, 5669,
            5683, 5689, 5693, 5701, 5711, 5717, 5737, 5741, 5743, 5749, 5779, 5783, 5791,
            5801, 5807, 5813, 5821, 5827, 5839, 5843, 5849, 5851, 5857, 5861, 5867, 5869,
            5879, 5881, 5897, 5903, 5923, 5927, 5939, 5953, 5981, 5987
        };
        #endregion

        #region Additional construction functions
        /// <summary>
        /// Create a BigInteger from the specified text and convertBase.
        /// </summary>
        /// <param name='text'>A text string representing the number</param>
        /// <param name='convertBase'>The convertion base</param>
        public static BigInteger CreateBigInteger(string text, int convertBase)
        {
            if (convertBase == 16)
            {
                var isEvenLength = (text.Length % 2 == 0);
                var length = (text.Length / 2) + (isEvenLength ? 0 : 1);
                var result = new byte[length + 1];
                for (var i = 0; i < length; i++)
                {
                    var j = text.Length - (i * 2) - 1;
                    var ch1 = '0';
                    if (j > 0)
                        ch1 = text[j - 1];
                    var b = GetHexadecimalByte(ch1, text[j]);
                    result[i] = b;
                }
                result[length] = 0;
                return new BigInteger(result).MakePositive();
            }
            if (convertBase == 10)
                return BigInteger.Parse(text).MakePositive();
            throw new Exception("Unsupported conversion base");
        }

        /// <summary>
        /// Create a random BigInteger with the specified number of bits.
        /// </summary>
        /// <param name='numberOfBits'>Number of bits</param>
        /// <param name='random'>The Random instance for generating the BigInteger</param>
        public static BigInteger CreateBigInteger(int numberOfBits, Random random)
        {
            var numberOfFullBytes = numberOfBits / 8;
            var numberOfRemainingBits = numberOfBits % 8;
            var numberOfBytes = numberOfFullBytes + (numberOfRemainingBits > 0 ? 1 : 0);

            // This retry loop was added to handle weird exceptions from Mono's version of the BigInteger class
            // ToDo: Verify exactly what is causing this problem
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    var randomBytes = new byte[numberOfBytes];
                    random.NextBytes(randomBytes);

                    // Mask off unused bits
                    if (numberOfRemainingBits > 0)
                        randomBytes[randomBytes.Length - 1] =
                            MaskBits(randomBytes[randomBytes.Length - 1], 8 - numberOfRemainingBits);

                    // Make unsigned and return result
                    return new BigInteger(randomBytes).MakePositive();
                }
                catch (Exception)
                {
                    // If it's the last retry iteration, just throw the exception
                    if (i == 2)
                        throw;
                }
            }

            // This should be unreachable code
            throw new Exception("Should not happen");
        }
        #endregion

        #region Instance extensions for BigInteger static math functions
        public static BigInteger ModPow(this BigInteger bigInt, BigInteger exponent,
            BigInteger modulus)
        {
            return BigInteger.ModPow(bigInt, exponent, modulus);
        }

        public static BigInteger Multiply(this BigInteger bigInt, BigInteger value)
        {
            return BigInteger.Multiply(bigInt, value);
        }

        public static BigInteger Add(this BigInteger bigInt, BigInteger value)
        {
            return BigInteger.Add(bigInt, value);
        }

        public static BigInteger Subtract(this BigInteger bigInt, BigInteger value)
        {
            return BigInteger.Subtract(bigInt, value);
        }

        public static BigInteger Divide(this BigInteger bigInt, BigInteger value)
        {
            return BigInteger.Divide(bigInt, value);
        }

        public static BigInteger Gcd(this BigInteger bigInt, BigInteger value)
        {
            return BigInteger.GreatestCommonDivisor(bigInt, value);
        }
        #endregion

        #region Bit masking and BitCount
        private static byte MaskBits(byte b, int bitCount)
        {
            if (bitCount == 7)
                return (byte)(b & 1);
            if (bitCount == 6)
                return (byte)(b & 3);
            if (bitCount == 5)
                return (byte)(b & 7);
            if (bitCount == 4)
                return (byte)(b & 15);
            if (bitCount == 3)
                return (byte)(b & 31);
            if (bitCount == 2)
                return (byte)(b & 63);
            if (bitCount == 1)
                return (byte)(b & 127);
            throw new ArgumentException("bitCount is not from 1-7");
        }

        public static int BitCount(this BigInteger bigInt)
        {
            byte[] bytes = bigInt.ToUnsignedByteArray();
            return (bytes.Length * sizeof(byte));
        }
        #endregion

        #region Prime number testing
        // Modified from http://www.codeproject.com/Articles/2728/C-BigInteger-Class for use with the BigInteger
        public static bool RabinMillerTest(this BigInteger thisVal, int confidence)
        {
            byte[] bytes = thisVal.ToUnsignedByteArray();
            if (bytes.Length == 1)
            {
                // test small numbers
                if (bytes[0] == 0 || bytes[0] == 1)
                    return false;
                if (bytes[0] == 2 || bytes[0] == 3)
                    return true;
            }

            if ((bytes[0] & 0x1) == 0)     // even numbers
                return false;

            // calculate values of s and t
            BigInteger pSub1 = thisVal - (new BigInteger(1));
            byte[] pSub1Bytes = pSub1.ToUnsignedByteArray();
            int s = 0;

            for (var index = 0; index < pSub1Bytes.Length; index++)
            {
                uint mask = 0x01;

                for (var i = 0; i < 32; i++)
                {
                    if ((pSub1Bytes[index] & mask) != 0)
                    {
                        index = pSub1Bytes.Length;      // to break the outer loop
                        break;
                    }
                    mask <<= 1;
                    s++;
                }
            }

            BigInteger t = pSub1 >> s;

            int bits = thisVal.BitCount();
            BigInteger a = new BigInteger().MakePositive();
            var rand = new Random();

            for (var round = 0; round < confidence; round++)
            {
                var done = false;

                while (!done)        // generate a < n
                {
                    int testBits = 0;

                    // make sure "a" has at least 2 bits
                    while (testBits < 2)
                        testBits = (int)(rand.NextDouble() * bits);

                    a = CreateBigInteger(testBits, rand);
                    byte[] aBytes = a.ToUnsignedByteArray();

                    // make sure "a" is not 0
                    if (aBytes.Length > 1 || (aBytes.Length == 1 && aBytes[0] != 1))
                        done = true;
                }

                // check whether a factor exists(fix for version 1.03)
                BigInteger gcdTest = a.Gcd(thisVal);
                byte[] gcdBytes = gcdTest.ToUnsignedByteArray();
                if (gcdBytes.Length == 1 && gcdBytes[0] != 1)
                    return false;

                BigInteger b = a.ModPow(t, thisVal);
                byte[] bBytes = b.ToUnsignedByteArray();

                var result = bBytes.Length == 1 && bBytes[0] == 1;

                for (var j = 0; result == false && j < s; j++)
                {
                    if (b == pSub1)         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
                    {
                        result = true;
                        break;
                    }

                    b = (b * b) % thisVal;
                }

                if (result == false)
                    return false;
            }
            return true;
        }

        public static bool IsProbablePrime(this BigInteger thisVal, int confidence)
        {
            // test for divisibility by the smaller primes
            for (var p = 0; p < SmallPrimes.Length; p++)
            {
                BigInteger divisor = SmallPrimes[p];

                if (divisor >= thisVal)
                    break;

                BigInteger resultNum = thisVal % divisor;
                if (resultNum == BigInteger.Zero)
                    return false;
            }

            return (thisVal.RabinMillerTest(confidence));
        }

        public static bool IsSafePrime(this BigInteger thisVal, int confidence)
        {
            if (!thisVal.IsProbablePrime(confidence))
                return false;
            // thisVal(q) is safe prime if in q =(2p - 1), p is also a probable prime
            // p =(q - 1) / 2
            var test = thisVal.Subtract(1).Divide(2);
            return test.IsProbablePrime(confidence);
        }

        public static BigInteger GeneratePseudoPrime(int bits, int confidence, Random rand)
        {
            while (true)
            {
                BigInteger result = CreateBigInteger(bits, rand);
                if (BigInteger.Remainder(result, Two) == BigInteger.Zero)
                    result++;
                // prime test
                if (result.IsProbablePrime(confidence))
                    return result;
            }
        }

        public static BigInteger GenerateSafePrime(int bits, int confidence, Random rand)
        {
            BigInteger pseudoPrime = GeneratePseudoPrime(bits, confidence, rand);
            bool found = false;

            while (!found)
            {
                found = pseudoPrime.IsSafePrime(confidence);
                if (!found)
                    pseudoPrime = GeneratePseudoPrime(bits, confidence, rand);
            }
            return pseudoPrime;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Produces a hex string representation the unsigned value of this BigInteger
        /// </summary>
        public static string ToHexString(this BigInteger bigInt)
        {
            return bigInt.ToUnsignedByteArray().ToHexString();
        }

        /// <summary>
        /// Returns a byte array for what the unsigned value of bigInt would be.
        /// </summary>
        public static byte[] ToUnsignedByteArray(this BigInteger bigInt)
        {
            byte[] bigIntByteArray = bigInt.ToByteArray();
            if (bigIntByteArray[bigIntByteArray.Length - 1] == 0)
            {
                var shortenedByteArray = new byte[bigIntByteArray.Length - 1];
                for (int i = 0; i < shortenedByteArray.Length; i++)
                {
                    shortenedByteArray[i] = bigIntByteArray[i];
                }
                bigIntByteArray = shortenedByteArray;
            }
            return bigIntByteArray;
        }

        /// <summary>
        /// Makes a BigInteger the value it would be if the BigInteger class was unsigned.
        /// </summary>
        private static BigInteger MakePositive(this BigInteger inValue)
        {
            if (inValue < 0)
            {
                byte[] oldBytes = inValue.ToByteArray();
                var newBytes = new byte[oldBytes.Length + 1];
                for (int i = 0; i < oldBytes.Length; i++)
                    newBytes[i] = oldBytes[i];
                newBytes[oldBytes.Length] = 0;
                return new BigInteger(newBytes);
            }
            return inValue;
        }

        private static byte GetHexadecimalByte(char c1, char c2)
        {
            byte upperByte = HexadecimalCharToByte(c1);
            byte lowByte = HexadecimalCharToByte(c2);
            upperByte = (byte)(upperByte << 4);
            return (byte)(upperByte | lowByte);
        }

        /// <summary>
        /// Returns the hexadecimal byte value that the specified character represents.
        /// </summary>
        private static byte HexadecimalCharToByte(char c)
        {
            switch (c)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'a': return 10;
                case 'A': return 10;
                case 'b': return 11;
                case 'B': return 11;
                case 'c': return 12;
                case 'C': return 12;
                case 'd': return 13;
                case 'D': return 13;
                case 'e': return 14;
                case 'E': return 14;
                case 'f': return 15;
                case 'F': return 15;
            }
            throw new FormatException("Invalid hexadecimal character");
        }
        #endregion
    }
}
