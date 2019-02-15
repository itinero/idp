using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IDP.Processors;

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
        public readonly string About;

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
        private readonly List<(List<string> argNames, bool isObligated, string comment, string defaultValue)>
            _extraParams;


        /// <summary>
        /// Should this switch be clearly showed in the documentation?
        /// </summary>
        /// <returns></returns>
        public readonly bool IsStable;

        protected DocumentedSwitch(
            string[] names, string about,
            List<(List<string> argName, bool isObligated, string comment, string defaultValue)> extraParams,
            bool isStable
        ) : base(new string[] { })
        {
            Names = names;
            About = about;
            _extraParams = extraParams;
            IsStable = isStable;
        }


        protected abstract (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous);

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
        /// The resulting dictionary will 
        /// 
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, string> ParseExtraParams(string[] arguments)
        {
            var result = new Dictionary<string, string>();

            var expected = _extraParams;
            var index = 0;

            var used = new HashSet<string>();

            // First, handle all the named arguments, thus every element containing '='
            // We _translate_ the argument name to the first index in the according list
            foreach (var argument in arguments)
            {
                if (!SwitchParsers.SplitKeyValue(argument, out var key, out var value)) continue;
                // The argument is of the format 'file=abc'


                var found = false;
                foreach (var (argNames, _, _, _) in expected)
                {
                    foreach (var argName in argNames)
                    {
                        // ReSharper disable once InvertIf
                        if (argName.Equals(key))
                        {
                            found = true;
                            key = argNames[0];
                            break;
                        }

                        if (found)
                        {
                            break;
                        }
                    }
                }

                if (!found)
                {
                    throw new ArgumentException(
                        $"Found a parameter with key {key}, but no such parameter is defined for this switch.\n\n{Help()}");
                }


                result.Add(key, value);
                used.Add(argument);
            }
            
            
            // Now, we handle all the boolean arguments, thus every element starting with '-', e.g. '-some-flag'
            // We _translate_ the argument name to the first index in the according list
            foreach (var argument in arguments)
            {
                if(used.Contains(argument)) continue;
                
                if (!argument.StartsWith("-")) continue;
                // The argument is of the format '-flag'

                var key = argument.Substring(1);
                var found = false;
                foreach (var (argNames, _, _, _) in expected)
                {
                    foreach (var argName in argNames)
                    {
                        // ReSharper disable once InvertIf
                        if (argName.Equals(key))
                        {
                            found = true;
                            key = argNames[0];
                            break;
                        }

                        if (found)
                        {
                            break;
                        }
                    }
                }

                if (!found)
                {
                    throw new ArgumentException(
                        $"Found a flag with key {key}, but no such flag is defined for this switch.\n\n{Help()}");
                }


                result.Add(key, "true");
                used.Add(argument);
            }


            // Now, we handle the resting elements without a name
            foreach (var argument in arguments)
            {
                if (used.Contains(argument)) continue;

                // We found an argument which does not use "="
                // Was it used already?
                var name = expected[index].argNames[0];
                while (result.ContainsKey(name))
                {
                    index++;
                    if (index > expected.Count)
                    {
                        throw new ArgumentException("Too many arguments are given");
                    }

                    name = expected[index].argNames[0];
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
            // And we fill out the default values if not obligated
            foreach (var (name, obligated, _, defaultValue) in expected)
            {
                if (obligated && !result.ContainsKey(name[0]))
                {
                    throw new ArgumentException($"The argument '{name[0]}' is missing");
                }

                if(!result.ContainsKey(name[0]))
                {
                    // Add default of optional parameter
                    result.Add(name[0], defaultValue);
                }
            }

            return result;
        }

        public string MarkdownName()
        {
            var text = "";
            text += Names[0];
            if (Names.Length > 1)
            {
                text += " (";
                for (var i = 1; i < Names.Length; i++)
                {
                    text += Names[i] + ", ";
                }

                text = text.Substring(0, text.Length - 2);
                text += ")";
            }

            if (!IsStable)
            {
                text += " (Experimental feature)";
            }

            return text;
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
                text += "#### " + MarkdownName();
            }
            else
            {
                text += string.Join(", ", Names);
            }


            text += "\n\n";
            text += "   " + About + "\n\n";

            if (markdown)
            {
                if (_extraParams.Count == 0)
                {
                    text += "\n\n*This switch does not need parameters*\n";
                }
                else
                {
                    text += "| Parameter  | Default value | Explanation       |\n";
                    text += "|----------- | ------------- | ----------------- |\n";
                }
            }

            foreach (var (argNames, isObligated, comment, defaultValue) in _extraParams)
            {
                var argName = string.Join(", ", argNames);


                if (markdown)
                {
                    if (isObligated)
                    {
                        text += $"| **{argName}** | _Obligated param_ | {comment} | ";
                    }
                    else
                    {
                        var defV = String.IsNullOrEmpty(defaultValue) ? "_NA_" : $"`{defaultValue}`";
                        text += $"| {argName} | {defV}| {comment} | ";
                    }
                }
                else
                {
                    text += $"   {argName}=*\n\t{comment}";
                    if (isObligated)
                    {
                        text += " (Obligated)";
                    }
                    else
                    {
                        text += $" (Optional, default is {defaultValue})";
                    }
                }

                text += "\n";
            }

            return text;
        }


    }
}