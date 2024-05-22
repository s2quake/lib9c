using System.Collections.Immutable;
using System.Linq;
using Bencodex.Types;
using Nekoyume.Action.DPoS.Control;
using Nekoyume.Action.DPoS.Misc;
using Nekoyume.Action.DPoS.Model;
using Libplanet.Action;
using Libplanet.Action.State;
using Nekoyume.Module;
using Libplanet.Types.Consensus;
using Libplanet.Types.Evidences;
using Libplanet.Crypto;
using Nekoyume.Action.DPoS.Util;

namespace Nekoyume.Action.DPoS
{
    /// <summary>
    /// A block action for DPoS that updates <see cref="ValidatorSet"/>.
    /// </summary>
    public sealed class SlashAction : IAction
    {
        /// <summary>
        /// Creates a new instance of <see cref="SlashAction"/>.
        /// </summary>
        public SlashAction()
        {
        }

        /// <inheritdoc cref="IAction.PlainValue"/>
        public IValue PlainValue => new Bencodex.Types.Boolean(true);

        /// <inheritdoc cref="IAction.LoadPlainValue(IValue)"/>
        public void LoadPlainValue(IValue plainValue)
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc cref="IAction.Execute(IActionContext)"/>
        public IWorld Execute(IActionContext context)
        {
            var states = context.PreviousState;
            var validatorSet = states.GetValidatorSet();
            var evidences = context.Evidences;

            foreach (var evidence in evidences)
            {
                states = SlashByEvidence(
                    world: states,
                    actionContext: context,
                    validatorSet: validatorSet,
                    evidence: evidence);
            }

            return states;
        }

        private static Address GetOperatorAddress(
            Libplanet.Types.Consensus.ValidatorSet validatorSet,
            PublicKey publicKey)
        {
            var validatorIndex = validatorSet.FindIndex(publicKey);
            var validator = validatorSet[validatorIndex];
            return validator.OperatorAddress;
        }

        private static IWorld SlashByEvidence(
            IWorld world,
            IActionContext actionContext,
            Libplanet.Types.Consensus.ValidatorSet validatorSet,
            Evidence evidence)
        {
            var targetAddress = evidence.TargetAddress;
            var validators = validatorSet.Validators;

            if (validators.FirstOrDefault(item => item.PublicKey.Address == targetAddress) is
                Libplanet.Types.Consensus.Validator validator)
            {
                var nativeTokens = ImmutableHashSet.Create(
                    Asset.GovernanceToken, Asset.ConsensusToken, Asset.Share);
                var operatorAddress = GetOperatorAddress(validatorSet, validator.PublicKey);
                var validatorAddress = Model.Validator.DeriveAddress(operatorAddress);
                world = EvidenceCtrl.Execute(
                    world: world,
                    actionContext: actionContext,
                    validatorAddress: validatorAddress,
                    evidence: new Equivocation
                    {
                        Height = evidence.Height,
                        Power = validator.Power,
                        Address = targetAddress,
                    },
                    nativeTokens: nativeTokens);
            }

            return world;
        }
    }
}
