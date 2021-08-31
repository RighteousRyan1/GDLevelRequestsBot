using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GDRequestsBot.Utilities
{
    public static class U_Discord
    {
        public static Color GetHighestRoleColor(this SocketCommandContext context, IGuildUser user)
        {
            int highest_permission = 0;
            ulong highest_role = 0;
            foreach (var roleId in user.RoleIds)
            {
                var role = context.Guild.GetRole(roleId);
                if (role.Position > highest_permission)
                {
                    if (!role.IsEveryone && role.Color.ToString() != "#000000")
                    {
                        highest_permission = role.Position;
                        highest_role = role.Id;
                    }
                }
            }
            return highest_role != 0 ? context.Guild.GetRole(highest_role).Color : Color.DarkerGrey;
        }
        public static bool HasRole(this IGuildUser user, ulong roleId)
        {
            foreach (var role in user.RoleIds)
            {
                if (role == roleId)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasRole(this SocketUser user, ulong roleId)
        {
            var sUser = user as IGuildUser;
            foreach (var role in sUser.RoleIds)
            {
                if (role == roleId)
                {
                    return true;
                }
            }
            return false;
        }

        public static Color RandomColor()
        {
            byte r1 = (byte)new Random().Next(0, 256);
            byte r3 = (byte)new Random().Next(0, 256);
            byte r2 = (byte)new Random().Next(0, 256);
            return new(r1, r2, r3);
        }
    }
}