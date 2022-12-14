using EEANWorks;
using EEANWorks.Games.TBSG._01.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01
{
    public class BattleSystemCore
    {
        public BattleSystemCore(Field _field)
        {
            Field = _field;

            IsMatchEnd = false;

            CurrentTurnPlayer = Field.Players[0]; // Players[0] Starts the Game

            CurrentPlayerTurns = new int[2] { 1, 0 };

            CurrentPhase = eGamePhase.BeginningOfMatch;

            UpdateSP(CurrentTurnPlayer);

            m_eventLogs = new List<EventLog>();
        }
        /// <summary>
        /// Copy Ctor
        /// </summary>
        public BattleSystemCore(BattleSystemCore _system)
        {
            Field = new Field(_system.Field);
            IsMatchEnd = false;
            CurrentTurnPlayer = _system.CurrentTurnPlayer;
            CurrentPlayerTurns = _system.CurrentPlayerTurns;
            CurrentPhase = _system.CurrentPhase;
            m_eventLogs = new List<EventLog>(_system.m_eventLogs);
        }

        #region Properties
        public Field Field { get; private set; }

        public bool IsMatchEnd { get; private set; }
        public bool IsPlayer1Winner { get; private set; }

        public decimal CurrentFullTurns { get {
                decimal result = 0.0m;
                foreach (int playerTurn in CurrentPlayerTurns)
                {
                    result += Convert.ToDecimal(playerTurn) / CurrentPlayerTurns.Length;
                }
                return result;
            } } //0.5 per player turn (2 Players).
        public int[] CurrentPlayerTurns { get; private set; }

        public PlayerOnBoard CurrentTurnPlayer { get; private set; }

        public eGamePhase CurrentPhase { get; private set; }

        public IList<EventLog> EventLogs { get { return m_eventLogs.AsReadOnly(); } }
        #endregion

        #region Private Fields
        private List<EventLog> m_eventLogs;
        #endregion

        #region Public Methods
        //public List<UnitInstance> SortCharacters(eCharacterPropertyType _property)
        //{
        //    List<UnitInstance> sortedList = new List<UnitInstance>();

        //    foreach (UnitInstance c in Units)
        //    {
        //        sortedList.Add(c);
        //    }

        //    switch (_property)
        //    {
        //        case eCharacterPropertyType.NAME:
        //            for(int i = 1; i <= sortedList.Count; i++)
        //            {
        //                if(sortedList.Sort())
        //            }
        //    }
        //}

        public Dictionary<_2DCoord, bool> GetMovableAndSelectableAreaForSimulation(UnitInstance _unit)
        {
            Dictionary<_2DCoord, bool> targetArea = new Dictionary<_2DCoord, bool>();

            foreach (_2DCoord coord in GetMovableArea(_unit))
            {
                if (Field.Board.Sockets[coord.X, coord.Y].Unit != null)
                    targetArea.Add(coord, false);
                else
                    targetArea.Add(coord, true);
            }

            return targetArea;
        }
        /// <summary>
        /// The bool value of the dictionary returned represents whether the coord is selectable.
        /// </summary>
        public Dictionary<_2DCoord, bool> GetMovableAndSelectableArea(UnitInstance _unit)
        {
            Dictionary<_2DCoord, bool> targetArea = new Dictionary<_2DCoord, bool>();

            foreach (_2DCoord coord in GetMovableArea(_unit))
            {
                if (_unit.OwnerInstance.Moved
                    // || _unit.moveBinded
                    || Field.Board.Sockets[coord.X, coord.Y].Unit != null)
                    targetArea.Add(coord, false);
                else
                    targetArea.Add(coord, true);
            }

            return targetArea;
        }
        public List<_2DCoord> GetMovableArea(UnitInstance _unit)
        {
            List<_2DCoord> targetArea = new List<_2DCoord>();

            List<_2DCoord> relativeTargetArea = Calculator.RelativeTargetArea(_unit, true, this);

            foreach (_2DCoord relativeCoord in relativeTargetArea)
            {
                _2DCoord realTargetAreaCoord = Field.ToRealCoord(Field.UnitLocation(_unit), Field.RelativeCoordToCorrectDirection(_unit.OwnerInstance, relativeCoord));

                if (Field.IsCoordWithinBoard(realTargetAreaCoord))
                    targetArea.Add(realTargetAreaCoord);
                //else the value(s) of realTargetAreaCoord is(are) out of the board 
            }

            return targetArea;
        }

        /// <summary>
        /// The bool value of the dictionary returned represents whether the coord is selectable.
        /// </summary>
        public Dictionary<_2DCoord, bool> GetAttackTargetableAndSelectableArea(UnitInstance _attacker)
        {
            Dictionary<_2DCoord, bool> targetArea = new Dictionary<_2DCoord, bool>();

            foreach (_2DCoord coord in GetAttackTargetableArea(_attacker))
            {
                UnitInstance unitAtCoordXY = Field.Board.Sockets[coord.X, coord.Y].Unit;

                if (!_attacker.OwnerInstance.Attacked
                    // && !_attacker.attackBinded
                    && (unitAtCoordXY != null) ? (unitAtCoordXY.OwnerInstance != _attacker.OwnerInstance) : false)
                    targetArea.Add(coord, true);
                else
                    targetArea.Add(coord, false);
            }

            return targetArea;
        }
        public List<_2DCoord> GetAttackTargetableArea(UnitInstance _attacker)
        {
            List<_2DCoord> targetArea = new List<_2DCoord>();

            List<_2DCoord> relativeTargetArea = Calculator.RelativeTargetArea(_attacker, false, this);

            foreach (_2DCoord relativeCoord in relativeTargetArea)
            {
                _2DCoord realTargetAreaCoord = Field.ToRealCoord(Field.UnitLocation(_attacker), Field.RelativeCoordToCorrectDirection(_attacker.OwnerInstance, relativeCoord));

                if (Field.IsCoordWithinBoard(realTargetAreaCoord))
                    targetArea.Add(realTargetAreaCoord);
                //else the value(s) of realTargetAreaCoord is(are) out of the board 
            }

            return targetArea;
        }

        public Dictionary<_2DCoord, bool> GetSkillTargetableAndSelectableAreaForSimulation(UnitInstance _skillUser, ActiveSkill _skill)
        {
            if (_skillUser == null || _skill == null)
                return null;

            Dictionary<_2DCoord, bool> targetArea = new Dictionary<_2DCoord, bool>();

            List<_2DCoord> realTargetArea = GetSkillTargetableArea(_skillUser, _skill);

            List<_2DCoord> selectableCoords = GetEffectSelectableCoords(_skillUser, _skill, _skill.BaseInfo.Effect, realTargetArea);
            foreach (_2DCoord coord in realTargetArea)
            {
                if (selectableCoords.Contains(coord))
                    targetArea.Add(coord, true);
                else
                    targetArea.Add(coord, false);
            }

            return targetArea;
        }
        public Dictionary<_2DCoord, bool> GetSkillTargetableAndSelectableArea(UnitInstance _skillUser, CostRequiringSkill _skill)
        {
            if (_skillUser == null || _skill == null)
                return null;

            Dictionary<_2DCoord, bool> targetArea = new Dictionary<_2DCoord, bool>();

            List<_2DCoord> realTargetArea = GetSkillTargetableArea(_skillUser, _skill);

            if (_skillUser.AreResourcesEnoughForSkillExecution(_skill))
            {
                List<_2DCoord> selectableCoords = GetEffectSelectableCoords(_skillUser, _skill, _skill.BaseInfo.Effect, realTargetArea);
                foreach (_2DCoord coord in realTargetArea)
                {
                    if (selectableCoords.Contains(coord))
                        targetArea.Add(coord, true);
                    else
                        targetArea.Add(coord, false);
                }
            }
            else
                foreach (_2DCoord coord in realTargetArea) { targetArea.Add(coord, false); }

            return targetArea;
        }
        public Dictionary<_2DCoord, bool> GetSkillTargetableAndSelectableArea(UnitInstance _skillUser, UltimateSkill _skill)
        {
            if (_skillUser == null || _skill == null)
                return null;

            Dictionary<_2DCoord, bool> targetArea = new Dictionary<_2DCoord, bool>();

            List<_2DCoord> realTargetArea = GetSkillTargetableArea(_skillUser, _skill);

            List<_2DCoord> selectableCoords = GetEffectSelectableCoords(_skillUser, _skill, _skill.BaseInfo.Effect, realTargetArea);
            foreach (_2DCoord coord in realTargetArea)
            {
                if (selectableCoords.Contains(coord))
                    targetArea.Add(coord, true);
                else
                    targetArea.Add(coord, false);
            }

            return targetArea;
        }
        public List<_2DCoord> GetSkillTargetableArea(UnitInstance _skillUser, ActiveSkill _skill)
        {
            List<_2DCoord> targetArea = new List<_2DCoord>();

            List<_2DCoord> relativeTargetArea = Calculator.RelativeTargetArea(_skillUser, false, this, _skill);

            foreach (_2DCoord relativeCoord in relativeTargetArea)
            {
                _2DCoord realTargetAreaCoord = Field.ToRealCoord(Field.UnitLocation(_skillUser), Field.RelativeCoordToCorrectDirection(_skillUser.OwnerInstance, relativeCoord));

                if (Field.IsCoordWithinBoard(realTargetAreaCoord))
                    targetArea.Add(realTargetAreaCoord);
            }

            return targetArea;
        }

        // Information required for UI
        private List<_2DCoord> GetEffectSelectableCoords(UnitInstance _effectUser, Skill _skill, Effect _effect, List<_2DCoord> _targetableCoords)
        {
            List<_2DCoord> candidateCoords = new List<_2DCoord>();

            try
            {
                if (_effect is UnitTargetingEffect) //--------------------------------------------UnitTargetingEffect-------------------------------------
                {
                    List<UnitInstance> targetPreCandidates = new List<UnitInstance>();
                    UnitTargetingEffect detailedEffect = _effect as UnitTargetingEffect;

                    targetPreCandidates = FindUnitsByTargetClassification(_effectUser, detailedEffect.TargetClassification, _targetableCoords);
                    foreach (UnitInstance targetPreCandidate in targetPreCandidates)
                    {
                        if (!(_effect is HealEffect && targetPreCandidate.RemainingHP >= Calculator.MaxHP(targetPreCandidate))) // If the remaining HP of the precandidate unit is not full at the same time that the _effect is heal effect
                        {
                            if (detailedEffect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, _effect, _targetableCoords, null, targetPreCandidate)) // If activation conditions against the unit is true
                                candidateCoords.Add(Field.UnitLocation(targetPreCandidate)); // Add coord to the list of target candidates
                        }
                    }
                }
                else if (_effect is TileTargetingEffect) //-----------------------------------------------TileTargetingEffects------------------------------------------
                {
                    List<Socket> targetPreCandidates = Field.GetSocketsInCoords(_targetableCoords);
                    TileTargetingEffect detailedEffect = _effect as TileTargetingEffect;

                    foreach (Socket targetPreCandidate in targetPreCandidates)
                    {
                        if (((_effect is MovementEffect) ? targetPreCandidate.Unit == null : true) // If the _effect isMovement effect, check whether there is no unit in the socket
                            && detailedEffect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, _effect, _targetableCoords, null, targetPreCandidate)) // If activation conditions against the socket is true
                        {
                            candidateCoords.Add(Field.SocketLocation(targetPreCandidate)); // Add socket to the list of target candidates
                        }
                    }
                }

                return candidateCoords;
            }
            catch (Exception ex)
            {
                candidateCoords.Clear();
                return candidateCoords;
            }
        }

        /// <summary>
        /// [Action Method] 
        /// PreCondition: _unit has been initialized successfully; _unit is assigned to a Socket of the Board;
        /// PostCondition: If the destination is not occupied by other Unit, _unit will be moved to destination and SP will be spent.
        /// </summary>
        /// <param name="_unit"></param>
        /// <param name="_direction"></param>
        /// <returns></returns>
        public bool MoveUnit(UnitInstance _unit, _2DCoord _destination)
        {
            _2DCoord initialLocation = Field.UnitLocation(_unit);

            if (ChangeUnitLocation(_unit, _destination)) // If moved successfully
            {
                _unit.OwnerInstance.Moved = true;

                m_eventLogs.Add(new ActionLog_Move(CurrentFullTurns, Field.GetUnitIndex(_unit), _unit.BaseInfo.Name, _unit.Nickname, initialLocation, _destination));

                return true;
            }

            return false;
        }

        // Chekck for any unexpected/wrong information before actually executing the skill.
        // No error shoud be returned if available SP and Item costs have been checked. _taretCoords must be coords included in the set of coords returned by the GetSkillTargetableAndSelectableArea() function.
        public void RequestAttack(UnitInstance _attacker, List<_2DCoord> _targetCoords)
        {
            if (!_attacker.OwnerInstance.Attacked)
            {
                int tileIndex_attackerLocation = Field.UnitLocation(_attacker).ToIndex();

                List<UnitInstance> targets = Field.GetUnitsInCoords(_targetCoords);

                foreach (UnitInstance target in targets)
                {
                    int tileIndex_targetLocation = Field.UnitLocation(target).ToIndex();

                    m_eventLogs.Add(new ActionLog_Attack(CurrentFullTurns, Field.GetUnitIndex(_attacker), _attacker.BaseInfo.Name, _attacker.Nickname, tileIndex_attackerLocation, Field.GetUnitIndex(target), tileIndex_targetLocation));

                    ActiveSkill skill = GameDataContainer.Instance.BasicAttackSkill;

                    List<_2DCoord> attackTargetableArea = GetAttackTargetableArea(_attacker);

                    ProcessStatusEffects(eEventTriggerTiming.OnActionExecuted, _attacker);
                    ProcessStatusEffects(eEventTriggerTiming.OnActiveSkillExecuted, _attacker, skill, attackTargetableArea, targets);

                    ExecuteEffect(_attacker, skill, skill.BaseInfo.Effect, attackTargetableArea, targets.Cast<object>().ToList(), target);
                }

                _attacker.OwnerInstance.Attacked = true;
            }
        }

        // Chekck for any unexpected/wrong information before actually executing the skill.
        // No error shoud be returned if available SP and Item costs have been checked. _taretCoords must be coords included in the set of coords returned by the GetSkillTargetableAndSelectableArea() function.
        // If working correctly, all targetCandidates must be eventual targets.
        public void RequestSkillUse(UnitInstance _skillUser, ActiveSkill _skill, List<_2DCoord> _targetCoords, List<_2DCoord> _secondaryTargetCoords)
        {
            if (_skillUser == null
                || _skill == null
                || _targetCoords == null
                || (_skill.BaseInfo.Effect is IComplexTargetSelectionEffect && _secondaryTargetCoords == null))
            {
                return;
            }

            // Check whether the player has enough resources to execute the skill
            if (_skill is CostRequiringSkill)
            {
                var skill = _skill as CostRequiringSkill;
                if (!_skillUser.AreResourcesEnoughForSkillExecution(skill))
                    return;
            }

            // Spend SP required to use the skill
            if (_skill is CostRequiringSkill)
                _skillUser.OwnerInstance.RemainingSP -= (_skill as CostRequiringSkill).BaseInfo.SPCost;

            int tileIndex_skillUser = Field.UnitLocation(_skillUser).ToIndex();

            List<object> targetCandidates = new List<object>();
            List<object> secondaryTargetCandidates = new List<object>();

            if (_skill.BaseInfo.Effect is UnitTargetingEffect) //----------------------------------UnitTargetingEffect-----------------
            {
                List<UnitInstance> tmp_targetCandidates = Field.GetUnitsInCoords(_targetCoords);
                List<UnitInstance> tmp_secondaryTargetCandidates = Field.GetUnitsInCoords(_secondaryTargetCoords);

                List<Tuple<string, string, string>> targetsName_Nickname_OwnerName = new List<Tuple<string, string, string>>();
                foreach (var targetCandidate in tmp_targetCandidates) { targetsName_Nickname_OwnerName.Add(new Tuple<string, string, string>(targetCandidate.BaseInfo.Name, targetCandidate.Nickname, targetCandidate.OwnerInstance.Name)); }
                List<Tuple<string, string, string>> secondaryTargetsName_Nickname_OwnerName = new List<Tuple<string, string, string>>();
                foreach (var secondaryTargetCandidate in tmp_secondaryTargetCandidates) { secondaryTargetsName_Nickname_OwnerName.Add(new Tuple<string, string, string>(secondaryTargetCandidate.BaseInfo.Name, secondaryTargetCandidate.Nickname, secondaryTargetCandidate.OwnerInstance.Name)); }

                m_eventLogs.Add(new ActionLog_UnitTargetingSkill(CurrentFullTurns, Field.GetUnitIndex(_skillUser), _skillUser.BaseInfo.Name, _skillUser.Nickname, _skill.BaseInfo.Name, tileIndex_skillUser, _skillUser.OwnerInstance.RemainingSP, _skill.BaseInfo.SkillActivationAnimationId, targetsName_Nickname_OwnerName, secondaryTargetsName_Nickname_OwnerName));
                targetCandidates = tmp_targetCandidates.Cast<object>().ToList();
                secondaryTargetCandidates = tmp_secondaryTargetCandidates.Cast<object>().ToList();
            }
            else if (_skill.BaseInfo.Effect is TileTargetingEffect) //----------------------------------TileTargetingEffect-------------------
            {
                m_eventLogs.Add(new ActionLog_TileTargetingSkill(CurrentFullTurns, Field.GetUnitIndex(_skillUser), _skillUser.BaseInfo.Name, _skillUser.Nickname, _skill.BaseInfo.Name, tileIndex_skillUser, _skillUser.OwnerInstance.RemainingSP, _skill.BaseInfo.SkillActivationAnimationId, _targetCoords, _secondaryTargetCoords));
                targetCandidates = Field.GetSocketsInCoords(_targetCoords).Cast<object>().ToList();
                secondaryTargetCandidates = Field.GetSocketsInCoords(_secondaryTargetCoords).Cast<object>().ToList();
            }

            List<_2DCoord> skillTargetableArea = GetSkillTargetableArea(_skillUser, _skill);

            ProcessStatusEffects(eEventTriggerTiming.OnActionExecuted, _skillUser);
            ProcessStatusEffects(eEventTriggerTiming.OnActiveSkillExecuted, _skillUser, _skill, skillTargetableArea, targetCandidates);

            InitiateEffectExecution(_skillUser, _skill, _skill.BaseInfo.Effect, GetSkillTargetableArea(_skillUser, _skill), targetCandidates, secondaryTargetCandidates);
        }

        private void InitiateEffectExecution(UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_effectUser == null
                || _skill == null
                || _effect == null
                || _effectRange == null
                || _targets == null
                || (_skill.BaseInfo.Effect is IComplexTargetSelectionEffect && _secondaryTargetsForComplexTargetSelectionEffect == null))
            {
                return;
            }

            foreach (object target in _targets)
            {
                ExecuteEffect(_effectUser, _skill, _effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect);
            }
        }

        /// <summary>
        /// Attempts to ExecuteEffect. Effects might fail if target protects itself, dodges the effect, etc.
        /// </summary>
        private void ExecuteEffect(UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null)
        {
            if (_effectUser == null
                || _skill == null
                || _effect == null
                || _effectRange == null
                || _target == null
                || _targets == null
                || (_skill.BaseInfo.Effect is IComplexTargetSelectionEffect && _secondaryTargetsForComplexTargetSelectionEffect == null))
            {
                return;
            }

            bool isActivationConditionTrue = _effect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            int timesToApply = _effect.TimesToApply.ToValue<int>(this, null, null, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

            _2DCoord _effectUserLocation = Field.UnitLocation(_effectUser);

            if (_effect is UnitTargetingEffect) //------------------------------------------------------------------UnitTargetingEffects----------------------------------------------------
            {
                UnitInstance target = _target as UnitInstance;
                List<UnitInstance> targets = _targets.Cast<UnitInstance>().ToList();
                List<UnitInstance> secondaryTargetsForComplexTargetSelectionEffect = _secondaryTargetsForComplexTargetSelectionEffect?.Cast<UnitInstance>().ToList();

                _2DCoord targetLocation = Field.UnitLocation(target);
                int tileIndex_targetLocation = targetLocation.ToIndex();

                if (_effect is UnitTargetingEffectsWrapperEffect) //-----------------------------------UnitTargetingEffectsWrapper-----------------------------------
                {
                    foreach (UnitTargetingEffect unitTargetingEffect in _effect.SecondaryEffects)
                    {
                        ExecuteEffect(_effectUser, _skill, unitTargetingEffect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
                    }
                }
                else if (_effect is DamageEffect) //---------------------------------------------Damage--------------------------------------------
                {
                    var effect = _effect as DamageEffect;

                    if (!isActivationConditionTrue)
                        m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, null, false, false, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, target.RemainingHP));
                    else
                    {
                        for (int i = 1; i <= timesToApply; i++)
                        {
                            if (Calculator.DoesSucceed(this, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect)) // If the effect succeeded
                            {
                                int damage = Calculator.Damage(this, _effectUser, _effectUserLocation, _skill, effect, _effectRange, targets, target, out bool isCritical, out eEffectiveness effectiveness, secondaryTargetsForComplexTargetSelectionEffect);
                                DealDamage(damage, target);

                                m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, true, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, isCritical, effectiveness, damage, target.RemainingHP));

                                // Execute weakened effect against adjacent units
                                int diffusionDistance = _effect.DiffusionDistance.ToValue<int>(this, null, null, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect);
                                if (diffusionDistance > 0)
                                    ExecuteDiffusedDamageEffect(diffusionDistance, _effectUser, _effectUserLocation, _skill, effect, _effectRange, _targets, targets, targetLocation, _secondaryTargetsForComplexTargetSelectionEffect, secondaryTargetsForComplexTargetSelectionEffect);

                                // Execute Secondary Effects
                                if (_effect.SecondaryEffects.Count > 0)
                                {
                                    foreach (Effect secondaryEffect in _effect.SecondaryEffects)
                                    {
                                        ExecuteEffect(_effectUser, _skill, secondaryEffect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
                                    }
                                }
                            }
                            else // If the effect failed
                                m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, target.RemainingHP));
                        }
                    }
                }
                else if (_effect is DrainEffect) //-----------------------------------------Drain(Damage + Heal)---------------------------------------------
                {
                    var effect = _effect as DrainEffect;

                    if (!isActivationConditionTrue)
                        m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, null, false, false, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, target.RemainingHP));
                    else
                    {
                        for (int i = 1; i <= timesToApply; i++)
                        {
                            if (Calculator.DoesSucceed(this, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect)) // If the effect succeeded
                            {
                                int damage = Calculator.Damage(this, _effectUser, _effectUserLocation, _skill, new DamageEffect(effect), _effectRange, targets, target, out bool isCritical, out eEffectiveness effectiveness, secondaryTargetsForComplexTargetSelectionEffect);
                                DealDamage(damage, target);

                                m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, true, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, isCritical, effectiveness, damage, target.RemainingHP));

                                decimal drainingEfficiency = effect.DrainingEfficiency.ToValue<decimal>(this, null, null, _effectUser, _skill, effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
                                foreach (UnitInstance secondaryTarget in _secondaryTargetsForComplexTargetSelectionEffect.Cast<UnitInstance>())
                                {
                                    int restoringAmount = Convert.ToInt32(Math.Floor((damage * drainingEfficiency) / _secondaryTargetsForComplexTargetSelectionEffect.Count));
                                    RestoreHP(restoringAmount, secondaryTarget);

                                    m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, true, Field.GetUnitIndex(secondaryTarget), secondaryTarget.BaseInfo.Name, secondaryTarget.Nickname, Field.UnitLocation(secondaryTarget).ToIndex(), false, restoringAmount, secondaryTarget.RemainingHP));
                                }

                                // Execute weakened effect against adjacent units
                                int diffusionDistance = _effect.DiffusionDistance.ToValue<int>(this, null, null, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect);
                                if (diffusionDistance > 0)
                                    ExecuteDiffusedDrainEffect(diffusionDistance, _effectUser, _effectUserLocation, _skill, effect, _effectRange, _targets, targets, target, targetLocation, _secondaryTargetsForComplexTargetSelectionEffect, secondaryTargetsForComplexTargetSelectionEffect);

                                // Execute Secondary Effects
                                if (_effect.SecondaryEffects.Count > 0)
                                {
                                    foreach (Effect secondaryEffect in _effect.SecondaryEffects)
                                    {
                                        ExecuteEffect(_effectUser, _skill, secondaryEffect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
                                    }
                                }
                            }
                            else // If the effect failed
                                m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, target.RemainingHP));
                        }
                    }
                }
                else if (_effect is HealEffect) //-------------------------------------------Heal---------------------------------------------
                {
                    var effect = _effect as HealEffect;

                    if (!isActivationConditionTrue)
                        m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, null, false, false, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, default, target.RemainingHP));
                    else
                    {
                        if (Calculator.DoesSucceed(this, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect)) // If the effect succeeded
                        {
                            int restoringAmount = Calculator.HealValue(this, _effectUser, _effectUserLocation, _skill, effect, _effectRange, targets, target, out bool isCritical, secondaryTargetsForComplexTargetSelectionEffect);
                            RestoreHP(restoringAmount, target);

                            m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, true, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, isCritical, restoringAmount, target.RemainingHP));

                            // Execute weakened effect against adjacent units
                            int diffusionDistance = _effect.DiffusionDistance.ToValue<int>(this, null, null, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect);
                            if (diffusionDistance > 0)
                                ExecuteDiffusedHealEffect(diffusionDistance, _effectUser, _effectUserLocation, _skill, effect, _effectRange, _targets, targets, targetLocation, _secondaryTargetsForComplexTargetSelectionEffect, secondaryTargetsForComplexTargetSelectionEffect);

                            // Execute Secondary Effects
                            if (_effect.SecondaryEffects.Count > 0)
                            {
                                foreach (Effect secondaryEffect in _effect.SecondaryEffects)
                                {
                                    ExecuteEffect(_effectUser, _skill, secondaryEffect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
                                }
                            }
                        }
                        else
                            m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, false, default, target.RemainingHP));
                    }
                }
                else if (_effect is StatusEffectAttachmentEffect) //-------------------------------------StatusEffectAttachment--------------------------------------------
                {
                    var effect = _effect as StatusEffectAttachmentEffect;

                    if (!isActivationConditionTrue)
                        m_eventLogs.Add(new EffectTrialLog_StatusEffectAttachmentEffect(CurrentFullTurns, null, false, false, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, default));
                    else
                    {
                        if (Calculator.DoesSucceed(this, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect)) // If the effect succeeded
                        {
                            StatusEffect statusEffect = null;
                            if (effect.DataOfStatusEffectToAttach is BuffStatusEffectData)
                            {
                                var data = effect.DataOfStatusEffectToAttach as BuffStatusEffectData;
                                statusEffect = new BuffStatusEffect(data, _effectUser, this, _effectUser, _skill, effect, _effectRange, _targets, target, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (effect.DataOfStatusEffectToAttach is DebuffStatusEffectData)
                            {
                                var data = effect.DataOfStatusEffectToAttach as DebuffStatusEffectData;
                                statusEffect = new DebuffStatusEffect(data, _effectUser, this, _effectUser, _skill, effect, _effectRange, _targets, target, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (effect.DataOfStatusEffectToAttach is TargetRangeModStatusEffectData)
                            {
                                var data = effect.DataOfStatusEffectToAttach as TargetRangeModStatusEffectData;
                                statusEffect = new TargetRangeModStatusEffect(data, _effectUser, this, _effectUser, _skill, effect, _effectRange, _targets, target, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (effect.DataOfStatusEffectToAttach is DamageStatusEffectData)
                            {
                                var data = effect.DataOfStatusEffectToAttach as DamageStatusEffectData;
                                statusEffect = new DamageStatusEffect(data, _effectUser, this, _effectUser, _skill, effect, _effectRange, _targets, target, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (effect.DataOfStatusEffectToAttach is HealStatusEffectData)
                            {
                                var data = effect.DataOfStatusEffectToAttach as HealStatusEffectData;
                                statusEffect = new HealStatusEffect(data, _effectUser, this, _effectUser, _skill, effect, _effectRange, _targets, target, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }

                            AttachEffect(statusEffect, target);

                            m_eventLogs.Add(new EffectTrialLog_StatusEffectAttachmentEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, true, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, effect.DataOfStatusEffectToAttach.Id));

                            // Execute weakened effect against adjacent units
                            int diffusionDistance = _effect.DiffusionDistance.ToValue<int>(this, null, null, _effectUser, _skill, effect, _effectRange, _targets, target, _secondaryTargetsForComplexTargetSelectionEffect);
                            if (diffusionDistance > 0)
                                ExecuteDiffusedStatusEffectAttachmentEffect(diffusionDistance, _effectUser, _effectUserLocation, _skill, effect, _effectRange, _targets, targetLocation, _secondaryTargetsForComplexTargetSelectionEffect);

                            // Execute Secondary Effects
                            if (_effect.SecondaryEffects.Count > 0)
                            {
                                foreach (Effect secondaryEffect in _effect.SecondaryEffects)
                                {
                                    ExecuteEffect(_effectUser, _skill, secondaryEffect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
                                }
                            }
                        }
                        else // If the effect failed
                            m_eventLogs.Add(new EffectTrialLog_StatusEffectAttachmentEffect(CurrentFullTurns, _effect.AnimationInfo, false, true, false, Field.GetUnitIndex(target), target.BaseInfo.Name, target.Nickname, tileIndex_targetLocation, default));
                    }
                }
            }
            else if (_effect is TileTargetingEffect) //----------------------------------------------------------------TileTargetingEffects-----------------------------------------------------
            {
                var target = (Socket)_target;

                if (_effect is MovementEffect) //------------------------------------------Movement-------------------------------------------------
                {
                    var effect = _effect as MovementEffect;

                    if (!isActivationConditionTrue)
                        m_eventLogs.Add(new EffectTrialLog_MovementEffect(CurrentFullTurns, null, false, null, null));
                    else
                    {
                        _2DCoord destination = Field.SocketLocation(target);

                        for (int i = 1; i <= timesToApply; i++)
                        {
                            if (i.IsOdd())
                            {
                                MoveUnit(_effectUser, destination);
                                m_eventLogs.Add(new EffectTrialLog_MovementEffect(CurrentFullTurns, _effect.AnimationInfo as MovementAnimationInfo, true, destination, _effectUserLocation));
                            }
                            else
                            {
                                MoveUnit(_effectUser, _effectUserLocation);
                                m_eventLogs.Add(new EffectTrialLog_MovementEffect(CurrentFullTurns, _effect.AnimationInfo as MovementAnimationInfo, true, _effectUserLocation, destination));
                            }

                            if (_effect.SecondaryEffects.Count > 0)
                            {
                                List<_2DCoord> steppedCoords = Calculator.CoordsBetween(_effectUserLocation, destination);
                                steppedCoords.Add(_effectUserLocation);
                                steppedCoords.Add(destination);

                                // Execute Secondary Effects
                                foreach (Effect secondaryEffect in _effect.SecondaryEffects)
                                {
                                    List<object> targetsInSteppedCoords = new List<object>();
                                    if (secondaryEffect is UnitTargetingEffect)
                                    {
                                        List<UnitInstance> unitsInSteppedCoords = FindUnitsByTargetClassification(_effectUser, (secondaryEffect as UnitTargetingEffect).TargetClassification, steppedCoords);
                                        foreach (UnitInstance unitInSteppedCoords in unitsInSteppedCoords)
                                        {
                                            if (secondaryEffect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, secondaryEffect, _effectRange, unitsInSteppedCoords.Cast<object>().ToList(), unitInSteppedCoords))
                                                targetsInSteppedCoords.Add(unitInSteppedCoords);
                                        }
                                    }
                                    else if (secondaryEffect is TileTargetingEffect)
                                    {
                                        List<Socket> socketsInSteppedCoords = Field.GetSocketsInCoords(steppedCoords);
                                        foreach (Socket socketInSteppedCoords in socketsInSteppedCoords)
                                        {
                                            if (secondaryEffect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, secondaryEffect, _effectRange, socketsInSteppedCoords.Cast<object>().ToList(), socketInSteppedCoords))
                                                targetsInSteppedCoords.Add(socketInSteppedCoords);
                                        }
                                    }

                                    foreach (object targetInSteppedCoords in targetsInSteppedCoords)
                                    {
                                        ExecuteEffect(_effectUser, _skill, secondaryEffect, _effectRange, targetsInSteppedCoords, targetInSteppedCoords);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteDiffusedDamageEffect(int _diffusionDistance, UnitInstance _effectUser, _2DCoord _effectUserLocation, ActiveSkill _skill, DamageEffect _effect, List<_2DCoord> _effectRange, List<object> _targetsAsObjects, List<UnitInstance> _targetsAsUnitInstances, _2DCoord _targetLocation, List<object> _secondaryTargetsAsObjects, List<UnitInstance> _secondaryTargetsAsUnitInstances)
        {
            for (int distance = _diffusionDistance; distance >= 1; distance--)
            {
                List<_2DCoord> coordsInDistance = Calculator.CoordsInDistance(_targetLocation, distance);
                foreach (UnitInstance targetCandidate in Field.GetUnitsInCoords(coordsInDistance))
                {
                    int tileIndex_targetLocation = Field.UnitLocation(targetCandidate).ToIndex();

                    if (_effect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsAsObjects))
                    {
                        if (Calculator.DoesSucceed(this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsAsObjects)) // If the effect succeeded
                        {
                            int damage = Calculator.Damage(this, _effectUser, _effectUserLocation, _skill, _effect, _effectRange, _targetsAsUnitInstances, targetCandidate, out bool isCritical, out eEffectiveness effectiveness, _secondaryTargetsAsUnitInstances, distance);
                            DealDamage(damage, targetCandidate);

                            m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, true, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, isCritical, effectiveness, damage, targetCandidate.RemainingHP));
                        }
                        else // If the effect failed
                            m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, targetCandidate.RemainingHP));
                    }
                    else // If the effect did not meet the activation requirements
                        m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, null, true, false, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, targetCandidate.RemainingHP));
                }
            }
        }

        private void ExecuteDiffusedDrainEffect(int _diffusionDistance, UnitInstance _effectUser, _2DCoord _effectUserLocation, ActiveSkill _skill, DrainEffect _effect, List<_2DCoord> _effectRange, List<object> _targetsAsObjects, List<UnitInstance> _targetsAsUnitInstances, UnitInstance _target, _2DCoord _targetLocation, List<object> _secondaryTargetsAsObjects, List<UnitInstance> _secondaryTargetsAsUnitInstances)
        {
            for (int distance = _diffusionDistance; distance >= 1; distance--)
            {
                List<_2DCoord> coordsInDistance = Calculator.CoordsInDistance(_targetLocation, distance);
                foreach (UnitInstance targetCandidate in Field.GetUnitsInCoords(coordsInDistance))
                {
                    int tileIndex_targetLocation = Field.UnitLocation(targetCandidate).ToIndex();

                    if (_effect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsAsObjects))
                    {
                        if (Calculator.DoesSucceed(this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsAsObjects)) // If the effect succeeded
                        {
                            int damage = Calculator.Damage(this, _effectUser, _effectUserLocation, _skill, new DamageEffect(_effect), _effectRange, _targetsAsUnitInstances, targetCandidate, out bool isCritical, out eEffectiveness effectiveness, _secondaryTargetsAsUnitInstances, distance);
                            DealDamage(damage, targetCandidate);

                            m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, true, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, isCritical, effectiveness, damage, targetCandidate.RemainingHP));

                            decimal drainingEfficiency = _effect.DrainingEfficiency.ToValue<decimal>(this, null, null, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, _target, _secondaryTargetsAsObjects);
                            foreach (UnitInstance secondaryTarget in _secondaryTargetsAsUnitInstances)
                            {
                                int restoringAmount = Convert.ToInt32(Math.Floor((damage * drainingEfficiency) / _secondaryTargetsAsUnitInstances.Count));
                                RestoreHP(restoringAmount, secondaryTarget);

                                m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, true, Field.GetUnitIndex(secondaryTarget), secondaryTarget.BaseInfo.Name, secondaryTarget.Nickname, Field.UnitLocation(secondaryTarget).ToIndex(), false, restoringAmount, secondaryTarget.RemainingHP));
                            }
                        }
                        else // If the effect failed
                            m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, targetCandidate.RemainingHP));
                    }
                    else // If the effect did not meet the activation requirements
                        m_eventLogs.Add(new EffectTrialLog_DamageEffect(CurrentFullTurns, null, true, false, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, false, eEffectiveness.Neutral, default, targetCandidate.RemainingHP));
                }
            }
        }

        private void ExecuteDiffusedHealEffect(int _diffusionDistance, UnitInstance _effectUser, _2DCoord _effectUserLocation, ActiveSkill _skill, HealEffect _effect, List<_2DCoord> _effectRange, List<object> _targetsAsObjects, List<UnitInstance> _targetsAsUnitInstances, _2DCoord _targetLocation, List<object> _secondaryTargetsAsObjects, List<UnitInstance> _secondaryTargetsAsUnitInstances)
        {
            for (int distance = _diffusionDistance; distance >= 1; distance--)
            {
                List<_2DCoord> coordsInDistance = Calculator.CoordsInDistance(_targetLocation, distance);
                foreach (UnitInstance targetCandidate in Field.GetUnitsInCoords(coordsInDistance))
                {
                    int tileIndex_targetLocation = Field.UnitLocation(targetCandidate).ToIndex();

                    if (_effect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsAsObjects))
                    {
                        if (Calculator.DoesSucceed(this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsAsObjects)) // If the effect succeeded
                        {
                            int restoringAmount = Calculator.HealValue(this, _effectUser, _effectUserLocation, _skill, _effect, _effectRange, _targetsAsUnitInstances, targetCandidate, out bool isCritical, _secondaryTargetsAsUnitInstances, distance);
                            RestoreHP(restoringAmount, targetCandidate);

                            m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, true, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, isCritical, restoringAmount, targetCandidate.RemainingHP));
                        }
                        else // If the effect failed
                            m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, default, targetCandidate.RemainingHP));
                    }
                    else // If the effect did not meet the activation requirements
                        m_eventLogs.Add(new EffectTrialLog_HealEffect(CurrentFullTurns, null, true, false, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, false, default, targetCandidate.RemainingHP));
                }
            }
        }

        private void ExecuteDiffusedStatusEffectAttachmentEffect(int _diffusionDistance, UnitInstance _effectUser, _2DCoord _effectUserLocation, ActiveSkill _skill, StatusEffectAttachmentEffect _effect, List<_2DCoord> _effectRange, List<object> _targetsAsObjects, _2DCoord _targetLocation, List<object> _secondaryTargetsForComplexTargetSelectionEffect)
        {
            for (int distance = _diffusionDistance; distance >= 1; distance--)
            {
                List<_2DCoord> coordsInDistance = Calculator.CoordsInDistance(_targetLocation, distance);
                foreach (UnitInstance targetCandidate in Field.GetUnitsInCoords(coordsInDistance))
                {
                    int tileIndex_targetLocation = Field.UnitLocation(targetCandidate).ToIndex();

                    if (_effect.ActivationCondition.IsTrue(this, null, null, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsForComplexTargetSelectionEffect))
                    {
                        if (Calculator.DoesSucceed(this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _secondaryTargetsForComplexTargetSelectionEffect)) // If the _effect succeeded
                        {
                            StatusEffect statusEffect = null;
                            if (_effect.DataOfStatusEffectToAttach is BuffStatusEffectData)
                            {
                                var data = _effect.DataOfStatusEffectToAttach as BuffStatusEffectData;
                                statusEffect = new BuffStatusEffect(data, _effectUser, this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (_effect.DataOfStatusEffectToAttach is DebuffStatusEffectData)
                            {
                                var data = _effect.DataOfStatusEffectToAttach as DebuffStatusEffectData;
                                statusEffect = new DebuffStatusEffect(data, _effectUser, this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (_effect.DataOfStatusEffectToAttach is TargetRangeModStatusEffectData)
                            {
                                var data = _effect.DataOfStatusEffectToAttach as TargetRangeModStatusEffectData;
                                statusEffect = new TargetRangeModStatusEffect(data, _effectUser, this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (_effect.DataOfStatusEffectToAttach is DamageStatusEffectData)
                            {
                                var data = _effect.DataOfStatusEffectToAttach as DamageStatusEffectData;
                                statusEffect = new DamageStatusEffect(data, _effectUser, this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }
                            else if (_effect.DataOfStatusEffectToAttach is HealStatusEffectData)
                            {
                                var data = _effect.DataOfStatusEffectToAttach as HealStatusEffectData;
                                statusEffect = new HealStatusEffect(data, _effectUser, this, _effectUser, _skill, _effect, _effectRange, _targetsAsObjects, targetCandidate, _skill.Level, _secondaryTargetsForComplexTargetSelectionEffect);
                            }

                            int activationTimes = statusEffect.Duration.ActivationTimes;
                            statusEffect.Duration.ActivationTimes = (activationTimes - distance < 0) ? 0 : activationTimes - distance;
                            decimal turns = statusEffect.Duration.Turns;
                            statusEffect.Duration.Turns = (turns - distance < 0) ? 0 : turns - distance;

                            AttachEffect(statusEffect, targetCandidate);

                            m_eventLogs.Add(new EffectTrialLog_StatusEffectAttachmentEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, true, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, _effect.DataOfStatusEffectToAttach.Id));
                        }
                        else // If the effect failed
                            m_eventLogs.Add(new EffectTrialLog_StatusEffectAttachmentEffect(CurrentFullTurns, _effect.AnimationInfo, true, true, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, default));
                    }
                    else // If the effect did not meet the activation requirements
                        m_eventLogs.Add(new EffectTrialLog_StatusEffectAttachmentEffect(CurrentFullTurns, null, true, false, false, Field.GetUnitIndex(targetCandidate), targetCandidate.BaseInfo.Name, targetCandidate.Nickname, tileIndex_targetLocation, default));
                }
            }
        }

        private bool DoesActivationTurnClassificationMatch(PlayerOnBoard _player, eActivationTurnClassification _actionTurnClassification)
        {
            switch (_actionTurnClassification)
            {
                default: //case eBackgroundActivationTiming.Always
                    return true;
                case eActivationTurnClassification.OnMyTurn:
                    return _player == CurrentTurnPlayer;
                case eActivationTurnClassification.OnOpponentTurn:
                    return _player != CurrentTurnPlayer;
            }
        }

        private bool DoesEventTriggerTimingMatchGamePhase(PlayerOnBoard _player, eEventTriggerTiming _eventTriggerTiming)
        {
            switch (_eventTriggerTiming)
            {
                case eEventTriggerTiming.OnStatusEffectActivated:
                    return true; // It can be triggered at any game phase

                case eEventTriggerTiming.BeginningOfMatch:
                    return CurrentPhase == eGamePhase.BeginningOfMatch;

                case eEventTriggerTiming.BeginningOfTurn:
                    return CurrentPhase == eGamePhase.BeginningOfTurn;

                default: // Any value other than the ones listed above or below is considered as DuringTurn
                    return CurrentPhase == eGamePhase.DuringTurn;

                case eEventTriggerTiming.EndOfTurn:
                    return CurrentPhase == eGamePhase.EndOfTurn;
            }
        }

        private void ExecuteStatusEffectsIfExecutable(eEventTriggerTiming _eventTriggerTiming, UnitInstance _effectHolder, ForegroundStatusEffect _statusEffect, params object[] _params)
        {
            try
            {
                switch (_eventTriggerTiming)
                {
                    case eEventTriggerTiming.OnActionExecuted:
                        {
                            UnitInstance actor = _params[0] as UnitInstance;

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, actor))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, actor);
                        }
                        break;

                    case eEventTriggerTiming.OnMoved:
                        {
                            UnitInstance actor = _params[0] as UnitInstance;
                            int actor_previousLocationTileIndex = Convert.ToInt32(_params[1]);

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, actor, null, null, null, null, null, null, 0, actor_previousLocationTileIndex))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, actor, null, null, null, null, null, null, 0, actor_previousLocationTileIndex);
                        }
                        break;

                    case eEventTriggerTiming.OnAttackExecuted:
                    case eEventTriggerTiming.OnTargetedByAction:
                    case eEventTriggerTiming.OnTargetedByAttack:
                        {
                            UnitInstance actor = _params[0] as UnitInstance;
                            ActiveSkill skill = _params[1] as ActiveSkill;
                            List<_2DCoord> effectRange = _params[2].ToList<_2DCoord>();
                            List<object> targets = _params[3].ToList<object>();

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, actor, skill, null, effectRange, targets))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, actor, skill, null, effectRange, targets);
                        }
                        break;

                    case eEventTriggerTiming.OnActiveSkillExecuted:
                    case eEventTriggerTiming.OnItemUsed:
                        {
                            UnitInstance actor = _params[0] as UnitInstance;
                            ActiveSkill skill = _params[1] as ActiveSkill;
                            List<_2DCoord> effectRange = _params[2].ToList<_2DCoord>();
                            List<object> targets = _params[3].ToList<object>();
                            List<object> secondaryTargetsForComplexTargetSelectionEffect = _params[6].ToList<object>();

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, actor, skill, null, effectRange, targets, null, secondaryTargetsForComplexTargetSelectionEffect))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, actor, skill, null, effectRange, targets, null, secondaryTargetsForComplexTargetSelectionEffect);
                        }
                        break;

                    case eEventTriggerTiming.OnTargetedBySkill:
                    case eEventTriggerTiming.OnTargetedByItemSkill:
                        {
                            UnitInstance actor = _params[0] as UnitInstance;
                            ActiveSkill skill = _params[1] as ActiveSkill;
                            List<_2DCoord> effectRange = _params[2].ToList<_2DCoord>();
                            List<object> targets = _params[3].ToList<object>();
                            object target = _params[5];
                            List<object> secondaryTargetsForComplexTargetSelectionEffect = _params[6].ToList<object>();

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, actor, skill, null, effectRange, targets, target, secondaryTargetsForComplexTargetSelectionEffect))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, actor, skill, null, effectRange, targets, target, secondaryTargetsForComplexTargetSelectionEffect);
                        }
                        break;

                    case eEventTriggerTiming.OnEffectSuccess:
                    case eEventTriggerTiming.OnHitByEffect:
                        {
                            UnitInstance actor = _params[0] as UnitInstance;
                            ActiveSkill skill = _params[1] as ActiveSkill;
                            Effect effect = _params[2] as Effect;
                            List<_2DCoord> effectRange = _params[3].ToList<_2DCoord>();
                            List<object> targets = _params[4]?.ToList<object>();
                            object target = _params[5];
                            List<object> secondaryTargetsForComplexTargetSelectionEffect = _params[6].ToList<object>();
                            int target_previousRemainingHP = Convert.ToInt32(_params[7]);
                            int target_previousLocationTileIndex = Convert.ToInt32(_params[8]);
                            List<StatusEffect> statusEffects = _params[9].ToList<StatusEffect>();
                            eTileType previousTileType = (eTileType)_params[10];

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, actor, skill, effect, effectRange, targets, target, secondaryTargetsForComplexTargetSelectionEffect, target_previousRemainingHP, target_previousLocationTileIndex, statusEffects, null, null, previousTileType))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, actor, skill, effect, effectRange, targets, target, secondaryTargetsForComplexTargetSelectionEffect, target_previousRemainingHP, target_previousLocationTileIndex, statusEffects, null, null, previousTileType);
                        }
                        break;

                    case eEventTriggerTiming.OnEffectFailure:
                    case eEventTriggerTiming.OnEvadedEffect:
                        {
                            UnitInstance actor = _params[0] as UnitInstance;
                            ActiveSkill skill = _params[1] as ActiveSkill;
                            Effect effect = _params[2] as Effect;
                            List<_2DCoord> effectRange = _params[3].ToList<_2DCoord>();
                            List<object> targets = _params[4]?.ToList<object>();
                            object target = _params[5];
                            List<object> secondaryTargetsForComplexTargetSelectionEffect = _params[6].ToList<object>();
                            bool applyEffectToSecondaryTargets = Convert.ToBoolean(_params[7]);

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, actor, skill, effect, effectRange, targets, target, secondaryTargetsForComplexTargetSelectionEffect))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, actor, skill, effect, effectRange, targets, target, secondaryTargetsForComplexTargetSelectionEffect);
                        }
                        break;

                    case eEventTriggerTiming.OnStatusEffectActivated:
                        {
                            UnitInstance effectHolderOfActivatedEffect = _params[0] as UnitInstance;
                            StatusEffect activatedStatusEffect = _params[1] as StatusEffect; // The StatusEffect that triggered OnStatusEffectActivated

                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect, null, null, null, null, null, null, null, 0, 0, null, effectHolderOfActivatedEffect, activatedStatusEffect))
                                ExecuteStatusEffect(_effectHolder, _statusEffect, null, null, null, null, null, null, null, 0, 0, null, effectHolderOfActivatedEffect, activatedStatusEffect);
                        }
                        break;

                    default: // BeginningOfMatch, BeginningOfTurn, and EndOfTurn
                        {
                            if (_statusEffect.ActivationCondition.IsTrue(this, _effectHolder, _statusEffect))
                                ExecuteStatusEffect(_effectHolder, _statusEffect);
                        }
                        break;
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void ExecuteStatusEffect(UnitInstance _effectHolder, ForegroundStatusEffect _statusEffect, UnitInstance _actor = null, ActiveSkill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null, int _targetPreviousHP = 0, int _targetPreviousLocationTileIndex = 0, List<StatusEffect> _statusEffects = null, UnitInstance _effectHolderOfActivatedEffect = null, StatusEffect _statusEffectActivated = null, eTileType _previousTileType = default(eTileType))
        {
            if (_statusEffect is DamageStatusEffect)
            {
                DamageStatusEffect damageStatusEffect = _statusEffect as DamageStatusEffect;

                decimal damage = damageStatusEffect.Damage.ToValue<decimal>(this, _effectHolder, damageStatusEffect, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _targetPreviousHP, _targetPreviousLocationTileIndex, _statusEffects, _effectHolderOfActivatedEffect, _statusEffectActivated, _previousTileType);

                Calculator.ApplyBuffAndDebuffStatusEffects_Simple(ref damage, this, _effectHolder, eStatusType.DamageResistance, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

                DealDamage(Convert.ToInt32(damage), _effectHolder);

                m_eventLogs.Add(new StatusEffectLog_HPModification(CurrentFullTurns, Field.GetUnitIndex(_effectHolder), _effectHolder.BaseInfo.Name, _effectHolder.Nickname, false, Convert.ToInt32(damage), _effectHolder.RemainingHP));
            }
            else if (_statusEffect is HealStatusEffect)
            {
                HealStatusEffect healStatusEffect = _statusEffect as HealStatusEffect;

                decimal hpAmount = healStatusEffect.HPAmount.ToValue<decimal>(this, _effectHolder, healStatusEffect, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _targetPreviousHP, _targetPreviousLocationTileIndex, _statusEffects, _effectHolderOfActivatedEffect, _statusEffectActivated, _previousTileType);

                Calculator.ApplyBuffAndDebuffStatusEffects_Simple(ref hpAmount, this, _effectHolder, eStatusType.FixedHeal_Self, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);

                RestoreHP(Convert.ToInt32(hpAmount), _effectHolder);

                m_eventLogs.Add(new StatusEffectLog_HPModification(CurrentFullTurns, Field.GetUnitIndex(_effectHolder), _effectHolder.BaseInfo.Name, _effectHolder.Nickname, true, Convert.ToInt32(hpAmount), _effectHolder.RemainingHP));
            }

            if (_statusEffect.Duration.ActivationTimes > 0)
                _statusEffect.Duration.ActivationTimes--; // Subtract one from the remaining activation times
        }

        public bool DoesStatusEffectActivationPhaseMatch(BackgroundStatusEffectData _statusEffectData, UnitInstance _effectHolder)
        {
            return DoesActivationTurnClassificationMatch(_effectHolder.OwnerInstance, _statusEffectData.ActivationTurnClassification);
        }
        public bool DoesStatusEffectActivationPhaseMatch(ForegroundStatusEffectData _statusEffectData, UnitInstance _effectHolder, eEventTriggerTiming _eventTriggerTiming)
        {
            return DoesActivationTurnClassificationMatch(_effectHolder.OwnerInstance, _statusEffectData.ActivationTurnClassification)
                    && _statusEffectData.EventTriggerTiming == _eventTriggerTiming
                    && DoesEventTriggerTimingMatchGamePhase(_effectHolder.OwnerInstance, _statusEffectData.EventTriggerTiming);
        }
        private bool DoesStatusEffectActivationPhaseMatch(ForegroundStatusEffect _statusEffect, UnitInstance _effectHolder, eEventTriggerTiming _eventTriggerTiming)
        {
            return DoesActivationTurnClassificationMatch(_effectHolder.OwnerInstance, _statusEffect.ActivationTurnClassification)
                    && _statusEffect.EventTriggerTiming == _eventTriggerTiming
                    && DoesEventTriggerTimingMatchGamePhase(_effectHolder.OwnerInstance, _statusEffect.EventTriggerTiming);
        }

        // To be used in order to execute ForegroundStatusEffects
        private void ProcessStatusEffects(eEventTriggerTiming _eventTriggerTiming, params object[] _params)
        {
            foreach (UnitInstance u in Field.Units)
            {
                if (u.StatusEffects.Count > 0)
                {
                    List<ForegroundStatusEffect> statusEffects = u.StatusEffects.OfType<ForegroundStatusEffect>()
                        .Where(x => DoesStatusEffectActivationPhaseMatch(x, u, _eventTriggerTiming)).ToList();

                    Calculator.AddPassiveSkillForegroundStatusEffects(statusEffects, this, u, _eventTriggerTiming);
                    Calculator.AddEquipmentForegroundStatusEffects(statusEffects, this, u, _eventTriggerTiming);

                    foreach (ForegroundStatusEffect se in statusEffects)
                    {
                        ExecuteStatusEffectsIfExecutable(_eventTriggerTiming, u, se, _params);
                    }

                    RemoveExpiredStatusEffect(u);
                }
            }
        }

        public void UpdateStatusEffects()
        {
            foreach (UnitInstance u in Field.Units)
            {
                if (u.StatusEffects.Count > 0)
                {
                    DecreaseStatusEffectsDurationTurns(u);
                    RemoveExpiredStatusEffect(u);
                }
            }
        }

        private void DecreaseStatusEffectsDurationTurns(UnitInstance _unit)
        {
            if (CurrentPhase == eGamePhase.EndOfTurn)
            {
                foreach (StatusEffect se in _unit.StatusEffects.Where(x => x.Duration.Turns > 0)) // For each status effect that could be activated during more than one player turn
                {
                    se.Duration.Turns -= 0.5m; // Subtract one player turn
                }
            }
        }

        private void RemoveExpiredStatusEffect(UnitInstance _unit)
        {
            List<StatusEffect> removingStatusEffects = new List<StatusEffect>();
            foreach (StatusEffect se in _unit.StatusEffects)
            {
                if (se.Duration.ActivationTimes <= 0
                    && se.Duration.Turns <= 0
                    && (se.Duration.WhileCondition.ConditionSets.Count == 0 
                            || (se.Duration.WhileCondition.ConditionSets.Count > 0 && !se.Duration.WhileCondition.IsTrue(this, _unit, se))))
                {
                    removingStatusEffects.Add(se); // Lifetime of the status effect has ended and, thus, remove it
                }
            }

            foreach (StatusEffect se in removingStatusEffects)
            {
                _unit.StatusEffects.Remove(se);
            }
        }

        /// <summary>
        /// PreCondition: _player has been initialized successfully; At least one of the units in _player.AlliedUnits isAlive; _unitIndex matches the index of a unit in _player.AlliedUnits and the unit isAlive;
        /// PostCondtion: If succeeded, assigns _unitIndex to _player.Id_SelectedUnit. Calls EndMatch() in case no units owned by _player isAlive.
        /// </summary>
        /// <param name="_player"></param>
        /// <param name="_alliedUnitIndex"></param>
        public void ChangeSelectedUnit(PlayerOnBoard _player, int _alliedUnitIndex)
        {
            if (_alliedUnitIndex >= _player.AlliedUnits.Count
                || _alliedUnitIndex < 0)
            {
                UnselectUnits(_player);
                return;
            }

            if (_player.AlliedUnits[_alliedUnitIndex].IsAlive)
            {
                _player.SelectedUnitIndex = _alliedUnitIndex;
                return;
            }

            List<UnitInstance> unitsAlive = _player.AlliedUnits.Where(x => x.IsAlive == true).ToList();

            if (unitsAlive.Count == 0)
            {
                EndMatch(_player);
                return;
            }
        }

        public void ChangeTurn()
        {
            CurrentPhase = eGamePhase.EndOfTurn;
            ProcessStatusEffects(eEventTriggerTiming.EndOfTurn);
            UpdateStatusEffects();

            // Values used for event log
            decimal eventTurn = CurrentFullTurns;
            int turnEndingPlayerId = CurrentTurnPlayer.IsPlayer1 ? 1 : 2;
            int turnInitiatingPlayerId = CurrentTurnPlayer.IsPlayer1 ? 2 : 1;
            int remainingSPForTurnEndingPlayer = CurrentTurnPlayer.RemainingSP;
            // End values used for event log

            if (CurrentTurnPlayer == Field.Players[0])
            {
                CurrentTurnPlayer = Field.Players[1];
                CurrentPlayerTurns[1]++;
            }
            else
            {
                CurrentTurnPlayer = Field.Players[0];
                CurrentPlayerTurns[0]++;
            }

            UpdateActionSelectionStatus(CurrentTurnPlayer);
            UpdateSP(CurrentTurnPlayer);

            // Values used for event log
            int remainingSPForTurnInitiatingPlayer = CurrentTurnPlayer.RemainingSP;
            // End values used for event log

            m_eventLogs.Add(new TurnChangeEventLog(eventTurn, turnEndingPlayerId, turnInitiatingPlayerId, remainingSPForTurnEndingPlayer, remainingSPForTurnInitiatingPlayer)); // Log to indicate turn transition

            CurrentPhase = eGamePhase.BeginningOfTurn;
            ProcessStatusEffects(eEventTriggerTiming.BeginningOfTurn);
        }

        public void EndMatch(PlayerOnBoard _loser)
        {
            IsMatchEnd = true;

            if (Field.Players[0] == _loser)
                IsPlayer1Winner = false;
            else
                IsPlayer1Winner = true;
        }
        #endregion

        #region Public Methods (Command)
        public List<UnitInstance> FindUnitsByName(string _name, bool _negateStringMatchType, eStringMatchType _stringMatchType, List<UnitInstance> _units = null)
        {
            List<UnitInstance> units;

            if (_units == null)
                units = Field.Units;
            else
                units = _units;

            switch (_stringMatchType)
            {
                default: //ExactMatch
                    if (_negateStringMatchType)
                        return units.FindAll(x => !x.BaseInfo.Name.Equals(_name));
                    else return units.FindAll(x => x.BaseInfo.Name.Equals(_name));
                case eStringMatchType.Contains:
                    if (_negateStringMatchType)
                        return units.FindAll(x => !x.BaseInfo.Name.Contains(_name));
                    else return units.FindAll(x => x.BaseInfo.Name.Contains(_name));
                case eStringMatchType.StartsWith:
                    if (_negateStringMatchType)
                        return units.FindAll(x => !x.BaseInfo.Name.StartsWith(_name));
                    else return units.FindAll(x => x.BaseInfo.Name.StartsWith(_name));
                case eStringMatchType.EndsWith:
                    if (_negateStringMatchType)
                        return units.FindAll(x => !x.BaseInfo.Name.EndsWith(_name));
                    else return units.FindAll(x => x.BaseInfo.Name.EndsWith(_name));
            }
        }

        public List<UnitInstance> FindUnitsByLabel(string _label, bool _excludeLabel, List<UnitInstance> _units = null)
        {
            List<UnitInstance> units;

            if (_units == null)
                units = Field.Units;
            else
                units = _units;

            if (_excludeLabel)
                return units.FindAll(x => !x.BaseInfo.Labels.Contains(_label));
            else return units.FindAll(x => x.BaseInfo.Labels.Contains(_label));
        }

        public List<UnitInstance> FindUnitsByGender(eGender _gender, bool _excludeGender, List<UnitInstance> _units = null)
        {
            List<UnitInstance> units;

            if (_units == null)
                units = Field.Units;
            else
                units = _units;

            if (_excludeGender)
                return units.FindAll(x => x.BaseInfo.Gender != _gender);
            else return units.FindAll(x => x.BaseInfo.Gender == _gender);
        }

        public List<UnitInstance> FindUnitsByElement(eElement _element1, bool _excludeElement, eElement _element2 = eElement.None, List<UnitInstance> _units = null)
        {
            List<UnitInstance> units;

            if (_units == null)
                units = Field.Units;
            else
                units = _units;

            if (_element2 == eElement.None)
            {
                if (_excludeElement)
                    return units.FindAll(x => x.BaseInfo.Elements[0] != _element1 && x.BaseInfo.Elements[1] != _element1);
                else return units.FindAll(x => x.BaseInfo.Elements[0] == _element1 || x.BaseInfo.Elements[1] == _element1);
            }
            else
            {
                if (_excludeElement)
                    return units.FindAll(x => (x.BaseInfo.Elements[0] != _element1 && x.BaseInfo.Elements[1] != _element2)
                                            || (x.BaseInfo.Elements[0] != _element2 && x.BaseInfo.Elements[1] != _element1));
                else
                    return units.FindAll(x => (x.BaseInfo.Elements[0] == _element1 && x.BaseInfo.Elements[1] == _element2)
                                            || (x.BaseInfo.Elements[0] == _element2 && x.BaseInfo.Elements[1] == _element1));
            }
        }

        public List<UnitInstance> FindUnitsByStatusValue(eUnitStatusType _statusType, eRelationType _relation, int _value, List<UnitInstance> _units = null)
        {
            List<UnitInstance> units;

            if (_units == null)
                units = Field.Units;
            else
                units = _units;

            switch (_statusType)
            {
                default: // Level
                    return units.FindAll(x => CoreFunctions.Compare(Calculator.Level(x), _relation, _value));
                case eUnitStatusType.MaxHP:
                    return units.FindAll(x => CoreFunctions.Compare(Calculator.MaxHP(x), _relation, _value));
                case eUnitStatusType.RemainingHP:
                    return units.FindAll(x => CoreFunctions.Compare(x.RemainingHP, _relation, _value));
                case eUnitStatusType.PhyStr:
                    return units.FindAll(x => CoreFunctions.Compare(Calculator.PhysicalStrength(x), _relation, _value));
                case eUnitStatusType.PhyRes:
                    return units.FindAll(x => CoreFunctions.Compare(Calculator.PhysicalResistance(x), _relation, _value));
                case eUnitStatusType.MagStr:
                    return units.FindAll(x => CoreFunctions.Compare(Calculator.MagicalStrength(x), _relation, _value));
                case eUnitStatusType.MagRes:
                    return units.FindAll(x => CoreFunctions.Compare(Calculator.MagicalResistance(x), _relation, _value));
                case eUnitStatusType.Vitality:
                    return units.FindAll(x => CoreFunctions.Compare(Calculator.Vitality(x), _relation, _value));
            }
        }

        public List<UnitInstance> FindUnitsByStatusValueRanking(eUnitStatusType _statusType, eSortType _sortType, int _ranking, List<UnitInstance> _units = null)
        {
            try
            {
                List<UnitInstance> units;

                if (_units == null)
                    units = Field.Units;
                else
                    units = _units;

                units = SortUnitsByStatusValue(_statusType, _sortType, units);

                List<int> rankedValues = SetStatusValueRanking(units, _statusType);

                int rankingIndex = _ranking - 1;
                if (rankingIndex < 0 || rankingIndex >= rankedValues.Count)
                    rankingIndex = 0;

                switch (_statusType)
                {
                    default: // Level
                        return units.FindAll(x => Calculator.Level(x) == rankedValues[rankingIndex]);
                    case eUnitStatusType.MaxHP:
                        return units.FindAll(x => Calculator.MaxHP(x) == rankedValues[rankingIndex]);
                    case eUnitStatusType.RemainingHP:
                        return units.FindAll(x => x.RemainingHP == rankedValues[rankingIndex]);
                    case eUnitStatusType.PhyStr:
                        return units.FindAll(x => Calculator.PhysicalStrength(x) == rankedValues[rankingIndex]);
                    case eUnitStatusType.PhyRes:
                        return units.FindAll(x => Calculator.PhysicalResistance(x) == rankedValues[rankingIndex]);
                    case eUnitStatusType.MagStr:
                        return units.FindAll(x => Calculator.MagicalStrength(x) == rankedValues[rankingIndex]);
                    case eUnitStatusType.MagRes:
                        return units.FindAll(x => Calculator.MagicalResistance(x) == rankedValues[rankingIndex]);
                    case eUnitStatusType.Vitality:
                        return units.FindAll(x => Calculator.Vitality(x) == rankedValues[rankingIndex]);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Field.FindUnitsByStatusValueRanking() : " + ex.Message);
                return null;
            }
        }

        public List<UnitInstance> FindUnitsByTargetClassification(UnitInstance _referenceUnit, eTargetUnitClassification _targetClassification, List<_2DCoord> _targetRange = null)
        {
            List<UnitInstance> targets = new List<UnitInstance>();

            if (_targetClassification == eTargetUnitClassification.Self
                || _targetClassification == eTargetUnitClassification.SelfAndAlly
                || _targetClassification == eTargetUnitClassification.SelfAndEnemy
                || _targetClassification == eTargetUnitClassification.Any
                || _targetClassification == eTargetUnitClassification.SelfAndAllyInRange
                || _targetClassification == eTargetUnitClassification.SelfAndEnemyInRange
                || _targetClassification == eTargetUnitClassification.UnitInRange
                || _targetClassification == eTargetUnitClassification.SelfAndAllyOnBoard
                || _targetClassification == eTargetUnitClassification.SelfAndEnemyOnBoard
                || _targetClassification == eTargetUnitClassification.UnitOnBoard)
            {
                targets.Add(_referenceUnit);
            }

            switch (_targetClassification)
            {
                case eTargetUnitClassification.Self:
                    break;
                case eTargetUnitClassification.Ally:
                case eTargetUnitClassification.SelfAndAlly:
                    targets.AddRange(_referenceUnit.OwnerInstance.AlliedUnits);
                    break;
                case eTargetUnitClassification.Enemy:
                case eTargetUnitClassification.SelfAndEnemy:
                    targets.AddRange(Field.Units.Where(x => x.OwnerInstance != _referenceUnit.OwnerInstance));
                    break;
                case eTargetUnitClassification.AllyAndEnemy:
                case eTargetUnitClassification.Any:
                    targets.AddRange(_referenceUnit.OwnerInstance.AlliedUnits);
                    targets.AddRange(Field.Units.Where(x => x.OwnerInstance != _referenceUnit.OwnerInstance));
                    break;
                case eTargetUnitClassification.AllyOnBoard:
                case eTargetUnitClassification.SelfAndAllyOnBoard:
                    targets.AddRange(_referenceUnit.OwnerInstance.AlliedUnits.Where(x => x.IsAlive));
                    break;
                case eTargetUnitClassification.EnemyOnBoard:
                case eTargetUnitClassification.SelfAndEnemyOnBoard:
                    targets.AddRange(Field.Units.Where(x => x.OwnerInstance != _referenceUnit.OwnerInstance && x.IsAlive));
                    break;
                case eTargetUnitClassification.AllyAndEnemyOnBoard:
                case eTargetUnitClassification.UnitOnBoard:
                    targets.AddRange(_referenceUnit.OwnerInstance.AlliedUnits.Where(x => x.IsAlive));
                    targets.AddRange(Field.Units.Where(x => x.OwnerInstance != _referenceUnit.OwnerInstance && x.IsAlive));
                    break;
                case eTargetUnitClassification.AllyDefeated:
                    targets.AddRange(_referenceUnit.OwnerInstance.AlliedUnits.Where(x => !x.IsAlive));
                    break;
                case eTargetUnitClassification.EnemyDefeated:
                    targets.AddRange(Field.Units.Where(x => x.OwnerInstance != _referenceUnit.OwnerInstance && !x.IsAlive));
                    break;
                case eTargetUnitClassification.AllyAndEnemyDefeated:
                    targets.AddRange(_referenceUnit.OwnerInstance.AlliedUnits.Where(x => !x.IsAlive));
                    targets.AddRange(Field.Units.Where(x => x.OwnerInstance != _referenceUnit.OwnerInstance && !x.IsAlive));
                    break;
                default: // InRange
                    {
                        if (_targetRange != null) // If the target range is null, then it is not possible to lookup any unit.
                        {
                            foreach (_2DCoord coord in _targetRange)
                            {
                                if (coord.X >= 0 && coord.X <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD - 1
                                    && coord.Y >= 0 && coord.Y <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD - 1) //it will search inside of the board
                                {
                                    if (Field.Board.Sockets[coord.X, coord.Y].Unit != null)
                                    {
                                        if (_targetClassification == eTargetUnitClassification.AllyInRange
                                            || _targetClassification == eTargetUnitClassification.SelfAndAllyInRange
                                            || _targetClassification == eTargetUnitClassification.AllyAndEnemyInRange
                                            || _targetClassification == eTargetUnitClassification.UnitInRange)
                                        {
                                            if (Field.Board.Sockets[coord.X, coord.Y].Unit.OwnerInstance == _referenceUnit.OwnerInstance)
                                                targets.Add(Field.Board.Sockets[coord.X, coord.Y].Unit);
                                        }

                                        if (_targetClassification == eTargetUnitClassification.EnemyInRange
                                            || _targetClassification == eTargetUnitClassification.SelfAndEnemyInRange
                                            || _targetClassification == eTargetUnitClassification.AllyAndEnemyInRange
                                            || _targetClassification == eTargetUnitClassification.UnitInRange)
                                        {
                                            if (Field.Board.Sockets[coord.X, coord.Y].Unit.OwnerInstance != _referenceUnit.OwnerInstance)
                                                targets.Add(Field.Board.Sockets[coord.X, coord.Y].Unit);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return targets;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// PreCondition: _unit has been initialized successfully; _unit is assigned to a Socket of the Board; _destination is a valid coord within the Board;
        /// PostCondition: _unit will be assigned to Board.Socket[_destination.X, _destination.Y] if not already occupied; If moved successfully, true will be returned;
        /// </summary>
        private bool ChangeUnitLocation(UnitInstance _unit, _2DCoord _destination)
        {
            _2DCoord currentLocation = Field.UnitLocation(_unit);

            try
            {
                if (_destination.X < 0 || Field.Board.Sockets.GetLength(0) <= _destination.X
                    || _destination.Y < 0 || Field.Board.Sockets.GetLength(1) <= _destination.Y)
                    return false;

                if (Field.Board.Sockets[_destination.X, _destination.Y].Unit == null) // if there is no Unit assigned to the destination Socket
                {
                    Field.Board.Sockets[currentLocation.X, currentLocation.Y].Unit = null; // remove _unit from the current Socket
                    Field.Board.Sockets[_destination.X, _destination.Y].Unit = _unit; // assign _unit to the destination Socket

                    return true;
                }
                //else if destination Socket is occupied
                return false;
            }
            catch (Exception ex)
            {
                Debug.Log("Field: at ChangeUnitLocation() " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// PreCondition: _damage is positive value or 0; _target has been initialized successfully and _target.isAlive is true;
        /// PostCondition: _damage will be subtracted from _target.RemainingHP; If _damage > _target.RemainingHP, _target.RemainingHP will be set to 0;
        /// </summary>
        /// <param name="_damage"></param>
        /// <param name="_target"></param>
        private void DealDamage(int _damage, UnitInstance _target)
        {
            if (_target.RemainingHP - _damage > 0)
                _target.RemainingHP -= _damage;
            else
            {
                _target.RemainingHP = 0;
                UpdateUnitLiveStatus(_target);
                Field.RemoveNonAliveUnitFromBoard(_target);
            }
        }

        /// <summary>
        /// PreCondition: _hpValueToRestore is positive value or 0; _target has been initialized successfully and _target.isAlive is true;
        /// PostCondition: _hpValueToRestore will be added to _target.RemainingHP; If _hpValueToRestore + _target.RemainingHP > MaxHP, _target.RemainingHP will be set to MaxHP;
        /// </summary>
        /// <param name="_hpValueToRestore"></param>
        /// <param name="_target"></param>
        private void RestoreHP(int _hpValueToRestore, UnitInstance _target)
        {
            int maxHP = Calculator.MaxHP(_target);

            if (_target.RemainingHP + _hpValueToRestore < maxHP)
                _target.RemainingHP += _hpValueToRestore;
            else
                _target.RemainingHP = maxHP;
        }

        /// <summary>
        /// PreCondition: _effectToAttach and _target have been initialized successfully.
        /// PostCondition: A cloned instance of _effectToAttach will be added to _target.ContinuousEffects;
        /// </summary>
        /// <param name="_effectToAttach"></param>
        /// <param name="_target"></param>
        private void AttachEffect(StatusEffect _effectToAttach, UnitInstance _target)
        {
            _target.StatusEffects.Add((_effectToAttach as IDeepCopyable<StatusEffect>).DeepCopy()); // Add a cloned instance not to modify data in original instance
        }

        private List<UnitInstance> SortUnitsByStatusValue(eUnitStatusType _statusType, eSortType _sortType, List<UnitInstance> _units)
        {
            List<UnitInstance> units;

            switch (_statusType)
            {
                default: //Level
                    units = _units.OrderByDescending(x => Calculator.Level(x)).ToList();
                    break;
                case eUnitStatusType.MaxHP:
                    units = _units.OrderByDescending(x => Calculator.MaxHP(x)).ToList();
                    break;
                case eUnitStatusType.RemainingHP:
                    units = _units.OrderByDescending(x => x.RemainingHP).ToList();
                    break;
                case eUnitStatusType.PhyStr:
                    units = _units.OrderByDescending(x => Calculator.PhysicalStrength(x)).ToList();
                    break;
                case eUnitStatusType.PhyRes:
                    units = _units.OrderByDescending(x => Calculator.PhysicalResistance(x)).ToList();
                    break;
                case eUnitStatusType.MagStr:
                    units = _units.OrderByDescending(x => Calculator.MagicalStrength(x)).ToList();
                    break;
                case eUnitStatusType.MagRes:
                    units = _units.OrderByDescending(x => Calculator.MagicalResistance(x)).ToList();
                    break;
                case eUnitStatusType.Vitality:
                    units = _units.OrderByDescending(x => Calculator.Vitality(x)).ToList();
                    break;
            }

            if (_sortType == eSortType.Ascending)
                units.Reverse();

            return units;
        }

        private List<int> SetStatusValueRanking(List<UnitInstance> _sortedUnits, eUnitStatusType _statusType)
        {
            List<int> values = new List<int>();

            foreach (UnitInstance unit in _sortedUnits)
            {
                int value;

                switch (_statusType)
                {
                    default: //Level
                        value = Calculator.Level(unit);
                        break;
                    case eUnitStatusType.MaxHP:
                        value = Calculator.MaxHP(unit);
                        break;
                    case eUnitStatusType.RemainingHP:
                        value = unit.RemainingHP;
                        break;
                    case eUnitStatusType.PhyStr:
                        value = Calculator.PhysicalStrength(unit);
                        break;
                    case eUnitStatusType.PhyRes:
                        value = Calculator.PhysicalResistance(unit);
                        break;
                    case eUnitStatusType.MagStr:
                        value = Calculator.MagicalStrength(unit);
                        break;
                    case eUnitStatusType.MagRes:
                        value = Calculator.MagicalResistance(unit);
                        break;
                    case eUnitStatusType.Vitality:
                        value = Calculator.Vitality(unit);
                        break;
                }

                if (!values.Contains(value))
                    values.Add(value);
            }

            return values;
        }

        private void UpdateSP(PlayerOnBoard _player)
        {
            if (Field.Players[0] == _player)
                _player.MaxSP = (CurrentPlayerTurns[0] > CoreValues.MAX_SP) ? CoreValues.MAX_SP : CurrentPlayerTurns[0];
            else
                _player.MaxSP = (CurrentPlayerTurns[1] > CoreValues.MAX_SP) ? CoreValues.MAX_SP : CurrentPlayerTurns[1];

            _player.RemainingSP = _player.MaxSP;
        }

        private void UpdateActionSelectionStatus(PlayerOnBoard _player)
        {
            _player.Moved = false;
            _player.Attacked = false;
        }

        private void UnselectUnits(PlayerOnBoard _player)
        {
            _player.SelectedUnitIndex = -1;
        }

        private void UpdateUnitLiveStatus(UnitInstance _unit)
        {
            if (_unit.RemainingHP <= 0 && _unit.IsAlive)
            {
                _unit.IsAlive = false;

                PlayerOnBoard owner = _unit.OwnerInstance;
                if (owner.SelectedUnitIndex >= 0 
                    && owner.SelectedUnitIndex < owner.AlliedUnits.Count
                    && _unit == owner.AlliedUnits[owner.SelectedUnitIndex])
                {
                    UnselectUnits(owner);
                }

                if (!owner.AlliedUnits.Any(x => x.IsAlive)) // If no unit of the specific player is alive
                    EndMatch(owner); // The player has lost
            }
            else if (_unit.RemainingHP > 0 && !_unit.IsAlive)
                _unit.IsAlive = true;
        }

        //private void UpdateUnitsLiveStatus()
        //{
        //    foreach (PlayerOnBoard p in Field.Players)
        //    {
        //        foreach (UnitInstance u in p.AlliedUnits)
        //        {
        //            if (u.RemainingHP <= 0 && u.IsAlive)
        //            {
        //                u.IsAlive = false;
        //                if (p.SelectedUnitIndex >= 0 && u == p.AlliedUnits[p.SelectedUnitIndex])
        //                    UnselectUnits(p);
        //            }
        //        }
        //    }

        //    Field.RemoveNonAliveUnitsFromBoard();
        //}
        #endregion
    }
}
