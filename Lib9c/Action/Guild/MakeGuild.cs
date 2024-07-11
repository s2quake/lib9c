using System;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Nekoyume.Extensions;
using Nekoyume.Module.Guild;

namespace Nekoyume.Action.Guild
{
    [ActionType(TypeIdentifier)]
    public class MakeGuild : ActionBase
    {
        public const string TypeIdentifier = "make_guild";

        public override IValue PlainValue => Dictionary.Empty
            .Add("type_id", TypeIdentifier)
            .Add("values", Null.Value);

        public override void LoadPlainValue(IValue plainValue)
        {
            if (plainValue is not Dictionary root ||
                !root.TryGetValue((Text)"values", out var rawValues) ||
                rawValues is not Null)
            {
                throw new InvalidCastException();
            }
        }

        public override IWorld Execute(IActionContext context)
        {
            var world = context.PreviousState;
            var guildAddress = context.GetGuildAddress();

            if (world.TryGetGuild(guildAddress, out _))
            {
                throw new InvalidOperationException("The signer already has a guild.");
            }

            return world.SetGuild(guildAddress, new Model.Guild.Guild(guildAddress));
        }
    }
}
