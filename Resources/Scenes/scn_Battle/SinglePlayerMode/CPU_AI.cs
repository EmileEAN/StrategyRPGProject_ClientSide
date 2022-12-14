using EEANWorks;
using EEANWorks.Games.TBSG._01.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(PlayerController_SinglePlayer))]
    public class CPU_AI : MonoBehaviour
    {

        #region Properties
        public Text CPUThinkingStatusText;
        #endregion

        #region Private Fields
        private UnityBattleSystem_SinglePlayer m_mainScript;
        private AnimationController_SinglePlayer m_animationControllerScript;
        private PlayerController_SinglePlayer m_playerController;

        private BattleSystemCore m_system;
        private Field m_field;

        private bool m_isInitialized;

        private bool m_isActing;

        private const decimal DAMAGE_TO_MAXHP_RATIO_FOR_DANGER = 0.2m;

        private List<UnitInstance> m_aliveAllies;
        private List<UnitInstance> m_aliveEnemies;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_isInitialized = false;
            m_isActing = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();
            else if (m_playerController.IsCPU && m_playerController.IsMyTurn && !m_isActing)
                Task.Run(() => Act());
        }

        // Updates once per frame
        void FixedUpdate()
        {
            if (m_isActing)
            {
                // Change the transparency of the loading text to let the player know that the application is still working.
                CPUThinkingStatusText.color = new Color(CPUThinkingStatusText.color.r, CPUThinkingStatusText.color.g, CPUThinkingStatusText.color.b, Mathf.PingPong(Time.time, 1));
            }
            else
                CPUThinkingStatusText.color = new Color(CPUThinkingStatusText.color.r, CPUThinkingStatusText.color.g, CPUThinkingStatusText.color.b, 0f);
        }

        private void Initialize()
        {
            if (m_mainScript == null)
                m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_SinglePlayer>();
            if (m_mainScript == null)
                return;

            if (m_animationControllerScript == null)
                m_animationControllerScript = this.transform.root.GetComponent<AnimationController_SinglePlayer>();
            if (m_animationControllerScript == null)
                return;

            if (m_playerController == null)
                m_playerController = this.GetComponent<PlayerController_SinglePlayer>();

            if (m_mainScript.IsInitialized && m_animationControllerScript.IsInitialized && m_playerController.IsInitialized)
            {
                m_system = m_mainScript.BattleSystemCore;
                m_field = m_system?.Field;

                m_isInitialized = m_field != null;
            }
        }

        async Task Act()
        {
            m_isActing = true;

            List<UnitInstance> units = (m_playerController.PlayerData as PlayerOnBoard).AlliedUnits;
            List<UnitInstance> enemies = (m_mainScript.PlayerControllers.First(x => x != m_playerController).PlayerData as PlayerOnBoard).AlliedUnits;

            m_aliveAllies = units.Where(x => x.IsAlive).ToList();
            m_aliveEnemies = enemies.Where(x => x.IsAlive).ToList();

            if (!m_playerController.HasMoved)
            {
                Tuple<UnitInstance, _2DCoord> bestMovementOption = await IdentifyLowestMovementRisk();
                if (bestMovementOption.Item2 != m_field.UnitLocation(bestMovementOption.Item1))
                {
                    m_mainScript.ChangeUnit(m_playerController.PlayerId, units.IndexOf(bestMovementOption.Item1));
                    m_mainScript.MoveUnit(bestMovementOption.Item2);
                }
            }

            if (!m_playerController.HasAttacked)
            {
                Tuple<UnitInstance, _2DCoord> bestAttackOption = await IdentifyTheBestAttackOption();
                if (bestAttackOption != null)
                {
                    m_mainScript.ChangeUnit(m_playerController.PlayerId, units.IndexOf(bestAttackOption.Item1));
                    m_mainScript.Attack(new List<_2DCoord>() { bestAttackOption.Item2 });
                }
            }

            if (m_playerController.RemainingSP > 0)
            {
                foreach (DamageSkillResult damageSkillResult in await IdentifyTheBestSkillUsageOption(m_playerController.RemainingSP))
                {
                    List<_2DCoord> targetCoords = new List<_2DCoord>();
                    foreach (UnitInstance target in damageSkillResult.Targets)
                    {
                        targetCoords.Add(m_field.UnitLocation(target));
                    }
                    m_mainScript.ChangeUnit(m_playerController.PlayerId, units.IndexOf(damageSkillResult.Attacker));
                    m_mainScript.UseSkill(damageSkillResult.Skill, targetCoords, null);
                }
            }

            m_mainScript.ChangeTurn();

            m_isActing = false;
        }

        /// <summary>
        /// This function is neither considering the possible movements of allied units nor enemy units that use a movement skill. It is not considering the degree of danger for non-damaging skills nor calculates the damage value dealt by secondary effects. Furthermore, it does not consider the skills which sp is 0. Also, it does not consider whether a skill requiring item cost can actually be executed nor whether another skill can create the items required for such skills. Moreover, it does not consider the cases where units move after dealing damage in order to deal damage to the same unit once again.
        /// </summary>
        async Task<Tuple<UnitInstance, _2DCoord>> IdentifyLowestMovementRisk()
        {
            List<Tuple<DamageToEachAllyInAParticularSituation, decimal>> combination_maxDamageToRemainingHPRatio = new List<Tuple<DamageToEachAllyInAParticularSituation, decimal>>();
            foreach (DamageToEachAllyInAParticularSituation maxDamageToEachAllyInAParticularSituation in await GetMaxDamageToEachAllyPerAllyAndEnemyMovementCombination())
            {
                decimal maxDamageToRemainingHPRatio = 0m;
                foreach (DamageOnCoordForUnit damagePerAlly in maxDamageToEachAllyInAParticularSituation.DamagePerAlly)
                {
                    decimal damageToRemainingHPRatio = Convert.ToDecimal(damagePerAlly.Damage) / damagePerAlly.Unit.RemainingHP;
                    if (maxDamageToRemainingHPRatio < damageToRemainingHPRatio)
                        maxDamageToRemainingHPRatio = damageToRemainingHPRatio;
                }

                combination_maxDamageToRemainingHPRatio.Add(new Tuple<DamageToEachAllyInAParticularSituation, decimal>(maxDamageToEachAllyInAParticularSituation, maxDamageToRemainingHPRatio));
            }

            if (combination_maxDamageToRemainingHPRatio.Count > 0)
            {
                combination_maxDamageToRemainingHPRatio = combination_maxDamageToRemainingHPRatio.OrderByDescending(x => x.Item2).ToList(); // Sort the list based on the max (damage / remaining HP) of the alive allied units for each particular situation. This list will show the movement risk from highest to lowest based on the amout of damage that allied units can receive in the following opponent's turn when moving (or not moving) a unit to a certain coord.

                return new Tuple<UnitInstance, _2DCoord>(combination_maxDamageToRemainingHPRatio[0].Item1.MoverAlly, combination_maxDamageToRemainingHPRatio[0].Item1.MoverAllyDestination);
            }
            else
            {
                MTRandom.RandInit();
                bool move = Convert.ToBoolean(MTRandom.GetRandInt(0, 1));
                if (move)
                {
                    while (true)
                    {
                        int numOfAliveAllies = m_aliveAllies.Count();
                        int indexOfAllyToMove = MTRandom.GetRandInt(0, numOfAliveAllies - 1);
                        UnitInstance allyToMove = m_aliveAllies[indexOfAllyToMove];
                        List<_2DCoord> movableAndSelectableArea = m_system.GetMovableAndSelectableAreaForSimulation(allyToMove).GetKeysWithValue(true);
                        if (movableAndSelectableArea.Count > 0)
                        {
                            int indexOfDestinationCoord = MTRandom.GetRandInt(0, movableAndSelectableArea.Count - 1);
                            return new Tuple<UnitInstance, _2DCoord>(allyToMove, movableAndSelectableArea[indexOfDestinationCoord]);
                        }
                    }
                }
                else
                {
                    UnitInstance aliveAlly = m_aliveAllies[0];
                    return new Tuple<UnitInstance, _2DCoord>(aliveAlly, m_field.UnitLocation(aliveAlly));
                }
            }
        }

        async Task<Tuple<UnitInstance, _2DCoord>> IdentifyTheBestAttackOption()
        {
            Tuple<UnitInstance, _2DCoord> result = null;

            OrdinarySkill basicAttackSkill = GameDataContainer.Instance.BasicAttackSkill;

            int maxPossibleDamageForBestAttackOption = 0;
            foreach (UnitInstance ally in m_aliveAllies) // For each alive ally
            {
                _2DCoord allyLocation = m_field.UnitLocation(ally);

                List<_2DCoord> attackTargetableArea = m_system.GetAttackTargetableArea(ally);

                List<_2DCoord> attackTargetableAndSelectableArea = m_system.GetAttackTargetableAndSelectableArea(ally).GetKeysWithValue(true);
                foreach (_2DCoord targetCoord in attackTargetableAndSelectableArea)
                {
                    UnitInstance target = m_field.Board.Sockets[targetCoord.X, targetCoord.Y].Unit;

                    int maxPossibleDamage = Convert.ToInt32(Math.Floor(Calculator.PossibleNonCriticalDamage(m_system, ally, allyLocation, basicAttackSkill, basicAttackSkill.BaseInfo.Effect as DamageEffect, attackTargetableArea, new List<UnitInstance>() { target }, target, out eEffectiveness _effectiveness) * CoreValues.MULTIPLIER_FOR_CRITICALHIT));

                    if (maxPossibleDamage > maxPossibleDamageForBestAttackOption)
                    {
                        maxPossibleDamageForBestAttackOption = maxPossibleDamage;
                        result = new Tuple<UnitInstance, _2DCoord>(ally, targetCoord);
                    }
                }
            }

            return result;
        }

        async Task<List<DamageSkillResult>> IdentifyTheBestSkillUsageOption(int _playerUsableSP)
        {
            List<DamageSkillResult> result = new List<DamageSkillResult>();

            List<DamageSkillResult> tmp_damageSkillResults = new List<DamageSkillResult>();
            List<Task> tasks = new List<Task>();
            foreach (UnitInstance ally in m_aliveAllies)
            {
                foreach (ActiveSkill skill in ally.Skills.OfType<ActiveSkill>().Where(x => !(x is CounterSkill) && x.BaseInfo.Effect is DamageEffect))
                {
                    tasks.Add(AddMaxPossibleDamageSkillOption(tmp_damageSkillResults, m_system, ally, skill));
                }
            }
            await Task.WhenAll(tasks);

            if (tmp_damageSkillResults.Count > 0)
            {
                List<DamageSkillResult> damageSkillResultsDamagePerSPRanking = tmp_damageSkillResults.Where(x => x.SPRequired != 0 && x.SPRequired <= _playerUsableSP).OrderByDescending(x => Convert.ToDecimal(x.TotalDamage) / x.SPRequired).ToList(); // List of skill results where sp required is not 0 and sp required is not greater than the player's max sp; ordered by highest (total damage / sp required) to lowest
                int remainingSP = _playerUsableSP;
                while (remainingSP > 0)
                {
                    for (int i = 0; i < damageSkillResultsDamagePerSPRanking.Count; i++)
                    {
                        if (damageSkillResultsDamagePerSPRanking[i].SPRequired <= remainingSP)
                        {
                            result.Add(damageSkillResultsDamagePerSPRanking[i]);
                            remainingSP -= damageSkillResultsDamagePerSPRanking[i].SPRequired;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        async Task AddMaxPossibleDamageSkillOption(List<DamageSkillResult> _damageSkillResults, BattleSystemCore _system, UnitInstance _ally, ActiveSkill _skill)
        {
            DamageSkillResult maxPossibleDamageSkillOption = await GetMaxPossibleDamageSkillOption(_system, _ally, _skill);
            if (maxPossibleDamageSkillOption.DamagePerTarget.Count > 0)
                _damageSkillResults.Add(maxPossibleDamageSkillOption);
        }

        //private void CheckDegreeOfDangerWithoutEnemyMovement(UnitInstance _unit, _2DCoord _unitLocation)
        //{
        //    foreach (UnitInstance enemy in m_aliveEnemies) // For each alive enemy
        //    {
        //        // Check whether the enemy's attack may target the Unit
        //        {
        //            List<_2DCoord> enemyAttackTargetableAndSelectableArea = m_system.GetAttackTargetableAndSelectableArea(enemy).GetKeysWithValue(true);
        //            if (enemyAttackTargetableAndSelectableArea.Contains(_unitLocation)) // If the Unit is within the enemy's attack area
        //            {
        //                OrdinarySkill skill = GameDataContainer.Instance.BasicAttackSkill;

        //                Tuple<int, int> tmp_minAndMaxPossibleDamage = GetMinAndMaxPossibleDamage(enemy, skill, enemyAttackTargetableAndSelectableArea, _unit);
        //                m_unit_enemy_skill_minPossibleDamage_maxPossibleDamage.Add(new Tuple<UnitInstance, UnitInstance, ActiveSkill, int, int>(_unit, enemy, skill, tmp_minAndMaxPossibleDamage.Item1, tmp_minAndMaxPossibleDamage.Item2));
        //            }
        //        }

        //        // Check whether the enemy's ordinary skills with an effect classification of DamageEffect may target the Unit
        //        foreach (OrdinarySkill skill in enemy.Skills.OfType<OrdinarySkill>().Where(x => x.BaseInfo.Effect is DamageEffect))
        //        {
        //            List<_2DCoord> enemySkillTargetableAndSelectableArea = m_system.GetSkillTargetableAndSelectableAreaForSimulation(enemy, skill).GetKeysWithValue(true);
        //            if (enemySkillTargetableAndSelectableArea.Contains(m_field.UnitLocation(_unit))) // If the Unit is within the enemy's skill area
        //            {
        //                Tuple<int, int> tmp_minAndMaxPossibleDamage = GetMinAndMaxPossibleDamage(enemy, skill, enemySkillTargetableAndSelectableArea, _unit);
        //                m_unit_enemy_skill_minPossibleDamage_maxPossibleDamage.Add(new Tuple<UnitInstance, UnitInstance, ActiveSkill, int, int>(_unit, enemy, skill, tmp_minAndMaxPossibleDamage.Item1, tmp_minAndMaxPossibleDamage.Item2));
        //            }
        //        }
        //    }
        //}

        //private void CheckDegreeOfDangerWithEnemyMovement(UnitInstance _unit, _2DCoord _unitLocation)
        //{
        //    foreach (UnitInstance enemy in m_aliveEnemies) // For each alive enemy
        //    {

        //    }
        //}

        //private void UpdateTileDangerInfo()
        //{
        //    m_coord_numOfEnemiesThatCanTargetTheCoord.Clear();
        //    m_coord_maxDamageThatCanBeDealtToTheUnitInTheCoord.Clear();

        //    foreach (UnitInstance enemy in m_aliveEnemies) // For each alive enemy
        //    {
        //        // Check
        //    }
        //}

        //private void UpdateNumOfEnemiesThatCanTargetEachCoord()
        //{
        //    foreach (UnitInstance enemy in m_aliveEnemies) // For each alive enemy
        //    {
        //        List<_2DCoord> targetableArea = m_system.GetAttackTargetableArea(enemy);
        //        foreach (ActiveSkill skill in enemy.Skills.Where(x => x is OrdinarySkill || x is UltimateSkill))
        //        {
        //            targetableArea.AddRange(m_system.GetSkillTargetableArea(enemy, skill));
        //        }
        //        targetableArea = targetableArea.Distinct().ToList();

        //        foreach (_2DCoord coord in targetableArea)
        //        {
        //            if (m_coord_numOfEnemiesThatCanTargetTheCoord.ContainsKey(coord))
        //                m_coord_numOfEnemiesThatCanTargetTheCoord[coord]++;
        //            else
        //                m_coord_numOfEnemiesThatCanTargetTheCoord.Add(coord, 1);
        //        }
        //    }
        //}

        private class SkillUserDamageInfo
        {
            public SkillUserDamageInfo(UnitInstance _skillUser, int _maxPossibleDamage, int _spRequiredForSkill, bool _isBasicAttack = false)
            {
                SkillUser = _skillUser;
                MaxPossibleDamage = _maxPossibleDamage;
                SPRequiredForSkill = _spRequiredForSkill;
                IsBasicAttack = _isBasicAttack;
            }

            public UnitInstance SkillUser { get; }
            public int MaxPossibleDamage { get; }
            public int SPRequiredForSkill { get; }
            public bool IsBasicAttack { get; }
        }

        async Task<List<DamageToEachAllyInAParticularSituation>> GetMaxDamageToEachAllyPerAllyAndEnemyMovementCombination()
        {
            List<DamageToEachAllyInAParticularSituation> result = new List<DamageToEachAllyInAParticularSituation>();

            BattleSystemCore systemCopy = new BattleSystemCore(m_system); // Create a copy of the system so that unit movements performed during simulation will not be reflected to the actual match
            Field fieldCopy = systemCopy.Field;

            int currentMaxEnemySP = fieldCopy.Players.First(x => x != (m_playerController.PlayerData as PlayerOnBoard)).MaxSP;
            int maxEnemySPOnNextTurn = (currentMaxEnemySP < CoreValues.MAX_SP) ? currentMaxEnemySP + 1 : CoreValues.MAX_SP;

            foreach (UnitInstance ally in m_aliveAllies) // For each alive ally
            {
                _2DCoord originalAllyLocation = fieldCopy.UnitLocation(ally);

                // Calculate the possible damage to each allied unit based on the possible movement of the ally during this turn, by placing the ally on each of them
                List<_2DCoord> possibleAllyLocations = systemCopy.GetMovableAndSelectableAreaForSimulation(ally).GetKeysWithValue(true);
                possibleAllyLocations.Add(originalAllyLocation);
                foreach (_2DCoord possibleAllyLocation in possibleAllyLocations)
                {
                    if (possibleAllyLocation != originalAllyLocation) // If the possible ally location is same as its original location
                        systemCopy.MoveUnit(ally, possibleAllyLocation); // Move the ally

                    foreach (UnitInstance enemy in m_aliveEnemies) // For each alive enemy
                    {
                        _2DCoord originalEnemyLocation = fieldCopy.UnitLocation(enemy);

                        // Calculate the possible damage to each allied units based on the possible movements of the enemy in the proceeding opponent's turn, by placing the enemy on each of its possible location
                        List<_2DCoord> possibleEnemyLocations = systemCopy.GetMovableAndSelectableAreaForSimulation(enemy).GetKeysWithValue(true);
                        foreach (_2DCoord possibleEnemyLocation in possibleEnemyLocations)
                        {
                            if (possibleEnemyLocation != originalEnemyLocation) // If the possible enemy location is same as its original location
                                systemCopy.MoveUnit(enemy, possibleEnemyLocation); // Move the enemy

                            // Get the set of damage per alive allied unit in the particular movement case
                            DamageToEachAllyInAParticularSituation maxDamageToEachAllyInAParticularSituation = await GetMaxPossibleDamageToEachAllyInAParticularSituation(systemCopy, ally, possibleAllyLocation, enemy, possibleEnemyLocation, maxEnemySPOnNextTurn);

                            result.Add(maxDamageToEachAllyInAParticularSituation); // Add the set of max possible damage against each ally on the particular situation

                            // Move enemy back to original location
                            systemCopy.MoveUnit(enemy, originalEnemyLocation);
                        }
                    }

                    if (possibleAllyLocation != originalAllyLocation) // If the Unit has moved
                        systemCopy.MoveUnit(ally, originalAllyLocation); // Move the Unit back to its original location
                }
            }

            return result;
        }

        async Task<DamageToEachAllyInAParticularSituation> GetMaxPossibleDamageToEachAllyInAParticularSituation(BattleSystemCore _system, UnitInstance _moverAlly, _2DCoord _moverAllyDestination, UnitInstance _moverEnemy, _2DCoord _moverEnemyDestination, int _maxEnemySPOnNextTurn)
        {
            DamageToEachAllyInAParticularSituation result = new DamageToEachAllyInAParticularSituation(_moverAlly, _moverAllyDestination, _moverEnemy, _moverEnemyDestination); // Instantiate the set of damage per alive allied unit for this particular situation

            // Get the set of damage per alive allied unit for this particular situation
            List<Task> tasks = new List<Task>();
            foreach (UnitInstance alliedTargetCandidate in m_aliveAllies) // For each alive ally
            {
                tasks.Add(AddMaxPossibleDamageToAllyInAParticularSituation(result, _system, alliedTargetCandidate, _maxEnemySPOnNextTurn));
            }
            await Task.WhenAll(tasks);

            return result;
        }

        async Task AddMaxPossibleDamageToAllyInAParticularSituation(DamageToEachAllyInAParticularSituation _maxDamageToEachAllyInAParticularSituation, BattleSystemCore _system, UnitInstance _alliedTargetCandidate, int _maxEnemySPOnNextTurn)
        {
            List<SkillUserDamageInfo> maxPossibleDamagePerEnemyActionToAllyOnLocation = await GetMaxPossibleDamagePerEnemyActionToAllyOnLocation(_system, _alliedTargetCandidate); ;

            int maxPossibleDamage = await GetMaxPossibleDamageToUnitOnLocation(maxPossibleDamagePerEnemyActionToAllyOnLocation, _maxEnemySPOnNextTurn);

            _maxDamageToEachAllyInAParticularSituation.DamagePerAlly.Add(new DamageOnCoordForUnit(_alliedTargetCandidate, _system.Field.UnitLocation(_alliedTargetCandidate), maxPossibleDamage)); // Save the max possible damage agains the allied target candidate on the location
        }

        /// <summary>
        /// This does not actually get the max possible damage, but a value near it
        /// </summary>
        async Task<int> GetMaxPossibleDamageToUnitOnLocation(List<SkillUserDamageInfo> _maxPossibleDamagePerSkillToUnitOnLocation, int _maxEnemySP)
        {
            // Identify the max possible damage against the unit for this particular situation
            int maxPossibleDamage = 0;
            {
                // Get the max possible damage for basic attack targeting the unit
                int maxPossibleBasicAttackDamage = 0;
                SkillUserDamageInfo enemyBasicAttackInfoWithMaxPossibleDamage = _maxPossibleDamagePerSkillToUnitOnLocation.Where(x => x.IsBasicAttack).OrderBy(x => x.MaxPossibleDamage).FirstOrDefault();
                if (enemyBasicAttackInfoWithMaxPossibleDamage != null)
                    maxPossibleBasicAttackDamage = enemyBasicAttackInfoWithMaxPossibleDamage.MaxPossibleDamage;
                maxPossibleDamage += maxPossibleBasicAttackDamage; // Add the max possible basic attack damage to the max possible damage

                // Get the pairs of max possible damage and required sp for damaging skills where required sp is not 0, ordered by the damage per sp (max possible damage / required sp)
                List<Tuple<int, int>> maxPossibleDamage_spRequiredForEnemySkill = new List<Tuple<int, int>>();
                {
                    foreach (var skillUserDamageInfo in _maxPossibleDamagePerSkillToUnitOnLocation.Where(x => !x.IsBasicAttack && x.SPRequiredForSkill != 0 && x.SPRequiredForSkill <= _maxEnemySP).OrderBy(x => Convert.ToDecimal(x.MaxPossibleDamage) / x.SPRequiredForSkill)) // For each skillUserDamageInfo where the skill is not the basic attack, the sp required is not 0, and the sp required is not greater than the mas enemy sp
                    {
                        maxPossibleDamage_spRequiredForEnemySkill.Add(new Tuple<int, int>(skillUserDamageInfo.MaxPossibleDamage, skillUserDamageInfo.SPRequiredForSkill));
                    }
                }

                if (maxPossibleDamage_spRequiredForEnemySkill.Count > 0)
                {
                    maxPossibleDamage_spRequiredForEnemySkill = maxPossibleDamage_spRequiredForEnemySkill.Distinct().ToList(); // Remove redundant pairs

                    // Group the pairs that have the same damage per sp and store the groups into a list maintaining their order
                    List<List<Tuple<int, int>>> damagePerSPRanking = new List<List<Tuple<int, int>>>();
                    {
                        decimal damagePerSP = -1;
                        foreach (var item in maxPossibleDamage_spRequiredForEnemySkill)
                        {
                            decimal tmp_damagePerSP = Convert.ToDecimal(item.Item1) / item.Item2;
                            if (tmp_damagePerSP != damagePerSP)
                            {
                                damagePerSP = tmp_damagePerSP;

                                damagePerSPRanking.Add(new List<Tuple<int, int>>());
                            }
                            damagePerSPRanking.Last().Add(item);
                        }
                    }

                    //if (_maxEnemySP < CoreValues.MAX_SP)
                    //{
                    if (damagePerSPRanking[0].Any(x => _maxEnemySP % x.Item2 == 0))
                    {
                        Tuple<int, int> damage_spRequired = damagePerSPRanking[0].First(x => _maxEnemySP % x.Item2 == 0);
                        maxPossibleDamage += damage_spRequired.Item1 * (_maxEnemySP / damage_spRequired.Item2); // Max possible damage equals the damage multiplied by (_maxEnemySP / sp required to deal the damage)
                    }
                    else
                    {
                        foreach (Tuple<int, int> damagePerSP in damagePerSPRanking[0])
                        {
                            int numOfTimesTheSkillCanBeUsed = Convert.ToInt32(Math.Floor(Convert.ToDecimal(_maxEnemySP) / damagePerSP.Item2));
                            maxPossibleDamage += damagePerSP.Item1 * numOfTimesTheSkillCanBeUsed;
                            int remainingSP = _maxEnemySP - damagePerSP.Item2 * numOfTimesTheSkillCanBeUsed;
                            if (remainingSP > 0)
                            {
                                for (int i = 1; i < damagePerSPRanking.Count; i++)
                                {
                                    if (damagePerSPRanking[i].Any(x => remainingSP % x.Item2 == 0))
                                    {
                                        Tuple<int, int> damage_spRequired = damagePerSPRanking[i].First(x => remainingSP % x.Item2 == 0);
                                        maxPossibleDamage += damage_spRequired.Item1 * (remainingSP / damage_spRequired.Item2);
                                        break;
                                    }
                                    else if (damagePerSPRanking[i].Any(x => x.Item2 < remainingSP))
                                    {
                                        Tuple<int, int> damage_spRequired = damagePerSPRanking[i].First(x => x.Item2 < remainingSP);
                                        int timesTheSkillCanBeUsed = Convert.ToInt32(Math.Floor(Convert.ToDecimal(remainingSP) / damage_spRequired.Item2));
                                        maxPossibleDamage += damage_spRequired.Item1 * timesTheSkillCanBeUsed;
                                        remainingSP -= damage_spRequired.Item2 * timesTheSkillCanBeUsed;
                                    }

                                    //yield return null;
                                }
                            }
                            else
                                break;
                        }
                    }
                    //}
                    /*
                    else
                    {
                        //Identify the max possible damage, assuming that all the damage will be directed to the single coord
                        if (damagePerSPRanking[0].Any(x => CoreValues.MAX_SP % x.Item2 == 0)) // If the group with the highest damage per sp contains a pair with the sp that is a divisor of 10 (the max number of sp)
                        {
                            Tuple<int, int> damage_spRequired = damagePerSPRanking[0].First(x => CoreValues.MAX_SP % x.Item2 == 0);
                            maxPossibleDamage += damage_spRequired.Item1 * (CoreValues.MAX_SP / damage_spRequired.Item2); // Max possible damage equals the damage multiplied by (MAX_SP / sp required to deal the damage)
                        }
                        else
                        {
                            if (damagePerSPRanking[0].Any(x => x.Item2 == 3) && damagePerSPRanking[0].Any(x => x.Item2 == 4))
                                maxPossibleDamage += damagePerSPRanking[0].First(x => x.Item2 == 3).Item1 * 2 + damagePerSPRanking[0].First(x => x.Item2 == 4).Item1; // 2 * [0]D3 + [0]D4
                            else if (damagePerSPRanking[0].Any(x => x.Item2 == 4) && damagePerSPRanking[0].Any(x => x.Item2 == 6))
                                maxPossibleDamage += damagePerSPRanking[0].First(x => x.Item2 == 4).Item1 + damagePerSPRanking[0].First(x => x.Item2 == 6).Item1; // [0]D4 + [0]D6
                            else if (damagePerSPRanking[0].Any(x => x.Item2 == 3) && damagePerSPRanking[0].Any(x => x.Item2 == 7))
                                maxPossibleDamage += damagePerSPRanking[0].First(x => x.Item2 == 3).Item1 + damagePerSPRanking[0].First(x => x.Item2 == 7).Item1; // [0]D3 + [0]D7
                            else // Not all sp can be spent by using skills with highest damage per sp ranking, hence include those from the second highest group if available
                            {
                                if (damagePerSPRanking[0].Any(x => x.Item2 == 3))
                                {
                                    int highestGroupDamageForSP3 = damagePerSPRanking[0].First(x => x.Item2 == 3).Item1;

                                    if (damagePerSPRanking.Count > 2) // If there is a second highest group
                                    {
                                        for (int i = 1; i < damagePerSPRanking.Count; i++)
                                        {
                                            if (damagePerSPRanking[i].Any(x => x.Item2 == 1))
                                            {
                                                maxPossibleDamage += highestGroupDamageForSP3 * 3 + damagePerSPRanking[1].First(x => x.Item2 == 1).Item1; // 3 * [0]D3 + [1]D1
                                                break;
                                            }
                                            else if (damagePerSPRanking[i].Any(x => x.Item2 == 2 || x.Item2 == 4))
                                            {
                                                Tuple<int, int> groupIdamage_spRequired = damagePerSPRanking[i].First(x => x.Item2 == 2 || x.Item2 == 4);
                                                int groupIDamageToSpendAllSP = groupIdamage_spRequired.Item1 * (4 / groupIdamage_spRequired.Item2);
                                                if (highestGroupDamageForSP3 < groupIDamageToSpendAllSP)
                                                    maxPossibleDamage += highestGroupDamageForSP3 * 2 + groupIDamageToSpendAllSP; // 2 * [0]D3 + (2 * [1]D2 or [1]D4)
                                                else
                                                    maxPossibleDamage += highestGroupDamageForSP3 * 3; // 3 * [0]D3

                                                break;
                                            }
                                            else if (damagePerSPRanking[1].Any(x => x.Item2 == 7))
                                            {
                                                int groupIDamageToSpendAllSP = damagePerSPRanking[0].First(x => x.Item2 == 7).Item1;
                                                if (highestGroupDamageForSP3 * 2 < groupIDamageToSpendAllSP)
                                                    maxPossibleDamage += highestGroupDamageForSP3 + groupIDamageToSpendAllSP; // [0]D3 + [1]D7
                                                else
                                                    maxPossibleDamage += highestGroupDamageForSP3 * 3; // 3 * [0]D3

                                                break;
                                            }

                                            if (i == damagePerSPRanking.Count - 1)
                                                maxPossibleDamage += highestGroupDamageForSP3 * 3; // 3 * [0]D3
                                        }
                                    }
                                    else // If there is not a second highest group
                                        maxPossibleDamage += highestGroupDamageForSP3 * 3; // 3 * [0]D3
                                }
                                else if (damagePerSPRanking[0].Any(x => x.Item2 == 4))
                                {
                                    int highestGroupDamageForSP4 = damagePerSPRanking[0].First(x => x.Item2 == 4).Item1;

                                    if (damagePerSPRanking.Count > 2) // If there is a second highest group
                                    {
                                        for (int i = 1; i < damagePerSPRanking.Count; i++)
                                        {
                                            if (damagePerSPRanking[i].Any(x => x.Item2 == 1 || x.Item2 == 2))
                                            {
                                                Tuple<int, int> groupIdamage_spRequired = damagePerSPRanking[i].First(x => x.Item2 == 1 || x.Item2 == 2);
                                                maxPossibleDamage += highestGroupDamageForSP4 * 2 + groupIdamage_spRequired.Item1 * (2 / groupIdamage_spRequired.Item2); // 2 * [0]D4 + (2 * [1]D1 or [1]D2)
                                                break;
                                            }
                                            else if (damagePerSPRanking[i].Any(x => x.Item2 == 3 || x.Item2 == 6))
                                            {
                                                Tuple<int, int> groupIdamage_spRequired = damagePerSPRanking[i].First(x => x.Item2 == 3 || x.Item2 == 6);
                                                int groupIDamageToSpendAllSP = groupIdamage_spRequired.Item1 * (6 / groupIdamage_spRequired.Item2);
                                                if (highestGroupDamageForSP4 < groupIDamageToSpendAllSP)
                                                    maxPossibleDamage += highestGroupDamageForSP4 + groupIDamageToSpendAllSP; // [0]D4 + (2 * [1]D3 or [1]D6)
                                                else
                                                    maxPossibleDamage += highestGroupDamageForSP4 * 2; // 2 * [0]D4
                                            }

                                            if (i == damagePerSPRanking.Count - 1)
                                                maxPossibleDamage += highestGroupDamageForSP4 * 2; // 2 * [0]D4
                                        }
                                    }
                                    else // If there is not a second highest group
                                        maxPossibleDamage += highestGroupDamageForSP4 * 2; // 2 * [0]D4
                                }
                                else if (damagePerSPRanking[0].Any(x => x.Item2 == 6))
                                {
                                    int highestGroupDamageForSP6 = damagePerSPRanking[0].First(x => x.Item2 == 6).Item1;

                                    if (damagePerSPRanking.Count > 2) // If there is a second highest group
                                    {
                                        for (int i = 1; i < damagePerSPRanking.Count; i++)
                                        {
                                            if (damagePerSPRanking[i].Any(x => x.Item2 == 1 || x.Item2 == 2 || x.Item2 == 4))
                                            {
                                                Tuple<int, int> groupIdamage_spRequired = damagePerSPRanking[i].First(x => x.Item2 == 1 || x.Item2 == 2 || x.Item2 == 4);
                                                maxPossibleDamage += highestGroupDamageForSP6 + groupIdamage_spRequired.Item1 * (4 / groupIdamage_spRequired.Item2); // [0]D6 + (4 * [i]D1 or 2 * [i]D2 or [i]D4)
                                                break;
                                            }

                                            if (i == damagePerSPRanking.Count - 1)
                                                maxPossibleDamage += highestGroupDamageForSP6; // [0]D6
                                        }
                                    }
                                    else // If there is not a second highest group
                                        maxPossibleDamage += highestGroupDamageForSP6; // [0]D6
                                }
                                else if (damagePerSPRanking[0].Any(x => x.Item2 == 7))
                                {
                                    int highestGroupDamageForSP7 = damagePerSPRanking[0].First(x => x.Item2 == 7).Item1;

                                    if (damagePerSPRanking.Count > 2) // If there is a second highest group
                                    {
                                        for (int i = 1; i < damagePerSPRanking.Count; i++)
                                        {
                                            if (damagePerSPRanking[i].Any(x => x.Item2 == 1 || x.Item2 == 3))
                                            {
                                                Tuple<int, int> groupIdamage_spRequired = damagePerSPRanking[i].First(x => x.Item2 == 1 || x.Item2 == 3);
                                                maxPossibleDamage += highestGroupDamageForSP7 + groupIdamage_spRequired.Item1 * (3 / groupIdamage_spRequired.Item2); // [0]D7 + (3 * [i]D1 or [i]D3)
                                                break;
                                            }

                                            if (i == damagePerSPRanking.Count - 1)
                                                maxPossibleDamage += highestGroupDamageForSP7; // [0]D7
                                        }
                                    }
                                    else // If there is not a second highest group
                                        maxPossibleDamage += highestGroupDamageForSP7; // [0]D7
                                }
                                else if (damagePerSPRanking[0].Any(x => x.Item2 == 8))
                                {
                                    int highestGroupDamageForSP8 = damagePerSPRanking[0].First(x => x.Item2 == 8).Item1;

                                    if (damagePerSPRanking.Count > 2) // If there is a second highest group
                                    {
                                        for (int i = 1; i < damagePerSPRanking.Count; i++)
                                        {
                                            if (damagePerSPRanking[i].Any(x => x.Item2 == 1 || x.Item2 == 2))
                                            {
                                                Tuple<int, int> groupIdamage_spRequired = damagePerSPRanking[i].First(x => x.Item2 == 1 || x.Item2 == 2);
                                                maxPossibleDamage += highestGroupDamageForSP8 + groupIdamage_spRequired.Item1 * (2 / groupIdamage_spRequired.Item2); // [0]D8 + (2 * [i]D1 or [i]D2)
                                                break;
                                            }

                                            if (i == damagePerSPRanking.Count - 1)
                                                maxPossibleDamage += highestGroupDamageForSP8; // [0]D8
                                        }
                                    }
                                    else // If there is not a second highest group
                                        maxPossibleDamage += highestGroupDamageForSP8; // [0]D8
                                }
                                else // if (damagePerSPRanking[0].Any(x => x.Item2 == 9))
                                {
                                    int highestGroupDamageForSP9 = damagePerSPRanking[0].First(x => x.Item2 == 9).Item1;

                                    if (damagePerSPRanking.Count > 2) // If there is a second highest group
                                    {
                                        for (int i = 1; i < damagePerSPRanking.Count; i++)
                                        {
                                            if (damagePerSPRanking[i].Any(x => x.Item2 == 1))
                                            {
                                                maxPossibleDamage += highestGroupDamageForSP9 + damagePerSPRanking[i].First(x => x.Item2 == 1).Item1; // [0]D9 + [i]D1
                                                break;
                                            }

                                            if (i == damagePerSPRanking.Count - 1)
                                                maxPossibleDamage += highestGroupDamageForSP9; // [0]D9
                                        }
                                    }
                                    else // If there is not a second highest group
                                        maxPossibleDamage += highestGroupDamageForSP9; // [0]D9
                                }
                            }
                        }
                    }
                    */
                }
            }

            return maxPossibleDamage;
        }

        async Task<List<SkillUserDamageInfo>> GetMaxPossibleDamagePerEnemyActionToAllyOnLocation(BattleSystemCore _system, UnitInstance _ally)
        {
            if (_system == null || _ally == null)
                return null;

            List<SkillUserDamageInfo> result = new List<SkillUserDamageInfo>();

            _2DCoord allyLocation = _system.Field.UnitLocation(_ally);

            List<Task> tasks = new List<Task>();
            foreach (UnitInstance enemyAttackerCandidate in m_aliveEnemies) // For each alive enemy
            {
                tasks.Add(AddMaxPossibleDamageForEachEnemyActionToAllyOnLocation(result, _system, _ally, allyLocation, enemyAttackerCandidate));
            }
            await Task.WhenAll(tasks);

            return result;
        }

        private async Task AddMaxPossibleDamageForEachEnemyActionToAllyOnLocation(List<SkillUserDamageInfo> _maxPossibleDamagePerEnemyActionToAllyOnLocation, BattleSystemCore _system, UnitInstance _ally, _2DCoord _allyLocation, UnitInstance _enemyAttackerCandidate)
        {
            if (_maxPossibleDamagePerEnemyActionToAllyOnLocation == null
                || _system == null
                || _ally == null
                || _allyLocation == null
                || _enemyAttackerCandidate == null)
            {
                return;
            }

            // Check whether the enemy's attack may target the ally
            {
                List<_2DCoord> enemyAttackTargetableAndSelectableArea = m_system.GetAttackTargetableAndSelectableArea(_enemyAttackerCandidate).GetKeysWithValue(true);
                if (enemyAttackTargetableAndSelectableArea.Contains(_allyLocation)) // If the ally is within the enemy's attack area
                {
                    OrdinarySkill skill = GameDataContainer.Instance.BasicAttackSkill;

                    Tuple<int, int> tmp_minAndMaxPossibleDamage = GetMinAndMaxPossibleDamage(_system, _enemyAttackerCandidate, skill, enemyAttackTargetableAndSelectableArea, _ally);
                    _maxPossibleDamagePerEnemyActionToAllyOnLocation.Add(new SkillUserDamageInfo(_enemyAttackerCandidate, tmp_minAndMaxPossibleDamage.Item2, 0, true));
                }
            }

            // Check whether the enemy's ordinary skills with an effect classification of DamageEffect may target the Unit
            List<Task> tasks = new List<Task>();
            foreach (ActiveSkill skill in _enemyAttackerCandidate.Skills.OfType<ActiveSkill>().Where(x => !(x is CounterSkill) && x.BaseInfo.Effect is DamageEffect)) // Iterate through skills that have a damaging effect as its main effect
            {
                tasks.Add(AddMaxPossibleDamageForEnemySkillToAllyOnLocation(_maxPossibleDamagePerEnemyActionToAllyOnLocation, _system, _ally, _allyLocation, _enemyAttackerCandidate, skill));
            }
            await Task.WhenAll(tasks);
        }

        private async Task AddMaxPossibleDamageForEnemySkillToAllyOnLocation(List<SkillUserDamageInfo> _maxPossibleDamagePerEnemyActionToAllyOnLocation, BattleSystemCore _system, UnitInstance _ally, _2DCoord _allyLocation, UnitInstance _enemyAttackerCandidate, ActiveSkill _skill)
        {
            List<_2DCoord> enemySkillTargetableAndSelectableArea = m_system.GetSkillTargetableAndSelectableAreaForSimulation(_enemyAttackerCandidate, _skill).GetKeysWithValue(true);
            if (enemySkillTargetableAndSelectableArea.Contains(_allyLocation)) // If the Unit is within the enemy's skill area
            {
                Tuple<int, int> tmp_minAndMaxPossibleDamage = GetMinAndMaxPossibleDamage(_system, _enemyAttackerCandidate, _skill, enemySkillTargetableAndSelectableArea, _ally);
                int spRequired = (_skill is CostRequiringSkill) ? (_skill as CostRequiringSkill).BaseInfo.SPCost : 0;
                _maxPossibleDamagePerEnemyActionToAllyOnLocation.Add(new SkillUserDamageInfo(_enemyAttackerCandidate, tmp_minAndMaxPossibleDamage.Item2, spRequired, false));
            }
        }

        private Tuple<int, int> GetMinAndMaxPossibleDamage(BattleSystemCore _system, UnitInstance _attacker, ActiveSkill _skill, List<_2DCoord> _targetableAndSelectableArea, UnitInstance _target)
        {
            if (!(_skill.BaseInfo.Effect is DamageEffect))
                return null;

            List<_2DCoord> effectRange = _system.GetSkillTargetableArea(_attacker, _skill);
            int maxNumOfTargets = Calculator.MaxNumOfTargets(_attacker, _skill, effectRange, _system);

            // Check the minimum and maximum possible damage that the Unit might receive, considering the all cases where the opponent player decides to target the Unit
            {
                List<UnitInstance> targetCandidates = m_field.GetUnitsInCoords(_targetableAndSelectableArea);
                targetCandidates.Remove(_target); // Remove Unit from the list of candidates, because Unit will always be a target in this simulation
                List<List<UnitInstance>> targetCombinations = targetCandidates.GetCombinations(1, maxNumOfTargets - 1); // Get all possible combinations of targets for the skill

                int minPossibleDamage = 0;
                int maxPossibleDamage = 0;
                foreach (List<UnitInstance> targets in targetCombinations)
                {
                    decimal possibleDamage = Calculator.PossibleNonCriticalDamage(_system, _attacker, m_field.UnitLocation(_attacker), _skill, _skill.BaseInfo.Effect as DamageEffect, _system.GetAttackTargetableArea(_attacker), targets, _target, out eEffectiveness effectiveness); // Get the possible damage to the Unit without it being critical

                    int tmp_minPossibleDamage = Convert.ToInt32(Math.Floor(possibleDamage));
                    int tmp_maxPossibleDamage = Convert.ToInt32(Math.Floor(possibleDamage * CoreValues.MULTIPLIER_FOR_CRITICALHIT)) * _skill.BaseInfo.Effect.TimesToApply.ToValue<int>(_system, null, null, _attacker, _skill, _skill.BaseInfo.Effect, effectRange, new List<object>(targets), _target); // For the max possible damage, consider the case that all repetitions of the effect succeed as critical hit

                    if (minPossibleDamage > tmp_minPossibleDamage)
                        minPossibleDamage = tmp_minPossibleDamage;
                    if (maxPossibleDamage < tmp_maxPossibleDamage)
                        maxPossibleDamage = tmp_maxPossibleDamage;
                }

                return new Tuple<int, int>(minPossibleDamage, maxPossibleDamage);
            }
        }

        private class DamageSkillResult
        {
            public DamageSkillResult(UnitInstance _attacker, ActiveSkill _skill)
            {
                Attacker = _attacker;
                Skill = _skill;
                DamagePerTarget = new List<Tuple<UnitInstance, int>>();
            }

            public UnitInstance Attacker { get; }

            public ActiveSkill Skill { get; }

            public List<Tuple<UnitInstance, int>> DamagePerTarget { get; }

            public int TotalDamage
            {
                get
                {
                    int result = 0;
                    foreach (var target_damage in DamagePerTarget)
                    {
                        result += target_damage.Item2;
                    }
                    return result;
                }
            }

            public int SPRequired { get { return (Skill is OrdinarySkill) ? (Skill as OrdinarySkill).BaseInfo.SPCost : 0; } }

            public List<UnitInstance> Targets
            {
                get
                {
                    List<UnitInstance> result = new List<UnitInstance>();
                    foreach (var target_damage in DamagePerTarget)
                    {
                        result.Add(target_damage.Item1);
                    }
                    return result;
                }
            }

            public int GetDamageForTarget(UnitInstance _target)
            {
                var target_damage = DamagePerTarget.FirstOrDefault(x => x.Item1 == _target);
                if (target_damage != null)
                    return target_damage.Item2;
                else
                    return 0;
            }
        }

        async Task<DamageSkillResult> GetMaxPossibleDamageSkillOption(BattleSystemCore _system, UnitInstance _attacker, ActiveSkill _skill)
        {
            if (!(_skill.BaseInfo.Effect is DamageEffect))
                return null;

            DamageSkillResult result = new DamageSkillResult(_attacker, _skill);

            List<_2DCoord> effectRange = _system.GetSkillTargetableArea(_attacker, _skill);
            List<_2DCoord> skillTargetableAndSelectableArea = _system.GetSkillTargetableAndSelectableAreaForSimulation(_attacker, _skill).GetKeysWithValue(true);
            int maxNumOfTargets = Calculator.MaxNumOfTargets(_attacker, _skill, effectRange, _system);

            // Check the minimum and maximum possible damage that the Unit might receive, considering the all cases where the opponent player decides to target the Unit
            {
                List<UnitInstance> targetCandidates = m_field.GetUnitsInCoords(skillTargetableAndSelectableArea);
                List<List<UnitInstance>> targetCombinations = targetCandidates.GetCombinations(1, maxNumOfTargets); // Get all possible combinations of targets for the skill

                foreach (List<UnitInstance> targets in targetCombinations)
                {
                    List<Tuple<UnitInstance, int>> tmp_possibleDamageSkillOptionSet = new List<Tuple<UnitInstance, int>>();
                    List<Task> tasks = new List<Task>();
                    foreach (UnitInstance target in targets)
                    {
                        tasks.Add(AddPossibleDamageSkillOption(tmp_possibleDamageSkillOptionSet, _system, _attacker, _skill, effectRange, targets, target));
                    }
                    await Task.WhenAll();

                    int tmp_totalDamage = 0;
                    foreach (Tuple<UnitInstance, int> possibleDamageSkillOption in tmp_possibleDamageSkillOptionSet)
                    {
                        tmp_totalDamage += possibleDamageSkillOption.Item2;
                    }
                    if (tmp_totalDamage > result.TotalDamage)
                    {
                        result.DamagePerTarget.Clear();
                        foreach (var target_damage in tmp_possibleDamageSkillOptionSet)
                        {
                            result.DamagePerTarget.Add(target_damage);
                        }
                    }
                }

                return result;
            }
        }

        async Task AddPossibleDamageSkillOption(List<Tuple<UnitInstance, int>> _possibleDamageSkillOptionSet, BattleSystemCore _system, UnitInstance _attacker, ActiveSkill _skill, List<_2DCoord> _effectRange, List<UnitInstance> _targets, UnitInstance _target)
        {
            decimal possibleDamage = Calculator.PossibleNonCriticalDamage(_system, _attacker, m_field.UnitLocation(_attacker), _skill, _skill.BaseInfo.Effect as DamageEffect, _system.GetAttackTargetableArea(_attacker), _targets, _target, out eEffectiveness effectiveness); // Get the possible damage to the Unit without it being critical

            int tmp_maxPossibleDamage = Convert.ToInt32(Math.Floor(possibleDamage * CoreValues.MULTIPLIER_FOR_CRITICALHIT)) * _skill.BaseInfo.Effect.TimesToApply.ToValue<int>(_system, null, null, _attacker, _skill, _skill.BaseInfo.Effect, _effectRange, new List<object>(_targets), _target); // For the max possible damage, consider the case that all repetitions of the effect succeed as critical hit

            _possibleDamageSkillOptionSet.Add(new Tuple<UnitInstance, int>(_target, tmp_maxPossibleDamage));
        }

        private class DamageToEachAllyInAParticularSituation
        {
            public DamageToEachAllyInAParticularSituation(UnitInstance _moverAlly, _2DCoord _moverAllyDestination, UnitInstance _moverEnemy, _2DCoord _moverEnemyDestination)
            {
                MoverAlly = _moverAlly;
                MoverAllyDestination = _moverAllyDestination;
                MoverEnemy = _moverEnemy;
                MoverEnemyDestination = _moverEnemyDestination;

                DamagePerAlly = new List<DamageOnCoordForUnit>();
            }

            public UnitInstance MoverAlly { get; }
            public _2DCoord MoverAllyDestination { get; }
            public UnitInstance MoverEnemy { get; }
            public _2DCoord MoverEnemyDestination { get; }
            public List<DamageOnCoordForUnit> DamagePerAlly { get; }
        }

        //private class DamageMap
        //{
        //    public DamageMap(UnitInstance _unit)
        //    {
        //        Unit = _unit;
        //        Coord_Damage = new List<Tuple<_2DCoord, int>>();
        //    }

        //    public UnitInstance Unit { get; }
        //    public List<Tuple<_2DCoord, int>> Coord_Damage { get; }
        //}

        private class DamageOnCoordForUnit
        {
            public DamageOnCoordForUnit(UnitInstance _unit, _2DCoord _coord, int _damage)
            {
                Unit = _unit;
                Coord = _coord;
                Damage = _damage;
            }

            public UnitInstance Unit { get; }
            public _2DCoord Coord { get; }
            public int Damage { get; }
        }
    }
}