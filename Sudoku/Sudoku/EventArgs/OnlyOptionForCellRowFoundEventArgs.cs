namespace Sudoku.EventArgs
{
    delegate void OnlyOptionForCellRowFoundEventHandler(object sender, OnlyOptionForCellRowFoundEventArgs e);

    class OnlyOptionForCellRowFoundEventArgs : System.EventArgs
    {
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellRow { get; set; }
        public int Option { get; set; }

        public OnlyOptionForCellRowFoundEventArgs(int subGridColumn, int subGridRow, int cellRow, int option)
        {
            SubGridColumn = subGridColumn;
            SubGridRow = subGridRow;
            CellRow = cellRow;
            Option = option;
        }
    }
}
