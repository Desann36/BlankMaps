using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        public LoadWindow()
        {
            InitializeComponent();
            this.LengthTextBox.IsEnabled = false;
        }

        private Bitmap map;
        private Bitmap mask;
        private String[] regions;
        public Map NewMap { get; private set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            String name = this.NameTextBox.Text;
            double pom = 0;
            
            if (this.map == null || this.mask == null || this.regions == null || (name != null && String.IsNullOrWhiteSpace(name)))
            {
                MessageBox.Show("Some fields are not filled!", "Empty fields", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!this.RegionCheckBox.IsChecked == true && !this.DistanceCheckBox.IsChecked == true)
            {
                MessageBox.Show("Choose the type of map!", "Map type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.map.Width != this.mask.Width || this.map.Height != this.mask.Height)
            {
                MessageBox.Show("Map and mask dimensions are not identical!", "Map problem", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.DistanceCheckBox.IsChecked == true && !Double.TryParse(this.LengthTextBox.Text, out pom))
            {
                MessageBox.Show("Length needs to be a number!", "Number", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                try
                {
                    var items = Parse();
                    if (!this.RegionCheck(new List<Region>(items)))
                    {
                        MessageBox.Show("Mask does not contain all regions!", "Regions problem", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (!this.DuplicateRegionColorCheck(new List<Region>(items)))
                    {
                         MessageBox.Show("Region file contains regions with the same color!", "Regions problem", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        if (this.RegionCheckBox.IsChecked == true)
                        {
                            this.NewMap = new RegionMap(name, this.map, this.mask, items);
                        }

                        if (this.DistanceCheckBox.IsChecked == true)
                        {
                            this.NewMap = new DistanceMap(name, this.map, this.mask, items, pom);
                        }

                        this.DialogResult = true;
                    }
                }catch(Exception){
                    MessageBox.Show("Wrong format of region file!", "Wrong format", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<Region> Parse()
        {
            var items = this.regions.Where(line => !String.IsNullOrWhiteSpace(line)).Select(line =>
            {
                var item = line.Split(';');
                System.Drawing.Point p = new System.Drawing.Point(Convert.ToInt32(item[1]), Convert.ToInt32(item[2]));
                System.Drawing.Color col = System.Drawing.Color.FromArgb(255, Convert.ToInt32(item[1]), Convert.ToInt32(item[2]), Convert.ToInt32(item[3]));
                return new Region() { Name = item[0], Color = col };
            }).ToList();

            return items;
        }

        private bool RegionCheck(List<Region> regions)
        {
            BitmapData data = mask.LockBits(
               new System.Drawing.Rectangle(0, 0, mask.Width, mask.Height),
               ImageLockMode.ReadWrite,
               System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int[] bits = new int[data.Stride / 4 * data.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    if (regions.Any() == false)
                    {
                        mask.UnlockBits(data);
                        return true;
                    }

                    int col = bits[x + y * data.Stride / 4];
                    for (int i = 0; i < regions.Count; i++)
                    {
                        if (regions.ElementAt(i).Color.ToArgb() == col)
                        {
                            regions.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            mask.UnlockBits(data);

            if (regions.Any() == false)
            {
                return true;
            }
            return false;
        }


        private bool DuplicateRegionColorCheck(List<Region> regions)
        {
            var hashset = new HashSet<System.Drawing.Color>();
            foreach (var region in regions)
            {
                if (!hashset.Add(region.Color))
                {
                    return false;
                }
            }
            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OpenMapButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "All Image Files | *.bmp; *.dib; *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif; *.tif; *.tiff|" +
                         "BMP (*.bmp, *.dib)| *.bmp; *.dib|" +
                         "JPEG (*.jpg, *.jpeg, *.jpe, *.jfif,) | *.jpg; *.jpeg; *.jpe; *.jfif|" +
                         "GIF (*.gif)|*.gif|" +
                         "TIFF (*.tif, *.tiff)| *.tif; *.tiff|" +
                         "PNG (*.png)|*.png"
            };

            if (ofd.ShowDialog() == true)
            {
                this.MapTextBox.Text = ofd.FileName;
                this.map = (Bitmap)Bitmap.FromFile(ofd.FileName);
            }
        }

        private void OpenMaskButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "All Image Files | *.bmp; *.dib; *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif; *.tif; *.tiff|" +
                         "BMP (*.bmp, *.dib)| *.bmp; *.dib|" +
                         "JPEG (*.jpg, *.jpeg, *.jpe, *.jfif,) | *.jpg; *.jpeg; *.jpe; *.jfif|" +
                         "GIF (*.gif)|*.gif|" +
                         "TIFF (*.tif, *.tiff)| *.tif; *.tiff|" +
                         "PNG (*.png)|*.png"
            };

            if (ofd.ShowDialog() == true)
            {
                this.MaskTextBox.Text = ofd.FileName;
                this.mask = (Bitmap)Bitmap.FromFile(ofd.FileName);
            }
        }

        private void OpenRegionsButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "CSV Files (*.csv)|*.csv|" +
                         "Text Files (*.txt)|*.txt"
            };

            if (ofd.ShowDialog() == true)
            {
                this.RegionsTextBox.Text = ofd.FileName;
                this.regions = System.IO.File.ReadAllLines(ofd.FileName, Encoding.Default);
            }
        }

        private void DistanceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.RegionCheckBox.IsChecked == true)
            {
                this.RegionCheckBox.IsChecked = false;
            }
            this.LengthTextBox.IsEnabled = true;
        }

        private void RegionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.DistanceCheckBox.IsChecked == true)
            {
                this.DistanceCheckBox.IsChecked = false;
            }
            this.LengthTextBox.IsEnabled = false;
        }

        private void DistanceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.LengthTextBox.IsEnabled = false;
        }
    }
}
