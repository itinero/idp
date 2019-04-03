using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.RouterDb;
using Itinero;
using Itinero.Algorithms.Weights;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.RouterDb
{
    class SwitchContractRouterDb : DocumentedSwitch
    {
        private static readonly string[] _names = {"--contract"};

        private const string _about = "Applies contraction on the graph." + "Solving queries on a contracted graph is _much_ faster, although preprocessing is quite a bit slower (at least 5 times slower);" + "most use cases will require this flag." + "To enable contraction for multiple profiles and/or multiple vehicles, simply add another --contraction\n\n" + "Contraction is able to speed up querying by building an index of _shortcuts_. Basically, between some points of the graph, an extra vertex is inserted in the routerdb." + "This extra vertex represents how one could travel between these points and which path one would thus take." + "The actual search for a shortest path can use these shortcuts instead of searching the whole graph. For more information, see [the wikipedia article on contraction hierarchies](https://en.wikipedia.org/wiki/Contraction_hierarchies)";

        private const bool _isStable = true;

        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams
                = new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("profile", "The profile for which a contraction hierarchy should be built"),
                    opt("augmented", "By default, only one metric is kept in the hierarchy - such as either time or distance (which one depends on the profile). " +
                                     "For some usecases, it is useful to have _both_ distance and time available in the routerdb. " +
                                     "Setting this flag to `true` will cause both metrics to be included.")
                        .SetDefault("false")
                };


        public SwitchContractRouterDb()
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

            var profile = args["profile"];
            var augmented = IsTrue(args["augmented"]);



            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                var profileInstance = routerDb.GetSupportedProfile(profile);

                if (!augmented)
                {
                    routerDb.AddContracted(profileInstance);
                }
                else
                {
                    routerDb.AddContracted(profileInstance, profileInstance.AugmentedWeightHandlerCached(routerDb));
                }

                return routerDb;
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}