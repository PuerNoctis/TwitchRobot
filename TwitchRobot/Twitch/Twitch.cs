using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchRobot.Interfaces;

namespace TwitchRobot.Twitch
{
    /// <summary>
    /// Serves as our facade against the Twitch API for easier use.
    /// </summary>
    public sealed class Twitch : ITwitch, IDisposable
    {
        /// <summary>
        /// The GET-URL to retrieve the user data from.
        /// </summary>
        private const string TWITCH_GET_USER_URL = "https://api.twitch.tv/helix/users?login={0}";

        /// <summary>
        /// The GET-URL to retrieve the stream data from.
        /// </summary>
        private const string TWITCH_GET_STREAM_URL = "https://api.twitch.tv/helix/streams?user_id={0}";

        /// <summary>
        /// Our HTTP client to send web requests around.
        /// </summary>
        private readonly HttpClient _http;

        /// <summary>
        /// The Client-ID with which we authenticate agains the Twitch API. This can be
        /// created via the Twitch Developer web site and serves as some sort of "login"
        /// for the Twitch API.
        /// </summary>
        public string ClientID { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clientID">The Client ID to use to authenticate with Twitch API.</param>
        public Twitch(string clientID)
        {
            if(string.IsNullOrWhiteSpace(clientID)) { throw new ArgumentNullException(nameof(clientID)); }
            ClientID = clientID;

            // Creates the HTTP client and adds the Client-ID to the header of the
            // web requests. Twitch API requires a valid Client-ID, otherwise all
            // requests will be rejected.
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Add("Client-ID", clientID);
        }

        /// <summary>
        /// Retrieves the data for a user synchronously.
        /// </summary>
        /// <param name="loginName">The name of the user to get the data for.</param>
        /// <returns>The user data, or null if no user with that name has been found.</returns>
        public IUserData GetUserData(string loginName)
        {
            return GetUserDataAsync(loginName).Result;
        }

        /// <summary>
        /// Retrieves the data for a user asynchronously. 
        /// </summary>
        /// <param name="loginName">The name of the user to get the data for.</param>
        /// <returns>The user data, or null if no user with that name has been found.</returns>
        public async Task<IUserData> GetUserDataAsync(string loginName)
        {
            // Construct or final URL to send the web request to.
            var url = string.Format(TWITCH_GET_USER_URL, loginName);

            // Send the little slugger and wait for a response from Twitch.
            var resp = _http.GetAsync(url).Result;

            // Read all the content from the response as a string. All data retrieved
            // from Twitch is in Json string format. Afterwards, deserialize it into
            // a .NET object for better usability.
            var content = await resp.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject(content) as JObject;
            var data = obj["data"].First;

            return data?.ToObject<UserData>();
        }

        /// <summary>
        /// Retrieves the stream data for a user synchronously. 
        /// </summary>
        /// <param name="user">The user to retrieve the stream data for.</param>
        /// <returns>The stream data, or null if the stream is offline or otherwise unavailable.</returns>
        public IStreamData GetStreamData(IUserData user)
        {
            return GetStreamDataAsync(user).Result;
        }

        /// <summary>
        /// Retrieves the stream data for a user asynchronously. 
        /// </summary>
        /// <param name="user">The user to retrieve the stream data for.</param>
        /// <returns>The stream data, or null if the stream is offline or otherwise unavailable.</returns>
        public async Task<IStreamData> GetStreamDataAsync(IUserData user)
        {
            // Construct our final URL to send the request to.
            var url = string.Format(TWITCH_GET_STREAM_URL, user.ID);

            // Send it and wait for Twitch to respond.
            var resp = _http.GetAsync(url).Result;

            // Read all the content from the response as a string. All data retrieved
            // from Twitch is in Json string format. Afterwards, deserialize it into
            // a .NET object for better usability.
            var content = await resp.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject(content) as JObject;
            var data = obj["data"].First;

            // We ignore pagination for now. It doesn't seem that we need the cursor
            // for our use-cases. Hopefully it stays that way...
            
            return data?.ToObject<StreamData>();
        }

        /// <summary>
        /// Cleanup. Not really necessary in this application; just thrown
        /// in here for good measure.
        /// </summary>
        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
