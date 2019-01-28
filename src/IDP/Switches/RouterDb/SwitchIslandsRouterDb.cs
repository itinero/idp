using IDP.Processors;
using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Osm.Vehicles;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to detect islands in a router db.
    /// </summary>
    class SwitchIslandsRouterDb : DocumentedSwitch
    {
        private static string[] _names = {"--islands"};

        private static string about =
            "Detects islands in a routerdb. An island is a subgraph which is not reachable via the rest of the graph.";


        private static readonly List<(string argName, bool isObligated, string comment)> ExtraParams =
            new List<(string argName, bool isObligated, string comment)>()
            {
                ("profile", false,
                    "The profile for which islands should be detected. This can be a comma-separated list of profiles as well. Default: apply island detection on _all_ profiles in the routerdb"),
            };

        /// <summary>
        /// Creates a switch.
        /// </summary>
        public SwitchIslandsRouterDb()
            : base(_names, about, ExtraParams, false)
        {
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            if (!(previous[previous.Count - 1] is Processors.RouterDb.IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }

            string[] profiles = null;
            if (arguments.ContainsKey("profile"))
            {
                if (!SwitchParsers.SplitValuesArray(arguments["profile"], out profiles))
                {
                    profiles = new[] {arguments["profile"]};
                }
            }

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                Profile[] profileInstances = null;
                if (profiles == null)
                {
                    profileInstances = routerDb.GetSupportedProfiles().ToArray();
                }
                else
                {
                    profileInstances = new Profile[profiles.Length];
                    for (var i = 0; i < profileInstances.Length; i++)
                    {
                        profileInstances[i] = routerDb.GetSupportedProfile(profiles[i]);
                    }
                }

                foreach (var profileInstance in profileInstances)
                {
                    Itinero.Logging.Logger.Log("SwitchIslandRouterDb", Itinero.Logging.TraceEventType.Information,
                        "Detecting islands for: {0}", profileInstance.FullName);
                    routerDb.AddIslandData(profileInstance);
                }

                return routerDb;
            }

            return (new Processors.RouterDb.ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}