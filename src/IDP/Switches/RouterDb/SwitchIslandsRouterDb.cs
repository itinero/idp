using System;
using System.Collections.Generic;
using System.Linq;
using IDP.Processors;
using IDP.Processors.RouterDb;
using Itinero;
using Itinero.Logging;
using Itinero.Profiles;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to detect islands in a router db.
    /// </summary>
    class SwitchIslandsRouterDb : DocumentedSwitch
    {
        private static readonly string[] _names = {"--islands"};

        private const string _about = "Detects islands in a routerdb. An island is a subgraph which is not reachable via the rest of the graph.";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)> _extraParams =
            new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            {
                opt("profile",
                    "The profile for which islands should be detected. This can be a comma-separated list of profiles as well. Default: apply island detection on _all_ profiles in the routerdb")
            };

        /// <summary>
        /// Creates a switch.
        /// </summary>
        public SwitchIslandsRouterDb()
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

            if (!(previous[previous.Count - 1] is IProcessorRouterDbSource source))
            {
                throw new Exception("Expected a router db source.");
            }

            string[] profiles = null;
            if (!string.IsNullOrEmpty(arguments["profile"]))
            {
                if (!SplitValuesArray(arguments["profile"], out profiles))
                {
                    profiles = new[] {arguments["profile"]};
                }
            }

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                Profile[] profileInstances;
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
                    Logger.Log("SwitchIslandRouterDb", TraceEventType.Information,
                        "Detecting islands for: {0}", profileInstance.FullName);
                    routerDb.AddIslandData(profileInstance);
                }

                return routerDb;
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}