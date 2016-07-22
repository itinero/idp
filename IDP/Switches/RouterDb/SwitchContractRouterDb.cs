using IDP.Processors;
using Itinero;
using Itinero.Osm.Vehicles;
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
            if (this.Arguments.Length != 1) { throw new ArgumentException("Exactly one argument is expected."); }
            if (previous.Count < 1) { throw new ArgumentException("Expected at least one processors before this one."); }
            
            if (!(previous[previous.Count - 1] is Processors.RouterDb.IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }

            var profile = Itinero.Profiles.Profile.Get(this.Arguments[0]);
            if (profile == null)
            {
                throw new Exception(string.Format("Cannot find profile {0}.", this.Arguments[0]));
            }

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                var routerDb = source.GetRouterDb();

                // contract.
                routerDb.AddContracted(profile);

                return routerDb;
            };
            processor = new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb);

            return 1;
        }
    }
}
