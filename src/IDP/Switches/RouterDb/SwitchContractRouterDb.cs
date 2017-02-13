using IDP.Processors;
using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Osm.Vehicles;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to add a contracted db to a router db.
    /// </summary>
    class SwitchContractRouterDb : Switch
    {
        /// <summary>
        /// Creates a switch.
        /// </summary>
        public SwitchContractRouterDb(string[] a)
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
                return new string[] { "--contract" };
            }
        }

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            if (previous.Count < 1) { throw new ArgumentException("Expected at least one processors before this one."); }
            
            if (!(previous[previous.Count - 1] is Processors.RouterDb.IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }

            Profile profile = null;
            var augmented = false;
            
            if (this.Arguments.Length == 1 && 
                !this.Arguments[0].Contains("="))
            {
                var profileName = this.Arguments[0];
                profile = Itinero.Profiles.Profile.GetRegistered(profileName);
                if (profile == null)
                {
                    throw new Exception(string.Format("Cannot find profile {0}.", this.Arguments[0]));
                }
            }
            else
            {
                for (var i = 0; i < this.Arguments.Length; i++)
                {
                    string key, value;
                    if (SwitchParsers.SplitKeyValue(this.Arguments[i], out key, out value))
                    {
                        switch (key.ToLower())
                        {
                            case "profile":
                                var profileName = value;
                                profile = Itinero.Profiles.Profile.GetRegistered(profileName);
                                if (profile == null)
                                {
                                    throw new Exception(string.Format("Cannot find profile {0}.", this.Arguments[0]));
                                }
                                break;
                            case "augmented":
                                if (SwitchParsers.IsTrue(value))
                                {
                                    augmented = true; ;
                                }
                                break;
                            default:
                                throw new SwitchParserException("--contract",
                                    string.Format("Invalid parameter for command --contract: {0} not recognized.", key));
                        }
                    }
                }
            }

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                var routerDb = source.GetRouterDb();
                
                if (augmented)
                {
                    routerDb.AddContracted(profile);
                }
                else
                {
                    routerDb.AddContracted(profile, profile.AugmentedWeightHandlerCached(routerDb));
                }

                return routerDb;
            };
            processor = new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb);

            return 1;
        }
    }
}
