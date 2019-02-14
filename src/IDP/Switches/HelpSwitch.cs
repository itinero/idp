using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IDP.Processors;
using static System.String;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches
{
    internal class HelpSwitch : DocumentedSwitch
    {
        private static readonly string[] _names = {"--help", "--?"};

        private static readonly List<(List<string>argName, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string>argName, bool isObligated, string comment, string defaultValue)>
                {
                    opt("about", "The command (or switch) you'd like more info about"),
                    opt("markdown", "md",
                        "Write the help text as markdown to a file. The documentation is generated with this flag."),
                    opt("experimental", "Include experimental switches in the output").SetDefault("false")
                };

        private const bool _isStable = true;
        private const string _about = "Print the help message";

        public HelpSwitch() :
            base(_names, _about, _extraParams, _isStable)
        {
        }


        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            return (new HelpProcessor(arguments), 0);
        }
    }


    internal sealed class HelpProcessor : Processor
    {
        private readonly Dictionary<string, string> _arguments;

        public HelpProcessor(Dictionary<string, string> arguments)
        {
            _arguments = arguments;
        }

        private static string GenerateAllHelp(bool markdown = false, bool includeExperimental = false)
        {
            var text = " Itinero Data Processor \n";
            text += " ====================== \n\n";
            text +=
                "The **Itinero Data Processor** *(IDP)* helps to convert a routable graph into a RouterDB," +
                " which can be used to quickly solve routing queries.\n\n" +
                "The minimal requirement to work with IDP is having a routable graph to serve as input. OpenStreetMap data for the entire world can be obtained for free at [geofrabrik.de](http://download.geofabrik.de/)\n";
            text += "\n\n";
            text +=
                " Some examples\n" +
                " -------------" +
                "\n" +
                "A minimal example which builds routing for bicycles is\n\n" +
                "        IDP --read-pbf <input-file.osm.pbf> --pr --create-routerdb bicycle --write-routerdb output.routerdb";
            text +=
                "\n\nTo include elevation data, add `--elevation`. To solve the queries even faster, use `--contract bicycle.<profile-to-optimize>`.\n" +
                "The full command would thus become\n\n" +
                "        IDP --read-pbf <input-file.osm.pbf> --pr --elevation --create-routerdb bicycle --contract bicycle.fastest --write-routerdb output.routerdb\n\n" +
                "For more advanced options, see the arguments below.";

            text +=
                "\n\n" +
                "Switch Syntax\n" +
                "-------------\n\n" +
                "The syntax of a switch is:\n\n" +
                "    --switch param1=value1 param2=value2\n" +
                "    # Or equivalent:\n" +
                "    --switch value1 value2\n" +
                "\n\nThere is no need to explicitly give the parameter name, as long as *unnamed* parameters" +
                " are in the same order as in the tables below. " +
                "It doesn't mater if only some arguments, all arguments or even no arguments are named. " +
                "`--switch value2 param1=value1`, `--switch value1 param2=value2` or `--switch param1=value1 value2` " +
                "are valid just as well.";
            text += "\n\n";
            text += "At last, `-param1` is a shorthand for `param=true`. This is useful for boolean flags\n\n";


            text += "\n\n Full overview of all options ";
            text += "\n ------------------------------- \n\n" +
                    "All switches are listed below. Click on a switch to get a full overview, including sub-arguments.\n\n";


            List<(string category, List<DocumentedSwitch>)> allSwitches;
            if (includeExperimental)
            {
                allSwitches = SwitchParsers.Documented;
            }
            else
            {
                // Only keep non-experimental switches
                allSwitches = new List<(string category, List<DocumentedSwitch>)>();
                foreach (var (cat, switches) in SwitchParsers.Documented)
                {
                    var sw = new List<DocumentedSwitch>();
                    foreach (var @switch in switches)
                    {
                        if (@switch.IsStable)
                        {
                            sw.Add(@switch);
                        }
                    }

                    if (sw.Any())
                    {
                        allSwitches.Add((cat, sw));
                    }
                }
            }


            // Build table of contents
            foreach (var (cat, switches) in allSwitches)
            {
                text += $"- [{cat}](#{cat.Replace(" ", "-")})\n";
                foreach (var @switch in switches)
                {
                    text += $"  * [{@switch.Names[0]}](#";
                    text += @switch.MarkdownName().Replace(" ", "-").Replace(",", "").Replace("(", "").Replace(")", "");

                    text += ") ";
                    var about = @switch.About;
                    var index = about.IndexOf('.');
                    text += index < 0 ? about : about.Substring(0, index + 1);
                    text += "\n";
                }
            }


            // Add docs
            foreach (var (cat, switches) in allSwitches)
            {
                text += $"### {cat}\n\n";

                foreach (var @switch in switches)
                {
                    text += @switch.Help(markdown) + "\n";
                }
            }

            return text;
        }


        public override void Execute()
        {
            if (!IsNullOrEmpty(_arguments["about"]))
            {
                var needed = _arguments["about"];
                var allSwitches = SwitchParsers.Documented;
                foreach (var (_, switches) in allSwitches)
                {
                    foreach (var documentedSwitch in switches)
                    {
                        foreach (var name in documentedSwitch.Names)
                        {
                            if (needed.Equals(name))
                            {
                                Console.WriteLine(documentedSwitch.Help());
                                return;
                            }
                        }
                    }
                }

                throw new ArgumentException(
                    $"Did not find documentation for switch {needed}. Don't worry, the switch probably exists but is not documented yet");
            }

            if (IsNullOrEmpty(_arguments["markdown"]))
            {
                Console.Write(GenerateAllHelp());
            }
            else
            {
                File.WriteAllText(_arguments["markdown"],
                    GenerateAllHelp(true, SwitchParsers.IsTrue(_arguments["experimental"])));
            }
        }

        public override bool CanExecute
        {
            get { return true; }
        }
    }
}