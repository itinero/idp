using System;
using System.Collections.Generic;
using IDP.Processors;

namespace IDP.Switches
{
    internal class HelpSwitch: DocumentedSwitch
    {
        private static readonly string[] names = {"--help", "--?"};
        private static readonly  List<(string argName, bool isObligated, string comment)> ExtraParams =
            new List<(string argName, bool isObligated, string comment)>()
            {
                ("about", false, "The command (or switch) you'd like more info about")
            };

        private const bool isStable = true;

        public HelpSwitch(string[] arguments) : base(arguments, names, ExtraParams, isStable)
        {
        }

        public HelpSwitch() :
            base(names, ExtraParams, isStable)
        {
        }

        private string GenerateAllHelp()
        {
            var text = " Itinero Data Processor \n";
            text += " ====================== \n\n";
            text +=
                "The Itinero Data Processor (IDP) helps you to convert an input graph (such as an OpenStreetMap data dump) into a RouterDB to quickly solve routing queries.";
            text += "\nTypical usage:\n        IDP --read-pbf <input-file> --pr --create-routerdb vehicles=bicycle.lua --write-routerdb output.routerdb";
            text += "\nUseful flags: '--contract bicycle.networks', '--elevation'.\n";

            text += "\n\n Full overview of switches ";
            text += "\n ------------------------- \n";
            var switches = SwitchParsers.DocumentedSwitches;
            foreach (var documentedSwitch in switches)
            {
                text += documentedSwitch.Help();
            }

            return text;
        }

        public override Processor Parse(Dictionary<string, string> arguments, List<Processor> previous)
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
                            return null;
                        }
                    }
                }
                throw new ArgumentException($"Did not find documentation for switch {needed}. Don't worry, the switch probably exists but is not documented yet");
            }
            Console.Write(GenerateAllHelp());
            return null;
        }    

        public override DocumentedSwitch SetArguments(string[] arguments)
        {
            return new HelpSwitch(arguments);
        }
    }
}