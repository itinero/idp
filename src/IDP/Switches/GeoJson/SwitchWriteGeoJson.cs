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
using Itinero;

namespace IDP.Switches.GeoJson
{
    /// <summary>
    /// A switch to write geojson.
    /// </summary>
    class SwitchWriteGeoJson : DocumentedSwitch
    {
        /// <summary>
        /// Gets the names.
        /// </summary>
        private static string[] _names => new[] {"--write-geojson"};

        private static readonly List<(string argName, bool isObligated, string comment)> Parameters =
            new List<(string argName, bool isObligated, string comment)>
            {
                ("file", true, "The output file which will contain the geojson. Will be overriden by the code"),
                ("left",   false, "Specifies the minimal latitude of the output. Used when specifying a bounding box for the output."),
                ("right",  false, "Specifies the maximal latitude of the output. Used when specifying a bounding box for the output."),
                ("top",    false, "Specifies the minimal longitude of the output. Used when specifying a bounding box for the output."),
                ("bottom", false, "Specifies the maximal longitude of the output. Used when specifying a bounding box for the output."),
            };


        private const bool IsStable = true;


        /// <inheritdoc />
        /// <summary>
        /// Creates a switch to write a geojson.
        /// </summary>
        private SwitchWriteGeoJson(string[] a)
            : base(a, _names, Parameters, IsStable)
        {
        }
        
        public SwitchWriteGeoJson()
            : base(_names, Parameters, IsStable)
        {
        }

        public override DocumentedSwitch SetArguments(string[] arguments)
        {
            return new SwitchWriteGeoJson(arguments);
        }

        /// <inheritdoc />
        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override Processor Parse(Dictionary<string, string> args, List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException(
                    "Expected at least one other argument before this one.");
            }

            if (!(previous[previous.Count - 1] is IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }


            var source = (previous[previous.Count - 1] as IProcessorRouterDbSource);

            var file = new FileInfo(args["file"]);


            var bounds =
                (args.ContainsKey("left") ? 1 : 0)
                + (args.ContainsKey("left") ? 1 : 0)
                + (args.ContainsKey("left") ? 1 : 0)
                + (args.ContainsKey("left") ? 1 : 0);
            if (bounds > 0 && bounds < 4)
            {
                throw new ArgumentException("When specifying bounds, give all arguments\n" + Help());
            }


            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                using (var stream = file.Open(FileMode.Create))
                using (var textStream = new StreamWriter(stream))
                {
                    if (bounds == 4)
                    {
                        var minLon = float.Parse(args["left"]);
                        var maxLon = float.Parse(args["right"]);
                        var minLat = float.Parse(args["bottom"]);
                        var maxLat = float.Parse(args["top"]);

                        if (minLat < -90)
                        {
                            throw new ArgumentException("Minimum latitude is out of range (< -90)");
                        }
                        
                        if (maxLat >  90)
                        {
                            throw new ArgumentException("Maximum latitude is out of range (>  90)");
                        }
                        
                        if (minLon < -180)
                        {
                            throw new ArgumentException("Minimum longitude is out of range (< -180)");
                        }
                        
                        if (maxLat > 180)
                        {
                            throw new ArgumentException("Maximum longitude is out of range (> 180)");
                        }
                        
                        if (minLat > maxLat)
                        {
                            throw new ArgumentException("The minimum latitude (bottom) is bigger then the maximum latitude (top)");
                        }
                        
                        if (minLon > maxLon)
                        {
                            throw new ArgumentException("The minimum longitude (left) is bigger then the maximum longitude (right)");
                        }
                        
                        routerDb.WriteGeoJson(textStream, minLat, minLon, maxLat, maxLon);
                    }
                    else
                    {
                        routerDb.WriteGeoJson(textStream);
                    }
                }

                return routerDb;
            }

            return new ProcessorRouterDbSource(GetRouterDb);
        }
    }
}