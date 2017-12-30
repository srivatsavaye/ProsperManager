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
using System.Threading.Tasks;

namespace Prosper.Service
{
    public partial class ProsperService : ServiceBase
    {
        private JsonSerializerSettings _jsonSerializerSettings;
        private AuthenticationToken _authenticationToken;
        private List<AccountSetting> _accountSettings;
        private AccountSetting _primaryAccount;
        private IProsperClient _prosperClient;
        public ProsperService()
        {
            InitializeComponent();
            SetJsonSerializationSettings();
            var settings = GetSettings();
            _accountSettings = settings.Item2;
            _primaryAccount = _accountSettings[0];
            _prosperClient = new ProsperClient(new Client(),settings.Item1, _jsonSerializerSettings);
        }

        protected override void OnStart(string[] args)
        {
            if (_authenticationToken.Expired)
            {
                var resp = _prosperClient.AuthenticateAsync(_primaryAccount);
                resp.Wait();
                _authenticationToken = resp.Result;
            }

            var listings = _prosperClient.GetListingsAsync(_authenticationToken.AccessToken, 500);
            listings.Wait();

            var filters = new List<ListingFilter> { Get36M_01Filter(), Get36M_04Filter() };


            foreach (var filter in filters.OrderBy(f => f.Priority))
            {
                var listingsFilteredBy = listings.Result.Result.Where(
                    l => l.ProsperRating == filter.Rating
                    && l.ListingTerm == filter.Term
                    && l.ProsperScore >= filter.ProsperScore.From
                    && l.ProsperScore <= filter.ProsperScore.To
                    && filter.CreditScores.Contains(l.CreditBureauValuesTransunionIndexed.FicoScore)
                    ).ToList();
                if (listingsFilteredBy.Any())
                {
                    foreach (var listing in listingsFilteredBy)
                    {
                        //TODO order by each listing
                    }
                }
            }
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
            var accountSettings = ReadFile(ConfigurationManager.AppSettings["AccountFileLocation"].ToString());
            var appSetting = new AppSettings
            {
                BaseUri = ConfigurationManager.AppSettings["BaseUri"].ToString(),
                ListingsBaseUri = ConfigurationManager.AppSettings["ListingsBaseUri"].ToString()
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
                Priority = 1,
                Rating = "HR",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(3, 5),
                CreditScores = new List<string> { "640-659", "680-699" }
            };
        }

        private ListingFilter Get36M_04Filter()
        {
            return new ListingFilter
            {
                Priority = 2,
                Rating = "A",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(9, 11),
                CreditScores = new List<string> { "640-659", "660-679", "720-739", "740-759", "760-779", "780-799", "800-819", "820-850" }
            };
        }
    }
}
