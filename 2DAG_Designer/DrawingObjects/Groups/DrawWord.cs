using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using _2DAG_Designer.FileIO;

namespace _2DAG_Designer.DrawingObjects.Groups
{
    class DrawWord : DrawGroup
    {
        public string _Word;


        public DrawWord(Point startPoint, string word)
        {
            //word wird in Großbuchstaben geschreiben
            word.ToUpper();

            this._Word = word;
            this.GroupStart = startPoint;

            var firstletter = true;

            //Jeder Buchstabe wird einzeln hinzugefügt
            foreach (var letter in word)
            {
                string filepath;

                if(letter == ' ')
                {
                    //Pfad befindet sich im letters ordner
                    filepath = @"letters\_.txt";
                }
                else
                {
                    //Pfad befindet sich im letters ordner
                    filepath = @"letters\" + letter + ".txt";
                }

                //Objekte des Buchstabens werden der Liste hinzugefügt
                var letterObjectList = DrawFile.ListFromFile(filepath);

                //Position wird für jedes Objekt berechnet
                foreach (var drawObject in letterObjectList)
                {
                    var index = letterObjectList.IndexOf(drawObject);

                    //Wenn das Object nicht das erste ist
                    if(index > 0)
                    {
                        //der Start ist das Ende des letzten Objekts
                        drawObject.ObjectStart = letterObjectList[index - 1].GetEnd();
                    }
                    else
                    {
                        if(firstletter)
                        {
                            //Beim ersten Buchstaben, ist der Start der startpoint des Worts
                            drawObject.ObjectStart = startPoint;

                            firstletter = false;
                        }
                        else
                        {
                            //dannach ist der Start das Ende des letzten Objekts
                            drawObject.ObjectStart = ContainingObjects.Last().GetEnd();
                        }
                    }

                    //Endpunkt wird berechnet
                    drawObject.CalculateEnd();

                    drawObject.Redraw();
                }

                //Liste des Buchstabens wird der Liste des gesamten Wortes hinzugefügt
                this.ContainingObjects.AddRange(letterObjectList);
            }

            //Start und Endpunkt wird abgespeichert
            SafeValues();

            //Die Objekte der Gruppe werden aus der DrawObjectList entfernt, da die ganze Gruppe hinzugefügt wird
            //index des ersten Objekts wird gespeichert
            var startIndex = MainWindow.DrawList.IndexOf(ContainingObjects.First());

            //alle Objekte werden entfernt
            MainWindow.DrawList.RemoveRange(startIndex, ContainingObjects.Count);


            //wird zur Liste hinzugefügt
            MainWindow.DrawList.Add(this);
        }


        public override void Edit(EditGroup edit)
        {
            throw new NotImplementedException();
        }

        public override void Edit(EditGroup edit, double value)
        {
            switch (edit)
            {
                //Größe wird bearbeitet
                case EditGroup.Size:
                    {
                        //der Start des ersten Objekts, ist der Start der Gruppe
                        ContainingObjects[0].SetStart(this.GroupStart);

                        //jedes Objekt wird vergrößert
                        for(int i = 0; i < ContainingObjects.Count; i++)
                        {
                            //bearbeiten des Objekts
                            ContainingObjects[i].Edit(DrawObject.EditObject.Size, value);

                            try
                            {
                                //Der Start des nächsten Objekts ist das ende des aktuellen Objekts
                                ContainingObjects[i + 1].SetStart(ContainingObjects[i].GetEnd());
                            }
                            catch { }
                        }

                        //Start und Endpunkt wird neu abgespeichert
                        SafeValues();
                    }
                    break;
                //Winkel wird bearbeitet
                case EditGroup.Angle:
                    {
                        //WinkelDifferenz zum vorherigen Winkel wird abgespeichert
                        var angleDifference = value - Angle;

                        //neuer Winkel wird abgespeichert
                        Angle = value;

                        //Winkel darf 360 nicht überschreiten
                        if (Angle >= 360)
                            Angle -= 360;

                        for (int i = 0; i < ContainingObjects.Count; i++)
                        {
                            //Wenn es sich nicht um das Erste Objekt handelt,
                            if (i != 0)
                                //Startpunkt ist der Enkpunkt des vorigen Objekts
                                ContainingObjects[i].ObjectStart = ContainingObjects[i - 1].GetEnd();

                            //neuer Endpunkt wird berechnet
                            ContainingObjects[i].CalculateEnd();

                            //Objekt wird erneut gezeichnet
                            ContainingObjects[i].Redraw();

                            //Die Winkeldifferenz wird dem Winkel des Objekts hinzugerechnet
                            ContainingObjects[i].Rotate(ContainingObjects[i].Angle + angleDifference);                            
                        }
                    }
                    break;
            }
        }

        public override string[] GetInformationString()
        {
            //beinhaltet alle informationStrings
            //für jedes Objekt in ContainingObjects, gibt es einen String
            var informationStrings = new string[ContainingObjects.Count];

            //alle Strings werden abgespeichert
            for (int i = 0; i < ContainingObjects.Count; i++)
            {
                informationStrings[i] = ContainingObjects[i].GetInformationString()[0];
            }

            //informationStrings wird zurückgegeben
            return informationStrings;
        }

        public override void Redraw()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start/Endpunkt wird abgespeichert
        /// </summary>
        private void SafeValues()
        {
            //Start der Gruppe, ist Start des ersten Objekts
            this.GroupStart = ContainingObjects.First().GetStart();

            //Ende der Gruppe, ist Ende des letzten Objekts
            this.GroupEnd = ContainingObjects.Last().GetEnd();
        }
    }
}
