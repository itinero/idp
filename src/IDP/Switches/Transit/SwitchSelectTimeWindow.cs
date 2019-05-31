using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Transit
{
    class SwitchSelectTimeWindow : DocumentedSwitch
    {
        private static readonly string[] _names = {"--select-time","--filter-time"};

        private static string _about =
            "Filters the transit-db so that only connections departing in the specified time window are kept. " +
            "This allows to take a small slice out of the transitDB, which can be useful to debug. " +
            "All locations will be kept.";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("window-start", "start", "The start time of the window, specified as `YYYY-MM-DD_hh:mm:ss` (e.g. `2019-12-31_23:59:59`)"),
                    obl("duration","window-end", "Either the length of the time window in seconds or the end of the time window in `YYYY-MM-DD_hh:mm:ss`"),
                    //   opt("interpretation",
                    //           "How the departure times are interpreted. Options are: `actual`, `planned` or `both`. If `planned` is specified, the connection will only be kept if the planned departure time is within the window (thus as if there would not have been a delay). With `actual`, only the actual (with delays) departure time is used. Both will keep the connection if either the actual or planned departure time are within the window.")
                    //      .SetDefault("both"),
                    opt("allow-empty", "If flagged, the program will not crash if no connections are retained")
                        .SetDefault("false")
                };

        private const bool _isStable = true;


        public SwitchSelectTimeWindow
            () :
            base(_names, _about, _extraParams, _isStable)
        {
        }

        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            if (!(previous[previous.Count - 1] is IProcessorTransitDbSource source))
            {
                throw new Exception("Expected a transit db source.");
            }


            var start = DateTime.ParseExact(arguments["window-start"], "yyyy-MM-dd_HH:mm:ss", null);
            start = start.ToUniversalTime();
            int duration;
            try
            {
                duration = int.Parse(arguments["duration"]);
            }
            catch (FormatException)
            {
                var endDate = DateTime.ParseExact(arguments["duration"], "yyyy-MM-dd_HH:mm:ss", null);
                endDate = endDate.ToUniversalTime();
                duration = (int) (endDate - start).TotalSeconds;
            }
            var allowEmpty = bool.Parse(arguments["allow-empty"]);

            TransitDb Run()
            {
                var old = source.GetTransitDb();

                var filtered = new TransitDb();
                var wr = filtered.GetWriter();


                var stopIdMapping = new Dictionary<LocationId, LocationId>();

                var stops = old.Latest.StopsDb.GetReader();
                while (stops.MoveNext())
                {
                    var newId = wr.AddOrUpdateStop(stops.GlobalId, stops.Longitude, stops.Latitude, stops.Attributes);
                    var oldId = stops.Id;
                    stopIdMapping.Add(oldId, newId);
                }


                var trips = old.Latest.TripsDb.GetReader();


                var conns = old.Latest.ConnectionsDb.GetDepartureEnumerator();
                if (!conns.MoveNext(start))
                {
                    if (!allowEmpty)
                    {
                        throw new Exception($"No data in the database after {start:O}");
                    }
                }

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

                var copied = 0;
                do
                {
                    trips.MoveTo(conns.TripId);
                    var newTripId = wr.AddOrUpdateTrip(trips.GlobalId, trips.Attributes);

                    var depTime =
                        epoch.AddSeconds(conns.DepartureTime);

                        
                    wr.AddOrUpdateConnection(
                        stopIdMapping[conns.DepartureStop],
                        stopIdMapping[conns.ArrivalStop],
                        conns.GlobalId,
                        depTime,
                        conns.TravelTime,
                        conns.DepartureDelay,
                        conns.ArrivalDelay,
                        newTripId,
                        conns.Mode
                    );
                    
                    copied++;
                    if ((depTime - start).TotalSeconds > duration)
                    {
                        break;
                    }
                } while (conns.MoveNext());
                wr.Close();


                if (!allowEmpty && copied == 0)
                {
                    throw new Exception("There are no connections in the given timeframe.");
                }
                

                Console.WriteLine($"There are {copied} connections in the filtered transitDB");
                return filtered;
            }

            return (new ProcessorTransitDbSource(Run), 1);
        }
    }
}