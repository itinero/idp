//// The MIT License (MIT)

//// Copyright (c) 2016 Ben Abelshausen

//// Permission is hereby granted, free of charge, to any person obtaining a copy
//// of this software and associated documentation files (the "Software"), to deal
//// in the Software without restriction, including without limitation the rights
//// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//// copies of the Software, and to permit persons to whom the Software is
//// furnished to do so, subject to the following conditions:

//// The above copyright notice and this permission notice shall be included in
//// all copies or substantial portions of the Software.

//// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//// THE SOFTWARE.

//using System;
//using System.Collections.Generic;
//using IDP.Processors;
//using Itinero.Transit.Data;
//using Itinero.Transit.Osm.Data;
//using Itinero.Profiles;

//namespace IDP.Switches.TransitDb
//{
//    /// <summary>
//    /// A switch to add transfers to a transit db.
//    /// </summary>
//    class SwitchAddTransfersDb : Switch
//    {
//        /// <summary>
//        /// Creates a switch to add transfers to a transit db.
//        /// </summary>
//        public SwitchAddTransfersDb(string[] a)
//            : base(a)
//        {

//        }

//        /// <summary>
//        /// Gets the names.
//        /// </summary>
//        public static string[] Names
//        {
//            get
//            {
//                return new string[] { "--add-transfers" };
//            }
//        }

//        /// <summary>
//        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
//        /// </summary>
//        public override int Parse(List<Processor> previous, out Processor processor)
//        {
//            var profileName = string.Empty; // no default profile.
//            var distance = 100; // default distance 100m.

//            for (var i = 0; i < this.Arguments.Length; i++)
//            {
//                string key, value;
//                if (SwitchParsers.SplitKeyValue(this.Arguments[i], out key, out value))
//                {
//                    switch (key.ToLower())
//                    {
//                        case "profile":
//                            profileName = value;
//                            break;
//                        case "distance":
//                            if (!int.TryParse(value, out distance))
//                            {
//                                throw new SwitchParserException("--add-transfers",
//                                    "Invalid parameter value for command --add-transfers: distance not a number.");
//                            }
//                            break;
//                        default:
//                            throw new SwitchParserException("--add-transfers",
//                                string.Format("Invalid parameter for command --add-transfers: {0} not recognized.", key));
//                    }
//                }
//            }

//            Profile profile;
//            if (!Profile.TryGet(profileName, out profile))
//            {
//                throw new SwitchParserException("--add-transfers",
//                    string.Format("Invalid parameter value for command --add-transfers: profile {0} not found.", profileName));
//            }

//            if (!(previous[previous.Count - 1] is Processors.TransitDb.IProcessorTransitDbSource))
//            {
//                throw new Exception("Expected a transit db stream source.");
//            }

//            var source = (previous[previous.Count - 1] as Processors.TransitDb.IProcessorTransitDbSource).GetTransitDb;
//            Func<Itinero.Transit.Data.TransitDb> getTransitDb = () =>
//            {
//                var db = source();

//                db.AddTransfersDb(profile, distance);

//                return db;
//            };
//            processor = new Processors.TransitDb.ProcessorTransitDbSource(getTransitDb);

//            return 1;
//        }
//    }
//}