using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;
using _2DAG_Designer.DrawingObjects.Objects;
using _2DAG_Designer.DrawingObjects;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace _2DAG_Designer.FileIO
{
    static class DrawFile
    {
        /// <summary>
        /// Informationen die in einer Linie gespeichert werden können
        /// </summary>
        public enum ObjectInformation
        {
            //Art des Objekte (Linie, Bogen, ...)
            objectType,

            //Endpunkt
            endX,
            endY,

            color,
            angle,

            width,
            height,

            keepHeight,
            inverted
        }

        /// <summary>
        /// Verschiedene Objekttypen
        /// </summary>
        public enum ObjectTypes
        {
            Ellipse,
            Line,
            Arc,
        }

        public static void SaveToFile(string filepath)
        {
            //Liste der Zeilen (pro DrawObject eine Zeile)
            var lines = new List<string>();

            //für jedes Objekt in der drawList wird eine Linie hinzugefügt     
            foreach (var drawObj in MainWindow.DrawList)
            {
                //Informationen des IDrawable wird abgespeichert
                //String stellt eine Linie in der Textdatei dar
                //für DrawGroups gibt es für jedes enthaltene Objekt eine Linie
                var line = drawObj.GetInformationString();

                //Linie wird der Liste hinzugefügt
                lines.AddRange(line);
            }

            //Liste wird in das Textdokument geschreiben
            File.WriteAllLines(filepath, lines);
        }

        public static List<DrawObject> ListFromFile(string filepath)
        {
            //Liste der Objekte
            var list = new List<DrawObject>();

            //alle Linien aus der Textdatei werden abgespeichert
            var allLines = File.ReadAllLines(filepath);
            
            //alle Linien werden durchgegangen
            foreach (var line in allLines)
            {
                try
                {
                    //neues Objekt wird erstellt und der Liste hinzugefügt
                    list.Add(DrawObject.GetObjectFromString(line));
                }
                catch
                {}
            }

            //Wenn ein Objekt existiert, 
            if (MainWindow.DrawList.Count > 0)
                //hat der Zeichenprozess bereits gestartet
                MainWindow.FirstDraw = false;

            //Liste mit allen hinzugefügten Objekten wird zurückgegeben
            return list;
        }

        /// <summary>
        /// offnet den Windows Explorer 
        /// </summary>
        /// <returns>gibt den ausgewählten Pfad + Dateiname zurück</returns>
        public static string BrowseExplorer()
        {
            //Explorer Dialog wird erstellt
            var dialog = new OpenFileDialog
            {
                //Filter
                DefaultExt = ".txt",
                Filter = "Text Document (.txt)|*.txt"
                
                
            };

            //wenn der Dialog einen Pfad zurückgegeben hat,
            if ((bool)dialog.ShowDialog())
                return dialog.FileName;
            else
                return "";
        }

        public static string SaveFileToDialog()
        {
            //Explorer Dialog wird erstellt
            var dialog = new SaveFileDialog
            {
                //Dateityp
                DefaultExt = ".txt",
                Filter = "Text Document (.txt)|*.txt",
            };

            //wenn der Dialog einen Pfad zurückgegeben hat,
            if ((bool)dialog.ShowDialog())
                return dialog.FileName;
            else
                return "";            
        }

        #region Helper

        /// <summary>
        /// gibt den zugehörigen string aus der Werteliste eines gewünschten Informationstyps zurück
        /// </summary>
        public static string GetObjectInformation(this Dictionary<ObjectInformation, string> informationDictionary, ObjectInformation informationType)
        {
            //findet Index des InformationsTyps
            //var index = informationTypeList.IndexOf(informationType);
            var index = informationDictionary.Keys.ToList().IndexOf(informationType);
            
            //Wenn die Information nicht gefunden wurde
            if (index < 0)
            {
                //Wird der Standertwert zurückgegeben
                return informationType.LoadDefaultValue();
            }

            //gibt den dazugehörigen Wert aud der Werteliste zurück
            //return informationValueList[index];
            return informationDictionary.Values.ElementAt(index);
        }

        /// <summary>
        /// konvertiert den angegebenen Informationstyp und den Wert in einen String
        /// </summary>
        public static string InformationToString(this ObjectInformation informationType, object value)
        {
            string line = informationType.ToString() + ":" + value + "/";

            return line;
        }

        /// <summary>
        /// gibt den Standartwert für den angegebenen Informationstyp zurück
        /// </summary>
        private static string LoadDefaultValue(this ObjectInformation informationType)
        {
            if(informationType == ObjectInformation.objectType)
            {
                //der Objekt Typ muss angegeben werden
                throw new Exception();
            }

            //Zahlenwerte sind standartmäßig 0
            else if((informationType == ObjectInformation.endX) ||
                (informationType == ObjectInformation.endY) ||
                (informationType == ObjectInformation.angle) ||
                (informationType == ObjectInformation.width) ||
                (informationType == ObjectInformation.height))
            {
                return "0";
            }

            //Farbe
            else if (informationType == ObjectInformation.color)
            {
                //schwarz
                return "#FF000000";
            }

            //bool
            else if ((informationType == ObjectInformation.keepHeight) ||
                (informationType == ObjectInformation.inverted))
            {
                return "false";
            }


            return "";
        }

        #endregion
    }
}