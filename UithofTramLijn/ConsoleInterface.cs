using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UithofTramLijn
{
  class ConsoleInterface
  {

    public ConsoleInterface()
    {

    }
    public SortedList<double, Event> DataBase = new SortedList<double, Event>();

    public void storeEvent(Event e, Double time)
    {
      DataBase.Add(time, e);
    }

  }
}
