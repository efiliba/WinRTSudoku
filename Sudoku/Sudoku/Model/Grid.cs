using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model
{
    public class Grid
    {
        private struct LimitedOption                                                                // Options limited to a column, row or both (sub-grid)
        {
            public int Column;
            public int Row;
            public int Options;
        }

        private int columns;
        private int rows;

        private SubGrid[][] subGrids;                                                               // Jagged array used for efficiency ???
        private int totalSet;                                                                       // Track how many cells have been set

        private static Combinations combinations;

        public Grid(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;

            subGrids = new SubGrid[rows][];
            for (int row = 0; row < rows; row++)
            {
                subGrids[row] = new SubGrid[columns];
                for (int column = 0; column < columns; column++)
                    subGrids[row][column] = new SubGrid(columns, rows, column, row);
            }

            totalSet = 0;
            combinations = new Combinations(columns * rows);
        }

        public SubGrid this[int column, int row]                                                    // grids called by [column, row] but accessed by [row][column] for efficiency
        {
            get { return subGrids[row][column]; }
            set { subGrids[row][column] = value; }
        }

        public bool Solve()
        {
            //bool simplified = true;

            //while (simplified)
            //{
            //    simplified = false;
            //    while (Simplify())
            //        simplified = true;

            //    if (simplified)
            //        simplified = Eliminate(columns * rows);
            //}


            while (Simplify() || Eliminate(columns * rows))                                         // an only option found or an option removed
                ;

            return Solved();// totalSet == columns * rows * columns * rows;
        }

        public bool Solved()
        {
            bool solved = true;

            int row = rows;
            while (solved && row-- > 0)
            {
                int column = columns;
                while (solved && column-- > 0)
                    solved = subGrids[row][column].Solved();
            }

            return solved;
        }

        public bool RemoveOption(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int optionColumn, int optionRow)
        {
            Cell cell = subGrids[subGridRow][subGridColumn][cellColumn, cellRow];
            if (cell.RemoveOption(optionColumn, optionRow))                                         // Check if last option left
            {
                totalSet++;
                StrikeOut(subGridColumn, subGridRow, cellColumn, cellRow, cell.Options);            // Remaining option
                return true;
            }
            else
                return false;
        }

        public bool RemoveOption(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int option)
        {
            Cell cell = subGrids[subGridRow][subGridColumn][cellColumn, cellRow];
            if (cell.RemoveOption(option))                                                          // Check if last option left
            {
                totalSet++;
                StrikeOut(subGridColumn, subGridRow, cellColumn, cellRow, cell.Options);            // Remaining option
                return true;
            }
            else
                return false;
        }

        public bool Simplify()
        {
            bool onlyOptionFound = false;

            // Check/remove only options in columns/rows/sub-grids and mulitipe options limited to a certain number of related cells i.e. if 2 cells in a row can only contain 1 or 2 => remove from other cells in row 
            while (RemoveOnlyOptions() || CheckLimitedOptions())
                onlyOptionFound = true;

            return onlyOptionFound;
        }

        private bool IsValid()
        {
            bool valid = MatrixValid(GetTransposedCellsMatrix()) && MatrixValid(GetCellsMatrix());  // Check columns and rows contain all options and no set cell duplicted 

            return valid;
        }

        private bool MatrixValid(Cell[][] matrix)
        {
            bool valid = true;
            int size = columns * rows;
            int index = size;
            while (valid && index-- > 0)
            {
                var setOptions = matrix[index].Where(x => x.IsSet).GroupBy(x => x.Options).Where(x => x.Count() == 1).Select(x => x.Key);   // Get unique set cells
                var unsetOptions = matrix[index].Where(x => !x.IsSet).Select(x => x.Options);

                valid = setOptions.Count() + unsetOptions.Count() == size &&                        // Ensures setOptions did not contain duplicates
                    (BitUtilities.BitwiseOR(setOptions) | BitUtilities.BitwiseOR(unsetOptions)) == (1 << size) - 1; // totalSetOptions | totalUnsetOptions must contain all the options
            }

            return valid;
        }

        private void Load(Cell[] cells)
        {
            Cell[][] subGrid = new Cell[columns][];
            for (int row = 0; row < columns; row++)                                                 // Use SubGrid's number of rows i.e. swopped columns
                subGrid[row] = new Cell[rows];

            int size = columns * rows;

            for (int subGridRow = 0; subGridRow < rows; subGridRow++)
                for (int subGridColumn = 0; subGridColumn < columns; subGridColumn++)
                {
                    for (int cellRow = 0; cellRow < columns; cellRow++)
                        for (int cellColumn = 0; cellColumn < rows; cellColumn++)
                            subGrid[cellRow][cellColumn] = new Cell(cells[subGridRow * size * columns + subGridColumn * rows + cellRow * size + cellColumn]);

                    subGrids[subGridRow][subGridColumn].SetCells(subGrid);
                }
        }

        public Cell[] Save()
        {
            int size = columns * rows;
            Cell[] cells = new Cell[size * size];

            for (int subGridRow = 0; subGridRow < rows; subGridRow++)
                for (int subGridColumn = 0; subGridColumn < columns; subGridColumn++)
                {
                    var subMatrix = subGrids[subGridRow][subGridColumn].GetCellsMatrix();
                    for (int cellRow = 0; cellRow < columns; cellRow++)
                        for (int cellColumn = 0; cellColumn < rows; cellColumn++)
                            cells[subGridRow * size * columns + subGridColumn * rows + cellRow * size + cellColumn] = new Cell(subMatrix[cellRow][cellColumn]);
                }

            return cells;
        }

        public void Reset()
        {
            totalSet = 0;
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    subGrids[row][column].Reset();
        }

        private bool Eliminate(int unsetOptionsDepth)
        {
            Cell[] cells = Save();                                                                  // Save current state
            int saveTotalSet = totalSet;

            bool valid = true;
            int totalUnsetOptions = 1;
            while (valid && ++totalUnsetOptions < unsetOptionsDepth)
            {
                int row = rows;
                while (valid && row-- > 0)
                {
                    int column = columns;
                    while (valid && column-- > 0)
                    {
                        IList<Cell> unsetCells = UnsetCells(ref column, ref row, totalUnsetOptions).ToList();   // May reduce column and row indices
                        var enumerator = unsetCells.GetEnumerator();
                        while (valid && enumerator.MoveNext())
                        {
                            Cell cell = enumerator.Current;

                            int options = cell.Options;

                            int tryOption = options & ~(options - 1);                               // lowest set bit value
                            while (tryOption > 0 && valid)
                            {
                                Set(column, row, cell.Column, cell.Row, tryOption);
                                Simplify();
                                valid = IsValid();
                                Load(cells);                                                        // Reset
                                totalSet = saveTotalSet;

                                if (valid)
                                {
                                    options -= tryOption;                                           // remove tried option
                                    tryOption = options & ~(options - 1);
                                }
                                else
                                    RemoveOption(column, row, cell.Column, cell.Row, tryOption);    // Remove tryOption i.e. resulted in an invalid state
                            }
                        }
                    }
                }
            }

            return !valid;                                                                          // Option removed?
        }

        private IEnumerable<Cell> UnsetCells(ref int column, ref int row, int totalUnsetOptions)
        {
            IEnumerable<Cell> cells = new List<Cell>();
            bool set = false;
            while (!set && row >= 0)
            {
                while (!set && column >= 0)
                {
                    cells = subGrids[row][column].UnsetCells(totalUnsetOptions);
                    if (!(set = cells.Count() > 0))
                        column--;
                }

                if (!set && row-- > 0)
                    column = columns - 1;
            }

            return cells;
        }

        public void StrikeOut(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int option)
        {
            List<Option> removedOptionsFromColumn;
            List<Option> removedOptionsFromRow;
            List<Option> lastOptions = subGrids[subGridRow][subGridColumn].StrikeOutCell(cellColumn, cellRow, option, out removedOptionsFromColumn, out removedOptionsFromRow);

            foreach (Option removeOption in removedOptionsFromColumn.Distinct())
                lastOptions.AddRange(RemoveOptionFromOtherColumns(removeOption.SubGridColumn, removeOption.SubGridRow, removeOption.CellColumn, removeOption.Bits));
            foreach (Option removeOption in removedOptionsFromRow)
                lastOptions.AddRange(RemoveOptionFromOtherRows(removeOption.SubGridColumn, removeOption.SubGridRow, removeOption.CellRow, removeOption.Bits));

            lastOptions.AddRange(RemoveOptionsFromColumn(subGridColumn, subGridRow, cellColumn, option));
            lastOptions.AddRange(RemoveOptionsFromRow(subGridColumn, subGridRow, cellRow, option));

            foreach (Option lastOption in lastOptions)
                StrikeOut(lastOption.SubGridColumn, lastOption.SubGridRow, lastOption.CellColumn, lastOption.CellRow, lastOption.Bits);

            totalSet += lastOptions.Count();
        }

        public void Fix(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int optionColumn, int optionRow)
        {
            totalSet++;
            subGrids[subGridRow][subGridColumn][cellColumn, cellRow].Fix(optionColumn, optionRow);
            StrikeOut(subGridColumn, subGridRow, cellColumn, cellRow, 1 << columns * optionRow + optionColumn);
        }

        public void Fix(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int option)
        {
            totalSet++;
            subGrids[subGridRow][subGridColumn][cellColumn, cellRow].Fix(option);
            StrikeOut(subGridColumn, subGridRow, cellColumn, cellRow, option);
        }

        public void Set(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int option)
        {
            totalSet++;
            subGrids[subGridRow][subGridColumn][cellColumn, cellRow].Set(option);
            StrikeOut(subGridColumn, subGridRow, cellColumn, cellRow, option);
        }

        public void Fix(int[] fixedOptions)
        {
            for (int subGridRow = 0; subGridRow < rows; subGridRow++)
                for (int subGridColumn = 0; subGridColumn < columns; subGridColumn++)
                    for (int cellRow = 0; cellRow < columns; cellRow++)
                        for (int cellColumn = 0; cellColumn < rows; cellColumn++)
                        {
                            int option = fixedOptions[(subGridRow * columns + cellRow) * columns * rows + subGridColumn * rows + cellColumn];
                            if (option > 0)
                            {
                                totalSet++;
                                subGrids[subGridRow][subGridColumn][cellColumn, cellRow].Fix(option);
                                StrikeOut(subGridColumn, subGridRow, cellColumn, cellRow, option);
                            }
                        }
        }

        public void Fix(string options)
        {
            int option;
            for (int subGridRow = 0; subGridRow < rows; subGridRow++)
                for (int subGridColumn = 0; subGridColumn < columns; subGridColumn++)
                    for (int cellRow = 0; cellRow < columns; cellRow++)
                        for (int cellColumn = 0; cellColumn < rows; cellColumn++)
                        {
                            int.TryParse(options.Substring((subGridRow * columns + cellRow) * columns * rows + subGridColumn * rows + cellColumn, 1), out option);
                            if (option > 0)
                            {
                                totalSet++;
                                option = 1 << (option - 1);
                                subGrids[subGridRow][subGridColumn][cellColumn, cellRow].Fix(option);
                                StrikeOut(subGridColumn, subGridRow, cellColumn, cellRow, option);
                            }
                        }
        }

        public void Unfix(int subGridColumn, int subGridRow, int cellColumn, int cellRow)
        {
            subGrids[subGridRow][subGridColumn][cellColumn, cellRow].Reset();
            int[] fixedCells = GetCellsArray().Select(x => x.IsFixed ? x.Options : 0).ToArray();    // Get fixed cells i.e. 0, 0, 0, 8, 4, 0, 0, 1, ...
            Reset();
            Fix(fixedCells);
            Solve();
        }

        private Cell[] GetCellsArray()
        {
            Cell[] array = new Cell[columns * rows * columns * rows];

            int subGridRow = rows;
            while (subGridRow-- > 0)
            {
                int subGridColumn = columns;
                while (subGridColumn-- > 0)
                {
                    var subMatrix = subGrids[subGridRow][subGridColumn].GetCellsMatrix();

                    int cellColumn = rows;
                    while (cellColumn-- > 0)
                    {
                        int cellRow = columns;
                        while (cellRow-- > 0)
                            array[(subGridRow * columns + cellRow) * columns * rows + subGridColumn * rows + cellColumn] = subMatrix[cellRow][cellColumn];
                    }
                }
            }

            return array;
        }

        // Remove option from the other sub grid's columns / rows when the option must belong in a specific sub grid's column / row
        public bool RemoveUnavailableOptions(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int optionColumn, int optionRow)
        {
            return RemoveUnavailableOptions(subGridColumn, subGridRow, cellColumn, cellRow, 1 << columns * optionRow + optionColumn);
        }

        private bool RemoveUnavailableOptions(int subGridColumn, int subGridRow, int cellColumn, int cellRow, int option)
        {
            List<Option> lastOptions = new List<Option>();

            // Check sub grid's column and if found remove option from other columns
            if (subGrids[subGridRow][subGridColumn].OptionRemovedFromColumn(cellColumn, cellRow, option))
                lastOptions = RemoveOptionFromOtherColumns(subGridColumn, subGridRow, cellColumn, option);

            // Check sub grid's row and if found remove option from other rows
            if (subGrids[subGridRow][subGridColumn].OptionRemovedFromRow(cellColumn, cellRow, option))
                lastOptions = RemoveOptionFromOtherRows(subGridColumn, subGridRow, cellRow, option);

            foreach (Option lastOption in lastOptions)
                StrikeOut(lastOption.SubGridColumn, lastOption.SubGridRow, lastOption.CellColumn, lastOption.CellRow, lastOption.Bits);

            return lastOptions != null;
        }

        // Check for mulitipe options limited to a certain number of related cells i.e. 2 cells in a row can only contain 1 or 2 => remove from other cells in row
        private bool CheckLimitedOptions()
        {
            List<LimitedOption> limitedOptions = FindOptionsLimitedToMatrix(GetTransposedCellsMatrix());    // Columns
            bool limitedOptionFound = RemoveIfExtraOptionsFromColumn(limitedOptions);               // Remove options iff the cell contains other options

            limitedOptions = FindOptionsLimitedToMatrix(GetCellsMatrix());                          // Rows
            limitedOptionFound |= RemoveIfExtraOptionsFromRow(limitedOptions);

            limitedOptions = FindOptionsLimitedToSubGrids();
            limitedOptionFound |= RemoveIfExtraOptionsFromSubGrid(limitedOptions);

            return limitedOptionFound;
        }

        private List<LimitedOption> FindOptionsLimitedToMatrix(Cell[][] cells)
        {
            List<LimitedOption> limitedOptions = new List<LimitedOption>();

            for (int index = 0; index < columns * rows; index++)
            {
                IEnumerable<Cell> unsetCells = cells[index].Where(x => !x.IsSet);                   // Get cells that are still to be set
                int totalUnsetCells = unsetCells.Count();

                bool found = false;
                int pick = 1;
                int maxRemainingOptions = unsetCells.Where(x => x.TotalOptionsRemaining < totalUnsetCells).Max(x => (int?)x.TotalOptionsRemaining) ?? 0;    // Max < totalUnsetCells
                while (!found && pick++ < maxRemainingOptions)
                {
                    IEnumerable<Cell> options = unsetCells.Where(x => x.TotalOptionsRemaining <= pick); // Get options with at least the number of bits to pick set
                    var enumerator = combinations.Select(options, pick).GetEnumerator();            // Get enumerator to each combination of options to select, based on the number of items to pick
                    while (!found && enumerator.MoveNext())
                    {
                        int removeOptions = BitUtilities.BitwiseOR(enumerator.Current.Select(x => x.Options));
                        if (found = BitUtilities.NumberOfBitsSet(removeOptions) <= pick)
                            limitedOptions.Add(new LimitedOption { Column = index, Row = index, Options = removeOptions });
                    }
                }
            }

            return limitedOptions;
        }

        private List<LimitedOption> FindOptionsLimitedToSubGrids()
        {
            List<LimitedOption> limitedOptions = new List<LimitedOption>();

            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                {
                    IEnumerable<Cell> unsetCells = subGrids[row][column].GetUnsetCells();
                    int totalUnsetCells = unsetCells.Count();

                    bool found = false;
                    int pick = 1;
                    int maxRemainingOptions = unsetCells.Where(x => x.TotalOptionsRemaining < totalUnsetCells).Max(x => (int?)x.TotalOptionsRemaining) ?? 0;    // Max < totalUnsetCells
                    while (!found && pick++ < maxRemainingOptions)
                    {
                        IEnumerable<Cell> options = unsetCells.Where(x => x.TotalOptionsRemaining <= pick); // Get options with at least the number of bits to pick set
                        var enumerator = combinations.Select(options, pick).GetEnumerator();        // Get enumerator to each combination of options to select, based on the number of items to pick
                        while (!found && enumerator.MoveNext())
                        {
                            int removeOptions = BitUtilities.BitwiseOR(enumerator.Current.Select(x => x.Options));
                            if (found = BitUtilities.NumberOfBitsSet(removeOptions) <= pick)
                                limitedOptions.Add(new LimitedOption { Column = column, Row = row, Options = removeOptions });
                        }
                    }
                }

            return limitedOptions;
        }

        private bool RemoveIfExtraOptionsFromColumn(List<LimitedOption> limitedOptions)
        {
            List<Option> lastOptions = new List<Option>();

            foreach (LimitedOption limitedOption in limitedOptions)
                for (int row = 0; row < rows; row++)
                    lastOptions.AddRange(subGrids[row][limitedOption.Column / rows].RemoveIfExtraOptionsFromColumn(limitedOption.Column % rows, limitedOption.Options));

            foreach (Option lastOption in lastOptions)
                StrikeOut(lastOption.SubGridColumn, lastOption.SubGridRow, lastOption.CellColumn, lastOption.CellRow, lastOption.Bits);

            totalSet += lastOptions.Count;
            return lastOptions.Count > 0;
        }

        private bool RemoveIfExtraOptionsFromRow(List<LimitedOption> limitedOptions)
        {
            List<Option> lastOptions = new List<Option>();

            foreach (LimitedOption limitedOption in limitedOptions)
                for (int column = 0; column < columns; column++)
                    lastOptions.AddRange(subGrids[limitedOption.Row / columns][column].RemoveIfExtraOptionsFromRow(limitedOption.Row % columns, limitedOption.Options));

            foreach (Option lastOption in lastOptions)
                StrikeOut(lastOption.SubGridColumn, lastOption.SubGridRow, lastOption.CellColumn, lastOption.CellRow, lastOption.Bits);

            totalSet += lastOptions.Count;
            return lastOptions.Count > 0;
        }

        private bool RemoveIfExtraOptionsFromSubGrid(List<LimitedOption> limitedOptions)
        {
            List<Option> lastOptions = new List<Option>();

            foreach (LimitedOption limitedOption in limitedOptions)
                lastOptions.AddRange(subGrids[limitedOption.Row][limitedOption.Column].RemoveIfExtraOptions(limitedOption.Options));

            foreach (Option lastOption in lastOptions)
                StrikeOut(lastOption.SubGridColumn, lastOption.SubGridRow, lastOption.CellColumn, lastOption.CellRow, lastOption.Bits);

            totalSet += lastOptions.Count;
            return lastOptions.Count > 0;
        }

        private List<Option> RemoveOptionsFromColumn(int subGridColumn, int subGridRow, int cellColumn, int options)
        {
            List<Option> lastOptions = new List<Option>();

            // Ignore subGridRow
            int row = rows;
            while (--row > subGridRow)
                lastOptions.AddRange(subGrids[row][subGridColumn].RemoveOptionsFromColumn(cellColumn, options));
            while (row-- > 0)
                lastOptions.AddRange(subGrids[row][subGridColumn].RemoveOptionsFromColumn(cellColumn, options));

            return lastOptions;
        }

        private List<Option> RemoveOptionsFromRow(int subGridColumn, int subGridRow, int cellRow, int options)
        {
            List<Option> lastOptions = new List<Option>();

            // Ignore subGridColumn
            int column = columns;
            while (--column > subGridColumn)
                lastOptions.AddRange(subGrids[subGridRow][column].RemoveOptionsFromRow(cellRow, options));
            while (column-- > 0)
                lastOptions.AddRange(subGrids[subGridRow][column].RemoveOptionsFromRow(cellRow, options));

            return lastOptions;
        }

        private bool RemoveOnlyOptions()
        {
            return RemoveOnlyColumnOptions() || RemoveOnlyRowOptions() || RemoveOnlySubGridOptions();
        }

        private bool RemoveOnlyColumnOptions()
        {
            bool onlyOptionFound = false;

            var matrix = GetTransposedAvailableOptionsMatrix();
            int option;

            // Check for only options in each column
            int column = rows * columns;
            while (onlyOptionFound && column-- > 0)
                if (BitUtilities.OnlyOption(matrix[column], out option))
                {
                    onlyOptionFound = true;
                    int matrixRow = BitUtilities.ContainingBitIndex(matrix[column], option);        // Row within grid where only option found                     
                    Set(column / rows, matrixRow / columns, column % rows, matrixRow % columns, option);
            }

            return onlyOptionFound;
        }

        private bool RemoveOnlyRowOptions()
        {
            bool onlyOptionFound = false;

            int[][] matrix = GetAvailableOptionsMatrix();
            int option;

            // Check for only options in each row
            int row = rows * columns;
            while (!onlyOptionFound && row-- > 0)
                if (BitUtilities.OnlyOption(matrix[row], out option))
                {
                    onlyOptionFound = true;
                    int matrixColumn = BitUtilities.ContainingBitIndex(matrix[row], option);        // Column within grid where only option found                     
                    Set(matrixColumn / rows, row / columns, matrixColumn % rows, row % columns, option);
                }

            return onlyOptionFound;
        }

        private bool RemoveOnlySubGridOptions()
        {
            bool onlyOptionFound = false;

            int option;

            // Check for only options in each sub grid
            int row = rows;
            while (!onlyOptionFound && row-- > 0)
            {
                int column = columns;
                while (!onlyOptionFound && column-- > 0)
                {
                    int[] values = subGrids[row][column].GetAvailableOptions();
                    if (BitUtilities.OnlyOption(values, out option))
                    {
                        onlyOptionFound = true;
                        int arrayIndex = BitUtilities.ContainingBitIndex(values, option);           // Index within array where only option found                     
                        Set(column, row, arrayIndex % rows, arrayIndex / rows, option);
                    }
                }
            }

            return onlyOptionFound;
        }

        private List<Option> RemoveOptionFromOtherColumns(int subGridColumn, int subGridRow, int cellColumn, int option)
        {
            // Check options removed from other columns (n - 1) columns must have the options removed i.e. option must exist in only 1 columns
            List<Option> lastOptions = new List<Option>();

            int totalExistingColumns = 0;
            int totalExistingRows = 0;

            int existingColumn = -1;
            int column = rows;                                                                      // Use SubGrid's number of columns i.e. swopped rows
            while (totalExistingColumns < 2 && --column > cellColumn)
                if (subGrids[subGridRow][subGridColumn].OptionExistsInColumn(column, option))
                {
                    existingColumn = column;
                    totalExistingColumns++;
                }
            while (totalExistingColumns < 2 && column-- > 0)
                if (subGrids[subGridRow][subGridColumn].OptionExistsInColumn(column, option))
                {
                    existingColumn = column;
                    totalExistingColumns++;
                }

            if (totalExistingColumns == 1)
                lastOptions = RemoveOptionsFromColumn(subGridColumn, subGridRow, existingColumn, option);
            else
            {
                // Check other sub grids in same column
                int existingRow = -1;
                int row = rows;
                while (totalExistingRows < 2 && --row > subGridRow)
                    if (subGrids[row][subGridColumn].OptionExistsInColumn(cellColumn, option))
                    {
                        existingRow = row;
                        totalExistingRows++;
                    }
                while (totalExistingRows < 2 && row-- > 0)
                    if (subGrids[row][subGridColumn].OptionExistsInColumn(cellColumn, option))
                    {
                        existingRow = row;
                        totalExistingRows++;
                    }

                if (totalExistingRows == 1)
                    lastOptions = subGrids[existingRow][subGridColumn].RemoveOptionsExceptFromColumn(cellColumn, option);
            }

            return lastOptions;
        }

        private List<Option> RemoveOptionFromOtherRows(int subGridColumn, int subGridRow, int cellRow, int option)
        {
            // Check options removed from other rows (n - 1) rows must have the options removed i.e. option must exist in only 1 row
            List<Option> lastOptions = new List<Option>();

            int totalExistingColumns = 0;
            int totalExistingRows = 0;

            int existingRow = -1;
            int row = columns;                                                                      // Use SubGrid's number of rows i.e. swopped columns
            while (totalExistingRows < 2 && --row > cellRow)
                if (subGrids[subGridRow][subGridColumn].OptionExistsInRow(row, option))
                {
                    existingRow = row;
                    totalExistingRows++;
                }
            while (totalExistingRows < 2 && row-- > 0)
                if (subGrids[subGridRow][subGridColumn].OptionExistsInRow(row, option))
                {
                    existingRow = row;
                    totalExistingRows++;
                }

            if (totalExistingRows == 1)
                lastOptions = RemoveOptionsFromRow(subGridColumn, subGridRow, existingRow, option);
            else
            {
                // Check other sub grids in same row
                int existingColumn = -1;
                int column = columns;
                while (totalExistingColumns < 2 && --column > subGridColumn)
                    if (subGrids[subGridRow][column].OptionExistsInRow(cellRow, option))
                    {
                        existingColumn = column;
                        totalExistingColumns++;
                    }
                while (totalExistingColumns < 2 && column-- > 0)
                    if (subGrids[subGridRow][column].OptionExistsInRow(cellRow, option))
                    {
                        existingColumn = column;
                        totalExistingColumns++;
                    }

                if (totalExistingColumns == 1)
                    lastOptions = subGrids[subGridRow][existingColumn].RemoveOptionsExceptFromRow(cellRow, option);
            }

            return lastOptions;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        // Convert sub grids to coluns * rows matrix
        ////////////////////////////////////////////////////////////////////////////////////////////

        private int[][] GetAvailableOptionsMatrix()                                                 // Get state of current grid - returned as an n*m matrix (not separated by sub grids)
        {
            int subGridRow = rows;
            int matrixRow = subGridRow * columns;                                                   // Use SubGrid's number of rows i.e. swopped columns
            int[][] matrix = new int[matrixRow][];

            int subGridColumns = columns;
            int matrixColumns = subGridColumns * rows;                                              // Use SubGrid's number of columns i.e. swopped rows
            while (matrixRow-- > 0)
                matrix[matrixRow] = new int[matrixColumns];

            while (subGridRow-- > 0)
            {
                int subGridColumn = subGridColumns;
                while (subGridColumn-- > 0)
                {
                    var subMatrix = subGrids[subGridRow][subGridColumn].GetAvailableOptionsMatrix();

                    int cellColumn = rows;
                    while (cellColumn-- > 0)
                    {
                        int cellRow = columns;
                        while (cellRow-- > 0)
                            matrix[subGridRow * columns + cellRow][subGridColumn * rows + cellColumn] = subMatrix[cellRow][cellColumn];
                    }
                }
            }

            return matrix;
        }

        private int[][] GetTransposedAvailableOptionsMatrix()                                       // Get state of current grid - returned as a transposed n*m matrix (not separated by sub grids)
        {
            int subGridColumn = columns;
            int matrixColumn = subGridColumn * rows;                                                // Use SubGrid's number of rows i.e. swopped columns
            int[][] matrix = new int[matrixColumn][];

            int subGridRows = rows;
            int matrixRows = subGridRows * columns;
            while (matrixColumn-- > 0)
                matrix[matrixColumn] = new int[matrixRows];

            while (subGridColumn-- > 0)
            {
                int subGridRow = subGridRows;
                while (subGridRow-- > 0)
                {
                    var subMatrix = subGrids[subGridRow][subGridColumn].GetAvailableOptionsMatrix();

                    int cellRow = columns;
                    while (cellRow-- > 0)
                    {
                        int cellColumn = rows;
                        while (cellColumn-- > 0)
                            matrix[subGridColumn * rows + cellColumn][subGridRow * columns + cellRow] = subMatrix[cellRow][cellColumn];
                    }
                }
            }

            return matrix;
        }

        private Cell[][] GetCellsMatrix()                                                           // Get cells in current grid - returned as an n*m matrix (not separated by sub grids)
        {
            int subGridRow = rows;
            int matrixRow = subGridRow * columns;                                                   // Use SubGrid's number of rows i.e. swopped columns
            Cell[][] matrix = new Cell[matrixRow][];

            int subGridColumns = columns;
            int matrixColumns = subGridColumns * rows;                                              // Use SubGrid's number of columns i.e. swopped rows
            while (matrixRow-- > 0)
                matrix[matrixRow] = new Cell[matrixColumns];

            while (subGridRow-- > 0)
            {
                int subGridColumn = subGridColumns;
                while (subGridColumn-- > 0)
                {
                    var subMatrix = subGrids[subGridRow][subGridColumn].GetCellsMatrix();

                    int cellColumn = rows;
                    while (cellColumn-- > 0)
                    {
                        int cellRow = columns;
                        while (cellRow-- > 0)
                            matrix[subGridRow * columns + cellRow][subGridColumn * rows + cellColumn] = subMatrix[cellRow][cellColumn];
                    }
                }
            }

            return matrix;
        }

        private Cell[][] GetTransposedCellsMatrix()                                                 // Get state of current grid - returned as a transposed n*m matrix (not separated by sub grids)
        {
            int subGridColumn = columns;
            int matrixColumn = subGridColumn * rows;                                                // Use SubGrid's number of rows i.e. swopped columns
            Cell[][] matrix = new Cell[matrixColumn][];

            int subGridRows = rows;
            int matrixRows = subGridRows * columns;
            while (matrixColumn-- > 0)
                matrix[matrixColumn] = new Cell[matrixRows];

            while (subGridColumn-- > 0)
            {
                int subGridRow = subGridRows;
                while (subGridRow-- > 0)
                {
                    var subMatrix = subGrids[subGridRow][subGridColumn].GetCellsMatrix();

                    int cellRow = columns;
                    while (cellRow-- > 0)
                    {
                        int cellColumn = rows;
                        while (cellColumn-- > 0)
                            matrix[subGridColumn * rows + cellColumn][subGridRow * columns + cellRow] = subMatrix[cellRow][cellColumn];
                    }
                }
            }

            return matrix;
        }
    }
}
