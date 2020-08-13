using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Nekoyume.Battle;
using Nekoyume.Model.BattleStatus;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using Serilog;

namespace Nekoyume.Action
{
    [Serializable]
    [ActionType("ranking_battle")]
    public class RankingBattle : GameAction
    {
        public const int StageId = 999999;
        public static readonly BigInteger EntranceFee = 100;

        public Address AvatarAddress;
        public Address EnemyAddress;
        public Address WeeklyArenaAddress;
        public List<int> costumeIds;
        public List<Guid> equipmentIds;
        public List<Guid> consumableIds;
        public BattleLog Result { get; private set; }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            IActionContext ctx = context;
            var states = ctx.PreviousStates;
            if (ctx.Rehearsal)
            {
                return states.SetState(ctx.Signer, MarkChanged)
                    .SetState(AvatarAddress, MarkChanged)
                    .SetState(WeeklyArenaAddress, MarkChanged)
                    .SetState(ctx.Signer, MarkChanged)
                    .MarkBalanceChanged(GoldCurrencyMock, ctx.Signer, WeeklyArenaAddress);
            }

            if (AvatarAddress.Equals(EnemyAddress))
            {
                return LogError(context, "Aborted as the signer tried to battle for themselves.");
            }

            if (!states.TryGetAgentAvatarStates(ctx.Signer, AvatarAddress, out var agentState,
                out var avatarState))
            {
                return LogError(context, "Aborted as the avatar state of the signer was failed to load.");
            }

            // 도전자의 장비가 유효한지 검사한다.
            // 피도전자의 장비도 검사해야 하는가는 모르겠다. 이후에 필요하다면 추가하는 것으로 한다.
            if (!avatarState.ValidateEquipments(equipmentIds, context.BlockIndex))
            {
                // 장비가 유효하지 않은 에러.
                return LogError(context, "Aborted as the equipment is invalid.");
            }

            if (!avatarState.worldInformation.TryGetUnlockedWorldByStageClearedBlockIndex(out var world))
            {
                return LogError(context, "Aborted as the WorldInformation was failed to load or not cleared yet.");
            }

            if (world.StageClearedId < GameConfig.RequireClearedStageLevel.ActionsInRankingBoard)
            {
                // 스테이지 클리어 부족 에러.
                return LogError(
                    context,
                    "Aborted as the signer is not cleared the minimum stage level required to battle with other players yet: {ClearedLevel} < {RequiredLevel}.",
                    world.StageClearedId,
                    GameConfig.RequireClearedStageLevel.ActionsInRankingBoard
                );
            }

            avatarState.EquipCostumes(costumeIds);
            avatarState.EquipEquipments(equipmentIds);

            var enemyAvatarState = states.GetAvatarState(EnemyAddress);
            if (enemyAvatarState is null)
            {
                return LogError(
                    context,
                    "Aborted as the avatar state of the opponent ({OpponentAddress}) was failed to load.",
                    EnemyAddress
                );
            }

            var weeklyArenaState = states.GetWeeklyArenaState(WeeklyArenaAddress);

            //FIXME 오류던지게 고쳐야함
            if (weeklyArenaState.Ended)
            {
                return LogError(context, "Aborted as the weekly arena state already ended.");
            }

            if (!weeklyArenaState.ContainsKey(AvatarAddress))
            {
                return LogError(context, "Aborted as the weekly arena state was failed to load.");
            }

            var arenaInfo = weeklyArenaState[AvatarAddress];

            if (arenaInfo.DailyChallengeCount <= 0)
            {
                return LogError(context, "Aborted as the arena state reached the daily limit.");
            }

            if (!arenaInfo.Active)
            {
                BigInteger agentBalance = states.GetBalance(ctx.Signer, states.GetGoldCurrency());

                if (agentBalance >= EntranceFee)
                {
                    states = states.TransferAsset(
                        ctx.Signer,
                        WeeklyArenaAddress,
                        states.GetGoldCurrency(),
                        EntranceFee);
                    arenaInfo.Activate();
                }
                else
                {
                    return LogError(
                        context,
                        "Aborted as the signer's balance ({Balance}) is insufficient to pay entrance fee/stake ({EntranceFee}).",
                        agentBalance,
                        EntranceFee
                    );
                }
            }

            if (!weeklyArenaState.ContainsKey(EnemyAddress))
            {
                return LogError(
                    context,
                    "Aborted as the opponent ({OpponentAddress}) is not registered in the weekly arena state.",
                    EnemyAddress
                );
            }

            Log.Debug(weeklyArenaState.address.ToHex());

            var tableSheetState = TableSheetsState.FromActionContext(ctx);
            var tableSheets = TableSheets.FromTableSheetsState(tableSheetState);

            var simulator = new RankingSimulator(
                ctx.Random,
                avatarState,
                enemyAvatarState,
                consumableIds,
                tableSheets,
                StageId);

            simulator.Simulate();

            simulator.Log.diffScore = arenaInfo.Update(avatarState, weeklyArenaState[EnemyAddress], simulator.Result);
            simulator.Log.score = arenaInfo.Score;

            Result = simulator.Log;

            var maxCount = ctx.Random.Next(1, 6);
            var rewards = new List<ItemBase>();
            var rewardSheet = tableSheets.WeeklyArenaRewardSheet;
            var itemSelector = new WeightedSelector<StageSheet.RewardData>(ctx.Random);
            foreach (var row in rewardSheet.Values)
            {
                var reward = row.Reward;
                itemSelector.Add(reward, reward.Ratio);
            }

            while (rewards.Count < maxCount)
            {
                try
                {
                    var data = itemSelector.Select(1).First();
                    if (tableSheets.MaterialItemSheet.TryGetValue(data.ItemId, out var itemData))
                    {
                        var count = context.Random.Next(data.Min, data.Max + 1);
                        for (var i = 0; i < count; i++)
                        {
                            var item = ItemFactory.CreateMaterial(itemData);
                            if (rewards.Count < maxCount)
                            {
                                rewards.Add(item);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch (ListEmptyException)
                {
                    break;
                }
            }

            foreach (var item in rewards)
            {
                avatarState.inventory.AddItem(item);
            }

            return states
                .SetState(ctx.Signer, agentState.Serialize())
                .SetState(WeeklyArenaAddress, weeklyArenaState.Serialize())
                .SetState(AvatarAddress, avatarState.Serialize());
        }

        protected override IImmutableDictionary<string, IValue> PlainValueInternal =>
            new Dictionary<string, IValue>
            {
                ["avatarAddress"] = AvatarAddress.Serialize(),
                ["enemyAddress"] = EnemyAddress.Serialize(),
                ["weeklyArenaAddress"] = WeeklyArenaAddress.Serialize(),
                ["costume_ids"] = new Bencodex.Types.List(costumeIds.Select(e => e.Serialize())),
                ["equipment_ids"] = new Bencodex.Types.List(equipmentIds.Select(e => e.Serialize())),
                ["consumable_ids"] = new Bencodex.Types.List(consumableIds.Select(e => e.Serialize())),
            }.ToImmutableDictionary();

        protected override void LoadPlainValueInternal(IImmutableDictionary<string, IValue> plainValue)
        {
            AvatarAddress = plainValue["avatarAddress"].ToAddress();
            EnemyAddress = plainValue["enemyAddress"].ToAddress();
            WeeklyArenaAddress = plainValue["weeklyArenaAddress"].ToAddress();
            costumeIds = ((Bencodex.Types.List) plainValue["costume_ids"]).Select(
                e => e.ToInteger()
            ).ToList();
            equipmentIds = ((Bencodex.Types.List) plainValue["equipment_ids"]).Select(
                e => e.ToGuid()
            ).ToList();
            consumableIds = ((Bencodex.Types.List) plainValue["consumable_ids"]).Select(
                e => e.ToGuid()
            ).ToList();

        }
    }
}
