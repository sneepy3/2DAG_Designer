using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Media;
using System.Data.SqlTypes;

namespace _2DAG_Designer.Arduino
{
    /*
    * ! am Anfang von Befehlen 
    * 
    * # am Anfang von Objektinformationen
    * 
    * Eine Nachricht darf nach den
    * Anfangszeichen nur 5 Zeichen enthalten
    * 
    * Am Ende einer Nachricht muss ein \n folgen
    */
    static class Communication
    {
        public static bool IsConnected = false;

        //aktiver Port
        private static SerialPort _serialPort = new SerialPort();

        //letzter vom Arduino gesendeter Befehl
        private static string _recievedCommand = "";

        /// <summary>
        /// fügt verfügbare Ports zur Auswahl hinzu
        /// </summary>
        public static void GetPorts()
        {
            //UI sichtbar
            enableUI();

            //verfügbare Ports werden abgerufen
            var availablePorts = SerialPort.GetPortNames();

            //Wenn kein Port existiert, wird der Vorgang beendet
            if (availablePorts[0] == null)
                return;

            //Jeder Port wird der Combobox hinzugefügt
            foreach (var port in availablePorts)
            {
                MainWindow.ThisWindow.AvailablePortsComboBox.Items.Add(port);
            }
        }


        public static void Connect()
        {
            try 
            {
                
                //ausgewählter Port
                string selectedPort = MainWindow.ThisWindow.AvailablePortsComboBox.Text;

                //Port wird eingerichtet
                _serialPort.PortName = selectedPort;
                _serialPort.BaudRate = 9600;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.ReceivedBytesThreshold = 1;
                _serialPort.DtrEnable = true;

                _serialPort.Open();

                // der dataRecieved Eventhandler wird hinzugefüht und aufgerufen wenn Daten empfangen werden
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(dataRecieved);

                MainWindow.ThisWindow.ConnectComButton.Content = "Trennen";

                //Wenn die Verbindung erfolgreich hergestellt wurde,
                IsConnected = true;

                //wenn alles funktioniert hat
                MainWindow.ThisWindow.ConnectComButton.BorderBrush = Brushes.LightGray;
            }
            catch
            {
                //bei einem Fehler
                MainWindow.ThisWindow.ConnectComButton.BorderBrush = Brushes.Red;
            }
        }

        public static void Disconnect()
        {
            //wenn eine Verbindung besteht,
            if(IsConnected)
            {
                IsConnected = false;

                //UI unsictbar
                disableUI();

                //disconnect an Arduino
                _serialPort.Write("!DCONN\n");
                _serialPort.Close();

                MainWindow.ThisWindow.ConnectComButton.Content = "Verbinden";
            }
        }


        /// <summary>
        /// sendet eine Nachricht
        /// </summary>
        public static void Send(string s)
        {
            //Nachricht wird gesendet, wenn eine Verbindung besteht
            if(IsConnected)
            {
                _serialPort.Write(s + "\n");

                //Wartet auf den Arduino
                WaitForArduinoResponse();
            }
        }

        public static void WaitForArduinoResponse()
        {
            //Wartet bis vom Arduino ein OK Befehl kommt
            while (_recievedCommand != "OK")
            {
                DisplayArduinoMessage();
            }


            //Befehl wird gelöscht, da er verwendet wurde
            _recievedCommand = String.Empty;
        }

        public static void DisplayArduinoMessage()
        {
            //Nachricht wird auf dem Fenster angezeigt
            MainWindow.ThisWindow.RecievedMessageLabel.Content = _recievedCommand;
        }


        public static void ConnectButtonClicked()
        {
            //Wenn eine Verbindung besteht, 
            if (IsConnected)
                //Verbindung wird getrennt
                Disconnect();
            else
                //Verbindung wird hergestellt
                Connect();
        }

        /// <summary>
        /// Wird ausgeführt, wenn Daten vom seriellen Port empfangen werden
        /// </summary>
        private static void dataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            var data = _serialPort.ReadLine();

            //Befehl wird abgespeichert
            _recievedCommand = getCommand(data);
        }


        #region private Helper

        /// <summary>
        /// macht die benögitge UI sichtbar
        /// </summary>
        private static void enableUI()
        {
            //Label
            MainWindow.ThisWindow.ComLabel.Visibility = System.Windows.Visibility.Visible;

            //Combobox
            MainWindow.ThisWindow.AvailablePortsComboBox.Visibility = System.Windows.Visibility.Visible;

            //Connect Button
            MainWindow.ThisWindow.ConnectComButton.Visibility = System.Windows.Visibility.Visible;
            MainWindow.ThisWindow.ConnectComButton.Content = "verbinden";
        }

        /// <summary>
        /// macht die benögitge UI unsichtbar
        /// </summary>
        private static void disableUI()
        {
            //Label
            MainWindow.ThisWindow.ComLabel.Visibility = System.Windows.Visibility.Hidden;
            
            //Combobox
            MainWindow.ThisWindow.AvailablePortsComboBox.Visibility = System.Windows.Visibility.Hidden;

            //Connect Button
            MainWindow.ThisWindow.ConnectComButton.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// gibt den Befehl einer Arduino Nachricht zurück
        /// </summary>
        private static string getCommand(string data)
        {
            //Wenn die Nachricht mit ! beginnt
            if (data[0] == '!')
            {
                // letztes Zeichen wird entfernt, da es \r ist
                data = data.Remove(data.Length - 1, 1);

                //! wird entfernt
                return data.Remove(0, 1);

            }
            else
            {      
                return data;
            }
        }

        #endregion
    }
}
