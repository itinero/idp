using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.RouterDb;
using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Logging;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.RouterDb
{
    class SwitchRemoveRestrictionsRouterDb : DocumentedSwitch
    {
        private static readonly string[] _names = {"--remove-restrictions"};

        private const string _about = "Removes restrictions for one or more given vehicle type.";

        private const bool _isStable = false;

        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams
                = new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    opt("vehicle-types", "The comma separated list of vehicle types for which to remove restrictions. When the list is empty, the default, it will remove all restrictions.")
                        .SetDefault(string.Empty)
                };

        public SwitchRemoveRestrictionsRouterDb()
            : base(_names, _about, _extraParams, _isStable)
        {
        }

        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> args,
            List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            if (!(previous[previous.Count - 1] is IProcessorRouterDbSource source))
            {
                throw new Exception("Expected a router db source.");
            }

            var vehicleTypes = args.ExtractVehicleTypeArguments();

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                if (vehicleTypes == null ||
                    vehicleTypes.Count == 0)
                {
                    vehicleTypes = new List<string>();
                    foreach (var restrictionsDb in routerDb.RestrictionDbs)
                    {
                        vehicleTypes.Add(restrictionsDb.Vehicle);
                    }
                }

                foreach (var vehicleType in vehicleTypes)
                {
                    if (!routerDb.HasRestrictions(vehicleType))
                    {
                        Logger.Log(nameof(SwitchReadRouterDb), TraceEventType.Warning,
                            $"No restrictions found for '{vehicleType}'");
                        continue;
                    }

                    Logger.Log(nameof(SwitchReadRouterDb), TraceEventType.Information,
                        $"Removing restrictions for '{vehicleType}'");
                    routerDb.RemoveRestrictions(vehicleType);
                }

                return routerDb;
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}