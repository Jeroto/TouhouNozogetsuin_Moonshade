using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
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

    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    public WeaknessAndResistance[] weaknessesAndResistances;

    public RuntimeAnimatorController animations;
    public bool hoverAnim;
    public float hoverDist;
    public int hoverTime;

    public DamageTypeUnlocks[] damageTypeUnlocks;

    public SkillUnlocks[] skills;

    public Sprite portrait;

    public enum CharType { Spiritual, Mental, Physical, Ancient, Elemental };
    public enum CharSubtype { Magic, Distortion, Power, Agility, Boundary, Holy, Hex, Audio, Visual, Resistance, Recovery, Peace };
    public enum Stat {None, HP, MP, Attack, SubAttack, Defense, MentalDef, TurnSpeed};

    public enum StatusEffectType
    {/*Negative*/
        FocusLock /*Cannot unfocus*/, UnfocusLock /*Cannot focus*/, Poison /*Spawn poison clouds while dodging*/, Weak /*Decrease attack*/,
        Frail /*decrease defense*/, Cursed /*Highly random. Lasts until cured, and gives one turn of a random debuff*/, MoveSpeedUp /*Move faster in shmup-space*/,
        MoveSpeedSlow /*Move slower in shmup-space*/, Confused /*Character only attacks at random -- including allies*/,
        Dizzy /*Full control over character, but may act upon wrong target*/, Silence /*Cannot bomb or use skills*/, GrowBigger /*Player size increased*/,
        /*Positive*/
        Regenerate /*Regain health every frame*/, AttackUp /*Raises attack*/, DefenseUp /*Raises defense*/, MentalDefUp /*Raises mental defense*/,
        TurnSpeedUp /*Raises rpg speed*/, Graze /*decrease hitbox size slightly*/, Concentrating /*Decrease skill cost for a short time*/
    };

    [System.Serializable]
    public struct StatusEffect
    {
        public StatusEffectType effectType;
        public int strength;
        public int turnDuration;
        public bool remainAfterBattle;
    }


    [System.Serializable]
    public struct WeaknessAndResistance
    {
        public GameMasterScript.DamageTypes damageType;
        public float damageMultiplier;
    }

    [System.Serializable]
    public struct DamageTypeUnlocks
    {
        public GameMasterScript.DamageTypes damageType;
        public uint levelToUnlock;
    }
    
    [System.Serializable]
    public struct SkillUnlocks
    {
        public SkillData skill;
        public uint levelToUnlock;
    }
}
