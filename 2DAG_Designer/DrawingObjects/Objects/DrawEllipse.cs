using System;
using System.Collections.Generic;
using System.Linq;
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
    class DrawEllipse : DrawObject
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public DrawEllipse (Point position, double width, double height)
        {
            //Werte werden festgelegt
            this.ActualObjectEnd = position;

            this.ObjectStart = ActualObjectEnd;

            this.Width = width;
            this.Height = height;

            //neue Ellipse wird erstellt
            ThisObject = new Ellipse()
            {
                //Höhe, Breite
                Width = width,
                Height = width,

                //Farbe
                Fill = Brushes.Black,

                //nach oben und nach links gebunden
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,

                //Position, Hälfte der Höhe/Breite wird abgezogen, damit die Ellipse in der Mitte ist
                Margin = new Thickness(ActualObjectEnd.X - (width / 2), ActualObjectEnd.Y - (height / 2), 0, 0)
            };

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
            //vom Canvas entfernen
            MainWindow.ThisWindow.RemoveFromCanvas(ThisObject);

            //neue Ellipse wird erstellt
            ThisObject = new Ellipse()
            {
                //Höhe, Breite
                Width = this.Width,
                Height = this.Height,
                //Farbe
                Fill = Brushes.Black,

                //nach oben und nach links gebunden
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,

                //Position, Hälfte der Höhe/Breite wird abgezogen, damit die Ellipse in der Mitte ist
                Margin = new Thickness(ActualObjectEnd.X - (Width / 2), ActualObjectEnd.Y - (Height / 2), 0, 0)
            };

            //zum Canvas hinzufügen
            MainWindow.ThisWindow.AddToCanvas(ThisObject);
        }
        
        public override void Rotate(double angle)
        {
            //keine Aktion nötig
        }

        public override void Edit(EditObject edit, double factor)
        {
            CalculateEnd();

            Redraw();
        }

        public override void CalculateEnd()
        {
            ActualObjectEnd = ObjectStart;
        }

        #endregion

        public override string[] GetInformationString()
        {
            var line = new string[1];

            //Objekt Typ
            line[0] += ObjectInformation.objectType.InformationToString(ObjectTypes.Ellipse);

            //Endpunkt
            line[0] += ObjectInformation.endX.InformationToString(MainWindow.PixelToCentimeter(GetEnd().X));
            line[0] += ObjectInformation.endY.InformationToString(MainWindow.PixelToCentimeter(GetEnd().Y));

            //Breite, Höhe
            line[0] += ObjectInformation.width.InformationToString(Width);
            line[0] += ObjectInformation.height.InformationToString(Height);

            //string mit den Informationen des Objekts wird zurückgegeben
            return line;
        }

    }
}
