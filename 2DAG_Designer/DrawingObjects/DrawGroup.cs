using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Windows.Media.TextFormatting;

namespace _2DAG_Designer.DrawingObjects
{

    /// <summary>
    /// Eine DrawGroup sind mehrere DrawObjects zusammengefasst
    /// </summary>
    public abstract class DrawGroup : IDrawable
    {
        //Breite/Höhe
        public double Width { get; protected set; } = 0;
        public double Height { get; protected set; } = 0;

        //Möglichkeiten der Veränderung
        public enum EditGroup 
        {
            Size,
            Angle,
        }

        //Startpunkt der Gruppe
        public Point GroupStart;

        //Endpunkt der gesamten Gruppe
        public Point GroupEnd;

        //Winkel
        public double Angle { get; protected set; } = 0;

        //Liste aller DrawObjects in der DrawGroup
        public List<DrawObject> ContainingObjects = new List<DrawObject>();


        /// <summary>
        /// Bearbeiten der Gruppe
        /// </summary>
        /// <param name="edit">Art der Bearbeitung</param>
        public abstract void Edit(EditGroup edit);

        /// <summary>
        /// Bearbeiten mit Faktor
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="factor"></param>
        public abstract void Edit(EditGroup edit, double factor);

        /// <summary>
        /// Gruppe erneut zeichnen
        /// </summary>
        public abstract void Redraw();

        public void Remove()
        {
            //jedes Objekt wird entfernt
            foreach (var drawObject in ContainingObjects)
            {
                drawObject.Remove();
            }
        }

        #region Getter/Setter

        public Point GetEnd()
        {
            return this.GroupEnd;
        }

        public Point GetStart()
        {
            return this.GroupStart;
        }

        public void SetStart(Point start)
        {
            GroupStart = start;
        }

        public void SetEnd(Point end)
        {
            GroupEnd = end;
        }

        public void AddToCanvas()
        {
            //jedes Objekt wird hinzugefügt
            foreach (var obj in ContainingObjects)
            {
                obj.AddToCanvas();
            }
        }

        public abstract string[] GetInformationString();

        public static IDrawable GetObjectFromString(string data)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
