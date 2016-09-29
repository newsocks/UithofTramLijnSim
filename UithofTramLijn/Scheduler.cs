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

    public Scheduler()
    {
      EventQue.Add(1000, new Event() { type = EventType.SimulationFinished});

    }

    public Event getNextEvent() {
      var result = EventQue.First();
      EventQue.Remove(result.Key);
      return result.Value;
    }

  }

  public enum EventType { ExpectedArival, Leaves, ExpectedArivalAtCross, ExpectedSpawn, SimulationFinished } 

  public struct Event
  {
    public EventType type;
    public int TramId;
  }
}
