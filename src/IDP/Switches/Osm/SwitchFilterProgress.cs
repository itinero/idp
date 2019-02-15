using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.Osm;
using OsmSharp.Streams.Filters;

namespace IDP.Switches.Osm
{
    class SwitchFilterProgress : DocumentedSwitch
    {
        private static readonly string[] _names = {"--progress-report", "--progress", "--pr"};

        private const string _about =
            "If this flag is specified, the progress will be printed to standard out. Useful to see how quickly the process goes and to do a bit of initial troubleshooting.";

        private static readonly List<(List<string> argName, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string>argName, bool isObligated, string comment, string defaultValue)>();

        private const bool _isStable = true;

        public SwitchFilterProgress() : base(_names, _about, _extraParams, _isStable)
        {
        }

        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {

            var progressFilter = new OsmStreamFilterProgress();
            if (!(previous[previous.Count - 1] is IProcessorOsmStreamSource source))
            {
                throw new Exception("Expected an OSM stream source.");
            }
            
            
            progressFilter.RegisterSource(source.Source);
            return (new ProcessorOsmStreamSource(progressFilter), 1);
        }
    }
}