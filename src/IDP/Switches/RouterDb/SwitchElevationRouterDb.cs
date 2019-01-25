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
    class SwitchElevationRouterDb : DocumentedSwitch
    {
        private const string DEFAULT_CACHE = "srtm-cache";
        
        
        
        
        private static string[] _names => new[] {"--elevation","--ele"};      
        private static string about = "Incorporates elevation data in the calculations.\n" +
                                      $"Specifying this flag will download the SRTM-dataset and cache this in {DEFAULT_CACHE}." +
                                      "This data will be reused upon further runs";


        private static readonly List<(string argName, bool isObligated, string comment)> Parameters =
            new List<(string argName, bool isObligated, string comment)>
            {
                ("cache", false, "Caching directory name, if another caching directory should be used."),
            };


        private const bool IsStable = true;


        public SwitchElevationRouterDb()
            : base(_names, about, Parameters, IsStable)
        {

        }


        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments, List<Processor> previous)
        {


            var cache = DEFAULT_CACHE;
            arguments.TryGetValue("cache", out cache);

            if (previous.Count < 1) { throw new ArgumentException("Expected at least one processors before this one."); }
            
            if (!(previous[previous.Count - 1] is Processors.RouterDb.IProcessorRouterDbSource))
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
                return (short)elevation;
            };

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                Itinero.Logging.Logger.Log("SwitchElevationRouterDb", Itinero.Logging.TraceEventType.Information, "Adding elevation.");
                routerDb.AddElevation();

                return routerDb;
            }

            return(new Processors.RouterDb.ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}
