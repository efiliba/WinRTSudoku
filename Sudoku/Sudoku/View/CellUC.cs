using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace Sudoku.View
{
    public class CellUC : SudokuGridBase
    {
        private static Color[] symbolColours;
        private bool isSet;                                                                         // Track if this cell has already been set
        
        static CellUC()
        {
            symbolColours = new Color[] { Colors.Gold, Colors.Blue, Colors.Brown, Colors.Teal, Colors.DarkGreen, Colors.Magenta, Colors.Lime, Colors.Indigo, Colors.Crimson };
        }

        public CellUC(int columns, int rows, int subGridColumn, int subGridRow, int cellColumn, int cellRow) : base(columns, rows)
        {
            isSet = false;
            byte colourValue = (byte)((10 + (subGridColumn + subGridRow) % 2 + (cellColumn + cellRow) % 2 * 3) << 4);   // Add 1 and 3 to base index and multiply by 16

            margin = new Windows.UI.Xaml.Thickness(0.5);
            background.Color = Color.FromArgb(255, colourValue, colourValue, colourValue);

            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    childElements[row][column] = new TextBlock
                    {
                        Text = (row * columns + column + 1).ToString(),
                        FontSize = 20,
                        Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Black),
                        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center
                    };

            AddChildren();
        }

        public TextBlock this[int column, int row]
        {
            get { return childElements[row][column] as TextBlock; }
        }

        public void StrikeOut(int column, int row)
        {
            Border border = (childElements[row][column] as TextBlock).Parent as Border;
            if (border != null)                                                                     // Not a Border i.e. already stiked out
            {
                Canvas canvas = new Canvas();
                canvas.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = border.ActualHeight,
                    X2 = border.ActualWidth,
                    Y2 = 0,
                    Stroke = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Red),
                    StrokeThickness = 1,
                    IsTapEnabled = false
                });

                border.Child = canvas;
                canvas.Children.Add(childElements[row][column]);
            }
        }

        public void ResetCells()
        {
            if (isSet)
            {
                isSet = false;
                this.Children.RemoveAt(columns * rows);
            }

            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                {
                    int index = row * columns + column;
                    Border border = this.Children[index] as Border;
                    if (border.Child is Canvas)
                    {
                        (border.Child as Canvas).Children.Clear();                                  // Clear items in the canvas
                        border.Child = childElements[row][column];                                  // Replace the canvas with the corresponding TextBlock
                    }

                    (border.Child as TextBlock).Text = (index + 1).ToString();
                }
        }

        public void Clear(int column, int row)
        {
            (childElements[row][column] as TextBlock).Text = "";
        }

        internal void Draw(Model.Cell cell)
        {
            if (cell.IsSet)
                SetOption(cell.Symbol, symbolColours[(cell.SetRow * columns + cell.SetColumn) % symbolColours.Length], cell.IsFixed);
            else
                for (int row = 0; row < rows; row++)
                    foreach (int column in cell.RemovedOptionsPerRow(row))
                        Clear(column, row);
        }

        private void SetOption(string symbol, Color colour, bool cellFixed = false)
        {
            if (!isSet)
            {
                isSet = true;
                Border border = new Border
                {
                    Margin = margin,
                    Background = background,
                    Child = cellFixed ? CreateHighLightedSymbol(symbol, colour) : CreateSymbol(symbol, colour)
                };

                this.Children.Add(border);
                Grid.SetColumnSpan(border, columns);
                Grid.SetRowSpan(border, rows);
            }
        }

        private Windows.UI.Xaml.UIElement CreateHighLightedSymbol(string symbol, Color colour)
        {
            return new Border
            {
                CornerRadius = new Windows.UI.Xaml.CornerRadius(40),
                BorderThickness = new Windows.UI.Xaml.Thickness(1),
                BorderBrush = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Red),
                Background = new Windows.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 96, 96, 96)),
                Padding = new Windows.UI.Xaml.Thickness(2, 5, 2, 2),
                Width = 80,
                Height = 80,
                Child = CreateSymbol(symbol, colour)
            };
        }

        private Windows.UI.Xaml.UIElement CreateSymbol(string symbol, Color colour)
        {
            return new TextBlock
            {
                Text = symbol,
                FontSize = 40,
                Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(colour),
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center
            };
        }
    }
}
