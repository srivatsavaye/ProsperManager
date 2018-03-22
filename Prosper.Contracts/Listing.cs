using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prosper.Contracts
{
    public class Listing
    {
        public long ListingNumber { get; set; }
        public string ProsperRating { get; set; }
        public int ListingTerm { get; set; }
        public int ProsperScore { get; set; }
        public CreditBureau CreditBureauValuesTransunionIndexed { get; set; }
        public string ListingTitle { get; set; }
        public double AmountRemaining { get; set; }

        public override string ToString()
        {
            return $"ListingNumber:{ListingNumber} ProsperRating:{ProsperRating} ListingTerm:{ListingTerm} ProsperScore:{ProsperScore} CreditBureau:{CreditBureauValuesTransunionIndexed} ListingTitle:{ListingTitle} AmountRemaining:{AmountRemaining} ";
        }
    }

    public class CreditBureau
    {
        public string FicoScore { get; set; }
        public override string ToString()
        {
            return $"FicoScore:{FicoScore}"; 
        }
    }

    public class ListingResult
    {
        public List<Listing> Result { get; set; }
        public int ResultCount { get; set; }
        public int TotalCount { get; set; }

    }
}
