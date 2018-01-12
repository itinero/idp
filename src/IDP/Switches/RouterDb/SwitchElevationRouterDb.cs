using IDP.Processors;
using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Elevation;
using Itinero.LocalGeo.Elevation;
using Itinero.Osm.Vehicles;
using Itinero.Profiles;
using SRTM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to detect elevation in a router db.
    /// </summary>
    class SwitchElevationRouterDb : Switch
    {
        private const string DEFAULT_CACHE = "srtm-cache";

        /// <summary>
        /// Creates a switch.
        /// </summary>
        public SwitchElevationRouterDb(string[] a)
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
                return new string[] { "--elevation" };
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

            var cache = DEFAULT_CACHE;
            if (this.Arguments.Length == 0)
            {
                // use default.
            }
            else if (this.Arguments.Length == 1 &&
                !this.Arguments[0].Contains("="))
            {
                throw new Exception("Invalid argument.");
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
                            case "cache":
                                cache = value;
                                break;
                            default:
                                throw new SwitchParserException("--elevation",
                                    string.Format("Invalid parameter for command --elevation: {0} not recognized.", key));
                        }
                    }
                }
            }
            
            // create a new srtm data instance.
            // it accepts a folder to download and cache data into.
            var srtmCache = new DirectoryInfo(cache);
            if (!srtmCache.Exists)
            {
                srtmCache.Create();
            }
            var srtmData = new SRTMData("srtm-cache");
            ElevationHandler.GetElevation = (lat, lon) =>
            {
                var elevation = srtmData.GetElevation(lat, lon);
                if (!elevation.HasValue)
                {
                    return null;
                }
                return (short)elevation;
            };

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                var routerDb = source.GetRouterDb();

                Itinero.Logging.Logger.Log("SwitchElevationRouterDb", Itinero.Logging.TraceEventType.Information,
                    "Adding elevation.");
                routerDb.AddElevation();
                
                return routerDb;
            };
            processor = new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb);

            return 1;
        }
    }
}
