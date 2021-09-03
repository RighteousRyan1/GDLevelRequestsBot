using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Geometric.Data.Parsed.Levels;
using Geometric.Web;
using GDRequestsBot.Utilities;
using System.Diagnostics;

namespace GDRequestsBot.Modules
{
    public class CommandHandler : ModuleBase<SocketCommandContext>
    {
        public void GetResources()
        {
            GDBot.PharaohsDen = GDBot.Instance.client.Guilds.First(x => x.Id == 877706747429679184);
            GDBot.emotes = new Emote[]
            {
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516050968100926), // downloads 0
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516088041537627), // like 1
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516051127476284), // dislike 2
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516100934844447), // stars 3
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516096740552714), // orbs 4
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881515895724331039), // copied 5
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516084900003860), // large 6
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881515893358739466), // gd coin 7
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516098380521484), // silver coin 8
                /*GDBot.PharaohsDen.Emotes.First(x => x.Id == 881516047033827408), // diamond 9
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 881515910307938354), // demon 10
                GDBot.PharaohsDen.Emotes.First(x => x.Id == 870788766376296548)*/ // cp 11
            };
        }

        public static ulong reqChannelId;
        public static ulong channelToAcceptRequestsFrom;

        [Command("request")]
        public async Task Request([Remainder] string id)
        {
            GetResources();
            if (Context.Channel.Id != GDBot.Instance.client.GetChannel(channelToAcceptRequestsFrom).Id)
            {
                await ReplyAsync($"{Context.User.Mention}, this command can only be used in <#{channelToAcceptRequestsFrom}>!");
                await Context.Message.AddReactionAsync(new Emoji("❌"));
                return;
            }
            if (int.TryParse(id, out var actualId))
            {
                GDClient client = new();
                ParsedLevel level;

                try
                {
                    level = await client?.GetLevelAsync(actualId);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("❌"));
                    await ReplyAsync($"{actualId} is not an existent ID in the GD database!");
                    return;
                }
                var requestEmbed = new EmbedBuilder()
                    .WithDescription($":checkered_flag: Your level has been submitted!\n\n{level.GetBasicStats()}")
                    .WithColor(U_Discord.RandomColor())
                    .WithTitle("Level Submitted!")
                    .WithUrl("https://gdbrowser.com/" + id)
                    .WithFooter(footer =>
                    {
                        footer.WithIconUrl("https://gdbrowser.com/assets/difficulties/" + level.StandardLevelData.DifficultyFace + ".png");
                        footer.WithText("I will DM you with suggestions and details if I get to your level!");
                    });
                await Context.Message.AddReactionAsync(new Emoji("✅"));

                var user = Context.User;
                var socketUser = user as IGuildUser;

                var embed = requestEmbed.Build();
                await ReplyAsync(embed: embed);

                var channel = GDBot.Instance.client.GetChannel(reqChannelId) as ISocketMessageChannel;
                var reqEmbed2 = new EmbedBuilder()
                    .WithDescription($":checkered_flag: A Level has been submitted by <@{user.Id}>!" +
                    $"\n\n{level.GetBasicStats()}")
                    .WithColor(Context.GetHighestRoleColor(socketUser))
                    .WithTitle("New Level Request!")
                    .WithCurrentTimestamp()
                    .WithUrl("https://gdbrowser.com/" + id)
                    .WithThumbnailUrl($"{user.GetAvatarUrl()}");
                var embed2 = reqEmbed2.Build();
                await channel.SendMessageAsync(embed: embed2);
                return;
            }
            await ReplyAsync("Invalid ID or argument.");
        }
        [Command("recievechannel")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You cannot use this command. You must have `administrator` permissions.")]
        public async Task SetRequestsRecieveChannel([Remainder] ISocketMessageChannel channel)
        {
            if (channel != null)
            {
                File.Delete(GDBot.ManagementPath + "/storedchannel.txt");

                await File.WriteAllTextAsync(GDBot.ManagementPath + "/storedchannel.txt", channel.Id.ToString());
                var requestEmbed = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($":white_check_mark: Set recieving channel from <#{reqChannelId}> to <#{channel.Id}>!");
                var embed = requestEmbed.Build();
                reqChannelId = channel.Id;
                await ReplyAsync(embed: embed);
            }
        }
        [Command("recievechannel")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You cannot use this command. You must have `administrator` permissions.")]
        public async Task SetRequestsRecieveChannel()
        {
            await ReplyAsync($"The current channel that requests are sent to is <#{reqChannelId}>");
        }
        [Command("acceptchannel")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You cannot use this command. You must have `administrator` permissions.")]
        public async Task SetRequestAcceptChannel([Remainder] ISocketMessageChannel channel)
        {
            if (channel != null)
            {
                File.Delete(GDBot.ManagementPath + "/acceptchannel.txt");

                await File.WriteAllTextAsync(GDBot.ManagementPath + "/acceptchannel.txt", channel.Id.ToString());
                var requestEmbed = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($":white_check_mark: Set request accepting channel from <#{channelToAcceptRequestsFrom}> to <#{channel.Id}>!");
                var embed = requestEmbed.Build();
                channelToAcceptRequestsFrom = channel.Id;
                await ReplyAsync(embed: embed);
            }
        }
        [Command("acceptchannel")]
        public async Task SetRequestAcceptChannel()
        {
            await ReplyAsync($"The current channel for requests to be accepted in is in <#{channelToAcceptRequestsFrom}>");
        }
        [Command("launchgd")]
        public async Task LaunchGD()
        {
            // potentially un-hardcode this IF i make this a public bot.
            var user = Context.User as IGuildUser;
            if (!user.HasRole(880202169121382401)) // reviewer
            {
                await ReplyAsync($"This command is for reviewers only! Please launch directly from steam.");
                return;
            }
            await ReplyAsync($"Launching your Geometry Dash client...");
            Process.Start(new ProcessStartInfo(@"steam://rungameid/322170")
            {
                UseShellExecute = true,
            });
        }

        [Command("help")]
        public async Task HelpNormal()
        {
        }
        [Command("helpadmin")]
        public async Task HelpAdmin()
        {
        }

        [Command("reviewfinish")]
        public async Task FinishReview(IGuildUser user, string sent, [Remainder] string summary)
        {
            if (user == null)
            {
                await ReplyAsync($"Invalid user.");
                return;
            }

            var dmChannel = await user.GetOrCreateDMChannelAsync();

            var emBuilder = new EmbedBuilder()
                .WithTitle($"Your level has been reviewed by {Context.User.Username}")
                .WithDescription(
                $"**Status**: {sent}\n\n" +
                $"__**Summary**__:\n" +
                summary)
                .WithImageUrl(Context.User.GetAvatarUrl())
                .WithCurrentTimestamp();

            var defEmbed = emBuilder.Build();

            await dmChannel.SendMessageAsync(embed: defEmbed);

            await ReplyAsync($"Review successfully sent to user.");

            // make these separate steps when done
        }
    }
}
