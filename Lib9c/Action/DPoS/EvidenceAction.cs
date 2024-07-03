using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Nekoyume.Action.DPoS.Control;
using Nekoyume.Action.DPoS.Exception;
using Nekoyume.Action.DPoS.Model;
using Nekoyume.Action.DPoS.Util;

namespace Nekoyume.Action.DPoS
{
    [ActionType(ActionTypeValue)]
    public sealed class EvidenceAction : IAction
    {
        private const string ActionTypeValue = "evidence";

        public EvidenceAction()
        {
        }

        public Address Validator { get; set; }

        public IValue PlainValue => Bencodex.Types.Dictionary.Empty
            .Add("type_id", new Text(ActionTypeValue))
            .Add("address", Validator.Serialize());

        public void LoadPlainValue(IValue plainValue)
        {
            var dict = (Bencodex.Types.Dictionary)plainValue;
            Validator = dict["address"].ToAddress();
        }

        public IWorld Execute(IActionContext context)
        {
            var world = context.PreviousState;
            var validatorAddress = Validator;

            if (ValidatorCtrl.GetValidator(world, validatorAddress) is not { } validator)
            {
                throw new NullValidatorException(validatorAddress);
            }

            var evidence = new Evidence()
            {
                Address = Validator,
                Height = context.BlockIndex,
                Power = validator.DelegatorShares.RawValue,
            };

            return EvidenceCtrl.SetEvidence(world, evidence);
        }
    }
}
