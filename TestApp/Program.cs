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
            NewMethod(GetSettings());
        }

        private static void SetJsonSerializationSettings()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            { ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() } };
        }

        private static AccountSettings GetSettings()
        {         
            var accountSettings = ReadFile(ConfigurationManager.AppSettings["AccountFileLocation"].ToString());
            accountSettings.BaseUri = ConfigurationManager.AppSettings["BaseUri"].ToString();
            return accountSettings;
        }

        private static AccountSettings ReadFile(string accountFileLocation)
        {
            if(File.Exists(accountFileLocation))
            {
                return JsonConvert.DeserializeObject<AccountSettings>(File.ReadAllText(accountFileLocation), _jsonSerializerSettings);
            }
            return null;
        }

        private static void NewMethod(AccountSettings accountSettings)
        {
            AuthenticationToken authenticationToken;
            var pClient = new ProsperClient(new Client(), accountSettings, _jsonSerializerSettings);
            var resp = pClient.AuthenticateAsync();
            resp.Wait();

            authenticationToken = resp.Result;
            var accont = pClient.GetAccountAsync(authenticationToken.AccessToken);

            accont.Wait();
        }
    }
}
