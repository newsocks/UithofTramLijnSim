using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace UithofTramLijn
{
    class Scheduler
    {
        public SortedList<double, Event> EventQue = new SortedList<double, Event>();
        public Random rand = new Random();
        UithofTrack uithofTrack;
        Queue<double> timeTablePR;
        Queue<double> timeTableCS;
        int despawnAtCS;
        int despawnAtPR;
        int freq;
        public StreamWriter delayCSWriter = new StreamWriter("PunctualityCS.txt");
        public StreamWriter delayPRWriter = new StreamWriter("PunctualityPR.txt");

        public Scheduler(UithofTrack track, int freq)
        {
            this.freq = freq;
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
            EventQue.Add(0-200, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = true });
            EventQue.Add(300 - 200, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = false });
            EventQue.Add(900 - 200, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = true });
            int toSpawn = (int)Math.Ceiling(2.0 * (double)freq / 3.0) - 3;
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
                    EventQue.Add(timesCS[i/2] - 200, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = false });
                }
                else
                {
                    EventQue.Add(timesPR[(i-1)/2] - 200, new Event() { type = EventType.ExpectedSpawn, SpawnAtPR = true });
                }
            }
            //despawn trams
            if (freq%2 == 0)
            {
                despawnAtPR = toSpawn / 2; // round down
                despawnAtCS = toSpawn - despawnAtCS; // round up
            }
            else
            {
                despawnAtCS = toSpawn / 2; // round down
                despawnAtPR = toSpawn - despawnAtCS; // round up
            }
            // schedule resets
            for (int i = 0; i < 64; i++)
            {
                EventQue.Add(i * 900-0.0001, new Event() { type = EventType.PassengerSpawnReset });
            }
        }

        public void scheduleEvent(EventType type, double currentTime, Tram tram)
        {
            if (currentTime > 28000)
            {
                ;
            }
            double key;
            Event value;
            switch (type)
            {
                case EventType.ExpectedArival:
                    double rng = rand.NextDouble();
                    double rng2 = rand.NextDouble();
                    double traveltime = uithofTrack.Stops[tram.nextStation].TravelTime + 0.125*uithofTrack.Stops[tram.nextStation].TravelTime
                        * Math.Sqrt(-2.0 * Math.Log(rng)) * Math.Sin(2.0 * Math.PI * rng2);
                    key = currentTime + traveltime;
                    value = new Event() { type = EventType.ExpectedArival, TramId = tram.id };
                    EventQue.Add(key, value);
                    break;
                case EventType.Leaves:
                    //if cs/pr
                    double earliestLeave = double.MinValue;
                    if((tram.nextStation == 0 || tram.nextStation == 17) && timeTableCS.Count > 0)
                    {
                        earliestLeave = timeTableCS.Dequeue();
                        if (earliestLeave > 60 / freq * 60 + 13 * 3600 && despawnAtCS > 0)
                        {
                            despawnAtCS--;
                            EventQue.Add(currentTime+0.0000001, new Event() { TramId = tram.id, type = EventType.Despawn });
                            break;
                        }
                    }
                    else if ((tram.nextStation == 8 || tram.nextStation == 9) && timeTablePR.Count > 0)
                    {
                        earliestLeave = timeTablePR.Dequeue();
                        if (earliestLeave > 13 * 3600 && despawnAtPR > 0)
                        {
                            despawnAtPR--;
                            EventQue.Add(currentTime+0.00000001, new Event() { TramId = tram.id, type = EventType.Despawn });
                            break;
                        }
                    }
                    if (tram.nextStation == 8 || tram.nextStation == 9 || tram.nextStation == 17 || tram.nextStation == 0)
                    {
                        if ((tram.nextStation == 8 || tram.nextStation == 9) && earliestLeave != double.MinValue)
                        {
                            delayPRWriter.WriteLine(Math.Max(0, currentTime - earliestLeave + 180) + "curtime = " + (currentTime+180) + " timetable: " + earliestLeave);
                        }
                        else if ((tram.nextStation == 17 || tram.nextStation == 0) && earliestLeave != double.MinValue)
                        {
                            delayCSWriter.WriteLine(Math.Max(0, currentTime - earliestLeave + 180) + "curtime = " + (currentTime+180) + " timetable: " + earliestLeave);
                        }
                        if (earliestLeave == double.MinValue)
                        {
                            EventQue.Add(currentTime+0.0000001, new Event() { TramId = tram.id, type = EventType.Despawn });
                            break;
                        }
                        else if (currentTime + 180 > earliestLeave + 60)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Out.WriteLine("TE LAAT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Out.WriteLine("OP TIJD :D:D:D:D:D:D:D::D:D:D:D:D:D:D:D:D:D:D:D:D::D:D:D:D:");
                        }
                        key = Math.Max(currentTime + 180, earliestLeave);
                    }
                    else
                    {
                        key = Math.Max(currentTime + 12.5 + 0.22*tram.entering + 0.13*tram.exiting, earliestLeave);
                    }
                    value = new Event() { type = EventType.Leaves, TramId = tram.id };
                    if(EventQue.ContainsKey(key))
                    {
                        key += 0.000000000001;
                    }
                    EventQue.Add(key, value);
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

    public enum EventType { ExpectedArival, Leaves, ExpectedSpawn, SimulationFinished, Despawn, PassengerSpawn, PassengerSpawnReset }

    public struct Event
    {
        public EventType type;
        public int TramId;
        public bool SpawnAtPR;
        public int SpawnStation;
    }
}
