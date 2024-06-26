using System;
using System.Collections;
using System.Collections.Generic;

namespace Nekoyume.Model.BattleStatus.Arena
{
    [Serializable]
    public class ArenaHeal : ArenaSkill
    {
        public ArenaHeal(
            int skillId,
            ArenaCharacter character,
            IEnumerable<ArenaSkillInfo> skillInfos,
            IEnumerable<ArenaSkillInfo> buffInfos)
            : base(skillId, character, skillInfos, buffInfos)
        {
        }

        public override IEnumerator CoExecute(IArena arena)
        {
            yield return arena.CoHeal(Character, SkillInfos, BuffInfos);
        }
    }
}
