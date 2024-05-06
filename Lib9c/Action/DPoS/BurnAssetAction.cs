using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume.Action.DPoS.Util;
using Nekoyume.Module;

namespace Nekoyume.Action.DPoS
{
    /// <summary>
    /// A system action for DPoS that <see cref="Delegate"/> specified <see cref="Amount"/>
    /// of tokens to a given <see cref="Address"/>.
    /// </summary>
    [ActionType(ActionTypeValue)]
    public sealed class BurnAssetAction : IAction
    {
        private const string ActionTypeValue = "burn_asset_action";

        /// <summary>
        /// Creates a new instance of <see cref="Delegate"/> action.
        /// </summary>
        /// <param name="address">The <see cref="Libplanet.Crypto.Address"/> of the validator
        /// to delegate tokens.</param>
        /// <param name="amount">The amount of the asset to be delegated.</param>
        public BurnAssetAction(Address address, FungibleAssetValue amount)
        {
            Address = address;
            Amount = amount;
        }

        public BurnAssetAction()
        {
            // Used only for deserialization.  See also class Libplanet.Action.Sys.Registry.
        }

        /// <summary>
        /// The <see cref="Libplanet.Crypto.Address"/> of the validator to <see cref="Delegate"/>.
        /// </summary>
        public Address Address { get; set; }

        public FungibleAssetValue Amount { get; set; }

        /// <inheritdoc cref="IAction.PlainValue"/>
        public IValue PlainValue => Bencodex.Types.Dictionary.Empty
            .Add("type_id", new Text(ActionTypeValue))
            .Add("address", Address.Serialize())
            .Add("amount", Amount.Serialize());

        /// <inheritdoc cref="IAction.LoadPlainValue(IValue)"/>
        public void LoadPlainValue(IValue plainValue)
        {
            var dict = (Bencodex.Types.Dictionary)plainValue;
            Address = dict["address"].ToAddress();
            Amount = dict["amount"].ToFungibleAssetValue();
        }

        /// <inheritdoc cref="IAction.Execute(IActionContext)"/>
        public IWorld Execute(IActionContext context)
        {
            IActionContext ctx = context;
            var states = ctx.PreviousState;
            return states.BurnAsset(context, Address, Amount);
        }
    }
}
