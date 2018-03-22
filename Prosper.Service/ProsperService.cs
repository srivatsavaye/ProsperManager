using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prosper.Contracts;
using ProsperLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Prosper.Service
{
    public partial class ProsperService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private JsonSerializerSettings _jsonSerializerSettings;
        private AuthenticationToken _authenticationToken;
        private List<AccountSetting> _accountSettings;
        private AppSettings _appSettings;
        private AccountSetting _primaryAccount;
        private IProsperClient _prosperClient;
        private ILogger _logger;
        public ProsperService()
        {
            _logger = new RootLogger(Level.Alert);
            InitializeComponent();
            SetJsonSerializationSettings();
            var settings = GetSettings();
            _appSettings = settings.Item1;
            _accountSettings = settings.Item2;
            _primaryAccount = _accountSettings[0];
            _prosperClient = new ProsperClient(new Client(), settings.Item1, _jsonSerializerSettings);
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            var timer = new Timer(Process, null, 0, 120000);
            //while (true)
            //{
            //    Process();

            //    if (NeedToSleep())
            //    {
            //        Thread.Sleep(1000 * 60);
            //    }
            //}
        }


        private bool NeedToSleep()
        {
            /*
             M to F: 11:00 AM and 5:00 PM Central Time
                S and S: 2:00 PM Central Time

             */
            var weekdayHours = new[] { 11, 17};
            var now = DateTime.Now;
            var weekdays = new[] {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday};
            if (weekdays.Contains(now.DayOfWeek) && IsTimeInRange(weekdayHours, 10, now))
            {
                return true;
            }

            var weekends = new[] {DayOfWeek.Saturday, DayOfWeek.Sunday};
            var weekendHours = new[] { 11, 17 };
            if (weekends.Contains(now.DayOfWeek) && IsTimeInRange(weekendHours, 10, now))
            {
                return true;
            }

            return false;
        }

        private bool IsTimeInRange(IEnumerable<int> hours, int rangeInMinutes, DateTime currentDate)
        {
            if(rangeInMinutes > 60)
                throw new Exception("Range can't be more than 60 minutes");
            foreach (var hour in hours)
            {
                var greaterThan = new TimeSpan(currentDate.Day, hour - 1, 60 - rangeInMinutes, 0);
                var lessThan = new TimeSpan(currentDate.Day, hour, rangeInMinutes, 0);
                if (currentDate.TimeOfDay >= greaterThan && currentDate.TimeOfDay < lessThan)
                {
                    return true;
                }
            }

            return false;
        }

        private void Process(object state)
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

        protected override void OnStop()
        {
        }


        private void SetJsonSerializationSettings()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            { ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() } };
        }

        private Tuple<AppSettings, List<AccountSetting>> GetSettings()
        {
            var accountSettings = ReadFile(ConfigurationManager.AppSettings["AccountFileLocation"]);
            if (!double.TryParse(ConfigurationManager.AppSettings["DefaultBidAmount"], out var defaultBidAmount))
            {
                defaultBidAmount = 25;
            }
            var appSetting = new AppSettings
            {
                BaseUri = ConfigurationManager.AppSettings["BaseUri"],
                ListingsBaseUri = ConfigurationManager.AppSettings["ListingsBaseUri"],
                BidAmount = defaultBidAmount
            };
            return new Tuple<AppSettings, List<AccountSetting>>(appSetting, accountSettings);
        }

        private List<AccountSetting> ReadFile(string accountFileLocation)
        {
            if (File.Exists(accountFileLocation))
            {
                return JsonConvert.DeserializeObject<List<AccountSetting>>(File.ReadAllText(accountFileLocation), _jsonSerializerSettings);
            }
            return null;
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
    }
}
