using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

namespace C5.Ucsc
{
    class UcscGenomeBank
    {
    }

    class UcscHumanGenomeParser
    {
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
