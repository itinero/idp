using System;
using System.Collections.Generic;
using System.Linq;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Transit
{
    class SwitchDumpTransitDbLocations : DocumentedSwitch
    {
        private static readonly string[] _names = {"--dump-locations"};

        private static string _about = "Writes all stops contained in a transitDB to console";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                };

        private const bool _isStable = false;


        public SwitchDumpTransitDbLocations
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

                var stops = tdb.Latest.StopsDb.GetReader();


                var knownAttributes = new List<string>();
                while (stops.MoveNext())
                {
                    var attributes = stops.Attributes;
                    foreach (var attribute in attributes)
                    {
                        if (!knownAttributes.Contains(attribute.Key))
                        {
                            knownAttributes.Add(attribute.Key);
                        }
                    }
                }


                var header = "globalId,Latitude,Longitude,internalId";
                foreach (var knownAttribute in knownAttributes)
                {
                    header += "," + knownAttribute;
                }
                Console.WriteLine(header);


                stops = tdb.Latest.StopsDb.GetReader();
                while (stops.MoveNext())
                {

                    var value = $"{stops.GlobalId},{stops.Latitude}, {stops.Longitude},{stops.Id}";
                   
                    var attributes = stops.Attributes;
                    foreach (var attribute in knownAttributes)
                    {
                        attributes.TryGetValue(attribute, out var val);
                        value += $",{val ?? ""}";
                    }

                    Console.WriteLine(value);
                }

                return tdb;
            }

            return (new ProcessorTransitDbSource(Run), 1);
        }
    }
}