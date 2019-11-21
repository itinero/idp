using System;
using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Osm
{
    internal class SwitchApplyChanges : DocumentedSwitch
    {
        private static readonly string[] _names = {"--apply-changes", "--ac"};
        private const string _about = "Applies an osmChange file to the source stream and outputs the modified stream.";

        private static readonly List<(List<string> argName, bool isObligated, string comment, string defaultValue)> _extraParams
            = new List<(List<string>argName, bool isObligated, string comment, string defaultValue)>
            {
                obl("osc", "The .osc or .osc.gz osmChange file.")
            };

        private const bool _isStable = true;

        public SwitchApplyChanges() : base(_names, _about, _extraParams, _isStable)
        {
            
        }


        /// <summary>
        /// Parses this command into a processor given the arguments for this switch.
        /// Consumes the previous processors and returns how many it consumes.
        /// </summary>
        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var localFile = Downloader.DownloadOrOpen(arguments["osc"]);
            var file = new FileInfo(localFile);
            if (!file.Exists)
            {
                throw new FileNotFoundException("File not found.", file.FullName);
            }

            if (file.Name.EndsWith(".gz"))
            {
                throw new NotSupportedException("GZIP not supported.");
            }

            var osmChange = 
//            var source = new XmlOsmStreamSource(file.OpenRead());
//            return (new ProcessorOsmStreamSource(source), 0);
        }
    }
}