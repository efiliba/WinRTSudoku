namespace Sudoku.EventArgs
{
    public delegate void CellClickedEventHandler(object sender, CellClickedEventArgs e);
    
    public class CellClickedEventArgs : System.EventArgs
    {
        public int GridColumn { get; set; }
        public int GridRow { get; set; }
        public int SubGridColumn { get; set; }
        public int SubGridRow { get; set; }
        public int CellColumn { get; set; }
        public int CellRow { get; set; }
        public string Symbol { get; set; }

        public CellClickedEventArgs(Model.GridLocation grid, Model.GridLocation subGrid, Model.GridLocation? cell = null, string symbol = null)
        {
            GridColumn = grid.Column;
            GridRow = grid.Row;
            SubGridColumn = subGrid.Column;
            SubGridRow = subGrid.Row;
            
            if (cell != null)
            {
                CellColumn = ((Model.GridLocation)cell).Column;
                CellRow = ((Model.GridLocation)cell).Row;
            }
            
            Symbol = symbol;
        }
    }
}