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

namespace GDRequestsBot.Modules
{
    public class CommandRequests : ModuleBase<SocketCommandContext>
    {
        public static ulong reqChannelId;
        public static ulong channelToAcceptRequestsFrom;
        public Color GetHighestRoleColor(IGuildUser user)
        {
            int highest_permission = 0;
            ulong highest_role = 0;
            foreach (var roleId in user.RoleIds)
            {
                var role = Context.Guild.GetRole(roleId);
                if (role.Position > highest_permission)
                {
                    if (!role.IsEveryone && role.Color.ToString() != "#000000")
                    {
                        highest_permission = role.Position;
                        highest_role = role.Id;
                    }
                }
            }
            return highest_role != 0 ? Context.Guild.GetRole(highest_role).Color : Color.DarkerGrey;
        }
        [Command("request")]
        public async Task Request([Remainder] string id)
        {
            if (Context.Channel.Id != GDBot.Instance._client.GetChannel(channelToAcceptRequestsFrom).Id)
            {
                await ReplyAsync($"{Context.User.Mention}, this command can only be used in <#{channelToAcceptRequestsFrom}>!");
                await Context.Message.AddReactionAsync(new Emoji("❌"));
                return;
            }
            if (ulong.TryParse(id, out var actualId))
            {
                if (actualId > 100000000 - 1)
                {
                    await ReplyAsync("Invalid ID!");
                    return;
                }

                var requestEmbed = new EmbedBuilder()
                    .WithDescription($":checkered_flag: Your level has been submitted!")
                    .WithColor(Color.Magenta)
                    .WithTitle("Level Submitted!")
                    .WithUrl("https://gdbrowser.com/" + id)
                    .WithFooter(footer =>
                    {
                        footer.WithText("I will DM you with suggestions and details if I get to your level!");
                        // footer.WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/7/71/Arbcom_ru_ready.svg/250px-Arbcom_ru_ready.svg.png");
                    });
                await Context.Message.AddReactionAsync(new Emoji("✅"));

                var user = Context.User;
                var socketUser = user as IGuildUser;

                var embed = requestEmbed.Build();
                await ReplyAsync(embed: embed);

                var channel = GDBot.Instance._client.GetChannel(reqChannelId) as ISocketMessageChannel;
                var reqEmbed2 = new EmbedBuilder()
                    .WithDescription($":checkered_flag: A Level has been submitted by <@{user.Id}>!\n The ID is `{id}`.")
                    .WithColor(GetHighestRoleColor(socketUser))
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
    }
}
