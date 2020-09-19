using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2DAG_Designer;
using _2DAG_Designer.FileIO;
using _2DAG_Designer.DrawingObjects;
using System.CodeDom;
using System.Data.SqlClient;

namespace _2DAG_Designer.DrawingObjects.Edit
{
    static class Edit
    {
        /// <summary>
        /// Möglichkeiten ein gezeichnetes Objekt zu verändern
        /// </summary>
        public enum EditType
        {
            EndUp,
            EndDown,
            EndRight,
            EndLeft,
            Invert,
            Angle,
            Size,
        }

        //Listen für undo/redo
        public static List<double> SizeChangedFactors         = new List<double>();
        public static List<double> ReversedSizeChangedFactors = new List<double>();

        /// <summary>
        /// Verbgrößert/Verkleinert alle gezeichneten Objekte mit dem angegebenen Faktor
        /// </summary>
        /// <param name="factor"></param>
        public static void EditObjectSize(double factor)
        {         
            //Prozentzahl in Kommazahl
            factor /= 100;
            
            //jedes Objekt in der Liste wird bearbeitet
            for (int i = 0; i < MainWindow.DrawList.Count; i++)
            {
                EditDraw(MainWindow.DrawList[i], EditType.Size, factor);

                try
                {
                    //Der Start des nächsten Objekts ist das ende des aktuellen Objekts
                    MainWindow.DrawList[i + 1].SetStart(MainWindow.DrawList[i].GetEnd());
                }
                catch
                { }
            }


            //für undo und redo
            if (MainWindow.Undooing)
            {
                ReversedSizeChangedFactors.Add(factor);

                SizeChangedFactors.Remove(SizeChangedFactors.Last());
            }         
            else if(MainWindow.Redooing)
            {
                SizeChangedFactors.Add(factor);


                ReversedSizeChangedFactors.Remove(ReversedSizeChangedFactors.Last());
            }
            else
            {
                SizeChangedFactors.Add(factor);

                //Aktion wird der Aktionsliste hinzugefügt
                MainWindow.ActionList.Add(MainWindow.ActionType.EditSize);
            }
        }

        //Bearbeiten eines einzelnen IDrawable
        public static void EditDraw(IDrawable obj, EditType edit)
        {
            try
            {
                //EditType wird als string abgespeichert
                var editString = edit.ToString();

                if(obj.GetType().BaseType == typeof(DrawObject))
                {
                    //drwaObject ist obj als DrawObject, so kann auf die DrawObject Funktionen zugegriffen werden
                    var drawObject = (DrawObject)obj;

                    //objekt wird bearbeitet
                    drawObject.Edit((DrawObject.EditObject)Enum.Parse(typeof(DrawObject.EditObject), editString));
                }
                else if(obj.GetType().BaseType == typeof(DrawGroup))
                {
                    var drawGroup = (DrawGroup)obj;

                    //objekt wird bearbeitet
                    drawGroup.Edit((DrawGroup.EditGroup)Enum.Parse(typeof(DrawGroup.EditGroup), editString));
                }
            }
            catch { }
        }
        public static void EditDraw (IDrawable obj, EditType edit, double value)
        {
            try
            {
                //EditType wird als string abgespeichert
                var editString = edit.ToString();

                if (obj.GetType().BaseType == typeof(DrawObject))
                {
                    //drwaObject ist obj als DrawObject, so kann auf die DrawObject Funktionen zugegriffen werden
                    var drawObject = (DrawObject)obj;

                    //objekt wird bearbeitet
                    drawObject.Edit((DrawObject.EditObject)Enum.Parse(typeof(DrawObject.EditObject), editString), value);
                }
                else if (obj.GetType().BaseType == typeof(DrawGroup))
                {
                    var drawGroup = (DrawGroup)obj;

                    //objekt wird bearbeitet
                    drawGroup.Edit((DrawGroup.EditGroup)Enum.Parse(typeof(DrawGroup.EditGroup), editString), value);
                }
            }
            catch { }
        }
    

    }
}
