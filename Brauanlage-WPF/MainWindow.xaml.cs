using System;
using System.Collections;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Brauanlage_WPF
{
    //TODO: Benutzereinstellungen speichern

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

        private enum ConnectionStatus
        {
            None, Connecting, Connected, Disconnected
        }

        //TWI Request Rückgabewert
        private ConnectionStatus twiConnStat;

        //Timer für autom. Aktualisierung der Status-Register
        private DispatcherTimer tmrUpdate;

        //TWI Statusregister
        //Relais-Ausgänge
        private BitArray bytRelaisState;
        //Digital-Eingänge
        private BitArray bytInputState;
        //4...20mA Eingänge 1 - 4
        private BitArray bytArrAnCurrIn1, bytArrAnCurrIn2, bytArrAnCurrIn3, bytArrAnCurrIn4;
        //Perzentile angaben
        private double dblAnCurrIn1, dblAnCurrIn2, dblAnCurrIn3, dblAnCurrIn4;
        //ADC Eingänge 5 - 8
        private BitArray bytArrAnIn5, bytArrAnIn6, bytArrAnIn7, bytArrAnIn8;


        private static SerialPort _serialPort;

        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);

        private delegate void SetTextCallback(string text);

        private string InputData = String.Empty;

        public MainWindow()
        {
            InitializeComponent();

            //TODO: Register initialisieren
            bytRelaisState = new BitArray(8, false);
            bytInputState = new BitArray(8, false);
            bytArrAnCurrIn1 = new BitArray(10, false);
            bytArrAnCurrIn2 = new BitArray(10, false);
            bytArrAnCurrIn3 = new BitArray(10, false);
            bytArrAnCurrIn4 = new BitArray(10, false);
            bytArrAnIn5 = new BitArray(10, false);
            bytArrAnIn6 = new BitArray(10, false);
            bytArrAnIn7 = new BitArray(10, false);
            bytArrAnIn8 = new BitArray(10, false);

            tmrUpdate = new DispatcherTimer();

            sldFU1.Value = 0;
            sldFU2.Value = 0;

            Navigate("Startseite");

            ChangeStatus("Die Brauanlagensteuerung wurde gestartet", IconType.Information);

            getComPorts();

            //TODO: Einstellungen laden
            txtUpdateSpeed.Text = Properties.Settings.Default["UpdateSpeed"].ToString();
        }

        private void setUpdateTimer()
        {
            tmrUpdate.Tick += OnUpdateTimer_Tick;
            //2 Sek
            tmrUpdate.Interval = new TimeSpan(0, 0, Convert.ToInt32(Properties.Settings.Default["UpdateSpeed"]));
        }

        private void OnUpdateTimer_Tick(object sender, EventArgs e)
        {
            readStatus();
        }

        /// <summary>
        /// Aktualisiert die Wertefelder der Analogen 4...20mA Eingänge
        /// </summary>
        private void changeCurrentInputStates()
        {
            int intBuffCurrIn1, intBuffCurrIn2, intBuffCurrIn3, intBuffCurrIn4;

            intBuffCurrIn1 = getIntFromBitArray(bytArrAnCurrIn1);
            intBuffCurrIn2 = getIntFromBitArray(bytArrAnCurrIn2);
            intBuffCurrIn3 = getIntFromBitArray(bytArrAnCurrIn3);
            intBuffCurrIn4 = getIntFromBitArray(bytArrAnCurrIn4);

            //Globale Prozentwerte setzen
            dblAnCurrIn1 = map(intBuffCurrIn1, 204, 1023, 0, 1000) / 10.0;
            dblAnCurrIn2 = map(intBuffCurrIn2, 204, 1023, 0, 1000) / 10.0;
            dblAnCurrIn3 = map(intBuffCurrIn3, 204, 1023, 0, 1000) / 10.0;
            dblAnCurrIn4 = map(intBuffCurrIn4, 204, 1023, 0, 1000) / 10.0;

            if (dblAnCurrIn1 < 0)
                dblAnCurrIn1 = 0;

            if (dblAnCurrIn2 < 0)
                dblAnCurrIn2 = 0;

            if (dblAnCurrIn3 < 0)
                dblAnCurrIn3 = 0;

            if (dblAnCurrIn4 < 0)
                dblAnCurrIn4 = 0;

            txtINCurr1.Text = dblAnCurrIn1.ToString("F1") + "% - " + (map(intBuffCurrIn1, 204, 1023, 4000, 20000) / 1000.0).ToString("F2") + " mA";
            txtINCurr2.Text = dblAnCurrIn2.ToString("F1") + "% - " + (map(intBuffCurrIn2, 204, 1023, 4000, 20000) / 1000.0).ToString("F2") + " mA";
            txtINCurr3.Text = dblAnCurrIn3.ToString("F1") + "% - " + (map(intBuffCurrIn3, 204, 1023, 4000, 20000) / 1000.0).ToString("F2") + " mA";
            txtINCurr4.Text = dblAnCurrIn4.ToString("F1") + "% - " + (map(intBuffCurrIn4, 204, 1023, 4000, 20000) / 1000.0).ToString("F2") + " mA";
        }


        /// <summary>
        /// Aktualisiert die Wertefelder der Analogen Eingänge 5 bis 8
        /// </summary>
        private void changeInputStates()
        {
            int intBuffIn5, intBuffIn6, intBuffIn7, intBuffIn8;

            intBuffIn5 = getIntFromBitArray(bytArrAnIn5);
            intBuffIn6 = getIntFromBitArray(bytArrAnIn6);
            intBuffIn7 = getIntFromBitArray(bytArrAnIn7);
            intBuffIn8 = getIntFromBitArray(bytArrAnIn8);

            txtIN5.Text = (map(intBuffIn5, 0, 1023, 0, 5000) ).ToString("F2") + " mV";
            txtIN6.Text = (map(intBuffIn6, 0, 1023, 0, 5000) ).ToString("F2") + " mV";
            txtIN7.Text = (map(intBuffIn7, 0, 1023, 0, 5000) ).ToString("F2") + " mV";
            txtIN8.Text = (map(intBuffIn8, 0, 1023, 0, 5000) ).ToString("F2") + " mV";
        }

        /// <summary>
        /// Ändere Image des Relais Buttons anhand des Ralais-Status
        /// </summary>
        /// <param name="intBitStelle">Stelle des Bits, im Relais-Status-Byte</param>
        /// <param name="imgControl">Image-Control, welches verändert werden soll</param>
        private void changeRelaisStateIcon(int intBitStelle, Image imgControl)
        {
            string strImage;

            if (bytRelaisState.Get(intBitStelle - 1))
                strImage = "bullet_green.png";
            else
                strImage = "bullet_red.png";

            imgControl.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{strImage}"));
        }

        /// <summary>
        /// Ändere Image des Eingang Buttons anhand des Eingang-Status
        /// </summary>
        /// <param name="intBitStelle">Stelle des Bits, im Eingang-Status-Byte</param>
        /// <param name="imgControl">Image-Control, welches verändert werden soll</param>
        private void changeInputStateIcon(int intBitStelle, Image imgControl)
        {
            string strImage;

            if (bytInputState.Get(intBitStelle - 1))
                strImage = "green.png";
            else
                strImage = "red.png";

            imgControl.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{strImage}"));
        }

        /// <summary>
        /// Fordert den Status der Relais und Eingänge an
        /// </summary>
        private void readStatus()
        {
            //Setze Register auf 0x00
            twiWrite("00", false);
            //Fordere 10 Bytes an
            twiRequest("14", false);
        }

        private void serialPort_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            InputData = _serialPort.ReadExisting();
            if (InputData != String.Empty)
            {
                Dispatcher.BeginInvoke(new SetTextCallback(receiveSerialData), new object[] { InputData });
            }
        }

        /// <summary>
        /// Bildet einen Wertebereich auf einen anderen, definierten ab
        /// </summary>
        /// <param name="value">Wert, welcher umgewandelt werden soll</param>
        /// <param name="fromLow"></param>
        /// <param name="fromHigh"></param>
        /// <param name="toLow"></param>
        /// <param name="toHigh"></param>
        /// <returns></returns>
        private static int map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        /// <summary>
        /// Konvertiere BitArray in einen Integer32
        /// </summary>
        /// <param name="bitArray">Zu konvertierendes BitArray</param>
        /// <returns></returns>
        private static int getIntFromBitArray(BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException("Das BitArray ist zu groß! (max. 32 Bit)");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }

        /// <summary>
        /// Konvertiere Hex-String in Bytes
        /// </summary>
        /// <param name="hex">Zu konvertierende Hex-Zeichen</param>
        /// <returns></returns>
        private static byte[] hexStringToByte(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("Ungerade Anzahl an Bits im Byte");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        private static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// Konvertiere ein BitArray zu einem Hex-String z.B. FF, 0A
        /// </summary>
        /// <param name="btArr">Zu konvertierendes BitArray</param>
        /// <returns></returns>
        private static string toHexString(BitArray btArr)
        {
            byte[] byteArray = BitArrayToByteArray(btArr);
            string hexString = BitConverter.ToString(byteArray);

            return hexString.Replace('-', ' ');
        }

        /// <summary>
        /// Hier werden Daten des Seriellen Ports verarbeitet
        /// </summary>
        /// <param name="text"></param>
        private void receiveSerialData(string strData)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strDataArr = strData.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            //Erste Verbindung mit serieller Schnittstelle
            if (twiConnStat == ConnectionStatus.Connecting && strDataArr[0].Contains("ELV USB-I2C-Interface"))
            {
                ChangeStatus("Der serielle Port wurde geöffnet und der TWI-Adapter ist erreichbar.", IconType.Information);
                _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_green.png"));
            }
            //Warte noch auf letzte Statusnachrichten
            else if (twiConnStat == ConnectionStatus.Connecting)
            {
                if (strDataArr[strDataArr.Length - 1].Contains("Y70") || strDataArr[strDataArr.Length - 1].Contains("Y71"))
                {
                    twiConnStat = ConnectionStatus.Connected;

                    readStatus();
                    //Update Timer starten
                    setUpdateTimer();
                    tmrUpdate.Start();
                }
            }
            //Verbunden
            else if (twiConnStat == ConnectionStatus.Connected)
            {
                if (strData.Contains("Err:TWI") || strData.Contains("Buffer full") || strData.Contains("Solve I2C-Bus-Lock"))
                {
                    if (strData.Contains("Err:TWI START"))
                    {
                        ChangeStatus("Die Brauanlagensteuerung ist nicht erreichbar!", IconType.Error);
                        tmrUpdate.Stop();
                    }
                    else
                    {
                        ChangeStatus("Ein Fehler ist auf dem TWI aufgetreten!", IconType.Error);
                    }
                }
                else
                {
                    //Daten einlesen
                    string[] strArray = strDataArr[0].Split(' ');

                    //Relais - 0x00
                    #region Relais
                    bytRelaisState = new BitArray(hexStringToByte(strArray[0]));

                    changeRelaisStateIcon(1, imgBtnRelais1);
                    changeRelaisStateIcon(2, imgBtnRelais2);
                    changeRelaisStateIcon(3, imgBtnRelais3);
                    changeRelaisStateIcon(4, imgBtnRelais4);
                    changeRelaisStateIcon(5, imgBtnRelais5);
                    changeRelaisStateIcon(6, imgBtnRelais6);
                    changeRelaisStateIcon(7, imgBtnRelais7);
                    changeRelaisStateIcon(8, imgBtnRelais8);
                    #endregion

                    //Eingänge - 0x01
                    #region Eingänge
                    bytInputState = new BitArray(hexStringToByte(strArray[1]));

                    changeInputStateIcon(1, imgInput1);
                    changeInputStateIcon(2, imgInput2);
                    changeInputStateIcon(3, imgInput3);
                    changeInputStateIcon(4, imgInput4);
                    changeInputStateIcon(5, imgInput5);
                    changeInputStateIcon(6, imgInput6);
                    changeInputStateIcon(7, imgInput7);
                    changeInputStateIcon(8, imgInput8);
                    #endregion

                    //4...20mA Eingänge 0x02...0x09
                    #region 4...20mA Eingänge
                    Byte[] bytArray = { 0x00, 0x00 };

                    bytArray[0] = hexStringToByte(strArray[3])[0];
                    bytArray[1] = hexStringToByte(strArray[2])[0];

                    bytArrAnCurrIn1 = new BitArray(bytArray);

                    bytArray[0] = hexStringToByte(strArray[5])[0];
                    bytArray[1] = hexStringToByte(strArray[4])[0];

                    bytArrAnCurrIn2 = new BitArray(bytArray);

                    bytArray[0] = hexStringToByte(strArray[7])[0];
                    bytArray[1] = hexStringToByte(strArray[6])[0];

                    bytArrAnCurrIn3 = new BitArray(bytArray);

                    bytArray[0] = hexStringToByte(strArray[9])[0];
                    bytArray[1] = hexStringToByte(strArray[8])[0];

                    bytArrAnCurrIn4 = new BitArray(bytArray);

                    changeCurrentInputStates();
                    #endregion

                    //Analog Eingänge 0x0A...0x11
                    #region ADC Eingänge
                    bytArray = new Byte[] { 0x00, 0x00 };

                    bytArray[0] = hexStringToByte(strArray[11])[0];
                    bytArray[1] = hexStringToByte(strArray[10])[0];

                    bytArrAnIn5 = new BitArray(bytArray);

                    bytArray[0] = hexStringToByte(strArray[13])[0];
                    bytArray[1] = hexStringToByte(strArray[12])[0];

                    bytArrAnIn6 = new BitArray(bytArray);

                    bytArray[0] = hexStringToByte(strArray[15])[0];
                    bytArray[1] = hexStringToByte(strArray[14])[0];

                    bytArrAnIn7 = new BitArray(bytArray);

                    bytArray[0] = hexStringToByte(strArray[17])[0];
                    bytArray[1] = hexStringToByte(strArray[16])[0];

                    bytArrAnIn8 = new BitArray(bytArray);

                    changeInputStates();
                    #endregion
                }
            }
            //Log schreiben
            txtLog.AppendText(strData + "\r\n");
            txtLog.ScrollToEnd();
        }

        /// <summary>
        /// Sende einen benutzerdefinierten Befehl über den seriellen Port
        /// </summary>
        /// <param name="strCommand">Zu sendener Befehl</param>
        /// <param name="bolShowStatus">Soll in der StatusBar angezeigt werden, dass ein Befehl gesendet wurde?</param>
        private void sendCommand(string strCommand, bool bolShowStatus = false)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Write(strCommand);

                txtLog.AppendText(strCommand + "\r\n");
                txtLog.ScrollToEnd();

                if (bolShowStatus)
                    ChangeStatus("Ein serieller Befehl wurde gesendet", IconType.Information);
            }
            else
            {
                ChangeStatus("Die serielle Verbindung ist getrennt!", IconType.Error);
                tmrUpdate.Stop();
            }
        }

        private void twiWrite(string strCommand, bool bolShowStatus)
        {
            if (_serialPort != null && _serialPort.IsOpen && twiConnStat == ConnectionStatus.Connected)
            {
                //Start-Bedingung + Slave-Adresse + Write-Bit (0x00)
                string strBufferCommand = "S" + (Convert.ToByte(txtSlaveAddress.Text) + 0).ToString();

                //Befehl
                strBufferCommand += strCommand;

                //Stopp-Bedingung
                strBufferCommand += "P";

                _serialPort.Write(strBufferCommand);

                txtLog.AppendText(strBufferCommand + "\r\n");
                txtLog.ScrollToEnd();

                if (bolShowStatus)
                    ChangeStatus("Ein serieller Befehl wurde gesendet", IconType.Information);
            }
            else
            {
                twiConnStat = ConnectionStatus.Disconnected;
                ChangeStatus("Die serielle Verbindung ist getrennt!", IconType.Error);
                tmrUpdate.Stop();
            }
        }

        private void twiRequest(string strCommand, bool bolShowStatus)
        {
            if (_serialPort != null && _serialPort.IsOpen && twiConnStat == ConnectionStatus.Connected)
            {
                //Start-Bedingung + Slave-Adresse + Read-Bit (0x01)
                string strBufferCommand = "S" + (Convert.ToByte(txtSlaveAddress.Text) + 1).ToString();

                //Befehl
                strBufferCommand += strCommand;

                //Stopp-Bedingung
                strBufferCommand += "P";

                _serialPort.Write(strBufferCommand);

                txtLog.AppendText(strBufferCommand + "\r\n");
                txtLog.ScrollToEnd();

                if (bolShowStatus)
                    ChangeStatus("Ein serieller Befehl wurde gesendet", IconType.Information);
            }
            else
            {
                twiConnStat = ConnectionStatus.Disconnected;
                ChangeStatus("Die serielle Verbindung ist getrennt!", IconType.Error);
            }
        }

        private void switchRelais(int intRelaisNumber, bool bolState)
        {
            if (intRelaisNumber == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    bytRelaisState[i] = bolState;
                }
            }
            else
                bytRelaisState[intRelaisNumber - 1] = bolState;

            //Register 0x14
            twiWrite("14" + toHexString(bytRelaisState), false);

            readStatus();
        }

        /// <summary>
        /// Fragt nach gedrückter [ENTER] Taste ab.
        /// Zum senden eines Befehls an den seriellen Port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridConn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox txtBox = e.Source as TextBox;
                if (txtBox != null)
                {
                    sendCommand(txtConnCommand.Text, true);
                }
            }
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
                ChangeStatus("Ein Fehler beim Auslesen der Rezepte ist aufgetreten!", MainWindow.IconType.Error);
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
                    txtStatusLog.AppendText("INFO: ");
                    break;

                case IconType.Error:
                    strSource = "Images/exclamation.png";
                    System.Media.SystemSounds.Beep.Play();
                    txtStatusLog.AppendText("FEHLER: ");
                    break;

                case IconType.Warning:
                    strSource = "Images/error.png";
                    txtStatusLog.AppendText("WARNUNG: ");
                    break;
            }

            txtStatusLog.AppendText(strMessage + "\r\n");
            _statImage.Source = new BitmapImage(new Uri($"pack://application:,,,/{strSource}"));
        }

        private void btnRefPorts_Click(object sender, RoutedEventArgs e)
        {
            getComPorts();
        }

        private void btnConnSerial_Click(object sender, RoutedEventArgs e)
        {
            if (txtConnStatus.Text == "Nicht verbunden")
            {
                twiConnStat = ConnectionStatus.Connecting;

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

                    _serialPort.Open();

                    txtConnStatus.Text = "Verbunden";
                    btnConnSerialText.Content = "Trennen";
                    _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_orange.png"));
                    ChangeStatus("Der Serielle Port wird geöffnet...", IconType.Information);
                    _serialPort.Write("?");
                }
                catch
                {
                    _serialPort.Close();
                    twiConnStat = ConnectionStatus.Disconnected;
                    btnConnSerialText.Content = "Verbinden";
                    txtConnStatus.Text = "Nicht verbunden";
                    _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_red.png"));
                    ChangeStatus("Fehler beim öffnen des seriellen Ports!", IconType.Error);
                }
            }
            else
            {
                _serialPort.Close();
                ChangeStatus("Der serielle Port ist geschlossen!", IconType.Information);

                if (tmrUpdate != null)
                {
                    tmrUpdate.Stop();
                }
                twiConnStat = ConnectionStatus.Disconnected;
                _statConnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/bullet_red.png"));
                _serialPort.DataReceived -= serialPort_DataReceived_1;
                btnConnSerialText.Content = "Verbinden";
                txtConnStatus.Text = "Nicht verbunden";
            }
        }

        private void btnConnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            sendCommand(txtConnCommand.Text, true);
        }

        private void btnClearConnLog_Click(object sender, RoutedEventArgs e)
        {
            txtLog.Text = "";
        }

        private void btnClearStatusLog_Click(object sender, RoutedEventArgs e)
        {
            txtStatusLog.Text = "";
        }

        /// <summary>
        /// Überprüft ob die Eingabe numerisch ist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValtxt(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #region RelaisButton Click Events

        private void btnRelais1_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(1, !bytRelaisState[0]);
        }

        private void btnRelais2_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(2, !bytRelaisState[1]);
        }

        private void btnRelais3_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(3, !bytRelaisState[2]);
        }

        private void btnRelais4_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(4, !bytRelaisState[3]);
        }

        private void btnRelais5_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(5, !bytRelaisState[4]);
        }

        private void btnRelais6_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(6, !bytRelaisState[5]);
        }

        private void btnRelais7_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(7, !bytRelaisState[6]);
        }

        private void btnRelais8_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(8, !bytRelaisState[7]);
        }

        #endregion RelaisButton Click Events

        private void btnSettingsOk_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Speichern
            Properties.Settings.Default["UpdateSpeed"] = txtUpdateSpeed.Text;

            Properties.Settings.Default.Save();

            setUpdateTimer();

            ChangeStatus("Die Einstellungen wurden gespeichert und angewendet!", IconType.Information);
        }

        private void btnRelaisOff_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(0, false);
        }

        private void btnRelaisOn_Click(object sender, RoutedEventArgs e)
        {
            switchRelais(0, true);
        }

        private void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            readStatus();
        }

        private void sldFU1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int intVal = Convert.ToInt32(sldFU1.Value);
            double dblVolt = map(intVal, 0, 255, 0, 10000);

            lblValueFU1.Content = "Wert: " + intVal + " / " + dblVolt + "mV";
        }

        private void sldFU2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int intVal = Convert.ToInt32(sldFU2.Value);
            double dblVolt = map(intVal, 0, 255, 0, 10000) ;

            lblValueFU2.Content = "Wert: " + intVal + " / " + dblVolt + "mV";
        }
    }
}