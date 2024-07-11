using System;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Nekoyume.Model.Guild;
using Nekoyume.Module.Guild;
using Nekoyume.TypedAddress;

namespace Nekoyume.Action.Guild
{
    [ActionType(TypeIdentifier)]
    public class JoinGuild : ActionBase
    {
        public const string TypeIdentifier = "join_guild";

        public JoinGuild() {}

        public JoinGuild(GuildAddress guildAddress)
        {
            GuildAddress = guildAddress;
        }

        public GuildAddress GuildAddress { get; private set; }

        public override IValue PlainValue => Dictionary.Empty
            .Add("type_id", TypeIdentifier)
            .Add("values", Dictionary.Empty
                .Add("guild_address", GuildAddress.Bencoded));

        public override void LoadPlainValue(IValue plainValue)
        {
            var root = (Dictionary)plainValue;
            if (plainValue is not Dictionary ||
                !root.TryGetValue((Text)"values", out var rawValues) ||
                rawValues is not Dictionary values ||
                !values.TryGetValue((Text)"guild_address", out var rawGuildAddress))
            {
                throw new InvalidCastException();
            }

            GuildAddress = new GuildAddress(rawGuildAddress);
        }

        public override IWorld Execute(IActionContext context)
        {
            var world = context.PreviousState;
            var guildParticipantAccount = world.GetAccount(Addresses.GuildParticipant);
            var signer = context.Signer;

            if (guildParticipantAccount.GetState(signer) is not null)
            {
                throw new InvalidOperationException("The signer is already joined in a guild.");
            }

            // NOTE: Check there is such guild.
            _ = world.GetGuild(GuildAddress);

            if (world.IsBanned(GuildAddress, signer))
            {
                throw new InvalidOperationException("The signer is banned from the guild.");
            }

            // TODO: Do something related with ConsensusPower delegation.

            guildParticipantAccount = guildParticipantAccount.SetState(
                signer,
                new GuildParticipant(GuildAddress).Bencoded);

            return world.SetAccount(Addresses.GuildParticipant, guildParticipantAccount);
        }
    }
}
