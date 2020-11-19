using _2DAG_Designer.DrawingObjects;
using _2DAG_Designer.DrawingObjects.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _2DAG_Designer.BurnSimulation
{
    static class StepmotorMovement
    {
        private static readonly System.Windows.Threading.DispatcherTimer Timer = new System.Windows.Threading.DispatcherTimer();

        //Theoretische Position
        static readonly Ellipse Position = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Red
        };

        //Positon auf der X Achse
        static readonly Ellipse MotorX = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Blue
        };

        //Positon auf der Y Achse
        static readonly Ellipse MotorY = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Green
        };

        //Positon die sich aus der X und Y Achse ergibt
        static readonly Ellipse ActualPos = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Gray
        };

        private static IDrawable[] lines;

        private const double StepDistance = 30;

        private static int currentObj = 0;
        private static Line currentLine;

        private static double XMovement;
        private static double YMovement;


        private static bool calledOnce = false;



        private static bool drawingCircle = false;
        private static double anglePerVertex = 0;
        private static double currentAngle;


        public static void Start(IDrawable[] drawables)
        {
            //Kreise werden hinzugefügt
            MainWindow.ThisWindow.DrawField.Children.Add(Position);
            MainWindow.ThisWindow.DrawField.Children.Add(MotorX);
            MainWindow.ThisWindow.DrawField.Children.Add(MotorY);
            MainWindow.ThisWindow.DrawField.Children.Add(ActualPos);

            Position.Margin = new Thickness(drawables[0].GetStart().X - 5, drawables[0].GetStart().Y - 5, 0, 0);
            MotorX.Margin = new Thickness(drawables[0].GetStart().X - 5, drawables[0].GetStart().Y - 5, 0, 0);
            MotorY.Margin = new Thickness(drawables[0].GetStart().X - 5, drawables[0].GetStart().Y - 5, 0, 0);
            ActualPos.Margin = new Thickness(drawables[0].GetStart().X - 5, drawables[0].GetStart().Y - 5, 0, 0);

            lines = drawables;

            currentLine = new Line()
            {
                X1 = lines.First().GetStart().X,
                Y1 = lines.First().GetStart().Y,

                Y2 = lines.First().GetEnd().Y,
                X2 = lines.First().GetEnd().X,
            };

            XMovement = lines[currentObj].Width /  (lines[currentObj].GetLineLength() * 2 );
            YMovement = lines[currentObj].Height / (lines[currentObj].GetLineLength() * 2 );

            //wird nur beim ersten mal ausgeführt
            if(!calledOnce)
            {
                //Timer für die ständige Aktualisierung der Positionen
                Timer.Tick += new EventHandler(Cycle);
                Timer.Interval = TimeSpan.FromMilliseconds(10);

                calledOnce = true;
            }

            Timer.Start();
        }


        private static void Cycle(object sender, EventArgs e)
        {
            //Wenn ein Schritt in X Richtung ausgeführt werden soll
            if (StepX())
            {
                MotorX.Margin = new Thickness(Position.Margin.Left, MotorX.Margin.Top, 0, 0);               
            }

            //Wenn ein Schritt in Y Richtung ausgeführt werden soll
            if (StepY())
            {
                MotorY.Margin = new Thickness(MotorY.Margin.Left, Position.Margin.Top, 0, 0);
            }
                   
            //Position die sich aus den 2 Schrittmotoren ergibt
            ActualPos.Margin = new Thickness(MotorX.Margin.Left, MotorY.Margin.Top, 0, 0);

            //theoretische Position
            Position.Margin = new Thickness(Position.Margin.Left + XMovement, Position.Margin.Top + YMovement, 0, 0);


            //wird true wenn linie beendet ist
            var lineFinished = false;

            // wenn die Breite größer als 0 ist
            if (currentLine.GetWidth() > 0)
            {                
                if (currentLine.X2 <= Position.Margin.Left +5)
                    lineFinished = true;
            }
            // wenn die Breite kleiner als 0 ist
            else if (currentLine.GetWidth() < 0)
            {
                if (currentLine.X2 >= Position.Margin.Left +5 )
                    lineFinished = true;
            }
            //bei Linien mit Breite = 0
            else
            {
                //bei positiver Höhe
                if(currentLine.GetHeight() > 0)
                {
                    if (currentLine.Y2 <= Position.Margin.Top +5)
                        lineFinished = true;
                }
                //bei negativer Höhe
                else
                {
                    if (currentLine.Y2 >= Position.Margin.Top +5)
                        lineFinished = true;
                }
            }


            if(lineFinished)
            {
                bool drawNextObject = true;

                // bei einem Kreis muss die X und Y Bewegung berechnet werden,
                // wenn der Kreis noch nicht beendet ist
                if (drawingCircle)
                {
                    // Kreis wird abgespeichert
                    var circle = (DrawCircle)lines[currentObj];

                    // neuer Winkel wird berechnet
                    currentAngle += anglePerVertex;


                    // Wenn der Kreis noch nicht beendet ist,
                    if (currentAngle < (circle.StartAngle - 90 + circle.CircleSizeAngle))
                    {
                        // nächstes Objekt soll nicht gezeichnet werden, da der Kreis noch nicht beendet ist
                        drawNextObject = false;

                        var startPoint = new Point(currentLine.X2, currentLine.Y2);

                        var endPoint = CalculatePoint(circle.CenterPoint, circle.Radius, currentAngle + anglePerVertex);
                     
                        // neue Linie zum nächsten Eckpunkt
                        currentLine = new Line()
                        {
                            X1 = startPoint.X,
                            Y1 = startPoint.Y,

                            X2 = endPoint.X,
                            Y2 = endPoint.Y
                        };

                        XMovement = currentLine.GetWidth() / (currentLine.GetLineLength() * 2);
                        YMovement = currentLine.GetHeight() / (currentLine.GetLineLength() * 2);
                    }
                }

                // Wenn das nächste Objekt gezeichnet werden soll
                if(drawNextObject)
                {
                    //zum nächsten Objekt
                    currentObj++;

                    currentLine = new Line()
                    {
                        X1 = lines.First().GetStart().X,
                        Y1 = lines.First().GetStart().Y,

                        Y2 = lines.First().GetEnd().Y,
                        X2 = lines.First().GetEnd().X,
                    };

                    //Wenn das Objekt existiert
                    if ((lines.Length - 1 >= currentObj))
                         {
                        // wenn das nächste Objekt eine Linie ist
                        if(lines[currentObj].GetType() == typeof(DrawLine))
                        {
                            drawingCircle = false;

                            // neue Linie, die eingebrannt werden soll, wird gespeichert
                            currentLine = new Line()
                            {
                                X1 = lines[currentObj].GetStart().X,
                                Y1 = lines[currentObj].GetStart().Y,

                                Y2 = lines[currentObj].GetEnd().Y,
                                X2 = lines[currentObj].GetEnd().X,
                            };

                            //Position wird festgelegt
                            Position.Margin = new Thickness(lines[currentObj].GetStart().X - 5, lines[currentObj].GetStart().Y - 5, 0, 0);

                            // bei Linien bleibt die Geschwindigkeit der X und Y Achse bis zum Ende der Linie gleich
                            XMovement = lines[currentObj].Width / (lines[currentObj].GetLineLength() * 2);
                            YMovement = lines[currentObj].Height / (lines[currentObj].GetLineLength() * 2);
                        }
                        // wenn es sich um einen Kreis handelt
                        else if(lines[currentObj].GetType() == typeof(DrawCircle)) 
                        {
                            drawingCircle = true;

                            // Kreis wird abgespeichert
                            var circle = (DrawCircle)lines[currentObj];

                            // Kreis wird als Vieleck gezeichnet
                            // Winkel pro Ecke
                            anglePerVertex = circle.CircleSizeAngle / (circle.GetLength() / 10);

                            // Mit dem Anfangswinkel des Kreises wird begonnen
                            currentAngle = circle.StartAngle - 90;

                            // Startpunkt der Linie
                            var startPoint = circle.GetStart();

                            // Endpunkt der Linie
                            var lineEnd = CalculatePoint(circle.CenterPoint, circle.Radius, currentAngle + anglePerVertex);

                            currentLine = new Line()
                            {
                                X1 = startPoint.X,
                                Y1 = startPoint.Y,

                                X2 = lineEnd.X,
                                Y2 = lineEnd.Y
                            };

                            // bei einem Kreis muss die Bewegung immer neu berechnet werden 
                            XMovement = currentLine.GetWidth() / (currentLine.GetLineLength() * 2);
                            YMovement = currentLine.GetHeight() / (currentLine.GetLineLength() * 2);
                        }
                    }
                    else
                {
                    Timer.Stop();

                    //Kreise werden entfernt
                    MainWindow.ThisWindow.DrawField.Children.Remove(Position);
                    MainWindow.ThisWindow.DrawField.Children.Remove(MotorX);
                    MainWindow.ThisWindow.DrawField.Children.Remove(MotorY);
                    MainWindow.ThisWindow.DrawField.Children.Remove(ActualPos);

                    currentObj = 0;
                }
                }

            }
        }


        private static bool StepX()
        {
            // wenn die Distanz 0 ist, soll kein Schritt ausgeführt werden
            if (currentLine.GetWidth() == 0)
                return false;

            // Wenn die Distanz positiv ist
            if (currentLine.GetWidth() > 0)
            {
                //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
                if ((ActualPos.Margin.Left + StepDistance) <= Position.Margin.Left)
                    return true;
            }
            //bei negativer Distanz
            else
            {
                //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
                if ((ActualPos.Margin.Left - StepDistance) >= Position.Margin.Left)
                    return true;
            }

            return false;
        }

        private static bool StepY()
        {
            // wenn die Distanz 0 ist, soll kein Schritt ausgeführt werden
            if (currentLine.GetHeight() == 0)
                return false;

            // Wenn die Distanz positiv ist
            if (currentLine.GetHeight() > 0)
            {
                //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
                if ((ActualPos.Margin.Top + StepDistance) <= Position.Margin.Top)
                    return true;
            }
            //bei negativer Distanz
            else
            {
                //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
                if ((ActualPos.Margin.Top - StepDistance) >= Position.Margin.Top)
                    return true;
            }

            return false;
        }
                                

        private static double GetHeight(this Line line)
        {
            return (line.Y2 - line.Y1);
        }

        private static double GetWidth(this Line line)
        {
            return (line.X2 - line.X1);
        }

        private static double GetLineLength(this IDrawable line)
        {
            return Math.Sqrt(Math.Pow(line.Width, 2) + Math.Pow(line.Height, 2));
        }

        private static double GetLineLength(this Line line)
        {
            return Math.Sqrt(Math.Pow(line.GetWidth(), 2) + Math.Pow(line.GetHeight(), 2));
        }

        /// <summary>
        /// Berechnet Punkt anhand eines anderen Punkts,
        /// dem Abstand und dem Winkel
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="disctance"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static Point CalculatePoint(Point startPoint, double disctance, double angle)
        {
            return new Point()
            {
                X = startPoint.X + (Math.Cos(angle * (Math.PI / 180.0)) * disctance),
                Y = startPoint.Y + (Math.Sin(angle * (Math.PI / 180.0)) * disctance)
            };
        }
    }
}
