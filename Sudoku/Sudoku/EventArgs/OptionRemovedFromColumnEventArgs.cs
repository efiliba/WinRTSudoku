namespace Sudoku.EventArgs
{
    delegate void OptionRemovedFromColumnEventHandler(object sender, OptionRemovedFromColumnEventArgs e);

    class OptionRemovedFromColumnEventArgs : System.EventArgs
    {
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellColumn { get; set; }
        public int RemovedOption { get; set; }

        public OptionRemovedFromColumnEventArgs(int subGridColumn, int subGridRow, int cellColumn, int removedOption)
        {
            SubGridColumn = subGridColumn;
            SubGridRow = subGridRow;
            CellColumn = cellColumn;
            RemovedOption = removedOption;
        }
    }
}