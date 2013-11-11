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
        public static void Main(string[] args)
        {
            UcscHumanGenomeParser.ParseMaf("//VBOXSVR/raw/maf.txt", "//VBOXSVR/parsed/maf_compressed.txt");
            //UcscHumanGenomeParser.ParseMaf("../../Data/maf.txt", "../../Data/maf_compressed.txt");
            /*
            var genes = UcscHumanGenomeParser.ParseFile("../../Data/ucsc-human-default.txt");
            Console.Out.WriteLine("Creating LCList");
            var lclist = new LayeredContainmentList<UcscHumanGene, GenomePosition>(genes);
            Console.Out.WriteLine("Done creating LCList");
            Console.Out.WriteLine(lclist.Count);
            Console.ReadLine();
             */
        }
    }
}
