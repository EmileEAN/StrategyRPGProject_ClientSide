using EEANWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01
{
    public static class Calculator
    {
        #region Public Methods
        public static int RequiredExperienceForLevelUp(Unit _unit) { return RequiredExperienceForLevelUp(AccumulatedLevel(_unit)); }
        public static int RequiredExperienceForLevelUp(int _accumulatedLevel)
        {
            int requiredExp = CoreValues.REQUIRED_EXPERIENCE_FOR_FIRST_LEVEL_UP;

            for (int lv = 1; lv < _accumulatedLevel; lv++)
            {
                int tmpExp = Convert.ToInt32(Math.Ceiling(requiredExp * CoreValues.LEVEL_EXPERIENCE_MULTIPLIER));
                if (tmpExp != requiredExp)
                    requiredExp = tmpExp;
                else
                    requiredExp++;
            }

            return requiredExp;
        }

        public static int MinimumAccumulatedExperienceRequired(IRarityMeasurable _rarityMeasurable, int _actualLevel) { return MinimumAccumulatedExperienceRequired(_rarityMeasurable.Rarity, _actualLevel); }
        public static int MinimumAccumulatedExperienceRequired(eRarity _rarity, int _actualLevel) { return MinimumAccumulatedExperienceRequired(AccumulatedLevel(_rarity, _actualLevel)); }
        public static int MinimumAccumulatedExperienceRequired(int _accumulatedLevel)
        {
            int minAccumulatedExpRequired = 0;

            for (int lv = 1; lv < _accumulatedLevel; lv++)
            {
                minAccumulatedExpRequired += RequiredExperienceForLevelUp(lv);
            }

            return minAccumulatedExpRequired;
        }

        //The amount of Exp gained after becoming the current level
        public static int LevelExperience(Unit _unit) { return LevelExperience(_unit); }
        public static int LevelExperience(int _accumulatedExp)
        {
            if (_accumulatedExp < 0)
                return -1;

            int level = Level(_accumulatedExp);

            return _accumulatedExp - MinimumAccumulatedExperienceRequired(level);
        }
        public static int MaxAccumulatedExperienceForRarity(eRarity _rarity) { return MinimumAccumulatedExperienceRequired(MaxAccumulatedLevelForRarity(_rarity)); }

        public static int Level(Unit _unit)
        {
            if (_unit == null)
                return -1;

            return Level(_unit);
        }
        public static int Level(Weapon _weapon)
        {
            if (_weapon == null)
                return -1;

            if (_weapon is LevelableWeapon)
                return Level((_weapon as LevelableWeapon).AccumulatedExperience);
            else if (_weapon is LevelableTransformableWeapon)
                return Level((_weapon as LevelableTransformableWeapon).AccumulatedExperience);

            return 0; // ...if not a levelable weapon
        }
        public static int Level(int _accumulatedExp)
        {
            int actualLevel = AccumulatedLevel(_accumulatedExp);

            int maxLevel_common = Convert.ToInt32(eRarity.Common);
            int maxLevel_uncommon = Convert.ToInt32(eRarity.Uncommon);
            int maxLevel_rare = Convert.ToInt32(eRarity.Rare);
            int maxLevel_epic = Convert.ToInt32(eRarity.Epic);

            if (actualLevel > maxLevel_common) //It must be eRarity.Uncommon or higher
                actualLevel -= maxLevel_common;

            if (actualLevel > maxLevel_uncommon) //It must be eRarity.Rare or higher
                actualLevel -= maxLevel_uncommon;

            if (actualLevel > maxLevel_rare) //It must be eRarity.Epic or higher
                actualLevel -= maxLevel_rare;

            if (actualLevel > maxLevel_epic) //It must be eRarity.Legendary
                actualLevel -= maxLevel_epic;

            return actualLevel;
        }
        private static int AccumulatedLevel(Unit _unit) { return AccumulatedLevel(_unit); }
        private static int AccumulatedLevel(int _accumulatedExp)
        {
            if (_accumulatedExp < 0)
                return -1;

            int accumulatedRequiredExpForLevel = 0;
            int accumulatedRequiredExpForNextLevel = 0;

            int maxAccumulatedLevel = MaxAccumulatedLevelForRarity(eRarity.Legendary);

            for (int lv = 1; lv < maxAccumulatedLevel; lv++)
            {
                accumulatedRequiredExpForLevel = accumulatedRequiredExpForNextLevel;
                accumulatedRequiredExpForNextLevel += RequiredExperienceForLevelUp(lv);

                if (_accumulatedExp >= accumulatedRequiredExpForLevel && _accumulatedExp < accumulatedRequiredExpForNextLevel)
                    return lv;
            }

            return maxAccumulatedLevel; // Must not exceed the maxAccumulatedLevel
        }
        private static int AccumulatedLevel(eRarity _rarity, int _actualLevel)
        {
            int maxLevel_common = Convert.ToInt32(eRarity.Common);
            int maxLevel_uncommon = Convert.ToInt32(eRarity.Uncommon);
            int maxLevel_rare = Convert.ToInt32(eRarity.Rare);
            int maxLevel_epic = Convert.ToInt32(eRarity.Epic);
            int maxLevel_legendary = Convert.ToInt32(eRarity.Legendary);

            switch (_rarity)
            {
                default: //eRarity.Common
                    {
                        if (_actualLevel > maxLevel_common)
                            return -1;
                        return _actualLevel;
                    }
                case eRarity.Uncommon:
                    {
                        if (_actualLevel > maxLevel_uncommon)
                            return -1;
                        return _actualLevel + maxLevel_common;
                    }
                case eRarity.Rare:
                    {
                        if (_actualLevel > maxLevel_rare)
                            return -1;
                        return _actualLevel + maxLevel_common + maxLevel_uncommon;
                    }
                case eRarity.Epic:
                    {
                        if (_actualLevel > maxLevel_epic)
                            return -1;
                        return _actualLevel + maxLevel_common + maxLevel_uncommon + maxLevel_rare;
                    }
                case eRarity.Legendary:
                    {
                        if (_actualLevel > maxLevel_legendary)
                            return -1;
                        return _actualLevel + maxLevel_common + maxLevel_uncommon + maxLevel_rare + maxLevel_epic;
                    }
            }
        }
        private static int MaxAccumulatedLevelForRarity(eRarity _rarity) { return AccumulatedLevel(_rarity, Convert.ToInt32(_rarity)); }

        public static int MaxLevelForRarity(eRarity _rarity) { return Convert.ToInt32(_rarity); }
        public static int MaxLevelForRarity(IRarityMeasurable _rarityMeasurableObject) { return MaxLevelForRarity(_rarityMeasurableObject.Rarity); }
        public static bool IsMaxLevel(Unit _unit)
        {
            int level = Level(_unit);

            return level == MaxLevelForRarity(_unit.BaseInfo);
        }
        public static bool IsMaxLevel(Weapon _weapon)
        {
            int level = Level(_weapon);

            if (_weapon is LevelableWeapon || _weapon is LevelableTransformableWeapon)
            {
                return level == MaxLevelForRarity(_weapon as IRarityMeasurable);
            }

            return true;
        }

        public static int MaxHPAtLevel(Unit _unit, int _actualLevel) { return MaxHP(_unit.BaseInfo, MinimumAccumulatedExperienceRequired(_unit.BaseInfo, _actualLevel)); }
        public static int MaxHP(Unit _unit) { return MaxHP(_unit.BaseInfo, _unit.AccumulatedExperience); }
        public static int MaxHP(UnitData _unitData, int _accumulatedExperience)
        {
            if (_unitData == null)
                return -1;

            decimal levelRate = 1.0m * AccumulatedLevel(_accumulatedExperience) / MaxAccumulatedLevelForRarity(_unitData.Rarity);

            return Convert.ToInt32(Math.Ceiling(_unitData.MaxLevel_HP * levelRate));
        }
        public static int MaxHP(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            return MaxHP_ActualDefinition(_unit, _system);
        }
        public static int MaxHP(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _targetArea, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skill == null
                || _effect == null
                || _targetArea == null)
            {
                return -1;
            }

            return MaxHP_ActualDefinition(_unit, _system, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }
        private static int MaxHP_ActualDefinition(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            decimal levelRate = 1.0m * AccumulatedLevel(_unit) / MaxAccumulatedLevelForRarity(_unit.BaseInfo.Rarity);

            decimal maxHP = _unit.BaseInfo.MaxLevel_HP * levelRate;

            ApplyBuffAndDebuffStatusEffects_Simple(ref maxHP, _system, _unit, eStatusType.MaxHP, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return Convert.ToInt32(Math.Ceiling(maxHP));
        }

        public static int PhysicalStrengthAtLevel(Unit _unit, int _actualLevel) { return PhysicalStrength(_unit.BaseInfo, MinimumAccumulatedExperienceRequired(_unit.BaseInfo, _actualLevel)); }
        public static int PhysicalStrength(Unit _unit) { return PhysicalStrength(_unit.BaseInfo, _unit.AccumulatedExperience); }
        public static int PhysicalStrength(UnitData _unitData, int _accumulatedExperience)
        {
            if (_unitData == null)
                return -1;

            decimal levelRate = 1.0m * AccumulatedLevel(_accumulatedExperience) / MaxAccumulatedLevelForRarity(_unitData.Rarity);

            return Convert.ToInt32(Math.Round(_unitData.MaxLevel_PhysicalStrength * levelRate, MidpointRounding.AwayFromZero));
        }
        public static int PhysicalStrength(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            return PhysicalStrength_ActualDefinition(_unit, _system);
        }
        public static int PhysicalStrength(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skill == null
                || _effect == null
                || _targetArea == null)
            {
                return -1;
            }

            return PhysicalStrength_ActualDefinition(_unit, _system, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }
        private static int PhysicalStrength_ActualDefinition(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            decimal levelRate = 1.0m * AccumulatedLevel(_unit) / MaxAccumulatedLevelForRarity(_unit.BaseInfo.Rarity);

            decimal phyStr = _unit.BaseInfo.MaxLevel_PhysicalStrength * levelRate;

            ApplyBuffAndDebuffStatusEffects_Simple(ref phyStr, _system, _unit, eStatusType.PhyStr, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return Convert.ToInt32(Math.Round(phyStr, MidpointRounding.AwayFromZero));
        }

        public static int PhysicalResistanceAtLevel(Unit _unit, int _actualLevel) { return PhysicalResistance(_unit.BaseInfo, MinimumAccumulatedExperienceRequired(_unit.BaseInfo, _actualLevel)); }
        public static int PhysicalResistance(Unit _unit) { return PhysicalResistance(_unit.BaseInfo, _unit.AccumulatedExperience); }
        public static int PhysicalResistance(UnitData _unitData, int _accumulatedExperience)
        {
            if (_unitData == null)
                return -1;

            decimal levelRate = 1.0m * AccumulatedLevel(_accumulatedExperience) / MaxAccumulatedLevelForRarity(_unitData.Rarity);

            return Convert.ToInt32(Math.Round(_unitData.MaxLevel_PhysicalResistance * levelRate, MidpointRounding.AwayFromZero));

        }
        public static int PhysicalResistance(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            return PhysicalResistance_ActualDefinition(_unit, _system);
        }
        public static int PhysicalResistance(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skill == null
                || _effect == null
                || _targetArea == null)
            {
                return -1;
            }

            return PhysicalResistance_ActualDefinition(_unit, _system, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }
        private static int PhysicalResistance_ActualDefinition(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            decimal levelRate = 1.0m * AccumulatedLevel(_unit) / MaxAccumulatedLevelForRarity(_unit.BaseInfo.Rarity);

            decimal phyRes = _unit.BaseInfo.MaxLevel_PhysicalResistance * levelRate;

            ApplyBuffAndDebuffStatusEffects_Simple(ref phyRes, _system, _unit, eStatusType.PhyRes, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return Convert.ToInt32(Math.Round(phyRes, MidpointRounding.AwayFromZero));
        }

        public static int MagicalStrengthAtLevel(Unit _unit, int _actualLevel) { return MagicalStrength(_unit.BaseInfo, MinimumAccumulatedExperienceRequired(_unit.BaseInfo, _actualLevel)); }
        public static int MagicalStrength(Unit _unit) { return MagicalStrength(_unit.BaseInfo, _unit.AccumulatedExperience); }
        public static int MagicalStrength(UnitData _unitData, int _accumulatedExperience)
        {
            if (_unitData == null)
                return -1;

            decimal levelRate = 1.0m * AccumulatedLevel(_accumulatedExperience) / MaxAccumulatedLevelForRarity(_unitData.Rarity);

            return Convert.ToInt32(Math.Round(_unitData.MaxLevel_MagicalStrength * levelRate, MidpointRounding.AwayFromZero));
        }
        public static int MagicalStrength(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            return MagicalStrength_ActualDefinition(_unit, _system);
        }
        public static int MagicalStrength(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skill == null
                || _effect == null
                || _targetArea == null)
            {
                return -1;
            }

            return MagicalStrength_ActualDefinition(_unit, _system, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }
        private static int MagicalStrength_ActualDefinition(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            decimal levelRate = 1.0m * AccumulatedLevel(_unit) / MaxAccumulatedLevelForRarity(_unit.BaseInfo.Rarity);

            decimal magStr = _unit.BaseInfo.MaxLevel_MagicalStrength * levelRate;

            ApplyBuffAndDebuffStatusEffects_Simple(ref magStr, _system, _unit, eStatusType.MagStr, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return Convert.ToInt32(Math.Round(magStr, MidpointRounding.AwayFromZero));
        }

        public static int MagicalResistanceAtLevel(Unit _unit, int _actualLevel) { return MagicalResistance(_unit.BaseInfo, MinimumAccumulatedExperienceRequired(_unit.BaseInfo, _actualLevel)); }
        public static int MagicalResistance(Unit _unit) { return MagicalResistance(_unit.BaseInfo, _unit.AccumulatedExperience); }
        public static int MagicalResistance(UnitData _unitData, int _accumulatedExperience)
        {
            if (_unitData == null)
                return -1;

            decimal levelRate = 1.0m * AccumulatedLevel(_accumulatedExperience) / MaxAccumulatedLevelForRarity(_unitData.Rarity);

            return Convert.ToInt32(Math.Round(_unitData.MaxLevel_MagicalResistance * levelRate, MidpointRounding.AwayFromZero));
        }
        public static int MagicalResistance(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            return MagicalResistance_ActualDefinition(_unit, _system);
        }
        public static int MagicalResistance(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skill == null
                || _effect == null
                || _targetArea == null)
            {
                return -1;
            }

            return MagicalResistance_ActualDefinition(_unit, _system, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }
        private static int MagicalResistance_ActualDefinition(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            decimal levelRate = 1.0m * AccumulatedLevel(_unit) / MaxAccumulatedLevelForRarity(_unit.BaseInfo.Rarity);

            decimal magRes = _unit.BaseInfo.MaxLevel_MagicalResistance * levelRate;

            ApplyBuffAndDebuffStatusEffects_Simple(ref magRes, _system, _unit, eStatusType.MagRes, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return Convert.ToInt32(Math.Round(magRes, MidpointRounding.AwayFromZero));
        }

        public static int VitalityAtLevel(Unit _unit, int _actualLevel) { return Vitality(_unit.BaseInfo, MinimumAccumulatedExperienceRequired(_unit.BaseInfo, _actualLevel)); }
        public static int Vitality(Unit _unit) { return Vitality(_unit.BaseInfo, _unit.AccumulatedExperience); }
        public static int Vitality(UnitData _unitData, int _accumulatedExperience)
        {
            if (_unitData == null)
                return -1;

            decimal levelRate = 1.0m * AccumulatedLevel(_accumulatedExperience) / MaxAccumulatedLevelForRarity(_unitData.Rarity);

            return Convert.ToInt32(Math.Round(_unitData.MaxLevel_Vitality * levelRate, MidpointRounding.AwayFromZero));
        }
        public static int Vitality(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            return Vitality_ActualDefinition(_unit, _system);
        }
        public static int Vitality(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skill == null
                || _effect == null
                || _targetArea == null)
            {
                return -1;
            }

            return Vitality_ActualDefinition(_unit, _system, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }
        private static int Vitality_ActualDefinition(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            decimal levelRate = 1.0m * AccumulatedLevel(_unit) / MaxAccumulatedLevelForRarity(_unit.BaseInfo.Rarity);

            decimal vit = _unit.BaseInfo.MaxLevel_Vitality * levelRate;

            ApplyBuffAndDebuffStatusEffects_Simple(ref vit, _system, _unit, eStatusType.Vit, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return Convert.ToInt32(Math.Round(vit, MidpointRounding.AwayFromZero));
        }

        public static decimal Precision(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            decimal pre = 1.0m;

            ApplyBuffAndDebuffStatusEffects_Simple(ref pre, _system, _unit, eStatusType.Pre);

            return pre;
        }
        public static decimal Precision(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _targetArea = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skill == null
                || _effect == null
                || _targetArea == null)
            {
                return -1;
            }

            decimal pre = 1.0m;

            ApplyBuffAndDebuffStatusEffects_Simple(ref pre, _system, _unit, eStatusType.Pre, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return pre;
        }

        public static decimal Evasion(UnitInstance _unit, BattleSystemCore _system)
        {
            if (_unit == null || _system == null)
                return -1;

            decimal eva = 1.0m;

            ApplyBuffAndDebuffStatusEffects_Simple(ref eva, _system, _unit, eStatusType.Eva);

            return eva;
        }
        public static decimal Evasion(UnitInstance _unit, BattleSystemCore _system, UnitInstance _skillUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _targetArea, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_unit == null
                || _system == null
                || _skillUser == null
                || _skill == null
                || _effect == null
                || _targetArea == null
                || _targets == null)
            {
                return -1;
            }

            decimal eva = 1.0m;

            ApplyBuffAndDebuffStatusEffects_Simple(ref eva, _system, _unit, eStatusType.Eva, _skillUser, _skill, _effect, _targetArea, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            bool isUnitEqualToTarget = false;
            if (_skill.BaseInfo.Effect is UnitTargetingEffect)
            {
                if (_target != null && _unit == _target)
                    isUnitEqualToTarget = true;
            }

            if (isUnitEqualToTarget)
            {
                //If _effect is positive to _unit, change the value of eva to its reciprocal. That is, increase _unit's probability to evade the _effect if eva is low and be hit by it if eva is high. Use it as dexterity in this particular case.
                if (IsEffectPositiveForTarget(_unit, _effect, _system))
                    eva = eva.Reciprocal();
            }

            return eva;
        }

        private static bool IsEffectPositiveForTarget(UnitInstance _target, Effect _effect, BattleSystemCore _system)
        {
            bool isEffectPositive = false;

            //Set of all possible positive effects
            if (_effect is HealEffect)
                isEffectPositive = true;
            else if (_effect is StatusEffectAttachmentEffect)
            {
                var detailedEffect = _effect as StatusEffectAttachmentEffect;

                if (detailedEffect.DataOfStatusEffectToAttach is BuffStatusEffectData
                    || detailedEffect.DataOfStatusEffectToAttach is HealStatusEffectData)
                {
                    isEffectPositive = true;
                }
                else if (detailedEffect.DataOfStatusEffectToAttach is TargetRangeModStatusEffectData)
                {
                    var statusEffectData = detailedEffect.DataOfStatusEffectToAttach as TargetRangeModStatusEffectData;

                    if (statusEffectData.ModificationMethod == eModificationMethod.Add // The method of target range modification is addition
                        || (statusEffectData.ModificationMethod == eModificationMethod.Overwrite && RelativeTargetArea(_target, statusEffectData.IsMovementRangeClassification, _system).Count > TargetArea.GetTargetArea(statusEffectData.IsMovementRangeClassification ? _target.BaseInfo.MovementRangeClassification : _target.BaseInfo.NonMovementActionRangeClassification).Count))// OR the method is Overwrite and the total number of targetable coords in the effect's target area is greater than the current target area
                        isEffectPositive = true;
                }
            }
            //End of set of possible postive effects

            return isEffectPositive;
        }

        public static int MaxNumOfTargets(UnitInstance _skillUser, ActiveSkill _skill, List<_2DCoord> _targetArea, BattleSystemCore _system)
        {
            decimal maxNumOfTargets = _skill.BaseInfo.MaxNumberOfTargets.ToValue<decimal>(_system, null, null, _skillUser, _skill, null, _targetArea);

            //if (_skill.BaseInfo.Effect is UnitSwapEffect || _skill.BaseInfo.Effect is TileSwapEffect)
                //maxNumOfTargets = 2m;

            ApplyBuffAndDebuffStatusEffects_Simple(ref maxNumOfTargets, _system, _skillUser, eStatusType.NumOfTargets, _skillUser, _skill, null, _targetArea);

            return Convert.ToInt32(Math.Floor(maxNumOfTargets));
        }

        public static List<_2DCoord> RelativeTargetArea(UnitInstance _unit, bool _isMovementRangeClassification, BattleSystemCore _system, ActiveSkill _skill = null)
        {
            List<_2DCoord> relativeTargetArea;
            if (_isMovementRangeClassification)
                relativeTargetArea = TargetArea.GetTargetArea(_unit.BaseInfo.MovementRangeClassification);
            else
                relativeTargetArea = TargetArea.GetTargetArea(_unit.BaseInfo.NonMovementActionRangeClassification);

            if (_skill == null)
                ApplyTargetRangeModStatusEffects(ref relativeTargetArea, _unit, _isMovementRangeClassification, _system);
            else
                ApplyTargetRangeModStatusEffects(ref relativeTargetArea, _unit, _isMovementRangeClassification, _system, _skill, _skill.BaseInfo.Effect); // _skill.Effect will actually not be used.

            return relativeTargetArea;
        }

        // Used to obtain actual damage
        public static int Damage(BattleSystemCore _system, UnitInstance _attacker, _2DCoord _attackerCoord, ActiveSkill _skill, DamageEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, out bool _isCritical, out eEffectiveness _effectiveness, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null, int _diffusionDistance = 0)
        {
            _isCritical = IsCritical(_attacker, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _system);

            decimal damage = Damage_ActualDefinition(_system, _attacker, _attackerCoord, _skill, _effect, _effectRange, _targets, _target, _isCritical, out _effectiveness, _secondaryTargetsForComplexTargetSelectionEffect, _diffusionDistance);

            return Convert.ToInt32(Math.Floor(damage));
        }
        private static decimal Damage_ActualDefinition(BattleSystemCore _system, UnitInstance _attacker, _2DCoord _attackerCoord, ActiveSkill _skill, DamageEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, bool _isCritical, out eEffectiveness _effectiveness, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null, int _diffusionDistance = 0)
        {
            if (_system == null
                || _attacker == null
                || _skill == null
                || _effect == null
                || _effectRange == null
                || _target == null
                || _attackerCoord == null)
            {
                _isCritical = false;
                _effectiveness = eEffectiveness.Neutral;
                return -1;
            }

            decimal damage = Convert.ToDecimal(CoreValues.DAMAGE_BASE_VALUE);

            decimal strength = 0;
            decimal resistance = 0;

            List<object> targets = _targets.Cast<object>().ToList();
            List<object> secondaryTargetsForComplexTargetSelectionEffect = _secondaryTargetsForComplexTargetSelectionEffect?.Cast<object>().ToList();

            if (_effect.AttackClassification == eAttackClassification.Physic)
            {
                strength = PhysicalStrength(_attacker, _system, _attacker, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
                resistance = PhysicalResistance(_target, _system, _attacker, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
            }
            else
            {
                strength = MagicalStrength(_attacker, _system, _attacker, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
                resistance = MagicalResistance(_target, _system, _attacker, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
            }

            damage *= (Convert.ToDecimal(strength) / resistance) * (Convert.ToDecimal(strength) / CoreValues.MAX_BASE_STR_AND_RES_VALUE); //use decimal to avoid unexpected round ups

            if (!_effect.IsFixedValue)
                damage *= CorrectionRate_Force(_system, _attacker, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            damage *= DamageCorrectionRate_Element(_attacker, _effect.Element, _target, out _effectiveness);

            //Based on relation ship between tile type and unit/effect
            damage *= CorrectionRate_TileType(_effect, _system.Field.Board.Sockets[_attackerCoord.X, _attackerCoord.Y].TileType);

            if (_isCritical)
                damage *= CoreValues.MULTIPLIER_FOR_CRITICALHIT;

            ApplyBuffAndDebuffStatusEffects_Compound(ref damage, _system, eStatusType.FixedDamage, _attacker, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect); // Applied the buff and debuff effects of effect user and target that are related to damage

            if (_effect.IsFixedValue)
                damage = _effect.Value.ToValue<decimal>(_system, null, null, _attacker, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

            if (_diffusionDistance > 0)
                damage *= (0.5m).Pow(_diffusionDistance);

            if (damage < 0) // damage must not be a negative value
                damage = 0;

            return damage;
        }
        /// <summary>
        /// Used to simulate possible non-critical damage
        /// </summary>
        public static decimal PossibleNonCriticalDamage(BattleSystemCore _system, UnitInstance _attacker, _2DCoord _attackerCoord, ActiveSkill _skill, DamageEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, out eEffectiveness _effectiveness, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null, int _diffusionDistance = 0)
        {
            return Damage_ActualDefinition(_system, _attacker, _attackerCoord, _skill, _effect, _effectRange, _targets, _target, false, out _effectiveness, _secondaryTargetsForComplexTargetSelectionEffect, _diffusionDistance);
        }

        public static int HealValue(BattleSystemCore _system, UnitInstance _effectUser, _2DCoord _effectUserCoord, ActiveSkill _skill, HealEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, out bool _isCritical, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null, int _diffusionDistance = 0)
        {
            if (_system == null
                || _effectUser == null
                || _skill == null
                || _effect == null
                || _effectRange == null
                || _targets == null
                || _target == null
                || _effectUserCoord == null)
            {
                _isCritical = false;
                return -1;
            }

            decimal restoringHp = 0.05m * (Vitality(_effectUser) + 1) * (Vitality(_target) + 1);

            List<object> targets = _targets.Cast<object>().ToList();
            List<object> secondaryTargetsForComplexTargetSelectionEffect = _secondaryTargetsForComplexTargetSelectionEffect?.Cast<object>().ToList();

            if (!_effect.IsFixedValue)
                restoringHp *= CorrectionRate_Force(_system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            //Based on whether the tile is heal tile
            restoringHp *= (_system.Field.Board.Sockets[_effectUserCoord.X, _effectUserCoord.Y].TileType == eTileType.Heal) ? CoreValues.MULTIPLIER_FOR_TILETYPEMATCH : 1.0m;

            if (IsCritical(_effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _system))
            {
                restoringHp *= CoreValues.MULTIPLIER_FOR_CRITICALHIT;
                _isCritical = true;
            }
            else
                _isCritical = false;

            ApplyBuffAndDebuffStatusEffects_Compound(ref restoringHp, _system, eStatusType.FixedHeal, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect); // Applied the buff and debuff effects of effect user and target that are related to healing

            if (_effect.IsFixedValue)
                restoringHp = _effect.Value.ToValue<decimal>(_system, null, null, _effectUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

            if (_diffusionDistance > 0)
                restoringHp *= (0.5m).Pow(_diffusionDistance);

            if (restoringHp < 0) // restoringHp must not be a negative value
                restoringHp = 0;

            return Convert.ToInt32(Math.Floor(restoringHp));
        }

        //private static void ApplyBuffAndDebuffStatusEffects(ref decimal _value, BattleSystemCore _system, UnitInstance _unit, eStatusType _statusType, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        //{
        //    if (_system == null
        //        || _unit == null)
        //    {
        //        return;
        //    }

        //    ApplyBuffAndDebuffStatusEffects_Compound(ref _value, _system, _unit, _statusType, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        //}

        private static void ApplyBuffAndDebuffStatusEffects_Simple(ref decimal _value, BattleSystemCore _system, UnitInstance _unit, eStatusType _statusType)
        {
            if (_system == null || _unit == null)
                return;

            List<BuffStatusEffect> buffStatusEffects = _unit.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();
            List<DebuffStatusEffect> debuffStatusEffects = _unit.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();

            AddPassiveSkillBuffAndDebuffStatusEffects(buffStatusEffects, debuffStatusEffects, _system, _unit, _statusType);
            AddEquipmentBuffAndDebuffStatusEffects(buffStatusEffects, debuffStatusEffects, _system, _unit, _statusType);

            ApplyMultiplicativeBuffStatusEffects(ref _value, _unit, buffStatusEffects, _system);
            ApplyMultiplicativeDebuffStatusEffects(ref _value, _unit, debuffStatusEffects, _system);

            ApplySummativeBuffStatusEffects(ref _value, _unit, buffStatusEffects, _system);
            ApplySummativeDebuffStatusEffects(ref _value, _unit, debuffStatusEffects, _system);
        }
        public static void ApplyBuffAndDebuffStatusEffects_Simple(ref decimal _value, BattleSystemCore _system, UnitInstance _unit, eStatusType _statusType, UnitInstance _skillUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_system == null
                || _unit == null
                || _skillUser == null
                || _effect == null)
            {
                return;
            }

            List<BuffStatusEffect> buffStatusEffects = new List<BuffStatusEffect>();
            List<DebuffStatusEffect> debuffStatusEffects = new List<DebuffStatusEffect>();

            // Get buff and debuff status effects
            buffStatusEffects = _unit.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();
            debuffStatusEffects = _unit.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();

            // Get status effects from skills if _skillUser
            if (_unit == _skillUser)
            {
                foreach (BuffStatusEffectData data in _skill.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _unit)))
                {
                    buffStatusEffects.Add(new BuffStatusEffect(data, null, _skill.Level));
                }

                foreach (DebuffStatusEffectData data in _skill.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _unit)))
                {
                    debuffStatusEffects.Add(new DebuffStatusEffect(data, null, _skill.Level));
                }
            }

            // Get status effects from passive skills and equipments
            AddPassiveSkillBuffAndDebuffStatusEffects(buffStatusEffects, debuffStatusEffects, _system, _unit, _statusType);
            AddEquipmentBuffAndDebuffStatusEffects(buffStatusEffects, debuffStatusEffects, _system, _unit, _statusType);

            // Apply multiplicative status effects
            ApplyMultiplicativeBuffStatusEffects(ref _value, _unit, buffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
            ApplyMultiplicativeDebuffStatusEffects(ref _value, _unit, debuffStatusEffects, _system, _unit, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            // Apply summative status effects
            ApplySummativeBuffStatusEffects(ref _value, _unit, buffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
            ApplySummativeDebuffStatusEffects(ref _value, _unit, debuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }
        //private static void ApplyBuffAndDebuffStatusEffects_Compound(ref decimal _value, BattleSystemCore _system, UnitInstance _unit, eStatusType _statusType, UnitInstance _skillUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null)
        //{
        //    if (_system == null
        //        || _unit == null
        //        || _skillUser == null
        //        || _effect == null
        //        || _effectRange == null
        //        || _targets == null
        //        || _target == null)
        //    {
        //        return;
        //    }

        //    if (_unit != _skillUser)
        //    {
        //        if (!(_skill.BaseInfo.Effect is UnitTargetingEffect))
        //            return;

        //        if (_unit != _target)
        //        {
        //            if (_secondaryTargetsForComplexTargetSelectionEffect == null || _unit != _secondaryTargetsForComplexTargetSelectionEffect)
        //                return;

        //            if (!(_skill.BaseInfo.Effect is IComplexTargetSelectionEffect) || !_applyEffectToSecondaryTargets)
        //                return;
        //        }
        //        else if (_applyEffectToSecondaryTargets)
        //            return;
        //    }
        //    else
        //    {
        //        if (_skill.BaseInfo.Effect is UnitTargetingEffect)
        //        {
        //            if (_secondaryTargetsForComplexTargetSelectionEffect == null)
        //                return;

        //            if (!_applyEffectToSecondaryTargets)
        //                return;

        //            if (_secondaryTargetsForComplexTargetSelectionEffect == null && _applyEffectToSecondaryTargets)
        //                return;
        //        }
        //    }

        //    //If it did not return, _unit is one of the entities involved in the execution of _skill and _effect.

        //    //Furthermore, if _unit is the _skillUser, either _skill.BaseInfo.Effect does not target objects of UnitInstance OR there exists an appropriate target UnitInstance.

        //    List<object> targets = _targets.Cast<object>().ToList();

        //    List<BuffStatusEffect> skillUserBuffStatusEffects = new List<BuffStatusEffect>();
        //    List<DebuffStatusEffect> skillUserDebuffStatusEffects = new List<DebuffStatusEffect>();

        //    List<BuffStatusEffect> targetBuffStatusEffects = new List<BuffStatusEffect>();
        //    List<DebuffStatusEffect> targetDebuffStatusEffects = new List<DebuffStatusEffect>();

        //    // Get buff and debuff status effects for _skillUser
        //    skillUserBuffStatusEffects = _skillUser.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();
        //    skillUserDebuffStatusEffects = _skillUser.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();

        //    foreach (BuffStatusEffectData data in _skill.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _skillUser)))
        //    {
        //        skillUserBuffStatusEffects.Add(new BuffStatusEffect(data, null, _skill.Level));
        //    }

        //    foreach (DebuffStatusEffectData data in _skill.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _skillUser)))
        //    {
        //        skillUserDebuffStatusEffects.Add(new DebuffStatusEffect(data, null, _skill.Level));
        //    }

        //    AddPassiveSkillBuffAndDebuffStatusEffects(skillUserBuffStatusEffects, skillUserDebuffStatusEffects, _system, _skillUser, _statusType);
        //    AddEquipmentBuffAndDebuffStatusEffects(skillUserBuffStatusEffects, skillUserDebuffStatusEffects, _system, _skillUser, _statusType);

        //    // Apply multiplicative status effects for _skillUser
        //    ApplyMultiplicativeBuffStatusEffects(ref _value, _skillUser, skillUserBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        //    ApplyMultiplicativeDebuffStatusEffects(ref _value, _skillUser, skillUserDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

        //    // Get buff and debuff status effects for target (if available).
        //    UnitInstance target = null;
        //    if (_skill.BaseInfo.Effect is UnitTargetingEffect)
        //    {
        //        target = (_applyEffectToSecondaryTargets) ? _secondaryTargetsForComplexTargetSelectionEffect as UnitInstance : _target as UnitInstance;

        //        targetBuffStatusEffects = target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();
        //        targetDebuffStatusEffects = target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();

        //        if (_statusType == eStatusType.FixedDamage) // It means that damage dealing effect called this function
        //        {
        //            targetBuffStatusEffects = target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.DamageResistance).ToList();
        //            targetDebuffStatusEffects = target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.DamageResistance).ToList();
        //            AddPassiveSkillBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, target, eStatusType.DamageResistance);
        //            AddEquipmentBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, target, eStatusType.DamageResistance);
        //        }
        //        else if (_statusType == eStatusType.FixedHeal) // It means that healin effect called this function
        //        {
        //            targetBuffStatusEffects = target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.FixedHeal_Self).ToList();
        //            targetDebuffStatusEffects = target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.FixedHeal_Self).ToList();
        //            AddPassiveSkillBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, target, eStatusType.FixedHeal_Self);
        //            AddEquipmentBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, target, eStatusType.FixedHeal_Self);
        //        }
        //        else if (_statusType == eStatusType.Cri)
        //        {
        //            targetBuffStatusEffects = target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.CriRes).ToList();
        //            targetDebuffStatusEffects = target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.CriRes).ToList();
        //            AddPassiveSkillBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, target, eStatusType.CriRes);
        //            AddEquipmentBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, target, eStatusType.CriRes);
        //        }

        //        // Apply multiplicative status effects for target
        //        ApplyMultiplicativeBuffStatusEffects(ref _value, target, targetBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        //        ApplyMultiplicativeDebuffStatusEffects(ref _value, target, targetDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        //    }

        //    // Apply summative status effects for _skillUser
        //    ApplySummativeBuffStatusEffects(ref _value, _skillUser, skillUserBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        //    ApplySummativeDebuffStatusEffects(ref _value, _skillUser, skillUserDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

        //    if (target != null)
        //    {
        //        // Apply summative status effects for target
        //        ApplySummativeBuffStatusEffects(ref _value, target, targetBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        //        ApplySummativeDebuffStatusEffects(ref _value, target, targetDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        //    }
        //}
        private static void ApplyBuffAndDebuffStatusEffects_Compound(ref decimal _value, BattleSystemCore _system, eStatusType _statusType, UnitInstance _skillUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_system == null
                || _skillUser == null
                || _effect == null
                || _effectRange == null
                || _targets == null
                || _target == null)
            {
                return;
            }

            if (_statusType != eStatusType.FixedDamage && _statusType != eStatusType.FixedHeal && _statusType != eStatusType.Cri)
                return;

            if (!(_skill.BaseInfo.Effect is UnitTargetingEffect))
                return;

            List<object> targets = _targets.Cast<object>().ToList();
            List<object> secondaryTargetsForComplexTargetSelectionEffect = _secondaryTargetsForComplexTargetSelectionEffect?.Cast<object>().ToList();

            List<BuffStatusEffect> skillUserBuffStatusEffects = new List<BuffStatusEffect>();
            List<DebuffStatusEffect> skillUserDebuffStatusEffects = new List<DebuffStatusEffect>();

            List<BuffStatusEffect> targetBuffStatusEffects = new List<BuffStatusEffect>();
            List<DebuffStatusEffect> targetDebuffStatusEffects = new List<DebuffStatusEffect>();

            // Get buff and debuff status effects for _skillUser
            skillUserBuffStatusEffects = _skillUser.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();
            skillUserDebuffStatusEffects = _skillUser.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();

            foreach (BuffStatusEffectData data in _skill.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _skillUser)))
            {
                skillUserBuffStatusEffects.Add(new BuffStatusEffect(data, null, _skill.Level));
            }

            foreach (DebuffStatusEffectData data in _skill.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _skillUser)))
            {
                skillUserDebuffStatusEffects.Add(new DebuffStatusEffect(data, null, _skill.Level));
            }

            AddPassiveSkillBuffAndDebuffStatusEffects(skillUserBuffStatusEffects, skillUserDebuffStatusEffects, _system, _skillUser, _statusType);
            AddEquipmentBuffAndDebuffStatusEffects(skillUserBuffStatusEffects, skillUserDebuffStatusEffects, _system, _skillUser, _statusType);

            // Apply multiplicative status effects for _skillUser
            ApplyMultiplicativeBuffStatusEffects(ref _value, _skillUser, skillUserBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
            ApplyMultiplicativeDebuffStatusEffects(ref _value, _skillUser, skillUserDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

            targetBuffStatusEffects = _target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();
            targetDebuffStatusEffects = _target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == _statusType).ToList();

            if (_statusType == eStatusType.FixedDamage) // It means that damage dealing effect called this function
            {
                targetBuffStatusEffects = _target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.DamageResistance).ToList();
                targetDebuffStatusEffects = _target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.DamageResistance).ToList();
                AddPassiveSkillBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, _target, eStatusType.DamageResistance);
                AddEquipmentBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, _target, eStatusType.DamageResistance);
            }
            else if (_statusType == eStatusType.FixedHeal) // It means that healin effect called this function
            {
                targetBuffStatusEffects = _target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.FixedHeal_Self).ToList();
                targetDebuffStatusEffects = _target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.FixedHeal_Self).ToList();
                AddPassiveSkillBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, _target, eStatusType.FixedHeal_Self);
                AddEquipmentBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, _target, eStatusType.FixedHeal_Self);
            }
            else if (_statusType == eStatusType.Cri)
            {
                targetBuffStatusEffects = _target.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.CriRes).ToList();
                targetDebuffStatusEffects = _target.StatusEffects.OfType<DebuffStatusEffect>().Where(x => x.TargetStatusType == eStatusType.CriRes).ToList();
                AddPassiveSkillBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, _target, eStatusType.CriRes);
                AddEquipmentBuffAndDebuffStatusEffects(targetBuffStatusEffects, targetDebuffStatusEffects, _system, _target, eStatusType.CriRes);
            }

            // Apply multiplicative status effects for target
            ApplyMultiplicativeBuffStatusEffects(ref _value, _target, targetBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
            ApplyMultiplicativeDebuffStatusEffects(ref _value, _target, targetDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

            // Apply summative status effects for _skillUser
            ApplySummativeBuffStatusEffects(ref _value, _skillUser, skillUserBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
            ApplySummativeDebuffStatusEffects(ref _value, _skillUser, skillUserDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

            // Apply summative status effects for target
            ApplySummativeBuffStatusEffects(ref _value, _target, targetBuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
            ApplySummativeDebuffStatusEffects(ref _value, _target, targetDebuffStatusEffects, _system, _skillUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);
        }

        private static void AddPassiveSkillBuffAndDebuffStatusEffects(List<BuffStatusEffect> _buffStatusEffects, List<DebuffStatusEffect> _debuffStatusEffects, BattleSystemCore _system, UnitInstance _unit, eStatusType _statusType)
        {
            if (_buffStatusEffects == null
                || _debuffStatusEffects == null
                || _system == null
                || _unit == null)
            {
                return;
            }

            foreach (PassiveSkill passiveSkill in _unit.Skills.OfType<PassiveSkill>())
            {
                foreach (BuffStatusEffectData data in passiveSkill.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _unit)))
                {
                    _buffStatusEffects.Add(new BuffStatusEffect(data, null, passiveSkill.Level));
                }
                foreach (DebuffStatusEffectData data in passiveSkill.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _unit)))
                {
                    _debuffStatusEffects.Add(new DebuffStatusEffect(data, null, passiveSkill.Level));
                }
            }

            if (_unit.InheritedSkill is PassiveSkill)
            {
                foreach (BuffStatusEffectData data in _unit.InheritedSkill.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _unit)))
                {
                    _buffStatusEffects.Add(new BuffStatusEffect(data, null, _unit.InheritedSkill.Level));
                }
                foreach (DebuffStatusEffectData data in _unit.InheritedSkill.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _unit)))
                {
                    _debuffStatusEffects.Add(new DebuffStatusEffect(data, null, _unit.InheritedSkill.Level));
                }
            }
        }

        private static void AddEquipmentBuffAndDebuffStatusEffects(List<BuffStatusEffect> _buffStatusEffects, List<DebuffStatusEffect> _debuffStatusEffects, BattleSystemCore _system, UnitInstance _equipmentOwner, eStatusType _statusType)
        {
            if (_buffStatusEffects == null
                || _debuffStatusEffects == null
                || _system == null
                || _equipmentOwner == null)
            {
                return;
            }

            if (_equipmentOwner.MainWeapon != null)
            {
                int mainWeaponLevel = Level(_equipmentOwner.MainWeapon);
                foreach (BuffStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _buffStatusEffects.Add(new BuffStatusEffect(data, null, 0, mainWeaponLevel));
                }
                foreach (DebuffStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _debuffStatusEffects.Add(new DebuffStatusEffect(data, null, 0, mainWeaponLevel));
                }

                if (_equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill != null && _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill is PassiveSkill)
                {
                    foreach (BuffStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                    {
                        _buffStatusEffects.Add(new BuffStatusEffect(data, null, _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.Level, mainWeaponLevel));
                    }
                    foreach (DebuffStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                    {
                        _debuffStatusEffects.Add(new DebuffStatusEffect(data, null, _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.Level, mainWeaponLevel));
                    }
                }
            }

            if (_equipmentOwner.SubWeapon != null)
            {
                int subWeaponLevel = Level(_equipmentOwner.SubWeapon);
                foreach (BuffStatusEffectData data in _equipmentOwner.SubWeapon.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _buffStatusEffects.Add(new BuffStatusEffect(data, null, 0, subWeaponLevel));
                }
                foreach (DebuffStatusEffectData data in _equipmentOwner.SubWeapon.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _debuffStatusEffects.Add(new DebuffStatusEffect(data, null, 0, subWeaponLevel));
                }
            }

            if (_equipmentOwner.Armour != null)
            {
                foreach (BuffStatusEffectData data in _equipmentOwner.Armour.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _buffStatusEffects.Add(new BuffStatusEffect(data));
                }
                foreach (DebuffStatusEffectData data in _equipmentOwner.Armour.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _debuffStatusEffects.Add(new DebuffStatusEffect(data));
                }
            }

            if (_equipmentOwner.Accessory != null)
            {
                foreach (BuffStatusEffectData data in _equipmentOwner.Accessory.BaseInfo.StatusEffectsData.OfType<BuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _buffStatusEffects.Add(new BuffStatusEffect(data));
                }
                foreach (DebuffStatusEffectData data in _equipmentOwner.Accessory.BaseInfo.StatusEffectsData.OfType<DebuffStatusEffectData>().Where(x => x.TargetStatusType == _statusType && _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner)))
                {
                    _debuffStatusEffects.Add(new DebuffStatusEffect(data));
                }
            }
        }

        private static void ApplyMultiplicativeBuffStatusEffects(ref decimal _value, UnitInstance _effectHolder, IEnumerable<BuffStatusEffect> _buffStatusEffects, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_effectHolder == null || _buffStatusEffects == null || _system == null)
                return;

            foreach (BuffStatusEffect bse in _buffStatusEffects.Where(x => !x.IsSum))
            {
                if (bse.ActivationCondition.IsTrue(_system, _effectHolder, bse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect))
                {
                    decimal effectValue = bse.Value.ToValue<decimal>(_system, _effectHolder, bse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

                    if (bse.TargetStatusType != eStatusType.DamageResistance && bse.TargetStatusType != eStatusType.CriRes)
                    {
                        if (effectValue >= 1.0m) // _value must not become negative. Furthermore, if effect value is less than 1, then multiplying _value by it means that _value decreases. That is, bse would no longer be a buff (positive) status effect.
                            _value *= effectValue;
                    }
                    else // As exception, _value must not increase.
                    {
                        if (effectValue >= 1.0m)
                            _value /= effectValue;
                    }

                    if (bse.Duration.ActivationTimes > 0)
                        bse.Duration.ActivationTimes--; // Subtract one from the remaining activation times
                }
            }
        }
        private static void ApplyMultiplicativeDebuffStatusEffects(ref decimal _value, UnitInstance _effectHolder, IEnumerable<
            DebuffStatusEffect> _debuffStatusEffects, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_effectHolder == null || _debuffStatusEffects == null || _system == null)
                return;

            foreach (DebuffStatusEffect dse in _debuffStatusEffects.Where(x => !x.IsSum))
            {
                if (dse.ActivationCondition.IsTrue(_system, _effectHolder, dse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect))
                {
                    decimal effectValue = dse.Value.ToValue<decimal>(_system, _effectHolder, dse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

                    if (dse.TargetStatusType != eStatusType.DamageResistance && dse.TargetStatusType != eStatusType.CriRes)

                    {
                        if (effectValue >= 1.0m) // _value must not become negative. Furthermore, if effect value is less than 1, then dividing _value by it means that _value increases. That is, dse would no longer be a debuff (negative) status effect.
                            _value /= effectValue;
                    }
                    else // As exception, _value must not decrease
                    {
                        if (effectValue >= 1.0m)
                            _value *= effectValue;
                    }

                    if (dse.Duration.ActivationTimes > 0)
                        dse.Duration.ActivationTimes--; // Subtract one from the remaining activation times
                }
            }
        }

        private static void ApplySummativeBuffStatusEffects(ref decimal _value, UnitInstance _effectHolder, IEnumerable<BuffStatusEffect> _buffStatusEffects, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_effectHolder == null || _buffStatusEffects == null || _system == null)
                return;

            foreach (BuffStatusEffect bse in _buffStatusEffects.Where(x => x.IsSum))
            {
                if (bse.ActivationCondition.IsTrue(_system, _effectHolder, bse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect))
                {
                    decimal effectValue = bse.Value.ToValue<decimal>(_system, _effectHolder, bse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

                    if (bse.TargetStatusType != eStatusType.DamageResistance && bse.TargetStatusType != eStatusType.CriRes)
                    {
                        if (effectValue >= 0) // If effect value is negative, then adding it to _value means that _value decreases. That is, bse would no longer be a buff (positive) status effect.
                            _value += effectValue;
                    }
                    else // As exception, _value must not increase. However, _value must not become negative either.
                    {
                        if (effectValue <= _value)
                            _value -= effectValue;
                    }

                    if (bse.Duration.ActivationTimes > 0)
                        bse.Duration.ActivationTimes--; // Subtract one from the remaining activation times
                }
            }
        }

        private static void ApplySummativeDebuffStatusEffects(ref decimal _value, UnitInstance _effectHolder, IEnumerable<DebuffStatusEffect> _debuffStatusEffects, BattleSystemCore _system, UnitInstance _skillUser = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_effectHolder == null || _debuffStatusEffects == null || _system == null)
                return;

            foreach (DebuffStatusEffect dse in _debuffStatusEffects.Where(x => x.IsSum))
            {
                if (dse.ActivationCondition.IsTrue(_system, _effectHolder, dse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect))
                {
                    decimal effectValue = dse.Value.ToValue<decimal>(_system, _effectHolder, dse, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

                    if (dse.TargetStatusType != eStatusType.DamageResistance && dse.TargetStatusType != eStatusType.CriRes)
                    {
                        if (effectValue >= _value) // _value must not become negative. Furthermore, if effect value is negative, then subtracting it from _value means that _value increases. That is, dse would no longer be a debuff (negative) status effect.
                            _value -= effectValue;
                    }
                    else // As exception, _valude must not decrease.
                    {
                        if (effectValue >= 0)
                            _value += effectValue;
                    }

                    if (dse.Duration.ActivationTimes > 0)
                        dse.Duration.ActivationTimes--; // Subtract one from the remaining activation times
                }
            }
        }

        //public static void ApplyMultipleBuffStatusEffects(ref decimal _value, UnitInstance _unit, List<eStatusType> _statusTypes, BattleSystemCore _system, ActiveSkill _skill = null)
        //{
        //    _statusTypes = _statusTypes.Distinct().ToList();

        //    List<BuffStatusEffect> buffStatusEffects = new List<BuffStatusEffect>();
        //    foreach (eStatusType statusType in _statusTypes)
        //    {
        //        buffStatusEffects.AddRange(_unit.StatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == statusType).ToList());

        //        if (_skill != null)
        //            buffStatusEffects.AddRange(_skill.TemporalStatusEffects.OfType<BuffStatusEffect>().Where(x => x.TargetStatusType == statusType).ToList());
        //    }

        //    foreach (BuffStatusEffect bse in buffStatusEffects.Where(x => !x.IsSum))
        //    {
        //        if (bse.ActivationCondition.IsTrue(_system, _unit))
        //        {
        //            decimal effectValue = bse.Value.ToValue<decimal>(_system);
        //            if (effectValue > 0) // If effect value is 0 or negative, then multiplying _value by it means that _value decreases. That is, bse would no longer be a buff (positive) status effect.
        //                _value *= effectValue;
        //        }
        //    }

        //    foreach (BuffStatusEffect bse in buffStatusEffects.Where(x => x.IsSum))
        //    {
        //        if (bse.ActivationCondition.IsTrue(_system, _unit))
        //        {
        //            decimal effectValue = bse.Value.ToValue<decimal>(_system);
        //            if (effectValue >= 0) // If effect value is negative, then adding it to _value means that _value decreases. That is, bse would no longer be a buff (positive) status effect.
        //                _value += effectValue;
        //        }
        //    }

        //    foreach (BuffStatusEffect bse in buffStatusEffects)
        //    {
        //        if (bse.Duration.ActivationTimes > 0) // If it could be activated more than once
        //            bse.Duration.ActivationTimes--; // Subtract one from the remaining activation times
        //    }
        //}

        public static void ApplyTargetRangeModStatusEffects(ref List<_2DCoord> _targetRange, UnitInstance _effectHolder, bool _isMovementRangeClassification, BattleSystemCore _system)
        {
            if (_targetRange == null
                || _effectHolder == null
                || _system == null)
            {
                return;
            }

            ApplyTargetRangeModStatusEffects_ActualDefinition(ref _targetRange, _effectHolder, _isMovementRangeClassification, _system);
        }
        public static void ApplyTargetRangeModStatusEffects(ref List<_2DCoord> _targetRange, UnitInstance _effectHolder, bool _isMovementRangeClassification, BattleSystemCore _system, ActiveSkill _skill, Effect _effect)
        {
            if (_targetRange == null
                || _effectHolder == null
                || _system == null
                || _skill == null
                || _effect == null)
            {
                return;
            }

            ApplyTargetRangeModStatusEffects_ActualDefinition(ref _targetRange, _effectHolder, _isMovementRangeClassification, _system, _skill, _effect);
        }
        private static void ApplyTargetRangeModStatusEffects_ActualDefinition(ref List<_2DCoord> _targetRange, UnitInstance _effectHolder, bool _isMovementRangeClassification, BattleSystemCore _system, Skill _skill = null, Effect _effect = null)
        {
            List<TargetRangeModStatusEffect> targetRangeModStatusEffects = _effectHolder.StatusEffects.OfType<TargetRangeModStatusEffect>().Where(x => x.IsMovementRangeClassification == _isMovementRangeClassification).ToList();

            AddPassiveSkillTargetRangeModStatusEffects(targetRangeModStatusEffects, _system, _effectHolder);
            AddEquipmentTargetRangeModStatusEffects(targetRangeModStatusEffects, _system, _effectHolder);

            if (_skill != null && !_isMovementRangeClassification)
            {
                foreach (TargetRangeModStatusEffectData data in _skill.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                {
                    targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data));
                }
            }

            // If there is at least one status effect such that its modification method is 'Overwrite,'
            // get a list of status effects where modification method is Overwrite.
            // Then apply only the last status effect in such list.
            if (targetRangeModStatusEffects.Any(x => x.ModificationMethod == eModificationMethod.Overwrite))
            {
                targetRangeModStatusEffects = targetRangeModStatusEffects.Where(x => x.ModificationMethod == eModificationMethod.Overwrite).ToList();
                _targetRange = TargetArea.GetTargetArea(targetRangeModStatusEffects.Last().TargetRangeClassification);
            }
            else
            {
                // Add each target range allowing duplicates
                foreach (TargetRangeModStatusEffect trmse in targetRangeModStatusEffects.Where(x => x.ModificationMethod == eModificationMethod.Add))
                {
                    if (trmse.ActivationCondition.IsTrue(_system, _effectHolder, trmse, _effectHolder, _skill, _effect))
                        _targetRange.AddRange(TargetArea.GetTargetArea(trmse.TargetRangeClassification));
                }

                // For each coordinate in each target range, subtract the first occurence of the coordinate from _targetRange if it contains the coordinate
                foreach (TargetRangeModStatusEffect trmse in targetRangeModStatusEffects.Where(x => x.ModificationMethod == eModificationMethod.Subtract))
                {
                    if (trmse.ActivationCondition.IsTrue(_system, _effectHolder, trmse, _effectHolder, _skill, _effect))
                    {
                        foreach (_2DCoord coord in TargetArea.GetTargetArea(trmse.TargetRangeClassification))
                        {
                            _targetRange.Remove(coord);
                        }
                    }
                }

                // Remove duplicates
                _targetRange = _targetRange.Distinct().ToList();
            }
        }

        private static void AddPassiveSkillTargetRangeModStatusEffects(List<TargetRangeModStatusEffect> _targetRangeModStatusEffects, BattleSystemCore _system, UnitInstance _unit)
        {
            if (_targetRangeModStatusEffects == null
                || _system == null
                || _unit == null)
            {
                return;
            }

            foreach (PassiveSkill passiveSkill in _unit.Skills.OfType<PassiveSkill>())
            {
                foreach (TargetRangeModStatusEffectData data in passiveSkill.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                {
                    _targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data, null, passiveSkill.Level));
                }
            }

            if (_unit.InheritedSkill is PassiveSkill)
            {
                foreach (TargetRangeModStatusEffectData data in _unit.InheritedSkill.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                {
                    _targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data, null, _unit.InheritedSkill.Level));
                }
            }
        }

        private static void AddEquipmentTargetRangeModStatusEffects(List<TargetRangeModStatusEffect> _targetRangeModStatusEffects, BattleSystemCore _system, UnitInstance _equipmentOwner, List<_2DCoord> _effectRange = null)
        {
            if (_targetRangeModStatusEffects == null
                || _system == null
                || _equipmentOwner == null)
            {
                return;
            }

            if (_equipmentOwner.MainWeapon != null)
            {
                int mainWeaponLevel = Level(_equipmentOwner.MainWeapon);
                foreach (TargetRangeModStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                {
                    _targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data, null, 0, mainWeaponLevel));
                }

                if (_equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill != null && _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill is PassiveSkill)
                {
                    foreach (TargetRangeModStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                    {
                        _targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data, null, _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.Level, mainWeaponLevel));
                    }
                }
            }

            if (_equipmentOwner.SubWeapon != null)
            {
                int subWeaponLevel = Level(_equipmentOwner.SubWeapon);
                foreach (TargetRangeModStatusEffectData data in _equipmentOwner.SubWeapon.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                {
                    _targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data, null, 0, subWeaponLevel));
                }
            }

            if (_equipmentOwner.Armour != null)
            {
                foreach (TargetRangeModStatusEffectData data in _equipmentOwner.Armour.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                {
                    _targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data));
                }
            }

            if (_equipmentOwner.Accessory != null)
            {
                foreach (TargetRangeModStatusEffectData data in _equipmentOwner.Accessory.BaseInfo.StatusEffectsData.OfType<TargetRangeModStatusEffectData>())
                {
                    _targetRangeModStatusEffects.Add(new TargetRangeModStatusEffect(data));
                }
            }
        }

        public static void AddPassiveSkillForegroundStatusEffects(List<ForegroundStatusEffect> _foregroundStatusEffects, BattleSystemCore _system, UnitInstance _unit, eEventTriggerTiming _eventTriggerTiming)
        {
            if (_foregroundStatusEffects == null
                || _system == null
                || _unit == null)
            {
                return;
            }

            foreach (PassiveSkill passiveSkill in _unit.Skills.OfType<PassiveSkill>())
            {
                foreach (ForegroundStatusEffectData data in passiveSkill.BaseInfo.StatusEffectsData.OfType<ForegroundStatusEffectData>()
                    .Where(x => _system.DoesStatusEffectActivationPhaseMatch(x, _unit, _eventTriggerTiming)))
                {
                    if (data is DamageStatusEffectData)
                        _foregroundStatusEffects.Add(new DamageStatusEffect(data as DamageStatusEffectData, null, passiveSkill.Level));
                    else if (data is HealStatusEffectData)
                        _foregroundStatusEffects.Add(new HealStatusEffect(data as HealStatusEffectData, null, passiveSkill.Level));
                }
            }

            if (_unit.InheritedSkill is PassiveSkill)
            {
                foreach (ForegroundStatusEffectData data in _unit.InheritedSkill.BaseInfo.StatusEffectsData.OfType<ForegroundStatusEffectData>()
                    .Where(x => _system.DoesStatusEffectActivationPhaseMatch(x, _unit, _eventTriggerTiming)))
                {
                    if (data is DamageStatusEffectData)
                        _foregroundStatusEffects.Add(new DamageStatusEffect(data as DamageStatusEffectData, null, _unit.InheritedSkill.Level));
                    else if (data is HealStatusEffectData)
                        _foregroundStatusEffects.Add(new HealStatusEffect(data as HealStatusEffectData, null, _unit.InheritedSkill.Level));
                }
            }
        }

        public static void AddEquipmentForegroundStatusEffects(List<ForegroundStatusEffect> _foregroundStatusEffects, BattleSystemCore _system, UnitInstance _equipmentOwner, eEventTriggerTiming _eventTriggerTiming)
        {
            if (_foregroundStatusEffects == null
                || _system == null
                || _equipmentOwner == null)
            {
                return;
            }

            if (_equipmentOwner.MainWeapon != null)
            {
                int mainWeaponLevel = Level(_equipmentOwner.MainWeapon);
                foreach (ForegroundStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.StatusEffectsData.OfType<ForegroundStatusEffectData>()
                    .Where(x => _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner, _eventTriggerTiming)))
                {
                    if (data is DamageStatusEffectData)
                        _foregroundStatusEffects.Add(new DamageStatusEffect(data as DamageStatusEffectData, null, 0, mainWeaponLevel));
                    else if (data is HealStatusEffectData)
                        _foregroundStatusEffects.Add(new HealStatusEffect(data as HealStatusEffectData, null, 0, mainWeaponLevel));
                }

                if (_equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill != null && _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill is PassiveSkill)
                {
                    foreach (ForegroundStatusEffectData data in _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.BaseInfo.StatusEffectsData.OfType<ForegroundStatusEffectData>()
                        .Where(x => _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner, _eventTriggerTiming)))
                    {
                        if (data is DamageStatusEffectData)
                            _foregroundStatusEffects.Add(new DamageStatusEffect(data as DamageStatusEffectData, null, _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.Level, mainWeaponLevel));
                        else if (data is HealStatusEffectData)
                            _foregroundStatusEffects.Add(new HealStatusEffect(data as HealStatusEffectData, null, _equipmentOwner.MainWeapon.BaseInfo.MainWeaponSkill.Level, mainWeaponLevel));
                    }
                }
            }

            if (_equipmentOwner.SubWeapon != null)
            {
                int subWeaponLevel = Level(_equipmentOwner.SubWeapon);
                foreach (ForegroundStatusEffectData data in _equipmentOwner.SubWeapon.BaseInfo.StatusEffectsData.OfType<ForegroundStatusEffectData>()
                    .Where(x => _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner, _eventTriggerTiming)))
                {
                    if (data is DamageStatusEffectData)
                        _foregroundStatusEffects.Add(new DamageStatusEffect(data as DamageStatusEffectData, null, 0, subWeaponLevel));
                    else if (data is HealStatusEffectData)
                        _foregroundStatusEffects.Add(new HealStatusEffect(data as HealStatusEffectData, null, 0, subWeaponLevel));
                }
            }

            if (_equipmentOwner.Armour != null)
            {
                foreach (ForegroundStatusEffectData data in _equipmentOwner.Armour.BaseInfo.StatusEffectsData.OfType<ForegroundStatusEffectData>()
                    .Where(x => _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner, _eventTriggerTiming)))
                {
                    if (data is DamageStatusEffectData)
                        _foregroundStatusEffects.Add(new DamageStatusEffect(data as DamageStatusEffectData, null));
                    else if (data is HealStatusEffectData)
                        _foregroundStatusEffects.Add(new HealStatusEffect(data as HealStatusEffectData, null));
                }
            }

            if (_equipmentOwner.Accessory != null)
            {
                foreach (ForegroundStatusEffectData data in _equipmentOwner.Accessory.BaseInfo.StatusEffectsData.OfType<ForegroundStatusEffectData>()
                    .Where(x => _system.DoesStatusEffectActivationPhaseMatch(x, _equipmentOwner, _eventTriggerTiming)))
                {
                    if (data is DamageStatusEffectData)
                        _foregroundStatusEffects.Add(new DamageStatusEffect(data as DamageStatusEffectData, null));
                    else if (data is HealStatusEffectData)
                        _foregroundStatusEffects.Add(new HealStatusEffect(data as HealStatusEffectData, null));
                }
            }
        }

        /// <summary>
        /// Example:<para/>
        /// 0 represents the initial position and other numbers show the distance.<para/>
        /// 6 5 4 3 4 5 6<para/>
        /// 5 4 3 2 3 4 5<para/>
        /// 4 3 2 1 2 3 4<para/>
        /// 3 2 1 0 1 2 3<para/>
        /// 4 3 2 1 2 3 4<para/>
        /// 5 4 3 2 3 4 5<para/>
        /// 6 5 4 3 4 5 6<para/>
        /// </summary>
        public static int Distance(_2DCoord _initialCoord, _2DCoord _targetCoord)
        {
            int xDistance = Math.Abs(_initialCoord.X - _targetCoord.X);
            int yDistance = Math.Abs(_initialCoord.Y - _targetCoord.Y);

            return xDistance + yDistance;
        }

        public static List<_2DCoord> CoordsInDistance(_2DCoord _referenceCoord, int _distance)
        {
            List<_2DCoord> result = new List<_2DCoord>();

            for (int y = 0; y < CoreValues.SIZE_OF_A_SIDE_OF_BOARD; y++)
            {
                for (int x = 0; x < CoreValues.SIZE_OF_A_SIDE_OF_BOARD; x++)
                {
                    _2DCoord coord = new _2DCoord(x, y);
                    if (Distance(_referenceCoord, coord) == _distance)
                        result.Add(coord);
                }
            }

            return result;
        }

        public static List<_2DCoord> CoordsBetween(_2DCoord _a, _2DCoord _b)
        {
            List<_2DCoord> result = new List<_2DCoord>();

            if (_a.X == _b.X)
            {
                int yDistance = Math.Abs(_a.Y - _b.Y);
                int lowerY = (_a.Y < _b.Y) ? _a.Y : _b.Y;
                for (int i = 1; i <= yDistance - 1; i++) // The number of coords between the given two coords is yDistance - 1
                {
                    result.Add(new _2DCoord(_a.X, lowerY + i));
                }
            }
            else if (_a.Y == _b.Y)
            {
                int xDistance = Math.Abs(_a.X - _b.X);
                int lowerX = (_a.X < _b.X) ? _a.X : _b.X;
                for (int i = 1; i <= xDistance - 1; i++) // The number of coords between the given two coords is xDistance - 1
                {
                    result.Add(new _2DCoord(lowerX + i, _a.Y));
                }
            }
            else 
            {
                int yDistance = Math.Abs(_a.Y - _b.Y);
                int xDistance = Math.Abs(_a.X - _b.X);
                if (yDistance == xDistance) // Diagonally connected
                {
                    int lowerY = (_a.Y < _b.Y) ? _a.Y : _b.Y;
                    int lowerX = (_a.X < _b.X) ? _a.X : _b.X;

                    for (int i = 1; i <= xDistance - 1; i++)
                    {
                        result.Add(new _2DCoord(lowerX + i, lowerY + i));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns element effectiveness as double
        /// Example:
        /// If the returned value is 2.0, it means that the effect will be twice as effective against the target
        /// </summary>
        /// <param name="_effectUserElement"></param>
        /// <param name=""></param>
        /// <returns></returns>
        /// 
        public static decimal DamageCorrectionRate_Element(UnitInstance _attacker, eElement _effectElement, UnitInstance _defender, out eEffectiveness _effectiveness)
        {
            decimal correctionRate = 1.0m;

            if (DoesElementMatch(_attacker.BaseInfo.Elements, _effectElement))
                correctionRate *= CoreValues.MULTIPLIER_FOR_ELEMENT_MATCH;

            correctionRate *= ElementEffectiveness(_attacker, _effectElement, _defender, out _effectiveness);

            return correctionRate;
        }

        /// <summary>
        /// PreCondition: _attacker, _defender, and _effect have been initialized successfully.
        /// PostCondition: Correct boolean value whether _effect succeeded or not will be returned.
        /// </summary>
        /// <param name="_skillUser"></param>
        /// <param name="_target"></param>
        /// <param name="_effect"></param>
        /// <returns></returns>
        public static bool DoesSucceed(BattleSystemCore _system, UnitInstance _skillUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_system == null
                || _skillUser == null
                || _skill == null
                || _effect == null
                || _effectRange == null)
            {
                return false;
            }

            decimal successRate = _effect.SuccessRate.ToValue<decimal>(_system, null, null, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            decimal precision = Precision(_skillUser, _system, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
            if (precision <= 0.0m)
                return false;
            else
                successRate *= precision;

            if (_target != null)
            {
                decimal evasion = Evasion(_skillUser, _system, _skillUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
                if (evasion <= 0.0m)
                    return true;
                else
                    successRate /= evasion;
            }

            return CoreFunctions.IsSuccess(successRate);
        }

        /// <summary>
        /// PreCondition: _attacker, _defender, and _effect have been initialized successfully.
        /// PostCondition: A boolean value representing whether the action(or skill) was critical will be returned based on the properties of _attacker, _defender, and _effect.
        /// </summary>
        /// <param name="_attacker"></param>
        /// <param name="_defender"></param>
        /// <param name="_effect"></param>
        /// <returns></returns>
        public static bool IsCritical(UnitInstance _attacker, ActiveSkill _skill, DamageEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect, BattleSystemCore _system)
        {
            if (_attacker == null
                || _skill == null
                || _effect == null
                || _effectRange == null
                || _target == null
                || _system == null)
            {
                return false;
            }

            decimal criticalRate = CoreValues.DEFAULT_CRITICAL_RATE;

            ApplyBuffAndDebuffStatusEffects_Compound(ref criticalRate, _system, eStatusType.Cri, _attacker, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return CoreFunctions.IsSuccess(criticalRate);
        }

        public static bool IsCritical(UnitInstance _effectUser, ActiveSkill _skill, HealEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect, BattleSystemCore _system)
        {
            if (_effectUser == null
                || _target == null
                || _effect == null)
            {
                return false;
            }

            decimal criticalRate = CoreValues.DEFAULT_CRITICAL_RATE;

            ApplyBuffAndDebuffStatusEffects_Compound(ref criticalRate, _system, eStatusType.Cri, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            return CoreFunctions.IsSuccess(criticalRate);
        }
        #endregion

        #region Private Methods
        private static decimal CorrectionRate_Force(BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, DamageEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_system == null
                || _effectUser == null
                || _skill == null
                || _effect == null
                || _target == null
                || _effectRange == null)
            {
                return -1;
            }

            try
            {
                List<object> targets = _targets.Cast<object>().ToList();
                List<object> secondaryTargetsForComplexTargetSelectionEffect = _secondaryTargetsForComplexTargetSelectionEffect?.Cast<object>().ToList();

                decimal force = _effect.Value.ToValue<decimal>(_system, null, null, _effectUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

                ApplyBuffAndDebuffStatusEffects_Simple(ref force, _system, _effectUser, eStatusType.DamageForce, _effectUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

                decimal multiplier = (force > 0) ? 1m + (force - 1m) * CoreValues.POW_ADJUSTMENT_CONST_D : 0; // D = 19/99 so that this equation equals 1 when force is 1 and 20 when force is 100

                return multiplier;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    
        private static decimal CorrectionRate_Force(BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, HealEffect _effect, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target, List<UnitInstance> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_system == null
                || _effectUser == null
                || _skill == null
                || _effect == null
                || _target == null
                || _effectRange == null)
            {
                return -1;
            }

            try
            {
                List<object> targets = _targets.Cast<object>().ToList();
                List<object> secondaryTargetsForComplexTargetSelectionEffect = _secondaryTargetsForComplexTargetSelectionEffect?.Cast<object>().ToList();

                decimal force = _effect.Value.ToValue<decimal>(_system, null, null, _effectUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

                ApplyBuffAndDebuffStatusEffects_Simple(ref force, _system, _effectUser, eStatusType.HealForce, _effectUser, _skill, _effect, _effectRange, targets, _target, secondaryTargetsForComplexTargetSelectionEffect);

                decimal multiplier = (force > 0) ? 1m + (force - 1m) * CoreValues.POW_ADJUSTMENT_CONST_D : 0; // D = 19/99 so that this equation equals 1 when force is 1 and 20 when force is 100

                return multiplier;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// PreCondition: _attacker and _defender have been initialized successfully.
        /// PostCondition: A double value representing the effectiveness of the effect will be returned based on the element properties of _attacker and _defender.
        /// </summary>
        /// <param name="_attacker"></param>
        /// <param name="_effectElement"></param>
        /// <param name="_defender"></param>
        /// <returns></returns>
        private static decimal ElementEffectiveness(UnitInstance _attacker, eElement _effectElement, UnitInstance _defender, out eEffectiveness _effectiveness)
        {
            decimal effectiveness = 1.0m;
            _effectiveness = eEffectiveness.Neutral;

            int effectiveCount = 0;
            int ineffectiveCount = 0;

            //Effect base effectiveness
            foreach (eElement targetElement in _defender.BaseInfo.Elements)
            {
                switch (_effectElement)
                {
                    case eElement.Blue:
                        if (targetElement == eElement.Red)
                            effectiveCount++;
                        else if (targetElement == eElement.Ocher)
                            ineffectiveCount++;
                        break;
                    case eElement.Red:
                        if (targetElement == eElement.Green)
                            effectiveCount++;
                        else if (targetElement == eElement.Blue)
                            ineffectiveCount++;
                        break;
                    case eElement.Green:
                        if (targetElement == eElement.Ocher)
                            effectiveCount++;
                        else if (targetElement == eElement.Red)
                            ineffectiveCount++;
                        break;
                    case eElement.Ocher:
                        if (targetElement == eElement.Blue)
                            effectiveCount++;
                        else if (targetElement == eElement.Green)
                            ineffectiveCount++;
                        break;
                    case eElement.Purple:
                        if (targetElement == eElement.Yellow)
                            effectiveCount++;
                        break;
                    case eElement.Yellow:
                        if (targetElement == eElement.Purple)
                            effectiveCount++;
                        break;
                    default: //case eElement.None
                        break;
                }
            }

            if (effectiveCount > ineffectiveCount)
            {
                effectiveness = CoreValues.MULTIPLIER_FOR_EFFECTIVE_ELEMENT * (effectiveCount - ineffectiveCount);
                _effectiveness = eEffectiveness.Effective;
            }
            else if (effectiveCount < ineffectiveCount)
            {
                effectiveness = CoreValues.MULTIPLIER_FOR_INEFFECTIVE_ELEMENT * (ineffectiveCount - effectiveCount);
                _effectiveness = eEffectiveness.Ineffective;
            }

            return effectiveness;
        }

        /// <summary>
        /// PreCondition: _unitElements is not null.
        /// PostCondition: Compares each eElement in _unitElements with _targetElement and returns true if at least one matches.
        /// </summary>
        /// <param name="_unitElements"></param>
        /// <param name="_targetElement"></param>
        /// <returns></returns>
        private static bool DoesElementMatch(IList<eElement> _unitElements, eElement _targetElement)
        {
            foreach (eElement e in _unitElements)
            {
                if (e == _targetElement)
                    return true;
            }

            return false;
        }

        private static decimal CorrectionRate_TileType(Effect _effect, eTileType _tileType)
        {
            eElement effectElement = (_effect is DamageEffect damageEffect) ? damageEffect.Element : default;

            switch (_tileType)
            {
                case eTileType.Blue:
                    if (effectElement == eElement.Blue)
                        return CoreValues.MULTIPLIER_FOR_TILETYPEMATCH;
                    break;
                case eTileType.Red:
                    if (effectElement == eElement.Red)
                        return CoreValues.MULTIPLIER_FOR_TILETYPEMATCH;
                    break;
                case eTileType.Green:
                    if (effectElement == eElement.Green)
                        return CoreValues.MULTIPLIER_FOR_TILETYPEMATCH;
                    break;
                case eTileType.Ocher:
                    if (effectElement == eElement.Ocher)
                        return CoreValues.MULTIPLIER_FOR_TILETYPEMATCH;
                    break;
                case eTileType.Purple:
                    if (effectElement == eElement.Purple)
                        return CoreValues.MULTIPLIER_FOR_TILETYPEMATCH;
                    break;
                case eTileType.Yellow:
                    if (effectElement == eElement.Yellow)
                        return CoreValues.MULTIPLIER_FOR_TILETYPEMATCH;
                    break;
                case eTileType.Heal:
                    if (_effect is HealEffect)
                        return CoreValues.MULTIPLIER_FOR_TILETYPEMATCH;
                    break;
                default: //case eTileType.Normal
                    return 1.0m;
            }

            return 1.0m;
        }
        #endregion
    }
}
