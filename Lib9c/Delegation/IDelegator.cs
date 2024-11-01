#nullable enable
using System;
using System.Collections.Immutable;
using System.Numerics;
using Libplanet.Crypto;
using Libplanet.Types.Assets;

namespace Nekoyume.Delegation
{
    public interface IDelegator
    {
        Address Address { get; }

        Address AccountAddress { get; }

        Address DelegationPoolAddress { get; }

        Address RewardAddress { get; }

        ImmutableSortedSet<Address> Delegatees { get; }

        void Delegate(
            Address delegateeAddress,
            FungibleAssetValue fav);

        void Undelegate(
            Address delegateeAddress,
            BigInteger share);

        void Redelegate(
            Address srcDelegateeAddress,
            Address dstDelegateeAddress,
            BigInteger share);

        void CancelUndelegate(
            Address delegateeAddress,
            FungibleAssetValue fav);

        void ClaimReward(
            Address delegateeAddress);
    }
}
