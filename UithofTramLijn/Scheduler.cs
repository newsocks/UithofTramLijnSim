using System;
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

        public Scheduler(UithofTrack track)
        {
            uithofTrack = track;
            scheduleEvent(EventType.SimulationFinished, 0, null, false);
            for (int i = 0; i < 15; i++)
            {
                EventQue.Add(i*240, new Event() { type = EventType.ExpectedSpawn });
            }
        }

        public void scheduleEvent(EventType type, double currentTime, Tram tram, bool buitenDienst)
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
                    key = currentTime + 20 + 40 * rand.NextDouble();
                    value = new Event() { type = EventType.Leaves, TramId = tram.id };
                    EventQue.Add(key, value);
                    break;
                case EventType.ExpectedArivalAtCross:
                    break;
                case EventType.ExpectedSpawn:
                    EventQue.Add(currentTime + 20, new Event() { type = EventType.ExpectedSpawn });//TODO: GOEDE TIJD
                    break;
                case EventType.SimulationFinished:
                    EventQue.Add(7200, new Event() { type = EventType.SimulationFinished });
                    break;
                default:
                    break;
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
    }
}
