using System;
using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Attributes;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Transit
{
    class SwitchDumpTransitDbConnections : DocumentedSwitch
    {
        private static readonly string[] _names = {"--dump-connections"};

        private const string _about = "Writes all connections contained in a transitDB to console";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>()
                {
                    opt("file", "The file to write the data to, in .csv format")
                        .SetDefault("")
                };

        private const bool _isStable = true;


        public SwitchDumpTransitDbConnections
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

            var writeTo = arguments["file"];

            TransitDb Run()
            {
                var tdb = source.GetTransitDb();
                using (var outStream =
                    string.IsNullOrEmpty(writeTo) ? Console.Out : new StreamWriter(File.OpenWrite(writeTo)))
                {
                    const string header = "GlobalId,DepartureStop,DepartureStopName,ArrivalStop,ArrivalStopName," +
                                          "DepartureTime,DepartureDelay,ArrivalTime,ArrivalDelay,TravelTime,Mode,TripId,TripHeadSign";
                    outStream.WriteLine(header);


                    var cons = tdb.Latest.ConnectionsDb.GetReader();
                    var dep = tdb.Latest.StopsDb.GetReader();
                    var arr = tdb.Latest.StopsDb.GetReader();
                    var trip = tdb.Latest.TripsDb.GetReader();

                    uint index = 0;

                    while (cons.MoveTo(index))
                    {
                        dep.MoveTo(cons.DepartureStop);
                        arr.MoveTo(cons.ArrivalStop);
                        trip.MoveTo(cons.TripId);                        
                        
                        var value = $"{cons.GlobalId}," +
                                    $"{dep.GlobalId}," +
                                    $"{dep.Attributes.Get("name")}," +
                                    $"{arr.GlobalId}," +
                                    $"{arr.Attributes.Get("name")}," +
                                    $"{cons.DepartureTime.FromUnixTime():O}," +
                                    $"{cons.DepartureDelay}," +
                                    $"{cons.ArrivalTime.FromUnixTime():O}," +
                                    $"{cons.ArrivalDelay}," +
                                    $"{cons.Mode},"+
                                    $"{trip.GlobalId}," +
                                    $"{trip.Attributes.Get("headsign")}";

                        outStream.WriteLine(value);

                        index++;
                    }
                }

                return tdb;
            }

            return (new ProcessorTransitDbSource(Run), 1);
        }
    }

    internal static class Helpers
    {
        public static string Get(this IAttributeCollection attributes, string name)
        {
            attributes.TryGetValue(name, out var result);
            return result;
        }
    }
}