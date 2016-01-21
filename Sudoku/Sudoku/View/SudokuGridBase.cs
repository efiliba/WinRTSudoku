using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Sudoku.View
{
    public class SudokuGridBase : Grid
    {
        public event EventArgs.CellClickedEventHandler CellClicked;
        public event EventArgs.CellClickedEventHandler CellRightClicked;
        public event EventArgs.CellClickedEventHandler ResetCellClicked;

        protected int columns;
        protected int rows;

        protected Thickness margin;
        protected SolidColorBrush background;
        protected UIElement[][] childElements;

        public SudokuGridBase(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
            background = new SolidColorBrush();
            childElements = new UIElement[rows][];

            for (int row = 0; row < rows; row++)
            {
                childElements[row] = new UIElement[columns];
                this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int column = 0; column < columns; column++)
                this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        protected void AddChildren(bool addEventHandler = false)
        {
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                {
                    Border border = new Border
                    {
                        Margin = margin,
                        Background = background,
                        Child = childElements[row][column],
                        Tag = new Model.GridLocation(column, row)
                    };

                    if (addEventHandler)
                    {
                        border.Tapped += border_Tapped;
                        border.RightTapped += border_RightTapped;
                    }

                    this.Children.Add(border);
                    Grid.SetColumn(border, column);
                    Grid.SetRow(border, row);
                }
        }

        private void border_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Border cell = e.OriginalSource as Border ?? (e.OriginalSource as TextBlock).Parent as Border;
            if (cell.Parent is CellUC && cell.Tag != null)                                          // Check clicks on grid line and set cells
            {
                Border subGrid = (cell.Parent as CellUC).Parent as Border;
                Border grid = (subGrid.Parent as SubGridUC).Parent as Border;

                string symbol = (cell.Child as TextBlock ?? (cell.Child as Canvas).Children[1] as TextBlock).Text;
                OnCellClicked(new EventArgs.CellClickedEventArgs((Model.GridLocation)grid.Tag, (Model.GridLocation)subGrid.Tag, (Model.GridLocation)cell.Tag, symbol));
            }
            else if (cell.Parent is Border && (cell.Parent as Border).Parent is CellUC)
            {
                Border subGrid = ((cell.Parent as Border).Parent as CellUC).Parent as Border;
                Border grid = (subGrid.Parent as SubGridUC).Parent as Border;

                OnResetCellClicked(new EventArgs.CellClickedEventArgs((Model.GridLocation)grid.Tag, (Model.GridLocation)subGrid.Tag));
            }
        }

        private void border_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            Border cell = e.OriginalSource as Border ?? (e.OriginalSource as TextBlock).Parent as Border;
            if (cell.Parent is CellUC && cell.Tag != null)                                          // Ignor clicks on grid line and set cells
            {
                Border subGrid = (cell.Parent as CellUC).Parent as Border;
                Border grid = (subGrid.Parent as SubGridUC).Parent as Border;
                string symbol = (cell.Child as TextBlock ?? (cell.Child as Canvas).Children[1] as TextBlock).Text;

                OnCellRightClicked(new EventArgs.CellClickedEventArgs((Model.GridLocation)grid.Tag, (Model.GridLocation)subGrid.Tag, (Model.GridLocation)cell.Tag, symbol));
            }
        }

        private void OnCellClicked(EventArgs.CellClickedEventArgs e)
        {
            if (CellClicked != null)
                CellClicked(this, e);
        }

        private void OnCellRightClicked(EventArgs.CellClickedEventArgs e)
        {
            if (CellRightClicked != null)
                CellRightClicked(this, e);
        }

        private void OnResetCellClicked(EventArgs.CellClickedEventArgs e)
        {
            if (ResetCellClicked != null)
                ResetCellClicked(this, e);
        }
    }
}