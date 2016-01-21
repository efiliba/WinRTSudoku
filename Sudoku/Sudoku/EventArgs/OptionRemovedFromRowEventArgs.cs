namespace Sudoku.EventArgs
{
    delegate void OptionRemovedFromRowEventHandler(object sender, OptionRemovedFromRowEventArgs e);

    class OptionRemovedFromRowEventArgs : System.EventArgs
    {
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellRow { get; set; }
        public int RemovedOption { get; set; }

        public OptionRemovedFromRowEventArgs(int subGridColumn, int subGridRow, int cellRow, int removedOption)
        {
            SubGridColumn = subGridColumn;
            SubGridRow = subGridRow;
            CellRow = cellRow;
            RemovedOption = removedOption;
        }
    }
}
