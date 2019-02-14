using System;
using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using IDP.Processors.RouterDb;
using Itinero.Elevation;
using Itinero.LocalGeo.Elevation;
using Itinero.Logging;
using SRTM;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to detect elevation in a router db.
    /// </summary>
    class SwitchElevationRouterDb : DocumentedSwitch
    {
        private const string _defaultCache = "srtm-cache";


        private static readonly string[] _names = {"--elevation", "--ele"};

        private const string _about = "Incorporates elevation data in the calculations.\n" + "Specifying this flag will download the SRTM-dataset and cache this on the file system." + "This data will be reused upon further runs";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)> _parameters =
            new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            {
                opt("cache", "Caching directory name, if another caching directory should be used.")
                    .SetDefault(_defaultCache)
            };


        private const bool _isStable = true;


        public SwitchElevationRouterDb()
            : base(_names, _about, _parameters, _isStable)
        {
        }


        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var cache = arguments["cache"];

            if (previous.Count < 1)
            {
                throw new ArgumentException("Expected at least one processors before this one.");
            }

            if (!(previous[previous.Count - 1] is IProcessorRouterDbSource source))
            {
                throw new Exception("Expected a router db source.");
            }


            // create a new srtm data instance.
            // it accepts a folder to download and cache data into.
            var srtmCache = new DirectoryInfo(cache);
            if (!srtmCache.Exists)
            {
                srtmCache.Create();
            }

            var srtmData = new SRTMData(cache);
            ElevationHandler.GetElevation = (lat, lon) =>
            {
                var elevation = srtmData.GetElevation(lat, lon);
                if (!elevation.HasValue)
                {
                    return null;
                }

                return (short) elevation;
            };

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                Logger.Log("SwitchElevationRouterDb", TraceEventType.Information,
                    "Adding elevation.");
                routerDb.AddElevation();

                return routerDb;
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}