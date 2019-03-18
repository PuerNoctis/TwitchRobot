using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TwitchRobot.Interfaces;

namespace TwitchRobot
{
    public static class MainClass
    {
        /// <summary>
        /// The Client ID which has been created on the Twitch Developer web site.
        /// </summary>
        private const string TWITCH_CLIENT_ID = "wj2dbjug7ch6tcrs195qx7ehafc9y6";

        /// <summary>
        /// The type-tag within a stream's metadata to indicate a live stream. I wanted
        /// to use enums, but I couldn't find a complete list of types in Twitch's docs.
        /// </summary>
        private const string TWITCH_TYPE_LIVE = "live";

        /// <summary>
        /// The default console foreground color used for coloring error messages.
        /// </summary>
        private static ConsoleColor DEFAULT_CONSOLE_FG;

        /// <summary>
        /// Excecutes any command as a so-called 'shell-command'. Basically enables
        /// us to open up Twitch in the browser.
        /// </summary>
        /// <param name="cmd">The command to execute in shell.</param>
        private static void ShellExecute(string cmd)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                }
            }.Start();
        }

        /// <summary>
        /// Shortcut for opening Twitch with a shell command.
        /// </summary>
        /// <param name="user">The user to open the Twitch stream for.</param>
        private static void OpenStream(IUserData user)
        {
            ShellExecute($"https://www.twitch.tv/{user.Login}");
        }

        /// <summary>
        /// Simple welcome banner.
        /// </summary>
        private static void PrintBanner()
        {
            var v = Assembly.GetEntryAssembly().GetName().Version;

            Console.WriteLine($"===========================================================");
            Console.WriteLine($" TWITCH VIEWER ROBOT (v{v.Major}.{v.Minor}.{v.Build}.{v.Revision})");
            Console.WriteLine($"===========================================================");
        }

        /// <summary>
        /// Prints the usage for the program, duh.
        /// </summary>
        private static void PrintUsage()
        {
            var asmName = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Name;
            Console.WriteLine($"Usage: dotnet {asmName} <interval> <user> [user...]");
        }

        /// <summary>
        /// Prints a colored error message.
        /// </summary>
        /// <param name="message">The message to print.</param>
        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = DEFAULT_CONSOLE_FG;
        }

        /// <summary>
        /// Prints a colored error message as well as the usage.
        /// </summary>
        /// <param name="message"></param>
        private static void PrintErrorAndUsage(string message)
        {
            PrintError(message);
            PrintUsage();
            Console.WriteLine();
        }

        /// <summary>
        /// Main-Entry-Point
        /// </summary>
        /// <param name="args">The arguments passed by command-line.</param>
        /// <returns>Non-Zero in case of error.</returns>
        public static int Main(string[] args)
        {
            // First, let's save the current foreground color of the console
            // and print the welcoming banner.
            DEFAULT_CONSOLE_FG = Console.ForegroundColor;
            PrintBanner();

            // Check if we have at least two argumens (interval and one user)
            if (args.Length < 2)
            {
                PrintErrorAndUsage("You need to specify an interval and at least one user.");
                return 1;
            }

            // Try to parse the interval to a valid integer number.
            if (!int.TryParse(args[0], out int interval))
            {
                PrintErrorAndUsage("First argument has to be a valid integer.");
                return 2;
            }

            Console.WriteLine($"Using interval: {interval} seconds");

            // The user LUT (Look-Up-Table) is used to store the currently observed
            // users and track their last checked stream data.
            var userLut = new Dictionary<IUserData, IStreamData>();

            // Retrieve the user information from Twitch. If there is no user data
            // returned, we print and error and abort.
            using (var twitch = new Twitch.Twitch(TWITCH_CLIENT_ID))
            {
                for (int i = 1; i < args.Length; i++)
                {
                    // Get and check the user data. If there is none, the user most
                    // likely does not exist.
                    var userData = twitch.GetUserData(args[i]);
                    if(userData == null)
                    {
                        PrintError($"The user '{args[i]}' could not be found on Twitch");
                        return 3;
                    }

                    // Get the initial stream data for that user and all information into
                    // the LUT.
                    var streamData = twitch.GetStreamData(userData);
                    userLut.Add(userData, streamData);

                    Console.WriteLine($"Added user: {args[i]}");
                }
            }

            // Check if any streams are already live when we start the application.
            foreach (var u in userLut)
            {
                var user = u.Key;
                var streamData = u.Value;

                // Do we have initial stream data?
                if(streamData != null)
                {
                    // If so, has it a "live" state?
                    if(streamData.Type == TWITCH_TYPE_LIVE)
                    {
                        // Yes? Well open it already!
                        Console.WriteLine($"User '{u.Key.Login}' is already live! Opening stream...");
                        OpenStream(u.Key);
                    }
                }
            }

            using (var twitch = new Twitch.Twitch(TWITCH_CLIENT_ID))
            {
                // Main application loop. We never leave this loop unless the user
                // closes the window or presses CTRL+C (or SIGINT in general). This
                // goes on forever from here on.
                while(true)
                {
                    Console.WriteLine("Checking streams...");

                    // Check each user.
                    for(int i = 0; i < userLut.Count; i++)
                    {
                        var user = userLut.ElementAt(i).Key;
                        var oldStreamData = userLut.ElementAt(i).Value;
                        var newStreamData = twitch.GetStreamData(user);

                        // Is there new stream data available? If not, we don't
                        // care, the user is most likely offline.
                        if(newStreamData != null)
                        {
                            // Is the stream live? And if so, was the previous stream data
                            // empty? If so, the user switched from "offline" to "live". If
                            // we wouldn't check the current state with the previous one, we
                            // might open live streams again and again every interval.
                            if((newStreamData.Type == TWITCH_TYPE_LIVE) && (oldStreamData == null))
                            {
                                Console.WriteLine($"User '{user.Login}' has gone live! Opening stream...");
                                OpenStream(user);
                            }
                        }

                        // Update the stream data in the LUT.
                        userLut[user] = newStreamData;
                    }

                    // Realizes the interval. It has to be noted though, that an interval of 3 seconds
                    // does not really mean "streams are checked every 3 seconds". The above code itself
                    // also runs for a number of seconds itself. So this is a very lazy interval implementation.
                    // Otherwise we would have to ensure that intervals don't overlap and yuck and bleh.
                    Thread.Sleep(interval * 1000);
                }
            };
        }
    }
}
