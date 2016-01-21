using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model
{
    public class Combinations
    {
        private delegate IEnumerable<T> SelectElementsDelegate<T>(IEnumerable<T> elements, int select);
        private delegate int[] NextValuesDelegate(int x);

        private int[][] setBitsLookupTable;                                                         // Lookup table of the integers with the indices number of bits set i.e. [2] = 3, 5, 6, ... (011, 101, 110 have 2 set bits)

        public Combinations(int maxItemsSelectFrom)
        {
            List<int> setBits = CreateSetBitsLookup(maxItemsSelectFrom);

            setBitsLookupTable = new int[maxItemsSelectFrom][];

            for (int index = 2; index < maxItemsSelectFrom; index++)
                setBitsLookupTable[index] = Enumerable.Range(0, setBits.Count).Where(x => setBits[x] == index).ToArray();   // Get indices of items with respective choices
        }

        public IEnumerable<IEnumerable<T>> Select<T>(IEnumerable<T> from, int pick)
        {
            //int v = pick;                                     // Calculates next combination - Bit Twiddling Hacks
            //int t = (v | (v - 1)) + 1;
            //int w = t | ((((t & -t) / (v & -v)) >> 1) - 1);

            SelectElementsDelegate<T> selectElements = (elements, select) => { return elements.Where((x, i) => (1 << i & select) > 0); };   // Return elements where the index is in the select bit flag

            foreach (int combination in setBitsLookupTable[pick].Where(x => x < 1 << from.Count())) // Get bit flags used to select the combinations from the lookup table, up to the number of items to select from
                yield return selectElements(from, combination);
        }

        // Populate array with the number of bits set i.e. [0] => 0, [1] => 1, [2] => 1, [3] => 2, ..., [333] => 5 (i.e. 101001101 has 5 bits set)
        private List<int> CreateSetBitsLookup(int n)
        {
            NextValuesDelegate nextValues = (x) => { return new int[] { x, x + 1, x + 1, x + 2 }; };

            var lookupTable = new List<int>(nextValues(0));                                         // Starting values { 0, 1, 1, 2 }

            for (int i = 2, tableSize = 4; i < n; i++, tableSize <<= 1)
                for (int j = 0, offset = tableSize >> 2; j < (tableSize >> 1) - offset; j++)
                    lookupTable.InsertRange(tableSize + (j << 2), nextValues(lookupTable[j + offset]));

            return lookupTable;
        }
    }
}