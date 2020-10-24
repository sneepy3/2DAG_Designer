﻿using _2DAG_Designer.DrawingObjects;
using _2DAG_Designer.DrawingObjects.Objects;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static _2DAG_Designer.Arduino.Communication;

namespace _2DAG_Designer.Arduino.Engrave
{

    public static class Engrave
    {
        private static int _progress = 0;

        public static int Progress
        {
            get { return _progress; }
        }

        //wird true, wenn sich der Fortschritt ändert
        private static bool progressChanged = true;

        /// <summary>
        /// Graviervorgang wird gestertet
        /// </summary>
        /// <param name="drawables">Elemente, die graviert werden sollen</param>
        public static void Start(IDrawable[] drawables)
        {
            //Vorgang wird beendet, wenn keine Verbindung zum Arduino besteht
            if (!Communication.IsConnected)
                return;


            //Der Eingraviervorgang beginnt
            Send("!ENGRAVE");

            //Anzahl der Objekte wird gesendet
            Send($"#COUNT{drawables.Length - 1}");

            foreach (var drawable in drawables)
            {
                if(drawable.GetType() == typeof(DrawLine))
                {
                    //Information der Linie wird gesendet
                    Send(drawable.GetLineInformation());
                }
                else
                {
                    //Andere Objektarten noch nich implementiert
                }
            }

            //Behandlung für die Änderung der Fortschrittsanzeige
            HandleProgressChanges();
        }

        /// <summary>
        /// wird aufgerufen, wenn der Arduino eine Information über den Fortschritt gibt
        /// </summary>
        /// <param name="progress">Fortschritt in %</param>
        public static void ProgressMessageRecieved(int progress)
        {
            //Fortschritt wird abgespeichert
            _progress = progress;

            //Fortschritt wurde geändert
            progressChanged = true;
        }

        /// <summary>
        /// Informationen zum Senden für eine Linie
        /// </summary>
        private static string GetLineInformation(this IDrawable drawable)
        {
            //Distanz Start-Ende in X Richtung
            var xDistance = Math.Round(MainWindow.PixelToCentimeter(drawable.Width), 5);

            //Distanz Start-Ende in Y Richtung
            var yDistance = Math.Round(MainWindow.PixelToCentimeter(drawable.Height), 5);


            var returnString = $"#OBJ{xDistance}/{yDistance}";

            returnString = returnString.Replace(",", ".");

            //Information wird zurückgegeben
            return returnString;
        }

        /// <summary>
        /// überprüft ständig, ob sich der Fortschritt ändert
        /// Änderung in der Fortschrittsanzeige müssen hier vorgenommen werde, da
        /// der ProgressMessageRecieved Handler keinen Zugriff auf den UI-Thread hat
        /// </summary>
        private static void HandleProgressChanges()
        {
            var timer = new DispatcherTimer();
            
            //Timer Interval
            timer.Interval = new TimeSpan(0, 0, 0, 1);

            //Timer Tick event
            timer.Tick += new EventHandler(delegate (object sender, EventArgs e)
            {
                //Wenn sich der Fortschritt verändert hat
                if (progressChanged)
                {
                    progressChanged = false;

                    //Fortschrittssleiste wird aktualisiert
                    MainWindow.ThisWindow.BurnProgressBar.Value = Progress;

                    //ist der Prozess fertig,
                    if (Progress == 100)
                        //Wird die ProgressBar unsichtbar
                        MainWindow.ThisWindow.BurnProgressBar.Visibility = Visibility.Collapsed;
                    else
                        //sonst sichtbar
                        MainWindow.ThisWindow.BurnProgressBar.Visibility = Visibility.Visible;
                }
            });

            //Timer wird gestartet
            timer.Start();
        }
        
        private static void timerTick(object sender, EventArgs e)
        {
          
        }
    }
}
