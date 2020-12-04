using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using static _2DAG_Designer.FileIO.DrawFile;

namespace _2DAG_Designer.DrawingObjects.Objects
{
    class DrawLine : DrawObject
    {   
        #region Constructor

        public DrawLine (Point startPoint, Point endPoint, SolidColorBrush color, bool roundEnd)
        {
            //Start und Endpunkt des neuen Objekts wird festgelegt
            this.ObjectStart = startPoint;
            this.ActualObjectEnd = endPoint;

            //Runden wenn gerundet werden soll
            if (roundEnd)
                Round();

            //berechnet Maße
            GetMeasures();

            //Winkel wird abgespeichert
            GetAngle();

            this.Color = color;

            //neue Linie wird erstellt
            ThisObject = new Line()
            {
                //Linie von X1, Y1
                X1 = ObjectStart.X,
                Y1 = ObjectStart.Y,

                //zu X2, Y2
                X2 = ActualObjectEnd.X,
                Y2 = ActualObjectEnd.Y,

                Stroke = color,         //Farbe der Linie wird festgelegt
                StrokeThickness = 2     // Dicke: 2
            };

            //neues Object wird der Liste hinzugefügt
            MainWindow.DrawList.Add(this);

            //Objekt wird zum Canvas hinzugefügt
            MainWindow.ThisWindow.AddToCanvas(this.ThisObject);            
        }

        #endregion

        #region Helper

        /// <summary>
        /// entfernt Objekt und zeichnet es erneut (nötig, wenn Werte verändert wurden)
        /// </summary>
        public override void Redraw()
        {
            var thisObject = (Line)ThisObject;
            //Linie von X1, Y1
            thisObject.X1 = ObjectStart.X;
            thisObject.Y1 = ObjectStart.Y;

            //zu X2, Y2
            thisObject.X2 = ActualObjectEnd.X;
            thisObject.Y2 = ActualObjectEnd.Y;

            //Farbe der Linie wird festgelegt
            thisObject.Stroke = Color;   
            
            // Dicke: 2
            thisObject.StrokeThickness = 2;   

            //Änderungen werden gespeichert
            ThisObject = thisObject;

            GetMeasures();

            GetAngle();
        }

        public override void Rotate(double angle)
        {

            //Winkel darf 360 nicht überschreiten
            if (angle >= 360)
                angle -= 360;

            Angle = angle;

            //Länge der Linie wird abgespeichert
            var length = Math.Sqrt(Math.Pow((ObjectStart.X - ActualObjectEnd.X), 2) + Math.Pow((ObjectStart.Y - ActualObjectEnd.Y), 2));

            //Tatsächliches ende = Start        + (     Verschiebung vom Start     )
            ActualObjectEnd.Y = ObjectStart.Y + (length) * Math.Sin(angle * (Math.PI / 180.0));
            ActualObjectEnd.X = ObjectStart.X + (length) * Math.Cos(angle * (Math.PI / 180.0)); //* (Math.PI / 180.0) rechnet grad in radianten um

            Redraw();
        }
 
        public override void Edit(EditObject edit, double value)
        {
            if(edit == EditObject.Size)
            {
                //Höhe und Breite wird mit dem Faktor gestreckt/gestaucht
                Width *= value;
                Height *= value;

                //Endpunkt wird berechnet
                CalculateEnd();

                Redraw();
            }
            else if(edit == EditObject.Angle)
            {
                this.Rotate(value);
            }
        }

        /// <summary>
        /// berechnet Breite und Höhe
        /// </summary>
        private void GetMeasures()
        {
            //Breite
            // Distanz zwischen End und Startpunkt auf der X Achse
            this.Width = this.ActualObjectEnd.X - this.ObjectStart.X;

            //Höhe
            // Distanz zwischen End und Startpunkt auf der Y Achse
            this.Height = this.ActualObjectEnd.Y - this.ObjectStart.Y;
        }

        #endregion
        
        public override string[] GetInformationString()
        {
            var line = new string[1];

            //Objekt Typ
            line[0] += ObjectInformation.objectType.InformationToString(ObjectTypes.Line); ;

            //Endpunkt
            line[0] += ObjectInformation.endX.InformationToString(MainWindow.PixelToCentimeter(GetEnd().X));
            line[0] += ObjectInformation.endY.InformationToString(MainWindow.PixelToCentimeter(GetEnd().Y));

            //Farbe
            line[0] += ObjectInformation.color.InformationToString(Color);

            //string mit den Informationen des Objekts wird zurückgegeben
            return line;
        }
    }
}
