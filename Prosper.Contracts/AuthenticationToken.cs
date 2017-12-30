using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prosper.Contracts
{
    public class AuthenticationToken
    {
        private DateTime _createdDate;
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        private int _expiresIn;
        public int ExpiresIn
        {
            get { return _expiresIn; }
            set
            {
                _createdDate = DateTime.UtcNow;
                _expiresIn = value;                
            }
        }
        public bool Expired { get { return (DateTime.UtcNow - _createdDate).Seconds > ExpiresIn; } }
    }
}
