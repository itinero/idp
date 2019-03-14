using System;

namespace IDP.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            IDP.Program.Main(new[]
            {
                "--read-routerdb", 
                "temp.routerdb", 
                "--remove-restrictions", 
                "--write-routerdb", 
                "temp.1.routerdb"
            });
        }
    }
}