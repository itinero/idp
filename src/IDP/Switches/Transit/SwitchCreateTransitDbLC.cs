using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
using Itinero.Transit.IO.LC;
using Itinero.Transit.Logging;

namespace IDP.Switches.Transit
{
    using static SwitchesExtensions;

    internal class SwitchCreateTransitDbLC : DocumentedSwitch
    {
        private static readonly string[] _names =
            {"--create-transit-db-with-linked-connections", "--create-transit-lc", "--ctlc"};

        private static string _about =
            "Creates a transit DB based on linked connections (or adds them to an already existing db). For this, the linked connections source and a timewindow should be specified.\n" +
            "If the previous switch reads or creates a transit db as well, the two transitDbs are merged into a single one.\n\n" +
            "Note that this switch only downloads the connections and keeps them in memory. To write them to disk, add --write-transit-db too.\n\n" +
            "Example usage to create the database for the Belgian Railway (SNCB/NMBS):\n\n" +
            "        idp --create-transit-db https://graph.irail.be/sncb/connections https://irail.be/stations/NMBS";



        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("connections", "curl",
                        "The URL where connections can be downloaded"),
                    obl("locations", "lurl",
                        "The URL where the location can be downloaded"),
                    opt("window-start", "start",
                            "The start of the timewindow to load. Specify 'now' to take the current date and time.")
                        .SetDefault("now"),
                    opt("window-duration", "duration", "The length of the window to load, in seconds. If zero is specified, no connections will be downloaded.")
                        .SetDefault("3600")
                };


        private const bool _isStable = true;
        
  

        public SwitchCreateTransitDbLC()
            : base(_names, _about, _extraParams, _isStable)
        {
        }

        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var curl = arguments["connections"];
            var lurl = arguments["locations"];
            var wStart = arguments["window-start"];
            var time = wStart.Equals("now") ? DateTime.Now : DateTime.Parse(wStart);
            time = time.ToUniversalTime();
            var duration = int.Parse(arguments["window-duration"]);

            IProcessorTransitDbSource source  = null;
            if (previous.Count >= 1)
            {
                if (!(previous[previous.Count - 1] is IProcessorTransitDbSource previousSource))
                {
                    throw new Exception("Expected a transit db source.");
                }

                source = previousSource;
            }

            TransitDb GetTransitDb()
            {
                Logger.LogAction = 
                    (origin, level, message, parameters) => 
                        Console.WriteLine($"[{DateTime.Now:O}] [{level}] [{origin}]: {message}");
                var tdb = source?.GetTransitDb() ?? new TransitDb();
                tdb.UseLinkedConnections(curl, lurl, time, time.AddSeconds(duration));
                return tdb;
            }

            return (new ProcessorTransitDbSource(GetTransitDb), 0);
        }
    }

 
}