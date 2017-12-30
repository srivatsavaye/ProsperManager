using Newtonsoft.Json;
using Prosper.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProsperLibrary
{
    public class ProsperManager
    {
        public ProsperManager(JsonSerializerSettings jsonSerializerSettings)
        {

        }

        public AuthenticationToken authenticationToken { get; set; }
    }
}
