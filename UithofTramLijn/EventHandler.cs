using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UithofTramLijn
{
  class EventHandler
  {
    Scheduler Scheduler;

    public EventHandler(Scheduler Scheduler)
    {
      this.Scheduler = Scheduler;
    }

    public bool HandleEvent() {
      Event next = Scheduler.getNextEvent();
      switch (next.type)
      {
        case EventType.SimulationFinished:
          Console.Out.WriteLine("sim end");
          return true;
        default:
          break;
      }
      return false;
    }

  }
}
