using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Data;
using DerpiViewer.Properties;

namespace DerpiViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // Initialize list of files downloaded this session
        public ObservableCollection<FileDisplay> FilesCollection { get; } = new ObservableCollection<FileDisplay>();

        // Initialize list of current image's tags
        public ObservableCollection<string> TagsCollection { get; } = new ObservableCollection<string>();

        // Initialize list of saved queries
        public ObservableCollection<Query> QueriesCollection { get; set; } = new ObservableCollection<Query>(Query.GetAll());

        // Store loaded files count and current file number
        public int TotalFiles = 0;
        public int CurrentFileNumber { get; set; } = 1;

        // Currently displayed file
        public FileDisplay CurrentFile;
        public int CurrentFileId;

        // Settings
        private readonly Settings _settings = Settings.Default;

        // Has query been changed
        private bool _wasQueryChanged = false;

        // Store the current query
        private string _query;

        // Store bookmark name border brush
        private new System.Windows.Media.Brush BorderBrush;

        public MainWindow()
        {
            // Create Json file to store bookmarks
            if (!File.Exists("bookmarks.json"))
                File.Create("bookmarks.json");

            InitializeComponent();
            DataContext = this;

            // Get app accent and theme from settings and set it
            ThemeManager.ChangeAppStyle(Application.Current,
                                            ThemeManager.GetAccent(_settings.Accent),
                                            ThemeManager.GetAppTheme(_settings.Theme));

            // Set accent combobox data source
            Accent_ComboBox.ItemsSource = Enum.GetValues(typeof(LooksManager.Accents)).Cast<LooksManager.Accents>();

            // Set accent combobox item based on settings
            Accent_ComboBox.SelectedIndex = (int)Enum.Parse(typeof(LooksManager.Accents), _settings.Accent);

            // Set theme button icon based on settings
            if (_settings.Theme == "BaseLight")
            {
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SunRegular;
                _darkmodeIconToggle = false;
            }
            else
            {
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.MoonRegular;
                _darkmodeIconToggle = true;
            }

            // Set download path
            if (_settings.DownloadLocation == "")
            {
                _settings.DownloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\DerpiViewer\";
            }

            var dlPath = _settings.DownloadLocation;

            // Create the directory if doesn't exist
            Directory.CreateDirectory(dlPath);

            // Set path text
            DlPathBox.Text = _settings.DownloadLocation;

            // Set token
            TokenBox.Text = _settings.Token;

            // Set ratings
            Safe_Switch.IsChecked = _settings.Safe;
            Questionable_Switch.IsChecked = _settings.Questionable;
            Explicit_Switch.IsChecked = _settings.Explicit;

            // Set border brush
            BorderBrush = QueryNameTextbox.BorderBrush;
        }

        // Handle main menu open/close
        bool _menuToggle;
        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout.IsOpen = !_menuToggle;
            _menuToggle = !_menuToggle;

            // Save settings
            _settings.Token = TokenBox.Text;
            _settings.Save();
        }

        // Handle accent selection combobox
        private void Accent_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

            ThemeManager.ChangeAppStyle(Application.Current,
                                        ThemeManager.GetAccent(Accent_ComboBox.SelectedItem.ToString()),
                                        theme.Item1);

            // Save in _settings
            _settings.Accent = Accent_ComboBox.SelectedItem.ToString();
        }

        // Handle theme selection switch
        private bool _darkmodeIconToggle;
        private void DarkMode_Toggle_Click(object sender, RoutedEventArgs e)
        {
            Tuple<AppTheme, Accent> completeTheme = ThemeManager.DetectAppStyle(Application.Current);
            string theme;

            if (_darkmodeIconToggle)
            {
                theme = "BaseLight";

                _darkmodeIconToggle = false;
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SunRegular;
            }
            else
            {
                theme = "BaseDark";

                _darkmodeIconToggle = true;
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.MoonRegular;
            }

            // Apply chosen theme
            ThemeManager.ChangeAppStyle(Application.Current,
                                            completeTheme.Item2,
                                            ThemeManager.GetAppTheme(theme));

            // Save in _settings
            _settings.Theme = theme;
        }

        // Handle titlebar minification switch
        private void MinifyTitlebar_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (ShowTitleBar)
            {
                ShowTitleBar = false;
                icoMinifyTitlebar.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.EyeOff;
            }
            else
            {
                ShowTitleBar = true;
                icoMinifyTitlebar.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Eye;
            }

            // Save in _settings
            _settings.MinifiedTitle = ShowTitleBar;
        }

        // Handle KoFi
        private void btnKoFi_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ko-fi.com/H2H365N9");
        }

        // Handle closing
        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            // Save token
            _settings.Token = TokenBox.Text;
            // Save _settings
            _settings.Save();
        }

        // Fetch
        private void FetchBtn_Click(object sender, RoutedEventArgs e)
        {
            // Empty collection if query was changed
            if (_wasQueryChanged)
            {
                FilesCollection.Clear();
                _wasQueryChanged = false;
            }

            string query = BuildQuery();

            if (query != "")
            {
                _query = query;

                // Populate grid
                PopulateGrid(_query);
            }
        }

        // Handle cleaning fetched files
        private void ClearFetched_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (FilesCollection.Any())
            {
                FilesCollection.Clear();
                DlPage.Value = 1;
                testBox.Text = "List has been cleared!";
            }
        }

        // Set download path
        private void SelectDownloadFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            // Initialize folder selection window

            var dlg = new CommonOpenFileDialog
            {
                Title = "Select download folder",
                IsFolderPicker = true,
                InitialDirectory = _settings.DownloadLocation,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = _settings.DownloadLocation,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };


            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dlg.FileName;

                _settings.DownloadLocation = folder + @"\";

                // Save _settings
                _settings.Save();

                // Set path text
                DlPathBox.Text = _settings.DownloadLocation;
            }

        }

        // Handle token help
        private void TokenHelp_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Atulin/DerpiViewer#token");
        }

        // Handle opening destination folder
        private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(_settings.DownloadLocation);
        }

        // Handle query change
        private void DlQueryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DlPage.Value = 1;
            _wasQueryChanged = true;
        }

        // Handle download button clicked
        private void Download_Btn_Click(object sender, RoutedEventArgs e)
        {
            FileDisplay fd = CurrentFile;

            // Download
            using (WebClient client = new WebClient())
            {
                if (fd != null)
                    client.DownloadFileAsync(new Uri(fd.File),
                        _settings.DownloadLocation + fd.Filename);
            }
        }

        // Handle downloading all files
        private void DowloadAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (FileDisplay fd in FilesCollection)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFileAsync(new Uri(fd.File), _settings.DownloadLocation + fd.Filename);
                }
            }
        }

        // Handle advanced search flyout open
        private void AdvancedSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            AdvancedSearch_Flyout.IsOpen = !AdvancedSearch_Flyout.IsOpen;
        }

        // Handle flyout closed
        private void AdvancedSearch_Flyout_OnIsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (!AdvancedSearch_Flyout.IsOpen)
            {
                _settings.Safe = Safe_Switch.IsChecked ?? false;
                _settings.Questionable = Questionable_Switch.IsChecked ?? false;
                _settings.Explicit = Explicit_Switch.IsChecked ?? false;

                _settings.Save();
            }
        }

        // Handle opening the file
        private void OpenFile_Btn_Click(object sender, RoutedEventArgs e)
        {
            FileDisplay fd = ((FrameworkElement)sender).DataContext as FileDisplay;

            CurrentFile = fd;
            CurrentFileId = FilesCollection.IndexOf(fd);
            FileIndexTxt.Text = CurrentFileId + 1 + "/" + TotalFiles;

            //Set infogrid
            SetImage(fd);

            CloseImageView.Visibility = Visibility.Visible;
            ToggleInfogridBtn.Visibility = Visibility.Visible;
            FileIndexTxt.Visibility = Visibility.Visible;

            Display_Flyout.IsOpen = true;
        }

        // Close file display
        private void CloseImageView_Click(object sender, RoutedEventArgs e)
        {
            Display_Flyout.IsOpen = false;

            ToggleInfogridBtn.Visibility = Visibility.Collapsed;
            CloseImageView.Visibility = Visibility.Collapsed;
            FileIndexTxt.Visibility = Visibility.Collapsed;
        }

        // Handle keyboard shortcuts
        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Display_Flyout.IsOpen)
            {
                switch (e.Key)
                {
                    case Key.A:
                        BrowserWindow.Address = FilesCollection[(CurrentFileId - 1).Clamp(0, FilesCollection.Count - 1)].File;
                        CurrentFileId = (CurrentFileId - 1).Clamp(0, FilesCollection.Count - 1);

                        CurrentFile = FilesCollection[CurrentFileId];
                        
                        FileIndexTxt.Text = CurrentFileId+1 + "/" + TotalFiles;

                        SetImage(CurrentFile);
                        break;

                    case Key.D:
                        BrowserWindow.Address = FilesCollection[(CurrentFileId + 1).Clamp(0, FilesCollection.Count - 1)].File;
                        CurrentFileId = (CurrentFileId + 1).Clamp(0, FilesCollection.Count - 1);

                        CurrentFile = FilesCollection[CurrentFileId];

                        // If nearing the end, fetch more
                        if (CurrentFileId >= FilesCollection.Count - 3)
                            FetchBtn_Click(sender, new RoutedEventArgs());
                        
                        FileIndexTxt.Text = CurrentFileId+1 + "/" + TotalFiles;

                        SetImage(CurrentFile);
                        break;

                    case Key.W:
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFileAsync(new Uri(CurrentFile.File), _settings.DownloadLocation + CurrentFile.Filename);
                        }
                        break;

                    case Key.E:
                        InfoGrid.Visibility = InfoGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                        break;

                    case Key.Escape:
                        CloseImageView_Click(sender, new RoutedEventArgs());
                        break;
                }
            }
        }

        // Handle clicking on tags
        private void TagClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView)?.SelectedItem;
            if (item != null)
            {
                DlQueryBox.Text += ", " + item;
                _wasQueryChanged = true;
            }
        }

        // Handle infogrid hover

        private void ToggleInfogrid_Click(object sender, RoutedEventArgs e)
        {
            InfoGrid.Visibility = InfoGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        // Set Infogrid
        private void SetImage(FileDisplay fd)
        {
            //tags
            TagsCollection.Clear();
            if (fd != null)
            {
                foreach (string s in fd.Tags)
                    TagsCollection.Add(s);

                BrowserWindow.Address = fd.File;
            }
            else
            {
                throw new NullReferenceException("File was null!");
            }

            //artist
            Info_Artist.Text = fd.Author;
            Info_Description.Text = fd.Description;
        }

        // Open image in browser
        private void OpenInBrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(CurrentFile.Link);
        }

        // Copy URL to clipboard
        private void CopyUrlBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentFile.File);
        }

        // Toggle query saving window
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SaveWindow.Visibility != Visibility.Visible)
                SaveWindow.Visibility = Visibility.Visible;

            SaveWindow.IsOpen = !SaveWindow.IsOpen;
        }

        // Handle query saving
        private void SaveQueryBtn_Click(object sender, RoutedEventArgs e)
        {
            Query queryToSave = QueriesCollection.FirstOrDefault(i => i.Name == QueryNameTextbox.Text);

            if (queryToSave == null)
            {
                QueryNameTextbox.BorderBrush = BorderBrush;

                Query q = new Query()
                {
                    Name = QueryNameTextbox.Text,
                    Tags = DlQueryBox.Text,
                    QueryStr = BuildQuery(),
                    MinScore = (MinimumScore_Numeric.Value != null) ? (int)MinimumScore_Numeric.Value : 0,
                    MaxScore = (MaximumScore_Numeric.Value != null) ? (int)MaximumScore_Numeric.Value : 0,
                    Ratings = new[]
                    {
                        Safe_Switch.IsChecked != null && (bool)Safe_Switch.IsChecked,
                        Questionable_Switch.IsChecked != null && (bool)Questionable_Switch.IsChecked,
                        Explicit_Switch.IsChecked != null && (bool)Explicit_Switch.IsChecked
                    }

                };
                QueriesCollection.Insert(0, q);
                q.SaveQuery();
            }
            else
            {
                QueryNameTextbox.BorderBrush = System.Windows.Media.Brushes.Red;
                testBox.Text = "There already is a query with this name";
            }
        }

        // Load the query
        private void QueryClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView)?.SelectedItem;
            if (item != null)
            {
                Query q = (Query) item;

                // Expose the query
                _query = q.QueryStr;
            
                // Substitute tags
                DlQueryBox.Text = q.Tags;
                // Set scores
                MinimumScore_Numeric.Value = q.MinScore;
                MaximumScore_Numeric.Value = q.MaxScore;
                // Set ratings
                Safe_Switch.IsChecked = q.Ratings[0];
                Questionable_Switch.IsChecked = q.Ratings[1];
                Explicit_Switch.IsChecked = q.Ratings[2];

                // Clear grid
                if (FilesCollection.Any())
                {
                    FilesCollection.Clear();
                    DlPage.Value = 1;;
                }

                // Populate grid
                PopulateGrid(_query);
            }
        }

        // Delete query
        private void DeleteQueryBtn_Click(object sender, RoutedEventArgs e)
        {
            Query queryToDelete = QueriesCollection.FirstOrDefault(i => i.Name == (string)(sender as Button)?.Tag);

            if (queryToDelete != null)
            {
                QueriesCollection.Remove(queryToDelete);
                queryToDelete.DeleteQuery();
            }
        }

        ///
        ///
        ///
        ///
        ///

        // Method used to build a query
        public string BuildQuery()
        {
            string query = "";

            if (!string.IsNullOrEmpty(DlQueryBox.Text))
            {
                //Prepare rating query
                string rating = "";

                if (Safe_Switch.IsChecked != null && (bool)Safe_Switch.IsChecked)
                    rating += "safe ";
                if (Questionable_Switch.IsChecked != null && (bool)Questionable_Switch.IsChecked)
                    rating += "questionable ";
                if (Explicit_Switch.IsChecked != null && (bool)Explicit_Switch.IsChecked)
                    rating += "explicit ";
                rating = rating.TrimEnd(' ').Replace(" ", "+OR+");

                if (rating != "")
                    rating = "%2C+(" + rating + ")";

                //Prepare score query
                string score = "";

                if (MinimumScore_Numeric.Value != null && MaximumScore_Numeric.Value != null)
                {
                    score += "(score.gte:" + MinimumScore_Numeric.Value;
                    score += " AND ";
                    score += "score.lte:" + MaximumScore_Numeric.Value;
                    score += ")";
                }
                else
                {
                    if (MinimumScore_Numeric.Value != null)
                        score += "score.gte:" + MinimumScore_Numeric.Value;
                    if (MaximumScore_Numeric.Value != null)
                        score += "score.lte:" + MaximumScore_Numeric.Value;
                }

                if (score != "")
                    score = "%2C+" + score;


                // Prepare API query
                query += "https://derpibooru.org/search.json?q=";
                query += DlQueryBox.Text
                        .TrimEnd(',')
                        .TrimEnd(' ')
                        .Replace(" ", "+")
                        .Replace(",", "%2C"); // clean up query
                query += rating;
                query += score;
                query += "&page=" + DlPage.Value;
                query += "&key=" + _settings.Token;
            }
            else
            {
                testBox.Text = "query cannot be empty";
            }

            // Return
            return query;
        }


        // Method used to populate the grid
        public void PopulateGrid(string query)
        {
            // Prepare json
            string json = JsonGetter.GetJson(query);

            Derpi derpi = Derpi.FromJson(json);

            testBox.Text = query + " at " + _settings.DownloadLocation;

            // Go through images in the search
            if (derpi == null)
            {
                testBox.Text = "No results for: " + query;
            }
            else
            {
                foreach (var search in derpi.Search)
                {
                    // Check if artist is given
                    string[] tags = search.Tags.Split(',').Select(sValue => sValue.Trim()).ToArray();
                    var artists = tags.Where(it => it.StartsWith("artist:"))
                        .Select(it => it.Replace("artist:", null));

                    // Format artists
                    string artist = artists.Aggregate("", (current, v) => current + (v.ToString() + ","));
                    artist = artist.TrimEnd(',');

                    // Add to list of downloaded files
                    FilesCollection.Add(new FileDisplay(search.FileName,
                        search.MimeType,
                        search.SourceUrl,
                        artist,
                        search.Tags,
                        search.Description,
                        (int)search.Faves,
                        "https:" + search.Representations.Thumb,
                        "https:" + search.Image,
                        search.Id));

                    // Log the file
                    DownloadLog.Text += search.Image + Environment.NewLine;

                }

                // Up page number
                DlPage.Value = DlPage.Value + 1;

                // Up the file count
                TotalFiles = FilesCollection.Count();
                FileIndexTxt.Text = CurrentFileNumber + "/" + TotalFiles;

            }
        }



    }
}
