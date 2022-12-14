using EEANWorks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EEANWorks.Games.TBSG._01
{
    public sealed class ComplexCondition : IDeepCopyable<ComplexCondition>
    {
        public ComplexCondition(List<ConditionSet> _conditionSets)
        {
            m_conditionSets = _conditionSets.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        #region Properties
        public IList<ConditionSet> ConditionSets { get { return m_conditionSets.AsReadOnly(); } }
        //List<Condition> represents the AND relationship between each Condition, and the List<Condition> instances have an OR relation in between.
        //Example:
        //A && (B || C)  (Where A, B, C are instances of Condition class)
        //which means (A and B) or (A and C)
        //may be stored as
        //ConditionSets[0][0] == A
        //ConditionSets[0][1] == B
        //ConditionSets[1][0] == A
        //ConditionSets[1][1] == C

        public string HierarchyString { get { return HierarchizeString(); } } //Method Wrapper
        #endregion

        #region Private Fields
        private List<ConditionSet> m_conditionSets;
        #endregion

        #region Public Methods
        public bool IsTrue(BattleSystemCore _system, UnitInstance _effectHolder = null, StatusEffect _statusEffect = null, UnitInstance _actor = null, Skill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null, int _targetPreviousHP = 0, int _targetPreviousLocationTileIndex = 0, List<StatusEffect> _statusEffects = null, UnitInstance _effectHolderOfActivatedEffect = null, StatusEffect _statusEffectActivated = null, eTileType _previousTileType = default)
        {
            if (m_conditionSets.Count == 0) // If no conditionSet is present
                return true;

            foreach (ConditionSet conditionSet in m_conditionSets)
            {
                // If at least one conditionSet is true
                if (conditionSet.IsTrue(_system, _effectHolder, _statusEffect, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _targetPreviousHP, _targetPreviousLocationTileIndex, _statusEffects, _effectHolderOfActivatedEffect, _statusEffectActivated, _previousTileType))
                    return true;
            }

            return false; // None of the conditionSets were true
        }

        public string ToFormattedString(int _level)
        {
            string result = string.Empty;

            for (int i = 0; i < m_conditionSets.Count; i++)
            {
                if (i != 0) // If it is not the first condition set
                    result += " OR ";

                result += "{" + m_conditionSets[i].ToFormattedString(_level) + "}";
            }

            return result;
        }

        public ComplexCondition DeepCopy()
        {
            ComplexCondition copy = (ComplexCondition)this.MemberwiseClone();

            copy.m_conditionSets = new List<ConditionSet>(m_conditionSets);

            return copy;
        }
        #endregion

        #region Private Methods
        private string HierarchizeString()
        {
            string result = string.Empty;

            for (int i = 0; i < m_conditionSets.Count; i++)
            {
                if (i > 1)
                    result += "OR ";

                result += "[Condition " + (i + 1).ToString() + "]\n";

                foreach (Condition compositeCondition in m_conditionSets[i].Conditions)
                {
                    result += compositeCondition.A.HierarchyString + "\n";
                    result += compositeCondition.RelationType.ToString();
                    result += compositeCondition.B.HierarchyString + "\n";

                    if (!compositeCondition.Equals(m_conditionSets[i].Conditions.Last()))
                        result += "*AND*";
                }
            }

            return result;
        }
        #endregion
    }

    public sealed class ConditionSet
    {
        public ConditionSet(List<Condition> _conditions)
        {
            m_conditions = _conditions.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects within GameDataContainer
        }

        #region Properties
        public IList<Condition> Conditions { get { return m_conditions.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<Condition> m_conditions;
        #endregion

        #region Public Methods
        public bool IsTrue(BattleSystemCore _system, UnitInstance _effectHolder, StatusEffect _statusEffect, UnitInstance _actor, Skill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect, int _targetPreviousHP, int _targetPreviousLocationTileIndex, List<StatusEffect> _statusEffects, UnitInstance _effectHolderOfActivatedEffect, StatusEffect _statusEffectActivated, eTileType _previousTileType)
        {
            foreach (Condition condition in m_conditions)
            {
                // If at least one condition is false
                if (!condition.IsTrue(_system, _effectHolder, _statusEffect, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _targetPreviousHP, _targetPreviousLocationTileIndex, _statusEffects, _effectHolderOfActivatedEffect, _statusEffectActivated, _previousTileType))
                    return false;
            }

            return true; // If no condition is present or if all conditions were true
        }

        public string ToFormattedString(int _level)
        {
            string result = string.Empty;

            for (int i = 0; i < m_conditions.Count; i++)
            {
                if (i != 0) // If it is not the first condition
                    result += " AND ";

                result += m_conditions[i].ToFormattedString(_level);
            }

            return result;
        }
        #endregion
    }

    public sealed class Condition
    {
        public Condition(Tag _a, eRelationType _relationType, Tag _b)
        {
            A = _a; // Getting a reference to an object within GameDataContainer

            RelationType = _relationType;

            B = _b; // Getting a reference to an object within GameDataContainer
        }

        #region Properties
        public Tag A { get; }
        public eRelationType RelationType { get; }
        public Tag B { get; }
        #endregion

        #region Public Methods
        public bool IsTrue(BattleSystemCore _system, UnitInstance _effectHolder = null, StatusEffect _statusEffect = null, UnitInstance _actor = null, Skill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null, int _targetPreviousHP = 0, int _targetPreviousLocationTileIndex = 0, List<StatusEffect> _statusEffects = null, UnitInstance _effectHolderOfActivatedEffect = null, StatusEffect _statusEffectActivated = null, eTileType _previousTileType = default(eTileType))
        {
            try
            {
                var valueA = A.ToValue<object>(_system, _effectHolder, _statusEffect, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _targetPreviousHP, _targetPreviousLocationTileIndex, _statusEffects, _effectHolderOfActivatedEffect, _statusEffectActivated, _previousTileType);
                var valueB = B.ToValue<object>(_system, _effectHolder, _statusEffect, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _targetPreviousHP, _targetPreviousLocationTileIndex, _statusEffects, _effectHolderOfActivatedEffect, _statusEffectActivated, _previousTileType);

                return CoreFunctions.Compare(valueA, RelationType, valueB);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string ToFormattedString(int _level)
        {
            string relationString = string.Empty;
            switch (RelationType)
            {
                case eRelationType.EqualTo:
                    relationString = " is ";
                    break;
                case eRelationType.NotEqualTo:
                    relationString = " is not ";
                    break;
                case eRelationType.GreaterThan:
                    relationString = " > ";
                    break;
                case eRelationType.LessThan:
                    relationString = " < ";
                    break;
                case eRelationType.GreaterThanOrEqualTo:
                    relationString = " ≧ ";
                    break;
                case eRelationType.LessThanOrEqualTo:
                    relationString = " ≦ ";
                    break;
                default:
                    break;
            }

            return A.ToFormattedString(_level) + relationString + B.ToFormattedString(_level);
        }
        #endregion
    }
}
