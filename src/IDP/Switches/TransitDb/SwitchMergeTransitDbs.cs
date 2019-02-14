// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

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

using System;
using System.Collections.Generic;
using IDP.Processors;
using IDP.Processors.TransitDb;
using Itinero.Transit.Data;
// ReSharper disable NotResolvedInText

namespace IDP.Switches.TransitDb
{
    /// <summary>
    /// A switch to merge transit db's together into one.
    /// </summary>
    class SwitchMergeTransitDbs : Switch
    {
        /// <summary>
        /// Creates the switch.
        /// </summary>
        public SwitchMergeTransitDbs(string[] a)
            : base(a)
        {

        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        public static string[] Names
        {
            get
            {
                return new[] { "--merge" };
            }
        }

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            if (previous == null || previous.Count == 0) { throw new ArgumentOutOfRangeException("processors"); }

            // ok combine all the previous transit db's.
            var getTransitDbs = new List<Func<Itinero.Transit.Data.TransitDb>>();
            while (getTransitDbs.Count < previous.Count &&
                previous[previous.Count - getTransitDbs.Count - 1] is IProcessorTransitDbSource src) 
            {
                getTransitDbs.Add(
                    src.GetTransitDb);
            }

            if (getTransitDbs.Count < 2)
            {
                throw new Exception("No transit db's found to merge.");
            }

            processor = new ProcessorTransitDbSource(() =>
            {
                var transitDb = new Itinero.Transit.Data.TransitDb();
                for (var i = 0; i < getTransitDbs.Count; i++)
                {
                    transitDb.CopyFrom(getTransitDbs[i]());
                }
                transitDb.SortConnections(DefaultSorting.DepartureTime, null);
                transitDb.SortStops();

                return transitDb;
            });

            return getTransitDbs.Count;
        }
    }
}
