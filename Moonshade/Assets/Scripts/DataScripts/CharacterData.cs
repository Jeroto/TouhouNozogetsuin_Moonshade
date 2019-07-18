using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    public string charName;
    public ulong charExp;
    public uint charLevel;

    public int currentHP;
    public int maxHP;
    public int currentMP;
    public int maxMP;

    public int attack;
    public int subAttack;
    public int defense;
    public int mentalDef;
    public int turnSpeed;

    public List<Character.StatusEffect> statusEffects = new List<Character.StatusEffect>();

    public Character.WeaknessAndResistance[] weaknessesAndResistances;
    public Character.DamageTypeUnlocks[] damageTypeUnlocks;
    public Character.SkillUnlocks[] skills;

    public RuntimeAnimatorController animations;
    public bool hoverAnim;
    public float hoverDist;
    public int hoverTime;
    [Space]
    public Sprite portrait;
}
