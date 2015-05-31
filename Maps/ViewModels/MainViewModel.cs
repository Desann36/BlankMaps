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
        public MainViewModel()
        {
            var map1 = new RegionMap("Slovensko - kraje", Properties.Resources.slovensko_map,
                Properties.Resources.slovensko_mask, this.LoadRegions(Properties.Resources.slovensko_regions));
            var map2 = new DistanceMap("Slovensko - rieky", Properties.Resources.slovensko_map,
                Properties.Resources.slovensko_maskrivers, this.LoadRegions(Properties.Resources.slovensko_rivers));
            this.MapsCollection = new ObservableCollection<Map>() { map1, map2 };
            this.SelectedMap = this.MapsCollection.ElementAt(0);
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private bool GameStarted = false;

        private Map selectedMap;
        public Map SelectedMap
        {
            get
            {
                return this.selectedMap;
            }
            set
            {
                this.selectedMap = value;
                RaisePropertyChanged("SelectedMap");

                GameStarted = false;
                this.Map = WPFBitmapConverter.ConvertBitmap(SelectedMap.MapToDraw);
                this.width = SelectedMap.MapToDraw.Width;
                this.height = SelectedMap.MapToDraw.Height;
                this.NumberOfFoundRegions = 0;
                this.NumberOfRegions = 0;
                this.NumberOfRegions = this.SelectedMap.Regions.Count;
                this.Information = new ObservableCollection<Scoring>();
                this.RegionToFind = null;

                if (this.SelectedMap is DistanceMap)
                {
                    this.InformationTitle = "Distances:";
                }

                if (this.SelectedMap is RegionMap)
                {
                    this.InformationTitle = "Marked:";
                }

            }
        }

        private BitmapSource map;
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

        private List<Region> ItemsToFind;

        private Region regionToFind;
        public Region RegionToFind
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

        private int numberOfRegions;
        public int NumberOfRegions
        {
            get
            {
                return numberOfRegions;
            }
            set
            {
                numberOfRegions = value;
                RaisePropertyChanged("NumberOfRegions");
            }
        }

        private int numberOfFoundRegions;
        public int NumberOfFoundRegions
        {
            get
            {
                return numberOfFoundRegions;
            }
            set
            {
                numberOfFoundRegions = value;
                RaisePropertyChanged("NumberOfFoundRegions");
            }
        }

        private ObservableCollection<Map> maps = new ObservableCollection<Map>();
        public ObservableCollection<Map> MapsCollection
        {
            get
            {
                return this.maps;
            }
            set
            {
                this.maps = value;
                RaisePropertyChanged("Maps");
            }
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

        private int width;
        public int Width
        {
            get
            {
                string[] tokens = Size.Split(new string[] { "%" }, StringSplitOptions.None);
                return (int)(width * Convert.ToDouble(tokens[0]) / 100);
            }
        }

        private int height;
        public int Height
        {
            get
            {
                string[] tokens = Size.Split(new string[] { "%" }, StringSplitOptions.None);
                return (int)(height * Convert.ToDouble(tokens[0]) / 100);
            }
        }

        private string informationTitle;
        public String InformationTitle
        {
            get
            {
                return informationTitle;
            }
            private set
            {
                this.informationTitle = value;
                RaisePropertyChanged("InformationTitle");
            }
        }

        private ObservableCollection<Scoring> information;
        public ObservableCollection<Scoring> Information
        {
            get
            {
                return information;
            }
            private set
            {
                this.information = value;
                RaisePropertyChanged("Information");
            }
        }

        public void MapClicked(System.Drawing.Point p)
        {
            if (this.SelectedMap == null || this.ItemsToFind == null || GameStarted == false)
            {
                return;
            }
            
            double nx = (double) p.X / this.Width;
            double ny = (double) p.Y / this.Height;

            int x = (int) (nx * this.Map.Width);
            int y = (int) (ny * this.Map.Height);

            Bitmap bmp = WPFBitmapConverter.BitmapFromSource(this.Map);

            if (this.SelectedMap is RegionMap)
            {
                RegionMap rm = this.SelectedMap as RegionMap;
                bool? correctness = rm.RegionClick(x, y, this.RegionToFind.Color);

                if (correctness == true)
                {
                    this.Map = WPFBitmapConverter.ConvertBitmap(MapOperations.PaintRegion(bmp, SelectedMap.Mask, this.RegionToFind.Color, Color.Green));
                    this.Information.Add(new Scoring() { Name = this.RegionToFind.Name, Score = "✔" });
                }
                else if (correctness == false)
                {
                    System.Drawing.Point point = this.SelectedMap.pointInRegion(this.RegionToFind.Color);
                    this.Map = WPFBitmapConverter.ConvertBitmap(MapOperations.PaintRegion(bmp, SelectedMap.Mask, this.RegionToFind.Color, Color.Red));
                    this.Information.Add(new Scoring() { Name = this.RegionToFind.Name, Score = "×" });
                }
                else if (correctness == null)
                {
                    return;
                }
            }

            if (this.SelectedMap is DistanceMap)
            {
                DistanceMap dm = SelectedMap as DistanceMap;
                double dist = MapOperations.NearestPixel(this.SelectedMap.Mask, new System.Drawing.Point(x, y), this.RegionToFind.Color);
                this.Map = WPFBitmapConverter.ConvertBitmap(MapOperations.PaintRegion(bmp, SelectedMap.Mask, this.RegionToFind.Color, Color.Green));
                this.Information.Add(new Scoring() { Name = this.RegionToFind.Name, Score = string.Format("{0:N2}km", dist) });
            }

            RaisePropertyChanged("Information");
            this.ItemsToFind.Remove(this.RegionToFind);
            if (!ItemsToFind.Any())
            {
                this.ItemsToFind = null;
                this.GameStarted = false;
                this.RegionToFind = null;
            }
            else
            {
                Random rnd = new Random();
                int regionIndex = rnd.Next(0, this.ItemsToFind.Count);
                this.RegionToFind = ItemsToFind.ElementAt(regionIndex);
                this.NumberOfFoundRegions += 1;
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
            GameStarted = true;
            this.Map = WPFBitmapConverter.ConvertBitmap(SelectedMap.MapToDraw);
            this.width = SelectedMap.MapToDraw.Width;
            this.height = SelectedMap.MapToDraw.Height;
            this.ItemsToFind = new List<Region>(SelectedMap.Regions);

            Random rnd = new Random();
            int regionIndex = rnd.Next(0, this.ItemsToFind.Count);
            this.RegionToFind = ItemsToFind.ElementAt(regionIndex);

            this.NumberOfFoundRegions = 1;
            this.Information = new ObservableCollection<Scoring>();
        }

        public ICommand NewMapCommand
        {
            get
            {
                return new RelayCommand(() => this.NewMap());
            }
        }

        private void NewMap()
        {
            LoadWindow lw = new LoadWindow();

            if (lw.ShowDialog() == true)
            {
                if (lw.NewMap != null)
                {
                    this.MapsCollection.Add(lw.NewMap);
                    this.SelectedMap = lw.NewMap;
                }
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                return new RelayCommand(() => Application.Current.Shutdown());
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class Scoring
    {
        public string Name { get; set; }
        public string Score { get; set; }

        public override string ToString()
        {
            return String.Format("{0}: {1}", this.Name, this.Score);
        }
    }
}
