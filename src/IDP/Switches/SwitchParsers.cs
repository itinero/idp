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
using IDP.Processors;
using IDP.Switches.GeoJson;
using IDP.Switches.Logging;
using IDP.Switches.Osm;
using IDP.Switches.RouterDb;
using IDP.Switches.Shape;
using IDP.Switches.Transit;

namespace IDP.Switches
{
    /// <summary>
    /// A switch parser.
    /// </summary>
    static class SwitchParsers
    {
        private static List<(string[] names, Switch @switch)> _switches =
            new List<(string[] names, Switch @switch)>();


        public static List<(string category, List<DocumentedSwitch>)>     Documented;

        /// <summary>
        /// Registe    rs all switches.
        /// </summary>
        public static void RegisterAll()
        {
            // The help function does print in the same order as here
            // so put the most important switches up top


            Documented = new List<(string category, List<DocumentedSwitch>)>
            {
                ("Input", new List<DocumentedSwitch>
                {
                    new SwitchReadPbf(),
                    new SwitchReadShape(),
                    new SwitchReadRouterDb(),
                }),

                ("Data processing", new List<DocumentedSwitch>
                {
                    new SwitchCreateRouterDb(),
                    new SwitchElevationRouterDb(),
                    new SwitchContractRouterDb()
                }),

                ("Data analysis", new List<DocumentedSwitch>
                {
                    new SwitchIslandsRouterDb(),
                    
                }),

                ("Output", new List<DocumentedSwitch>
                {
                    new SwitchWriteRouterDb(),
                    new SwitchWritePbf(),
                    new SwitchWriteShape(),
                    new SwitchWriteGeoJson(),
                }),

                ("Transit-Db", new List<DocumentedSwitch>
                {
                    
                    new SwitchCreateTransitDbLC(),
                    new SwitchCreateTransitDbOSM(),
                    new SwitchReadTransitDb(),
                    new SwitchSelectTimeWindow(),
                    new SwitchSelectStops(),
                    new SwitchDumpTransitDbStops(),
                    new SwitchDumpTransitDbConnections(),
                    new SwitchWriteTransitDb(),
                    
                }),
                
                
                ("Usability", new List<DocumentedSwitch>
                {
                    new SwitchFilterProgress(),
                    new SwitchLogging(),
                    new HelpSwitch()
                }),

            };


            foreach (var (_, switches) in Documented)
            {
                foreach (var @switch in switches)
                {
                    Register(@switch);
                }
            }

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
        public static Processor Parse(string[] args)
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

            var processors = new List<Processor>();
            foreach (var t in switches)
            {
                Processor newProcessor;

                int p;
                try
                {
                    var (sw, parameters) = t;
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


            var names = "";
            foreach (var (nms, _) in _switches)
            {
                names += nms[0] + "\n";
            }

            throw new Exception($"Cannot find switch with name: {name}.\nKnown switches are: {names}");
        }

    }
}