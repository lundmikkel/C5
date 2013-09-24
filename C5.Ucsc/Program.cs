using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C5.intervals;

namespace C5.Ucsc
{
    class Program
    {
        static void Main(string[] args)
        {
            var genes = UcscHumanGenomeParser.ParseFile("../../Data/ucsc-human-default.txt");
            Console.Out.WriteLine("Creating LCList");
            var lclist = new LayeredContainmentList2<GenomePosition>(genes);
            Console.Out.WriteLine("Done creating LCList");
            Console.Out.WriteLine(lclist.Count);
            Console.ReadLine();
        }
    }
}
