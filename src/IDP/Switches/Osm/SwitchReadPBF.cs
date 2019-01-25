// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using IDP.Processors.Osm;
using OsmSharp.Streams;

namespace IDP.Switches.Osm
{
    class SwitchReadPBF : DocumentedSwitch
    {
        private static readonly string[] names = {"--read-pbf", "--rb"};
        private static string about = "Read an .osm.pbf file to serve as input";

        private static readonly List<(string argName, bool isObligated, string comment)> extraParams
            = new List<(string argName, bool isObligated, string comment)>()
            {
                ("file", true, "The .osm.pbf file that serves as input")
            };

        private const bool IsStable = true;

        public SwitchReadPBF() : base(names, about, extraParams, IsStable)
        {
        }


        /// <summary>
        /// Parses this command into a processor given the arguments for this switch.
        /// Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var localFile = Downloader.DownloadOrOpen(arguments["file"]);
            var file = new FileInfo(localFile);
            if (!file.Exists)
            {
                throw new FileNotFoundException("File not found.", file.FullName);
            }

            var pbfSource = new PBFOsmStreamSource(file.OpenRead());
            return (new ProcessorOsmStreamSource(pbfSource), 0);
        }
    }
}