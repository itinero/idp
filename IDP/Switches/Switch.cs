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

using IDP.Processors;
using System.Collections.Generic;

namespace IDP.Switches
{
    /// <summary>
    /// Represents a switch.
    /// </summary>
    abstract class Switch
    {
        private readonly string[] _arguments;

        /// <summary>
        /// Creates a new switch.
        /// </summary>
        public Switch(string[] arguments)
        {
            _arguments = arguments;
        }

        /// <summary>
        /// Gets the arguments for this switch.
        /// </summary>
        public string[] Arguments
        {
            get
            {
                return _arguments;
            }
        }

        /// <summary>
        /// Parses this switch.
        /// </summary>
        public abstract int Parse(List<Processor> previous, out Processor processor);
    }
}