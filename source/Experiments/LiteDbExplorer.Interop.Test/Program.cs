using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDbExplorer.Interop;

namespace LiteDbExplorer.Interop.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            DatabaseVersionsTest.CreateDatabases();

            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
