using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

            Navigate("Startseite");

            getComPorts();
        }

        private void getComPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            cmbPorts.Items.Clear();

            foreach (string portName in ports)
            {
                cmbPorts.Items.Add(portName);
            }

            if (cmbPorts.Items.Count > 0)
                cmbPorts.SelectedIndex = 0;
        }

        private void getRecipes()
        {
            if (Recipes.LeseRezepte("Rezepte") == 0)
            {
                lstRecipe.ItemsSource = null;
                lstRecipe.ItemsSource = Recipes.RezepteListe;

                if (lstRecipe.Items.Count > 1)
                    ChangeStatus($"{lstRecipe.Items.Count} Rezepte wurden ausgelesen.", MainWindow.IconType.Information);
                else
                    ChangeStatus("1 Rezept wurde ausgelesen.", MainWindow.IconType.Information);

                lstRecipe.SelectedIndex = 0;
            }
            else
                ChangeStatus("Ein Fehler beim auslesen der Rezepte ist aufgetreten!", MainWindow.IconType.Error);
        }

        private void Navigate(string headerText)
        {
            foreach (TabItem item in TabMain.Items)
            {
                if (item.Header.ToString() == headerText)
                    item.IsSelected = true;
                else
                    item.IsSelected = false;
            }

            Header.Content = headerText;
        }

        private void nav_HomePage_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Startseite");
        }

        private void nav_RecipePage_Click(object sender, RoutedEventArgs e)
        {
            getRecipes();
            Navigate("Rezepte");
        }

        private void navConnPage_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Verbindung");
        }

        private void navSettings_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Einstellungen");
        }

        /// <summary>
        /// Legt den anzuzeigenden Status in der StatusBar des Hauptfensters fest
        /// </summary>
        /// <param name="strMessage">Statusnachricht</param>
        /// <param name="icon">Statusicon</param>
        public void ChangeStatus(string strMessage, IconType icon)
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

            _statImage.Source = new BitmapImage(new Uri($"pack://application:,,,/{strSource}"));
        }

        private void btnRefPorts_Click(object sender, RoutedEventArgs e)
        {
            getComPorts();
        }
    }
}