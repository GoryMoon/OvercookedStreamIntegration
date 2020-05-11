using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using StreamIntegrationApp.API;

namespace OvercookedActions
{
    public class Move: IntegrationAction
    {
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        private readonly float playerMin;

        [DefaultValue(1.5)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        private readonly float playerMax;
        
        [DefaultValue(1.5)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        private readonly float vanMin;
        
        [DefaultValue(3)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        private readonly float vanMax;
        
        public override string Execute(string username, string from, Dictionary<string, object> parameters)
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}