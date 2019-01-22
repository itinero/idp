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
            string[] names,
            List<(string argName, bool isObligated, string comment)> extraParams,
            bool isStable
        ) : base(arguments)
        {
            Names = names;
            _extraParams = extraParams;
            _isStable = isStable;
        }
        
        protected DocumentedSwitch(string[] names,
            List<(string argName, bool isObligated, string comment)> extraParams,
            bool isStable
        ) : this(new string[]{}, names, extraParams, isStable)
        {
        }

        protected DocumentedSwitch(string[] arguments,
            DocumentedSwitch cloneFrom) : this(arguments, cloneFrom.Names, cloneFrom._extraParams, cloneFrom._isStable)
        {
        }


        public abstract Processor Parse(Dictionary<string, string> arguments, List<Processor> previous);

        // Legacy, to be removed
        public abstract DocumentedSwitch SetArguments(string[] arguments);
        
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            // I'm not really keen on keeping track of the Argument in this object, hence my "translation" to make transition easier
            processor = Parse(ParseExtraParams(Arguments), previous);
            return 1;
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

            // First, handle all the named arguments
            foreach (var argument in arguments)
            {
                if (!argument.Contains("=")) continue;

                // The argument is of the format 'file=abc'
                var split = argument.IndexOf("=", StringComparison.Ordinal);
                var name = argument.Substring(0, split);
                var arg = argument.Substring(split + 1);
                result.Add(name, arg);
            }

            // Now, we handle the resting elements
            foreach (var argument in arguments)
            {
                if (argument.Contains("=")) continue;

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
            }


            // At last, do we have all obligated arguments?
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
        public string Help()
        {
            var text = "";

            foreach (var name in Names)
            {
                text += name + " ";
            }


            if (!_isStable)
            {
                text += " (Experimental feature)";
            }
            text += "\n";

            var parameters = _extraParams;
            foreach (var (argName, isObligated, comment) in parameters)
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

                text += "\n";
            }

            return text;
        }
    }
}