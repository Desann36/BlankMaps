using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Maps
{
    /// <summary>
    /// Interaction logic for LoadingGuideWindow.xaml
    /// </summary>
    public partial class LoadingGuideWindow : Window
    {
        public LoadingGuideWindow()
        {
            InitializeComponent();
            this.MapExample.Source = WPFBitmapConverter.ConvertBitmap(Properties.Resources.slovensko_map);
            this.MaskExample.Source = WPFBitmapConverter.ConvertBitmap(Properties.Resources.slovensko_mask);
            this.CSVExample.Source = WPFBitmapConverter.ConvertBitmap(Properties.Resources.RegionsCSV); ;
        }
    }
}
