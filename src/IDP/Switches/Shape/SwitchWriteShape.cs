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
using Itinero.IO.Shape;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace IDP.Switches.Shape
{
    /// <summary>
    /// A switch to write a shapefile.
    /// </summary>
    class SwitchWriteShape : DocumentedSwitch
    {
        private static string[] _names = {"--write-shape"};

        private static string about = "Write the result as shapefile";

        private static readonly List<(string argName, bool isObligated, string comment)> ExtraParams =
            new List<(string argName, bool isObligated, string comment)>()
            {
                ("file", true, "The output file to write to"),
            };

        public SwitchWriteShape()
            : base(_names, about, ExtraParams, true)
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

            if (!(previous[previous.Count - 1] is IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }

            var source = (previous[previous.Count - 1] as IProcessorRouterDbSource);

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                var profiles = new List<Profile>();
                foreach (var vehicle in routerDb.GetSupportedVehicles())
                {
                    profiles.Add(vehicle.Fastest());
                }

                routerDb.WriteToShape(file.FullName, profiles.ToArray());
                return routerDb;
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}