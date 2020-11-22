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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using Brauanlage_WPF.Pages;

namespace Brauanlage_WPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum IconType
        {
            Information = 0,
            Error = 1,
            Warning = 2
        }

        public MainWindow()
        {
            InitializeComponent();
            Navigate("Pages/HomePage.xaml", "Startseite");
        }

        private void Navigate(string path, string headerText)
        {
            _NavigationFrame.Navigate(new Uri(path, UriKind.Relative));
            Header.Content = headerText;
        }

        private void nav_HomePage_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Pages/HomePage.xaml", "Startseite");
        }

        private void nav_RecipePage_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Pages/RecipePage.xaml", "Rezepte");
        }

        private void navConnPage_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Pages/ConnectionPage.xaml", "Verbindung");
        }

        private void navSettings_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Pages/SettingsPage.xaml", "Einstellungen");
        }

        public static void ChangeStatus(string strMessage, IconType icon)
        {
            string strSource = "";

            _statText.Text = strMessage;

            switch (icon)
            {
                case IconType.Information:
                    strSource = "Images/information.png";
                    break;
                case IconType.Error:
                    strSource = "Images/exclamation.png";
                    break;
                case IconType.Warning:
                    strSource = "Images/error.png";
                    break;
                default:
                    break;
            }

            _statImage.Source = new BitmapImage(new Uri($"ms-appx:///{strSource}"));
        }
    }
}
