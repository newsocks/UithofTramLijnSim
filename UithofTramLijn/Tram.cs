using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UithofTramLijn
{
    class Tram
    {
        public int[] exitingPassengers = new int[18];
        public int nextStation;
        public int id;
        public int inFrontId;
        public int behindId;

        public Tram(int id, int next, int inFrontId, int behindId)
        {
            this.id = id;
            nextStation = next;
            this.inFrontId = inFrontId;
            this.behindId = behindId;
        }
    }
}
