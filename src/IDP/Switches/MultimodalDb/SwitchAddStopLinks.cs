﻿// The MIT License (MIT)

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
using IDP.Processors.MultimodalDb;
using Itinero.Profiles;
using Itinero.Transit.Data;
// ReSharper disable NotResolvedInText

namespace IDP.Switches.MultimodalDb
{
    /// <summary>
    /// A switch to add links to a multimodal db.
    /// </summary>
    class SwitchAddStopLinks : Switch
    {
        /// <summary>
        /// Creates a switch to add links to a multimodal db.
        /// </summary>
        public SwitchAddStopLinks(string[] a)
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
                return new[] { "--add-links" };
            }
        }

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            var profileName = "pedestrian"; // default profile.
            var distance = 100; // default distance 100m.

            for (var i = 0; i < Arguments.Length; i++)
            {
                string key, value;
                if (SwitchParsers.SplitKeyValue(Arguments[i], out key, out value))
                {
                    switch (key.ToLower())
                    {
                        case "profile":
                            profileName = value;
                            break;
                        case "distance":
                            if (!int.TryParse(value, out distance))
                            {
                                throw new ArgumentException("--add-links",
                                    "Invalid parameter value for command --add-links: distance not a number.");
                            }
                            break;
                        default:
                            throw new ArgumentException("--add-links",
                                string.Format("Invalid parameter for command --add-links: {0} not recognized.", key));
                    }
                }
            }

            Profile profile;
            if (!Profile.TryGet(profileName, out profile))
            {
                throw new ArgumentException("--add-links",
                    string.Format("Invalid parameter value for command --add-links: profile {0} not found.", profileName));
            }

            if (!(previous[previous.Count - 1] is IProcessorMultimodalDbSource source))
            {
                throw new Exception("Expected a multimodal db stream source.");
            }

            Func<Itinero.Transit.Data.MultimodalDb> getMultimodalDb = () =>
            {
                var db = source.GetMultimodalDb();

                db.AddStopLinksDb(profile, maxDistance: distance);

                return db;
            };
            processor = new ProcessorMultimodalDbSource(getMultimodalDb);

            return 1;
        }
    }
}