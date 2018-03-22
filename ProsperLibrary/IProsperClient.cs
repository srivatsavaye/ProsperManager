using System.Collections.Generic;
using System.Threading.Tasks;
using Prosper.Contracts;

namespace ProsperLibrary
{
    public interface IProsperClient
    {
        Task<AuthenticationToken> AuthenticateAsync(AccountSetting setting);
        Task<Account> GetAccountAsync(string accessToken);
        Task<ListingResult> GetListingsAsync(string accessToken, int limit);
        Task<bool> OrderAsync(string accessToken, long listingId, double amount);
    }
}