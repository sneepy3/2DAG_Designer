#include "DrawObject.h"
#include "Arduino.h"

// Constructor Linien
DrawObject::DrawObject(double width, double height)
{
    // es handelt sich um eine Linie
    isLine = true;

    // Werte für die Linie werden abgespeichert
    Width = width;
    Height = height;
}

// Constructor Kreise
DrawObject::DrawObject(double radius, double cireSizeAngle, double firstAngle, bool inverted)
{
    // es handelt sich um einen Kreis
    isLine = false;


    // Parameter werden abgespeichert
    Radius = radius;
    CircleSizeAngle = cireSizeAngle;
    FirstAngle = firstAngle;
    IsInverted = inverted;
}

DrawObject::DrawObject()
{
    
}

DrawObject DrawObject::CalculateLine(double lastAngle, double newAngle)
{
    double X1 = cos(lastAngle * (M_PI / 180.0)) * Radius;
    double Y1 = sin(lastAngle * (M_PI / 180.0)) * Radius;
    
    double X2 = cos(newAngle * (M_PI / 180.0)) * Radius;
    double Y2 = sin(newAngle * (M_PI / 180.0)) * Radius;

    double width = X2 - X1;
    double height = Y2 - Y1;

    // Serial.print(width);
    // Serial.print(" ");
    // Serial.println(height);

    DrawObject line = DrawObject(width, height);

    return line;
}


double DrawObject::getLength()
{
    // Wenn es sich um eine Linie handelt
    if(isLine)
    {
        // Entfernung zwischen Start und Ende wird zurückgegeben
        return sqrt(pow(Width, 2) + pow(Height, 2));
    }
    // bei Kreisen
    else
    {
        double length = (Radius *  M_PI * CircleSizeAngle) / 180;

        // die Länge des Kreises wird aus dem Radius und dem Kreisanteil berechnet
        return length;
    }
}