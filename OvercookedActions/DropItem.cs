using System.Collections.Generic;
using Newtonsoft.Json;
using StreamIntegrationApp.API;

namespace OvercookedActions
{
    public class DropItem: IntegrationAction
    {
        
        public override string Execute(string username, string @from, Dictionary<string, object> parameters)
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}