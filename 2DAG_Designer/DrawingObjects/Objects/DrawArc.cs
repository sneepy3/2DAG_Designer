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
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static _2DAG_Designer.FileIO.DrawFile;

namespace _2DAG_Designer.DrawingObjects.Objects
{
    class DrawArc : DrawObject
    {
        #region Variablen
       
        private Point ObjectEnd;

        //true wenn der Bogen in die andere Richtung zeigen soll
        public bool ArcInverted = false;

        //true wenn die Höhe beibehalten werden soll
        public bool KeepHeight = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor mit Endpunkt
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="roundEnd">true wenn das Ende auf 0,5 cm gerundet werden soll</param>
        public DrawArc(Point startPoint, Point endPoint,
            SolidColorBrush color, bool roundEnd)
        {
            // druch Klicken erstellt, höhe soll nicht beibehalten werden
            KeepHeight = false;

            //s. http://www.blackwasp.co.uk/WPFArcSegment.aspx

            this.ObjectStart = startPoint;
            this.ActualObjectEnd = endPoint;


            //Runden wenn gerundet werden soll
            if(roundEnd)
                Round();

            //Abstand von Start und Endpunkt wird berechnet
            var distanceStartEnd = Math.Sqrt(Math.Pow((ObjectStart.X - ActualObjectEnd.X), 2) + Math.Pow((ObjectStart.Y - ActualObjectEnd.Y), 2));

            //Breite und Höhe werden berechnet
            this.Width = distanceStartEnd; 
            this.Height = Width / 2; // geteilt durch 2, da width nur ein Radius ist

            //Ende wird berechnet
            //auf gleicher Höhe wie der Start
            this.ObjectEnd.Y = ObjectStart.Y;


            //Wird der Abstand zwischen Start und Ende addiert
            this.ObjectEnd.X = ObjectStart.X + distanceStartEnd;


            //Farbe und Winkel festgelegt
            this.Color = color;
            
            //Winkel wird berechnet 
            GetAngle();
      

            //neuer Arc wird mit den Werten erstellt
            ThisObject = CreatePath();

            MainWindow.DrawList.Add(this);

            //Objekt wird zum Canvas hinzugefügt
            MainWindow.ThisWindow.AddToCanvas(this.ThisObject);
        }

        /// <summary>
        /// Constructor ohne Endpunkt
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        public DrawArc(Point startPoint, 
            double width, double height, double angle, SolidColorBrush color)
        {
            //durch das Menü erstellt, Höhe soll beibehalten werden
            KeepHeight = true;

            //s. http://www.blackwasp.co.uk/WPFArcSegment.aspx

            this.ObjectStart = startPoint;

            //das Objektende wird aus dem Startpunkt und der Breite berechnet
            this.ObjectEnd.X = ObjectStart.X + width;
            this.ObjectEnd.Y = ObjectStart.Y;


            //Farbe und Winkel festgelegt
            this.Color = color;
            this.Angle = angle;

            //Tatsächliches Ende wird berechnet
            CalculateActualEnd();


            this.Width = width;
            this.Height = height;


            //neuer Arc wird mit den Werten erstellt
            ThisObject = CreatePath();

            MainWindow.DrawList.Add(this);

            //Objekt wird zum Canvas hinzugefügt
            MainWindow.ThisWindow.AddToCanvas(this.ThisObject);
        }

        #endregion

        #region Helper

        public override void Redraw()
        {
            //vom Canvas entfernen
            MainWindow.ThisWindow.RemoveFromCanvas(ThisObject);

            #region Werte werden berechnet
            //die Breite ist der Abstend zwischen dem Start und dem Endpunkt
            Width = Math.Sqrt(Math.Pow(ActualObjectEnd.X - ObjectStart.X, 2) + Math.Pow(ActualObjectEnd.Y - ObjectStart.Y, 2));

            //Wenn die Höhe nicht beibehalten werden soll, ist sie gleich wei die Breite
            if ((!KeepHeight)) 
                Height = Width / 2; // geteilt durch 2, da width nur ein Radius ist

            //Winkel wird berechnet
            GetAngle();

            //Wenn das Ende links vom Start liegt, wird die Breite abgezogen 
            //kommt nur bei Objekten vor, die nicht vom Menü aus erstellt wurden, da das Menu das Ende immer rechts vom Anfang setzt
            if ((ObjectEnd.X < ObjectStart.X))
            {
                //Objektende ist Objektstart - Breite 
                ObjectEnd.X = ObjectStart.X - Width ;
                Angle -= 180;
            }
            else
            {
                //Objektende ist Objektstart + Breite 
                ObjectEnd.X = ObjectStart.X + Width;
            }

            //Die Höhe vom Ende ist gleich der Höhe vom Start
            ObjectEnd.Y = ObjectStart.Y;

            #endregion

            //neuer Path wird erstelle, mit den berechneten Werten
            ThisObject = CreatePath();

            //zum Canvas hinzufügen
            MainWindow.ThisWindow.AddToCanvas(ThisObject);
        }

        /// <summary>
        /// rotiert einen Bogen
        /// </summary>
        public override void Rotate(double angle)
        {
            //vom Canvas entfernen
            MainWindow.ThisWindow.RemoveFromCanvas(ThisObject);

            //neuer Winkel wird gespeichert
            Angle = angle;

            //Winkel darf 360 nicht überschreiten
            if (Angle >= 360)
            {
                Angle -= 360;
            }


            //neuer Path wird erstellt
            ThisObject = CreatePath();

            //ActualObjectEnd wird berechnet
            CalculateActualEnd();


            //zum Canvas hinzufügen
            MainWindow.ThisWindow.AddToCanvas(ThisObject);
        }

        public override void Edit(EditObject edit)
        {
            if (edit == EditObject.Invert)
            {
                //Arc wird invertiert
                ArcInverted = !ArcInverted;
            }

            base.Edit(edit);
        }

        public override void Edit(EditObject edit, double factor)
        {
            if (edit == EditObject.Size)
            {
                //Höhe und Breite wird mit dem Faktor gestreckt/gestaucht
                Width *= factor;
                Height *= factor;

                //Endpunkt wird berechnet
                CalculateEnd();

                Redraw();
            }
        }

        /// <summary>
        /// Berechnet den tatsächlichen Endpunkt des Bogens
        /// </summary>
        private void CalculateActualEnd() 
        {
            //Tatsächliches ende = Start        + (     Verschiebung vom Start     )
            ActualObjectEnd.X = ObjectStart.X + (ObjectEnd.X - ObjectStart.X) * Math.Cos(Angle * (Math.PI / 180.0)); //* (Math.PI / 180.0) rechnet grad in radianten um
            ActualObjectEnd.Y = ObjectStart.Y + (ObjectEnd.X - ObjectStart.X) * Math.Sin(Angle * (Math.PI / 180.0));
        }

        public override void CalculateEnd()
        {
            //Endpunkt wird berechnet
            ObjectEnd.X = ObjectStart.X + Width;
            ObjectEnd.Y = ObjectStart.Y + Height;

            //tatsächliches Ende wird berechnet
            CalculateActualEnd();
        }

  
        
        private Path CreatePath()
        {
            Point startpoint;
            Point endpoint;

            // wenn der Bogen invertiert ist, dann wird der Start und EndPunkt vertauscht
            if (ArcInverted)
            {
                startpoint = ObjectEnd;
                endpoint = ObjectStart;
            }
            else
            {
                startpoint = ObjectStart;
                endpoint = ObjectEnd;
            }

            var rt = new RotateTransform()
            {
                CenterX = ObjectStart.X,
                CenterY = ObjectStart.Y,
                Angle = Angle
            };

            //Bogen wird erstellt
            var arc = new ArcSegment()
            {
                //größe des Bogens
                Size = new Size(Width / 2, Height), // geteilt durch 2, da width nur ein Radius ist

                //Endpunkt des Bogens
                Point = endpoint,
            };

            var pf = new PathFigure()
            {
                //Startpunkt 
                StartPoint = startpoint,
                IsClosed = false,
            };
            //arc wird zu den Pfadfiguren hinzugefügt
            pf.Segments.Add(arc);

            var pg = new PathGeometry()
            {
                Transform = rt
            };


            //pf wird pg hinzugefügt
            pg.Figures.Add(pf);


            //Ein Pfad wird erstellt
            var newPath = new Path()
            {
                Stroke = Color,
                StrokeThickness = 2,

                //Inhalt des Pfades ist pg
                Data = pg,


            };

            return newPath;
        }

        #endregion

        public override string[] GetInformationString()
        {
            var line = new string[1];

            //Objekt Typ
            line[0] += ObjectInformation.objectType.InformationToString(ObjectTypes.Arc);

            //Endpunkt
            line[0] += ObjectInformation.endX.InformationToString(MainWindow.PixelToCentimeter(GetEnd().X));
            line[0] += ObjectInformation.endY.InformationToString(MainWindow.PixelToCentimeter(GetEnd().Y));

            //Farbe
            line[0] += ObjectInformation.color.InformationToString(Color);

            //Winkel                                                     
            line[0] += ObjectInformation.angle.InformationToString(Angle);

            //Breite und Höhe                                            
            line[0] += ObjectInformation.width.InformationToString(Width);
            line[0] += ObjectInformation.height.InformationToString(Height);

            //sonstige Informationen                                     
            line[0] += ObjectInformation.keepHeight.InformationToString(KeepHeight);
            line[0] += ObjectInformation.inverted.InformationToString(ArcInverted);

            //string mit den Informationen des Objekts wird zurückgegeben
            return line;
        }
    }
}
