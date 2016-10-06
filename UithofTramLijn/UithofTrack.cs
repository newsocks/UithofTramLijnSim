using System;
using System.Collections.Generic;
using System.IO;
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
            public List<double> WaitingPassengers;
            public double[] ArrivalRate;
            public double[] departureQuotent;
        }

        public UithofTrack()
        {
            StreamReader reader = new StreamReader("input-data-passengers.csv");
            String[] StopNames = new String[9] { "Centraal Station", "Vaartse Rijn", "Galgenwaard",
                                           "Kromme Rijn", "Padualaan", "Heidelberglaan", "UMC",
                                           "WKZ", "P&R De Uithof" };
            double[] StopTravalTime = new double[18] { 134, 243, 59, 101, 60, 86, 78, 113, 110, 110, 78, 82, 60, 100, 59, 243, 135, 134 };

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
                        WaitingPassengers = new List<double>(),
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
                        WaitingPassengers = new List<double>(),
                    };
                }
                Stops[i].ArrivalRate = new double[64];
                Stops[i].departureQuotent = new double[64];
                string[] arr = reader.ReadLine().Split(new char[] { ';' });
                int counter = 0;
                for (int j = 0; j < arr.Length; j+=3)
                {
                    for (int k = 0; k < int.Parse(arr[j+2]); k++)
                    {
                        Stops[i].ArrivalRate[counter] = double.Parse(arr[j]);
                        Stops[i].departureQuotent[counter] = double.Parse(arr[j + 1]);
                        counter++;
                    }
                }
            }
            for (int i = 0; i < 17; i++)
            {
                Stops[i].Name = Stops[i].Name + "(" + i + ")";
            }
        }
    }
}
