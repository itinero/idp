using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.Osm;
using IDP.Processors.RouterDb;
using Itinero;
using Itinero.Algorithms.Weights;

namespace IDP.Switches.RouterDb
{
    class SwitchContractRouterDb : DocumentedSwitch
    {
       
        private static string[] names = {"--contract"};

        private static string about = "Applies contraction on the graph." +
                                      "Solving queries on a contracted graph is _much_ faster, although preprocessing is quite a bit slower (at least 5 times slower);" +
                                      "most use cases will require this flag." +
                                      "To enable contraction for multiple profiles and/or multiple vehicles, simply add another --contraction";

        private const bool isStable = true;

        private static readonly List<(string argName, bool isObligated, string comment)> extraParams
            = new List<(string argName, bool isObligated, string comment)>()
            {
                ("profile", true, "The profile for which a contraction hierarchy should be built"),
                ("augmented", false, "If specified with 'yes', an augmented weight handler will be used")
            };


        public SwitchContractRouterDb()
            : base(names, about, extraParams, isStable)
        {
        }


        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> args, List<Processor> previous)
        {
            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            if (!(previous[previous.Count - 1] is IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }

            string profile = args["profile"];
            var augmented = args.ContainsKey("augmented") &&
                            SwitchParsers.IsTrue(args["augmented"]);


            var source = (previous[previous.Count - 1] as IProcessorRouterDbSource);

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