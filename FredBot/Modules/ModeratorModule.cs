using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FredBot.Modules
{
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("timeout")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        public async Task TimeoutUser(IGuildUser user, int minutes = 15, [Remainder] string reason = null)
        {
            await user.ModifyAsync((props) =>
            {
                props.Channel = null;
            });
            var timeoutRole = Context.Guild.GetRole(705377897044639744);
            await user.AddRoleAsync(timeoutRole);
            _ = await ReplyAsync($"User has been timed out for {minutes} minutes due to: {reason}");

            await Task.Delay(TimeSpan.FromMinutes(minutes));
            await user.RemoveRoleAsync(timeoutRole);

            await ReplyAsync($"Timeout has expired for user {user.Nickname} that was put in place for {minutes} minutes due to: {reason}");
        }
    }
}
