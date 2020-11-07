using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using _2DAG_Designer;
using _2DAG_Designer.DrawingObjects.Objects;
using static _2DAG_Designer.FileIO.DrawFile;



namespace _2DAG_Designer.DrawingObjects
{
    public abstract class DrawObject : IDrawable
    {
        #region Variablen

        /// <summary>
        /// Möglichkeiten ein gezeichnetes Objekt zu verändern
        /// </summary>
        public enum EditObject
            {
                EndUp,
                EndDown,
                EndRight,
                EndLeft,

                /// <summary>
                /// Verändert die Größe des gesammten Objekts
                /// </summary>
                Size,
                Angle,

                Invert
            }

        //Farbe
        public SolidColorBrush Color { get; set;}

        //tatsächliches Objekt, das angezeigt werden soll
        public UIElement ThisObject;

        //Startpunkt
        public Point ObjectStart; 

        //tatsächlicher Endpunkt
        public Point ActualObjectEnd;

        //Breite und Höhe des Objekts in Pixeln
        public double Width { get; protected set; } = 0;
        public double Height { get; protected set; } = 0;

        //Winkel
        public double Angle { get; protected set; } = 0;

        #endregion

        #region Helper

        /// <summary>
        /// rundet in 0,5 cm Schritten
        /// </summary>
        public void Round()
        {
            //teilt die Grid in 30 Teile ein und rundet auf eine Ganzzahl
            int xRound = (int)Math.Round((ActualObjectEnd.X / (MainWindow.DrawRowSize / 30)));
            int yRound = (int)Math.Round((ActualObjectEnd.Y / (MainWindow.DrawRowSize / 30)));

            // 
            ActualObjectEnd.X = xRound * (MainWindow.DrawRowSize / 30);
            ActualObjectEnd.Y = yRound * (MainWindow.DrawRowSize / 30);
        }

        /// <summary>
        /// zeichnet das Objekt erneut
        /// </summary>
        public abstract void Redraw();

        /// <summary>
        /// Rotieren des Objekts
        /// </summary>
        public abstract void Rotate(double angle);

        /// <summary>
        /// Bearbeiten des Objekts
        /// </summary>
        /// <param name="edit">Art der Veränderung</param>
        public virtual void Edit(EditObject edit)
        {
            switch (edit)
            {
                case EditObject.EndDown:
                    {
                        ActualObjectEnd.Y += (MainWindow.DrawRowSize / 150);
                        
                        //Wenn das Ende das Zeichenfeld überschreitet, wird die Aktion rückgängig gemacht
                        if (!MainWindow.InDrawField(ActualObjectEnd))
                        {
                            ActualObjectEnd.Y -= (MainWindow.DrawRowSize / 150);
                        }
                    }
                    break;
                case EditObject.EndUp:
                    {
                        ActualObjectEnd.Y -= (MainWindow.DrawRowSize / 150);

                        //Wenn das Ende das Zeichenfeld überschreitet, wird die Aktion rückgängig gemacht
                        if (!MainWindow.InDrawField(ActualObjectEnd))
                        {
                            ActualObjectEnd.Y += (MainWindow.DrawRowSize / 150);
                        }
                    }
                    break;
                case EditObject.EndLeft:
                    {
                        ActualObjectEnd.X -= (MainWindow.DrawRowSize / 150);

                        //Wenn das Ende das Zeichenfeld überschreitet, wird die Aktion rückgängig gemacht
                        if (!MainWindow.InDrawField(ActualObjectEnd))
                        {
                            ActualObjectEnd.X += (MainWindow.DrawRowSize / 150);
                        }
                    }
                    break;
                case EditObject.EndRight:
                    {
                        ActualObjectEnd.X += (MainWindow.DrawRowSize / 150);

                        //Wenn das Ende das Zeichenfeld überschreitet, wird die Aktion rückgängig gemacht
                        if (!MainWindow.InDrawField(ActualObjectEnd))
                        {
                            ActualObjectEnd.X -= (MainWindow.DrawRowSize / 150);
                        }
                    }
                    break;
            }


            Redraw();
        }

        /// <summary>
        /// Bearbeiten mit Faktor
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="factor"></param>
        public abstract void Edit(EditObject edit, double value);

        /// <summary>
        /// Berechnet den Winkel des Objekts zur Horizontalen
        /// </summary>
        public void GetAngle()
        {
            //Wenn das Ende rechts vom Start ist, wird endRight true
            bool endRight = ActualObjectEnd.X >= ObjectStart.X;

            //            Gegenkathete                              Ankathete
            var sine = (ActualObjectEnd.Y - ObjectStart.Y) / (ActualObjectEnd.X - ObjectStart.X);

            //Winkel in Grad wird festgelegt
            Angle = Math.Atan(sine) * (180 / Math.PI);


            if (!endRight)
            {
                Angle -= 180;
            }

            //ist der Winkel negativ, wird 360 addiert (visuell den gleichen Effekt z.B. -5° entspricht 355°)
            if (Angle < 0)
                Angle += 360;
        }

        /// <summary>
        /// berechnet den Endpunkt anhand von Breite und Höhe
        /// </summary>
        public virtual void CalculateEnd()
        {
            ActualObjectEnd.X = ObjectStart.X + Width;
            ActualObjectEnd.Y = ObjectStart.Y + Height;
        }

        public void Remove()
        {
            //Objekt wird aus dem drawField entfernt
            MainWindow.ThisWindow.RemoveFromCanvas(this.ThisObject);
        }

        public void AddToCanvas()
        {
            MainWindow.ThisWindow.AddToCanvas(ThisObject);
        }

        #endregion

        #region Getter/Setter

        public Point GetEnd()
        {
            return this.ActualObjectEnd;
        }

        public Point GetStart()
        {
            return this.ObjectStart;
        }

        public void SetStart(Point start)
        {
            ObjectStart = start;
        }

        public void SetEnd(Point end)
        {
            ActualObjectEnd = end;
        }

        public abstract string[] GetInformationString();

        public static DrawObject GetObjectFromString(string s)
        {

            var informations = new Dictionary<ObjectInformation, string>();



            //einzelne Daten werden in einem StringArray abgespeichert
            string[] data = s.Split('/');

            //einzelne Informationen werden abgespeichert
            foreach (var informatin in data)
            {
                //teilt information in Art der Information und den Wert auf
                string[] splitInformation = informatin.Split(':');

                try
                {
                    informations.Add((ObjectInformation)Enum.Parse(typeof(ObjectInformation), splitInformation[0]), splitInformation[1]);
                }
                catch
                { }
            }

            //ObjektTyp wird abgespeichert
            ObjectTypes objectType = (ObjectTypes)Enum.Parse(typeof(ObjectTypes), informations.GetObjectInformation(ObjectInformation.objectType));

            //-----Informationen werden abgespeichert

            //Endpunkt
            var endPoint = new Point(
                            MainWindow.CentimeterTopixel(Double.Parse(informations.GetObjectInformation(ObjectInformation.endX))),
                            MainWindow.CentimeterTopixel(Double.Parse(informations.GetObjectInformation(ObjectInformation.endY))));

            //Farbe
            var color = (SolidColorBrush)new BrushConverter().
                           ConvertFromString(informations.GetObjectInformation(ObjectInformation.color));

            //Breite
            var width = double.Parse(informations.GetObjectInformation(ObjectInformation.width));

            //Höhe
            var height = double.Parse(informations.GetObjectInformation(ObjectInformation.height));

            //Winkel
            var angle = double.Parse(informations.GetObjectInformation(ObjectInformation.angle));

            DrawObject newObject = null;

            switch (objectType)
            {
                case ObjectTypes.Line:
                    {
                        //ende des letzen Objekts ist der Start der Linie
                        newObject = new DrawLine(MainWindow.DrawList.Last().GetEnd(), endPoint, color, false);

                    }
                    break;
                case ObjectTypes.Ellipse:
                    {
                        newObject = new DrawEllipse(endPoint, width, height, color, false);
                    }
                    break;
                case ObjectTypes.Arc:
                    {
                        //var keepHeight = bool.Parse(informations.GetObjectInformation(ObjectInformation.keepHeight));

                        //DrawArc newArc;

                        ////Wenn höhe beibehalten werden soll,
                        //if (keepHeight)
                        //{
                        //    //wird der Bogen ohne Endpunkt erstellt
                        //    newArc = new DrawArc(MainWindow.DrawList.Last().GetEnd(), width, height, angle, color);
                        //}
                        //else
                        //{
                        //    //sonst wird er mit Endpunkt erstellt
                        //    newArc = new DrawArc(MainWindow.DrawList.Last().GetEnd(), endPoint, color, false);
                        //}

                        //newArc.ArcInverted = bool.Parse(informations.GetObjectInformation(ObjectInformation.inverted));

                        ////ist der Bogen invertiert, wird er erneut gezeichnet
                        //if (newArc.ArcInverted)
                        //    newArc.Redraw();

                        //newObject = newArc;

                    }
                    break;
                default:
                    {
                        newObject = null;
                    }
                    break;
            }

            return newObject;
        }

        #endregion
    }
}
