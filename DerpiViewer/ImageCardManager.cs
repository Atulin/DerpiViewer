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
    public class ImageCardManager
    {
        public static void AddCard (StackPanel sp, int id, string author, string source)
        {
            StackPanel btnBody = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            btnBody.Children.Add(new Label { Content = id });
            btnBody.Children.Add(new Label { Content = author });
            btnBody.Children.Add(new Label { Content = source });

            Button btn = new Button
            {
                Content = btnBody
            };

            sp.Children.Add(new Button());
        }
    }
}
