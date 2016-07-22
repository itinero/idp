using IDP.Processors;
using System;
using System.Collections.Generic;
using System.IO;

namespace IDP.Switches.Osm
{
    /// <summary>
    /// Represents a switch to add a filter to report progress.
    /// </summary>
    class SwitchFilterProgress : Switch
    {
        /// <summary>
        /// Creates a new switch.
        /// </summary>
        public SwitchFilterProgress(string[] arguments)
            : base(arguments)
        {

        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        public static string[] Names
        {
            get
            {
                return new string[] { "--pr", "--progress" };
            }
        }

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            if (!(previous[previous.Count - 1] is Processors.Osm.IProcessorOsmStreamSource))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var progressFilter = new OsmSharp.Streams.Filters.OsmStreamFilterProgress();
            progressFilter.RegisterSource((previous[previous.Count - 1] as Processors.Osm.IProcessorOsmStreamSource).Source);
            processor = new Processors.Osm.ProcessorOsmStreamSource(progressFilter);

            return 1;
        }
    }
}
