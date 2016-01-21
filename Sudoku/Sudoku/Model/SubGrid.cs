using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model
{
    public class SubGrid
    {
        private int columns;
        private int rows;

        private Cell[][] cells;                                                                     // Jagged array used for efficiency ???

        public int Column { get; set; }
        public int Row { get; set; }        

        public SubGrid(int columns, int rows, int column, int row)
        {
            this.columns = rows;                                                                    // Swop columns and rows values
            this.rows = columns;
            Column = column;
            Row = row;

            cells = new Cell[this.rows][];
            for (row = 0; row < this.rows; row++)
            {
                cells[row] = new Cell[this.columns];
                for (column = 0; column < this.columns; column++)
                    cells[row][column] = new Cell(columns, rows, column, row);
            }
        }

        public Cell this[int column, int row]
        {
            get { return cells[row][column]; }                                                      // grids called by [column, row] but accessed by [row][column] for efficiency
            set { cells[row][column] = value; }
        }

        public void Reset()
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    cells[row][column].Reset();
        }

        public bool Solved()
        {
            bool solved = true;

            int row = rows;
            while (solved && row-- > 0)
            {
                int column = columns;
                while (solved && column-- > 0)
                    solved = cells[row][column].Solved;
            }

            return solved;
        }

        public int[][] GetAvailableOptionsMatrix()
        {
            int[][] matrix = new int[rows][];

            int row = rows;
            while (row-- > 0)
            {
                matrix[row] = new int[columns];
                int column = columns;
                while (column-- > 0)
                    matrix[row][column] = cells[row][column].AvailableOptions;
            }

            return matrix;
        }

        public Cell[][] GetCellsMatrix()
        {
            Cell[][] matrix = new Cell[rows][];

            int row = rows;
            while (row-- > 0)
            {
                matrix[row] = new Cell[columns];
                int column = columns;
                while (column-- > 0)
                    matrix[row][column] = cells[row][column];
            }

            return matrix;
        }

        public IList<Cell> GetUnsetCells()
        {
            IList<Cell> unsetCells = new List<Cell>();

            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    if (!cells[row][column].IsSet)
                        unsetCells.Add(cells[row][column]);

            return unsetCells;
        }

        public IEnumerable<Cell> UnsetCells(int totalUnsetOptions)
        {
            return GetUnsetCells().Where(x => x.TotalOptionsRemaining == totalUnsetOptions);
        }

        public int[] GetAvailableOptions()
        {
            int[] array = new int[columns * rows];

            int row = rows;
            while (row-- > 0)
            {
                int column = columns;
                while (column-- > 0)
                    array[row * columns + column] = cells[row][column].AvailableOptions;
            }

            return array;
        }

        // Remove option from all other cells in this sub grid - return array of last options found and options removed from all columns / rows in the sub grid
        public List<Option> StrikeOutCell(int cellColumn, int cellRow, int option, out List<Option> removedOptionsFromColumn, out List<Option> removedOptionsFromRow)
        {
            List<Option> lastOptions = new List<Option>();
            removedOptionsFromColumn = new List<Option>();
            removedOptionsFromRow = new List<Option>();

            int column;
            int row = rows;
            while (--row > cellRow)
            {
                column = columns;
                while (column-- > 0)
                    if (cells[row][column].RemoveOption(option))
                        lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
                    else
                    {
                        if (OptionRemovedFromColumn(column, row, option))
                            removedOptionsFromColumn.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, Bits = option });
                        if (OptionRemovedFromRow(column, row, option))
                            removedOptionsFromRow.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellRow = row, Bits = option });
                    }
            }

            column = columns;
            while (--column > cellColumn)
                if (cells[row][column].RemoveOption(option))
                    lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
                else
                {
                    if (OptionRemovedFromColumn(column, row, option))
                        removedOptionsFromColumn.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, Bits = option });
                    if (OptionRemovedFromRow(column, row, option))
                        removedOptionsFromRow.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellRow = row, Bits = option });
                }

            while (column-- > 0)
                if (cells[row][column].RemoveOption(option))
                    lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
                else
                {
                    if (OptionRemovedFromColumn(column, row, option))
                        removedOptionsFromColumn.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, Bits = option });
                    if (OptionRemovedFromRow(column, row, option))
                        removedOptionsFromRow.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellRow = row, Bits = option });
                }

            while (row-- > 0)
            {
                column = columns;
                while (column-- > 0)
                    if (cells[row][column].RemoveOption(option))
                        lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
                    else
                    {
                        if (OptionRemovedFromColumn(column, row, option))
                            removedOptionsFromColumn.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, Bits = option });
                        if (OptionRemovedFromRow(column, row, option))
                            removedOptionsFromRow.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellRow = row, Bits = option });
                    }
            }

            return lastOptions;
        }

        public List<Option> RemoveOptionsFromColumn(int cellColumn, int options)
        {
            List<Option> lastOptions = new List<Option>();

            for (int row = 0; row < rows; row++)
                if (cells[row][cellColumn].RemoveOptions(options))
                    lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = cellColumn, CellRow = row, Bits = cells[row][cellColumn].Options });

            return lastOptions;
        }

        public List<Option> RemoveOptionsFromRow(int cellRow, int options)
        {
            List<Option> lastOptions = new List<Option>();

            for (int column = 0; column < columns; column++)
                if (cells[cellRow][column].RemoveOptions(options))
                    lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = cellRow, Bits = cells[cellRow][column].Options });

            return lastOptions;
        }

        public List<Option> RemoveOptionsExceptFromColumn(int excludeColumn, int options)
        {
            List<Option> lastOptions = new List<Option>();

            int row;
            int column = columns;
            while (--column > excludeColumn)
            {
                row = rows;
                while (row-- > 0)
                    if (cells[row][column].RemoveOptions(options))
                        lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
            }

            while (column-- > 0)
            {
                row = rows;
                while (row-- > 0)
                    if (cells[row][column].RemoveOptions(options))
                        lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
            }

            return lastOptions;
        }

        public List<Option> RemoveOptionsExceptFromRow(int excludeRow, int options)
        {
            List<Option> lastOptions = new List<Option>();

            int column;
            int row = rows;
            while (--row > excludeRow)
            {
                column = columns;
                while (column-- > 0)
                    if (cells[row][column].RemoveOptions(options))
                        lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
            }

            while (row-- > 0)
            {
                column = columns;
                while (column-- > 0)
                    if (cells[row][column].RemoveOptions(options))
                        lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });
            }

            return lastOptions;
        }

        public List<Option> RemoveIfExtraOptionsFromColumn(int column, int options)
        {
            List<Option> lastOptions = new List<Option>();

            for (int row = 0; row < rows; row++)
                if (cells[row][column].RemoveIfExtraOptions(options))
                    lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });

            return lastOptions;
        }

        public List<Option> RemoveIfExtraOptionsFromRow(int row, int options)
        {
            List<Option> lastOptions = new List<Option>();

            for (int column = 0; column < columns; column++)
                if (cells[row][column].RemoveIfExtraOptions(options))
                    lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });

            return lastOptions;
        }

        public List<Option> RemoveIfExtraOptions(int options)
        {
            List<Option> lastOptions = new List<Option>();

            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    if (cells[row][column].RemoveIfExtraOptions(options))
                        lastOptions.Add(new Option { SubGridColumn = Column, SubGridRow = Row, CellColumn = column, CellRow = row, Bits = cells[row][column].Options });

            return lastOptions;
        }

        public bool OptionExistsInColumn(int column, int option)
        {
            bool found = false;
            int row = rows;
            while (!found && row-- > 0)
                found = cells[row][column].ContainsOption(option);

            return found;
        }

        public bool OptionExistsInRow(int row, int option)
        {
            bool found = false;
            int column = columns;
            while (!found && column-- > 0)
                found = cells[row][column].ContainsOption(option);

            return found;
        }

        public bool OptionRemovedFromColumn(int cellColumn, int cellRow, int option)
        {
            // Check if option removed from column
            bool optionFound = false;

            int row = rows;
            while (!optionFound && --row > cellRow)
                optionFound = (cells[row][cellColumn].Options & option) > 0;
            while (!optionFound && row-- > 0)
                optionFound = (cells[row][cellColumn].Options & option) > 0;

            return !optionFound;                                                                    // If option not found then it was removed from this sub grid's column
        }

        public bool OptionRemovedFromRow(int cellColumn, int cellRow, int removedOption)
        {
            // Check if option removed from row
            bool optionFound = false;
            int column = columns;
            while (!optionFound && --column > cellColumn)
                optionFound = (cells[cellRow][column].Options & removedOption) > 0;
            while (!optionFound && column-- > 0)
                optionFound = (cells[cellRow][column].Options & removedOption) > 0;

            return !optionFound;                                                                    // If option not found then it was removed from this sub grid's row
        }

        public bool OptionsOnlyInRow()                                                              // Check sub-grid for unset rows that contain only remaining options i.e. those options do not exist in the other rows
        {
            for (int row = 0; row < rows; row++)
                ;

            return false;
        }

        public void SetCells(Cell[][] subGrid)
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    cells[row][column] = subGrid[row][column];
        }
    }
}
