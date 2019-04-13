using System;
using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using IDP.Processors.Osm;
using OsmSharp.Streams;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Osm
{
    class SwitchWriteBin : DocumentedSwitch
    {
        private static readonly string[] _names = {"--write-bin"};

        private const string _about = "Writes the result of the calculations as a binary osm file. The file format is `.osm.bin`";

        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams
                = new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("file", "The file to write the .osm.bin to")
                };

        private const bool _isStable = true;

        public SwitchWriteBin() : base(_names, _about, _extraParams, _isStable)
        {
        }

        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            var file = new FileInfo(arguments["file"]);

            if (!(previous[previous.Count - 1] is IProcessorOsmStreamSource source))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var pbfTarget = new BinaryOsmStreamTarget(file.Open(FileMode.Create));
            pbfTarget.RegisterSource(source.Source);
            return (new ProcessorOsmStreamTarget(pbfTarget), 1);
        }
    }
}