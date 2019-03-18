using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TwitchRobot.Interfaces
{
    public interface ITwitch
    {
        string ClientID { get; }

        IUserData GetUserData(string loginName);
        Task<IUserData> GetUserDataAsync(string loginName);
        IStreamData GetStreamData(IUserData user);
        Task<IStreamData> GetStreamDataAsync(IUserData user);
    }
}
