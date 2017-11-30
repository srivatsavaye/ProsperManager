using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prosper.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProsperLibrary
{
    public class ProsperClient
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly AccountSettings _accountSettings;
        private readonly IClient _client;

        public ProsperClient(IClient client, AccountSettings accountSettings, JsonSerializerSettings jsonSerializerSettings)
        {
            _client = client;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _jsonSerializerSettings = jsonSerializerSettings;
            _accountSettings = accountSettings;
        }

        public async Task<AuthenticationToken> AuthenticateAsync()
        {
            var uri = $"{_accountSettings.BaseUri}/security/oauth/token?grant_type=password&client_id={_accountSettings.ClientId}&client_secret={_accountSettings.ClientSecret}&username={_accountSettings.Username}&password={_accountSettings.Password}";
            var authenticationToken = new AuthenticationToken();
            try
            {
                authenticationToken = await _client.Post<AuthenticationToken>(uri,new Dictionary<string, string>(), string.Empty, _jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw;
            }

            return authenticationToken;
        }


        public async Task<Account> GetAccountAsync(string accessToken)
        {
            var uri = $"{ _accountSettings.BaseUri}/accounts/prosper";
            var account = new Account();
            try
            {
                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {accessToken}"
                };

                account = await _client.Get<Account>(uri, headers, _jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw;
            }

            return account;
        }


        public async Task<ListingResult> GetListingsAsync(string accessToken, int limit)
        {
            var uri = $"{ _accountSettings.ListingsBaseUri}/listingsvc/v2/listings?limit={limit}";
            var listings = new ListingResult();
            try
            {
                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {accessToken}"
                };

                listings = await _client.Get<ListingResult>(uri, headers, _jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw;
            }

            return listings;
        }

    }
}
