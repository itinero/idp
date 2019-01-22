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

using System;
using System.Collections.Generic;
using IDP.Processors;

namespace IDP.Switches
{
    /// <summary>
    /// A switch is a command line flag, such as '--write-geojson output.file'.
    ///
    /// A class implementing 'switch' has a name (--write-geojson) and
    /// is responsible for constructing a processor based on arguments ("output.file") in this case
    ///
    /// It does this in the 'ParseArguments'-method
    /// 
    /// </summary>
    internal abstract class Switch
    {
        private readonly string[] _arguments;

        /// <summary>
        /// Creates a new switch.
        /// </summary>
        /// <param name="arguments">The command line arguments that were typed</param>
        protected Switch(string[] arguments)
        {
            _arguments = arguments;
        }

        /// <summary>
        /// Gets the arguments for this switch that are given with the programs invocation.
        /// </summary>
        protected string[] Arguments => _arguments;

        /// <summary>
        /// Constructs a processor based on the command line arguments
        /// </summary>
        public abstract int Parse(List<Processor> previous, out Processor processor);


        
    }
}