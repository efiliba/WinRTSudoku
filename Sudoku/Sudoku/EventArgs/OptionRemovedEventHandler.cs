namespace Sudoku.EventArgs
{
    delegate void OptionRemovedEventHandler(object sender, OptionRemovedEventArgs e);

    public class OptionRemovedEventArgs : System.EventArgs
    {
        public int CellColumn { get; set; }
        public int CellRow { get; set; }
        public int RemovedOption { get; set; }
        public int RemainingOptionsFlag { get; set; }
        public int TotalOptionsRemaining { get; set; }

        public OptionRemovedEventArgs(int cellColumn, int cellRow, int removedOption, int remainingOptionsFlag, int totalOptionsRemaining)
        {
            CellColumn = cellColumn;
            CellRow = cellRow;
            RemovedOption = removedOption;
            RemainingOptionsFlag = remainingOptionsFlag;
            TotalOptionsRemaining = totalOptionsRemaining;
        }
    }
}
