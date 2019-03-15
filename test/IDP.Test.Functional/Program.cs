using System;

namespace IDP.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            IDP.Program.Main(new[]
            {
                "--read-bin", 
                "source.osm.bin", 
                "--create-routerdb", 
                "bigtruck.lua", 
                "--write-shape",
                "source"
            });
        }
    }
}