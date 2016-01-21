namespace Sudoku.Model
{
    public class GridBase
    {

        protected int columns;
        protected int rows;

        protected GridBase[][] cells;                                                               // Jagged array used for efficiency ???

        public GridBase(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
        }

        public GridBase this[int column, int row]                                                   // grids called by column, row but accessed by row, column for efficiency
        {
            get { return cells[row][column]; }
            set { cells[row][column] = value; }
        }

        public virtual void RemoveOption(int column, int row)
        {
            throw new System.NotImplementedException();
        }

        internal virtual void OnLastOption(EventArgs.LastOptionEventArgs e)
        {
//            if (LastOption != null)
//                LastOption(this, e);
        }

    }
}
