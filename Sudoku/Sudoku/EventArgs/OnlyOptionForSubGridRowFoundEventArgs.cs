namespace Sudoku.EventArgs
{
    delegate void OnlyOptionForSubGridRowFoundEventHandler(object sender, OnlyOptionForSubGridRowFoundEventArgs e);

    class OnlyOptionForSubGridRowFoundEventArgs : System.EventArgs
    {
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellRow { get; set; }
        public int Option { get; set; }

        public OnlyOptionForSubGridRowFoundEventArgs(int subGridColumn, int subGridRow, int cellRow, int option)
        {
            SubGridColumn = subGridColumn;
            SubGridRow = subGridRow;
            CellRow = cellRow;
            Option = option;
        }
    }
}
