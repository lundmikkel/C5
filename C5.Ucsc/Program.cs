using System;
using System.Collections.Generic;
using System.IO;
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
            //var interval = IntervalExtensions.ParseIntInterval("[12:13)");

            //UcscHumanGenomeParser.ParseMafToAlignments("//VBOXSVR/raw/maf.txt", "//VBOXSVR/parsed/maf_compressed.txt");
            //Console.Out.WriteLine(interval);

            var intervals = UcscHumanGenomeParser.ParseCompressedMaf("//VBOXSVR/parsed/chr1.compressed.maf");
            var lclist = new LayeredContainmentList<UcscHumanGenomeParser.UcscHumanAlignmentGene, int>(intervals);
            Console.Out.WriteLine(lclist.Count);
            Console.Out.WriteLine(lclist.ContainmentDegree);
            Console.Out.WriteLine(lclist.MaximumOverlap);
            Console.Out.WriteLine(lclist.CountOverlaps(270000000));

            //Console.Out.WriteLine("Number of intervals: " + File.ReadLines(@"//VBOXSVR/parsed/chr1.compressed.maf").Count());
            //UcscHumanGenomeParser.ParseMaf("//VBOXSVR/raw/chr1.maf", "//VBOXSVR/parsed/maf_compressed.txt");
            //UcscHumanGenomeParser.ParseMaf("../../Data/maf.txt", "../../Data/maf_compressed.txt");
            /*
            var genes = UcscHumanGenomeParser.ParseFile("../../Data/ucsc-human-default.txt");
            Console.Out.WriteLine("Creating LCList");
            var lclist = new LayeredContainmentList<UcscHumanGene, GenomePosition>(genes);
            Console.Out.WriteLine("Done creating LCList");
            Console.Out.WriteLine(lclist.Count);
             */
            Console.ReadLine();
        }
    }
}
