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
using IDP.Processors.Osm;
using IDP.Processors.RouterDb;
using Itinero.Algorithms.Networks;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.IO.Osm;
using Itinero.IO.Osm.Streams;
using Itinero.Profiles;
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Streams.Filters;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to create a router db.
    /// </summary>
    class SwitchCreateRouterDb : DocumentedSwitch
    {
        private static readonly string[] _names = {"--create-routerdb"};

        private const string _about = "Converts an input source (such as an `osm`-file) into a routable graph. If no vehicle is specified, `car` is used.\n" + "If the routing graph should be built for another vehicle, the `vehicle`-parameter can be used\n\n" + "1) specify a **file** containing a routing profile [(examples in our repository)](https://github.com/anyways-open/routing-profiles/), or...\n" + "2) a **built-in** profile can be used. This should be one of:\n\n" + " - `Bicycle`\n" + " - `BigTruck`\n" + " - `Bus`\n" + " - `Car`\n" + " - `Moped`\n" + " - `MotorCycle`\n" + " - `Pedestrian`\n" + " - `SmallTruck`\n" + "\n" + "Additionally, there are two special values:\n\n" + "- `all`: Adds all of the above vehicles to the routing graph\n" + "- `motors `(or `motorvehicles`): adds all motor vehicles to the routing graph\n\n" + "Note that one can specify multiple vehicles at once too, using the `vehicles` parameter (note the plural)";


        private static readonly List<(List<string>argName, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> argName, bool isObligated, string comment, string defaultValue)>
                {
                    opt("vehicle", "vehicles",
                            "The vehicle (or comma separated list of vehicles) that the routing graph should be built for.")
                        .SetDefault("car"),
                    opt("keepwayids", "wayids", "If specified, the identifiers in the source data are kept. By default, they are discarded. Specify this flag if the calculated routes will have to be backreferenced to the source data set.")
                        .SetDefault("false"),
                    opt("allcore", "If true, all nodes in the source data will be converted into vertices, even if they have only two neighbours." +
                                   "By default, only nodes at intersections will be kept in the routerdb as vertices. Nodes with only two neighbours are just part of a road and skipped.")
                        .SetDefault("false"),
                    opt("simplification",
                            "Parameter to steer simplification. Simplification removes points from each edge in order to have simpler (yet similar) lines using [Ramer-Doublas-Peucker](https://en.wikipedia.org/wiki/Ramer%E2%80%93Douglas%E2%80%93Peucker_algorithm)")
                        .SetDefault("" + new LoadSettings().NetworkSimplificationEpsilon),
                    opt("normalize",
                            "When building the routerdb, a table is built for each combination of tags. E.g. `highway=residential` will get entry `1`, whereas `highway=residential & access=public` will get entry `2`. If this table has to be kept small, `normalize` can be used. Tags will be rewritten to equivalent forms. In our example, `access=public` will be dropped, as this is implied by `highway=residential`.")
                        .SetDefault("false")
                };

        private const bool _isStable = true;


        /// <summary>
        /// Creates a switch to create a router db.
        /// </summary>
        public SwitchCreateRouterDb()
            : base(_names, _about, _extraParams, _isStable)
        {
        }


        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            // Various options parsing
            var allCore = IsTrue(arguments["allcore"]);
            var keepWayIds = IsTrue(arguments["keepwayids"]);

            var simplification = new LoadSettings().NetworkSimplificationEpsilon;
            if (arguments.TryGetValue("simplification", out var sArg))
            {
                var parsedInt = Parse(sArg);
                if (parsedInt != null)
                {
                    simplification = parsedInt.Value;
                }
            }

            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var vehicles = arguments.ExtractVehicleArguments();

            var normalize = IsTrue(arguments["normalize"]);

            // All arguments have been set up!

            // Only thing left to do: grab an OSM stream source and actually get stuff done

            if (!(previous[previous.Count - 1] is IProcessorOsmStreamSource src))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var source = src.Source;
            return CreateProcessor(source, vehicles, allCore, keepWayIds, simplification, normalize);
        }

        private (ProcessorRouterDbSource, int) CreateProcessor(OsmStreamSource source, List<Vehicle> vehicles,
            bool allCore, bool keepWayIds, float simplification, bool normalize)
        {
            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = new Itinero.RouterDb();

                // make sure the routerdb can handle multiple edges.
                routerDb.Network.GeometricGraph.Graph.MarkAsMulti();

                // load the data.
                var target = new RouterDbStreamTarget(routerDb, vehicles.ToArray(), allCore, processRestrictions: true);
                if (keepWayIds)
                {
                    // add way id's.
                    var eventsFilter = new OsmStreamFilterDelegate();
                    eventsFilter.MoveToNextEvent += EventsFilter_AddWayId;
                    eventsFilter.RegisterSource(source);
                    target.RegisterSource(eventsFilter, normalize);
                }
                else
                {
                    // use the source as-is.
                    target.RegisterSource(source, normalize);
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
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 1);
        }


        private static OsmGeo EventsFilter_AddWayId(OsmGeo osmGeo, object param)
        {
            if (osmGeo.Type == OsmGeoType.Way)
            {
                osmGeo.Tags.Add("way_id", osmGeo.Id.ToString());
            }

            return osmGeo;
        }
    }
}