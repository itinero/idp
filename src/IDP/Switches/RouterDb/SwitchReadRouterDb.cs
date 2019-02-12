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
using IDP.Processors.RouterDb;
using Itinero;
using Itinero.Logging;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.RouterDb
{
    /// <inheritdoc />
    /// <summary>
    /// A switch to read or open a routerdb.
    /// </summary>
    internal class SwitchReadRouterDb : DocumentedSwitch
    {
        private static string[] _names => new[] {"--read-routerdb"};

        private static string about =
            "Reads a routerdb file for processing. This can be useful to e.g. translate it to a geojson or shapefile.";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)> Parameters =
            new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            {
                obl("file", "The path where the routerdb should be read."),
                opt("mapped", "m",
                        "Enable memory-mapping: only fetch the parts from disk that are needed. There is less memory used, but the queries are slower.")
                    .SetDefault("false"),
            };


        private const bool IsStable = true;


        public SwitchReadRouterDb()
            : base(_names, about, Parameters, IsStable)
        {
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var fileName = arguments["file"];
            var mapped = (arguments.ContainsKey("mapped") && SwitchParsers.IsTrue(arguments["mapped"]) ||
                          (arguments.ContainsKey("m") && SwitchParsers.IsTrue(arguments["m"])));


            // check if the file exists.
            var localFile = Downloader.DownloadOrOpen(fileName);
            var file = new FileInfo(localFile);

            if (!file.Exists)
            {
                throw new FileNotFoundException("File not found.", file.FullName);
            }

            Itinero.RouterDb GetRouterDb()
            {
                if (mapped)
                {
                    // use the mapped version of the routerdb.
                    // WARNING: the source stream will remain open and cannot be written to.
                    Logger.Log(nameof(SwitchReadRouterDb), TraceEventType.Information,
                        "Opening RouterDb: " + file.FullName);
                    var stream = file.OpenRead();
                    return Itinero.RouterDb.Deserialize(stream, RouterDbProfile.NoCache);
                }


                // load the entire routerdb in RAM.
                Logger.Log(nameof(SwitchReadRouterDb), TraceEventType.Information,
                    "Reading RouterDb: " + file.FullName);
                using (var stream = file.OpenRead())
                {
                    return Itinero.RouterDb.Deserialize(stream);
                }
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 0);
        }
    }
}