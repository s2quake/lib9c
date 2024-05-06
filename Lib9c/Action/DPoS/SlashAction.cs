using System.Collections.Immutable;
using Bencodex.Types;
using Nekoyume.Action.DPoS.Control;
using Nekoyume.Action.DPoS.Misc;
using Nekoyume.Action.DPoS.Model;
using Libplanet.Action;
using Libplanet.Action.State;
using Nekoyume.Module;
using Libplanet.Types.Consensus;
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
            var votes = context.LastCommit?.Votes ?? ImmutableArray.Create<Vote>();
            var nativeTokens = ImmutableHashSet.Create(
                Asset.GovernanceToken, Asset.ConsensusToken, Asset.Share);

            var validatorSet = states.GetValidatorSet();
            foreach (var vote in votes)
            {
                var operatorAddress = GetOperatorAddress(validatorSet, vote.ValidatorPublicKey);
                var validatorAddress = Model.Validator.DeriveAddress(operatorAddress);

                var noVoteAddress = validatorAddress.Derive("no-vote");
                if (states.GetDPoSState(noVoteAddress) is Integer integer)
                {
                    SlashCtrl.Execute(
                        world: states,
                        actionContext: context,
                        operatorAddress: operatorAddress,
                        power: integer.Value,
                        signed: true,
                        nativeTokens: nativeTokens);
                    states.RemoveDPoSState(noVoteAddress);
                }

                if (ValidatorCtrl.GetValidator(states, validatorAddress) is { } &&
                    EvidenceCtrl.GetEvidence(states, validatorAddress) is { } evidence)
                {
                    states = EvidenceCtrl.Execute(
                        world: states,
                        actionContext: context,
                        validatorAddress: validatorAddress,
                        evidence: evidence,
                        nativeTokens: nativeTokens);
                }
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
    }
}
