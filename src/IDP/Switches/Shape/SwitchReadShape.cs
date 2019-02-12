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
using Itinero.Data.Edges;
using Itinero.IO.Shape;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Shape
{
    /// <summary>
    /// Represents a switch to read a shapefile for routing.
    /// </summary>
    class SwitchReadShape : DocumentedSwitch
    {
        public static readonly string[] Names = {"--read-shape", "--rs"};

        private static string about = "Read a shapefile as input to do all the data processing.";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            ExtraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>()
                {
                    obl("file", "The input file to read"),
                    obl("vehicle", "The profile to read. This can be a comma-separated list too."),
                    obl("svc", "The `source-vertex-column`"), // TODO Clarify this
                    obl("tvc", "The `target-vertex-column`") // TODO Clarify this
                };

        private const bool IsStable = true;


        public SwitchReadShape()
            : base(Names, about, ExtraParams, IsStable)
        {
        }


        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var localShapefile = arguments["file"];
            var vehicles = arguments.ExtractVehicleArguments();
            var sourceVertexColumn = arguments.GetOrDefault("svc", "");
            var targetVertexColumn = arguments.GetOrDefault("tvc", "");

            if (vehicles.Count == 0)
            {
                throw new ArgumentException("At least one vehicle expected.");
            }

            if (string.IsNullOrWhiteSpace(sourceVertexColumn))
            {
                throw new ArgumentException("Source vertex column not defined.");
            }

            if (string.IsNullOrWhiteSpace(targetVertexColumn))
            {
                throw new ArgumentException("Target vertex column not defined.");
            }

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = new Itinero.RouterDb(EdgeDataSerializer.MAX_DISTANCE);
                var file = new FileInfo(localShapefile);
                routerDb.LoadFromShape(file.DirectoryName, file.Name, sourceVertexColumn, targetVertexColumn,
                    vehicles.ToArray());

                return routerDb;
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 0);
        }
    }
}