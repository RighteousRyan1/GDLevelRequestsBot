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

namespace GDRequestsBot
{
    class GDBot
    {
        public static string prefix = "-";
        public static GDBot Instance { get; private set; }

        internal static string ManagementPath => Directory.GetCurrentDirectory().Remove(Directory.GetCurrentDirectory().Length - @"\bin\Debug\net5.0".Length);

        private GDBot()
        {
            Instance = this;
        }

        static void Main()
        {
            #region Setup
            if (File.ReadAllLines(ManagementPath + "/storedchannel.txt").Length > 0)
            {
                if (ulong.TryParse(File.ReadAllLines(ManagementPath + "/storedchannel.txt")[0], out var id))
                {
                    CommandRequests.reqChannelId = id;
                }
            }
            if (File.ReadAllLines(ManagementPath + "/acceptchannel.txt").Length > 0)
            {
                if (ulong.TryParse(File.ReadAllLines(ManagementPath + "/acceptchannel.txt")[0], out var id))
                {
                    CommandRequests.channelToAcceptRequestsFrom = id;
                }
            }
            new GDBot().RunAsync().GetAwaiter().GetResult();
            #endregion
        }

        internal DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _serviceProvider;

        internal SocketUser lastTrackedUser;

        public async Task RunAsync()
        {
            _client = new();
            _commands = new();

            await _client.SetActivityAsync(new Game("Geometry Dash levels", ActivityType.Playing, ActivityProperties.None));
            await _client.SetStatusAsync(UserStatus.Online);

            _serviceProvider = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();

            #region Token
            string tkn = File.ReadAllText(ManagementPath + "/token.txt");
            #endregion

            _client.Log += Logging;

            await RegisterCommands();

            await _client.LoginAsync(TokenType.Bot, tkn);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Logging(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommands()
        {
            _client.MessageReceived += CommandHandle;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task CommandHandle(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
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
