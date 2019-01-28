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
using Itinero.Profiles;
using Itinero.Algorithms.Search.Hilbert;
using System.IO;
using System.Runtime.CompilerServices;
using IDP.Processors.RouterDb;
using Itinero.Algorithms.Networks;
using OsmSharp.Streams;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to create a router db.
    /// </summary>
    class SwitchCreateRouterDb : DocumentedSwitch
    {
        private static readonly string[] names = {"--create-routerdb"};

        private static string about = "Converts an input source (such as an `osm`-file) into a routable graph. If no vehicle is specified, `car` is used.\n" +
                                      "If the routing graph should be built for another vehicle, the `vehicle`-parameter can be used\n\n" +
                                      "1) specify a **file** containing a routing profile [(examples in our repository)](https://github.com/anyways-open/routing-profiles/), or...\n" + 
                                      "2) a **built-in** profile can be used. This should be one of:\n\n" +
                                      " - `Bicycle`\n" +
                                      " - `BigTruck`\n" +
                                      " - `Bus`\n" +
                                      " - `Car`\n" +
                                      " - `Moped`\n" +
                                      " - `MotorCycle`\n" +
                                      " - `Pedestrian`\n" +
                                      " - `SmallTruck`\n" +
                                      "\n" +
                                      "Additionally, there are two special values:\n\n" +
                                      "- `all`: Adds all of the above vehicles to the routing graph\n" +
                                      "- `motors `(or `motorvehicles`): adds all motor vehicles to the routing graph\n\n" +
                                      "Note that one can specify multiple vehicles at once too, using the `vehicles` parameter (note the plural)";


        private static readonly List<(string argName, bool isObligated, string comment)> ExtraParams =
            new List<(string argName, bool isObligated, string comment)>()
            {
                ("vehicle", false,
                    "The vehicle that the routing graph should be built for. Default is 'car'."),
                ("vehicles", false, "A comma separated list containing vehicles that should be used"),
                ("keepwayids", false, "Boolean indicating that the way IDs should be kept"), // TODO Clarify this
                ("wayids", false, "Same as `keepwayids`"),
                ("allcore", false, "Boolean indicating allcore"), // TODO WTF? Clarify this
                ("simplification", false,
                    "Integer indicating the simplification factor. Default: very small"), // TODO Clarify this
            };

        private const bool IsStable = true;


        /// <summary>
        /// Creates a switch to create a router db.
        /// </summary>
        public SwitchCreateRouterDb()
            : base(names, about, ExtraParams, IsStable)
        {
        }


        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            // Various options parsing
            var allCore = arguments.ContainsKey("allcore")
                          && SwitchParsers.IsTrue(arguments["allcore"]);
            var keepWayIds = (arguments.ContainsKey("keepwayids")
                              && SwitchParsers.IsTrue(arguments["keepwayids"]))
                             || (arguments.ContainsKey("wayids")
                                 && SwitchParsers.IsTrue(arguments["wayids"]));

            var simplification = (new Itinero.IO.Osm.LoadSettings()).NetworkSimplificationEpsilon;
            if (arguments.TryGetValue("simplification", out var sArg))
            {
                var parsedInt = SwitchParsers.Parse(sArg);
                if (parsedInt != null)
                {
                    simplification = parsedInt.Value;
                }
            }


            if (arguments.ContainsKey("vehicle") && arguments.ContainsKey("vehicles"))
            {
                throw new ArgumentException(
                    "Do not specify both `vehicle` and `vehicles`. Add the sole vehicle to the list.");
            }

            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();
            string vehiclesArg;
            if (arguments.ContainsKey("vehicle"))
            {
                vehiclesArg = arguments["vehicle"];
            }
            else if (arguments.ContainsKey("vehicles"))
            {
                vehiclesArg = arguments["vehicles"];
            }
            else
            {
                vehiclesArg = "car";
            }


            string[] vehicleNames;
            if (!SwitchParsers.SplitValuesArray(vehiclesArg.ToLower(), out vehicleNames))
            {
                // No commas found or something
                vehicleNames = new[] {vehiclesArg};
            }

            // The resulting list containing everything
            var vehicles = new List<Vehicle>();

            foreach (var vehicleName in vehicleNames)
            {
                var name = vehicleName.ToLower();
                if (Vehicle.TryGet(name, out var vehicle))
                {
                    vehicles.Add(vehicle);
                    continue;
                }

                if (name.Equals("all"))
                {
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Bicycle);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.BigTruck);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Bus);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Car);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Moped);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.MotorCycle);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Pedestrian);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.SmallTruck);
                    continue;
                }


                if (name.Equals("motors") || name.Equals("motorvehicles"))
                {
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.BigTruck);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Bus);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Car);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.MotorCycle);
                    vehicles.Add(Itinero.Osm.Vehicles.Vehicle.SmallTruck);
                    continue;
                }


                // Assume that this is a filename

                // assume a filename.
                var vehicleFile = new FileInfo(name);
                if (!vehicleFile.Exists)
                {
                    throw new FileNotFoundException($"Loading the vehicle profile {name} failed. This profile was not recognized as a built-in profile, neither was it found as a file. Profiles can be found online: https://github.com/anyways-open/routing-profiles/", name);
                }

                using (var stream = vehicleFile.OpenRead())
                {
                    vehicles.Add( DynamicVehicle.LoadFromStream(stream));
                }
            }

            // All arguments have been set up!
            
            // Only thing left to do: grab an OSM stream source and actually get stuff done
            
            if (!(previous[previous.Count - 1] is Processors.Osm.IProcessorOsmStreamSource))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var source = (previous[previous.Count - 1] as Processors.Osm.IProcessorOsmStreamSource).Source;
            return CreateProcessor(source, vehicles, allCore, keepWayIds, simplification);
        }

        private (ProcessorRouterDbSource, int) CreateProcessor(OsmStreamSource source, List<Vehicle> vehicles,
            bool allCore, bool keepWayIds, float simplification)
        {
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                var routerDb = new Itinero.RouterDb();

                // make sure the routerdb can handle multiple edges.
                routerDb.Network.GeometricGraph.Graph.MarkAsMulti();

                // load the data.
                var target = new Itinero.IO.Osm.Streams.RouterDbStreamTarget(routerDb,
                    vehicles.ToArray(), allCore, processRestrictions: true);
                if (keepWayIds)
                {
                    // add way id's.
                    var eventsFilter = new OsmSharp.Streams.Filters.OsmStreamFilterDelegate();
                    eventsFilter.MoveToNextEvent += EventsFilter_AddWayId;
                    eventsFilter.RegisterSource(source);
                    target.RegisterSource(eventsFilter, false);
                }
                else
                {
                    // use the source as-is.
                    target.RegisterSource(source);
                }

                target.Pull();

                // optimize the network for routing.
                routerDb.SplitLongEdges();
                routerDb.ConvertToSimple();

                // sort the network.
                routerDb.Sort();

                // optimize the network if requested.
                if (simplification != 0)
                {
                    routerDb.OptimizeNetwork(simplification);
                }

                // compress the network.
                routerDb.Network.Compress();

                return routerDb;
            };
            return (new ProcessorRouterDbSource(getRouterDb), 1);
        }


        private static OsmSharp.OsmGeo EventsFilter_AddWayId(OsmSharp.OsmGeo osmGeo, object param)
        {
            if (osmGeo.Type == OsmSharp.OsmGeoType.Way)
            {
                osmGeo.Tags.Add("way_id", osmGeo.Id.ToString());
            }

            return osmGeo;
        }
    }
}