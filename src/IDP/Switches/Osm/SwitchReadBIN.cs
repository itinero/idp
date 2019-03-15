using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using IDP.Processors.Osm;
using OsmSharp.Streams;
using static IDP.Switches.SwitchesExtensions;
namespace IDP.Switches.Osm
{
    class SwitchReadBin : DocumentedSwitch
    {
        private static readonly string[] _names = {"--read-bin"};
        private const string _about = "Reads an OpenStreetMap input file. The format should be an `.osm.bin` file.";

        private static readonly List<(List<string> argName, bool isObligated, string comment, string defaultValue)> _extraParams
            = new List<(List<string>argName, bool isObligated, string comment, string defaultValue)>
            {
                obl("file", "The .osm.bin file that serves as input")
            };

        private const bool _isStable = true;

        public SwitchReadBin() : base(_names, _about, _extraParams, _isStable)
        {
        }


        /// <summary>
        /// Parses this command into a processor given the arguments for this switch.
        /// Consumes the previous processors and returns how many it consumes.
        /// </summary>
        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var localFile = Downloader.DownloadOrOpen(arguments["file"]);
            var file = new FileInfo(localFile);
            if (!file.Exists)
            {
                throw new FileNotFoundException("File not found.", file.FullName);
            }

            var pbfSource = new BinaryOsmStreamSource(file.OpenRead());
            return (new ProcessorOsmStreamSource(pbfSource), 0);
        }
    }
}