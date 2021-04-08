#include "Schrittmotor.h"
#include "DrawObjects/DrawObject.h"
#include "Arduino.h"


/*#region -------für serielle Kommunikation */

//Eingegangene Nachricht
String inputMessage = "";

//Eingegangener Befehl
String command = "";

//Eingegangene Information
String information = "";

//true wenn die  Nachricht komplett
bool messageComplete = false;




/*#endregion*/

/*#region -------für das Einbrennen */

/*#region Schrittmotoren */

Schrittmotor MotorX(12, 11, 10, 9, 2000, Schrittmotor::StepMode::TWOPHASE);
Schrittmotor MotorY(0, 1, 17, 16, 2000, Schrittmotor::StepMode::TWOPHASE);

//Pause zwischen den Schrittmotor Schritten
const int Break = 70;

/*#endregion*/

//wird true, wenn das Gravieren gestartet wird
bool startEngraving = false;

//gibt die Distanz in x und y Richtung an, die durch einen Schritt zurückgelegt wird
const double distancePerTurnX = 0.082;
const double distancePerTurnY = 0.108;

//Pause im engrave while loop
int engraveBreak = 50;

//Anzahl der zu gravierenden Objekte
int objectCount;

//Liste aller zu zeichnenden Objekte
DrawObject* objects;

//Nummer des aktuellen Objekts
//für den Abspeicherungs- und Brennprozess
int currentObj = 0;

//globale Position wird bei jedem Linienende aktualisiert
double globalPosX = 0, globalPosY = 0;

/*#endregion*/


void setup()
{
  Serial.begin(9600);

  pinMode(22, INPUT);
}

void loop()
{
    serialHandler();

    if(messageComplete)
    {
        handleMessage();
        messageComplete = false;


        //Wenn ein neuer Befehl eingegangen ist
        if(!command.equals(""))
        {

            //Gravieren wird gestartet
            if(command.startsWith("ENGRAVE"))
            {
                    //Vorgang wird gestartet
                    startEngraving = true;

                    //OK befehl
                    Serial.println("!OK");
            }

            if(command.startsWith("START"))
            {
                    //Wenn eingraviert werden soll
                    if(startEngraving)
                    {
                        startEngraving = false;
                            
                        //OK befehl
                        Serial.println("!OK");


                        //Brennprozess wird gestartet
                        engrave();
                    }
            }

            //Command wird gelöscht, da er ausgeführt wurde
            command = "";
        }

        //Wenn eine neue Information eingagangen ist
        if(!information.equals(""))
        {
            //Wenn gerade eingraviert wird
            if(startEngraving)
            {
                //Wenn eine Objektinformation erhalten wird
                if(information.startsWith("OBJ"))
                {
                    //Anfang wird entfernt
                    information.remove(0, 3);
                    
                    // Wenn es sich bei dem Objekt um eine Linie handelt
                    if(information.startsWith("L"))
                    {
                        // L wird entfernt
                        information.remove(0, 1);

                        //die beiden Informationen werden mit / abgetrennt
                        int splitIndex = information.indexOf('/');

                        //Es wird eine neue Linie mit den erhaltenen Informationen erstellt
                        DrawObject newLine(
                            //Breite
                            information.substring(0, splitIndex).toDouble(),
                            //Höhe
                            information.substring(splitIndex + 1).toDouble());

                        objects[currentObj] = newLine;

                        //zum nächsten Objekt
                        currentObj++;

                        //OK Befehl wird gesendet
                        Serial.println("!OK");
                    }  
                    // Wenn es sich um einen Kreis handelt
                    else
                    {
                        // C wird entfernt
                        information.remove(0, 1);

                        // Informationen werden mit / abgetrennt
                        int firstSplitIndex = information.indexOf('/');
                        int secondSplitIndex = information.indexOf('/', firstSplitIndex + 1);
                        int thirdSplitIndex = information.indexOf('/', secondSplitIndex + 1);

                        // Werte werden abgespeichert
                        double radius = information.substring(0, firstSplitIndex).toDouble();
                        double circleSizeAngle = information.substring(firstSplitIndex + 1, secondSplitIndex).toDouble();
                        double firstAngle = information.substring(secondSplitIndex + 1, thirdSplitIndex).toDouble();
                        bool isInverted = (bool)information.substring(thirdSplitIndex + 1).toInt();

                        //Serial.println(isInverted);

                        // neuer Kreis wird erstellt
                        DrawObject newCircle(
                            // Radius
                            radius,
                            // CircleSizeAngle
                            circleSizeAngle,
                            // firstAngle
                            firstAngle,
                            // Invertierung
                            isInverted);

                        objects[currentObj] = newCircle;

                        // zum nächsten Objekt
                        currentObj++;

                        // OK Befehl wird gesendet
                        Serial.println("!OK");
                    }

                    //Wenn alle Objekte empfangen wurden, wird der Brennvorgang gestartet
                    // if(currentObj >= objectCount )
                    //   engrave();
                }
                else if(information.startsWith("COUNT"))
                {
                    //aktuelles Objekt auf 0
                    currentObj = 0;

                    //Anfang wird entfernt
                    information.remove(0, 5);

                    // Aktuelles auf 0
                    currentObj = 0;


                    //Anzahl der Objekte wird abgespeichert
                    objectCount = information.toInt();
                    
                    //Wenn bereits Linien gespeichert wurden, werden sie gelöscht
                    if(objects) delete[] objects;

                    //lines ist ein array
                    //Anzahl der Linien, die im array gespeichert werden müssen,
                    //wird hier festgelegt
                    objects = new DrawObject[objectCount];

                    //OK Befehl wird gesendet
                    Serial.println("!OK");
                }

            }

            //information wird zurückgesetzt
            information = "";
        }
    }


    if(digitalRead(22))
    {
        // for(int i = 0; i < objectCount; i++)
        // {
        //   Serial.println(i);
        
    

        //   Serial.print("X: ");
        //   Serial.println(lines[i].Width);
        
        //   Serial.print("Y: " );
        //   Serial.println(lines[i].Height);
        // }


        //Vorbereitung beendet
        startEngraving = false;

        //Brennvorgang wird gestartet
        engrave();
    } 
}

/**
 * empfängt serialle Nachrichten
 */
void serialHandler()
{
    while(Serial.available())
    {
            char input = (char)Serial.read();

            if (input == '\n') {
            messageComplete = true;
            }
            else
            inputMessage += input;
    }
}

/**
 * behandelt eingegangene Nachrichten
 */
void handleMessage()
{
    if(inputMessage.length()>0)
    {
        //Präfix der Nachricht
        switch(inputMessage[0])
        {
            //Bei Befehlen
            case('!'):
            {
                //Befehl ist die eingegangene Nachricht, ohne den Präfix
                inputMessage.remove(0,1);

                command = inputMessage;
            }

            //Bei Informationen
            case('#'):
            {
                //Befehl ist die eingegangene Nachricht, ohne den Präfix
                inputMessage.remove(0, 1);

                information = inputMessage;
            }break;
        }

        //inputMessage wird zurückgesetzt
        inputMessage = "";
    }
}

void engrave()
{  
    /*#region Vorbereitung*/

    //theoretische Position des Brenners
    //Beginn bei globaler Position
    double posX = globalPosX;
    double posY = globalPosY;

    //tatsächliche Position des Brenners, wird bei Schrittmotorbewegungen verändert
    //Beginn bei globaler Position
    double actualPosX = globalPosX;
    double actualPosY = globalPosY;

    /*#region Kreiszeichnung*/

    // wird true, wenn ein Kreis gezeichnet wird
    bool drawingCircle = false;

    double anglePerVertex = 0;

    double currentAngle;

    /*#endregion */


    //aktuelle Objektnummer
    currentObj = 0;

    // Linie die gerade eingraviert wird
    DrawObject currentLine;

    //Bewegung in X und Y Richtung
    double xMovement, yMovement;

    // Wenn es sich um eine Linie handelt
    if(objects[0].isLine)
    {   
        //Linie wird abgespeichert
        currentLine = objects[0];

        //die ganze Strecke wird in kleine Bruchteile auftegeilt
        //bei jedem Durchlauf wird x und yMovement der Position hinzugerechnet
        xMovement = currentLine.Width /  (currentLine.getLength() * 20 );
        yMovement = currentLine.Height / (currentLine.getLength() * 20 );

    }
    // Wenn es sich um einen Kreis handelt
    else
    {
        drawingCircle = true;

        //Kreis wird abgespeichert
        DrawObject circle = objects[0];

        // Kreis wird als Vieleck gezeichnet
        // Winkel pro Ecke
        anglePerVertex = circle.CircleSizeAngle / (circle.getLength() * 10);

        // es wird mit dem ersten Winkel des Kreises begonnen
        currentAngle = circle.FirstAngle;

        if(circle.IsInverted)
        {
            // Bewegung gegen den Uhrzeigersinn
            anglePerVertex = -anglePerVertex;
        }

        //Kreis wird in ein Vieleck aufgeteilt
        //Linie zum ersten Eckpunkt wird berechnet
        DrawObject newLine = circle.CalculateLine(currentAngle, currentAngle + anglePerVertex);

        currentLine.Width = newLine.Width;
        currentLine.Height = newLine.Height;

        //Variablen für die Positionsberechnung
        xMovement = currentLine.Width /  (currentLine.getLength() * 20 );
        yMovement = currentLine.Height / (currentLine.getLength() * 20 );
    }


    //wird false, wenn der Vorgang beendet ist
    bool engraving = true;

    /*#endregion*/


    //Durchlauf, bis der Brennprozess beendet ist
    while(engraving)
    {
        //Wenn ein Schritt in X Richtung ausgeführt werden soll
        if(stepX(currentLine, actualPosX, posX))
        {
            // Wenn die Distanz positiv ist
            if(currentLine.Width > 0)
            {
                //Schritt in X Richtung wird ausgeführt
                MotorX.StepBackward(1, Break);

                //die Schrittdistanz wird zur tatsächlichen Position hinzugefügt
                actualPosX += distancePerTurnX;
            }
            //bei negativer Distanz
            else
            {
                //Rückwärtsschritt in X Richtung
                MotorX.StepForward(1, Break);

                //die Schrittdistanz wird zur tatsächlichen Position hinzugefügt
                actualPosX -= distancePerTurnX;
            }
        }

        //Wenn ein Schritt in Y Richtung ausgeführt werden soll
        if(stepY(currentLine, actualPosY, posY))
        {
            // Wenn die Distanz positiv ist
            if(currentLine.Height > 0)
            {
                //Schritt in Y Richtung wird ausgeführt
                MotorY.StepForward(1, Break);

                //die Schrittdistanz wird zur tatsächlichen Position hinzugefügt
                actualPosY += distancePerTurnY;
            }
            //bei negativer Distanz
            else
            {
                //Rückwärtsschritt in Y Richtung wird ausgeführt
                MotorY.StepBackward(1, Break);

                //die Schrittdistanz wird zur tatsächlichen Position hinzugefügt
                actualPosY -= distancePerTurnY;
            }
        }

        /*#region Linien Durchlauf (Berechnung der theoretischen Position)*/

        //Bewegung in Richtung der Linie
        posX += xMovement;
        posY += yMovement;

        //wird true, wenn die Linie fertig eingebrannt ist
        bool lineFinished = false;

        /*#region Überprüfung, ob Linie beendet ist */    

        // wenn die Breite größer als 0 ist
        if(currentLine.Width > 0)
        {
            //wenn das Ende der Linie erreicht wurde
            if((globalPosX + currentLine.Width) <= posX)
                lineFinished = true;
        }
        // wenn die Breite kleiner als 0 ist
        else if(currentLine.Width < 0)
        {
            //wenn das Ende der Linie erreicht wurde
            if((globalPosX + currentLine.Width) >= posX)
                lineFinished = true;
        }
        //bei Linien mit Breite = 0
        else
        {
            //bei positiver Höhe
            if(currentLine.Height > 0)
            {
            //wenn das Ende der Linie erreicht wurde
            if((globalPosY + currentLine.Height) <= posY)
                lineFinished = true;
            }
            //bei negativer Höhe
            else
            {
            //wenn das Ende der Linie erreicht wurde
            if((globalPosY + currentLine.Height) >= posY)
                lineFinished = true;
            }
        }

        /*#endregion */

        //Wenn die Linie fertig ist
        if(lineFinished)
        {
            //gibt an, ob das Nächste Objekt gebrannt werden soll
            bool drawNextObject = true;

            if(drawingCircle)
            {
                //Kreis wird abgespeichert
                DrawObject circle = objects[currentObj];
                
                // neuer Winkel wird berechnet
                currentAngle += anglePerVertex;

                // wird true, wenn Kreis noch nicht beendet ist
                bool circleNotFinished;

                // Kreis gegen Uhrzeigersinn
                if(circle.IsInverted)
                {
                    circleNotFinished = currentAngle > (circle.FirstAngle + 0.0001 - circle.CircleSizeAngle);
                }
                // Kreis mit Uhrzeigersinn
                else
                {
                    circleNotFinished = currentAngle < (circle.FirstAngle - 0.0001 + circle.CircleSizeAngle);
                }

                if(circleNotFinished)
                {
                    //globale Position wird aktualisiert
                    globalPosX += currentLine.Width;
                    globalPosY += currentLine.Height;

                    // nächstes Objekt soll nicht gezeichnet werden, da der Kreis noch nicht beendet ist
                    drawNextObject = false;

                    //Linie zum nächsten Eckpunkt wird berechnet
                    DrawObject newLine = circle.CalculateLine(currentAngle, currentAngle + anglePerVertex);

                    currentLine.Width = newLine.Width;
                    currentLine.Height = newLine.Height;

                    //Variablen für die Positionsberechnung werden aktualisiert
                    xMovement = currentLine.Width /  (currentLine.getLength() * 20 );
                    yMovement = currentLine.Height / (currentLine.getLength() * 20 );
                }
            }

            // Wenn das nächste Objekt gezeichnet werden soll
            if(drawNextObject)
            {
                //Fortschritt in % wird abgespeichert
                int progress = (int)(((double)(currentObj + 1) / (double)objectCount) * 100);

                //Fortschritt wird gesendet
                Serial.println("PROG" + String(progress));

                //globale Position wird aktualisiert
                globalPosX += currentLine.Width;
                globalPosY += currentLine.Height;

                //zur nächsten Linie
                currentObj++;

                //wenn das Objekt existiert
                if(objectCount - 1  >= currentObj)
                {
                    // Wenn es sich um eine Linie handelt
                    if(objects[currentObj].isLine)
                    {
                        drawingCircle = false;

                        // Aktuelles Objek wird zu einer Linie konvertiert und abgespeichert
                        currentLine = objects[currentObj];                            

                        //die ganze Strecke wird in kleine Bruchteile auftegeilt
                        //bei jedem Durchlauf wird x und yMovement der Position hinzugerechnet
                        xMovement = currentLine.Width /  (currentLine.getLength() * 20 );
                        yMovement = currentLine.Height / (currentLine.getLength() * 20 );
                    }
                    // Wenn es sich um einen Kreis handelt
                    else
                    {
                        drawingCircle = true;

                        //Kreis wird abgespeichert
                        DrawObject circle = objects[currentObj];

                        // Kreis wird als Vieleck gezeichnet
                        // Winkel pro Ecke
                        anglePerVertex = circle.CircleSizeAngle / (circle.getLength() * 10);

                        currentAngle = circle.FirstAngle;

                        if(circle.IsInverted)
                        {
                            // Bewegung gegen den Uhrzeigersinn
                            anglePerVertex = -anglePerVertex;
                        }

                        //Kreis wird in ein Vieleck aufgeteilt
                        //Linie zum ersten Eckpunkt wird berechnet
                        DrawObject newLine = circle.CalculateLine(currentAngle, currentAngle + anglePerVertex);

                        currentLine.Width = newLine.Width;
                        currentLine.Height = newLine.Height;

                        //Variablen für die Positionsberechnung
                        xMovement = currentLine.Width /  (currentLine.getLength() * 20 );
                        yMovement = currentLine.Height / (currentLine.getLength() * 20 );
                    }          
                }
                else
                {
                    //Brennvorgang wird beendet
                    engraving = false;

                    //Variablen werden zurückgesetzt
                    globalPosX = 0;
                    globalPosY = 0;
                }
            }
        }

        /*#endregion*/

        //Wartezeit
        delay(engraveBreak);
    }
}


/**
 * gibt zurück ob ein Schritt in X Richtung ausgeführt werden soll
 * @param  actualPosX tatsächliche Position des Brenners
 */
bool stepX(DrawObject line, double actualPosX, double posX)
{
    // wenn die Distanz 0 ist, soll kein Schritt ausgeführt werden
    if(line.Width == 0)
        return false;

    // Wenn die Distanz positiv ist
    if(line.Width > 0)
    {
        //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
        if((actualPosX + distancePerTurnX) <= posX)
        return true;
    }
    //bei negativer Distanz
    else
    {
        //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
        if((actualPosX - distancePerTurnX) >= posX)
        return true;
    }

    return false;

}

/**
 * gibt zurück ob ein Schritt in Y Richtung ausgeführt werden soll
 * @param  actualPosY tatsächliche Position der Brenners
 */
bool stepY(DrawObject line, double actualPosY, double posY)
{
    // wenn die Distanz 0 ist, soll kein Schritt ausgeführt werden
    if(line.Height == 0)
        return false;

    // Wenn die Distanz positiv ist
    if(line.Height > 0)
    {
        //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
        if((actualPosY + distancePerTurnY) <= posY)
        return true;
    }
    else
    {
        //Wenn die Position die Distanz erreicht, soll ein Schritt ausgeführt werden
        if((actualPosY - distancePerTurnY) >= posY)
        return true;
    }

    return false;

}
