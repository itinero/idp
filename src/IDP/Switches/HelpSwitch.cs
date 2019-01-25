using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using IDP.Processors;
using IDP.Processors.Osm;
using IDP.Processors.RouterDb;
using NetTopologySuite.Operation.Valid;

namespace IDP.Switches
{
    internal class HelpSwitch : DocumentedSwitch
    {
        private static readonly string[] names = {"--help", "--?"};

        private static readonly List<(string argName, bool isObligated, string comment)> ExtraParams =
            new List<(string argName, bool isObligated, string comment)>()
            {
                ("about", false, "The command (or switch) you'd like more info about"),
                ("markdown", false, "Write the help text as markdown to a file")
            };

        private const bool IsStable = true;
        private static string about = "Print the help message";

        public HelpSwitch() :
            base(names, about, ExtraParams, IsStable)
        {
        }

        private static string GenerateAllHelp(bool markdown = false)
        {
            var text = " Itinero Data Processor \n";
            text += " ====================== \n\n";
            text +=
                "The **Itinero Data Processor** *(IDP)* helps you to convert a routable graph into a RouterDB," +
                " which can help to quickly solve routing queries.\n\nTo work with IDP, you only need an input graph. OpenStreetMap data dumps can be obtained at [geofrabrik.de](http://download.geofabrik.de/)\n";
            text += "\n\n";
            text +=
                "Typical usage:\n\n" +
                "        IDP --read-pbf <input-file> --pr --create-routerdb bicycle --write-routerdb output.routerdb";
            text += "\n\nOften in combination with `--contract bicycle.networks` and `--elevation` for production.\n";

            text += "\n\n Full overview of all options ";
            text += "\n ------------------------------- \n\n";
            var allSwitches = SwitchParsers.documented;

            foreach (var (cat, switches) in allSwitches)
            {
                text += $"- [{cat}](#{cat})\n";
                
                foreach (var @switch in switches)
                {
                    
                //    text += $" + [{@switch.Names[0]}[]\n";
                }
            }

            

            foreach (var (cat, switches) in allSwitches)
            {
                text += $"### {cat}\n";
                
                foreach (var @switch in switches)
                {
                    text += @switch.Help(markdown) + "\n";
                }
            }

            return text;
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (arguments.ContainsKey("about"))
            {
                var needed = arguments["about"];
                var allSwitches = SwitchParsers.documented;
                foreach (var (cat, switches) in allSwitches)
                {
                    foreach (var documentedSwitch in switches)
                    {
                        foreach (var name in documentedSwitch.Names)
                        {
                            if (needed.Equals(name))
                            {
                                Console.WriteLine(documentedSwitch.Help());
                                return (null, 0);
                            }
                        }
                    }
                }

                throw new ArgumentException(
                    $"Did not find documentation for switch {needed}. Don't worry, the switch probably exists but is not documented yet");
            }

            string md = null;
            if (arguments.TryGetValue("markdown", out md))
            {
                File.WriteAllText(md, GenerateAllHelp(true));
            }
            else
            {
                Console.Write(GenerateAllHelp());
            }

            return (null, 0);
        }
    }
}