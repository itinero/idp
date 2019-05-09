using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
using Itinero.Transit.Logging;

namespace IDP.Switches.Transit
{
    using static SwitchesExtensions;

    internal class SwitchCreateTransitDbOSM : DocumentedSwitch
    {
        private static readonly string[] _names =
            {"--create-transit-db-with-open-street-map-relation", "--create-transit-osm", "--ctosm"};

        private static string _about =
            "Creates a transit DB based on an OpenStreetMap-relation following the route scheme (or adds it to an already existing db). For all information on Public Transport tagging, refer to [the OSM-Wiki](https://wiki.openstreetmap.org/wiki/Public_transport).n\n" +
            "A timewindow should be specified to indicate what period the transitDB should cover. \n\n" +
            "Of course, the relation itself should be provided. Either:\n\n - Pass the ID of the relation to download it\n - Pass the URL of a relation.xml\n - Pass the filename of a relation.xml\n\n" +
            "If the previous switch reads or creates a transit db as well, the two transitDbs are merged into a single one.\n\n" +
            "Note that this switch only downloads/reads the relation and keeps them in memory. To write them to disk, add --write-transit-db too.\n\n" +
            "Example usage to create the database:\n\n" +
            "        idp --create-transit-osm 9413958";


        private const bool _isStable = true;

        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("relation", "id",
                        "Either a number, an url (starting with http or https) or a path where the relation can be found"),
                    opt("window-start", "start",
                            "The start of the timewindow to load. Specify 'now' to take the current date and time.")
                        .SetDefault("now"),
                    opt("window-duration", "duration",
                            "The length of the window to load, in seconds. If zero is specified, no connections will be downloaded.")
                        .SetDefault("3600")
                };


        public SwitchCreateTransitDbOSM()
            : base(_names, _about, _extraParams, _isStable)
        {
        }


        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var wStart = arguments["window-start"];

            var durationSeconds = int.Parse(arguments["window-duration"]);

            var start = wStart.Equals("now") ? DateTime.Now : DateTime.Parse(wStart);
            var end = start.AddSeconds(durationSeconds);
            var arg = arguments["relation"];

            IProcessorTransitDbSource source = null;
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


                tdb.UseOsmRoute(arg, start, end);

                return tdb;
            }

            return (new ProcessorTransitDbSource(GetTransitDb), 0);
        }
    }
}