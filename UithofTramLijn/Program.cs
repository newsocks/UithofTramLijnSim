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
            UithofTrack uithofTrack = new UithofTrack();
            Scheduler scheduler = new Scheduler(uithofTrack, 15);
            EventHandler eventhandler = new EventHandler(scheduler, uithofTrack);

            bool end = false;

            while (!end)
            {
                end = eventhandler.HandleEvent();
            }
            Console.In.ReadLine();
        }
    }
}
