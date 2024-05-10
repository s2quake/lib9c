using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Blockchain;
using Libplanet.Crypto;
using Libplanet.Store;
using Libplanet.Types.Assets;
using Libplanet.Types.Blocks;
using Libplanet.Types.Tx;
using Nekoyume.Action.DPoS;
using Nekoyume.Action.DPoS.Misc;
using Nekoyume.Action.DPoS.Sys;

namespace Nekoyume;

public class DPoSBlockHelper
{
    public static Block ProposeGenesisBlock(
        PrivateKey? privateKey,
        IStateStore stateStore,
        Dictionary<Address, BigInteger> initialNCGs,
        Dictionary<PublicKey, BigInteger> initialValidators)
    {
        privateKey ??= new PrivateKey();
        var trie = stateStore.GetStateRoot(null);
        IWorld world = new World(new WorldBaseState(trie, stateStore));
        foreach (var pair in initialNCGs)
        {
            var actionContext = new ActionContext
            {
                PreviousState = world,
                Signer = privateKey.Address,
            };
            var amount = new FungibleAssetValue(
                Asset.GovernanceToken,
                pair.Value,
                BigInteger.Zero);
            world = world.MintAsset(actionContext, pair.Key, amount);
        }

        foreach (var pair in initialValidators)
        {
            var actionContext = new ActionContext
            {
                PreviousState = world,
                Signer = pair.Key.Address,
            };
            var amount = new FungibleAssetValue(
                Asset.GovernanceToken,
                pair.Value,
                BigInteger.Zero);
            world = new PromoteValidator(pair.Key, amount).Execute(actionContext);
        }

        world = new UpdateValidators().Execute(
            new ActionContext
            {
                PreviousState = world,
                Signer = privateKey.Address
            });
        trie = stateStore.Commit(world.Trie);

        return
            BlockChain.ProposeGenesisBlock(
                privateKey: privateKey,
                stateRootHash: trie.Hash,
                transactions: ImmutableList<Transaction>.Empty,
                timestamp: DateTimeOffset.UtcNow);
    }

    private class ActionContext : IActionContext
    {
        public Address Signer { get; set; }

        public TxId? TxId => null;

        public Address Miner { get; set; }

        public int BlockProtocolVersion { get; set; }

        public BlockCommit? LastCommit => null;

        public long BlockIndex => 0;

        public IWorld PreviousState { get; set; }

        public int RandomSeed { get; set; }

        public bool IsBlockAction => true;

        public FungibleAssetValue? MaxGasPrice => null;

        public IReadOnlyList<ITransaction> Txs => Enumerable.Empty<ITransaction>().ToList();

        public void UseGas(long gas)
        {
            throw new NotImplementedException();
        }

        public IRandom GetRandom()
        {
            throw new NotImplementedException();
        }

        public long GasUsed()
        {
            throw new NotImplementedException();
        }

        public long GasLimit()
        {
            throw new NotImplementedException();
        }
    }
}
