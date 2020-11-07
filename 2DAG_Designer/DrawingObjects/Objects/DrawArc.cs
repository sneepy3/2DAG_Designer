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

        //Radius des Kreise
        public double Radius;

        //true wenn der Bogen in die andere Richtung zeigen soll
        public bool ArcInverted = false;


        private Point ObjectEnd;

        //Mittelpunkt des Kreises
        private Point _centerPoint;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public DrawArc(Point startPoint, double radius, double circleSizeAngle,  double startAngle,
            SolidColorBrush color)
        {
            //Startpunkt
            this.ObjectStart = startPoint;

            //Winkel für die Berechnung des Mittelpunkts
            //Mittelpunkt ist vom Startpunkt aus um Startwinkel + 90 Grad verschoben
            var centerPointAngle = startAngle;


            //Mittelpunkt wird mit dem startpunkt, dem Radius und dem Winkel berechnet
            this._centerPoint = CalculatePoint(startPoint, radius, centerPointAngle);

            //Berechnung des Endpunktes
            this.ActualObjectEnd = CalculatePoint(_centerPoint, radius, -180.0 + ((90 - startAngle) + startAngle));

            //Radius des Kreises
            this.Radius = radius;

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

        ///// <summary>
        ///// Constructor ohne Endpunkt
        ///// </summary>
        ///// <param name="startPoint"></param>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <param name="angle"></param>
        ///// <param name="color"></param>
        //public DrawArc(Point startPoint, 
        //    double width, double height, double angle, SolidColorBrush color)
        //{
        //    //durch das Menü erstellt, Höhe soll beibehalten werden
        //    KeepHeight = true;

        //    //s. http://www.blackwasp.co.uk/WPFArcSegment.aspx

        //    this.ObjectStart = startPoint;

        //    //das Objektende wird aus dem Startpunkt und der Breite berechnet
        //    this.ObjectEnd.X = ObjectStart.X + width;
        //    this.ObjectEnd.Y = ObjectStart.Y;


        //    //Farbe und Winkel festgelegt
        //    this.Color = color;
        //    this.Angle = angle;

        //    //Tatsächliches Ende wird berechnet
        //    CalculateActualEnd();


        //    this.Width = width;
        //    this.Height = height;


        //    //neuer Arc wird mit den Werten erstellt
        //    ThisObject = CreatePath();

        //    MainWindow.DrawList.Add(this);

        //    //Objekt wird zum Canvas hinzugefügt
        //    MainWindow.ThisWindow.AddToCanvas(this.ThisObject);
        //}

        #endregion

        #region Helper

        public override void Redraw()
        {
            //vom Canvas entfernen
            MainWindow.ThisWindow.RemoveFromCanvas(ThisObject);

            #region Werte werden berechnet
            //die Breite ist der Abstend zwischen dem Start und dem Endpunkt
            Width = Math.Sqrt(Math.Pow(ActualObjectEnd.X - ObjectStart.X, 2) + Math.Pow(ActualObjectEnd.Y - ObjectStart.Y, 2));


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

        private Point CalculatePoint(Point startPoint, double disctance, double angle)
        {
            return new Point()
            {
                X = startPoint.X + (Math.Sin(angle * (Math.PI / 180.0)) * disctance),
                Y = startPoint.Y + (Math.Cos(angle * (Math.PI / 180.0)) * disctance)
            };
        }
        
        private Path CreatePath()
        {
            //Pfad wird erstellt
            var pf = new PathFigure();
            var pg = new PathGeometry();
            var path = new Path()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 3,
                Data = pg
            };
            pg.Figures.Add(pf);

            //Startpunkt
            pf.StartPoint = this.ObjectStart;

            //Der PathFigure wird ein ArcSegment hinzugefügt
            //es beihaltet den gewünschten Kreisanteil
            pf.Segments.Add(new ArcSegment()
            {
                Size = new Size(this.Radius, this.Radius),
                Point = this.ActualObjectEnd,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            });

            //Pfad wird zurückgegeben
            return path;
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

            

            //string mit den Informationen des Objekts wird zurückgegeben
            return line;
        }
    }
}
