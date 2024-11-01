#nullable enable
using System;
using System.Collections.Immutable;
using System.Numerics;
using Libplanet.Crypto;
using Libplanet.Types.Assets;

namespace Nekoyume.Delegation
{
    public interface IDelegatee
    {
        Address Address { get; }

        Address AccountAddress { get; }

        Currency DelegationCurrency { get; }

        Currency RewardCurrency { get; }

        Address DelegationPoolAddress { get; }

        Address RewardRemainderPoolAddress { get; }

        long UnbondingPeriod { get; }

        int MaxUnbondLockInEntries { get; }

        int MaxRebondGraceEntries { get; }

        Address RewardPoolAddress { get; }

        ImmutableSortedSet<Address> Delegators { get; }

        FungibleAssetValue TotalDelegated { get; }

        BigInteger TotalShares { get; }

        bool Jailed { get; }

        long JailedUntil { get; }

        bool Tombstoned { get; }

        BigInteger ShareFromFAV(FungibleAssetValue fav);

        FungibleAssetValue FAVFromShare(BigInteger share);

        BigInteger Bond(Address delegatorAddress, FungibleAssetValue fav);

        FungibleAssetValue Unbond(Address delegatorAddress, BigInteger share);

        void DistributeReward(Address delegatorAddress);

        void CollectRewards();

        void Slash(BigInteger slashFactor, long infractionHeight);

        void Jail(long releaseHeight);

        void Unjail();

        void Tombstone();

        Address BondAddress(Address delegatorAddress);

        Address UnbondLockInAddress(Address delegatorAddress);

        Address RebondGraceAddress(Address delegatorAddress);

        Address CurrentLumpSumRewardsRecordAddress();

        Address LumpSumRewardsRecordAddress(long height);

        event EventHandler<DelegationChangedEventArgs>? DelegationChanged;
    }
}
