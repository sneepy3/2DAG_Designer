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
        /// wird aufgerufen, wenn der Arduino eine Information über den Fortschritt gibt
        /// </summary>
        /// <param name="progress">Fortschritt in %</param>
        public static void ProgressMessageRecieved(int progress)
        {
            //Fortschritt wird abgespeichert
            _progress = progress;

            //Fortschrittssleiste wird aktualisiert
            MainWindow.ThisWindow.BurnProgressBar.Value = progress;

            //ist der Prozess fertig,
            if(progress == 100)
                //Wird die ProgressBar unsichtbar
                MainWindow.ThisWindow.BurnProgressBar.Visibility = Visibility.Collapsed;
            else
                //sonst sichtbar
                MainWindow.ThisWindow.BurnProgressBar.Visibility = Visibility.Visible;
        }
    }
}
