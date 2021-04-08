#ifndef Schrittmotor_H
#define Schrittmotor_H

//zum Abspeichern von Informationen zur Bewegung des Schrittmotors
class Move
{
public:

  //Arten der Bewegung für einen Schrittmotor
  enum MoveType {STEPFORWARD, STEPBACKWARD, TURNFORWARD, TURNBACKWARD};

//Anzahl der Wiederholungen
  int Count;

//Pause zwischen den Schritten
  int StepBreak;

//true wenn Bewegung beendet ist1
  bool Finished;
};

class Schrittmotor
{
public:

  //Betriebsmodus
  enum StepMode {ONEPHASE, TWOPHASE, HALFSTEP};


/**
 * Constructor
 * @param sp1A         [Spule 1, Anschluss A]
 * @param sp2B         [Spule 2, Anschluss B]
 * @param sp1B         [Spule 1, Anschluss B]
 * @param sp2A         [Spule 2, Anschluss A]
 * @param stepsPerTurn [Schritte pro Umdrehung]
 */
  Schrittmotor( unsigned int sp1A,
                unsigned int sp2B,
                unsigned int sp1B,
                unsigned int sp2A,
                unsigned int stepsPerTurn, StepMode mode );

  //#region Funktionen zur Steuerung

  //Betriebsmodus festlegen
  void SetMode(StepMode mode);

  //vorwärts oder rückwärts drehen
  /**
 * @param Schritte  Anzahl der Auszuführenden Schritte
 * @param stepBreak Pause zwischen den Schritten
 */
  void StepForward (int Schritte, unsigned int stepBreak );
  void StepBackward(int Schritte, unsigned int stepBreak );

// komplette Umdrehungen vorwärts oder rückwärts
  void TurnForward (double turns, unsigned int stepBreak);
  void TurnBackward(double turns, unsigned int stepBreak);

// schaltet den Motor aus (alle Ports auf LOW)
  void Off();

// #endregion


private:

  //#region private Variablen

  //array in dem alle Ports enthalten sind
  unsigned int ports[4];

  //Anzahl der Schritte pro Umdrehung
  unsigned int runsPerTurn_;

  //Betriebsmodus
  StepMode mode_;



  //#endregion

/**
 * [StepForwardMillis  description]
 * @param  stepBreak Pause zwischen den Schritten
 * @param  startTime Startzeitstempel der ersten Ausführung
 * @param  progress  Fortschritt der Schritte (0 zum beginnen)
 * @return           gibt Fortschritt zurück (-1, wenn der Schritt beendet ist)
 */
  int StepForwardMillis (unsigned int stepBreak,
    unsigned long startTime, int progress);
};


#endif
