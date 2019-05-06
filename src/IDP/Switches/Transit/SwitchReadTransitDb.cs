// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.IO;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Transit
{
    /// <summary>
    /// Represents a switch to read a shapefile for routing.
    /// </summary>
    class SwitchReadTransitDb : DocumentedSwitch
    {
        private static readonly string[] _names = {"--read-transit-db", "--read-transit","--read-tdb", "--rt"};

        private static string _about = "Read a transitDB file as input to do all the data processing. A transitDB is a database containing connections between multiple stops";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    obl("file", "The input file to read"),
                };

        private const bool _isStable = true;


        public SwitchReadTransitDb()
            : base(_names, _about, _extraParams, _isStable)
        {
        }


        protected override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            var fileName = arguments["file"];

            TransitDb GetTransitDb()
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return TransitDb.ReadFrom(stream, 0);
                }
            }

            return (new ProcessorTransitDbSource(GetTransitDb), 0);
        }
    }
}