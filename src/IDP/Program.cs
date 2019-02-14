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
using System.Globalization;
using IDP.Processors;

namespace IDP
{
    class Program
    {
        static void Main(string[] args)
        {
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}-{3}] {1} - {2}", origin, level, message,
                    DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            };
            Itinero.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}-{3}] {1} - {2}", origin, level, message,
                    DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            };

            // register switches.
            Switches.SwitchParsers.RegisterAll();

            // parses arguments.
            Processor processor;
            try
            {
                processor = Switches.SwitchParsers.Parse(args);
            }
            catch (ArgumentException e)
            {
                Exception exc = e;
                Console.WriteLine("Parsing arguments failed:");
                do
                {
                    Console.WriteLine($"  {exc.Message}");
                    exc = exc.InnerException;
                } while (exc != null);
                
                
                return ;
            }

            var ticks = DateTime.Now.Ticks;
            processor.Execute();
            Itinero.Logging.Logger.Log("Program", Itinero.Logging.TraceEventType.Information,
                "Processing finished, took {0}.",
                (new TimeSpan(DateTime.Now.Ticks - ticks)).ToString());
        }
    }
}