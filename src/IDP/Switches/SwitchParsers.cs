// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace IDP.Switches
{
    /// <summary>
    /// A switch parser.
    /// </summary>
    static class SwitchParsers
    {
        private static List<(string[] names, Switch @switch)> _switches =
            new List<(string[] names, Switch @switch)>();


        public static List<(string category, List<DocumentedSwitch>)> documented;

        /// <summary>
        /// Registers all switches.
        /// </summary>
        public static void RegisterAll()
        {
            // The help function does print in the same order as here
            // so put the most important switches up top


            documented = new List<(string category, List<DocumentedSwitch>)>()
            {
                ("Input", new List<DocumentedSwitch>
                {
                    new Osm.SwitchReadPBF(),
                    new RouterDb.SwitchReadRouterDb()
                }),

                ("Data processing", new List<DocumentedSwitch>
                {
                    new RouterDb.SwitchCreateRouterDb(),
                    new RouterDb.SwitchElevationRouterDb(),
                    new RouterDb.SwitchContractRouterDb()
                }),


                ("Output", new List<DocumentedSwitch>
                {
                    new RouterDb.SwitchWriteRouterDb(),
                    new Osm.SwitchWritePBF(),
                    new GeoJson.SwitchWriteGeoJson()
                }),

                ("Usability", new List<DocumentedSwitch>()
                {
                    new Osm.SwitchFilterProgress(),
                    new Logging.SwitchLogging(),
                    new HelpSwitch()
                })
            };


            foreach (var (_, switches) in documented)
            {
                foreach (var @switch in switches)
                {
                    Register(@switch);
                }
            }

            Register(Shape.SwitchReadShape.Names, new Shape.SwitchReadShape(null));
            Register(Shape.SwitchWriteShape.Names, new Shape.SwitchWriteShape(null));
            // -- Old switches, documentation pending --

            Register(RouterDb.SwitchIslandsRouterDb.Names, new RouterDb.SwitchIslandsRouterDb(null));

            Register(GTFS.SwitchReadGTFS.Names, new GTFS.SwitchReadGTFS(null));
            Register(TransitDb.SwitchMergeTransitDbs.Names, new TransitDb.SwitchMergeTransitDbs(null));
            Register(TransitDb.SwitchReadTransitDb.Names, new TransitDb.SwitchReadTransitDb(null));
            Register(TransitDb.SwitchCreateTransitDb.Names, new TransitDb.SwitchCreateTransitDb(null));
            Register(TransitDb.SwitchWriteTransitDb.Names, new TransitDb.SwitchWriteTransitDb(null));
            Register(TransitDb.SwitchAddTransfersDb.Names, new TransitDb.SwitchAddTransfersDb(null));
            Register(MultimodalDb.SwitchCreateMultimodalDb.Names, new MultimodalDb.SwitchCreateMultimodalDb(null));
            Register(MultimodalDb.SwitchAddStopLinks.Names, new MultimodalDb.SwitchAddStopLinks(null));
            Register(MultimodalDb.SwitchWriteMultimodalDb.Names, new MultimodalDb.SwitchWriteMultimodalDb(null));
        }

        private static void Register(DocumentedSwitch swtch)
        {
            Register(swtch.Names, swtch);
        }

        /// <summary>
        /// Registers a new switch.
        /// </summary>
        private static void Register(string[] names, Switch swtch)
        {
            _switches.Add((names, swtch));
        }

        /// <summary>
        /// Returns true if this argument is a switch.
        /// </summary>
        private static bool IsSwitch(string name)
        {
            return name.StartsWith("--");
        }

        /// <summary>
        /// Parses the given arguments.
        /// </summary>
        public static Processors.Processor Parse(string[] args)
        {
            var switches = new List<(Switch, string[] args)>();
            for (var i = 0; i < args.Length;)
            {
                var swtch = FindSwitch(args[i]);
                i++;
                var parameters = new List<string>();
                while (i < args.Length &&
                       !IsSwitch(args[i]))
                {
                    parameters.Add(args[i]);
                    i++;
                }

                switches.Add((swtch, parameters.ToArray()));
            }

            var processors = new List<Processors.Processor>();
            for (var i = 0; i < switches.Count; i++)
            {
                Processors.Processor newProcessor;

                int p;
                try
                {
                    var (sw, parameters) = switches[i];
                    sw.Arguments = parameters;
                    p = sw.Parse(processors, out newProcessor);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(string.Format("Parsing of arguments failed for switch: {0}",
                        switches[0].ToString()), e);
                }

                if (p < 0)
                {
                    throw new ArgumentException(string.Format("Parsing of arguments failed for switch: {0}",
                        switches[0].ToString()));
                }

                while (p > 0)
                {
                    processors.RemoveAt(processors.Count - 1);
                    p--;
                }

                if (newProcessor != null)
                {
                    processors.Add(newProcessor);
                }
            }

            if (processors.Count > 1)
            {
                throw new Exception("More than one processor left over after parsing switches.");
            }

            if (!processors[0].CanExecute)
            {
                throw new Exception("Processor left over after parsing switches cannot be executed.");
            }

            return processors[0];
        }

        /// <summary>
        /// Finds a switch by it's name.
        /// </summary>
        private static Switch FindSwitch(string name)
        {
            foreach (var tuple in _switches)
            {
                if (tuple.Item1.Contains(name))
                {
                    return tuple.Item2;
                }
            }

            throw new Exception($"Cannot find switch with name: {name}");
        }


        /// <summary>
        /// Returns true if the given string contains a key value like 'key=value'.
        /// </summary>
        public static bool SplitKeyValue(string keyValueString, out string key, out string value)
        {
            key = null;
            value = null;
            if (keyValueString.Count(x => x == '=') == 1)
            {
                // there is only one '=' sign here.
                int idx = keyValueString.IndexOf('=');
                if (idx > 0 && idx < keyValueString.Length - 1)
                {
                    key = keyValueString.Substring(0, idx);
                    value = keyValueString.Substring(idx + 1, keyValueString.Length - (idx + 1));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the given string contains one or more comma seperated values.
        /// </summary>
        public static bool SplitValuesArray(string valuesArray, out string[] values)
        {
            values = valuesArray.Split(',');
            return true;
        }

        /// <summary>
        /// Remove quotes from strings if they occur at exactly the beginning and end.
        /// </summary>
        public static string RemoveQuotes(string stringToParse)
        {
            if (string.IsNullOrEmpty(stringToParse))
            {
                return stringToParse;
            }

            if (stringToParse.Length < 2)
            {
                return stringToParse;
            }

            if (stringToParse[0] == '"' && stringToParse[stringToParse.Length - 1] == '"')
            {
                return stringToParse.Substring(1, stringToParse.Length - 2);
            }

            if (stringToParse[0] == '\'' && stringToParse[stringToParse.Length - 1] == '\'')
            {
                return stringToParse.Substring(1, stringToParse.Length - 2);
            }

            return stringToParse;
        }

        /// <summary>
        /// Returns true if the given string value represent true.
        /// </summary>
        internal static bool IsTrue(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   (value.ToLowerInvariant() == "yes" ||
                    value.ToLowerInvariant() == "true");
        }

        /// <summary>
        /// Parses an integer from the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int? Parse(string value)
        {
            if (int.TryParse(value, out var val))
            {
                return val;
            }

            return null;
        }
    }
}