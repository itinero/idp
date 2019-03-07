using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Attributes;

namespace IDP.Switches.Transit
{
    class SwitchDumpTransitDbConnections : DocumentedSwitch
    {
        private static readonly string[] _names = {"--dump-connections"};

        private const string _about = "Writes all connections contained in a transitDB to console";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>();

        private const bool _isStable = false;


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

            TransitDb Run()
            {
                var tdb = source.GetTransitDb();


                var header = "GlobalId,GlobalId,Attributes,GlobalId,Attributes,DepartureTime,DepartureDelay,ArrivalTime,ArrivalDelay,TravelTime,TripId";
                Console.WriteLine(header);
                
                
                var cons = tdb.Latest.ConnectionsDb.GetReader();
                var dep = tdb.Latest.StopsDb.GetReader();
                var arr = tdb.Latest.StopsDb.GetReader();


                uint index = 0;
                while (cons.MoveTo(index))
                {
                    dep.MoveTo(cons.DepartureStop);
                    arr.MoveTo(cons.ArrivalStop);
                    var value = $"{cons.GlobalId}," +
                                $"{dep.GlobalId}" +
                                $"{dep.Attributes.Get("Name")}" +
                                $"{arr.GlobalId}" +
                                $"{arr.Attributes.Get("Name")}" +
                                $"{cons.DepartureTime}," +
                                $"{cons.DepartureDelay}," +
                                $"{cons.ArrivalTime}," +
                                $"{cons.ArrivalDelay}," +
                                $"{cons.TravelTime}," +
                                $"{cons.TripId}";

                    Console.WriteLine(value);

                    index++;
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