using System;
using System.Collections;
using System.Collections.Generic;

namespace Nekoyume.Model.BattleStatus.Arena
{
    [Serializable]
    public class ArenaBlowAttack : ArenaSkill
    {
        public ArenaBlowAttack(
            int skillId,
            ArenaCharacter character,
            IEnumerable<ArenaSkillInfo> skillInfos,
            IEnumerable<ArenaSkillInfo> buffInfos)
            : base(skillId, character, skillInfos, buffInfos)
        {
        }

        public override IEnumerator CoExecute(IArena arena)
        {
            yield return arena.CoBlowAttack(Character, SkillInfos, BuffInfos);
        }
    }
}
