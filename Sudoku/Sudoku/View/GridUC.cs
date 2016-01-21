namespace Sudoku.View
{
    public class GridUC : SudokuGridBase
    {
        public GridUC(int columns, int rows) : base(columns, rows)
        {
            margin = new Windows.UI.Xaml.Thickness(2);
            background.Color = Windows.UI.Colors.Black;

            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    childElements[row][column] = new SubGridUC(columns, rows, column, row);

            AddChildren(true);
        }

        public SubGridUC this[int column, int row]
        {
            get { return childElements[row][column] as SubGridUC; }
        }

        public void Draw(Model.Grid grid)
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    (childElements[row][column] as SubGridUC).Draw(grid[column, row]);
        }

        public void ResetCells()
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    (childElements[row][column] as SubGridUC).ResetCells();
        }
    }
}