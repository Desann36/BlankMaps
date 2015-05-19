using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Maps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var Image = sender as Image;
            var viewModel = Image.DataContext as Maps.ViewModels.MainViewModel;
            Point point = e.GetPosition(Image);
            System.Drawing.Point p = new System.Drawing.Point((int) point.X, (int) point.Y);
            viewModel.MapClicked(p);
        }

        private void image1_MouseMove(object sender, MouseEventArgs e)
        {
            var Image = sender as Image;
            this.Coordinates.Text = e.GetPosition(Image).ToString();
        }

        private void SizeCombobox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (Key.Enter == e.Key)
                comboBox.RaiseEvent(new RoutedEventArgs(LostFocusEvent, comboBox));
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as UIElement;
            grid.Focus();
        }

        private void SizeCombobox_GotFocus(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string[] tokens = comboBox.Text.Split(new string[] { "%" }, StringSplitOptions.None);
            comboBox.Text = tokens[0];
        }
    }
}
