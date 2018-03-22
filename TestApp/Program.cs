using Prosper.Contracts;
using ProsperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TestApp
{
    class Program
    {
        private static JsonSerializerSettings _jsonSerializerSettings;
        static void Main(string[] args)
        {
            SetJsonSerializationSettings();
            //NewMethod(GetAccountSettings(), GetAppSettings());
            var manager = new ProsperManager(_jsonSerializerSettings);
        }

        private static void SetJsonSerializationSettings()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            { ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() } };
        }

        private static List<AccountSetting> GetAccountSettings()
        {
            var accountSettings = ReadFile(ConfigurationManager.AppSettings["AccountFileLocation"].ToString());
            return accountSettings;
        }

        private static AppSettings GetAppSettings()
        {
           
            return new AppSettings(){BaseUri = ConfigurationManager.AppSettings["BaseUri"] , ListingsBaseUri = ConfigurationManager.AppSettings["ListingsBaseUri"] };
        }

        private static List<AccountSetting> ReadFile(string accountFileLocation)
        {
            if (File.Exists(accountFileLocation))
            {
                return JsonConvert.DeserializeObject<List<AccountSetting>>(File.ReadAllText(accountFileLocation), _jsonSerializerSettings);
            }
            return null;
        }

        private static void NewMethod(List<AccountSetting> accountSettings, AppSettings appSettings)
        {
            AuthenticationToken authenticationToken;
            var pClient = new ProsperClient(new Client(), appSettings, _jsonSerializerSettings);
            var resp = pClient.AuthenticateAsync(accountSettings[0]);
            resp.Wait();

            authenticationToken = resp.Result;

            var listings = pClient.GetListingsAsync(authenticationToken.AccessToken, 500);

            listings.Wait();

            var filter = Get36M_01Filter();
            filter = Get36M_04Filter();
            var listingsFilteredBy1 = listings.Result.Result.Where(
                l => l.ProsperRating == filter.Rating
                && l.ListingTerm == filter.Term
                && l.ProsperScore >= filter.ProsperScore.From
                && l.ProsperScore <= filter.ProsperScore.To
                && filter.CreditScores.Contains(l.CreditBureauValuesTransunionIndexed.FicoScore)
                ).ToList();

            //var accont = pClient.GetAccountAsync(authenticationToken.AccessToken);

            //accont.Wait();
        }

        private static ListingFilter Get36M_01Filter()
        {
            return new ListingFilter
            {
                Rating ="HR",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(3, 5),
                CreditScores = new List<string> { "640-659", "680-699"}
            };
        }

        private static ListingFilter Get36M_04Filter()
        {
            return new ListingFilter
            {
                Rating = "A",
                Term = 36,
                LoanCategories = null,
                ProsperScore = new Range(9, 11),
                CreditScores = new List<string> { "640-659", "660-679", "720-739", "740-759", "760-779", "780-799", "800-819", "820-850" }
            };
        }


    }
}
