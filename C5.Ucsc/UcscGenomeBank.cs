using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Linq;
using C5.intervals;
using Microsoft.VisualBasic.FileIO;

namespace C5.Ucsc
{
    class UcscGenomeBank
    {
    }

    class UcscHumanGenomeParser
    {
        public static IEnumerable<UcscHumanAlignmentGene> ParseCompressedMaf(string source)
        {
            var intervals = new ArrayList<UcscHumanAlignmentGene>(3000000);

            using (var sr = new StreamReader(source))
            {
                while (!sr.EndOfStream)
                {
                    var parts = sr.ReadLine().Split(' ');

                    var low = Int32.Parse(parts[0]);
                    var high = Int32.Parse(parts[1]);
                    //var alignments = parts[2].Split(',').Select(Int32.Parse);

                    intervals.Add(new UcscHumanAlignmentGene(low, high));

                    if (intervals.Count % 100000 == 0)
                        Console.Out.WriteLine(intervals.Count);
                }
            }

            return intervals;
        }

        public static void ParseMaf(string source, string target)
        {
            using (var sr = new StreamReader(source))
            {
                using (var sw = new StreamWriter(target))
                {
                    var lineNumber = 0;
                    var sequenceNumber = 0;
                    var alignments = new ArrayList<int>();
                    var start = 0;
                    var length = 0;


                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        lineNumber++;

                        // Skip comments
                        if (line.StartsWith("#"))
                            continue;

                        // Skip blank lines and comments
                        if (String.IsNullOrWhiteSpace(line))
                        {
                            var gene = new UcscHumanAlignmentGene(start, length, alignments);
                            sw.WriteLine(gene.CompactFormat());
                            continue;
                        }

                        if (line.StartsWith("a"))
                        {
                            sequenceNumber++;
                            alignments = new ArrayList<int>();
                            continue;
                        }

                        if (line.StartsWith("s"))
                        {
                            var parts = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                            // Reference 
                            if (parts[1].Equals("hg19.chr1"))
                            {
                                start = Int32.Parse(parts[2]);
                                length = Int32.Parse(parts[3]);
                            }
                            else
                            {
                                alignments.Add(lineNumber);
                            }
                        }
                    }
                }
            }
        }

        public static void ParseMafToAlignments(string source, string target)
        {
            using (var sr = new StreamReader(source))
            {
                using (var sw = new StreamWriter(target))
                {
                    IInterval<int> humanGene = null;

                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();

                        // Skip comments
                        if (line.StartsWith("#"))
                            continue;

                        // Skip blank lines and comments
                        if (String.IsNullOrWhiteSpace(line))
                            continue;

                        if (line.StartsWith("a"))
                            continue;

                        if (line.StartsWith("s"))
                        {
                            var parts = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                            // Reference 
                            if (parts[1].Equals("hg19.chr1"))
                            {
                                var start = Int32.Parse(parts[2]);
                                var length = Int32.Parse(parts[3]);
                                humanGene = new IntervalBase<int>(start, start + length - 1, IntervalType.Closed);
                            }
                            else
                            {
                                var chromosome = parts[1].Split('.');
                                var type = Animal.Human;
                                var match = true;

                                switch (chromosome[0])
                                {
                                    case "panTro2":
                                        type = Animal.Chimp;
                                        break;
                                    case "papHam1":
                                        type = Animal.Baboon;
                                        break;
                                    case "mm9":
                                        type = Animal.Mouse;
                                        break;
                                    case "rn4":
                                        type = Animal.Rat;
                                        break;
                                    case "bosTau4":
                                        type = Animal.Cow;
                                        break;
                                    case "felCat3":
                                        type = Animal.Cat;
                                        break;
                                    case "canFam2":
                                        type = Animal.Dog;
                                        break;
                                    default:
                                        match = false;
                                        break;
                                }

                                if (match)
                                {
                                    var gene = new UcscAlignmentGene(Int32.Parse(parts[2]), Int32.Parse(parts[3]), type, chromosome[1], humanGene);
                                    sw.WriteLine(gene.CompactFormat());
                                }
                            }
                        }
                    }
                }
            }
        }

        internal struct UcscHumanAlignmentGene : IInterval<int>
        {
            public UcscHumanAlignmentGene(int start, int length, IEnumerable<int> alignments)
                : this()
            {
                Low = start;
                High = start + length - 1;
                Alignments = alignments;
            }

            public UcscHumanAlignmentGene(int low, int high)
                : this()
            {
                Low = low;
                High = high;
            }

            public int Low { get; private set; }
            public int High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return true; } }
            public IEnumerable<int> Alignments { get; private set; }

            public override string ToString()
            {
                return String.Format("{0} - {1}", IntervalExtensions.ToString(this), Alignments);
            }

            public string CompactFormat()
            {
                return String.Format("{0} {1} {2}", Low, High, string.Join(",", Alignments.Select(i => i.ToString())));
            }
        }

        internal enum Animal
        {
            [Description("Human")]
            Human,

            [Description("Chimp")]
            Chimp,

            [Description("Baboon")]
            Baboon,

            [Description("Mouse")]
            Mouse,

            [Description("Rat")]
            Rat,

            [Description("Cat")]
            Cat,

            [Description("Dog")]
            Dog,

            [Description("Cow")]
            Cow,

            [Description("Pig")]
            Pig
        }

        internal struct UcscAlignmentGene : IInterval<int>
        {
            public UcscAlignmentGene(int start, int length, Animal type, string chromosome, IInterval<int> humanGene)
                : this()
            {
                Low = start;
                High = start + length - 1;
                Type = type;
                Chromosome = chromosome;
                HumanGene = humanGene;
            }

            public int Low { get; private set; }
            public int High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return true; } }
            public Animal Type { get; private set; }
            public string Chromosome { get; private set; }
            public IInterval<int> HumanGene { get; private set; }

            public override string ToString()
            {
                return String.Format("{0}: {1} - Human: {2}", Type, IntervalExtensions.ToString(this), IntervalExtensions.ToString(HumanGene));
            }

            public string CompactFormat()
            {
                return String.Format("{0,-8} {1,-8} {2,10} {3,10} {4}", Type, Chromosome, Low, High, HumanGene);
            }
        }

        public static IEnumerable<UcscHumanGene> ParseFile(string filepath, bool headerRow = true)
        {
            var list = new ArrayList<UcscHumanGene>();

            using (var parser = new TextFieldParser(filepath) { Delimiters = new[] { "\t" } })
            {

                string[] parts;

                while ((parts = parser.ReadFields()) != null)
                {
                    // Skip header row
                    if (headerRow)
                    {
                        headerRow = false;
                        continue;
                    }

                    // Plain name
                    var name = parts[0];
                    // Chromosome number without "chr" prefix
                    var chromosome = parts[1];
                    // + or - for strand
                    var strand = parts[2].Equals("+") ? Strand.Plus : Strand.Minus;
                    // Transcription
                    var txStart = Int32.Parse(parts[3]);
                    var txEnd = Int32.Parse(parts[4]);
                    var transcription = new GenomeInterval(chromosome, txStart, txEnd);
                    // Coding region
                    var cdsStart = Int32.Parse(parts[5]);
                    var cdsEnd = Int32.Parse(parts[6]);
                    var codingRegion = new GenomeInterval(chromosome, cdsStart, cdsEnd);
                    // Exons
                    var exonCount = Int32.Parse(parts[7]);
                    var exonStarts = parts[8].Split(',');
                    var exonEnds = parts[9].Split(',');
                    var exons = new ArrayList<GenomeInterval>();
                    for (var i = 0; i < exonCount; i++)
                        exons.Add(new GenomeInterval(
                            chromosome,
                            Int32.Parse(exonStarts[i]),
                            Int32.Parse(exonEnds[i])
                        ));
                    // UniProt display ID for Known Genes, UniProt accession or RefSeq protein ID for UCSC Genes
                    var proteinId = parts[10];
                    // Unique identifier for each (known gene, alignment position) pair
                    var alignmentId = parts[11];

                    list.Add(new UcscHumanGene(
                        name,
                        chromosome,
                        strand,
                        transcription,
                        codingRegion,
                        exons,
                        proteinId,
                        alignmentId
                    ));
                }
            }

            return list;
        }
    }

    struct UcscHumanGene : IInterval<GenomePosition>
    {
        // Name of gene
        private readonly string _name;
        // Reference sequence chromosome or scaffold
        private readonly string _chrom;
        // Plus or Minus for strand
        private readonly Strand _strand;
        // Transcription
        private readonly GenomeInterval _transcription;
        // Coding region
        private readonly GenomeInterval _codingRegion;
        // Exons
        private readonly IEnumerable<GenomeInterval> _exons;
        // UniProt display ID for Known Genes, UniProt accession or RefSeq protein ID for UCSC Genes
        private readonly string _proteinId;
        // Unique identifier for each (known gene, alignment position) pair
        private readonly string _alignId;

        public UcscHumanGene(string name, string chrom, Strand strand, GenomeInterval transcription, GenomeInterval codingRegion, IEnumerable<GenomeInterval> exons, string proteinId, string alignId)
        {
            _name = name;
            _chrom = chrom;
            _strand = strand;
            _transcription = transcription;
            _codingRegion = codingRegion;
            _exons = exons;
            _proteinId = proteinId;
            _alignId = alignId;
        }

        public string Name { get { return _name; } }
        public string Chromosome { get { return _chrom; } }
        public Strand Strand { get { return _strand; } }
        public GenomeInterval Transcription { get { return _transcription; } }
        public GenomeInterval CodingRegion { get { return _codingRegion; } }
        public IEnumerable<GenomeInterval> Exons { get { return _exons; } }
        public string ProteinId { get { return _proteinId; } }
        public string AlignmentId { get { return _alignId; } }

        public override string ToString()
        {
            return String.Format("{0} {1}", _name, _transcription);
        }

        public GenomePosition Low { get { return _transcription.Low; } }
        public GenomePosition High { get { return _transcription.High; } }
        public bool LowIncluded { get { return true; } }
        public bool HighIncluded { get { return true; } }
    }

    struct GenomeInterval : IInterval<GenomePosition>
    {
        private readonly string _chrom;
        private readonly GenomePosition _low;
        private readonly GenomePosition _high;

        public GenomeInterval(string chromosome, int low, int high)
            : this()
        {
            _chrom = chromosome;
            _low = new GenomePosition(chromosome, low);
            _high = new GenomePosition(chromosome, high);
        }

        public GenomePosition Low { get { return _low; } }
        public GenomePosition High { get { return _high; } }
        public bool LowIncluded { get { return true; } }
        public bool HighIncluded { get { return true; } }

        public override string ToString()
        {
            return String.Format("{0}:{1}-{2}", _chrom, _low.Position, _high.Position);
        }
    }

    struct GenomePosition : IComparable<GenomePosition>
    {
        private readonly string _chrom;
        private readonly int _position;

        public GenomePosition(string chrom, int position)
            : this()
        {
            _chrom = chrom;
            _position = position;
        }

        public string Chromosome { get { return _chrom; } }
        public int Position { get { return _position; } }

        public int CompareTo(GenomePosition other)
        {
            var compare = _chrom.CompareTo(other._chrom);
            return compare != 0 ? compare : _position.CompareTo(other._position);
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", _chrom, _position);
        }
    }

    enum Strand
    {
        Plus,
        Minus
    }
}
