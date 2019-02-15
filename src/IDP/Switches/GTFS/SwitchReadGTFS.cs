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
using GTFS;
using GTFS.IO;
using IDP.Processors;
using IDP.Processors.GTFS;

namespace IDP.Switches.GTFS
{
    /// <summary>
    /// Represents a switch to read a GTFS feed.
    /// </summary>
    class SwitchReadGTFS : DocumentedSwitch
    {
        private static string[] _names = {"--rg", "--read-gtfs"};

        private static string _about = "Read a GTFS-datastream to route over public transport networks.";

        private static bool _isStable = false;


        private static List<(List<string> argName, bool isObligated, string comment, string defaultValue)> extraParams
            = new List<(List<string> argName, bool isObligated, string comment, string defaultValue)>
            {
                SwitchesExtensions.obl("directory", "The directory where the GTFS-feed is saved")
            };


        public SwitchReadGTFS()
            : base(_names, _about,
                extraParams,
                _isStable)
        {
        }

        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var directory = new DirectoryInfo(arguments["directory"]);
            if (!directory.Exists)
            {
                throw new FileNotFoundException("Directory not found.", directory.FullName);
            }

            // create the reader.
            var reader = new GTFSReader<GTFSFeed>(false);

            // build the get GTFS function.
            GTFSFeed GetGtfs()
            {
                return reader.Read(new GTFSDirectorySource(directory));
            }

            return (new ProcessorGTFSSource(GetGtfs), 0);
        }
    }
}