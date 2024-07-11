#nullable enable
using System.Diagnostics.CodeAnalysis;
using Bencodex.Types;
using Libplanet.Action.State;
using Nekoyume.Action;
using Nekoyume.TypedAddress;

namespace Nekoyume.Module.Guild
{
    public static class GuildModule
    {
        public static Model.Guild.Guild GetGuild(this IWorldState worldState, GuildAddress guildAddress)
        {
            var value = worldState.GetAccountState(Addresses.Guild).GetState(guildAddress);
            if (value is List list)
            {
                return new Model.Guild.Guild(list);
            }

            throw new FailedLoadStateException("There is no such guild.");
        }

        public static bool TryGetGuild(this IWorldState worldState,
            GuildAddress guildAddress, [NotNullWhen(true)] out Model.Guild.Guild? guild)
        {
            try
            {
                guild = GetGuild(worldState, guildAddress);
                return true;
            }
            catch
            {
                guild = null;
                return false;
            }
        }

        public static IWorld SetGuild(this IWorld world, GuildAddress guildAddress, Model.Guild.Guild guild)
        {
            var account = world.GetAccount(Addresses.Guild);
            account = account.SetState(guildAddress, guild.Bencoded);
            return world.SetAccount(Addresses.Guild, account);
        }

        public static IWorld RemoveGuild(this IWorld world, GuildAddress guildAddress)
        {
            var account = world.GetAccount(Addresses.Guild);
            account = account.RemoveState(guildAddress);
            return world.SetAccount(Addresses.Guild, account);
        }
    }
}
