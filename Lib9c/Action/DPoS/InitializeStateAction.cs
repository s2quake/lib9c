using System.Collections.Immutable;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume.Action.DPoS.Control;
using Nekoyume.Action.DPoS.Misc;
using Nekoyume.Action.DPoS.Util;
using Nekoyume.Model.State;
using Nekoyume.Module;

namespace Nekoyume.Action.DPoS
{
    [ActionType(ActionTypeValue)]
    public sealed class InitializeStateAction : ActionBase
    {
        private const string ActionTypeValue = "initialize_state";

        public override IValue PlainValue => Bencodex.Types.Dictionary.Empty
            .Add("type_id", new Text(ActionTypeValue));

        public override void LoadPlainValue(IValue plainValue)
        {
        }

        public override IWorld Execute(IActionContext context)
        {
            context.UseGas(1);
            var currency = Currency.Legacy("NCG", 2, null);
            var world = context.PreviousState;
            var goldCurrencyState = new GoldCurrencyState(currency);

            return world.SetLegacyState(GoldCurrencyState.Address, goldCurrencyState.Serialize());
        }
    }
}
