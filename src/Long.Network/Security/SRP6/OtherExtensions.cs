using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Long.Network.Security.SRP6
{
    /// <summary>
    /// Srp6 extenstion methods
    /// </summary>
    public static class OtherExtensions
    {
        /// <summary>
        /// Create a BigInteger from a hash of the salt and identity hash (SaltedIdentityHash).
        /// </summary>
        /// <param name='salt'>The salt</param>
        /// <param name='identityHash'>The identity hash</param>
        public static BigInteger CreateSaltedIdentityHash(this BigInteger salt, byte[] identityHash)
        {
            return BigIntegerExtensions.CreateBigInteger(
                Concatenate(salt.ToByteArray(), identityHash).Sha1Hash().ToHexString(), 16);
        }

        /// <summary>
        /// This is a SHA1 hash extension method.
        /// </summary>
        /// <returns>The hash</returns>
        /// <param name='arg1'>The byte array</param>
        public static byte[] Sha1Hash(this byte[] arg1)
        {
            try
            {
                if ((arg1 == null) || (arg1.Length == 0))
                    throw new InvalidOperationException("arg1 can not be null or have a length of zero.");
                return SHA1.HashData(arg1);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sha1Hash: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Concatenates multiple byte arrays
        /// </summary>
        /// <returns>The concatenated array</returns>
        /// <param name='arrays'>The source arrays</param>
        private static byte[] Concatenate(params byte[][] arrays)
        {
            var length = arrays.Where(array => array != null).Sum(array => array.Length) + 1;
            var result = new byte[length];
            length = 0;
            foreach (var array in arrays.Where(array => array != null))
            {
                Array.Copy(array, 0, result, length, array.Length);
                length += array.Length;
            }
            return result;
        }

        /// <summary>
        /// Extracts an array from an array
        /// </summary>
        /// <returns>The new array.</returns>
        /// <param name='sourceArray'>The source array</param>
        /// <param name='offset'>The offset to start building the new array</param>
        /// <param name='length'>The length of the new array.</param>
        public static T[] SubArray<T>(this T[] sourceArray, int offset, int length)
        {
            var result = new T[length];
            Array.Copy(sourceArray, offset, result, 0, length);
            return result;
        }

        /// <summary>
        /// Converts a byte array into a hex formatted text string.
        /// </summary>
        /// <returns>Hex formatted text string</returns>
        /// <param name="byteArray">The byte array to convert</param>
        public static string ToHexString(this byte[] byteArray)
        {
            if (byteArray == null)
                return "";
            string result = "";
            for (int i = byteArray.Length - 1; i >= 0; i--)
                result += byteArray[i].ToString("X2");
            return result;
        }

        public static string ToString(this byte[] totalArray, int offset, int length)
        {
            byte[] array = SubArray(totalArray, offset, length);
            return Encoding.ASCII.GetString(array);
        }

    }
}
