using System;
using System.Collections.Generic;
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

        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments, List<Processor> previous)
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

                var header = "stops.GlobalId, Latitude, Longitude, Id";
                var attributesHeader = "";

                Console.Write("Id, ");
                while (stops.MoveNext())
                {

                    var attributes = stops.Attributes;
                    if (string.IsNullOrEmpty(attributesHeader))
                    {
                        foreach (var attribute in attributes)
                        {
                            attributesHeader += $", {attribute.Key}";
                        }
                        Console.WriteLine(header+attributesHeader);
                    }
                    
                    var value = $"{stops.GlobalId},{stops.Latitude}, {stops.Longitude},{stops.Id}";
                    foreach (var attribute in attributes)
                    {
                        value += $", {attribute.Value}";
                    }
                    Console.WriteLine(value);
                }

                return tdb;
            }

            return (new ProcessorTransitDbSource(Run), 1);
            
            
            
        }
    }
}