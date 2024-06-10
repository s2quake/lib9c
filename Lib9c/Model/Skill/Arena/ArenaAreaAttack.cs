using System;
using System.Collections.Generic;
using Nekoyume.Model.Stat;
using Nekoyume.TableData;

namespace Nekoyume.Model.Skill.Arena
{
    [Serializable]
    public class ArenaAreaAttack : ArenaAttackSkill
    {
        public ArenaAreaAttack(
            SkillSheet.Row skillRow,
            long power,
            int chance,
            int statPowerRatio,
            StatType referencedStatType) : base(skillRow, power, chance, statPowerRatio, referencedStatType)
        {
        }

        public override BattleStatus.Arena.ArenaSkill Use(
            ArenaCharacter caster,
            ArenaCharacter target,
            int turn,
            IEnumerable<Buff.Buff> buffs)
        {
            var clone = (ArenaCharacter)caster.Clone();
            var damage = ProcessDamage(caster, target, turn);
            var buff = ProcessBuff(caster, target, turn, buffs);

            return new BattleStatus.Arena.ArenaAreaAttack(SkillRow.Id, clone, damage, buff);
        }
    }
}
