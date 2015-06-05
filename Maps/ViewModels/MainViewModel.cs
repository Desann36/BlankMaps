using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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
            this.LoadMaps();
            this.Background = WPFBitmapConverter.ConvertBitmap(Properties.Resources.Pozadie);
            this.NewItem = "100%";
            if (this.MapsCollection.Any())
            {
                this.SelectedMap = this.MapsCollection.ElementAt(0);
            }
        }

        public readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".JPE", ".JFIF", 
            ".BMP", ".DIB", ".GIF", ".PNG", ".TIF", ".TIFF",  };

        public readonly List<string> TextFilesExtensions = new List<string> { ".TXT", ".CSV" };

        private void LoadMaps()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathData = System.IO.Path.Combine(path, "data");

            if (!Directory.Exists(pathData))
            {
                Directory.CreateDirectory(pathData);
            }

            string[] folders = System.IO.Directory.GetDirectories(pathData);

            foreach (var folder in folders)
            {
                var pathString = System.IO.Path.Combine(pathData, folder);
                string[] files = Directory.GetFiles(pathString);

                String mapType = null;
                String name = null;
                Bitmap map = null;
                Bitmap mask = null;
                List<Region> regions = null;
                double horizontalLength = -1;

                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    if (ImageExtensions.Contains(Path.GetExtension(file).ToUpperInvariant()))
                    {
                        var pom = fileName.Split('_');
                        var type = pom[pom.Length - 1];

                        if (type.Equals("map"))
                        {
                            BitmapSource image = new BitmapImage(new Uri(file, UriKind.Absolute));
                            map = WPFBitmapConverter.BitmapFromSource(image);
                        }

                        if (type.Equals("mask"))
                        {
                            BitmapSource image = new BitmapImage(new Uri(file, UriKind.Absolute));
                            mask = WPFBitmapConverter.BitmapFromSource(image);
                        }
                    }

                    if (TextFilesExtensions.Contains(Path.GetExtension(file).ToUpperInvariant()))
                    {
                        try
                        {
                            string[] lines = System.IO.File.ReadAllLines(file, Encoding.Default);
                        
                            int skip = 0;
                            mapType = (lines[0].Split(';'))[0];
                            name = (lines[1].Split(';'))[0];

                            if (mapType.Equals("Regions"))
                            {
                                skip = 2;
                            }

                            if (mapType.Equals("Distances"))
                            {
                                skip = 3;
                                horizontalLength = Convert.ToDouble((lines[2].Split(';'))[0]);
                            }

                            regions = lines.Skip(skip).Where(line => !String.IsNullOrWhiteSpace(line)).Select(line =>
                            {
                                var item = line.Split(';');
                                Color col = Color.FromArgb(255, Convert.ToInt32(item[1]), Convert.ToInt32(item[2]), Convert.ToInt32(item[3]));
                                return new Region() { Name = item[0], Color = col };
                            }).ToList();
                        }
                        catch (System.IO.IOException e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (FormatException e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Map loading problem: wrong format of region file!", "Wrong format", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

                if (this.NewMapAttributesCheck(name, map, mask, regions, horizontalLength, mapType))
                {
                    if (mapType.Equals("Regions"))
                    {
                        var newMap = new RegionMap(name, map, mask, regions);
                        this.MapsCollection.Add(newMap);
                    }

                    if (mapType.Equals("Distances"))
                    {
                        var newMap = new DistanceMap(name, map, mask, regions, horizontalLength);
                        this.MapsCollection.Add(newMap);
                    }
                }
            }
        }

        private bool NewMapAttributesCheck(String name, Bitmap map, Bitmap mask, List<Region> regions, double horizontalLength, string mapType)
        {
            if ((name == null || map == null || mask == null || regions == null) || (mapType.Equals("Distances") && horizontalLength <= 0))
            {
                MessageBox.Show("Problem with loading a map from data package!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (map.Width != mask.Width || map.Height != mask.Height)
            {
                MessageBox.Show("Map loading problem: map and mask dimensions are not identical!", "Map problem", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            if (!this.RegionCheck(new List<Region>(regions), mask))
            {
                MessageBox.Show("Map loading problem: mask does not contain all regions!", "Regions problem", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            if (!this.DuplicateRegionColorCheck(new List<Region>(regions)))
            {
                MessageBox.Show("Region file contains regions with the same color!", "Regions problem", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool RegionCheck(List<Region> regions, Bitmap mask)
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
                this.correct = 0;

                if (this.SelectedMap is DistanceMap)
                {
                    this.InformationTitle = "Distances:";
                    this.Score = "0km";
                }

                if (this.SelectedMap is RegionMap)
                {
                    this.InformationTitle = "Marked:";
                    this.Score = "0/0";
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

        private readonly ObservableCollection<string> items = new ObservableCollection<string>()
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
                            if (pom > 500)
                            {
                                Size = "500%";
                                newItem = "500%";
                                RaisePropertyChanged("NewItem");

                            }
                            else if (pom < 10)
                            {
                                Size = "10%";
                                newItem = "10%";
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

        private string score;
        public String Score
        {
            get
            {
                return this.score;
            }
            set
            {
                this.score = value;
                RaisePropertyChanged("Score");
            }
        }

        private Color listBoxItemColor = Color.Red;
        public Color ListBoxItemColor
        {
            get
            {
                return this.listBoxItemColor;
            }
            set
            {
                this.listBoxItemColor = value;
                RaisePropertyChanged("ListBoxItemColor");
            }
        }

        public BitmapSource Background { get; set; }

        private double completeDistance;
        private int correct;

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
                    this.Information.Insert(0, new Scoring() { Name = this.RegionToFind.Name, Score = "✔", ItemColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("Green") });
                    this.correct++;
                    this.Score = String.Format("{0}/{1}", correct, this.Information.Count);
                }
                else if (correctness == false)
                {
                    System.Drawing.Point point = this.SelectedMap.pointInRegion(this.RegionToFind.Color);
                    BitmapSource animated = WPFBitmapConverter.ConvertBitmap(MapOperations.PaintRegion(bmp, SelectedMap.Mask, this.RegionToFind.Color, Color.Red));
                    this.Information.Insert(0, new Scoring() { Name = this.RegionToFind.Name, Score = "×", ItemColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("Red") });
                    this.Score = String.Format("{0}/{1}", correct, this.Information.Count);

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
                this.Information.Insert(0, new Scoring() { Name = this.RegionToFind.Name, Score = string.Format("{0:N2}km", realDistance), ItemColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("Black") });
                this.completeDistance += realDistance;
                this.Score = String.Format("{0:N2}km", this.completeDistance);
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

            if (this.SelectedMap is RegionMap)
            {

                MessageBox.Show(String.Format("Test completed!{0}Your score: {1} correct out of {2} - {3:N1}%", Environment.NewLine, this.correct,
                    this.Information.Count, (double)correct / this.Information.Count * 100),
                    "Test ended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(String.Format("Test completed!{0}Sum of distances to the regions: {1:N2}km", Environment.NewLine, this.completeDistance),
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
            if (SelectedMap == null) return;
            GameStarted = true;
            this.Map = WPFBitmapConverter.ConvertBitmap(SelectedMap.MapToDraw);
            this.Width = SelectedMap.MapToDraw.Width;
            this.Height = SelectedMap.MapToDraw.Height;
            this.ItemsToFind = new List<Region>(SelectedMap.Regions);
            this.completeDistance = 0;
            this.correct = 0;

            if (this.SelectedMap is DistanceMap)
            {
                this.Score = "0km";
            }

            if (this.SelectedMap is RegionMap)
            {
                this.Score = "0/0";
            }

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
        public System.Windows.Media.Color ItemColor { get; set; }

        public override string ToString()
        {
            return String.Format("{0}: {1}", this.Name, this.Score);
        }
    }
}
