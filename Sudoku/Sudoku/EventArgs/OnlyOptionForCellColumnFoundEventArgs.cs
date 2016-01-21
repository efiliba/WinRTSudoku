namespace Sudoku.EventArgs
{
    delegate void OnlyOptionForCellColumnFoundEventHandler(object sender, OnlyOptionForCellColumnFoundEventArgs e);

    class OnlyOptionForCellColumnFoundEventArgs : System.EventArgs
    {
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellColumn { get; set; }
        public int Option { get; set; }

        public OnlyOptionForCellColumnFoundEventArgs(int subGridColumn, int subGridRow, int cellColumn, int option)
        {
            SubGridColumn = subGridColumn;
            SubGridRow = subGridRow;
            CellColumn = cellColumn;
            Option = option;
        }
    }
}
