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
        ConsoleInterface ConsoleInterface;
        int tramId;
        public List<Event> onHold = new List<Event>();

        public EventHandler(Scheduler Scheduler, UithofTrack track, ConsoleInterface consoleInterface)
        {
            this.Scheduler = Scheduler;
            UithofTrack = track;
            ConsoleInterface = consoleInterface;
            tramId = 0;
        }

        public bool HandleEvent()
        {
            var next = Scheduler.getNextEvent();
            double curTime = next.Key;
            Event Event = next.Value;
            Tram tram, tramInFront;
            
            switch (Event.type)
            {
                case EventType.Despawn:
                    tram = UithofTrack.Trams.Where(x => x.id == Event.TramId).First();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Out.WriteLine("Despawned tram " + tram.id);
                    tramInFront = UithofTrack.Trams.Where(x => x.id == tram.inFrontId).First();
                    Tram tramBehind = UithofTrack.Trams.Where(x => x.id == tram.behindId).First();

                    if (tram.crossedOver)
                    {
                        UithofTrack.Stops[(tram.nextStation + 1) % 18].Occupied = false;
                        UithofTrack.Stops[(tram.nextStation + 1) % 18].LastOccupied = curTime;
                    }
                    else
                    {
                        UithofTrack.Stops[tram.nextStation].Occupied = false;
                        UithofTrack.Stops[tram.nextStation].LastOccupied = curTime;
                    }
                    UithofTrack.Trams.Remove(tram);
                    tramInFront.behindId = tramBehind.id;
                    tramBehind.inFrontId = tramInFront.id;
                    break;
                case EventType.ExpectedArival:
                    tram = UithofTrack.Trams.Where(x => x.id == Event.TramId).First();
                    tramInFront = UithofTrack.Trams.Where(x => x.id == tram.inFrontId).First();
                    if(tram.nextStation == 17 || tram.nextStation == 8)
                    {
                        double CrossBlockedUntill = UithofTrack.CrossPRBlockedUntill;
                        if(tram.nextStation == 17)
                        {
                            CrossBlockedUntill = UithofTrack.CrossCSBlockedUntill;
                        }
                        // if 0/9 and cross free go there
                        if(!UithofTrack.Stops[(tram.nextStation+1)%18].Occupied && CrossBlockedUntill <= curTime + 0.000001)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Out.WriteLine("Tram " + tram.id + " arrived at station " + UithofTrack.Stops[(tram.nextStation+1)%18].Name + " at time " + curTime);
                            tram.crossedOver = true;
                            UithofTrack.Stops[(tram.nextStation+1)%18].Occupied = true;
                            if (tram.nextStation == 17)
                            {
                                UithofTrack.CrossCSBlockedUntill = curTime + 40;
                            }
                            else
                            {
                                UithofTrack.CrossPRBlockedUntill = curTime + 40;
                            }
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, tram);
                        }
                        // if 17/8 free go there
                        else if(!UithofTrack.Stops[tram.nextStation].Occupied && CrossBlockedUntill <= curTime + 0.000001)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Out.WriteLine("Tram " + tram.id + " arrived at station " + UithofTrack.Stops[tram.nextStation].Name + " at time " + curTime);
                            UithofTrack.Stops[tram.nextStation].Occupied = true;
                            if (tram.nextStation == 17)
                            {
                                UithofTrack.CrossCSBlockedUntill = curTime + 40;
                            }
                            else
                            {
                                UithofTrack.CrossPRBlockedUntill = curTime + 40;
                            }
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, tram);
                        }
                        // reschedule otherwise
                        else if (!UithofTrack.Stops[tram.nextStation].Occupied || !UithofTrack.Stops[(tram.nextStation + 1) % 18].Occupied)
                        {
                            if (Scheduler.EventQue.ContainsKey(CrossBlockedUntill))
                            {
                                if (Scheduler.EventQue.Values[Scheduler.EventQue.IndexOfKey(CrossBlockedUntill)].TramId == tram.inFrontId)
                                {
                                    Scheduler.EventQue.Add(CrossBlockedUntill + 0.0000001, Event);
                                }
                                else
                                {
                                    Scheduler.EventQue.Add(CrossBlockedUntill - 0.0000001, Event);
                                }
                            }
                            else
                            {
                                Scheduler.EventQue.Add(CrossBlockedUntill, Event);
                            }
                        }
                        else
                        {
                            Hold(EventType.ExpectedArival, tram.id);
                        }
                    }
                    else if(!UithofTrack.Stops[tram.nextStation].Occupied 
                        && (tramInFront.nextStation != tram.nextStation || tramInFront.id == tram.id))
                    {
                        //TODO: 20 vervangen door goede waarde
                        if (UithofTrack.Stops[tram.nextStation].LastOccupied + 20 > curTime)
                        {
                            {
                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                Console.Out.WriteLine("Reschedule tram " + tram.id + " because within 20 sec");
                                Scheduler.EventQue.Add(UithofTrack.Stops[tram.nextStation].LastOccupied + 20, new Event()
                                {
                                    TramId = tram.id,
                                    type = EventType.ExpectedArival,
                                });
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Out.WriteLine("Tram " + tram.id + " arrived at station " + UithofTrack.Stops[tram.nextStation].Name + " at time " + curTime);
                            UithofTrack.Stops[tram.nextStation].Occupied = true;
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, tram);
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
                    int station = 0;
                    if (Event.SpawnAtPR)
                    {
                        station = 8;
                    }
                    if (!UithofTrack.Stops[station].Occupied)
                    {
                        int id = tramId++;
                        if (UithofTrack.Trams.Count() == 0)
                        {
                            Tram cur = new Tram(id, station, id, id);
                            UithofTrack.Trams.Add(cur);
                            UithofTrack.Stops[station].Occupied = true;
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, cur);
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
                                if(bestNextStation > station)
                                {
                                    if(candidate.nextStation <= station)
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
                                    if (candidate.nextStation > station)
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
                            Tram cur = new Tram(id, station, best.id, behind.id);
                            behind.inFrontId = id;
                            best.behindId = id;
                            UithofTrack.Trams.Add(cur);
                            UithofTrack.Stops[station].Occupied = true;
                            Scheduler.scheduleEvent(EventType.Leaves, curTime, cur);
                        }
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Out.WriteLine("Tram " + id + " spawned!");
                    }
                    else if(!UithofTrack.Stops[station+1].Occupied)
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
                            if (bestNextStation > station+1)
                            {
                                if (candidate.nextStation <= station+1)
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
                                if (candidate.nextStation > station+1)
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
                        Tram cur = new Tram(id, station+1, best.id, behind.id);
                        behind.inFrontId = id;
                        best.behindId = id;
                        UithofTrack.Trams.Add(cur);
                        UithofTrack.Stops[station+1].Occupied = true;
                        Scheduler.scheduleEvent(EventType.Leaves, curTime, cur);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Out.WriteLine("Tram " + id + " spawned!");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Out.WriteLine("SPAWNHOLD");
                        Hold(EventType.ExpectedSpawn, 0);
                    }
                    break;
                case EventType.Leaves:
                    tram = UithofTrack.Trams.Where(x => x.id == Event.TramId).First();
                    if (tram.nextStation == 17 && curTime +0.0001 < UithofTrack.CrossCSBlockedUntill)
                    {
                        Scheduler.EventQue.Add(UithofTrack.CrossCSBlockedUntill-0.0000000001, Event);
                    }
                    else if (tram.nextStation == 8 && curTime + 0.0001 < UithofTrack.CrossPRBlockedUntill)
                    {
                        Scheduler.EventQue.Add(UithofTrack.CrossPRBlockedUntill-0.0000000001, Event);
                    }
                    else
                    {
                        if (tram.nextStation == 17)
                        {
                            UithofTrack.CrossCSBlockedUntill = curTime + 40;
                        }
                        else if (tram.nextStation == 8)
                        {
                            UithofTrack.CrossPRBlockedUntill = curTime + 40;
                        }
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        // if 9/0
                        if (tram.crossedOver)
                        {
                            tram.crossedOver = false;
                            Console.Out.WriteLine("Tram " + tram.id + " leaves station " + UithofTrack.Stops[(tram.nextStation+1)%18].Name + " at " + curTime);
                            UithofTrack.Stops[(tram.nextStation+1)%18].Occupied = false;
                            UithofTrack.Stops[(tram.nextStation+1)%18].LastOccupied = curTime;
                        }
                        else
                        {
                            Console.Out.WriteLine("Tram " + tram.id + " leaves station " + UithofTrack.Stops[tram.nextStation].Name + " at " + curTime);
                            UithofTrack.Stops[tram.nextStation].Occupied = false;
                            UithofTrack.Stops[tram.nextStation].LastOccupied = curTime;
                        }
                        // Check holding trams
                        bool doSomething = false;
                        int doWithId = 0;
                        Tram itemTram = null;
                        foreach (Event item in onHold)
                        {
                            itemTram = UithofTrack.Trams.Where(x => x.id == item.TramId).First();
                            if(item.type == EventType.ExpectedArival
                                && legalNextstation(itemTram, tram)
                                && tram.behindId == itemTram.id)
                            {
                                doWithId = onHold.IndexOf(item);
                                doSomething = true;
                                break;
                            }
                        }
                        if (!doSomething)
                        {
                            foreach (Event item in onHold)
                            {
                                itemTram = UithofTrack.Trams.Where(x => x.id == item.TramId).First();
                                if (item.type == EventType.ExpectedArival
                                    && legalNextstation(itemTram, tram))
                                {
                                    doWithId = onHold.IndexOf(item);
                                    doSomething = true;
                                    break;
                                }
                            }
                        }
                        if (doSomething)
                        {
                            if(tram.nextStation == 8 || tram.nextStation == 9)
                            {
                                if (Scheduler.EventQue.ContainsKey(UithofTrack.CrossPRBlockedUntill))
                                {
                                    if (Scheduler.EventQue.Values[Scheduler.EventQue.IndexOfKey(UithofTrack.CrossPRBlockedUntill)].TramId == tram.inFrontId)
                                    {
                                        Scheduler.EventQue.Add(UithofTrack.CrossPRBlockedUntill + 0.0000001, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                                    }
                                    else
                                    {
                                        Scheduler.EventQue.Add(UithofTrack.CrossPRBlockedUntill - 0.0000001, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                                    }
                                }
                                else
                                {
                                    Scheduler.EventQue.Add(UithofTrack.CrossPRBlockedUntill, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                                }
                            }
                            else if (tram.nextStation == 17 || tram.nextStation == 0)
                            {
                                if (Scheduler.EventQue.ContainsKey(UithofTrack.CrossCSBlockedUntill))
                                {
                                    if (Scheduler.EventQue.Values[Scheduler.EventQue.IndexOfKey(UithofTrack.CrossCSBlockedUntill)].TramId == tram.inFrontId)
                                    {
                                        Scheduler.EventQue.Add(UithofTrack.CrossCSBlockedUntill + 0.0000001, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                                    }
                                    else
                                    {
                                        Scheduler.EventQue.Add(UithofTrack.CrossCSBlockedUntill - 0.0000001, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                                    }
                                }
                                else
                                {
                                    Scheduler.EventQue.Add(UithofTrack.CrossCSBlockedUntill, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                                }
                            }
                            else
                            {
                                Scheduler.EventQue.Add(curTime + 20, new Event() { type = EventType.ExpectedArival, TramId = itemTram.id });
                            }
                            // TODO: DIE 20 ><
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
                                Scheduler.EventQue.Add(curTime + 20, new Event() { type = EventType.ExpectedSpawn, TramId = itemTram.id });
                                onHold.RemoveAt(doWithId);
                            }
                        }
                        if (doSomething)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Out.WriteLine("Hold opgeheven, nog " + onHold.Count + " in de lijst");
                        }

                        Scheduler.scheduleEvent(EventType.ExpectedArival, curTime, tram);
                        // makes tram go to correct station, always +1, 8=>10, 17=>1
                        if (tram.nextStation == 8)
                        {
                            tram.nextStation++;
                        }
                        else if (tram.nextStation == 17)
                        {
                            tram.nextStation = 0;
                        }
                        tram.nextStation++;
                    }
                    break;
                case EventType.SimulationFinished:
                    Console.Out.WriteLine("sim end");
                    return true;
                default:
                    break;
            }

            ConsoleInterface.storeEvent(Event, 0, curTime);
            return false;
        }

        private bool legalNextstation(Tram holding, Tram leaving)
        {
            if (leaving.nextStation == holding.nextStation)// && leaving.behindId == holding.id)
            {
                return true;
            }
            if (leaving.nextStation == 9 && holding.nextStation == 8)/*5 &&
                (leaving.behindId == holding.id || leaving.behindId == holding.inFrontId))*/
            {
                return true;
            }
            if (leaving.nextStation == 0 && holding.nextStation == 17 )/*&&
                 (leaving.behindId == holding.id || leaving.behindId == holding.inFrontId))*/
            {
                return true;
            }
            return false;
        }

        internal void Hold(EventType type, int id)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Out.WriteLine("Tram " + id + " staat op hold met type " + type);
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
