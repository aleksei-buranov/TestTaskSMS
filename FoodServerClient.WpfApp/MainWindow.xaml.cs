using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FoodServerClient.WpfApp.ViewModels;

namespace FoodServerClient.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.Row.Item is not EnvVarRow row)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                if (DataContext is MainViewModel vm)
                    vm.SaveRow(row);
            });
        }
    }
}