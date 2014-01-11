using System;
using C5.Intervals;

namespace C5.Ucsc
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var interval = IntervalExtensions.ParseIntInterval("[12:13)");

            //UcscHumanGenomeParser.ParseMafToAlignments("//VBOXSVR/raw/chr1.maf", "//VBOXSVR/parsed/chr1.compressed.txt");
            //Console.Out.WriteLine(interval);

            /*
            UcscHumanGenomeParser.ParseMafToAlignments("//VBOXSVR/raw/maf.txt", "//VBOXSVR/parsed/maf_compressed.txt");
            /*/
            var intervals = UcscHumanGenomeParser.ParseCompressedMaf("//VBOXSVR/parsed/chr1.compressed.txt");
            var lclist = new LayeredContainmentList<UcscHumanGenomeParser.SequenceInterval, int>(intervals);
            Console.Out.WriteLine(lclist.Count);
            Console.Out.WriteLine(lclist.ContainmentDegree);
            Console.Out.WriteLine(lclist.MaximumDepth);
            Console.Out.WriteLine(lclist.CountOverlaps(270000000));
            //*/

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

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
