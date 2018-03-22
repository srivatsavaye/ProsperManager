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
        public int Priority { get; set; }
        public override string ToString()
        {
            return
                $"Name:{Name} Rating:{Rating} Term:{Term} ProsperScore:{ProsperScore.From}-{ProsperScore.To} CreditScores:{string.Join(",", CreditScores)}";
        }
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
        public override string ToString()
        {
            return $"From:{From}-To:{To}"; 
        }
    }
}
