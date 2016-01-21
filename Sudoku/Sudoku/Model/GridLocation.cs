namespace Sudoku.Model
{
    public struct GridLocation
    {
        public int Column;
        public int Row;

        public GridLocation(int columns, int rows)
        {
            Column = columns;
            Row = rows;
        }
    }
}
