namespace Long.Shared.Mathematics
{
    public class BitVector32
    {
        public uint[] Params;

        public int Size { get { return 32 * Params.Length; } }

        public BitVector32(int nSize)
        {
            int num = nSize / 32;
            if (nSize % 32 != 0)
                num++;

            Params = new uint[num];
        }

        public void Add(int index)
        {
            if (index < Size)
            {
                int num = index / 32;
                uint num2 = (uint)(1UL << (index - 1) % 32);
                Params[num] |= num2;
            }
        }

        public void Remove(int index)
        {
            if (index < Size)
            {
                int num = index / 32;
                uint num2 = (uint)(1UL << (index - 1) % 32);
                Params[num] &= ~num2;
            }
        }

        public bool Contain(int index)
        {
            if (index > Size)
            {
                return false;
            }
            int num = index / 32;
            uint num2 = (uint)(1 << index % 32);
            return (Params[num] & num2) == num2;
        }

        public int Count()
        {
            int num = 0;
            for (int i = 0; i < Size / 32; i++)
                for (int j = 0; j < 32; j++)
                    if ((Params[i] & (1 << j)) == 1 << j)
                        num++;

            return num;
        }

        public void Clear()
        {
            ushort num = (byte)(Size / 32);
            for (byte b = 0; b < num; b = (byte)(b + 1))
                Params[b] = 0u;
        }
    }
}
