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

using System;
using System.Collections.Generic;
using IDP.Processors;
using System.IO;
using Itinero;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to read or open a routerdb.
    /// </summary>
    internal class SwitchReadRouterDb : DocumentedSwitch
    {
        private static string[] _names => new[] {"--read-routerdb"};

        private static string about =
            "Reads a routerdb file for processing, e.g. to translate it to a geojson.";


        private static readonly List<(string argName, bool isObligated, string comment)> Parameters =
            new List<(string argName, bool isObligated, string comment)>
            {
                ("file", true, "The path where the routerdb should be read."),
                ("mapped", false,
                    "Enable memory-mapping: only fetch the parts from disk that are needed. There is less memory used, but the queries are slower. Use 'mapped=true'"),
                ("m", false, "Same as 'mapped'.")
            };


        private const bool IsStable = true;


        public SwitchReadRouterDb()
            : base(_names, about, Parameters, IsStable)
        {
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var fileName = string.Empty;
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
                    Itinero.Logging.Logger.Log(nameof(SwitchReadRouterDb), Itinero.Logging.TraceEventType.Information,
                        "Opening RouterDb: " + file.FullName);
                    var stream = file.OpenRead();
                    return Itinero.RouterDb.Deserialize(stream, RouterDbProfile.NoCache);
                }

                
                
                // load the entire routerdb in RAM.
                Itinero.Logging.Logger.Log(nameof(SwitchReadRouterDb), Itinero.Logging.TraceEventType.Information,
                    "Reading RouterDb: " + file.FullName);
                using (var stream = file.OpenRead())
                {
                    return Itinero.RouterDb.Deserialize(stream);
                }
            }

            return (new Processors.RouterDb.ProcessorRouterDbSource(GetRouterDb), 0);
        }
    }
}