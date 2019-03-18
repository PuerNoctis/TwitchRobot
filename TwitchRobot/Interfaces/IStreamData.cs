using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchRobot.Interfaces
{
    public interface IStreamData
    {
        string ID { get; set; }
        string UserID { get; set; }
        string UserName { get; set; }
        string GameID { get; set; }
        IList<string> CommunityIDs { get; }
        string Type { get; set; }
        string Title { get; set; }
        int ViewerCount { get; set; }
        DateTimeOffset StartedAt { get; set; }
        string Language { get; set; }
        string ThumbnailURL { get; set; }
        IList<string> TagIDs { get; }
    }
}
