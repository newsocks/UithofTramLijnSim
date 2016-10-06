﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UithofTramLijn
{
    class Scheduler
    {
        public SortedList<double, Event> EventQue = new SortedList<double, Event>();
        Random rand = new Random();
        UithofTrack uithofTrack;
        Queue<double> timeTablePR;
        Queue<double> timeTableCS;

        public Scheduler(UithofTrack track, int freq)
        {
            uithofTrack = track;
            scheduleEvent(EventType.SimulationFinished, 0, null);
            timeTablePR = new Queue<double>();
            // make schedule
            for (int i = 0; i < 4; i++)
            {
                timeTablePR.Enqueue(i * 900);
            }
            for (int i = 0; i < 12*freq; i++)
            {
                timeTablePR.Enqueue(3600 + 60 * 60 * i / freq);
            }
            for (int i = 0; i < 10; i++)
            {
                timeTablePR.Enqueue(13 * 3600 + i * 900);
            }
            timeTableCS = new Queue<double>();
            for (int i = 0; i < 4; i++)
            {
                timeTableCS.Enqueue(300 + i * 900);
            }
            for (int i = 0; i < 12 * freq; i++)
            {
                double starttime = (5 + 60 / freq) % (60 / freq) * 60;
                timeTableCS.Enqueue(3600 + starttime + 60 * 60 * i / freq);
            }
            for (int i = 0; i < 10; i++)
            {
                timeTableCS.Enqueue(13 * 3600 + 300 + i * 900);
            }
            //spawn trams
            EventQue.Add(0-60, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = true });
            EventQue.Add(300 - 60, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = false });
            EventQue.Add(900 - 60, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = true });
            int toSpawn = (int)Math.Ceiling(2.0 * freq / 3) - 3;
            double t = (5 + 60 / freq) % (60 / freq) * 60;
            List<int> timesCS = new List<int>();
            List<int> timesPR = new List<int>();
            for (int i = 0; i < toSpawn; i++)
            {
                timesCS.Add((int)(3600 + t + 60 * 60 * i / freq));
                timesPR.Add((int)(3600 + 60 * 60 * i / freq));
            }
            int j = 0;
            while(true)
            {
                if(timesCS[j] >= 3900)
                {
                    timesCS.Remove(timesCS[j]);
                    break;
                }
                j++;
            }
            timesPR.Remove(3600);
            for (int i = 0; i < toSpawn; i++)
            {
                if(i%2 == 0)
                {
                    EventQue.Add(timesCS[i/2] - 60, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = false });
                }
                else
                {
                    EventQue.Add(timesPR[(i-1)/2] - 60, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = true });
                }
            }
        }

        public void scheduleEvent(EventType type, double currentTime, Tram tram)
        {
            double key;
            Event value;
            switch (type)
            {
                case EventType.ExpectedArival:
                    key = currentTime + uithofTrack.Stops[tram.nextStation].TravelTime;
                    value = new Event() { type = EventType.ExpectedArival, TramId = tram.id };
                    EventQue.Add(key, value);
                    break;
                case EventType.Leaves:
                    //if cs/pr
                    double earliestLeave = double.MinValue;
                    if((tram.nextStation == 0 || tram.nextStation == 17) && timeTableCS.Count > 0)
                    {
                        earliestLeave = timeTableCS.Dequeue();
                    }
                    else if ((tram.nextStation == 8 || tram.nextStation == 9) && timeTablePR.Count > 0)
                    {
                        earliestLeave = timeTablePR.Dequeue();
                    }
                    if (tram.nextStation == 8 || tram.nextStation == 9 || tram.nextStation == 17 || tram.nextStation == 0)
                    {
                        key = Math.Max(currentTime + 300, earliestLeave);
                    }
                    else
                    {
                        key = Math.Max(currentTime + 20 + 40 * rand.NextDouble(), earliestLeave);
                    }
                    value = new Event() { type = EventType.Leaves, TramId = tram.id };
                    if(EventQue.ContainsKey(key))
                    {
                        key += 0.000000000001;
                    }
                    EventQue.Add(key, value);
                    break;
                case EventType.ExpectedArivalAtCross:
                    break;
                case EventType.SimulationFinished:
                    EventQue.Add(16*3600, new Event() { type = EventType.SimulationFinished });
                    break;
                default:
                    throw new System.ArgumentException("We komen bij default Oo", "original");
            }
        }

        public KeyValuePair<double, Event> getNextEvent()
        {
            var result = EventQue.First();
            EventQue.Remove(result.Key);
            return result;
        }
    }

    public enum EventType { ExpectedArival, Leaves, ExpectedArivalAtCross, ExpectedSpawn, SimulationFinished }

    public struct Event
    {
        public EventType type;
        public int TramId;
        public bool SpawnAtPR;
    }
}
