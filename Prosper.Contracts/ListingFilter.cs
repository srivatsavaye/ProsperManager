using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prosper.Contracts
{
    public class ListingFilter
    {
        public string Name { get; set; }
        public string Rating { get; set; }
        public int Term { get; set; }
        public List<string> LoanCategories { get; set; }
        public Range ProsperScore { get; set; }
        public List<string> CreditScores { get; set; }
    }


    public class Range
    {
        public Range(int from, int to)
        {
            From = from;
            To = to;
        }
        public int From { get; set; }
        public int To { get; set; }
    }
}
