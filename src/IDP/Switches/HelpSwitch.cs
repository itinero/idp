using System;
using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using static System.String;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches
{
    internal class HelpSwitch : DocumentedSwitch
    {
        private static readonly string[] names = {"--help", "--?"};

        private static readonly List<(List<string>argName, bool isObligated, string comment, string defaultValue)>
            ExtraParams =
                new List<(List<string>argName, bool isObligated, string comment, string defaultValue)>()
                {
                    opt("about", "The command (or switch) you'd like more info about"),
                    opt("markdown", "md", "Write the help text as markdown to a file. The documentation is generated with this flag.")
                };

        private const bool IsStable = true;
        private const string About = "Print the help message";

        public HelpSwitch() :
            base(names, About, ExtraParams, IsStable)
        {
        }


        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
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

        private static string GenerateAllHelp(bool markdown = false)
        {
            var text = " Itinero Data Processor \n";
            text += " ====================== \n\n";
            text +=
                "The **Itinero Data Processor** *(IDP)* helps to convert a routable graph into a RouterDB," +
                " which can be used to quickly solve routing queries.\n\n" +
                "To work with IDP, you only need an input graph. OpenStreetMap data dumps can be obtained at [geofrabrik.de](http://download.geofabrik.de/)\n";
            text += "\n\n";
            text +=
                "Typical usage:\n\n" +
                "        IDP --read-pbf <input-file> --pr --create-routerdb bicycle --write-routerdb output.routerdb";
            text +=
                "\n\nTo include elevation data, add `--elevation`. To solve the queries even faster, use `--contract bicycle.<profile-to-optimize>`.  \n";

            text +=
                "\n\n" +
                "Switch Syntax\n" +
                "-------------\n\n" +
                "The syntax of a switch is:\n\n" +
                "    --switch param1=value1 param2=value2\n" +
                "    # Or equivalent:\n" +
                "    --switch value1 value2\n" +
                "\n\nThere is no need to explicitly give the parameter name, as long as the *unnamed* parameters      are in the same order as in the tables below. " +
                "Note that you are free to name some (but not all) arguments. " +
                "`--switch value2 param1=value1`, `--switch value1 param2=value2` or `--switch param1=value1 value2` " +
                "are valid just as well.";
            text += "\n\n";
            text += "At last, `-param1` is a shorthand for `param=true`. This is useful for flags\n\n";


            text += "\n\n Full overview of all options ";
            text += "\n ------------------------------- \n\n" +
                    "All switches are listed below. Click on a switch to get a full overview, including sub-arguments.\n\n";
            var allSwitches = SwitchParsers.documented;


            foreach (var (cat, switches) in allSwitches)
            {
                text += $"- [{cat}](#{cat.Replace(" ", "-")})\n";

                foreach (var @switch in switches)
                {
                    text += $"  * [{@switch.Names[0]}](#";
                    text += @switch.MarkdownName().Replace(" ", "-").Replace(",", "").Replace("(", "").Replace(")", "");

                    text += ") ";
                    var about = @switch._about;
                    var index = about.IndexOf('.');
                    text += index < 0 ? about : about.Substring(0, index + 1);
                    text += "\n";
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


        public override void Execute()
        {
            if (!IsNullOrEmpty(_arguments["about"]))
            {
                var needed = _arguments["about"];
                var allSwitches = SwitchParsers.documented;
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
                File.WriteAllText(_arguments["markdown"], GenerateAllHelp(true));
            }
        }

        public override bool CanExecute
        {
            get { return true; }
        }
    }
}