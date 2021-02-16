using Discord;
using Discord.Commands;
using Markov;
using OpenNLP.Tools.SentenceDetect;
using OpenNLP.Tools.Tokenize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FredBot.Modules
{
    public class MarkovModule : ModuleBase<SocketCommandContext>
    {
        public ulong FredID = 86859572790181888;

        [Command("markov")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        public async Task Markov(IGuildUser user, int count = 200, string language = "eng")
        {
            var messages = await Context.Channel.GetMessagesAsync(count, CacheMode.AllowDownload, RequestOptions.Default).ToListAsync();
            var filtered = messages.SelectMany(msgs => msgs.Where(msg => msg.Author == user && !msg.MentionedUserIds.Contains(Context.Client.CurrentUser.Id)).Select(msg => msg.Content)).Where(msg => !string.IsNullOrWhiteSpace(msg));
            var users = Context.Guild.Users;
            var usersCleaned = filtered.Select(msg =>
            {
                var cleanedString = msg;
                foreach (var user in users)
                {
                    cleanedString = cleanedString.Replace($"{user.Id}", user.Username);
                }

                return cleanedString;
            });
            File.AppendAllLines($"{user.Id}_{language}.txt", usersCleaned);
            await Context.Channel.SendMessageAsync($"Done! (Found {usersCleaned.Count()} messages from the specified user)");
        }

        [Command("generate")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        public async Task Generate(IGuildUser user, int count = 1, string language = "eng", int depth = 2)
        {
            if (!File.Exists($"{user.Id}_{language}.txt"))
            {
                return;
            }

            var messages = File.ReadAllLines($"{user.Id}_{language}.txt").ToList();

            var chain = new MarkovChain<string>(depth);
            var tokenizer = new EnglishRuleBasedTokenizer(false);
            var sentenceDetector = new EnglishMaximumEntropySentenceDetector("EnglishSD.nbin");

            messages.ForEach(msg =>
            {
                var sentences = sentenceDetector.SentenceDetect(msg);
                foreach(var sentence in sentences)
                {
                    var tokens = tokenizer.Tokenize(sentence);
                    chain.Add(tokens, tokens.Length);
                }
                
            });
            var rand = new Random();
            var messageString = "";
            for (int i = 0; i < count; i++)
            {
                var c = chain.Chain(rand);
                messageString += ">> " + string.Join(" ", c);
                if (count > 1 && i < count - 1) messageString += "\n";
            }
            await Context.Channel.SendMessageAsync(messageString);
        }

        [Command("hey")]
        [RequireContext(ContextType.Guild)]
        public async Task Hey([Remainder] string message = "hey")
        {
            var messages = new List<string>();

            // If sentences for fred do not exist for the server, generate them
            if (!File.Exists($@"{Context.Guild.Id}\{FredID}.txt"))
            {
                messages = await GenerateSentenceFile(FredID);
            }
            else
            {
                messages = File.ReadAllLines($@"{Context.Guild.Id}\{FredID}.txt").ToList();
            }

            var chain = new MarkovChain<string>(3);
            var tokenizer = new EnglishRuleBasedTokenizer(false);
            var sentenceDetector = new EnglishMaximumEntropySentenceDetector("EnglishSD.nbin");

            messages.ForEach(msg =>
            {
                var sentences = sentenceDetector.SentenceDetect(msg);
                foreach (var sentence in sentences)
                {
                    var tokens = tokenizer.Tokenize(sentence);
                    chain.Add(tokens, tokens.Length);
                }

            });

            var rand = new Random();

            var reply = string.Join(" ", chain.Chain(rand));

            if (string.IsNullOrWhiteSpace(reply))
            {
                return;
            }

            await Context.Channel.SendMessageAsync(reply);
        }

        public async Task<List<string>> GenerateSentenceFile(ulong userID, int count = 30000)
        {
            var messages = await Context.Channel.GetMessagesAsync(count, CacheMode.AllowDownload, RequestOptions.Default).ToListAsync();
            var filtered = messages.SelectMany(msgs => msgs.Where(msg => msg.Author.Id == userID && !msg.MentionedUserIds.Contains(Context.Client.CurrentUser.Id)).Select(msg => msg.Content)).Where(msg => !string.IsNullOrWhiteSpace(msg));
            var users = Context.Guild.Users;
            var usersCleaned = filtered.Select(msg =>
            {
                var cleanedString = msg;
                foreach (var user in users)
                {
                    cleanedString = cleanedString.Replace($"{user.Id}", user.Username);
                }

                return cleanedString;
            });

            if (!Directory.Exists($"{Context.Guild.Id}"))
            {
                Directory.CreateDirectory($"{Context.Guild.Id}");
            }

            File.AppendAllLines($@"{Context.Guild.Id}\{userID}.txt", usersCleaned);
            await Context.Channel.SendMessageAsync($"Done! (Found {usersCleaned.Count()} messages from the specified user)");
            return usersCleaned.ToList();
        }
    }
}
