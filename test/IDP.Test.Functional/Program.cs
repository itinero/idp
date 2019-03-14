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
                "--dump-profile-speeds", 
                "car",
                "car.csv"
            });
        }
    }
}