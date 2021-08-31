using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using GDRequestsBot.Modules;
using System.Net;
using System.Net.NetworkInformation;
using Geometric;
using Geometric.Data;
using Geometric.Data.Parsed;
using Geometric.Data.Raw;
using Geometric.Data.Parsed.Levels;
using Geometric.Web;
using System.Linq;

namespace GDRequestsBot
{
    public class GDBot
    {
        public static string prefix = "-";
        public static GDBot Instance { get; private set; }

        public static SocketGuild PharaohsDen;

        public static Emote[] emotes;

        internal static string ManagementPath => Directory.GetCurrentDirectory().Remove(Directory.GetCurrentDirectory().Length - @"\bin\Debug\net5.0".Length);

        static void Main()
        {
            #region Setup
            Instance = new();
            // GetResources();
            if (File.ReadAllLines(ManagementPath + "/storedchannel.txt").Length > 0)
            {
                if (ulong.TryParse(File.ReadAllLines(ManagementPath + "/storedchannel.txt")[0], out var id))
                {
                    CommandHandler.reqChannelId = id;
                }
            }
            if (File.ReadAllLines(ManagementPath + "/acceptchannel.txt").Length > 0)
            {
                if (ulong.TryParse(File.ReadAllLines(ManagementPath + "/acceptchannel.txt")[0], out var id))
                {
                    CommandHandler.channelToAcceptRequestsFrom = id;
                }
            }
            Instance.RunAsync().GetAwaiter().GetResult();
            #endregion
        }

        internal DiscordSocketClient client;
        private CommandService _commands;
        private IServiceProvider _serviceProvider;

        internal SocketUser lastTrackedUser;

        public async Task RunAsync()
        {
            client = new();
            _commands = new();

            await client.SetActivityAsync(new Game("Geometry Dash levels", ActivityType.Playing, ActivityProperties.None));
            await client.SetStatusAsync(UserStatus.Online);

            _serviceProvider = new ServiceCollection().AddSingleton(client).AddSingleton(_commands).BuildServiceProvider();

            #region Token
            string tkn = File.ReadAllText(ManagementPath + "/token.txt");
            #endregion

            client.Log += Logging;

            await RegisterCommands();

            await client.LoginAsync(TokenType.Bot, tkn);

            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Logging(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommands()
        {
            client.MessageReceived += CommandHandle;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task CommandHandle(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.IsBot)
                return;

            lastTrackedUser = message.Author;

            int pos = 0;
            if (message.HasStringPrefix(prefix, ref pos))
            {
                var result = await _commands.ExecuteAsync(context, pos, _serviceProvider);
                if (!result.IsSuccess)
                    Console.WriteLine($"{result.ErrorReason}");
                if (result.Error == CommandError.UnmetPrecondition)
                    await message.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}
