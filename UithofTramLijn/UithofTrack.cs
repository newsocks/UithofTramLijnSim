using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UithofTramLijn
{
    class UithofTrack
    {
        public Stop[] Stops = new Stop[18];
        public List<Tram> Trams = new List<Tram>();
        public double CrossCSBlockedUntill = 0;
        public double CrossPRBlockedUntill = 0;
        public class Stop
        {
            public String Name;
            public bool DirectionToCentralStation;
            public double TravelTime;
            public bool Occupied;
            public double LastOccupied;
        }

        public UithofTrack()
        {
            String[] StopNames = new String[9] { "Centraal Station", "Vaartse Rijn", "Galgenwaard",
                                           "Kromme Rijn", "Padualaan", "Heidelberglaan", "UMC",
                                           "WKZ", "P&R De Uithof" };
            double[] StopTravalTime = new double[18] { 0, 134, 243, 59, 101, 60, 86, 78, 113, 0, 110, 78, 82, 60, 100, 59, 243, 135 };

            for (int i = 0; i < 18; i++)
            {
                if (i < 9)
                {
                    Stops[i] = new Stop()
                    {
                        Name = StopNames[i],
                        DirectionToCentralStation = false,
                        TravelTime = StopTravalTime[i],
                        Occupied = false,
                        LastOccupied = -21,
                    };
                }
                else
                {
                    Stops[i] = new Stop()
                    {
                        Name = StopNames[9 - (i - 8)],
                        DirectionToCentralStation = true,
                        TravelTime = StopTravalTime[i],
                        Occupied = false,
                        LastOccupied = -21,
                    };
                }
            }
            for (int i = 0; i < 17; i++)
            {
                Stops[i].Name = Stops[i].Name + "(" + i + ")";
            }
        }
    }
}
