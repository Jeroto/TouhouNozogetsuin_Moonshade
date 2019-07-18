using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public string skillName;
    public int skillCost;
    public bool captureRequired;
    public int specialAbility;
    [Space]

    public SkillType skillType;
    public int casterHpChange;
    public int casterMpChange;
    public int hpChange;
    public int mpChange;
    public Character.StatusEffect[] statusEffectsToRemove;
    public Character.StatusEffect[] statusEffects;



    public enum SkillType {EnemyDebuff, EnemyPartyDebuff, EnemyAreaDebuff,
    AllyBuff, AllyPartyBuff, AllyAreaBuff, FieldWide, Selfcast};

    public static void UseSkill(SkillData skill, Character caster, Character[] targets)
    {
        caster.currentMP = Mathf.Clamp(caster.currentMP - skill.skillCost, 0, caster.maxMP);

        SpecialSkillAbility(skill, caster, targets);

        //Place character in photo-mode in order to place or remove debuffs
        Character[] affected;
        if(skill.skillType == SkillType.Selfcast)
        {
            affected = new Character[] { caster };
        }
        else
        {
            affected = new Character[targets.Length];
            for (int i = 0; i < affected.Length; i++)
            {
                affected[i] = targets[i];
            }
        }
        
        caster.currentHP = Mathf.Clamp(caster.currentHP + skill.casterHpChange, 0, caster.maxHP);
        caster.currentMP = Mathf.Clamp(caster.currentMP + skill.casterMpChange, 0, caster.maxMP);

        bool matchingStatus;
        bool incompatibleStatus;
        for (int i = 0; i < affected.Length; i++)
        {
            for (int a = 0; a < skill.statusEffects.Length; a++)
            {
                matchingStatus = false;
                incompatibleStatus = false;
                for (int b = 0; b < affected[i].statusEffects.Count; b++)
                {
                    if (affected[i].statusEffects[b].effectType == skill.statusEffects[a].effectType &&
                        affected[i].statusEffects[b].strength == skill.statusEffects[a].strength &&
                        affected[i].statusEffects[b].turnDuration >= skill.statusEffects[a].turnDuration &&
                        affected[i].statusEffects[b].remainAfterBattle == skill.statusEffects[a].remainAfterBattle)
                    {
                        matchingStatus = true;
                    }
                    if ((affected[i].statusEffects[b].effectType == Character.StatusEffectType.FocusLock && skill.statusEffects[a].effectType == Character.StatusEffectType.UnfocusLock) ||
                        (affected[i].statusEffects[b].effectType == Character.StatusEffectType.UnfocusLock && skill.statusEffects[a].effectType == Character.StatusEffectType.FocusLock) ||
                        (affected[i].statusEffects[b].effectType == Character.StatusEffectType.MoveSpeedUp && skill.statusEffects[a].effectType == Character.StatusEffectType.MoveSpeedSlow) ||
                        (affected[i].statusEffects[b].effectType == Character.StatusEffectType.MoveSpeedSlow && skill.statusEffects[a].effectType == Character.StatusEffectType.MoveSpeedUp))
                    {
                        incompatibleStatus = true;
                    }
                }
                if(!matchingStatus)
                {
                    if(!incompatibleStatus)
                    {
                        for (int b = 0; b < affected[i].statusEffects.Count; b++)
                        {
                            if (affected[i].statusEffects[b].effectType == skill.statusEffects[a].effectType &&
                                affected[i].statusEffects[b].strength == skill.statusEffects[a].strength &&
                                affected[i].statusEffects[b].turnDuration < skill.statusEffects[a].turnDuration)
                            {
                                affected[i].statusEffects.RemoveAt(b);
                            }
                        }
                        affected[i].statusEffects.Add(skill.statusEffects[a]);
                    }
                    else
                    {
                        while(incompatibleStatus)
                        {
                            incompatibleStatus = false;
                            for (int b = 0; b < affected[i].statusEffects.Count; b++)
                            {
                                if ((affected[i].statusEffects[b].effectType == Character.StatusEffectType.FocusLock && skill.statusEffects[a].effectType == Character.StatusEffectType.UnfocusLock) ||
                                    (affected[i].statusEffects[b].effectType == Character.StatusEffectType.UnfocusLock && skill.statusEffects[a].effectType == Character.StatusEffectType.FocusLock) ||
                                    (affected[i].statusEffects[b].effectType == Character.StatusEffectType.MoveSpeedUp && skill.statusEffects[a].effectType == Character.StatusEffectType.MoveSpeedSlow) ||
                                    (affected[i].statusEffects[b].effectType == Character.StatusEffectType.MoveSpeedSlow && skill.statusEffects[a].effectType == Character.StatusEffectType.MoveSpeedUp))
                                {
                                    incompatibleStatus = true;
                                    if (affected[i].statusEffects[b].strength <= skill.statusEffects[a].strength)
                                    {
                                        affected[i].statusEffects.RemoveAt(b);
                                        affected[i].statusEffects.Add(skill.statusEffects[a]);
                                    }
                                    else
                                        affected[i].statusEffects.Remove(skill.statusEffects[a]);
                                }
                            }
                        }
                        matchingStatus = true;
                        while(matchingStatus)
                        {
                            matchingStatus = false;

                            for (int b = 0; b < affected[i].statusEffects.Count; b++)
                            {
                                for (int c = 0; c < affected[i].statusEffects.Count; c++)
                                {
                                    if(b != c)
                                    {
                                        if (affected[i].statusEffects[b].effectType == affected[i].statusEffects[c].effectType &&
                                        affected[i].statusEffects[b].strength == affected[i].statusEffects[c].strength &&
                                        affected[i].statusEffects[b].turnDuration >= affected[i].statusEffects[c].turnDuration &&
                                        affected[i].statusEffects[b].remainAfterBattle == affected[i].statusEffects[c].remainAfterBattle)
                                        {
                                            affected[i].statusEffects.RemoveAt(c);
                                            matchingStatus = true;
                                            break;
                                        }
                                    }
                                }
                                if (matchingStatus)
                                    break;
                            }
                        }
                    }
                }
            }
            targets[i].currentHP = Mathf.Clamp(targets[i].currentHP + skill.hpChange, 0, targets[i].maxHP);
            targets[i].currentMP = Mathf.Clamp(targets[i].currentMP + skill.mpChange, 0, targets[i].maxMP);
        }
        
    }



    public static void SpecialSkillAbility(SkillData skill, Character caster, Character[] targets)
    {
        switch(skill.specialAbility)
        {
            case 1:
                DebugSkillAbility();
                break;
        }
    }

    public static void DebugSkillAbility()
    {
        Debug.Log("Special skill used~ owo");
    }
}
