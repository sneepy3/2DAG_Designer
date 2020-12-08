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
    class DrawCircle : DrawObject
    {
        #region Variablen

        //Radius des Kreise
        public double Radius;

        //Mittelpunkt des Kreises
        public Point CenterPoint;

        //Größe des Kreises (90 => 1/4 Kreis)
        public double CircleSizeAngle;

        //Anfangswinkel des Kreises
        public double StartAngle;

        //gibt an, ob der Kreis invertiert ist (nach oben geht)
        public bool IsInverted;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public DrawCircle(Point startPoint, double radius, double circleSizeAngle,  double startAngle, bool inverted,
            SolidColorBrush color)
        {
            #region Werte abspeichern

            //Startpunkt
            this.ObjectStart = startPoint;

            //Anfangswinkel
            this.StartAngle = startAngle;

            //Radius des Kreises
            this.Radius = radius;
         

            //Farbe und Winkel festgelegt
            this.Color = color;

            //Größe des Kreises
            this.CircleSizeAngle = circleSizeAngle;

            //Invertierung
            this.IsInverted = inverted;

            //Die größe des Kreises, darf nicht größer als 360 sein
            if(circleSizeAngle > 360)
            {
                CircleSizeAngle = 360;
            }

            #endregion

            //Winkel wird berechnet 
            GetAngle();

            //Endpunkt und Mittelpunkt wird berechnet
            CalculateEnd();

            //Breite und Höhe werden berechnet
            GetMeasures();
                        

            //neuer Pfad wird mit den Werten erstellt
            CreatePath();


            MainWindow.DrawList.Add(this);

            //Objekt wird zum Canvas hinzugefügt
            MainWindow.ThisWindow.AddToCanvas(this.ThisObject);
        }

        #endregion

        #region Helper

        public override void GetAngle()
        {
            //Winkel für die Berechnung des Mittelpunkts
            //Mittelpunkt ist vom Startpunkt aus um Startwinkel + 90 Grad verschoben
            var centerPointAngle = StartAngle + 90;

            // Wenn der Kreis gegen den Uhrzeigersinn geht,
            if (IsInverted)
                centerPointAngle = StartAngle - 90;



            //Winkel vom Mittel zum Endpunkt wird berechnet
            var endPointAngle = -180.0 + (centerPointAngle + CircleSizeAngle);

            if (IsInverted)
                endPointAngle += 180;

            //Berechnung des Winkels am Ende des Kreises
            this.Angle = 90 + endPointAngle;

            if (IsInverted)
                this.Angle = endPointAngle - 90;

            //Wenn der Winkel größer als 360 ist, wird 360 abgezogen, kein visueller Effekt
            if (Angle >= 360)
                Angle -= 360;
            // Wenn er kleiner als 0 ist, wird 360 addiert
            else if (Angle <= 0)
                Angle += 360;
        }

        public override void Redraw()
        {
            #region Werte werden berechnet

            #region Berechnung Radius und Mittelpunkt

            //Distanz Startpunkt Endpunkt
            var distanceStartEnd = MainWindow.DistanceBetween(ObjectStart, ActualObjectEnd);

            // ^auf X Achse
            var distanceStartEndX = ActualObjectEnd.X - ObjectStart.X;
            // ^auf Y Achse
            var distanceStartEndY = ActualObjectEnd.Y - ObjectStart.Y;

            //Startpunkt, Endpunkt und Mittelpunkt bilden ein gleichschenkliches Dreieck
            //Winkel am Mittelpunkt ist CircleSizeAngle
            //Winkel an den beiden äußeren Ecken (Startpunkt, Eckpunkt)
            var cornerAngle = (180 - CircleSizeAngle) / 2;

            //Winkel für die Berechnung des Mittelpunkts
            var centerPointAngle = (Math.Atan(distanceStartEndY / distanceStartEndX) * (180.0/ Math.PI)) 
                + cornerAngle;

            // wenn das Ende links liegt, ist die obige Berechnung des centerPointAngle fehlerhaft
            if(distanceStartEndX < 0)
            {
                //richtige Berechnung
                centerPointAngle = 180 - (Math.Atan(distanceStartEndY / (-distanceStartEndX)) * (180.0 / Math.PI))
                    + cornerAngle;
            }

            //Radius wird berechnet
            this.Radius = (distanceStartEnd / 2) / Math.Sin((CircleSizeAngle / 2) * (Math.PI / 180.0));
            Width = Radius;

            //Mittelpunkt wird mit dem Startpunkt, dem Radius und dem Winkel berechnet
            this.CenterPoint = CalculatePoint(ObjectStart, Radius, centerPointAngle);

            //der Anfangswinkel des Kreises wird aktualisiert
            StartAngle = centerPointAngle - 90;

            #endregion


            // Winkel wird berechnet
            GetAngle();

            // Maße werden berechnet
            GetMeasures();

            #endregion

            //neuer Path wird mit den berechneten Werten erstellt
            CreatePath();
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
            CreatePath();

            // Ende wird berechnet
            CalculateEnd();


            //zum Canvas hinzufügen
            MainWindow.ThisWindow.AddToCanvas(ThisObject);
        }

        public override void Edit(EditObject edit)
        {
            //Wenn kein ganzer Kreis angezeigt wird, können Änderungen vorgenommen werden
            if(CircleSizeAngle != 360)
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


        public override void CalculateEnd()
        {
            //Winkel für die Berechnung des Mittelpunkts
            //Mittelpunkt ist vom Startpunkt aus um Startwinkel + 90 Grad verschoben
            var centerPointAngle = StartAngle + 90;

            // Wenn der Kreis gegen den Uhrzeigersinn geht,
            if (IsInverted)
                centerPointAngle = StartAngle - 90;

            //Mittelpunkt wird mit dem Startpunkt, dem Radius und dem Winkel berechnet
            this.CenterPoint = CalculatePoint(ObjectStart, Radius, centerPointAngle);

            //Markierung Mittelpunkt
            //MainWindow.ThisWindow.AddToCanvas(new Ellipse() { Margin = new Thickness(_centerPoint.X - 2.5, _centerPoint.Y - 2.5, 0, 0), Width = 5, Height = 5, Fill = Brushes.Red });
            
            //Die größe des Kreises, darf nicht 360 betragen, sonst wird nichts angezeigt
            if (CircleSizeAngle == 360)
                CircleSizeAngle = 359.999;

            //Winkel vom Mittel zum Endpunkt wird berechnet
            var endPointAngle = -180.0 + (centerPointAngle + CircleSizeAngle);

            if (IsInverted)
                endPointAngle += 180;


            //Berechnung des Endpunktes
            this.ActualObjectEnd = CalculatePoint(CenterPoint, Radius, endPointAngle);

            MainWindow.ThisWindow.AddToCanvas(new Ellipse() { Margin = new Thickness(ActualObjectEnd.X - 2.5, ActualObjectEnd.Y - 2.5, 0, 0), Width = 5, Height = 5, Fill = Brushes.Red });
        }

        /// <summary>
        /// Berechnet Punkt anhand eines anderen Punkts,
        /// dem Abstand und dem Winkel
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="disctance"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Point CalculatePoint(Point startPoint, double disctance, double angle)
        {
            return new Point()
            {
                X = startPoint.X + (Math.Cos(angle * (Math.PI / 180.0)) * disctance),
                Y = startPoint.Y + (Math.Sin(angle * (Math.PI / 180.0)) * disctance)
            };
        }
        
        /// <summary>
        /// Berechnung von Breite und Höhe
        /// </summary>
        private void GetMeasures()
        {

        }

        private void CreatePath()
        {
            //Pfad wird erstellt
            var pf = new PathFigure();
            var pg = new PathGeometry();
            var path = new Path()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2.5,
                Data = pg
            };
            pg.Figures.Add(pf);

            //Startpunkt
            pf.StartPoint = this.ObjectStart;

            // gibt an, ob der Bogen dir größere oder kleinere Seite nehmen soll,
            // bei einem Winkel von mehr als 180 Grad, soll die größere Seite genommen werden
            bool isLargeArc = CircleSizeAngle > 180;

            // Richtung der Kreiszeichnung
            var sweepDirection = SweepDirection.Clockwise;

            if (IsInverted)
                sweepDirection = SweepDirection.Counterclockwise;

            //Der PathFigure wird ein ArcSegment hinzugefügt
            //es beihaltet den gewünschten Kreisanteil
            pf.Segments.Add(new ArcSegment()
            {
                Size = new Size(this.Radius, this.Radius),
                Point = this.ActualObjectEnd,
                SweepDirection = sweepDirection,
                IsLargeArc = isLargeArc
            });

            if(ThisObject != null)
            {
                var thisObject = (Path)ThisObject;

                thisObject.Data = pg;
            }
            else
            { 
                //Pfad wird zurückgegeben
                ThisObject = path;
            }
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

        /// <summary>
        /// Länge des Kreises berechnen
        /// </summary>
        /// <returns></returns>
        public double GetLength()
        {
            // Die Länge wird aus dem Winkel und dem Radius berechnet
            return (this.Radius * Math.PI * this.CircleSizeAngle) / 180;
        }
    }
}
