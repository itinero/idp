using IDP.Processors;
using System;
using System.Collections.Generic;
using System.IO;

namespace IDP.Switches.Osm
{
    class SwitchFilterProgress : DocumentedSwitch
    {
        private static readonly string[] names = {"--progress-report", "--progress", "--pr"};

        private const string about = "If this flag is specified, the progress will be printed to standard out. Useful to see how quickly the process goes and to do a bit of initial troubleshooting.";

        private static readonly List<(string argName, bool isObligated, string comment)> ExtraParams = 
          new List<(string argName, bool isObligated, string comment)>();

        private const bool IsStable = true;

        public SwitchFilterProgress() : base(names, about, ExtraParams, IsStable)
        {
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (!(previous[previous.Count - 1] is Processors.Osm.IProcessorOsmStreamSource))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var progressFilter = new OsmSharp.Streams.Filters.OsmStreamFilterProgress();
            var source = previous[previous.Count - 1] as Processors.Osm.IProcessorOsmStreamSource;
            progressFilter.RegisterSource(source.Source);
            return (new Processors.Osm.ProcessorOsmStreamSource(progressFilter), 1);
        }

      
    }
}