using System.Collections.Generic;
using System.IO;
using Itinero.Profiles;

namespace IDP.Switches
{
    public static class ParseHelpers
    {

        public static TV GetOrDefault<TK, TV>(this Dictionary<TK, TV> dict,TK key, TV deflt)
        {
            return dict.ContainsKey(key) ? dict[key] : deflt;
        }
        /// <summary>
        /// Creates a list of vehicles based on the 'vehicle' and 'vehicles' parameters.
        /// Either uses builtins or read from file
        /// </summary>
        /// <returns></returns>
        public static List<Vehicle> ExtractVehicleArguments(this Dictionary<string, string> arguments)
        {
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


            if (!DocumentedSwitch.SplitValuesArray(vehiclesArg.ToLower(), out var vehicleNames))
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
                    throw new FileNotFoundException(
                        $"Loading the vehicle profile {name} failed. This profile was not recognized as a built-in profile, neither was it found as a file. Profiles can be found online: https://github.com/anyways-open/routing-profiles/",
                        name);
                }

                using (var stream = vehicleFile.OpenRead())
                {
                    vehicles.Add(DynamicVehicle.LoadFromStream(stream));
                }
            }

            return vehicles;
        }
    }
}