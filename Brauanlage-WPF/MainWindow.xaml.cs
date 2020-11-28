using System;
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

        private static SerialPort _serialPort;

        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);

        private delegate void SetTextCallback(string text);

        private string InputData = String.Empty;

        private bool bolSerConnJustOpened, bolSerConnEstablished;

        public MainWindow()
        {
            InitializeComponent();

            Navigate("Startseite");

            getComPorts();
        }

        private void serialPort_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            InputData = _serialPort.ReadExisting();
            if (InputData != String.Empty)
            {
                Dispatcher.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
            }
        }

        /// <summary>
        /// Hier werden Daten des Seriellen Ports verarbeitet
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            if (bolSerConnJustOpened && text.Contains("ELV USB-I2C-Interface"))
            {
                ChangeStatus("Serieller Port wurde geöffnet und I2C-Adapter ist erreichbar.", IconType.Information);
                _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_green.png"));
                bolSerConnJustOpened = false;
                bolSerConnEstablished = true;
            }

            txtLog.Text += text;
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

        private void navInOutPage_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Ein-/Ausgänge");
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

        private void btnRelais1_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnConnSerial_Click(object sender, RoutedEventArgs e)
        {
            if (txtConnStatus.Text == "Nicht verbunden")
            {
                _serialPort = new SerialPort
                {
                    PortName = cmbPorts.SelectedItem.ToString(),
                    BaudRate = Convert.ToInt32(txtBaud.Text),
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,

                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                try
                {
                    if (!_serialPort.IsOpen)
                        _serialPort.DataReceived += serialPort_DataReceived_1;
                    bolSerConnJustOpened = true;
                    _serialPort.Open();

                    txtConnStatus.Text = "Verbunden";
                    btnConnSerialText.Content = "Trennen";
                    _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_orange.png"));
                    ChangeStatus("Serieller Port wird geöffnet...", IconType.Information);
                    _serialPort.Write("?");
                }
                catch
                {
                    _serialPort.Close();
                    bolSerConnJustOpened = false;
                    bolSerConnEstablished = false;
                    btnConnSerialText.Content = "Verbinden";
                    txtConnStatus.Text = "Nicht verbunden";
                    _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_red.png"));
                    ChangeStatus("Fehler beim öffnen des seriellen Ports!", IconType.Error);
                }
            }
            else
            {
                _serialPort.Close();
                _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_red.png"));
                bolSerConnJustOpened = false;
                bolSerConnEstablished = false;
                _serialPort.DataReceived -= serialPort_DataReceived_1;
                btnConnSerialText.Content = "Verbinden";
                txtConnStatus.Text = "Nicht verbunden";
            }
        }

        private void btnClearConnLog_Click(object sender, RoutedEventArgs e)
        {
            txtLog.Text = "";
        }
    }
}