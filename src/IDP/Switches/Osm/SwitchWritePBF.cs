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

using IDP.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using IDP.Processors.Osm;
using IDP.Processors.RouterDb;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Osm
{
    class SwitchWritePBF : DocumentedSwitch
    {
        private static readonly string[] names = {"--write-pbf", "--wb"};

        private static readonly string about =
            "Writes the result of the calculations as protobuff-osm file. The file format is `.osm.pbf`";

        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            extraParams
                = new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("file", "The file to write the .osm.pbf to")
                };

        private const bool isStable = true;

        public SwitchWritePBF() : base(names, about, extraParams, isStable)
        {
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            var file = new FileInfo(arguments["file"]);
            if (!file.Exists)
            {
                throw new FileNotFoundException("File not found.", file.FullName);
            }

            if (!(previous[previous.Count - 1] is Processors.Osm.IProcessorOsmStreamSource))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var pbfTarget = new OsmSharp.Streams.PBFOsmStreamTarget(file.OpenRead());
            pbfTarget.RegisterSource((previous[previous.Count - 1] as Processors.Osm.IProcessorOsmStreamSource).Source);
            return (new Processors.Osm.ProcessorOsmStreamTarget(pbfTarget), 1);
        }
    }
}