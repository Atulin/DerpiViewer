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

        // Currently displayed file
        public FileDisplay CurrentFile;
        public int CurrentFileId;

        // Settings
        private readonly Settings _settings = Settings.Default;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            FileGrid.ItemsSource = FilesCollection;

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

                if (MinimumScore_Numeric.Value != null && MaxiumumScore_Numeric.Value != null)
                {
                    score += "(score.gte:" + MinimumScore_Numeric.Value;
                    score += " AND ";
                    score += "score.lte:" + MaxiumumScore_Numeric.Value;
                    score += ")";
                }
                else
                {
                    if (MinimumScore_Numeric.Value != null)
                        score += "score.gte:" + MinimumScore_Numeric.Value;
                    if (MaxiumumScore_Numeric.Value != null)
                        score += "score.lte:" + MaxiumumScore_Numeric.Value;
                }

                if (score != "")
                    score = "%2C+" + score;


                // Prepare API query
                string query = "";
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
                        string[] tags = search.Tags.Split(',');
                        var artists = tags.Where(it => it.StartsWith("artist:"))
                            .Select(it => it.Replace("artist:", null));

                        // Format artists
                        string artist = artists.Aggregate("", (current, v) => current + (v.ToString() + ","));
                        artist = artist.TrimEnd(',');

                        // Prepare file name
                        var sb = new System.Text.StringBuilder();

                        sb.Append(search.Id)
                            .Append("_")
                            .Append(Path.GetFileNameWithoutExtension(search.FileName))
                            .Append("_by_")
                            .Append(artist != "" ? artist : "no-author")
                            .Append(".")
                            .Append(search.OriginalFormat);

                        string filename = sb.ToString();

                        // Add to list of downloaded files
                        FilesCollection.Add(new FileDisplay(search.FileName,
                            search.SourceUrl,
                            artist,
                            search.Tags,
                            search.Description,
                            (int) search.Faves,
                            "https:" + search.Representations.Thumb,
                            "https:" + search.Image));

                        // Log the file
                        DownloadLog.Text += search.Image + Environment.NewLine;

                    }

                    // Up page number
                    DlPage.Value = DlPage.Value + 1;
                }

            }
            else
            {
                testBox.Text = "query cannot be empty";
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
        }

        // Handle download button clicked
        private void Download_Btn_Click(object sender, RoutedEventArgs e)
        {
            FileDisplay fd = ((FrameworkElement)sender).DataContext as FileDisplay;

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

            if (fd != null) BrowserWindow.Address = fd.File;
            CloseImageView.Visibility = Visibility.Visible;
            Display_Flyout.IsOpen = true;
        }

        // Close file display
        private void CloseImageView_Click(object sender, RoutedEventArgs e)
        {
            Display_Flyout.IsOpen = false;
            CloseImageView.Visibility = Visibility.Collapsed;
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
                        break;

                    case Key.D:
                        BrowserWindow.Address = FilesCollection[(CurrentFileId + 1).Clamp(0, FilesCollection.Count - 1)].File;
                        CurrentFileId = (CurrentFileId + 1).Clamp(0, FilesCollection.Count - 1);

                        // If nearing the end, fetch more
                        if (CurrentFileId == FilesCollection.Count - 3)
                            FetchBtn_Click(sender, new RoutedEventArgs());

                        break;

                    case Key.W:
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFileAsync(new Uri(CurrentFile.File), _settings.DownloadLocation + CurrentFile.Filename);
                        }
                        break;

                    case Key.Escape:
                        CloseImageView_Click(sender, new RoutedEventArgs());
                        break;
                }
            }
        }
    }
}
