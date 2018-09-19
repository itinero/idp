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
using System.IO;
using Itinero;

namespace IDP.Switches.RouterDb
{
    /// <summary>
    /// A switch to read or open a routerdb.
    /// </summary>
    internal class SwitchReadRouterDb : Switch
    {
        /// <summary>
        /// Creates a new switch.
        /// </summary>
        public SwitchReadRouterDb(string[] a)
            : base(a)
        {

        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        public static string[] Names => new string[] { "--read-routerdb" };

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            if (this.Arguments.Length < 1) { throw new ArgumentException("At least one argument is expected."); }

            var fileName = string.Empty;
            var mapped = false;
            if (this.Arguments.Length == 1)
            { // just the single argument, this is supposed to be the filename.

                if (SwitchParsers.SplitKeyValue(this.Arguments[0], out var key, out var value))
                { // this is a pair, should file={filename}
                    if (key != "file")
                    {
                        throw new ArgumentException("Only one argument found and it's not the filename.");
                    }
                    fileName = value;
                }
                else
                { // this should be the raw filename.
                    fileName = this.Arguments[0];
                }
            }
            else
            {
                // try to parse multiple arguments.
                for (var i = 0; i < this.Arguments.Length; i++)
                {
                    if (!SwitchParsers.SplitKeyValue(this.Arguments[i], out var key, out var value))
                    { // this is a pair, should file={filename}
                        throw new ArgumentException($"Cannot parse argument {i}: {this.Arguments[i]}");
                    }

                    switch (key)
                    {
                        case "file":
                            fileName = value;
                            break;
                        case "m":
                        case "mapped":
                            mapped = SwitchParsers.IsTrue(value);
                            break;
                        default:
                            throw new ArgumentException($"Unknown argument {i}: {this.Arguments[i]}");
                    }
                }
            }
            
            // check if the file exists.
            var localFile = Downloader.DownloadOrOpen(fileName);
            var file = new FileInfo(localFile);
            if (!file.Exists)
            {
                throw new FileNotFoundException("File not found.", file.FullName);
            }

            Itinero.RouterDb GetRouterDb()
            {
                if (mapped)
                { // use the mapped version of the routerdb.
                    // WARNING: the source stream will remain open and cannot be written to.
                    Itinero.Logging.Logger.Log(nameof(SwitchReadRouterDb), Itinero.Logging.TraceEventType.Information, 
                        "Opening RouterDb: " + file.FullName);
                    var stream = file.OpenRead();
                    return Itinero.RouterDb.Deserialize(stream, RouterDbProfile.NoCache);
                }
                else
                {
                    // load the entire routerdb in RAM.
                    Itinero.Logging.Logger.Log(nameof(SwitchReadRouterDb), Itinero.Logging.TraceEventType.Information, 
                        "Reading RouterDb: " + file.FullName);
                    using (var stream = file.OpenRead())
                    {
                        return Itinero.RouterDb.Deserialize(stream);
                    }
                }
            };

            processor = new Processors.RouterDb.ProcessorRouterDbSource(GetRouterDb);

            return 0;
        }
    }
}