﻿using System;

using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.AI.v2.Params.Almighty;
using AAEmu.Game.Models.Game.Models;
using AAEmu.Game.Models.Game.Skills;
using AAEmu.Game.Models.Game.Skills.Static;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Game.Units.Movements;

namespace AAEmu.Game.Models.Game.AI.v2.Behaviors.Common;

public class SpawningBehavior : BaseCombatBehavior
{
    private bool _usedSpawnSkills = false;
    private bool _enter;

    public override void Enter()
    {
        Ai.Owner.CurrentGameStance = GameStanceType.Relaxed;
        Ai.Owner.CurrentAlertness = MoveTypeAlertness.Idle;
        if (Ai.Owner is { } npc)
        {
            npc.Events.OnSpawn(this, new OnSpawnArgs { Npc = npc });
        }

        // TODO 
        var _aiParams = Ai.Owner.Template.AiParams as AlmightyNpcAiParams;
        if (_aiParams != null && _aiParams.AlertToAttack && _aiParams.AlertDuration == 0)
        {
            CheckAggression();
        }
        _enter = true;
    }

    public override void Tick(TimeSpan delta)
    {
        if (!_enter)
            return; // not initialized yet Enter()

        // TODO: Figure out how to do this on spawn
        if (Ai.Owner.Template.Skills.ContainsKey(SkillUseConditionKind.OnSpawn) && !_usedSpawnSkills)
        {
            _usedSpawnSkills = true;
            var skills = Ai.Owner.Template.Skills[SkillUseConditionKind.OnSpawn];

            foreach (var npcSkill in skills)
            {
                var skillTemplate = SkillManager.Instance.GetSkillTemplate(npcSkill.SkillId);
                var skill = new Skill(skillTemplate);

                var skillCaster = SkillCaster.GetByType(SkillCasterType.Unit);
                skillCaster.ObjId = Ai.Owner.ObjId;

                var skillTarget = SkillCastTarget.GetByType(SkillCastTargetType.Unit);
                skillTarget.ObjId = Ai.Owner.ObjId;

                skill.Use(Ai.Owner, skillCaster, skillTarget, null, true, out _);
            }
        }

        // TODO: This follows the game's way of doing it. This will need code later, obviously
        Ai.GoToRunCommandSet();
    }

    public override void Exit()
    {
        _enter = false;
    }
}
