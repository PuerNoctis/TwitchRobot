using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchRobot.Interfaces
{
    public interface IUserData
    {
        string ID { get; set; }
        string Login { get; set; }
        string DisplayName { get; set; }
        string Type { get; set; }
        string BroadcasterType { get; set; }
        string Description { get; set; }
        string ProfileImageURL { get; set; }
        string OfflineImageURL { get; set; }
        int ViewCount { get; set; }
    }
}
