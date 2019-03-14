using System;
using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using IDP.Processors.RouterDb;
using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Logging;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.RouterDb
{
    class SwitchDumpProfileSpeedsRouterDb : DocumentedSwitch
    {
        private static readonly string[] _names = {"--dump-profile-speeds"};

        private const string _about = "Dumps the routing profile speeds in CSV form.";

        private const bool _isStable = true;

        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams
                = new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("profile", "The profile for which to dump all the speeds."),
                    obl("file", "The CSV file to dump the data in.")
                };

        public SwitchDumpProfileSpeedsRouterDb()
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
            var file = args["file"];

            Itinero.RouterDb GetRouterDb()
            {
                var routerDb = source.GetRouterDb();

                if (!routerDb.SupportProfile(profile))
                {
                    Logger.Log(nameof(SwitchReadRouterDb), TraceEventType.Warning,
                        $"Cannot dump speeds for unsupported profile: '{profile}'");
                    return routerDb;
                }
                var profileInstance = routerDb.GetSupportedProfile(profile);

                var fileInfo = new FileInfo(file);
                if (!fileInfo.Directory.Exists)
                {
                    throw new DirectoryNotFoundException($"{nameof(SwitchDumpProfileSpeedsRouterDb)}: Parent directory for output file doesn't exist.");
                }

                using (var stream = File.Open(file, FileMode.Create))
                using (var streamWriter = new StreamWriter(stream))
                {
                    streamWriter.WriteLine($"factor;speed-from-factor;speed;direction;edgeprofile");

                    for (uint p = 0; p < routerDb.EdgeProfiles.Count; p++)
                    {
                        var edgeProfile = routerDb.EdgeProfiles.Get(p);
                        var factorAndSpeed = profileInstance.FactorAndSpeed(edgeProfile);
                        
                        streamWriter.WriteLine($"{factorAndSpeed.Value};{1/factorAndSpeed.Value*3.6};{1/factorAndSpeed.SpeedFactor*3.6};{factorAndSpeed.Direction};{edgeProfile}");
                    }
                }
                
                return routerDb;
            }

            return (new ProcessorRouterDbSource(GetRouterDb), 1);
        }
    }
}