namespace Sudoku.EventArgs
{
    delegate void LastOptionEventHandler(object sender, LastOptionEventArgs e);

    public class LastOptionEventArgs : System.EventArgs
    {
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellColumn { get; set; }
        public int CellRow { get; set; }
        public int OptionColumn { get; set; }
        public int OptionRow { get; set; }

        public LastOptionEventArgs(int cellColumn, int cellRow, int optionColumn, int optionRow)
        {
            CellColumn = cellColumn;
            CellRow = cellRow;
            OptionColumn = optionColumn;
            OptionRow = optionRow;
        }
    }
}
