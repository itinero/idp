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
using IDP.Processors.RouterDb;
using System;
using System.Collections.Generic;
using System.IO;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to write a router db.
    /// </summary>
    class SwitchWriteRouterDb : DocumentedSwitch
    {
        private static string[] _names => new[] {"--write-routerdb"};

        private static string about =
            "Specifies that the routable graph should be saved to a file. This routerdb can be used later to perform queries.";


        private static readonly List<(string argName, bool isObligated, string comment)> Parameters =
            new List<(string argName, bool isObligated, string comment)>
            {
                ("file", true, "The path where the routerdb should be written."),
            };


        private const bool IsStable = true;


        public SwitchWriteRouterDb()
            : base(_names, about, Parameters, IsStable)
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

            if (!(previous[previous.Count - 1] is Processors.RouterDb.IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                var routerDb = source.GetRouterDb();
                using (var stream = File.Open(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    routerDb.Serialize(stream, true);
                }

                return routerDb;
            };

            return (new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb), 1);
        }
    }
}