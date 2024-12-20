using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bencodex.Types;
using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.Module;
using Nekoyume.TableData;
using Serilog;
using static Lib9c.SerializeKeys;

namespace Nekoyume.Action
{
    /// <summary>
    /// Hard forked at https://github.com/planetarium/lib9c/pull/602
    /// Updated at https://github.com/planetarium/lib9c/pull/861
    /// Updated at https://github.com/planetarium/lib9c/pull/957
    /// </summary>
    [Serializable]
    [ActionType("redeem_code3")]
    public class RedeemCode : GameAction, IRedeemCodeV1
    {
        public string Code { get; internal set; }

        public Address AvatarAddress {get; internal set; }

        string IRedeemCodeV1.Code => Code;
        Address IRedeemCodeV1.AvatarAddress => AvatarAddress;

        public RedeemCode()
        {
        }

        public RedeemCode(string code, Address avatarAddress)
        {
            Code = code;
            AvatarAddress = avatarAddress;
        }

        public override IWorld Execute(IActionContext context)
        {
            GasTracer.UseGas(1);
            var states = context.PreviousState;
            var addressesHex = GetSignerAndOtherAddressesHex(context, AvatarAddress);
            var started = DateTimeOffset.UtcNow;
            Log.Debug("{AddressesHex}RedeemCode exec started", addressesHex);

            if (!states.TryGetAvatarState(context.Signer, AvatarAddress, out AvatarState avatarState))
            {
                return states;
            }

            var redeemState = states.GetRedeemCodeState();
            if (redeemState is null)
            {
                return states;
            }

            int redeemId;
            try
            {
                redeemId = redeemState.Redeem(Code, AvatarAddress);
            }
            catch (InvalidRedeemCodeException)
            {
                Log.Error("{AddressesHex}Invalid Code", addressesHex);
                throw;
            }
            catch (DuplicateRedeemException e)
            {
                Log.Warning("{AddressesHex}{Message}", addressesHex, e.Message);
                throw;
            }

            var row = states.GetSheet<RedeemRewardSheet>().Values.First(r => r.Id == redeemId);
            var itemSheets = states.GetItemSheet();

            var random = context.GetRandom();
            foreach (RedeemRewardSheet.RewardInfo info in row.Rewards)
            {
                switch (info.Type)
                {
                    case RewardType.Item:
                        for (var i = 0; i < info.Quantity; i++)
                        {
                            if (info.ItemId is int itemId)
                            {
                                ItemBase item = ItemFactory.CreateItem(itemSheets[itemId], random);
                                // We should fix count as 1 because ItemFactory.CreateItem
                                // will create a new item every time.
                                avatarState.inventory.AddItem(item, count: 1);
                            }
                        }
                        break;
                    case RewardType.Gold:
                        states = states.TransferAsset(
                            context,
                            GoldCurrencyState.Address,
                            context.Signer,
                            states.GetGoldCurrency() * info.Quantity
                        );
                        break;
                    default:
                        // FIXME: We should raise exception here.
                        break;
                }
            }
            var ended = DateTimeOffset.UtcNow;
            Log.Debug("{AddressesHex}RedeemCode Total Executed Time: {Elapsed}", addressesHex, ended - started);
            return states
                .SetAvatarState(AvatarAddress, avatarState)
                .SetLegacyState(RedeemCodeState.Address, redeemState.Serialize());
        }

        protected override IImmutableDictionary<string, IValue> PlainValueInternal =>
            new Dictionary<string, IValue>
            {
                [nameof(Code)] = Code.Serialize(),
                [nameof(AvatarAddress)] = AvatarAddress.Serialize()
            }.ToImmutableDictionary();

        protected override void LoadPlainValueInternal(IImmutableDictionary<string, IValue> plainValue)
        {
            Code = (Text) plainValue[nameof(Code)];
            AvatarAddress = plainValue[nameof(AvatarAddress)].ToAddress();
        }
    }
}
