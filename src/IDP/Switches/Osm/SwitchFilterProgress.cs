using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.Osm;
using OsmSharp.Streams.Filters;

namespace IDP.Switches.Osm
{
    class SwitchFilterProgress : DocumentedSwitch
    {
        private static readonly string[] names = {"--progress-report", "--progress", "--pr"};

        private const string about =
            "If this flag is specified, the progress will be printed to standard out. Useful to see how quickly the process goes and to do a bit of initial troubleshooting.";

        private static readonly List<(List<string> argName, bool isObligated, string comment, string defaultValue)>
            ExtraParams =
                new List<(List<string>argName, bool isObligated, string comment, string defaultValue)>();

        private const bool IsStable = true;

        public SwitchFilterProgress() : base(names, about, ExtraParams, IsStable)
        {
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (!(previous[previous.Count - 1] is IProcessorOsmStreamSource))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var progressFilter = new OsmStreamFilterProgress();
            var source = previous[previous.Count - 1] as IProcessorOsmStreamSource;
            progressFilter.RegisterSource(source.Source);
            return (new ProcessorOsmStreamSource(progressFilter), 1);
        }
    }
}