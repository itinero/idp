using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.Osm;
using IDP.Processors.RouterDb;
using Xunit;
using IDP.Switches;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Tests.Switches
{
    public class ParseSwitchesTests
    {
        [Fact]
        public void TestParseDict()
        {
            var args = new[] {"abc", "def=ghi", "jkl"};
            var d = new Dummy(new[] {"--test-flag"},
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("a", "Test"),
                    obl("x", "test"),
                    opt("def", "test")
                },
                false
            );

            var dict = d.ParseExtraParams(args);
            Assert.Equal(3, dict.Count);
            Assert.Equal("abc", dict["a"]);
            Assert.Equal("jkl", dict["x"]);
            Assert.Equal("ghi", dict["def"]);

            args = new[] {"a"};
            try
            {
                d.ParseExtraParams(args);
                Assert.True(false);
            }
            catch (ArgumentException e)
            {
                Assert.Equal("The argument 'x' is missing", e.Message);
            }
        }
    }

    internal class Dummy : DocumentedSwitch
    {
        public Dummy(string[] names,
            List<(List<string> argName, bool isObligated, string comment, string defaultValue)> extraParams,
            bool isStable) : base(
            names, "", extraParams, isStable)
        {
        }

        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            throw new System.NotImplementedException();
        }

        public virtual DocumentedSwitch SetArguments(string[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}