using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UithofTramLijn
{
  class ConsoleInterface
  {

    UithofTrack track;

    public ConsoleInterface(UithofTrack tr)
    {
      track = tr;
    }

    public struct ExtendedEvent {
      public Event Event;
      public int NextStation;
    }

    public SortedList<double, ExtendedEvent> DataBase = new SortedList<double, ExtendedEvent>();

    public void storeEvent(Event e, int nextstation, Double time)
    {
      ExtendedEvent ev = new ExtendedEvent();
      ev.Event = e;
      ev.NextStation = nextstation;
      DataBase.Add(time,ev);
    }

    public void printPair(KeyValuePair<Double, ExtendedEvent> pair) {
      double time = pair.Key;
      Event ev = pair.Value.Event;
      int nextstation = pair.Value.NextStation;
      switch (ev.type) {
        case EventType.ExpectedArival:
          Console.ForegroundColor = ConsoleColor.Blue;
          Console.Out.WriteLine("Tram " + ev.TramId + " arrived at station " + (nextstation + 1) % 18 + " at time " + time);
          break;
        default:
          break;
      }

    }

    public void printAllPairs(List<KeyValuePair<Double, ExtendedEvent>> eventlist) {
      foreach (var li in eventlist) {
        printPair(li);
      }
    }

    public void consinter() {
      SortedList<double, Event> localbase = new SortedList<double, Event>();

      //foreach (KeyValuePair<double,Event> i in DataBase) {
      //  localbase.Add(i.Key, i.Value);
      //}

      String input = Console.In.ReadLine();
      int index = 0;

      string[] test = input.Split(' ');

      switch (test[index]) {
        case "t":
          index++;
          int tramid = int.Parse(test[index]);
          index++;
          var t = DataBase.Where(x => x.Value.Event.TramId == tramid).ToList();
          printAllPairs(t);
          break;
        default:
          break;
      }
    }

  }
}
