using System.Collections.Immutable;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume.Action.DPoS.Control;
using Nekoyume.Action.DPoS.Misc;
using Nekoyume.Action.DPoS.Util;
using Nekoyume.Module;

namespace Nekoyume.Action.DPoS
{
    [ActionType(ActionTypeValue)]
    public sealed class InitializeValidator : ActionBase
    {
        private const string ActionTypeValue = "initialize_validator";

        public InitializeValidator(PublicKey validator, FungibleAssetValue amount)
        {
            Validator = validator;
            Amount = amount;
        }

        public InitializeValidator()
        {
            Validator = new PrivateKey().PublicKey;
        }

        public PublicKey Validator { get; set; }

        public FungibleAssetValue Amount { get; set; }

        public override IValue PlainValue => Bencodex.Types.Dictionary.Empty
            .Add("type_id", new Text(ActionTypeValue))
            .Add("validator", Validator.Serialize())
            .Add("amount", Amount.Serialize());

        public override void LoadPlainValue(IValue plainValue)
        {
            var dict = (Bencodex.Types.Dictionary)plainValue;
            Validator = dict["validator"].ToPublicKey();
            Amount = dict["amount"].ToFungibleAssetValue();
        }

        public override IWorld Execute(IActionContext context)
        {
            context.UseGas(1);
            var world = context.PreviousState;
            var nativeTokens = world.GetNativeTokens();

            world = ValidatorCtrl.Create(
                world,
                context,
                Validator.Address,
                Validator,
                Amount,
                nativeTokens);

            return world;
        }
    }
}
