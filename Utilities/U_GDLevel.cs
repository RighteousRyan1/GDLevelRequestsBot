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
using Geometric;
using Geometric.Data;
using Geometric.Data.Parsed;
using Geometric.Data.Raw;
using Geometric.Data.Parsed.Levels;
using Geometric.Web;

namespace GDRequestsBot.Utilities
{
    public static class U_GDLevel
    {
        public static List<string> GetAllLevelData(this ParsedLevel level)
        {
            object[] datas =
            {
                level.AuthorData.AccountId, level.AuthorData.PlayerId, level.SongLevelData.SongAuthor,
                    level.SongLevelData.GetSongId(), level.SongLevelData.SongName, level.SongLevelData.SongLink,
                    level.StandardLevelData.Name, level.StandardLevelData.Description
            };
            var list = new List<string>();

            foreach (var data in datas) list.Add(data.ToString());

            return list;
        }
        public static string GetBasicStats(this ParsedLevel level)
        {
            string showCoins = "";
            for (int i = 0; i < level.StandardLevelData.CoinsPlaced; i++)
                showCoins += GDBot.emotes[8].ToString() + " ";
            return
                $"__**Level Name**__\n{level.StandardLevelData.Identifier} | {level.GetName()} {(level.StatisticLevelData.CopiedId > 0 ? GDBot.emotes[5].ToString() : "")} {(level.StatisticLevelData.Objects > 40000 ? GDBot.emotes[6].ToString() : "")}\n" +
                    $"\n__**Description**__\n {level.GetDescription()}\n" +
                    $"\n__**General Stats**__" +
                    $"\n {GDBot.emotes[0]} Downloads: {level.StatisticLevelData.Downloads}" +
                    $"\n {(level.StatisticLevelData.Disliked ? GDBot.emotes[2].ToString() : GDBot.emotes[1].ToString())} Liking: {level.StatisticLevelData.Likes}" +
                    $"\n {GDBot.emotes[7]} Amount Coins: {(level.StandardLevelData.CoinsPlaced > 0 ? showCoins : "None")}\n" +
                    $"\n:notes: __**Song**__\n {level.GetSongName()} by {level.GetSongAuthor()}\n" +
                    $"\n__**Rating**__\n {level.StandardLevelData.Difficulty}, {level.StatisticLevelData.Stars} {GDBot.emotes[3]}\n" +
                    $"\n:pencil2: __**Author**__\n {(level.GetAccountId() > 0 ? "https://gdbrowser.com/u/" + level.GetAccountId().ToString() : "Account not valid!")}";
        }
        public static string GetBasicAuthorStats(this ParsedLevel level)
        {
            var author = level.AuthorData;
            return $"__**User Stats**__" +
                $"{GDBot.emotes[3]} star" +
                $"{GDBot.emotes[9]} diamond" +
                $"{GDBot.emotes[7]} coin" +
                $"{GDBot.emotes[8]} uCoin" +
                $"{GDBot.emotes[10]} demon" +
                $"" +
                $"";
        }
        public static int GetAccountId(this ParsedLevel level)
            => level.AuthorData.AccountId;
        public static int GetPlayerId(this ParsedLevel level)
            => level.AuthorData.PlayerId;
        public static string GetSongAuthor(this ParsedLevel level)
            => level.SongLevelData.SongAuthor;
        public static int GetSongId(this ParsedLevel level)
            => level.SongLevelData.GetSongId();
        public static string GetSongName(this ParsedLevel level)
            => level.SongLevelData.SongName;
        public static string GetSongLink(this ParsedLevel level)
            => level.SongLevelData.SongLink;
        public static string GetName(this ParsedLevel level)
            => level.StandardLevelData.Name;
        public static string GetDescription(this ParsedLevel level)
            => level.StandardLevelData.Description;
    }
}