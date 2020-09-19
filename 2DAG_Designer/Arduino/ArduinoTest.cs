using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DAG_Designer.Arduino
{
    public static class ArduinoTest



    {
        public static void MoveX(int turns)
        {
            //Sendet dem Arduino die Bewegung in X Richtung
            Communication.Send($"#X{turns}");
        }

        public static void MoveY(int turns)
        {
            //Sendet dem Arduino die Bewegung in Y Richtung
            Communication.Send($"#Y{turns}");
        }
    }
}
