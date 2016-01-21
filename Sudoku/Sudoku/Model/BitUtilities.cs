namespace Sudoku.Model
{
    public static class BitUtilities
    {
        public static int NumberOfBitsSet(int bits)
        {
            int count = 0;
            while (bits > 0)
            {
                bits &= bits - 1;
                count++;
            }

            return count;
        }

        public static int BitwiseOR(System.Collections.Generic.IEnumerable<int> elements)
        {
            int totalOred = 0;
            foreach(int element in elements)
                totalOred |= element;

            return totalOred;
        }

        // XOR all the values passed in to find an only option
        public static bool OnlyOption(int[] options, out int option)
        {
            option = 0;
            int filled = 0;
            for (int index = 0; index < options.Length; index++)
                if ((options[index] & options[index] - 1) > 0)                                      // Not a single base of 2 number (1, 2, 4, 8, ...)
                {
                    filled |= option & options[index];
                    option ^= options[index];                                                       // XOR
                }

            option &= ~filled;
            return option > 0 && (option & option - 1) == 0;                                        // Single base of 2 number, but not 0
        }

        // First index of item in array containing bit
        // PRE CONDITION: bit set within one of the items
        public static int ContainingBitIndex(int[] array, int bit)
        {
            return System.Array.FindIndex(array, x => (x & bit) > 0);
        }
    }
}
