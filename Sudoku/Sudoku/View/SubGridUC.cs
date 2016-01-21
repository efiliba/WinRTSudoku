namespace Sudoku.View
{
    public class SubGridUC : SudokuGridBase
    {
        public SubGridUC(int columns, int rows, int subGridColumn, int subGridRow) : base(rows, columns)// Swop columns and rows values
        {
            margin = new Windows.UI.Xaml.Thickness(1);
            background.Color = Windows.UI.Colors.Gray;

            for (int row = 0; row < this.rows; row++)
                for (int column = 0; column < this.columns; column++)
                    childElements[row][column] = new CellUC(columns, rows, subGridColumn, subGridRow, column, row);

            AddChildren();
        }

        public CellUC this[int column, int row]
        {
            get { return childElements[row][column] as CellUC; }
        }

        public void Draw(Model.SubGrid subGrid)
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    (childElements[row][column] as CellUC).Draw(subGrid[column, row]);
        }

        public void ResetCells()
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    (childElements[row][column] as CellUC).ResetCells();
        }
    }
}
