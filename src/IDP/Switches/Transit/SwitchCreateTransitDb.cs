using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
using Itinero.Transit.IO.LC;

namespace IDP.Switches.Transit
{
    using static SwitchesExtensions;

    internal class SwitchCreateTransitDb : DocumentedSwitch
    {
        private static readonly string[] _names = {"--create-transit-db", "--create-transit", "--ct"};

        private static string _about =
            "Creates or updates a transit DB based on linked connections. For this, the linked connections source and a timewindow should be specified.\n" +
            "If the previous switch reads or creates a transit db as well, the two transitDbs are merged into a single one.\n\n" +
            "Note that this switch only downloads the connections and keeps them in memory. To write them to disk, add --write-transit-db too";


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
                    opt("window-duration", "duration", "The length of the window to load, in seconds")
                        .SetDefault("3600")
                };

        private const bool _isStable = false;


        public SwitchCreateTransitDb()
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
                Itinero.Transit.Logging.Logger.LogAction = 
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