using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "SkillData", menuName = "SpecialInventoryDatas/SkillData", order = 2)]
public class SkillData : ScriptableObject
{
    public string skillName;
    public int skillCost;
    public bool captureRequired;
    public int specialAbility;
    [Space]

    public Skill.SkillType skillType;
    public bool livingTarget;
    [Space]
    public int casterHpChange;
    public int casterMpChange;
    public int hpChange;
    public int mpChange;
    public Character.StatusEffect[] statusEffectsToRemove;
    public Character.StatusEffect[] statusEffects;
}
