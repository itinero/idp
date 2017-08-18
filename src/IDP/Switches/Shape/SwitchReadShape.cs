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
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace IDP.Switches.Shape
{
    /// <summary>
    /// Represents a switch to read a shapefile for routing.
    /// </summary>
    class SwitchReadShape : Switch
    {
        /// <summary>
        /// Creates a new switch.
        /// </summary>
        public SwitchReadShape(string[] arguments)
            : base(arguments)
        {

        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        public static string[] Names
        {
            get
            {
                return new string[] { "--rs", "--read-shape" };
            }
        }

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            if (this.Arguments.Length < 2) { throw new ArgumentException("At least two arguments are expected."); }

            var localShapefile = this.Arguments[0];
            if (!File.Exists(localShapefile))
            {
                throw new FileNotFoundException("File not found.", localShapefile);
            }

            var vehicles = new List<Vehicle>();
            var sourceVertexColumn = string.Empty;
            var targetVertexColumn = string.Empty;

            for (var i = 0; i < this.Arguments.Length; i++)
            {
                string key, value;
                if (SwitchParsers.SplitKeyValue(this.Arguments[i], out key, out value))
                {
                    switch (key.ToLower())
                    {
                        case "vehicles":
                        case "vehicle":
                            string[] vehicleValues;
                            if (SwitchParsers.SplitValuesArray(value.ToLower(), out vehicleValues))
                            { // split the values array.
                                vehicles = new List<Vehicle>(vehicleValues.Length);
                                for (int v = 0; v < vehicleValues.Length; v++)
                                {
                                    Vehicle vehicle;
                                    if (!Vehicle.TryGet(vehicleValues[v], out vehicle))
                                    {// assume a filename.
                                        var vehicleFile = new FileInfo(vehicleValues[v]);
                                        if (!vehicleFile.Exists)
                                        {
                                            throw new SwitchParserException("--create-routerdb",
                                                string.Format("Invalid parameter value for command --create-routerdb: Vehicle profile '{0}' not found.",
                                                    vehicleValues[v]));
                                        }
                                        using (var stream = vehicleFile.OpenRead())
                                        {
                                            vehicle = DynamicVehicle.LoadFromStream(stream);
                                            vehicle.Register();
                                            vehicles.Add(vehicle);
                                        }
                                    }
                                }
                            }
                            break;
                        case "source-vertex-column":
                        case "svc":
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                sourceVertexColumn = value;
                            }
                            break;
                        case "target-vertex-column":
                        case "tvc":
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                targetVertexColumn = value;
                            }
                            break;
                        default:
                            throw new SwitchParserException("--read-shape",
                                string.Format("Invalid parameter for command --read-shape: {0} not recognized.", key));
                    }
                }
            }

            if (vehicles.Count == 0)
            {
                throw new Exception("At least one vehicle expected.");
            }
            if (string.IsNullOrWhiteSpace(sourceVertexColumn))
            {
                throw new Exception("Source vertex column not defined.");
            }
            if (string.IsNullOrWhiteSpace(targetVertexColumn))
            {
                throw new Exception("Target vertex column not defined.");
            }
            
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                throw new NotImplementedException("Not implemented yet, waiting for a .NET Standard supported implementation.");
            };
            processor = new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb);
            return 0;
        }
    }
}
