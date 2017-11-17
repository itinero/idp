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
    class SwitchIslandsRouterDb : Switch
    {
        /// <summary>
        /// Creates a switch.
        /// </summary>
        public SwitchIslandsRouterDb(string[] a)
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
                return new string[] { "--islands" };
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

            string[] profiles = null;
            if (this.Arguments.Length == 0)
            {
                // do all profiles.
            }
            else if (this.Arguments.Length == 1 && 
                !this.Arguments[0].Contains("="))
            {
                if (!SwitchParsers.SplitValuesArray(this.Arguments[0], out profiles))
                {
                    throw new Exception("Cannot split profile parameter.");
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
                            case "profiles":
                                if (!SwitchParsers.SplitValuesArray(value, out profiles))
                                {
                                    throw new Exception("Cannot split profile parameter.");
                                }
                                break;
                            default:
                                throw new SwitchParserException("--islands",
                                    string.Format("Invalid parameter for command --islands: {0} not recognized.", key));
                        }
                    }
                }
            }

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);
            Func<Itinero.RouterDb> getRouterDb = () =>
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
                    routerDb.AddIslandData(profileInstance);
                }

                return routerDb;
            };
            processor = new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb);

            return 1;
        }
    }
}
