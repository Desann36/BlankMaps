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
            InitializeComponent();
            this.mouse = new Point(0, 0);
        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image Image = sender as Image;
            var viewModel = Image.DataContext as Maps.ViewModels.MainViewModel;
            Point point = e.GetPosition(Image);
            System.Drawing.Point p = new System.Drawing.Point((int)point.X, (int)point.Y);
            viewModel.MapClicked(p, this.image2);
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

        private Point mouse;

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            if (handle)
            {
                var Grid = sender as Grid;
                var viewModel = Grid.DataContext as Maps.ViewModels.MainViewModel;
                double h = (double)mouse.Y / viewModel.Height;
                double w = (double)mouse.X / viewModel.Width;
                this.ScrollMap.ScrollToVerticalOffset(h * (double) this.ScrollMap.ScrollableHeight);
                this.ScrollMap.ScrollToHorizontalOffset(w * (double) this.ScrollMap.ScrollableWidth);
                
                string[] tokens = viewModel.NewItem.Split(new string[] { "%" }, StringSplitOptions.None);
                int size = Convert.ToInt32(tokens[0]);

                if (e.Delta > 0)
                {
                    viewModel.NewItem = (size + 10).ToString();
                }

                if (e.Delta < 0)
                {
                    viewModel.NewItem = (size - 10).ToString();
                }
            }
        }

        private void image1_MouseMove(object sender, MouseEventArgs e)
        {
            var Image = sender as Image;
            this.mouse = e.GetPosition(Image);
        }

        private void ScrollMap_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            if (handle)
            {
                e.Handled = true;
            }
        }
    }
}
