using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Maps.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            var map1 = new RegionMap("Slovensko - kraje", Properties.Resources.slovensko_map,
                Properties.Resources.slovensko_mask, this.LoadRegions(Properties.Resources.slovensko_regions));
            var map2 = new DistanceMap("Slovensko - rieky", Properties.Resources.sr_rieky_map,
                Properties.Resources.sr_rieky_mask, this.LoadRegions(Properties.Resources.slovensko_rivers), 413);
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
                this.Width = SelectedMap.MapToDraw.Width;
                this.Height = SelectedMap.MapToDraw.Height;
                this.NumberOfFoundRegions = 0;
                this.NumberOfRegions = 0;
                this.NumberOfRegions = this.SelectedMap.Regions.Count;
                this.Information = new ObservableCollection<Scoring>();
                this.RegionToFind = null;
                this.completeDistance = 0;

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
            set
            {
                this.width = value;
                RaisePropertyChanged("Width");
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
            set
            {
                this.height = value;
                RaisePropertyChanged("Height");
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

        private double completeDistance;

        public void MapClicked(System.Drawing.Point p, System.Windows.Controls.Image img)
        {
            if (this.SelectedMap == null || this.ItemsToFind == null || GameStarted == false)
            {
                return;
            }

            double nx = (double)p.X / this.Width;
            double ny = (double)p.Y / this.Height;

            int x = (int)(nx * this.Map.Width);
            int y = (int)(ny * this.Map.Height);

            Bitmap bmp = WPFBitmapConverter.BitmapFromSource(this.Map);

            if (this.SelectedMap is RegionMap)
            {
                RegionMap rm = this.SelectedMap as RegionMap;
                bool? correctness = rm.RegionClick(x, y, this.RegionToFind.Color);

                if (correctness == true)
                {
                    this.Map = WPFBitmapConverter.ConvertBitmap(MapOperations.PaintRegion(bmp, SelectedMap.Mask, this.RegionToFind.Color, Color.Green));
                    this.Information.Insert(0, new Scoring() { Name = this.RegionToFind.Name, Score = "✔" });
                }
                else if (correctness == false)
                {
                    System.Drawing.Point point = this.SelectedMap.pointInRegion(this.RegionToFind.Color);
                    BitmapSource animated = WPFBitmapConverter.ConvertBitmap(MapOperations.PaintRegion(bmp, SelectedMap.Mask, this.RegionToFind.Color, Color.Red));
                    this.Information.Insert(0, new Scoring() { Name = this.RegionToFind.Name, Score = "×" });

                    img.Visibility = Visibility.Visible;
                    img.Opacity = 1.0;
                    img.Source = animated;
                    DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(750));
                    fadeOut.Completed += (s, e) =>
                    {
                        img.Visibility = Visibility.Collapsed;
                        DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(0));
                        img.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, fadeIn);
                    };
                    img.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, fadeOut);
                }
                else if (correctness == null)
                {
                    return;
                }
            }

            if (this.SelectedMap is DistanceMap)
            {
                var dm = this.SelectedMap as DistanceMap;
                double pixelDistance = MapOperations.NearestPixel(this.SelectedMap.Mask, new System.Drawing.Point(x, y), this.RegionToFind.Color);
                this.Map = WPFBitmapConverter.ConvertBitmap(MapOperations.PaintRegion(bmp, SelectedMap.Mask, this.RegionToFind.Color, Color.Green));
                double realDistance = pixelDistance * dm.HorizontalLength / this.Map.Width;
                this.Information.Insert(0, new Scoring() { Name = this.RegionToFind.Name, Score = string.Format("{0:N2}km", realDistance) });
                this.completeDistance += realDistance;
            }

            RaisePropertyChanged("Information");
            this.ItemsToFind.Remove(this.RegionToFind);
            if (!ItemsToFind.Any())
            {
                this.TestEnded();
            }
            else
            {
                Random rnd = new Random();
                int regionIndex = rnd.Next(0, this.ItemsToFind.Count);
                this.RegionToFind = ItemsToFind.ElementAt(regionIndex);
                this.NumberOfFoundRegions += 1;
            }
        }

        private void TestEnded()
        {
            this.ItemsToFind = null;
            this.GameStarted = false;
            this.RegionToFind = null;
            int correct = 0;

            if (this.SelectedMap is RegionMap)
            {
                foreach (var item in this.Information)
                {
                    if (item.Score.Equals("✔"))
                    {
                        correct++;
                    }
                }

                MessageBox.Show(String.Format("Test completed!{0}Your score: {1} correct out of {2} - {3:N1}%", Environment.NewLine, correct,
                    this.Information.Count, (double)correct / this.Information.Count * 100),
                    "Test ended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(String.Format("Test completed!{0}Your score: {1:N2}km", Environment.NewLine, this.completeDistance),
                    "Test ended", MessageBoxButton.OK, MessageBoxImage.Information);
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
            this.Width = SelectedMap.MapToDraw.Width;
            this.Height = SelectedMap.MapToDraw.Height;
            this.ItemsToFind = new List<Region>(SelectedMap.Regions);
            this.completeDistance = 0;

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

        public ICommand NewMapGuide
        {
            get
            {
                return new RelayCommand(() => this.Guide());
            }
        }

        private void Guide()
        {
            var nmg = new LoadingGuideWindow();
            nmg.Show();
        }

        public ICommand AboutCommand
        {
            get
            {
                return new RelayCommand(() => this.About());
            }
        }

        private void About()
        {
            var about = new AboutWindow();
            about.ShowDialog();
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
