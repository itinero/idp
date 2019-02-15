using System.Collections.Generic;

namespace IDP.Switches
{
    public static class SwitchesExtensions
    {
        public static (List<string> args, bool isObligated, string comment, string defaultValue) obl(string argName,
            string comment)
        {
            return (new List<string> {argName}, true, comment, "");
        }

        public static (List<string> args, bool isObligated, string comment, string defaultValue) obl(string argName,
            string argName0, string comment)
        {
            return (new List<string> {argName, argName0}, true, comment, "");
        }


        public static (List<string> args, bool isObligated, string comment, string defaultValue) opt(string argName,
            string comment)
        {
            return (new List<string> {argName}, false, comment, "");
        }

        public static (List<string> args, bool isObligated, string comment, string defaultValue) opt(string argName,
            string argName0, string comment)
        {
            return (new List<string> {argName, argName0}, false, comment, "");
        }

        public static (List<string>argName, bool isObligated, string comment, string defaultValue) SetDefault(
            this (List<string> args, bool isObligated, string comment, string def) tuple, string defaultValue)
        {
            return (tuple.args, tuple.isObligated, tuple.comment, defaultValue);
        }
    }
}