using _2DAG_Designer.DrawingObjects;
using _2DAG_Designer.DrawingObjects.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using static _2DAG_Designer.Arduino.Communication;

namespace _2DAG_Designer.Arduino.Engrave
{

    public static class Engrave
    {


        
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
            var xDistance = Math.Round(MainWindow.PixelToCentimeter(drawable.Width));

            //Distanz Start-Ende in Y Richtung
            var yDistance = Math.Round(MainWindow.PixelToCentimeter(drawable.Height));

            //Information wird zurückgegeben
            return $"#OBJ{xDistance}/{yDistance}";
        }
    }
}
