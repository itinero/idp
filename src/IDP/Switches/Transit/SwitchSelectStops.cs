using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit;
using Itinero.Transit.Data;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Transit
{
    class SwitchSelectStops : DocumentedSwitch
    {
        private static readonly string[] _names = {"--select-stops", "--filter-stops", "--bounding-box", "--bb"};

        private static string _about =
            "Filters the transit-db so that only stops withing the bounding box are kept. " +
            "All connections containing a removed location will be removed as well.\n\n" +
            "This switch is mainly used for debugging.";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("left",
                        "Specifies the minimal latitude of the output."),
                    obl("right",
                        "Specifies the maximal latitude of the output."),
                    obl("top", "up",
                        "Specifies the minimal longitude of the output."),
                    obl("bottom", "down",
                        "Specifies the maximal longitude of the output."),

                    opt("allow-empty", "If flagged, the program will not crash if no stops are retained")
                        .SetDefault("false"),
                    opt("allow-empty-connections",
                            "If flagged, the program will not crash if no connections are retained")
                        .SetDefault("false")
                };

        private const bool _isStable = true;

        public SwitchSelectStops() :
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


            var minLon = float.Parse(arguments["left"]);
            var maxLon = float.Parse(arguments["right"]);
            var minLat = float.Parse(arguments["bottom"]);
            var maxLat = float.Parse(arguments["top"]);


            var allowEmpty = bool.Parse(arguments["allow-empty"]);
            var allowEmptyCon = bool.Parse(arguments["allow-empty-connections"]);

            TransitDb Run()
            {
                var old = source.GetTransitDb();

                var filtered = new TransitDb();
                var wr = filtered.GetWriter();


                var stopIdMapping = new Dictionary<(uint, uint), (uint, uint)>();

                var stops = old.Latest.StopsDb.GetReader();
                var copied = 0;
                while (stops.MoveNext())
                {
                    var lat = stops.Latitude;
                    var lon = stops.Longitude;
                    if (
                        !(minLat <= lat && lat <= maxLat && minLon <= lon && lon <= maxLon))
                    {
                        continue;
                    }

                    var newId = wr.AddOrUpdateStop(stops.GlobalId, stops.Longitude, stops.Latitude, stops.Attributes);
                    var oldId = stops.Id;
                    stopIdMapping.Add(oldId, newId);
                    copied++;
                }

                if (!allowEmpty && copied == 0)
                {
                    throw new Exception("There are no stops in the selected bounding box");
                }


                var con = old.Latest.ConnectionsDb.GetReader();
                var trip = old.Latest.TripsDb.GetReader();

                uint index = 0;
                var stopCount = copied;
                copied = 0;

                while (con.MoveTo(index)) 
                {
                    if (!(stopIdMapping.ContainsKey(con.DepartureStop) && stopIdMapping.ContainsKey(con.ArrivalStop)))
                    {
                        // One of the stops is outside of the bounding box
                        index++;
                        continue;
                    }

                    trip.MoveTo(con.TripId); // The old trip
                    var newTripId = wr.AddOrUpdateTrip(trip.GlobalId, trip.Attributes);

                    wr.AddOrUpdateConnection(
                        stopIdMapping[con.DepartureStop],
                        stopIdMapping[con.ArrivalStop],
                        con.GlobalId,
                        con.DepartureTime.FromUnixTime(),
                        con.TravelTime,
                        con.DepartureDelay,
                        con.ArrivalDelay,
                        newTripId,
                        con.Mode
                    );
                    index++;
                    copied++;
                }

                wr.Close();


                if (!allowEmptyCon && copied == 0)
                {
                    throw new Exception("There are no connections in this bounding box");
                }


                Console.WriteLine($"There are {stopCount} stops and {copied} connections in the bounding box");
                return filtered;
            }

            return (new ProcessorTransitDbSource(Run), 1);
        }
    }
}