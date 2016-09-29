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
        UithofTrack UithofTrack;
        int tramId;
        public List<Event> onHold = new List<Event>();

        public EventHandler(Scheduler Scheduler, UithofTrack track)
        {
            this.Scheduler = Scheduler;
            UithofTrack = track;
            tramId = 0;
        }

        public bool HandleEvent()
        {
            var next = Scheduler.getNextEvent();
            double curTime = next.Key;
            Event Event = next.Value;
            Tram tram;
            switch (Event.type)
            {
                case EventType.ExpectedArival:
                    tram = UithofTrack.Trams.Where(x => x.id == Event.TramId).First();
                    Tram tramInFront = UithofTrack.Trams.Where(x => x.id == tram.inFrontId).First();
                    if(!UithofTrack.Stops[tram.nextStation].Occupied 
                        && (tramInFront.nextStation != tram.nextStation || tramInFront.id == tram.id))
                    {
                        //TODO: 20 vervangen door goede waarde
                        if (UithofTrack.Stops[tram.nextStation].LastOccupied + 20 > curTime)
                        {
                            {
                                Console.Out.WriteLine("Reschedule because within 20 sec");
                                Scheduler.EventQue.Add(UithofTrack.Stops[tram.nextStation].LastOccupied + 20, new Event()
                                {
                                    TramId = tram.id,
                                    type = EventType.ExpectedArival,
                                });
                            }
                        }
                        else
                        {
                            Console.Out.WriteLine("Tram " + tram.id + " arrived at station " + tram.nextStation + " at time " + curTime);
                            UithofTrack.Stops[tram.nextStation].Occupied = true;
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, tram, false);
                        }
                    }
                    else
                    {
                        Hold(EventType.ExpectedArival, tram.id);
                    }
                    break;
                case EventType.ExpectedArivalAtCross:
                    break;
                case EventType.ExpectedSpawn:
                    if (!UithofTrack.Stops[8].Occupied)
                    {
                        int id = tramId++;
                        if (UithofTrack.Trams.Count() == 0)
                        {
                            Tram cur = new Tram(id, 8, id, id);
                            UithofTrack.Trams.Add(cur);
                            UithofTrack.Stops[8].Occupied = true;
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, cur, false);
                        }
                        else
                        {
                            Tram best = UithofTrack.Trams[0];
                            Tram candidate = best;
                            int bestNextStation = best.nextStation;
                            bool done = false;
                            int i = UithofTrack.Trams.Count;
                            while (!done)
                            {
                                candidate = UithofTrack.Trams.Where(x => x.id == candidate.behindId).First();
                                if(bestNextStation > 8)
                                {
                                    if(candidate.nextStation <= 8)
                                    {
                                        done = true;
                                    }
                                    else if (candidate.nextStation <= bestNextStation)
                                    {
                                        best = candidate;
                                        bestNextStation = candidate.nextStation;
                                    }
                                }
                                else
                                {
                                    if (candidate.nextStation > 8)
                                    {
                                        best = candidate;
                                        bestNextStation = candidate.nextStation;
                                    }
                                    else if (candidate.nextStation <= bestNextStation)
                                    {
                                        best = candidate;
                                        bestNextStation = candidate.nextStation;
                                    }
                                }
                                if (i == 0)
                                {
                                    done = true;
                                }
                                i--;
                            }
                            Tram behind = UithofTrack.Trams.Where(x => x.id == best.behindId).First();
                            Tram cur = new Tram(id, 8, best.id, behind.id);
                            behind.inFrontId = id;
                            best.behindId = id;
                            UithofTrack.Trams.Add(cur);
                            UithofTrack.Stops[8].Occupied = true;
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, cur, false);
                        }
                        Console.Out.WriteLine("Tram " + id + " spawned!");
                    }
                    else if(!UithofTrack.Stops[9].Occupied)
                    {
                        int id = tramId++;
                        Tram best = UithofTrack.Trams[0];
                        Tram candidate = best;
                        int bestNextStation = best.nextStation;
                        bool done = false;
                        int i = UithofTrack.Trams.Count;
                        while (!done)
                        {
                            candidate = UithofTrack.Trams.Where(x => x.id == candidate.behindId).First();
                            if (bestNextStation > 9)
                            {
                                if (candidate.nextStation <= 9)
                                {
                                    done = true;
                                }
                                else if (candidate.nextStation <= bestNextStation)
                                {
                                    best = candidate;
                                    bestNextStation = candidate.nextStation;
                                }
                            }
                            else
                            {
                                if (candidate.nextStation > 9)
                                {
                                    best = candidate;
                                    bestNextStation = candidate.nextStation;
                                }
                                else if (candidate.nextStation <= bestNextStation)
                                {
                                    best = candidate;
                                    bestNextStation = candidate.nextStation;
                                }
                            }
                            if (i == 0)
                            {
                                done = true;
                            }
                            i--;
                        }
                        Tram behind = UithofTrack.Trams.Where(x => x.id == best.behindId).First();
                        Tram cur = new Tram(id, 9, best.id, behind.id);
                        behind.inFrontId = id;
                        best.behindId = id;
                        UithofTrack.Trams.Add(cur);
                        UithofTrack.Stops[9].Occupied = true;
                        Scheduler.scheduleEvent(EventType.Leaves, curTime, cur, false);
                        Console.Out.WriteLine("Tram " + id + " spawned!");
                    }
                    else
                    {
                        Console.Out.WriteLine("SPAWNHOLD");
                        Hold(EventType.ExpectedSpawn, 0);
                    }
                    break;
                case EventType.Leaves:
                    tram = UithofTrack.Trams.Where(x => x.id == Event.TramId).First();
                    Console.Out.WriteLine("Tram " + tram.id + " leaves station " + tram.nextStation);
                    UithofTrack.Stops[tram.nextStation].Occupied = false;
                    UithofTrack.Stops[tram.nextStation].LastOccupied = curTime;
                    // Check holding trams
                    bool doSomething = false;
                    int doWithId = 0;
                    Tram itemTram = null;
                    foreach (Event item in onHold)
                    {
                        itemTram = UithofTrack.Trams.Where(x => x.id == item.TramId).First();
                        if (item.type == EventType.ExpectedArival 
                            && itemTram.nextStation == tram.nextStation
                            && tram.behindId == itemTram.id)
                        {
                            doWithId = onHold.IndexOf(item);
                            doSomething = true;
                            break;
                        }
                    }
                    if (doSomething)
                    {
                        // TODO: DIE 20 ><
                        Scheduler.EventQue.Add(curTime + 20, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                        onHold.RemoveAt(doWithId);
                    }
                    else
                    {
                        foreach (Event item in onHold)
                        {
                            itemTram = UithofTrack.Trams.Where(x => x.id == item.TramId).First();
                            if (item.type == EventType.ExpectedSpawn
                                && tram.nextStation == 8 || tram.nextStation == 9)
                            {
                                doWithId = onHold.IndexOf(item);
                                doSomething = true;
                                break;
                            }
                        }
                        if (doSomething)
                        {
                            Scheduler.EventQue.Add(curTime + 20, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                            onHold.RemoveAt(doWithId);
                        }
                    }
                    if (doSomething)
                    {
                        Console.Out.WriteLine("Hold opgeheven, nog " + onHold.Count + " in de lijst");
                    }

                    // makes tram go to correct station, always +1, 8=>10, 17=>1
                    if (tram.nextStation == 8)
                    {
                        tram.nextStation++;
                    }
                    else if(tram.nextStation == 17)
                    {
                        tram.nextStation = 0;
                    }
                    tram.nextStation++;
                    Scheduler.scheduleEvent(EventType.ExpectedArival, curTime, tram, false);
                    break;
                case EventType.SimulationFinished:
                    Console.Out.WriteLine("sim end");
                    return true;
                default:
                    break;
            }
            return false;
        }

        internal void Hold(EventType type, int id)
        {
            Console.Out.WriteLine("ONHOLD!");
            switch (type)
            {
                case EventType.ExpectedArival:
                    onHold.Add(new Event() { type = type, TramId = id });
                    break;
                case EventType.Leaves:
                    break;
                case EventType.ExpectedArivalAtCross:
                    break;
                case EventType.ExpectedSpawn:
                    onHold.Add(new Event() { type = EventType.ExpectedSpawn });
                    break;
                case EventType.SimulationFinished:
                    break;
                default:
                    break;
            }
        }
    }
}
