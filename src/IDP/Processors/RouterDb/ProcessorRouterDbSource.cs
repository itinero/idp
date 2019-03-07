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

namespace IDP.Processors.RouterDb
{
    /// <summary>
    /// Represents a processor to get a router db.
    /// </summary>
    class ProcessorRouterDbSource : Processor, IProcessorRouterDbSource
    {
        private readonly Func<Itinero.RouterDb> _getRouterdb;

        /// <summary>
        /// Creates a new processor router db.
        /// </summary>
        public ProcessorRouterDbSource(Func<Itinero.RouterDb> getRouterDb)
        {
            _getRouterdb = getRouterDb;
        }

        /// <summary>
        /// Gets a router db.
        /// </summary>
        public Func<Itinero.RouterDb> GetRouterDb
        {
            get
            {
                return _getRouterdb;
            }
        }

        /// <summary>
        /// Returns true if this processor can execute.
        /// </summary>
        public override bool CanExecute
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Executes this processor.
        /// </summary>
        public override void Execute()
        {
            // We simply execute the routerDB. It might have side effects (such as printing stuff)
            // We do not need the result and throw it away
            _getRouterdb();
        }
    }
}