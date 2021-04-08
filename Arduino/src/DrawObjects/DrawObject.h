#ifndef DrawObject_H
#define DrawObject_H

class DrawObject
{
public:

    /**
     *  Gibt an, ob es sich um eine Linie handelt
     * */
    bool isLine;

    /* #region Linie */

    //Lange der Linie in X und Y Richtung
    double Width, Height;

    /**
     * Constructor für Linien
     */
    DrawObject(double width, double height);

    /* #endregion */

    /* #region Kreis */

    // gibt an, ob der Kreis invertiert ist (gegen den Uhrzeigersinn verläuft)
    bool IsInverted;
    
    // Radius des Kreises
    double Radius;

    // Winkel, der den Anteil des Krieses angibt (90° => 1/4 Kreis)
    double CircleSizeAngle;

    // Winkel mit dem begonnen wird
    double FirstAngle;

    /**
     * Constructor für Kreise
     */
    DrawObject(double radius, double cireSizeAngle, double firstAngle, bool inverted);

    /** 
     * Berechnen einer Linie von einem Punkt auf dem Kreis zu einem anderen
     * anhand von 2 Winkeln vom Mittelpunkt aus
     */
    DrawObject CalculateLine(double lastAngle, double newAngle);

    /* #endregion */

    /**
     * Standardkonstruktor
     */
    DrawObject();

    /**
     * Länge des Objekts berechnen
     */
    double getLength();

};

#endif
