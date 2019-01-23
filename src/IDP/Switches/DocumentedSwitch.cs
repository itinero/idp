using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IDP.Processors;
using IDP.Processors.Osm;

[assembly: InternalsVisibleTo("IDP.Test")]
[assembly: InternalsVisibleTo("IDP.Test")]

namespace IDP.Switches
{
    /// <inheritdoc />
    /// <summary>
    /// A documented switch contains all flags explicitly,
    /// in order to be able to generate documentation and to make parsing easier 
    /// </summary>
    abstract class DocumentedSwitch : Switch
    {
        /// <summary>
        /// The names of the switch
        /// </summary>
        public readonly string[] Names;

        /// <summary>
        /// What does this switch do?
        /// </summary>
        private readonly string _about;

        /// <summary>
        /// 
        /// Give a list of expected and optional arguments.
        /// E.g. (for write-geojson), this would be:
        /// [( "file", true, "The file where the geojson will be written to")
        /// , ("left", false, "The minimum latitude")
        /// , ("right", false, ...)
        /// , ("top", false, ...)
        /// , ("bottom", false, ...)]
        ///
        /// Indicating an must-have argument 'file' and four optional arguments
        /// </summary>
        private readonly List<(string argName, bool isObligated, string comment)> _extraParams;


        /// <summary>
        /// Should this switch be clearly showed in the documentation?
        /// </summary>
        /// <returns></returns>
        private readonly bool _isStable;

        protected DocumentedSwitch(string[] arguments,
            string[] names, string about,
            List<(string argName, bool isObligated, string comment)> extraParams,
            bool isStable
        ) : base(arguments)
        {
            Names = names;
            _about = about;
            _extraParams = extraParams;
            _isStable = isStable;
        }

        protected DocumentedSwitch(string[] names, string about,
            List<(string argName, bool isObligated, string comment)> extraParams,
            bool isStable
        ) : this(new string[] { }, names, about, extraParams, isStable)
        {
        }

        protected DocumentedSwitch(string[] arguments,
            DocumentedSwitch cloneFrom) : this(arguments, cloneFrom.Names, cloneFrom._about, cloneFrom._extraParams,
            cloneFrom._isStable)
        {
        }


        public abstract (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous);

        // Legacy, to be removed
        public abstract DocumentedSwitch SetArguments(string[] arguments);

        public override int Parse(List<Processor> previous, out Processor processor)
        {
            int consumed;
            // I'm not really keen on keeping track of the Argument in this object, hence my "translation" to make transition easier
            (processor, consumed) = Parse(ParseExtraParams(Arguments), previous);
            return consumed;
        }

        /// <summary>
        /// Converts the command line arguments into a dictionary, using ExtraParams.
        /// Unnamed parameters are matched with the beginning of the list
        ///
        /// E.g. --write-geojson left=123 someFile right=456 ...
        /// will match someFile with the first 'file' argument
        /// 
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, string> ParseExtraParams(string[] arguments)
        {
            var result = new Dictionary<string, string>();

            var expected = _extraParams;
            var index = 0;

            var used = new HashSet<string>();

            // First, handle all the named arguments
            foreach (var argument in arguments)
            {
                if (!SwitchParsers.SplitKeyValue(argument, out var key, out var value)) continue;
                // The argument is of the format 'file=abc'
                result.Add(key, value);
                used.Add(argument);

                bool found = false;
                foreach (var (argName, _, _) in expected)
                {
                    // ReSharper disable once InvertIf
                    if (argName.Equals(key))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new ArgumentException(
                        $"Found a parameter with key {key}, but no such parameter is defined for this switch.\n\n{Help()}");
                }
            }

            // Now, we handle the resting elements
            foreach (var argument in arguments)
            {
                if (SwitchParsers.SplitKeyValue(argument, out _, out _)) continue;

                // We found an argument which does not use "="
                // Was it used already?
                var name = expected[index].argName;
                while (result.ContainsKey(name))
                {
                    index++;
                    if (index > expected.Count)
                    {
                        throw new ArgumentException("Too many arguments are given");
                    }

                    name = expected[index].argName;
                }

                result.Add(name, argument);
                used.Add(argument);
            }


            // Did we use all arguments?
            if (arguments.Length != used.Count)
            {
                var unused = "";
                foreach (var argument in arguments)
                {
                    if (!used.Contains(argument))
                    {
                        unused += argument + ", ";
                    }
                }

                unused = unused.Substring(unused.Length - 2);
                throw new ArgumentException($"Some arguments were not used: \n  {unused}");
            }


            // At last, do we have all obligated parameters?
            foreach (var (name, obligated, _) in expected)
            {
                if (obligated && !result.ContainsKey(name))
                {
                    throw new ArgumentException($"The argument '{name}' is missing");
                }
            }

            return result;
        }


        /// <summary>
        /// Creates a help text for this switch
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public string Help(bool markdown = false)
        {
            var text = "";

            if (markdown)
            {
                text += "### ";
                text += Names[0];
                if (Names.Length > 1)
                {
                    text += " (";
                    for (int i = 1; i < Names.Length; i++)
                    {
                        text += Names[i] + ", ";
                    }

                    text = text.Substring(0, text.Length - 2);
                    text += ")";
                }
            }
            else
            {
                foreach (var name in Names)
                {
                    text += name + " ";
                }
            }


            if (!_isStable)
            {
                text += " (Experimental feature)";
            }

            text += "\n";
            text += "   " + _about + "\n\n";

            if (markdown)
            {
                if (_extraParams.Count == 0)
                {
                    text += "\n\n*This switch does not need parameters*\n";
                }
                else
                {
                    text += "| Parameter  | Obligated? | Explanation       |\n";
                    text += "|----------- | ---------- | ----------------- |\n";
                }
            }

            foreach (var (argName, isObligated, comment) in _extraParams)
            {
                if (markdown)
                {
                    if (isObligated)
                    {
                        text += $"| **{argName}** | ✓ | {comment} | ";
                    }
                    else
                    {
                        text += $"| {argName} | | {comment} | ";
                    }
                }
                else
                {
                    text += $"   {argName}=*\n\t{comment}";
                    if (isObligated)
                    {
                        text += "(Obligated)";
                    }
                    else
                    {
                        text += "(Optional)";
                    }
                }

                text += "\n";
            }

            return text;
        }
    }
}