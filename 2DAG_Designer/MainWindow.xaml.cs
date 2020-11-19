using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using _2DAG_Designer.DrawingObjects;
using _2DAG_Designer.DrawingObjects.Objects;
using _2DAG_Designer.FileIO;
using _2DAG_Designer.DrawingObjects.Edit;
using _2DAG_Designer.DrawingObjects.Groups;
using _2DAG_Designer.Arduino;
using _2DAG_Designer.Arduino.Engrave;
using _2DAG_Designer.BurnSimulation;
using System.Diagnostics;

namespace _2DAG_Designer
{    /*    TODO:
      *    
      * undo/redo   
      * 
    */
    public partial class MainWindow : Window
    {
        //--------------- Variablen------------------------
        #region Variablen

        //größe der Zeichenreihe (Höhe und Breite immer gleich, deswegen nur ein Wert)
        public static double DrawRowSize;

        // ist true, wenn eine Funktion von der undoLastAction Funktion aufgerufen wird
        // ist nötig, da beim undo Prozess nicht in die Aktionsliste eingetragen werden soll
        public static bool Undooing = false;

        public static bool Redooing = false;

        //für den ersten Klick in die DrawArea
        public static bool FirstDraw = true;

        //Wird benötigt, dass von anderen Dateien auf die Funktionen wie z.B. addToCanvas zugegriffen werden kann
        public static MainWindow ThisWindow;

        #region Enums       

        /// <summary>
        /// Verschiedene Aktionsarten in der App
        /// </summary>
        public enum ActionType
        {
            Draw,
            Delete,
            
            Restore,
            RestoreAll,

            EditSize,
        }

        #endregion

        #region Listen

        //Liste der ausgeführten Aktionen
        public static List<ActionType> ActionList = new List<ActionType>();

        //Liste der rückgängig gemachten Aktionen
        public static List<ActionType> reversedActionList = new List<ActionType>();


        //Liste aller gezeichneten Objekte
        public static List<IDrawable> DrawList = new List<IDrawable>();
   

        //Liste der gelöschten Objekte (Arrayliste, da auch mehrere Objekte auf einmal gelöscht werden können)
        public static List<IDrawable[]> DeletedDrawList = new List<IDrawable[]>();

        #endregion

        private string _currentFilePath = "";

        //Fenstergröße
        double WindowHeight;
        double WindowWidth;

        //true wenn Linien gezeichnet werden, false wenn Bögen gezeichnet werden
        bool DrawLines = true;

        #endregion
        //-------------------------------------------------

        #region ctor

        public MainWindow()
        {
            InitializeComponent();

            ThisWindow = this;
        }

        #endregion

        // -------------- EVENTHANDLER --------------------
        #region Eventhandler

        #region Menu Bar Handler

        //Datei öffnen
        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //alle vorhandenen Objekte werden entfernt
                UndrawallObjects();

                //Explorer wird geöffnet und der zurückgegebene Dateipfad abgespeichert
                _currentFilePath = DrawFile.BrowseExplorer();

                //Wenn der Pfad nicht leer ist,
                if(_currentFilePath != "")
                    // Datei wird eingelesen und Objekte werden erstellt
                    DrawFile.ListFromFile(_currentFilePath);

                //Aktion wird hinzugefüt
                ActionList.Add(ActionType.RestoreAll);
            }
            catch
            {
                //Fehlermeldung
                MessageBox.Show("Datei konnte nicht geöffnet werden!");
            }
        }

        //Datei speichern
        private void MenuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //wenn noch kein Pfad vorhanden ist,
                if(_currentFilePath == "")
                {
                    //Dateipfad wird abgespeichert
                    _currentFilePath = DrawFile.SaveFileToDialog();
                }

                //Wenn der Pfad nicht leer ist,
                if (_currentFilePath != "")
                    //Objekte werden abgespeichert
                    DrawFile.SaveToFile(_currentFilePath);
            }
            catch
            {
                //Fehlermeldung
                MessageBox.Show("Datei konnte nicht gespeichert werden!");
            }
        }

        //Datei speichern unter
        private void MenuSaveFileTo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Dateipfad wird abgespeichert
                _currentFilePath = DrawFile.SaveFileToDialog();

                //Wenn der Pfad nicht leer ist,
                if (_currentFilePath != "")
                    //Datei wird gespeichert
                    DrawFile.SaveToFile(_currentFilePath);
            }
            catch
            {
                //Fehlermeldung
                MessageBox.Show("Datei konnte nicht gespeichert werden!");
            }
        }


        //Dokumentation öffnen
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Prozess zum öffnen des Dokuments wird erstellt
            var openDocumentation = new Process();

            //Länge des Pfades der Basedirectory wird abgespeichert
            var length = AppDomain.CurrentDomain.BaseDirectory.Length;

            // Basedirectory befindet sich in bin/Debug
            // Dokumentation befindet sich direkt im Projektordner
            // bin//Debug wird also entfernt
            openDocumentation.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory.Remove(length - 10) + "Dokumentation.docx";

            //Prozess wird gestartet, Dokument öffnet sich
            openDocumentation.Start();
        }
        
        #endregion

        #region CheckBox Handler

        private void DrawLineCheck_Click(object sender, RoutedEventArgs e)
        {
            //Es kann immer nur eine der beiden Checkboxen aktiviert sein
            DrawCircleCheck.IsChecked = !DrawLineCheck.IsChecked;

            DrawLines = (bool)DrawLineCheck.IsChecked;
        }

        private void DrawCircleCheck_Click(object sender, RoutedEventArgs e)
        {
            //Es kann immer nur eine der beiden Checkboxen aktiviert sein
            DrawLineCheck.IsChecked = !DrawCircleCheck.IsChecked;

            DrawLines = (bool)DrawLineCheck.IsChecked;
        }

        #endregion

        #region Button Handler

        #region Reverse Redo

        private void Reverse_Click(object sender, RoutedEventArgs e)
        {
            UndoLastAction();
        }
 
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            RedoAction();
        }        

        #endregion

        #region delete

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            UndrawlastObject();
        }

        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            UndrawallObjects();
        }

        #endregion

        // Eventhandler für den Zeichenbereich
        private void DrawArea_Click(object sender, RoutedEventArgs e)
        {
            // Wenn das erste mal angeklickt wird, 
            if(FirstDraw)
            {
                // wird der erste Startpunkt festgelegt
                var startPosition = Mouse.GetPosition(DrawArea); //Mausposition im Bezug auf DrawArea wird abgefragt

                //es ein Punkt erstellt und am Start angezeigt
                new DrawEllipse(startPosition, 4, 4, Brushes.Black, true);

                //addToCanvas(DrawList.Last().ThisObject);

                // ab jetzt werden Linien gezeichnet
                FirstDraw = false;

                //Aktion wird zur Aktionsliste hinzgefügt
                ActionList.Add(ActionType.Draw);
            }
            else
            {
                //der Start für das neue Objekt, ist das ende des letzten Objekts
                var newShapeStart = DrawList.Last().GetEnd();

                //Endposition ist der Mausklick
                var newShapeEnd = Mouse.GetPosition(DrawArea); //Mausposition im Bezug auf DrawArea wird abgefragt

                //wenn shift gedrückt ist, wird die Linie in grau angezeigt
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    //graue Linie wird gezeichnet, diese Linien werden nicht eingraviert
                    DrawLine(newShapeStart, newShapeEnd, Brushes.DarkSlateGray);
                }
                else
                {
                    if(DrawLines)
                        // die Linie wird gezeichnet in schwarz
                        DrawLine(newShapeStart, newShapeEnd, Brushes.Black);
                    //else
                        //Bogen wird in schwarz gezeichnet
                        //DrawArc(newShapeStart, newShapeEnd, Brushes.Black);

                    //drawArc(drawList.Last().GetStart(),
                    //       100, 100, 0, Brushes.Black);
                }

                Measure();

            }
        }

        private void CreateArcButton_Click(object sender, RoutedEventArgs e)
        {         
            try
            {
                //Werte aus den Textboxen werden abgespeichert
                double radius = Convert.ToDouble(RadiusTextBox.Text);
                double circleSizeAngle = Convert.ToDouble(CircleSizeTextBox.Text);


                //der Radius und die Größe des Kreises müssen positiv sein
                if((circleSizeAngle <= 0) || (radius <= 0))
                {
                    throw new Exception("");
                }

                //Wenn die Konvertierung nicht funktioniert, wird der folgende Teil automatisch übersprungen

                //Umrechnung von cm in Pixel
                radius = CentimeterTopixel(radius);

                //der Kreis wird gezeichnet
                DrawCircle(DrawList.Last().GetEnd(),
                        radius, circleSizeAngle, DrawList.Last().Angle, Brushes.Black);

                //wenn alles funktioniert hat, bleibt die Umrandung des Buttons schwarz
                CreateArcButton.BorderBrush = Brushes.Gray;

                //Maße werden angezeigt
                Measure();
            }
            catch
            {
                // wenn etwas nicht funktioniert hat, wird der createArcButton rot umrandet
                CreateArcButton.BorderBrush = Brushes.Red;
            }
        }
        
        private void ApplySizeChangeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChangeSizeTextBox.Text == "0")
                    throw new Exception("");

                Edit.EditObjectSize(Double.Parse(ChangeSizeTextBox.Text));

                ApplySizeChangeButton.BorderBrush = Brushes.Gray;
            }
            catch
            {
                // wenn etwas nicht funktioniert hat, wird der Button rot umrandet
                ApplySizeChangeButton.BorderBrush = Brushes.Red;
            }
         
            //Messungen werden aktualisiert
            Measure();
        }
        
        private void ApplyTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //neues Wort wird erstellt, mit dem eingegebenen Text
                new DrawWord(DrawList.Last().GetEnd(), DrawLettersTextBox.Text);

                //Zeichenaktion wird hinzugefügt
                ActionList.Add(ActionType.Draw);

                //Standartfarbe
                ApplyTextButton.BorderBrush = Brushes.Gray;
            }
            catch
            {
                //bei einem Fehler wird die Umrandung des Buttons rot
                ApplyTextButton.BorderBrush = Brushes.Red;
            }
        }

        #region Arduino

        private void ConnectComButton_Click(object sender, RoutedEventArgs e)
        {
            //ConnectButton wurde gedrückt
            Arduino.Communication.ConnectButtonClicked();

            //Wenn eine Verbindung existiert
            if(Communication.IsConnected)
            {
                //UploadButton wird sichtbar
                ArduinoUploadButton.IsEnabled = true;
            }
            else
            {
                //UploadButton wird unsichtbar
                ArduinoUploadButton.IsEnabled = false;
            }
        }

        private void SearchPortsButton_Click(object sender, RoutedEventArgs e)
        {
            //Ports werden gesucht
            var ports = Arduino.Communication.GetPorts();

            //bisherige Ports werden entfernt
            AvailablePortsComboBox.Items.Clear();

            //Wenn kein Port gefunden wurde
            if (ports[0] == null)
                AvailablePortsComboBox.Items.Add("kein Port gefunden");

            //alle zurückgegebenen ports werden hinzugefügt
            foreach (var port  in ports)
            {
                //Port wird hinzugefügt
                AvailablePortsComboBox.Items.Add(port);
            }
        }

        private void ArduinoUploadButton_Click(object sender, RoutedEventArgs e)
        {
            //Wenn eine Verbindung zum Arduino besteht
            if(Communication.IsConnected)
            {
                ////Bewegung in X Richtung
                //ArduinoTest.MoveX((int)Math.Round(PixelToCentimeter(DrawList.Last().Width)));

                ////Bewegung in Y Richtung
                //ArduinoTest.MoveY((int)Math.Round(PixelToCentimeter(DrawList.Last().Height)));

                Engrave.Upload(DrawList.ToArray());
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Wenn eine Verbindung besteht
            if (Communication.IsConnected)
                // Prozess wird gestartet
                Engrave.Start();
        }

        #endregion

        private void Schrittmotor_Click(object sender, RoutedEventArgs e)
        {
            StepmotorMovement.Start(DrawList.GetRange(1, DrawList.Count - 1).ToArray());
        }

        #endregion
        
        /// <summary>
        /// wird ausgeführt, wenn auf der Tastatur eine Taste gedrückt wurde
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //wird true wenn eine der Aktionen ausgeführt wird
            bool anyActionTriggered = false;

            //Wenn die Textbox fokusiert ist, finden keine Verschiebungen etc statt
            if(!DrawLettersTextBox.IsFocused)
            {
                //Aktionen, die nur ausgeführt werden können, wenn mindestens 1 Objekt existiert
                if(DrawList.Count > 0)
                {
                    //nach oben verschieben
                     if (Keyboard.IsKeyDown(Key.NumPad8))
                    {
                        //um 4 Pixel verschoben
                        Edit.EditDraw(DrawList.Last(), Edit.EditType.EndUp);

                        anyActionTriggered = true;
                    }

                    // nach unten verschieben
                    if (Keyboard.IsKeyDown(Key.NumPad2))
                    {
                        //um 4 Pixel verschoben
                        Edit.EditDraw(DrawList.Last(), Edit.EditType.EndDown);

                        anyActionTriggered = true;
                    }

                    // nach links verschieben
                    if (Keyboard.IsKeyDown(Key.NumPad4))
                    {
                        //um 4 Pixel verschoben
                        Edit.EditDraw(DrawList.Last(), Edit.EditType.EndLeft);

                        anyActionTriggered = true;
                    }

                    // nach rechts verschieben
                    if (Keyboard.IsKeyDown(Key.NumPad6))
                    {
                        //um 4 Pixel verschoben
                        Edit.EditDraw(DrawList.Last(), Edit.EditType.EndRight);

                        anyActionTriggered = true;
                    }

                    //invert
                    if (Keyboard.IsKeyDown(Key.NumPad5))
                    {
                        //
                        Edit.EditDraw(DrawList.Last(), Edit.EditType.Invert);

                        anyActionTriggered = true;
                    }

                    //rotieren
                    if (Keyboard.IsKeyDown(Key.R))
                    {
                        Edit.EditDraw(DrawList.Last(), Edit.EditType.Angle,DrawList.Last().Angle + 2.5);

                        anyActionTriggered = true;
                    }

                }
            }

            //Rückgängig machen STRG + Z
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
            {
                //letzte Aktion wird rückgängig gemacht
                UndoLastAction();

                anyActionTriggered = true;
            }

            //wieder Ausführen STRG + Y
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Y))
            {
                //letzte rückgängig gemachte Aktion wird wieder ausgeführt
                RedoAction();

                anyActionTriggered = true;
            }

            //Speichern mit STRG + S
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                //Speichern
                MenuSaveFile_Click(null, null);

                anyActionTriggered = true;
            }

            //nur wenn eine der Aktionen durchgeführt wurde, werden die Maße angezeigt
            if (anyActionTriggered)
                Measure();
        }
                
        #endregion
        //-------------------------------------------------


        //--------------- HELPER --------------------------
        #region Helper

        #region drawing

        /// <summary>
        /// zeichnet eine Line vom Start zum Endpunkt   
        /// </summary>
        /// <param name="lineStart">Start der Linie</param>
        /// <param name="lineEnd">Ende der Linie</param>
        private void DrawLine(Point lineStart, Point lineEnd, SolidColorBrush color)
        {
            //neue Linie wird erstellt und der objectCanvas hinzugefügt
            new DrawLine(lineStart, lineEnd, color, true);

            //Aktion wird der Aktionsliste hinzugefügt
            ActionList.Add(ActionType.Draw);
        }

        /// <summary>
        /// zeichnet Arc mit Endpunktangabe
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        private void DrawCircle(Point startPoint,double radius, double circleSizeAngle, double startAngle, SolidColorBrush color)
        {
            //Bogen wird erstellt und der objectCanvas hinzugefügt
            new DrawCircle(startPoint, radius, circleSizeAngle, startAngle, color);
           
            //addToCanvas(DrawList.Last().ThisObject);

            //Aktion wird der Aktionsliste hinzugefügt
            ActionList.Add(ActionType.Draw);
        }
        #endregion

        #region undraw/restore

        /// <summary>
        /// letztes Objekt wird entfernt
        /// </summary>
        public void UndrawlastObject()
        {
            //Wenn mindestens 1 Objekt existiert, kann das Letzte gelöscht werden
            if(DrawList.Count >= 1)
            { 
                //Objekt wird aus der objectCanvas entfernt
                DrawList.Last().Remove();

                //Objekt wird zu den gelöschten Objekten hinzugefügt
                DeletedDrawList.Add(new IDrawable[1] { DrawList.Last() });
  
                //Objekt wird aus der Liste gelöscht 
                DrawList.RemoveAt(DrawList.Count - 1);

                //aktion wird der Aktionsliste hinzugefügt, es sei denn diese Funktion wird von der undoLastAction Funktion aufgerufen
                if(!Undooing)
                    ActionList.Add(ActionType.Delete);
                else
                    reversedActionList.Add(ActionType.Delete);
            }

            //Wenn jetzt kein Objekt mehr existiert,
            if(DrawList.Count == 0)
            {
                //startet der Zeichenprozess von vorne
                FirstDraw = true;
            }

            //Messungen werden aktualisiert
            Measure();
        }

        /// <summary>
        /// löscht alle Objekte
        /// </summary>
        public void UndrawallObjects()
        {
            //enthält die in diesem Druchlauf gelöschten Objekte
            var deleted = new List<IDrawable>();

            foreach(var obj in DrawList)
            {
                //Objekt wird aus der objectCanvas entfernt
                obj.Remove();

                //und zu den gelöschten Objekten hinzugefüht 
                deleted.Add(obj);
            }

            //DrawList wird geleert
            DrawList.Clear();

            //gelöschte Objekte werden der gelöscht List hinzugefügt
            DeletedDrawList.Add(deleted.ToArray());

            //Zeichenprozess wird von vorn gestartet
            FirstDraw = true;

            //Aktion wird zur Aktionsliste hinzgefügt, wenn diese Funktion nicht von der undoLastAction Funktion aufgerufen wirde
            if(!Undooing)
                ActionList.Add(ActionType.Delete);
            else
                reversedActionList.Add(ActionType.Delete);

            //Messungen werden aktualisiert
            Measure();
        }

        /// <summary>
        /// stellt die zuletzt gelöschten Objekte wirder her
        /// </summary>
        public void RestoredeledetObject()
        {
            //wenn bereits ein Objekt gelöscht wurde
            if(DeletedDrawList.Count > 0)
            {
                //Aktion wird zur Aktionsliste hinzgefügt
                //Wenn mehrere Objekte wiederhergestellt werden, wird die Aktion restoreall hinzugefügt
                if(Undooing)
                {
                    // wenn die Funktion von der undoLastAction Funktion aufgerufen wird, wird nicht in die actionList eingetragen
                    //sondern in die reversedActionList
                    //Aktion wird der Liste für rückgängig gemachte Aktionen hinzugefügt

                    if (DeletedDrawList.Last().Count() > 1)
                        reversedActionList.Add(ActionType.RestoreAll);
                    else
                        reversedActionList.Add(ActionType.Restore);
                }
                else if (DeletedDrawList.Last().Count() > 1)
                    ActionList.Add(ActionType.RestoreAll);
                else
                    ActionList.Add(ActionType.Restore);

                //jedes zuletzt gelöschte Objekt wird wiederhergestellt
                for (int i = 0; i < DeletedDrawList.Last().Count(); i++)
                {
                    //zur drawList
                    DrawList.Add(DeletedDrawList.Last()[i]);

                    //wird gezeichnet
                    DeletedDrawList.Last()[i].AddToCanvas();
                }

                //aus den gelöschten entfernt
                DeletedDrawList.Remove(DeletedDrawList.Last());

                //Zeichenprozess hat in jedem Fall begonnen
                FirstDraw = false;

                Measure();
            }
        }


        /// <summary>
        /// macht die letzte Aktion rückgängig
        /// </summary>
        private void UndoLastAction()
        {
            Undooing = true;

            try
            {

                //für jeden Aktionstyp gibt es einen case, dort wird die Aktion rückgängig gemacht
                switch (ActionList.Last())
                {
                    case ActionType.Delete:
                        {
                            RestoredeledetObject();
                        }
                        break;

                    case ActionType.Draw:
                        {
                            UndrawlastObject();
                        }
                        break;

                    case ActionType.Restore:
                        {
                            UndrawlastObject();
                        }
                        break;

                    case ActionType.RestoreAll:
                        {
                            UndrawallObjects();
                        }
                        break;
                    case ActionType.EditSize:
                        {
                            RemoveAll();

                            Edit.EditObjectSize(100 / Edit.SizeChangedFactors.Last());

                            DrawAll();

                            reversedActionList.Add(ActionType.EditSize);
                        }
                        break;
                }

                //rückgängig gemachte Aktion wird aus der Aktionsliste entfernt
                ActionList.RemoveAt(ActionList.Count - 1);
            }
            catch
            {
            }

            Undooing = false;

            Measure();
        }

        /// <summary>
        /// führt eine rückgängig gemachte Aktion erneut aus
        /// </summary>
        private void RedoAction()
        {
            Undooing = false;
            Redooing = true;


            try
            {
                //für jeden Aktionstyp gibt es einen case, dort wird die Aktion wieder ausgeführt
                switch (reversedActionList.Last())
                {
                    case ActionType.Delete:
                        {
                            RestoredeledetObject();
                        }
                        break;

                    case ActionType.Draw:
                        {
                            UndrawlastObject();
                        }
                        break;

                    case ActionType.Restore:
                        {
                            UndrawlastObject();
                        }
                        break;

                    case ActionType.RestoreAll:
                        {
                            UndrawallObjects();
                        }
                        break;
                    case ActionType.EditSize:
                        {
                            RemoveAll();

                            Edit.EditObjectSize(100 / Edit.ReversedSizeChangedFactors.Last());

                            ActionList.Add(reversedActionList.Last());

                            DrawAll();
                        }
                        break;
                }

                //wieder ausgeführte Aktion wird aus der Liste entfernt
                reversedActionList.Remove(reversedActionList.Last());
            }
            catch
            {

            }

            Redooing = false;
        }

        #endregion

        /// <summary>
        /// fügt ein Objekt der objectCanvas hinzu
        /// </summary>
        /// <param name="element"></param>
        public void AddToCanvas(UIElement element)
        {
            //element wird zur objectCanvas hinzugefügt
            DrawField.Children.Add(element);
        }

        public void RemoveFromCanvas(UIElement element)
        {
            DrawField.Children.Remove(element);
        }

        private void RemoveAll()
        {
            foreach (var drawObject in DrawList)
            {
                drawObject.Remove();
            }
        }

        private void DrawAll()
        {
            foreach (var drawObject in DrawList)
            {
                drawObject.AddToCanvas();
            }
        }

        /// <summary>
        /// Zeigt Maße an
        /// </summary>
        public void Measure()
        {
            try
            {
                //Breite und Höhe werden in von Pixel in cm umgerechnet
                double width   = PixelToCentimeter(DrawList.Last().Width);
                double height  = PixelToCentimeter(DrawList.Last().Height);
                
                // Winkel wird auf 2 Nachkommastellen gerundet
                double angle   = Math.Round(DrawList.Last().Angle, 2);

                if (angle == 360)
                    angle = 0;

                //negative Werte werden als positiv angezeigt
                if (width < 0)
                    width = -width;
                if (height < 0)
                    height = -height;
                
                //die Werte werden in den Textlabeln angezeigt
                WidthLabel.Content = "Breite: " + (Math.Round(width, 2)) + "cm";
                HeightLabel.Content = "Höhe: " + (Math.Round(height, 2)) + "cm";

                //der Winkel wird nur angezeigt, wenn er nicht 0 ist
                if (angle != 0)
                {
                    //Winkel wird im angleLabel angezeigt
                    AngleLabel.Content = "Winkel: " + angle + "°";

                    //Bei einer Linie wird der Winkel in Klammern angezeigt, da er hier nich so wichtig sit
                    if(DrawList.Last().GetType() == typeof(DrawLine))
                    {
                        AngleLabel.Content = "(" + AngleLabel.Content + ")";
                    }
                }
                else
                    AngleLabel.Content = "";
            }
            catch
            {
                //wenn es nicht funktioniert, werden keine Werte angezeigt
                WidthLabel.Content = "";
                HeightLabel.Content = "";
                AngleLabel.Content = "";
            }           
        }

        /// <summary>
        /// Überprüft ob sich ein Punkt inerhalb des DrawField befindet
        /// </summary>
        /// <param name="point"></param>
        public static bool InDrawField(Point point)
        {
            //überprüft, ober der Punkt im Bereich ist
            return (point.X <= DrawRowSize + 0.5 && point.X >= -0.5) && (point.Y <= DrawRowSize + 0.5 && point.Y >= -0.5); //Tolleranz von 0,5
        }

        /// <summary>
        /// Umrechnung von Pixel in Centimeter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double PixelToCentimeter(double value)
        {
            return value / (DrawRowSize / 15);
        }

        /// <summary>
        /// Umrechnung von Centimeter in Pixel
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double CentimeterTopixel(double value)
        {
            return value * (DrawRowSize / 15);
        }

        /// <summary>
        /// gibt die Distanz zwischen 2 Punkten zurück
        /// </summary>
        public static double DistanceBetween(Point p1, Point p2)
        {
            //Entfernung der Punkte wird berechnet und zurückgegeben
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        #endregion
        //-------------------------------------------------


        //-------------- WINDOW ---------------------------
        #region Window

        /// <summary>
        /// zeichnet die Orientierungslinien
        /// </summary>
        private void DrawAreaLines()
        {
            //alls wird aus dem Canvas entfernt
            DrawField.Children.Clear();

            XLabelCanvas.Children.Clear();
            YLabelCanvas.Children.Clear();

            // Linien auf dem Zeichenfeld werden gezeichnet
            //horizontal
            //cm
            for (int i = 0; i <= 15; i++)
            {

                var Line = new Line()
                {
                    //Linie von X1, Y1
                    X1 = 0,
                    Y1 = i * (DrawRow.ActualHeight / 15),

                    //zu X2, Y2
                    X2 = DrawColumn.ActualWidth,
                    Y2 = i * (DrawRow.ActualHeight / 15),

                    Stroke = Brushes.Gray,         //Farbe der Linie wird festgelegt
                    StrokeThickness = 2
                };
                DrawField.Children.Add(Line);

                //cm Bezeichnung
                var Label = new Label()
                {
                    Content = i,

                    Margin = new Thickness((i * (DrawRow.ActualHeight / 15)) - 10, 15, 0, 0),
                };
                XLabelCanvas.Children.Add(Label);
            }
            //mm
            for (int i = 0; i <= 150; i++)
            {
                var Line = new Line()
                {
                    //Linie von X1, Y1
                    X1 = 0,
                    Y1 = i * (DrawRow.ActualHeight / 150),

                    //zu X2, Y2
                    X2 = DrawColumn.ActualWidth,
                    Y2 = i * (DrawRow.ActualHeight / 150),

                    Stroke = Brushes.LightSlateGray,         //Farbe der Linie wird festgelegt
                    StrokeThickness = 0.7
                };

                DrawField.Children.Add(Line);
            }

            //vertikal
            //cm
            for (int i = 0; i <= 15; i++)
            {
                var Line = new Line()
                {
                    //Linie von X1, Y1
                    X1 = i * (DrawColumn.ActualWidth / 15),
                    Y1 = 0,

                    //zu X2, Y2
                    X2 = i * (DrawColumn.ActualWidth / 15),
                    Y2 = DrawRow.ActualHeight,

                    Stroke = Brushes.Gray,         //Farbe der Linie wird festgelegt
                    StrokeThickness = 2
                };
                DrawField.Children.Add(Line);

                //cm Bezeichnung
                var Label = new Label()
                {
                    Content = i,


                    Margin = new Thickness( YLabelCanvas.ActualWidth - 20, (i * (DrawRow.ActualHeight / 15)) - 10, 0, 0),

                    //nach oben und nach links gebunden
                };
                YLabelCanvas.Children.Add(Label);

            }
            //mm
            for (int i = 0; i <= 150; i++)
            {
                var Line = new Line()
                {
                    //Linie von X1, Y1
                    X1 = i * (DrawColumn.ActualWidth / 150),
                    Y1 = 0,

                    //zu X2, Y2
                    X2 = i * (DrawColumn.ActualWidth / 150),
                    Y2 = DrawRow.ActualHeight,

                    Stroke = Brushes.LightSlateGray,         //Farbe der Linie wird festgelegt
                    StrokeThickness = 0.7
                };


                DrawField.Children.Add(Line);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // um die Applikation zu beenden
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //Zeichenlinien werden gezeichnet
            DrawAreaLines();

            //damit die Labels zurück gesetzt werden
            Measure();  
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //man kann die größe des Fensters nur verändern, wenn noch nichts gezeichnet wurde!
            if (FirstDraw)
            {
                //Seitenverhältnis wird beibehalten und Werte werden abgespeichert
                WindowWidth = Application.Current.MainWindow.Width = Application.Current.MainWindow.Height * 1.5;
                WindowHeight = Application.Current.MainWindow.Height;

                //DrawRow und Column sind immer gleich groß
                DrawRow.Height = new GridLength(Application.Current.MainWindow.Height * 0.85);
                DrawColumn.Width = DrawRow.Height;

                //DrawRowSize wird abgespeichert
                DrawRowSize = DrawRow.ActualHeight;

                //AreaLines werden neu gezeichnet
                DrawAreaLines();
            }
            else
            {
                //Werte werden auf die abgespeicherten Werte gesetzt
                Application.Current.MainWindow.Height = WindowHeight;
                Application.Current.MainWindow.Width = WindowWidth;
            }
        }


        #endregion
        //-------------------------------------------------
    }
}