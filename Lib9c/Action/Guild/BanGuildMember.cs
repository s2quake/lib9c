using System;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Nekoyume.Extensions;
using Nekoyume.Module.Guild;

namespace Nekoyume.Action.Guild
{
    [ActionType(TypeIdentifier)]
    public class BanGuildMember : ActionBase
    {
        public const string TypeIdentifier = "ban_guild_member";

        public Address Target { get; private set; }

        public override IValue PlainValue => Dictionary.Empty
            .Add("type_id", TypeIdentifier)
            .Add("values", Dictionary.Empty
                .Add("target", Target.Bencoded));

        public BanGuildMember() {}

        public BanGuildMember(Address target)
        {
            Target = target;
        }

        public override void LoadPlainValue(IValue plainValue)
        {
            if (plainValue is not Dictionary root ||
                !root.TryGetValue((Text)"values", out var rawValues) ||
                rawValues is not Dictionary values ||
                !values.TryGetValue((Text)"target", out var rawTarget))
            {
                throw new InvalidCastException();
            }

            Target = new Address(rawTarget);
        }

        public override IWorld Execute(IActionContext context)
        {
            var world = context.PreviousState;

            // NOTE: GuildMaster address and GuildAddress are the same with signer address.
            var guildAddress = context.GetGuildAddress();

            if (!world.TryGetGuild(guildAddress, out var guild))
            {
                throw new InvalidOperationException("The signer does not have a guild.");
            }

            if (guild.GuildMaster != guildAddress)
            {
                throw new InvalidOperationException("The signer is not a guild master.");
            }

            if (guild.GuildMaster == Target)
            {
                throw new InvalidOperationException("The guild master cannot be banned.");
            }

            world = world.Ban(guildAddress, Target);

            if (world.TryGetGuildParticipant(Target, out var guildParticipant) && guildParticipant.GuildAddress == guildAddress)
            {
                world = world.RemoveGuildParticipant(Target);
            }

            return world;
        }
    }
}
