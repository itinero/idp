using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.Osm;
using IDP.Processors.RouterDb;

namespace IDP.Switches
{
    internal class HelpSwitch : DocumentedSwitch
    {
        private static readonly string[] names = {"--help", "--?"};

        private static readonly List<(string argName, bool isObligated, string comment)> ExtraParams =
            new List<(string argName, bool isObligated, string comment)>()
            {
                ("about", false, "The command (or switch) you'd like more info about"),
                ("markdown", false, "Output the help text in markdown")
            };

        private const bool IsStable = true;
        private static string about = "Print the help message";

        public HelpSwitch(string[] arguments) : base(arguments, names, about, ExtraParams, IsStable)
        {
        }

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
                " which can help to quickly solve routing queries.\n\nTo work with IDP, you need:\n\n- An input graph. OpenStreetMap data dumps can be obtained at [geofrabrik.de](http://download.geofabrik.de/)\n- A routing profile, which can be obtained in [our repo](https://github.com/anyways-open/routing-profiles/) ";
            text += "\n\n";
            text +=
                "Typical usage:\n\n" +
                "        IDP --read-pbf <input-file> --pr --create-routerdb bicycle.lua --write-routerdb output.routerdb";
            text += "\n\nOften in combination with `--contract bicycle.networks` and `--elevation` for production.\n";

            text += "\n\n Full overview of all options ";
            text += "\n ------------------------------- \n\n";
            var switches = SwitchParsers.DocumentedSwitches;
            foreach (var documentedSwitch in switches)
            {
                text += documentedSwitch.Help(markdown)+"\n";
            }

            return text;
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            if (arguments.ContainsKey("about"))
            {
                var needed = arguments["about"];
                var switches = SwitchParsers.DocumentedSwitches;
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

                throw new ArgumentException(
                    $"Did not find documentation for switch {needed}. Don't worry, the switch probably exists but is not documented yet");
            }

            var md = arguments.ContainsKey("markdown") && SwitchParsers.IsTrue(arguments["markdown"]);
            
            Console.Write(GenerateAllHelp(md));
            return (null, 0);
        }

        public override DocumentedSwitch SetArguments(string[] arguments)
        {
            return new HelpSwitch(arguments);
        }
    }
}
