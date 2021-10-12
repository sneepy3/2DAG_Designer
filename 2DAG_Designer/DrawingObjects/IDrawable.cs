using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _2DAG_Designer.DrawingObjects
{
    public interface IDrawable
    {
        //Werte
        double Angle { get; }
        double Width { get; }
        double Height { get; }

        /// <summary>
        /// erneut zeichnen
        /// </summary>
        void Redraw();

        /// <summary>
        /// entfernen aus dem DrawField
        /// </summary>
        void Remove();

        /// <summary>
        /// Objekt wird dem DrawField hinzugefügt
        /// </summary>
        void AddToCanvas();

        #region Getter/Setter

        /// <summary>
        /// Gibt Informationen des Objekts in einem String zurück (für abspeichern in txt Datei)
        /// </summary>
        string[] GetInformationString();

        /// <summary>
        /// Startpunkt wird zurückgegeben
        /// </summary>
        Point GetStart();
        
        /// <summary>
        /// Endpunkt wird zurückgegeben
        /// </summary>
        Point GetEnd();

        /// <summary>
        /// Startpunkt wird festgelegt
        /// </summary>
        void SetStart(Point start);

        /// <summary>
        /// Endpunkt wird festgelegt
        /// </summary>
        void SetEnd(Point end);

        #endregion
    };
}
