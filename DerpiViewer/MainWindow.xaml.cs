using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.ComponentModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;

namespace DerpiViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // Initialize download path
        string dlPath;

        // Initialize list of files downloaded this session
        ObservableCollection<FileDisplay> downloadedFiles = new ObservableCollection<FileDisplay>();
        public ObservableCollection<FileDisplay> FilesCollection
        {
            get { return this.downloadedFiles; }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            FileGrid.ItemsSource = FilesCollection;

            // Get app accent and theme from settings and set it
            ThemeManager.ChangeAppStyle(Application.Current,
                                            ThemeManager.GetAccent(Properties.Settings.Default.Accent),
                                            ThemeManager.GetAppTheme(Properties.Settings.Default.Theme));

            // Set accent combobox data source
            Accent_ComboBox.ItemsSource = Enum.GetValues(typeof(LooksManager.Accents)).Cast<LooksManager.Accents>();

            // Set accent combobox item based on settings
            Accent_ComboBox.SelectedIndex = (int)Enum.Parse(typeof(LooksManager.Accents), Properties.Settings.Default.Accent);

            // Set theme button icon based on settings
            if (Properties.Settings.Default.Theme == "BaseLight")
            {
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SunRegular;
                darkmodeIconToggle = false;
            }
            else
            {
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.MoonRegular;
                darkmodeIconToggle = true;
            }

            // Set download path
            if (Properties.Settings.Default.DownloadLocation == "")
            {
                Properties.Settings.Default.DownloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\DerpiViewer\";
            }

            dlPath = Properties.Settings.Default.DownloadLocation;

            // Create the directory if doesn't exist
            System.IO.Directory.CreateDirectory(dlPath);

            // Set path text
            DlPathBox.Text = Properties.Settings.Default.DownloadLocation;

            // Set token
            TokenBox.Text = Properties.Settings.Default.Token;
        }

        // Handle main menu open/close
        bool menuToggle = false;
        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout.IsOpen = !menuToggle;
            menuToggle = !menuToggle;

            // Save settings
            Properties.Settings.Default.Token = TokenBox.Text;
            Properties.Settings.Default.Save();
        }

        // Handle accent selection combobox
        private void Accent_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

            ThemeManager.ChangeAppStyle(Application.Current,
                                        ThemeManager.GetAccent(Accent_ComboBox.SelectedItem.ToString()),
                                        theme.Item1);

            // Save in settings
            Properties.Settings.Default.Accent = Accent_ComboBox.SelectedItem.ToString();
        }

        // Handle theme selection switch
        private bool darkmodeIconToggle = false;
        private void DarkMode_Toggle_Click(object sender, RoutedEventArgs e)
        {
            Tuple<AppTheme, Accent> completeTheme = ThemeManager.DetectAppStyle(Application.Current);
            string theme = "";

            if (darkmodeIconToggle)
            {
                theme = "BaseLight";

                darkmodeIconToggle = false;
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SunRegular;
            }
            else
            {
                theme = "BaseDark";

                darkmodeIconToggle = true;
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.MoonRegular;
            }

            // Apply chosen theme
            ThemeManager.ChangeAppStyle(Application.Current,
                                            completeTheme.Item2,
                                            ThemeManager.GetAppTheme(theme));

            // Save in settings
            Properties.Settings.Default.Theme = theme;
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

            // Save in settings
            Properties.Settings.Default.MinifiedTitle = ShowTitleBar;
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
            Properties.Settings.Default.Token = TokenBox.Text;
            // Save settings
            Properties.Settings.Default.Save();
        }

        // Download
        private void DlBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DlQueryBox.Text != "" && DlQueryBox.Text != null)
            {
                // Prepare API query
                string query = "";
                query += "https://derpibooru.org/search.json?q=";
                query += DlQueryBox.Text.TrimEnd(',').TrimEnd(' ').Replace(" ", "+").Replace(",", "%2C"); // clean up query
                query += "&page=" + DlPage.Value;
                query += "&key=" + Properties.Settings.Default.Token;

                // Prepare json
                string json = JsonGetter.GetJson(query);

                Derpi derpi = Derpi.FromJson(json);

                testBox.Text = query + " at " + Properties.Settings.Default.DownloadLocation; 

                // Go through images in the search
                foreach (var search in derpi.Search)
                {
                    // Check if artist is given
                    string[] tags = search.Tags.Split(',');
                    var artists = tags.Where(it => it.StartsWith("artist:")).Select(it => it.Replace("artist:", null));

                    // Format artists
                    string artist = "";
                    foreach (var v in artists)
                        artist += v.ToString() + ",";
                    artist = artist.TrimEnd(',');

                    // Prepare file name
                    string filename = "";
                    filename += search.Id;
                    filename += "_";
                    filename += System.IO.Path.GetFileNameWithoutExtension(search.FileName);
                    filename += "_by_";
                    if (artist != "")
                        filename += artist;
                    else
                        filename += "no-author";
                    filename += ".";
                    filename += search.OriginalFormat;

                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFileAsync(new Uri("https:" + search.Image), Properties.Settings.Default.DownloadLocation + filename);
                    }

                    // Add to list of downloaded files
                    downloadedFiles.Add(new FileDisplay(search.FileName,
                                                        search.SourceUrl,
                                                        artist,
                                                        search.Description,
                                                        (int)search.Faves,
                                                        "https:" + search.Representations.Small,
                                                        "https:" + search.Image));
                    
                }

                // Up page number
                DlPage.Value = DlPage.Value + 1;

            }
            else
            {
                testBox.Text = "query cannot be empty";
            }


        }

        // Set download path
        private void SelectDownloadFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            // Initialize folder selection window

            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select download folder";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Properties.Settings.Default.DownloadLocation;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = Properties.Settings.Default.DownloadLocation;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dlg.FileName;

                Properties.Settings.Default.DownloadLocation = folder + @"\";

                // Save settings
                Properties.Settings.Default.Save();

                // Set path text
                DlPathBox.Text = Properties.Settings.Default.DownloadLocation;
            }

        }

        // Handle token help
        private void TokenHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        // Handle opening destination folder
        private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Settings.Default.DownloadLocation);
        }

        // Handle query change
        private void DlQueryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DlPage.Value = 1;
        }
    }
}
