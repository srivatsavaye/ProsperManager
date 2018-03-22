using Newtonsoft.Json;
using Prosper.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;

namespace ProsperLibrary
{
    public class ProsperManager
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private JsonSerializerSettings _jsonSerializerSettings;
        private AuthenticationToken _authenticationToken;
        private List<AccountSetting> _accountSettings;
        private AppSettings _appSettings;
        private AccountSetting _primaryAccount;
        private IProsperClient _prosperClient;
        private ILogger _logger;
        public ProsperManager(JsonSerializerSettings jsonSerializerSettings)
        {

        }

        public void Process()
        {
            if (_authenticationToken == null || _authenticationToken.Expired)
            {
                var resp = _prosperClient.AuthenticateAsync(_primaryAccount);
                resp.Wait();
                _authenticationToken = resp.Result;
            }

            var listings = _prosperClient.GetListingsAsync(_authenticationToken.AccessToken, 500);
            listings.Wait();

            var filters = new List<ListingFilter>
            {
                Get36M_01Filter(),
                Get36M_02Filter(),
                Get36M_03Filter(),
                Get36M_04Filter()
            };

            foreach (var filter in filters.OrderBy(f => f.Priority))
            {
                var listingsFilteredBy = listings.Result.Result.Where(
                    l => l.ProsperRating == filter.Rating
                         && l.ListingTerm == filter.Term
                         && l.ProsperScore >= filter.ProsperScore.From
                         && l.ProsperScore <= filter.ProsperScore.To
                         && filter.CreditScores.Contains(l.CreditBureauValuesTransunionIndexed.FicoScore)
                ).ToList();

                Log.Debug($"Filter- {filter} Matched-{listingsFilteredBy.Count}");
                if (listingsFilteredBy.Any())
                {
                    Log.Debug($"Matched Listings- {string.Join(",", listingsFilteredBy)}");
                    foreach (var listing in listingsFilteredBy)
                    {
                        var bidAmount = GetBidAmount(_authenticationToken.AccessToken);
                        //_prosperClient.OrderAsync(_authenticationToken.AccessToken, listing.ListingNumber, bidAmount);

                        foreach (var accountSetting in _accountSettings)
                        {
                            if (accountSetting.Username != _primaryAccount.Username)
                            {
                                var resp = _prosperClient.AuthenticateAsync(accountSetting);
                                resp.Wait();
                                var authenticationToken = resp.Result;
                                var currentBidAmount = GetBidAmount(authenticationToken.AccessToken);
                                //_prosperClient.OrderAsync(authenticationToken.AccessToken, listing.ListingNumber, currentBidAmount);
                            }
                        }
                    }
                }
            }
        }

        private double GetBidAmount(string accessToken)
        {
            var account = _prosperClient.GetAccountAsync(accessToken);
            var availableCashBalance = account.Result.AvailableCashBalance;
            var bidAmount = availableCashBalance > _appSettings.BidAmount
                ? _appSettings.BidAmount
                : availableCashBalance - _appSettings.BidAmount;
            return bidAmount;
        }

        private ListingFilter Get36M_01Filter()
        {
            return new ListingFilter
            {
                Name = "36M_01",
                Priority = 1,
                Rating = "HR",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(3, 5),
                CreditScores = new List<string> { "640-659", "680-699" }
            };
        }

        private ListingFilter Get36M_02Filter()
        {
            return new ListingFilter
            {
                Name = "36M_02",
                Priority = 2,
                Rating = "E",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(4, 6),
                CreditScores = new List<string> { "640-659", "720-739" }
            };
        }

        private ListingFilter Get36M_03Filter()
        {
            return new ListingFilter
            {
                Name = "36M_03",
                Priority = 3,
                Rating = "A",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(3, 3),
                CreditScores = new List<string> { "640-659", "660-679", "720-739", "740-759", "760-779", "780-799", "800-819", "820-850" }
            };
        }

        private ListingFilter Get36M_04Filter()
        {
            return new ListingFilter
            {
                Name = "36M_04",
                Priority = 4,
                Rating = "A",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(9, 11),
                CreditScores = new List<string> { "640-659", "660-679", "720-739", "740-759", "760-779", "780-799", "800-819", "820-850" }
            };
        }



        public AuthenticationToken authenticationToken { get; set; }
    }
}
