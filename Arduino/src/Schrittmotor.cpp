#include "Schrittmotor.h"
#include <Arduino.h>

Schrittmotor::Schrittmotor( unsigned int sp1A,
                            unsigned int sp2B,
                            unsigned int sp1B,
                            unsigned int sp2A,
                            unsigned int stepsPerTurn, StepMode mode)
{
  //Anschlusspins werden festgelegt
  ports[0] = sp1A;
  ports[1] = sp2B;
  ports[2] = sp1B;
  ports[3] = sp2A;

  //jeder port in ports wird als OUTPUT festgelegt
  for(int port : ports)
  {
    pinMode(port, OUTPUT);
  }

  //Benötigte Anzahl an Durchläufen für eine Umdrehung wird festgelegt
  runsPerTurn_ = stepsPerTurn / 4; // (Ein Durchlauf beinhaltet 4 Schritte)

  //Betriebsmodus wird festgelegt
  mode_ = mode;
}

void Schrittmotor::SetMode(StepMode mode)
{
  //Betriebsmodus wird festgelegt
  mode_ = mode;
}

void Schrittmotor::StepForward(int Schritte, unsigned int stepBreak )
{
  //In einem Durchlauf werden immer 4 Schritte getätigt()
  for (; Schritte > 0; Schritte--)
  {
    //Schritte werden je nach Betriebsmodus ausgeführt
    switch (mode_)
    {
      case (StepMode::ONEPHASE):
      {
          for(int index = 0; index < 4; index++)
          {
            Off();// Alle Ports auf LOW
            digitalWrite(ports[index], HIGH);// Port auf HIGH
            delay(stepBreak);//Pause zwischen Schritt
          }
      }break;

      case (StepMode::TWOPHASE):
      {
          for(int index = 0; index < 4; index++)
          {
            Off();// Alle Ports auf LOW
            digitalWrite(ports[index], HIGH);// Port auf HIGH

            //Im Zweiphasenbetrieb müssen immer 2 Ports auf HIGH sein
            if(index == 3) // falls der letzte Port bereits HIGH ist,
            {
              digitalWrite(ports[0], HIGH); //muss der erste auch HIGH sein
            }
            else
            {
              digitalWrite(ports[index + 1], HIGH); // sonst wir der folgende Port HIGH
            }
            delay(stepBreak);//Pause zwischen Schritt
          }
      }break;

      case (StepMode::HALFSTEP):
      {
        for( int index = 0; index < 4; index++)
        {
          //Normaler Schritt
          Off();// Alle Ports auf LOW
          digitalWrite(ports[index], HIGH);// Port auf HIGH
          delay(stepBreak);//Pause zwischen Schritt


          //zusätzlicher Halbschritt
          if(index == 3) // falls der letzte Port bereits HIGH ist,
          {
            digitalWrite(ports[0], HIGH); //muss der erste auch HIGH sein
          }
          else
          {
            digitalWrite(ports[index + 1], HIGH); // sonst wir der folgende Port HIGH
          }
          delay(stepBreak);//Pause zwischen Schritt

        }
      }break;
    }
  }
}

void Schrittmotor::StepBackward(int Schritte, unsigned int stepBreak )
{
  //In einem Durchlauf werden immer 4 Schritte getätigt()
  for (; Schritte > 0; Schritte--)
  {
    switch (mode_)
    {
      case (StepMode::ONEPHASE):
      {
          for(int index = 3; index >= 0; index--)
          {
            Off();// Alle Ports auf LOW
            digitalWrite(ports[index], HIGH);// Port auf HIGH
            delay(stepBreak);//Pause zwischen Schritt
          }
      }break;

      case (StepMode::TWOPHASE):
      {
          for(int index = 3; index >= 0; index--)
          {
            Off();// Alle Ports auf LOW
            digitalWrite(ports[index], HIGH);// Port auf HIGH

            //Im Zweiphasenbetrieb müssen immer 2 Ports auf HIGH sein
            if(index == 0) // falls der letzte Port bereits HIGH ist,
            {
              digitalWrite(ports[3], HIGH); //muss der erste auch HIGH sein
            }
            else
            {
              digitalWrite(ports[index - 1], HIGH); // sonst wir der folgende Port HIGH
            }
            delay(stepBreak);//Pause zwischen Schritt
          }
      }break;

      case (StepMode::HALFSTEP):
      {
        for( int index = 3; index  >= 0; index--)
        {
          //Normaler Schritt
          Off();// Alle Ports auf LOW
          digitalWrite(ports[index], HIGH);// Port auf HIGH
          delay(stepBreak);//Pause zwischen Schritt


          //zusätzlicher Halbschritt
          if(index == 0) // falls der letzte Port bereits HIGH ist,
          {
            digitalWrite(ports[3], HIGH); //muss der erste auch HIGH sein
          }
          else
          {
            digitalWrite(ports[index - 1], HIGH); // sonst wir der folgende Port HIGH
          }
          delay(stepBreak);//Pause zwischen Schritt

        }
      }break;
    }
    }
}

void Schrittmotor::TurnForward(double turns, unsigned int stepBreak)
{
  //der Motor macht so lange ganze Umdrehungen, bis turns kleiner als 1 ist
  //es bleibt z.B. 0,6 übrig
  for(;turns > 1; turns--)
  {
    StepForward(runsPerTurn_, stepBreak);
  }

  //übriger Rundenbruchteil wird ausgeführt
  StepForward((int)(runsPerTurn_ * turns), stepBreak);
}

void Schrittmotor::TurnBackward(double turns, unsigned int stepBreak)
{
  //der Motor macht so lange ganze Umdrehungen, bis turns kleiner als 1 ist
  //es bleibt z.B. 0,6 übrig
  for(;turns > 0; turns--)
  {
    StepBackward(runsPerTurn_, stepBreak);
  }

  //übriger Rundenbruchteil wird ausgeführt
  StepBackward((int)(runsPerTurn_ * turns), stepBreak);
}

void Schrittmotor::Off()
{
  //jeder Port wird auf LOW gesetzt
  for(int port : ports)
  {
    digitalWrite(port, LOW);
  }
}

int Schrittmotor::StepForwardMillis (unsigned int stepBreak,
  unsigned long startTime, int progress )
{
  bool progressChanged = false;

//wenn ein neuer Schritt kommen muss,
  if((millis() - startTime) >= stepBreak)
  {
    //Fortschritt + 1
    progress++;

    //Fortschritt wurde geändert
    progressChanged = true;
  }

//Änderungen der Ports müssen nur gemacht werden, bei erstmaliger Aufrufung oder beim
//ändern des Fortschritts
  if(progressChanged || progress == 0)
  {
    //Schritte für jeden Betriebsmodus
    switch(mode_)
    {
      case(StepMode::ONEPHASE):
      {
        switch(progress)
        {
          case(0):
          {
            digitalWrite(ports[0], HIGH);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], LOW);
          }break;

          case(1):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], HIGH);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], LOW);
          }break;

          case(2):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], HIGH);
            digitalWrite(ports[3], LOW);
          }break;

          case(3):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], HIGH);
          }break;

          case(4):
          {
            //beenden
            return -1;
          }break;
        }
      }break;

      case(StepMode::TWOPHASE):
      {
        switch(progress)
        {
          case(0):
          {
            digitalWrite(ports[0], HIGH);
            digitalWrite(ports[1], HIGH);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], LOW);
          }break;

          case(1):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], HIGH);
            digitalWrite(ports[2], HIGH);
            digitalWrite(ports[3], LOW);
          }break;

          case(2):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], HIGH);
            digitalWrite(ports[3], HIGH);
          }break;

          case(3):
          {
            digitalWrite(ports[0], HIGH);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], HIGH);
          }break;

          case(4):
          {
            //beenden
            return -1;
          }break;
        }
      }break;

      case(StepMode::HALFSTEP):
      {
        switch(progress)
        {
          case(0):
          {
            digitalWrite(ports[0], HIGH);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], LOW);
          }break;

          case(1):
          {
            digitalWrite(ports[0], HIGH);
            digitalWrite(ports[1], HIGH);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], LOW);
          }break;

          case(2):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], HIGH);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], LOW);
          }break;

          case(3):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], HIGH);
            digitalWrite(ports[2], HIGH);
            digitalWrite(ports[3], LOW);
          }break;

          case(4):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], HIGH);
            digitalWrite(ports[3], LOW);
          }break;

          case(5):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], HIGH);
            digitalWrite(ports[3], HIGH);
          }break;

          case(6):
          {
            digitalWrite(ports[0], LOW);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], HIGH);
          }break;

          case(7):
          {
            digitalWrite(ports[0], HIGH);
            digitalWrite(ports[1], LOW);
            digitalWrite(ports[2], LOW);
            digitalWrite(ports[3], HIGH);
          }break;

          case(9):
          {
            //beenden
            return -1;
          }break;
        }
      }break;
    }
  }

//Fortschritt wird zurückgegeben
  return progress;
}
