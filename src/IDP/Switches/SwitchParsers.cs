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
        private static List<Tuple<string[], Func<string[], Switch>>> _switches = 
            new List<Tuple<string[], Func<string[], Switch>>>();

        /// <summary>
        /// Registers all switches.
        /// </summary>
        public static void RegisterAll()
        {
            Register(Osm.SwitchReadPBF.Names, (a) => new Osm.SwitchReadPBF(a));
            //Register(Osm.SwitchWritePBF.Names, (a) => new Osm.SwitchWritePBF(a));
            Register(RouterDb.SwitchContractRouterDb.Names, (a) => new RouterDb.SwitchContractRouterDb(a));
            Register(Osm.SwitchFilterProgress.Names, (a) => new Osm.SwitchFilterProgress(a));
            Register(RouterDb.SwitchCreateRouterDb.Names, (a) => new RouterDb.SwitchCreateRouterDb(a));
            Register(RouterDb.SwitchIslandsRouterDb.Names, (a) => new RouterDb.SwitchIslandsRouterDb(a));
            Register(RouterDb.SwitchElevationRouterDb.Names, (a) => new RouterDb.SwitchElevationRouterDb(a));
            Register(RouterDb.SwitchReadRouterDb.Names, (a) => new RouterDb.SwitchReadRouterDb(a));
            Register(RouterDb.SwitchWriteRouterDb.Names, (a) => new RouterDb.SwitchWriteRouterDb(a));
            Register(GTFS.SwitchReadGTFS.Names, (a) => new GTFS.SwitchReadGTFS(a));
            Register(TransitDb.SwitchMergeTransitDbs.Names, (a) => new TransitDb.SwitchMergeTransitDbs(a));
            Register(TransitDb.SwitchReadTransitDb.Names, (a) => new TransitDb.SwitchReadTransitDb(a));
            Register(TransitDb.SwitchCreateTransitDb.Names, (a) => new TransitDb.SwitchCreateTransitDb(a));
            Register(TransitDb.SwitchWriteTransitDb.Names, (a) => new TransitDb.SwitchWriteTransitDb(a));
            Register(TransitDb.SwitchAddTransfersDb.Names, (a) => new TransitDb.SwitchAddTransfersDb(a));
            Register(MultimodalDb.SwitchCreateMultimodalDb.Names, (a) => new MultimodalDb.SwitchCreateMultimodalDb(a));
            Register(MultimodalDb.SwitchAddStopLinks.Names, (a) => new MultimodalDb.SwitchAddStopLinks(a));
            Register(MultimodalDb.SwitchWriteMultimodalDb.Names, (a) => new MultimodalDb.SwitchWriteMultimodalDb(a));
            Register(Shape.SwitchReadShape.Names, (a) => new Shape.SwitchReadShape(a));
            Register(Shape.SwitchWriteShape.Names, (a) => new Shape.SwitchWriteShape(a));
            Register(Logging.SwitchLogging.Names, (a) => new Logging.SwitchLogging(a));
        }

        /// <summary>
        /// Registers a new switch.
        /// </summary>
        public static void Register(string[] names, Func<string[], Switch> create)
        {
            _switches.Add(new Tuple<string[], Func<string[], Switch>>(names, create));
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
            var switches = new List<Switch>();
            for(var i = 0; i < args.Length; )
            {
                var s = FindSwitch(args[i]);
                i++;
                var parameters = new List<string>();
                while(i < args.Length &&
                    !IsSwitch(args[i]))
                {
                    parameters.Add(args[i]);
                    i++;
                }
                switches.Add(s(parameters.ToArray()));
            }

            var processors = new List<Processors.Processor>();
            for(var i = 0; i < switches.Count; i++)
            {
                Processors.Processor newProcessor;
                var p = switches[i].Parse(processors, out newProcessor);
                if (p < 0)
                {
                    throw new Exception(string.Format("Parsing of arguments failed for switch: {0}", 
                        switches[0].ToString()));
                }
                while(p > 0)
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
        private static Func<string[], Switch> FindSwitch(string name)
        {
            foreach (var tuple in _switches)
            {
                if (tuple.Item1.Contains(name))
                {
                    return tuple.Item2;
                }
            }
            throw new Exception(string.Format("Cannot find switch with name: {0}", name));
        }
        
        /// <summary>
        /// Returns true if the given string contains a key value like 'key=value'.
        /// </summary>
        public static bool SplitKeyValue(string keyValueString, out string key, out string value)
        {
            key = null;
            value = null;
            if (keyValueString.Count(x => x == '=') == 1)
            { // there is only one '=' sign here.
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
            if (!string.IsNullOrWhiteSpace(value) &&
                (value.ToLowerInvariant() == "yes" ||
                value.ToLowerInvariant() == "true"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parses an integer from the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int? Parse(string value)
        {
            int val;
            if (int.TryParse(value, out val))
            {
                return val;
            }
            return null;
        }
    }
}