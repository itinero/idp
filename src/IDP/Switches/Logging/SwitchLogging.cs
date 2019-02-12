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
using System.Globalization;
using IDP.Processors;
using OsmSharp.Logging;
using Serilog;
using static IDP.Switches.SwitchesExtensions;

namespace IDP.Switches.Logging
{
    class SwitchLogging : DocumentedSwitch
    {
        private static readonly string[] names = {"--log"};

        private const string About =
            "If specified, creates a logfile where all the output will be written to - useful to debug a custom routing profile";

        private static readonly List<(List<string> argName, bool isObligated, string comment, string defaultValue)> ExtraParams =
            new List<(List<string> argName, bool isObligated, string comment, string defaultValue)>()
            {
                opt("file", "The name of the file where the logs will be written to").SetDefault("log.txt")
            };

        private const bool IsStable = true;


        public SwitchLogging() :
            base(names, About, ExtraParams, IsStable)
        {
        }


        public override (Processor, int nrOfUsedProcessors) Parse(Dictionary<string, string> arguments,
            List<Processor> previous)
        {
            // enable logging by adding serilog
            Logger.LogAction = (origin, level, message, parameters) =>
            {
                Console.WriteLine(
                    $"[{origin}-{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] {level} - {message}");
                Log.Information(string.Format(
                    $"[{origin}-{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] {level} - {message}"));
            };
            Itinero.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                Console.WriteLine(
                    $"[{origin}-{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] {level} - {message}");
                Log.Information(string.Format(
                    $"[{origin}-{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] {level} - {message}"));
            };

            var logFile = arguments["file"];
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFile)
                .CreateLogger();

            return (null, 0);
        }
    }
}