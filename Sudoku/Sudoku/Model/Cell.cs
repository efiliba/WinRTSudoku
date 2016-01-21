namespace Sudoku.Model
{
    public class Cell
    {
        public int Column { get; private set; }                                                     // This cell's column within it's sub-grid
        public int Row { get; private set; }

        public bool IsFixed { get; private set; }                                                   // Given / starting value of cell
        public bool IsSet { get; private set; }                                                     // Cell's option has been determined
        public int SetColumn { get; private set; }                                                  // Column of the set / final option
        public int SetRow { get; private set; }

        public int Options { get; private set; }                                                    // Bit-flag of remaining options
        public int TotalOptionsRemaining { get; private set; }                                      // The number of options remaining for this cell

        private int columns;
        private int rows;

        public Cell(int columns, int rows, int column, int row)                                     // Set total columns / rows and this cell's column / row within it's sub-grid
        {
            this.columns = columns;
            this.rows = rows;
            Column = column;
            Row = row;

            IsFixed = false;
            IsSet = false;

            Options = (1 << columns * rows) - 1;                                                    // Set all bits
            TotalOptionsRemaining = columns * rows;
        }

        // Copy constructer
        public Cell(Cell cell)
        {
            columns = cell.columns;
            rows = cell.rows;
            Column = cell.Column;
            Row = cell.Row;

            IsFixed = cell.IsFixed;
            IsSet = cell.IsSet;
            SetColumn = cell.SetColumn;
            SetRow = cell.SetRow;

            Options = cell.Options;
            TotalOptionsRemaining = cell.TotalOptionsRemaining;
        }

        public string Symbol
        {
            get { return (SetRow * columns + SetColumn + 1).ToString(); }
        }

        public bool Solved
        {
            get { return TotalOptionsRemaining == 1; }
        }

        public bool RemoveOption(int column, int row)                                               // Return if last option left after removing this option
        {
            bool lastOptionFound = false;

            int bit = 1 << columns * row + column;
            if ((Options & bit) > 0)                                                                // Check if option to remove exists
            {
                Options &= ~bit;
                if (--TotalOptionsRemaining == 1)
                {
                    RemainingOption(Options, out column, out row);                                  // Find last remaining option
                    IsSet = true;
                    SetColumn = column;
                    SetRow = row;
                    lastOptionFound = true;
                }
            }

            return lastOptionFound;
        }

        public bool RemoveOption(int option)
        {
            bool lastOptionFound = false;

            if ((Options & option) > 0)
            {
                Options &= ~option;
                if (--TotalOptionsRemaining == 1)
                {
                    int column;
                    int row;
                    RemainingOption(Options, out column, out row);                                  // Find last remaining option
                    IsSet = true;
                    SetColumn = column;
                    SetRow = row;
                    lastOptionFound = true;
                }
            }

            return lastOptionFound;
        }

        public bool RemoveOptions(int remove)
        {
            bool lastOptionFound = false;

            int removeOptions = Options & remove;
            if (removeOptions > 0)
            {
                Options -= removeOptions;
                TotalOptionsRemaining -= BitUtilities.NumberOfBitsSet(removeOptions);

                if (TotalOptionsRemaining == 1)
                {
                    int column;
                    int row;
                    RemainingOption(Options, out column, out row);                                  // Find last remaining option
                    IsSet = true;
                    SetColumn = column;
                    SetRow = row;
                    lastOptionFound = true;
                }
            }

            return lastOptionFound;
        }

        public int AvailableOptions
        {
            get { return Options; }
        }

        public void Fix(int option)
        {
            IsFixed = true;
            Set(option);
        }

        public void Fix(int column, int row)
        {
            IsFixed = true;
            Set(column, row);
        }

        public void Set(int column, int row)                                                      // Set final option by column, row
        {
            IsSet = true;
            SetColumn = column;
            SetRow = row;
            ClearAllExcept(column, row);
        }

        public void Set(int option)                                                               // Set final option by option value
        {
            IsSet = true;

            int remainingOption = option;
            int rowMask = (1 << columns) - 1;

            SetRow = 0;
            while ((remainingOption & rowMask) == 0)
            {
                remainingOption >>= columns;
                SetRow++;
            }

            SetColumn = 0;
            while (remainingOption > 1)
            {
                remainingOption >>= 1;
                SetColumn++;
            }

            ClearAllExcept(option);
        }

        public void Reset()
        {
            IsFixed = false;
            IsSet = false;

            Options = (1 << columns * rows) - 1;                                                    // Set all bits
            TotalOptionsRemaining = columns * rows;
        }

        public bool ContainsOption(int column, int row)
        {
            int bit = 1 << column + row * columns;
            return (Options & bit) > 0;
        }

        public bool ContainsOption(int checkOption)
        {
            return (Options & checkOption) > 0;
        }

        public bool ContainsOptions(int checkOptions)
        {
            return (Options & checkOptions) == checkOptions;
        }

        private void RemainingOption(int options, out int column, out int row)
        {
            row = 0;
            for (int mask = (1 << columns) - 1; options > mask; options >>= columns)                // Find first row containing an option
                row++;

            for (column = 0; options > 1; options >>= 1)
                column++;
        }

        private void ClearAllExcept(int column, int row)
        {
            ClearAllExcept(1 << columns * row + column);
        }

        private void ClearAllExcept(int option)
        {
            Options = option;
            TotalOptionsRemaining = 1;
        }

        public int[] RemovedOptionsPerRow(int row)
        {
            System.Collections.Generic.List<int> removedOptions = new System.Collections.Generic.List<int>(columns);

            for (int column = 0, bit = 1 << columns * row; column < columns; column++, bit <<= 1)   // bit = 1 << column + row * columns
                if ((Options & bit) == 0)
                    removedOptions.Add(column);

            return removedOptions.ToArray();
        }

        // Remove options iff cell contains other options 
        public bool RemoveIfExtraOptions(int options)
        {
            return TotalOptionsRemaining > 1 && (Options & ~options) > 0 && RemoveOptions(options);
        }
    }
}