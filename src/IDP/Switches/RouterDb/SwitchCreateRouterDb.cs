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
using Itinero.Algorithms.Networks;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to create a router db.
    /// </summary>
    class SwitchCreateRouterDb : Switch
    {
        /// <summary>
        /// Creates a switch to create a router db.
        /// </summary>
        public SwitchCreateRouterDb(string[] a)
            : base(a)
        {

        }
        
        /// <summary>
        /// Gets the names.
        /// </summary>
        public static string[] Names
        {
            get
            {
                return new string[] { "--create-routerdb" };
            }
        }

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            var vehicles = new List<Vehicle>(new Vehicle[]
            {
                Itinero.Osm.Vehicles.Vehicle.Car
            });
            var allCore = false;
            var keepWayIds = false;
            var normalize = true;
            var simplification = (new Itinero.IO.Osm.LoadSettings()).NetworkSimplificationEpsilon;
            
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

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
                                    {
                                        if (vehicleValues[v] == "all")
                                        { // all vehicles.
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Bicycle);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.BigTruck);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Bus);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Car);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Moped);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.MotorCycle);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Pedestrian);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.SmallTruck);
                                        }
                                        else if (vehicleValues[v] == "motorvehicle" ||
                                            vehicleValues[v] == "motorvehicles")
                                        { // all motor vehicles.
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.BigTruck);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Bus);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.Car);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.MotorCycle);
                                            vehicles.Add(Itinero.Osm.Vehicles.Vehicle.SmallTruck);
                                        }
                                        else
                                        { // assume a filename.
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
                                                vehicles.Add(vehicle);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        vehicles.Add(vehicle);
                                    }
                                }
                            }
                            break;
                        case "allcore":
                            if (SwitchParsers.IsTrue(value))
                            {
                                allCore = true;;
                            }
                            break;
                        case "wayids":
                        case "keepwayids":
                            if (SwitchParsers.IsTrue(value))
                            {
                                keepWayIds = true; ;
                            }
                            break;
                        case "s":
                        case "simplification":
                            var parsedInt = SwitchParsers.Parse(value);
                            if (parsedInt != null)
                            {
                                simplification = parsedInt.Value;
                            }
                            break;
                        case "n":
                        case "normalize":
                            var parsedBool = SwitchParsers.ParseBool(value);
                            if (parsedBool.HasValue)
                            {
                                normalize = parsedBool.Value;
                            }
                            break;
                        default:
                            throw new SwitchParserException("--create-routerdb",
                                string.Format("Invalid parameter for command --create-routerdb: {0} not recognized.", key));
                    }
                }
            }
            
            if (!(previous[previous.Count - 1] is Processors.Osm.IProcessorOsmStreamSource))
            {
                throw new Exception("Expected an OSM stream source.");
            }

            var source = (previous[previous.Count - 1] as Processors.Osm.IProcessorOsmStreamSource).Source;
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                var routerDb = new Itinero.RouterDb();

                // make sure the routerdb can handle multiple edges.
                routerDb.Network.GeometricGraph.Graph.MarkAsMulti();
                
                // load the data.
                var target = new Itinero.IO.Osm.Streams.RouterDbStreamTarget(routerDb,
                    vehicles.ToArray(), allCore, processRestrictions: true);
                if (keepWayIds)
                { // add way id's.
                    var eventsFilter = new OsmSharp.Streams.Filters.OsmStreamFilterDelegate();
                    eventsFilter.MoveToNextEvent += EventsFilter_AddWayId;
                    eventsFilter.RegisterSource(source);
                    target.RegisterSource(eventsFilter, false);
                }
                else
                { // use the source as-is.
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
            };
            processor = new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb);

            return 1;
        }

        
        static OsmSharp.OsmGeo EventsFilter_AddWayId(OsmSharp.OsmGeo osmGeo, object param)
        {
            if (osmGeo.Type == OsmSharp.OsmGeoType.Way)
            {
                osmGeo.Tags.Add("way_id", osmGeo.Id.ToString());
            }
            return osmGeo;
        }
    }
}