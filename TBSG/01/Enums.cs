namespace EEANWorks.Games.TBSG._01
{
    public enum eStringMatchType
    {
        Equals,
        Contains,
        StartsWith,
        EndsWith,
    }

    public enum eGender
    {
        Undefinable,
        Female,
        Male,
    }

    public enum eTargetRangeClassification
    {
        //4 Coordinates
        Cross_I,
        DiagonalCross_I,

        //8 Coordinates
        Cross_II,
        DiagonalCross_II,
        Square,
        Knight,
        FixedDistance_II,

        //12 Coordinates
        Cross_III,
        DiagonalCross_III,
        Cross_Alter,
        DiagonalCross_Alter,
        Cross_Knight,
        DiagonalCross_Knight,
        FixedDistance_III
    }

    public enum eElement
    {
        None, //Default
        Blue,
        Red,
        Green,
        Ocher,
        Purple,
        Yellow
    }

    public enum eEffectiveness
    {
        Effective,
        Neutral,
        Ineffective
    }

    public enum eTargetUnitClassification
    {
        Self,
        Ally,
        Enemy,
        SelfAndAlly,
        SelfAndEnemy,
        AllyAndEnemy,
        Any,
        AllyOnBoard,
        EnemyOnBoard,
        SelfAndAllyOnBoard,
        SelfAndEnemyOnBoard,
        AllyAndEnemyOnBoard,
        UnitOnBoard,
        AllyInRange,
        EnemyInRange,
        SelfAndAllyInRange,
        SelfAndEnemyInRange,
        AllyAndEnemyInRange,
        UnitInRange,
        AllyDefeated,
        EnemyDefeated,
        AllyAndEnemyDefeated
    }

    public enum eRarity
    {
        //The numbers express the max level of objects of the rarity
        Common = CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES,
        Uncommon = 2 * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES,
        Rare = 3 * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES,
        Epic = 4 * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES,
        Legendary = 5 * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES
    }

    public enum eUnitStatusType
    {
        Level,
        MaxHP,
        RemainingHP,
        PhyStr,
        PhyRes,
        MagStr,
        MagRes,
        Vitality
    }

    public enum eAttackClassification
    {
        Physic,
        Magic
    }

    public enum eWeaponClassification
    {
        Special,
        Ax,
        Blunt,
        Book,
        Bow,
        Glove,
        Gun,
        Katana,
        Knife,
        Mech,
        MusicalInstrument,
        Nuckle,
        Rapier,
        Scythe,
        Shield,
        Shoe,
        SniperRifle,
        Spear,
        Sword,
        Throwing,
        Wand,
        Whip
    }

    public enum eArmourClassification
    {
        ExtraLight,
        Light,
        Heavy
    }

    public enum eAccessoryClassification
    {
        Head,
        Neck,
        Body,
        Arm,
        Leg
    }

    public enum eTileType
    {
        Normal,
        Blue,
        Red,
        Green,
        Ocher,
        Purple,
        Yellow,
        Heal
    }

    public enum eGamePhase
    {
        BeginningOfMatch,
        BeginningOfTurn,
        DuringTurn,
        EndOfTurn
    }

    public enum eEventTriggerTiming
    {
        BeginningOfMatch, // eActivationTurnClassification will be ignored

        //All the values below might be used in conjunction with eActivationTurnClassification
        BeginningOfTurn,

        #region Can be triggered by actions executed by Units or any other event not produced by a Unit
        OnStatusEffectActivated,
        #endregion

        #region When a Unit is performing an action
        OnActionExecuted,
        OnMoved,
        OnAttackExecuted,
        OnActiveSkillExecuted,
        OnItemUsed,
        OnEffectSuccess,
        OnEffectFailure,
        #endregion

        #region When an action has been done against a Unit
        OnTargetedByAction,
        OnTargetedByAttack,
        OnTargetedBySkill,
        OnTargetedByItemSkill,
        OnHitByEffect,
        OnEvadedEffect,
        #endregion

        EndOfTurn
    }

    public enum eActivationTurnClassification
    {
        OnAnyTurn,
        OnMyTurn,
        OnOpponentTurn
    }

    public enum eStatusType //Used by BuffStatusEffect and DebuffStatusEffect
    {
        MaxHP,
        PhyStr,
        PhyRes,
        MagStr,
        MagRes,
        Vit,
        Pre,
        Eva,
        Cri,
        CriRes,
        DamageForce,
        FixedDamage, // Add/Subtract or Multiply/Divide value to final damage for the target of effect used by the StatusEffectHolder
        DamageResistance, // Add/Subtract or Multiply/Divide value to final damage when the StatusEffectHolder is being damaged
        HealForce,
        FixedHeal, // Add/Subtract or Multiply/Divide value to final healing amount for the target of effect used by the StatusEffectHolder
        FixedHeal_Self, // Add/Subtract or Multiply/Divide value to final healing amount when the StatusEffectHolder is being healed
        NumOfTargets,
    }

    public enum eModificationMethod
    {
        Add,
        Subtract,
        Overwrite
    }

    public enum eWeaponType
    {
        Ordinary,
        Levelable,
        Transformable,
        LevelableTransformable
    }

    public enum eStatusEffectClassification
    {
        Bind,
        Buff,
        Damage,
        DamageCut,
        Debuff,
        Dispel,
        Drain,
        Heal,
        Marker,
        StatusEffectResistance,
        TargetRangeMod,
    }

    public enum eCostType
    {
        Gem,
        Gold,
        Item
    }
}

namespace EEANWorks.Games.TBSG._01.Unity
{
    public enum eActionType
    {
        None,
        Attack,
        Move,
        Item,
        Skill,
        USkill
    }

    public enum eMaskType
    {
        NonSelectable,
        Selectable,
        Selected
    }
}
