using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UithofTramLijn
{


  class Program
  {
    static void Main(string[] args)
    {
      
      Scheduler scheduler = new Scheduler();
      EventHandler eventhandler = new EventHandler(scheduler);

      bool end = false;

      while (!end) {
        end = eventhandler.HandleEvent();
      }
      
    }
  }
}
