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
using System.IO;
using IDP.Processors;
using IDP.Processors.RouterDb;
using Itinero.IO.Shape;
using Itinero.Profiles;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Shape
{
    /// <summary>
    /// A switch to write a shapefile.
    /// </summary>
    class SwitchWriteShape : DocumentedSwitch
    {
        private static readonly string[] _names = {"--write-shape"};

        private const string _about = "Write the result as shapefile";

        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)> _extraParams =
            new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            {
              obl  ("file", "The output file to write to")
            };

        public SwitchWriteShape()
            : base(_names, _about, _extraParams, true)
        {
        }


        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            var file = new FileInfo(arguments["file"]);

            if (!(previous[previous.Count - 1] is IProcessorRouterDbSource source))
            {
                throw new Exception("Expected a router db source.");
            }

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