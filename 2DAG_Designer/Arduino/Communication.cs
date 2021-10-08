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
        // Bewegungsrichtung für den Brenner
        public enum Direction
        {
            Left,
            Right, 
            Up,
            Down,
            Stop,
        }

        public static bool IsConnected = false;

        //aktiver Port
        private static SerialPort _serialPort = new SerialPort();

        //letzter vom Arduino gesendeter Befehl
        private static string _recievedCommand = "";

        // gibt an, ob der Brenner gerade bewegt wird, um eine Startposition auszuwählen
        public static bool MovingBurner = false;

        /// <summary>
        /// fügt verfügbare Ports zur Auswahl hinzu
        /// </summary>
        public static string[] GetPorts()
        {
            //UI sichtbar
            enableUI();

            //verfügbare Ports werden zurückgegeben
            return SerialPort.GetPortNames();
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

                //Verbindung wird geschlossen
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
            while (_recievedCommand != "OK") { }

            //Befehl wird gelöscht, da er verwendet wurde
            _recievedCommand = String.Empty;
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
        /// Brenner bewegen, um Startposition auszuwählen
        /// </summary>
        /// <param name="dir">Bewegungsrichtung</param>
        /// <param name="start">Bewegung starten/stoppen</param>
        public static void MoveBurner(Direction dir)
        {
            // Befehl an den Arduino
            string sendMessage = "!MV";
          
            switch(dir)
            {
                case Direction.Up:
                    {
                        sendMessage += "U";

                        // Brenner wird bewegt
                        MovingBurner = true;
                    }
                    break;
                case Direction.Down:
                    {
                        sendMessage += "D";

                        // Brenner wird bewegt
                        MovingBurner = true;
                    }
                    break;
                case Direction.Left:
                    {
                        sendMessage += "L";

                        // Brenner wird bewegt
                        MovingBurner = true;
                    }
                    break;
                case Direction.Right:
                    {
                        sendMessage += "R";

                        // Brenner wird bewegt
                        MovingBurner = true;
                    }
                    break;
                case Direction.Stop:
                    {
                        sendMessage += "S";

                        // Brenner wird nicht bewegt
                        MovingBurner = false;
                    }
                    break;
            }

            // Befehl wird gesendet
            Send(sendMessage);
        }

        /// <summary>
        /// Wird ausgeführt, wenn Daten vom seriellen Port empfangen werden
        /// </summary>
        private static void dataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {   
                //Eingegangene Nachricht wird gespeichert
                var data = _serialPort.ReadLine();

                //Eingegangene Nachricht wird behandelt
                handleIncomingMessage(data);
            }
            catch { }
        }


        #region private Helper

        /// <summary>
        /// macht die benögitge UI sichtbar
        /// </summary>
        private static void enableUI()
        {
            //Label
            MainWindow.ThisWindow.ComLabel.IsEnabled = true;

            //Combobox
            MainWindow.ThisWindow.AvailablePortsComboBox.IsEnabled = true;

            //Connect Button
            MainWindow.ThisWindow.ConnectComButton.IsEnabled = true;
            MainWindow.ThisWindow.ConnectComButton.Content = "verbinden";
        }

        /// <summary>
        /// macht die benögitge UI unsichtbar
        /// </summary>
        private static void disableUI()
        {
            //Label
            MainWindow.ThisWindow.ComLabel.IsEnabled = false;

            //Combobox
            MainWindow.ThisWindow.AvailablePortsComboBox.IsEnabled = false;

            //Connect Button
            MainWindow.ThisWindow.ConnectComButton.IsEnabled = false;
        }

        /// <summary>
        /// Behandelt eingehende Arduino Nachrichten
        /// </summary>
        private static void handleIncomingMessage(string data)
        {
            //Wenn die Nachricht mit ! beginnt
            if (data[0] == '!')
            {
                // letztes Zeichen wird entfernt, da es \r ist
                data = data.Remove(data.Length - 1, 1);

                //! wird entfernt
                _recievedCommand = data.Remove(0, 1);

            }
            else
            {      
                if(data.StartsWith("PROG"))
                {
                    Engrave.Engrave.ProgressMessageRecieved(int.Parse(data.Substring(4)));
                }


                _recievedCommand =  data;
            }
        }

        #endregion
    }
}
