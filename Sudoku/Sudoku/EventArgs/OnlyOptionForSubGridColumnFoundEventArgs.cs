namespace Sudoku.EventArgs
{
    delegate void OnlyOptionForSubGridColumnFoundEventHandler(object sender, OnlyOptionForSubGridColumnFoundEventArgs e);

    class OnlyOptionForSubGridColumnFoundEventArgs : System.EventArgs
    {
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellColumn { get; set; }
        public int Option { get; set; }

        public OnlyOptionForSubGridColumnFoundEventArgs(int subGridColumn, int subGridRow, int cellColumn, int option)
        {
            SubGridColumn = subGridColumn;
            SubGridRow = subGridRow;
            CellColumn = cellColumn;
            Option = option;
        }
    }
}
