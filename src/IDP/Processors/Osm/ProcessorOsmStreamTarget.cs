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
using OsmSharp.Streams;
using System;

namespace IDP.Processors.Osm
{
    /// <summary>
    /// Represents a processor that represents an osm stream target.
    /// </summary>
    class ProcessorOsmStreamTarget : Processor
    {
        private readonly OsmStreamTarget _target;

        /// <summary>
        /// Creates a new processor target.
        /// </summary>
        public ProcessorOsmStreamTarget(OsmStreamTarget target)
        {
            _target = target;
        }
        
        /// <summary>
        /// Collapses this processor.
        /// </summary>
        public virtual int Collapse(List<Processor> processors, int i)
        {
            if (processors == null) { throw new ArgumentNullException("processors"); }
            if (processors.Count == 0) { throw new ArgumentOutOfRangeException("processors", "There has to be at least on processor there to collapse this target."); }
            if (processors[processors.Count - 1] == null) { throw new ArgumentOutOfRangeException("processors", "The last processor in the processors list is null."); }
            if (i < 1) { throw new ArgumentOutOfRangeException("i"); }
            
            // take the last processor and collapse.
            if (processors[i - 1] is IProcessorOsmStreamSource)
            { // ok, processor is a source.
                var source = processors[i - 1] as IProcessorOsmStreamSource;
                processors.RemoveAt(i - 1);

                _target.RegisterSource(source.Source);
                return -1;
            }
            throw new InvalidOperationException("Last processor before filter is not a source.");
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
            if (!this.CanExecute) { throw new InvalidOperationException("Cannot execute processor!"); }

            _target.Pull();
            _target.Close();
        }
    }
}