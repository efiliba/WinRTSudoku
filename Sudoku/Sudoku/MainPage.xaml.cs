using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Sudoku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int COLUMNS = 3;
        private const int ROWS = 3;

        private View.GridUC gridView;
        private Model.Grid grid;

        public MainPage()
        {
            this.InitializeComponent();

            gridView = new View.GridUC(COLUMNS, ROWS);
            gridView.CellClicked += grid_CellClicked;
            gridView.CellRightClicked += gridView_CellRightClicked;
            gridView.ResetCellClicked += gridView_ResetCellClicked;

            grid = new Model.Grid(COLUMNS, ROWS);

            //Test6x6();
            //Hard6x6();
            //Easy4x4();
            //Easy9x9();
            //ExtraHard9x9();
            //Diabolical9x9();
            //Test6x6b();

 //           ReadFile();

            grid.Reset();
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
//                grid.Fix("000000012400090000000000050070200000600000400000108000018000000000030700502000000");
                grid.Solve();

                bool solved = grid.Solve();
            }
            double elapsedTime = DateTime.Now.Subtract(startTime).TotalSeconds;


            //int i = grid.Fix(0, 0, 0, 0, 1);
            //i = grid.Fix(0, 0, 1, 0, 2);
            //i = grid.Fix(0, 0, 0, 1, 4);
            
            //i = grid.Fix(1, 0, 0, 0, 4);
            //i = grid.Fix(1, 0, 0, 1, 1);
            
            //i = grid.Fix(0, 1, 0, 0, 2);
            //i = grid.Fix(0, 1, 1, 0, 1);



            Windows.UI.Core.CoreWindow.GetForCurrentThread().KeyDown += MainPage_KeyDown;

            this.Loaded += MainPage_Loaded;                                                         // Update display after load - display removed options
        }

        private async void ReadFile()
        {
            Windows.Storage.StorageFolder localFolder = Windows.ApplicationModel.Package.Current.InstalledLocation; // Read data - 
            Windows.Storage.StorageFile file = await localFolder.GetFileAsync("Sudoku Puzzles.txt");

            string puzzles = await Windows.Storage.FileIO.ReadTextAsync(file);
            
            int totalSolved = 0;
            int totalUnsolved = 0;
            DateTime startTime = DateTime.Now;
            foreach (string puzzle in puzzles.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                grid.Reset();
                grid.Fix(puzzle);
                if (grid.Solve())
                    totalSolved++;
                else
                    totalUnsolved++;

                if (totalSolved > 1000)
                    break;
            }
            double elapsedTime = DateTime.Now.Subtract(startTime).TotalSeconds;
        }

        private void MainPage_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            switch (e.VirtualKey)
            {
                case Windows.System.VirtualKey.Escape:                                              // Exit
                    Application.Current.Exit();
                    break;
                case Windows.System.VirtualKey.C:                                                   // Clear
                    gridView.ResetCells();
                    grid.Reset();
                    gridView.Draw(grid);
                    break;
            }
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            gridView.Draw(grid);
        }

        private void Easy4x4()
        {
            grid.Fix(0, 0, 1, 0, 1);
            grid.Fix(1, 0, 0, 0, 2);
            grid.Fix(0, 1, 0, 0, 4);
            grid.Fix(1, 1, 1, 0, 1);
        }

        private void Hard6x6()
        {
            grid.Fix(0, 0, 0, 1, 1);

            grid.Fix(0, 0, 2, 0, 32);

            grid.Fix(0, 1, 0, 0, 4);
            grid.Fix(0, 1, 0, 1, 16);

            grid.Fix(0, 2, 1, 0, 16);
            grid.Fix(0, 2, 1, 1, 8);

            grid.Fix(1, 0, 1, 0, 4);
            grid.Fix(1, 0, 1, 1, 2);

            grid.Fix(1, 1, 2, 0, 32);
            grid.Fix(1, 1, 2, 1, 2);

            grid.Fix(1, 2, 0, 1, 2);
            grid.Fix(1, 2, 2, 0, 4);            
        }

        private void Test6x6()
        {
            grid.Fix(0, 0, 0, 0, 1);
            grid.Fix(0, 0, 1, 0, 2);
            grid.Fix(0, 0, 2, 0, 4);

            grid.Fix(0, 1, 0, 1, 2);

            grid.Fix(0, 2, 1, 0, 1);
            grid.Fix(0, 2, 2, 1, 2);



            grid.Fix(1, 0, 1, 1, 2);


            grid.Fix(1, 1, 0, 0, 4);

            grid.Fix(1, 1, 1, 1, 16);


            grid.Fix(1, 2, 2, 0, 8);
//            grid.Fix(1, 2, 2, 1, 16);
        }

        private void Test6x6b()
        {
            grid.Fix(0, 0, 0, 0, 1);
            grid.Fix(0, 0, 1, 0, 2);

            grid.Fix(1, 0, 0, 0, 4);
            grid.Fix(1, 0, 1, 0, 8);

            grid.Fix(2, 0, 0, 0, 16);

            grid.Fix(0, 0, 0, 1, 4);
//            grid.Fix(0, 0, 1, 1, 8);
        }

        private void Easy9x9()
        {
            grid.Fix(0, 0, 0, 2, 256);
            grid.Fix(0, 0, 1, 0, 2);
            grid.Fix(0, 0, 2, 0, 128);
                         
            grid.Fix(0, 1, 0, 0, 2);
            grid.Fix(0, 1, 1, 0, 32);
            grid.Fix(0, 1, 2, 1, 64);
            grid.Fix(0, 1, 2, 2, 4);
                         
            grid.Fix(0, 2, 0, 1, 64);
            grid.Fix(0, 2, 1, 0, 8);
            grid.Fix(0, 2, 2, 2, 32);
                         
            grid.Fix(1, 0, 0, 1, 16);
            grid.Fix(1, 0, 2, 0, 256);
            grid.Fix(1, 0, 2, 2, 32);
                         
            grid.Fix(1, 1, 0, 1, 8);
            grid.Fix(1, 1, 1, 0, 16);
            grid.Fix(1, 1, 1, 2, 256);
            grid.Fix(1, 1, 2, 1, 1);
                         
            grid.Fix(1, 2, 0, 0, 4);
            grid.Fix(1, 2, 0, 2, 2);
            grid.Fix(1, 2, 1, 0, 1);
          //grid.Fix(1, 2, 2, 1, 32);
                         
            grid.Fix(2, 0, 0, 0, 1);
            grid.Fix(2, 0, 0, 1, 32);
            grid.Fix(2, 0, 1, 2, 64);
            grid.Fix(2, 0, 2, 1, 4);
                         
            grid.Fix(2, 1, 0, 0, 256);
            grid.Fix(2, 1, 0, 1, 4);
            grid.Fix(2, 1, 1, 2, 16);
        }

        private void ExtraHard9x9()
        {
            grid.Fix(0, 0, 2, 2, 1);

            grid.Fix(1, 0, 1, 0, 16);
            grid.Fix(1, 0, 0, 1, 128);
            grid.Fix(1, 0, 1, 2, 64);

            grid.Fix(2, 0, 1, 0, 1);
            grid.Fix(2, 0, 0, 2, 4);
            grid.Fix(2, 0, 1, 2, 128);

            grid.Fix(0, 1, 0, 0, 16);
            grid.Fix(0, 1, 1, 1, 256);
            grid.Fix(0, 1, 0, 2, 64);

            grid.Fix(1, 1, 0, 0, 4);
            grid.Fix(1, 1, 2, 2, 32);

            grid.Fix(2, 1, 2, 0, 32);
            grid.Fix(2, 1, 1, 1, 2);
            grid.Fix(2, 1, 2, 2, 128);

            grid.Fix(0, 2, 1, 0, 4);
            grid.Fix(0, 2, 2, 0, 128);
            grid.Fix(0, 2, 1, 2, 16);

            grid.Fix(1, 2, 1, 0, 2);
            grid.Fix(1, 2, 2, 1, 64);
            grid.Fix(1, 2, 1, 2, 8);

            grid.Fix(2, 2, 0, 0, 256);

            //grid.Fix(0, 0, 2, 1, 16);       - only 5 in subGrid
            //grid.Fix(2, 0, 2, 2, 16);
            //grid.Fix(1, 1, 0, 1, 64);
        }

        private void Diabolical9x9()
        {
            grid.Fix(0, 0, 1, 1, 2);

            grid.Fix(1, 0, 0, 0, 64);
            grid.Fix(1, 0, 2, 0, 8);
            grid.Fix(1, 0, 1, 1, 1);
            grid.Fix(1, 0, 1, 2, 128);

            grid.Fix(2, 0, 2, 0, 16);
            grid.Fix(2, 0, 1, 1, 64);
            grid.Fix(2, 0, 2, 2, 2);

            grid.Fix(0, 1, 1, 0, 256);
            grid.Fix(0, 1, 0, 1, 32);
            grid.Fix(0, 1, 1, 2, 16);
            grid.Fix(0, 1, 2, 2, 4);

            grid.Fix(1, 1, 2, 0, 32);
            grid.Fix(1, 1, 1, 1, 64);
            grid.Fix(1, 1, 0, 2, 2);

            grid.Fix(2, 1, 0, 0, 2);
            grid.Fix(2, 1, 1, 0, 16);
            grid.Fix(2, 1, 2, 1, 128);
            grid.Fix(2, 1, 1, 2, 1);

            grid.Fix(0, 2, 0, 0, 8);
            grid.Fix(0, 2, 1, 1, 4);
            grid.Fix(0, 2, 0, 2, 2);

            grid.Fix(1, 2, 1, 0, 256);
            grid.Fix(1, 2, 1, 1, 32);
            grid.Fix(1, 2, 0, 2, 8);
            grid.Fix(1, 2, 2, 2, 64);

            grid.Fix(2, 2, 1, 1, 256);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LayoutRoot.Children.Add(gridView);
        }

        private void grid_CellClicked(object sender, EventArgs.CellClickedEventArgs e)
        {
            if (e.Symbol.Length > 0)
            {
                grid.Fix(e.GridColumn, e.GridRow, e.SubGridColumn, e.SubGridRow, e.CellColumn, e.CellRow);
                grid.Solve();
            }
                
            gridView.Draw(grid);
        }

        private void gridView_ResetCellClicked(object sender, EventArgs.CellClickedEventArgs e)
        {
            (sender as View.GridUC).ResetCells();
            grid.Unfix(e.GridColumn, e.GridRow, e.SubGridColumn, e.SubGridRow);
            gridView.Draw(grid);
        }

        private void gridView_CellRightClicked(object sender, EventArgs.CellClickedEventArgs e)
        {
            bool updateDisplay = grid.RemoveOption(e.GridColumn, e.GridRow, e.SubGridColumn, e.SubGridRow, e.CellColumn, e.CellRow);    // Remaining option found
            if (!updateDisplay)
                (sender as View.GridUC)[e.GridColumn, e.GridRow][e.SubGridColumn, e.SubGridRow].StrikeOut(e.CellColumn, e.CellRow);

            // Remove option from the other sub grid's columns / rows when the option must belong in a specific sub grid's column / row
            updateDisplay |= grid.RemoveUnavailableOptions(e.GridColumn, e.GridRow, e.SubGridColumn, e.SubGridRow, e.CellColumn, e.CellRow);
            updateDisplay |= grid.Simplify();

            if (updateDisplay)
                gridView.Draw(grid);
        }
    }
}