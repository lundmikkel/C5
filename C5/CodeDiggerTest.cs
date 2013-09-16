using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5 {
    class CodeDiggerTest {
        public string RevString(String s)
        {
            if (s.Equals("Citron"))
                return "Fredag";
            var array = s.ToCharArray();
            Array.Reverse(array);
            return null;
        }

        public static int Sum(int[] values) {
            return values.Sum();
        }

        public static void Puzzle(int[] a) {
            if (a == null) return;
            if (a.Length > 0)
                if (a[3] == 1234567890)
                    throw new Exception("bug");
        }

        //More Complex Example
        public static decimal CalculateCandyPricePerServing(IEnumerable<BagOfCandy> bagsOfCandy) {
            return bagsOfCandy.Average(p => p.Servings / p.Price);
        }
        //A Bag of Candy
        public class BagOfCandy {
            public int Servings { get; set; }
            public decimal Price { get; set; }
            public BagOfCandy() { }
        }
    }
}
