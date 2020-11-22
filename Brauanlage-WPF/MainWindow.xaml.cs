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

        public MainWindow()
        {
            InitializeComponent();
            Navigate("Pages/HomePage.xaml", "Startseite");
        }

        private void Navigate(string path, string headerText)
        {
            this._NavigationFrame.Navigate(new Uri(path, UriKind.Relative));
            this.Header.Content = headerText;
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
    }
}
