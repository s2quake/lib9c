using System.Linq;
using Libplanet.Action.State;
using Nekoyume.Action;
using Nekoyume.Model.Item;
using Nekoyume.Module;
using Nekoyume.TableData;

namespace Nekoyume.Helper
{
    public static class InventoryExtensions
    {
        public static int GetEquippedFullCostumeOrArmorId(this Inventory inventory)
        {
            foreach (var costume in inventory.Costumes
                         .Where(e =>
                             e.ItemSubType == ItemSubType.FullCostume &&
                             e.Equipped))
            {
                return costume.Id;
            }

            foreach (var armor in inventory.Equipments
                         .Where(e =>
                             e.ItemSubType == ItemSubType.Armor &&
                             e.Equipped))
            {
                return armor.Id;
            }

            return GameConfig.DefaultAvatarArmorId;
        }

        public static long UseActionPoint(
            this Inventory inventory,
            long originalAp,
            int requiredAp,
            bool chargeAp,
            MaterialItemSheet materialItemSheet,
            long blockIndex)
        {
            if (originalAp < requiredAp)
            {
                switch (chargeAp)
                {
                    case true:
                        var row = materialItemSheet
                            .OrderedList!
                            .First(r => r.ItemSubType == ItemSubType.ApStone);
                        if (!inventory.RemoveFungibleItem(row.ItemId, blockIndex))
                        {
                            throw new NotEnoughMaterialException("not enough ap stone.");
                        }

                        originalAp = DailyReward.ActionPointMax;
                        break;
                    case false:
                        throw new NotEnoughActionPointException("");
                }
            }

            originalAp -= requiredAp;
            return originalAp;
        }
    }
}
