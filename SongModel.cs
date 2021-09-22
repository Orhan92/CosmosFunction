using System;
using Newtonsoft.Json;

namespace CosmosFunction
{
    public class SongModel 
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Artist { get;set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
    }
}
