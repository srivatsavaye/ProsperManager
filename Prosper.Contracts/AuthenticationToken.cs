using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prosper.Contracts
{
    public class AuthenticationToken
    {
        private readonly DateTime _createdDate;
        public AuthenticationToken()
        {
            _createdDate = DateTime.UtcNow;
        }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public bool Expired { get { return (DateTime.UtcNow - _createdDate).Seconds > ExpiresIn; } }
    }
}
