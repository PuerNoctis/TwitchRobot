using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchRobot.Interfaces;

namespace TwitchRobot.Twitch
{
    public class StreamData : IStreamData
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("user_id")]
        public string UserID { get; set; }
        [JsonProperty("user_name")]
        public string UserName { get; set; }
        [JsonProperty("game_id")]
        public string GameID { get; set; }
        [JsonProperty("community_ids")]
        public IList<string> CommunityIDs { get; } = new List<string>();
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("viewer_count")]
        public int ViewerCount { get; set; }
        [JsonProperty("started_at")]
        public DateTimeOffset StartedAt { get; set; }
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("thumbnail_url")]
        public string ThumbnailURL { get; set; }
        [JsonProperty("tag_ids")]
        public IList<string> TagIDs { get; } = new List<string>();
    }
}
