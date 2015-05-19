using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Maps.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool GameStarted = false;

        private List<Region> ItemsToFind;

        private RegionMap actualMap;

        private List<Region> LoadRegions(string resource_data)
        {
            List<string> lines = resource_data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var items = lines.Where(line => !String.IsNullOrWhiteSpace(line)).Select(line =>
            {
                var item = line.Split(';');
                System.Drawing.Point p = new System.Drawing.Point(Convert.ToInt32(item[1]), Convert.ToInt32(item[2]));
                Color col = Color.FromArgb(255, Convert.ToInt32(item[1]), Convert.ToInt32(item[2]), Convert.ToInt32(item[3]));
                return new Region() { Name = item[0], Color = col };
            }).ToList();

            return items;
        }

        private ObservableCollection<string> items = new ObservableCollection<string>()
        {
            "500%", "300%", "200%", "150%", "100%", "90%", "75%", "50%", "20%", "10%"
        };

        public ObservableCollection<string> Items
        {
            get
            {
                return items;
            }
        }

        private string size = "100%";
        public string Size
        {
            get
            {
                return size;
            }

            set
            {
                if (value != size && value != null)
                {
                    size = value;
                    RaisePropertyChanged("Size");
                    RaisePropertyChanged("Width");
                    RaisePropertyChanged("Height");
                }
            }
        }

        private string newItem;
        public string NewItem
        {
            get
            {
                return newItem;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    int pom;
                    string[] tokens = value.Split(new string[] {"%"}, StringSplitOptions.None);
                    if ((tokens.Length == 1) || (tokens.Length == 2 && tokens[1].Equals("")))
                    {
                        if (Int32.TryParse(tokens[0], out pom))
                        {
                            if (pom > 2000)
                            {
                                Size = "2000%";
                                newItem = "2000%";
                                RaisePropertyChanged("NewItem");

                            }
                            else if (pom < 0)
                            {
                                Size = "1%";
                                newItem = "1%";
                                RaisePropertyChanged("NewItem");
                            }
                            else
                            {
                                Size = String.Format("{0}%", tokens[0]); ;
                                newItem = String.Format("{0}%",tokens[0]);
                                RaisePropertyChanged("NewItem");
                            }
                        }
                    }
                }
            }
        }

        private int width = Properties.Resources.slovensko_map.Width;
        public int Width
        {
            get
            {
                string[] tokens = Size.Split(new string[] { "%" }, StringSplitOptions.None);
                return (int)(width * Convert.ToDouble(tokens[0]) / 100);
            }
        }

        private int height = Properties.Resources.slovensko_map.Height;
        public int Height
        {
            get
            {
                string[] tokens = Size.Split(new string[] { "%" }, StringSplitOptions.None);
                return (int)(height * Convert.ToDouble(tokens[0]) / 100);
            }
        }

        private BitmapSource map = WPFBitmapConverter.ConvertBitmap(Properties.Resources.slovensko_map);
        public BitmapSource Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
                RaisePropertyChanged("Map");
            }
        }

        private string regionToFind;
        public string RegionToFind
        {
            get
            {
                return regionToFind;
            }
            set
            {
                regionToFind = value;
                RaisePropertyChanged("RegionToFind");
            }
        }

        public void MapClicked(System.Drawing.Point p)
        {

            if (this.actualMap == null || this.ItemsToFind == null || GameStarted == false)
            {
                return;
            }
            
            double nx = (double) p.X / this.Width;
            double ny = (double) p.Y / this.Height;

            int x = (int) (nx * this.Map.Width);
            int y = (int) (ny * this.Map.Height);

            Region wanted = ItemsToFind.First();
            Console.WriteLine(wanted);
            Bitmap bmp = WPFBitmapConverter.BitmapFromSource(this.Map);
            bool? correctness = this.actualMap.RegionClick(nx, ny, wanted.Color);

            if (correctness == true)
            {
                this.Map = WPFBitmapConverter.ConvertBitmap(MapOperations.FloodFill(bmp, new System.Drawing.Point(x, y), Color.Green));
            }
            else if (correctness == false)
            {
                System.Drawing.Point point = this.actualMap.pointInRegion(wanted.Color);
                this.Map = WPFBitmapConverter.ConvertBitmap(MapOperations.FloodFill(bmp, point, Color.Red));
            }
            else if (correctness == null)
            {
                return;
            }

            this.ItemsToFind.RemoveAt(0);
            if (!ItemsToFind.Any())
            {
                this.ItemsToFind = null;
                this.GameStarted = false;
                this.RegionToFind = "";
            }
            else
            {
                this.RegionToFind = ItemsToFind.First().Name;
            }

        }

        public ICommand StartNewMap
        {
            get
            {
                return new RelayCommand(() => this.StartMap());
            }
        }

        private void StartMap()
        {
            this.actualMap = new RegionMap("Slovakia", Properties.Resources.slovensko_map, 
                Properties.Resources.slovensko_mask, this.LoadRegions(Properties.Resources.slovensko_regions));
            this.Map = WPFBitmapConverter.ConvertBitmap(actualMap.MapToDraw);
            this.width = actualMap.MapToDraw.Width;
            this.height = actualMap.MapToDraw.Height;
            this.ItemsToFind = actualMap.Regions;
            GameStarted = true;
            this.RegionToFind = ItemsToFind.First().Name;
        }

        public ICommand ExitCommand
        {
            get
            {
                return new RelayCommand(() => Application.Current.Shutdown());
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return new RelayCommand(() => this.OpenFile());
            }
        }

        public void OpenFile()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "ZIP files|*.zip"
            };

            if (ofd.ShowDialog() == true)
            {
                this.Parse();
            }
        }

        private void Parse()
        {
            
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
