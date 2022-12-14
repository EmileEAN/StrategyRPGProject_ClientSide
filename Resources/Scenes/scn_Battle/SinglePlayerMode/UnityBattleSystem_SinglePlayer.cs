using EEANWorks.Games.TBSG._01.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(AnimationController_SinglePlayer))]
    public class UnityBattleSystem_SinglePlayer : MonoBehaviour
    {

        #region Serialized Field
        public GameObject UnitPrefab;
        #endregion

        #region Properties
        public GameObject[] GOs_Player { get; private set; }
        public List<PlayerController_SinglePlayer> PlayerControllers { get; private set; }
        public PlayerController_SinglePlayer PlayerController { get; private set; } // Not CPU

        public List<GameObject> GOs_Unit { get; private set; }
        public List<GameObject> GOs_Player1Unit { get; private set; }
        public List<GameObject> GOs_Player2Unit { get; private set; }

        public BattleSystemCore BattleSystemCore { get; private set; }

        public bool IsInitialized { get; private set; }
        #endregion

        #region Private Fields
        private bool m_isInitializing;

        private bool m_arePlayersSet;

        private AnimationController_SinglePlayer m_animationController;

        private TileMaskManager_SinglePlayer m_tileMaskManager;
        private GameObject m_go_gameBoardSet;
        private Transform m_transform_baseTile;
        private Vector2 m_tileSize;

        private int m_selectedTeamIndex;

        private List<List<eTileType>> m_tileSets;
        private List<List<FloorInstanceMemberInfo>> m_floorInstanceInfos;
        private int m_lastFloorNumber;
        private int m_currentFloorNumber;

        private bool m_areFloorDataLoaded;
        private bool m_areOpponentTeamsLoaded;
        private bool m_isFieldSet;
        private bool m_areUnitsSet;

        private bool m_processingFloorTransitionOrSceneExit;
        #endregion

        // Use this for Initialization of simple properties
        void Awake()
        {
            IsInitialized = false;
            m_isInitializing = false;
            m_areFloorDataLoaded = false;
            m_areOpponentTeamsLoaded = false;
            m_isFieldSet = false;
            m_areUnitsSet = false;
            m_arePlayersSet = false;

            m_processingFloorTransitionOrSceneExit = false;

            m_selectedTeamIndex = GameDataContainer.Instance.Player.Teams.IndexOf(GameDataContainer.Instance.SelectedTeam);

            m_tileSets = new List<List<eTileType>>();
            m_floorInstanceInfos = new List<List<FloorInstanceMemberInfo>>();
            m_lastFloorNumber = GameDataContainer.Instance.DungeonToPlay.Floors.Count;
            m_currentFloorNumber = 1;

            m_animationController = this.GetComponent<AnimationController_SinglePlayer>();
            m_go_gameBoardSet = GameObject.FindGameObjectWithTag("GameBoard");
            m_tileMaskManager = m_go_gameBoardSet.GetComponent<TileMaskManager_SinglePlayer>();
            m_transform_baseTile = m_go_gameBoardSet.transform.Find("Tile0");
            MeshRenderer meshRenderer = m_transform_baseTile.GetComponent<MeshRenderer>();
            m_tileSize = new Vector2(meshRenderer.bounds.size.x, meshRenderer.bounds.size.z);

            GOs_Unit = new List<GameObject>();
            GOs_Player1Unit = new List<GameObject>();
            GOs_Player2Unit = new List<GameObject>();

            PlayerControllers = new List<PlayerController_SinglePlayer>();
        }

        // Use this for detailed initialization
        // Update() is being used to make sure that all Player Instances are created before the detailed initialization
        void Update()
        {
            if (!IsInitialized && !m_isInitializing)
                StartCoroutine(Initialize());

            if (IsInitialized)
            {
                if (BattleSystemCore.IsMatchEnd && !m_processingFloorTransitionOrSceneExit)
                {
                    if (m_currentFloorNumber < m_lastFloorNumber) // If the floor won is not the last one
                        ToNextFloor();
                    else
                        SceneExit();
                }
            }
        }

        IEnumerator Initialize()
        {
            Debug.Log("BattleSystem_SinglePlayer: Start Initialization.");

            m_isInitializing = true;

            if (!m_arePlayersSet)
            {
                if (GOs_Player == null || GOs_Player.Length < 2)
                    GOs_Player = GameObject.FindGameObjectsWithTag("Player");
                if (GOs_Player.Length < 2) // if not all Players have been spawned
                {
                    m_isInitializing = false;
                    yield break; // Stop Initialization
                }

                PlayerControllers.Clear(); // Get the primary script of each Player Object
                foreach (GameObject go_player in GOs_Player)
                {
                    PlayerController_SinglePlayer playerController = go_player.GetComponent<PlayerController_SinglePlayer>();
                    if (playerController == null || !playerController.IsInitialized)
                    {
                        m_isInitializing = false;
                        yield break; // Stop Initialization
                    }

                    PlayerControllers.Add(playerController);
                }

                if (PlayerController == null)
                    PlayerController = PlayerControllers.First(x => !x.IsCPU); // Set reference to the local player's PlayerController

                if (!SetPlayOrder()) // If failed to set Playing Order
                {
                    m_isInitializing = false;
                    yield break; // Stop Initialization
                }

                Debug.Log("Players set successfully!");
                m_arePlayersSet = true;
            }

            if (!m_areFloorDataLoaded)
            {
                // Load the sets of available tiles per floor
                for (int i = 0; i < m_lastFloorNumber; i++)
                {
                    int tileSetId = GameDataContainer.Instance.DungeonToPlay.Floors[i].TileSetId;
                    m_tileSets.Add(GameDataContainer.Instance.TileSets[tileSetId].TileTypes.ToList());
                }

                m_floorInstanceInfos = GameDataContainer.Instance.FloorInstanceInfos;

                //Check whether the number of loaded tile sets and floor instance infos match the number of floors in the dungeon; and whether all tile sets have at least one tile type
                if (m_tileSets.Count == m_lastFloorNumber && m_tileSets.All(x => x.Count > 0) && m_floorInstanceInfos.Count == m_lastFloorNumber)
                {
                    Debug.Log("Tile data set successfully!");
                    m_areFloorDataLoaded = true;
                }
                else
                {
                    m_isInitializing = false;
                    yield break; // Stop Initialization
                }
            }

            if (!m_areOpponentTeamsLoaded)
            {
                Player opponent = PlayerControllers.First(x => x.IsCPU).PlayerData as Player;
                opponent.Teams = new List<Team>();
                int uniqueId = -1;
                for (int floorIndex = 0; floorIndex < m_lastFloorNumber; floorIndex++)
                {
                    List<Unit> members = new List<Unit>();

                    List<FloorInstanceMemberInfo> memberInfos = m_floorInstanceInfos[floorIndex];
                    int memberCount = (memberInfos.Count > CoreValues.MAX_MEMBERS_PER_TEAM) ? CoreValues.MAX_MEMBERS_PER_TEAM : memberInfos.Count;
                    for (int memberIndex = 0; memberIndex < memberCount; memberIndex++)
                    {
                        FloorInstanceMemberInfo memberInfo = memberInfos[memberIndex];

                        List<Skill> skills = new List<Skill>();
                        foreach (SkillData skillData in memberInfo.UnitData.Skills)
                        {
                            int skillLevel = ((memberInfo.UnitLevel - 1) / 10) + 1; // If unit level is 1 ~ 10, skill level will be 1. If 11 ~ 20, it will be 2, and so on...
                            skills.Add(skillData.ToSkill(skillLevel));
                        }

                        Unit member = new Unit(memberInfo.UnitData, uniqueId, default, Calculator.MinimumAccumulatedExperienceRequired(memberInfo.UnitLevel), default, skills);
                        members.Add(member);

                        uniqueId--;
                    }

                    opponent.Teams.Add(new Team(members, null));

                    m_areOpponentTeamsLoaded = true;
                }
            }

            yield return StartCoroutine(InitializeField());

            if (m_arePlayersSet
                && m_areFloorDataLoaded
                && m_areOpponentTeamsLoaded
                && m_isFieldSet
                && m_areUnitsSet)
            {
                m_isInitializing = false;
                IsInitialized = true;
                Debug.Log("BattleSystem_SinglePlayer: End Initialization.");
            }
        }

        IEnumerator InitializeField()
        {
            if (IsInitialized) // If it is not the first floor
            {
                // Invert the player order so that each player has the chance to start the round
                {
                    {
                        GameObject tmp = GOs_Player[0];
                        GOs_Player[0] = GOs_Player[1];
                        GOs_Player[1] = tmp;
                    }

                    {
                        PlayerController_SinglePlayer tmp = PlayerControllers[0];
                        PlayerControllers[0] = PlayerControllers[1];
                        PlayerControllers[1] = tmp;
                    }

                    PlayerControllers[0].SyncProperties(1); // Giving it an Id of 1 so that it knows it is Player 1. Starts first
                    PlayerControllers[1].SyncProperties(2); // Giving it an Id of 2 so that it knows it is Player 2. Starts after Player 1
                }
            }

            yield return StartCoroutine(SetField());

            yield return StartCoroutine(SetUnits());
        }

        IEnumerator SetField()
        {
            if (!IsInitialized && m_isFieldSet)
                yield break;

            bool firstInitialization = !IsInitialized && !m_isFieldSet;

            int player1TeamIndex = (PlayerControllers[0] == PlayerController) ? m_selectedTeamIndex : m_currentFloorNumber - 1;
            int player2TeamIndex = (PlayerControllers[1] == PlayerController) ? m_selectedTeamIndex : m_currentFloorNumber - 1;
            Field field = Field.NewField(PlayerControllers[0].PlayerData as Player, player1TeamIndex, PlayerControllers[1].PlayerData as Player, player2TeamIndex, m_tileSets[m_currentFloorNumber - 1]);
            BattleSystemCore = new BattleSystemCore(field);
            if (firstInitialization && BattleSystemCore == null) // if FieldInstance was not created successfully
                yield break;

            // Sync player data values
            for (int i = 0; i < GOs_Player.Length; i++)
            {
                PlayerControllers[i].PlayerData = BattleSystemCore.Field.Players[i]; // Swap PlayerData (currently instance of Player class) with instance of newly created PlayerOnBoard (child class of Player class)
                PlayerControllers[i].SyncProperties();
            }

            if (firstInitialization)
            {
                Debug.Log("Field set successfully!");
                m_isFieldSet = true;
            }
        }

        IEnumerator SetUnits()
        {
            if (!IsInitialized && m_areUnitsSet)
                yield break;

            bool firstInitialization = !IsInitialized && !m_areUnitsSet;

            GOs_Unit.Clear(); // Reset List in case it already contains game objects
            foreach (UnitInstance unit in BattleSystemCore.Field.Players[0].AlliedUnits) // Spawn Units owned by Player 1
            {
                if (!SpawnUnit(unit, 0) && firstInitialization)
                    yield break;
            }

            foreach (UnitInstance unit in BattleSystemCore.Field.Players[1].AlliedUnits) // Spawn Units owned by Player 2
            {
                if (!SpawnUnit(unit, 1) && firstInitialization)
                    yield break;
            }

            if (firstInitialization)
            {
                Debug.Log("Units set successfully!");
                m_areUnitsSet = true;
            }
        }

        public void ChangeTurn()
        {
            if (!m_animationController.IsInitialized || m_animationController.LockUI)
                return;

            BattleSystemCore.ChangeTurn();
            foreach (PlayerController_SinglePlayer playerController in PlayerControllers)
            {
                playerController.SyncProperties();
            }
        }

        public void Concede()
        {
            if (!m_animationController.IsInitialized || m_animationController.LockUI)
                return;

            int currentPlayerIndex = PlayerControllers[0].IsMyTurn ? 0 : 1;
            int currentPlayerId = PlayerControllers[currentPlayerIndex].PlayerId;

            BattleSystemCore.EndMatch(BattleSystemCore.Field.Players[currentPlayerId - 1]);
        }

        private void ToNextFloor()
        {
            m_processingFloorTransitionOrSceneExit = true;

            m_currentFloorNumber++;

            StartCoroutine(ProcessFloorTransition());
        }
        IEnumerator ProcessFloorTransition()
        {
            yield return StartCoroutine(m_animationController.FadeUIOut());

            yield return StartCoroutine(m_animationController.FadeDoorIn());

            yield return StartCoroutine(InitializeField());

            yield return StartCoroutine(m_animationController.AnimateDoor());

            yield return StartCoroutine(m_animationController.FadeUIIn());

            m_processingFloorTransitionOrSceneExit = false;
        }

        private void SceneExit()
        {
            m_processingFloorTransitionOrSceneExit = true;

            bool isPlayerWinner = BattleSystemCore.IsPlayer1Winner;

            if (PlayerController.PlayerId != 1)
                isPlayerWinner = !isPlayerWinner;

            StartCoroutine(ProcessSceneExit(isPlayerWinner));
        }
        IEnumerator ProcessSceneExit(bool _isPlayerWinner)
        {
            yield return StartCoroutine(m_animationController.FadeMatchResultPanelIn(_isPlayerWinner));

            m_processingFloorTransitionOrSceneExit = false;

            if (_isPlayerWinner)
                SceneConnector.GoToScene("scn_DungeonRewards");
            else
                SceneConnector.GoToScene("scn_StorySelection");
        }

        private bool SpawnUnit(UnitInstance _unit, int _playerIndex)
        {
            try
            {
                Debug.Log("BattleSystem_SinglePlayer: Start Unit Spawning.");

                GameObject go_unit = Instantiate(UnitPrefab, Vector3.zero, Quaternion.identity, this.transform.root);

                int unitIndex = BattleSystemCore.Field.GetUnitIndex(_unit);
                go_unit.name = unitIndex.ToString();
                _2DCoord coord = BattleSystemCore.Field.UnitLocation(_unit); // Get location as logical 2D Coordinate (x, y) --- this is not Unity coordinate
                go_unit.transform.position = GetActualPosition(coord); // Get and Set the actual position in the Unity world
                if (_playerIndex != 0)
                    go_unit.transform.Rotate(0f, 180f, 0f); //Rotate 180 degrees to adjust player 2's units' direction facing.

                //Return false if not succeeded
                if (!go_unit.GetComponent<UnitController_SinglePlayer>()
                    .SetInitializationData(BattleSystemCore.Field.Units.IndexOf(_unit),
                                           BattleSystemCore.Field.Players[_playerIndex].AlliedUnits.IndexOf(_unit),
                                           _playerIndex + 1,
                                           _unit))
                {
                    Destroy(go_unit); // destroy not to leave the unsynced instance in unity world
                    return false;
                }

                if (_playerIndex == 0)
                    GOs_Player1Unit.Add(go_unit);
                else
                    GOs_Player2Unit.Add(go_unit);

                GOs_Unit.Add(go_unit);

                Debug.Log("BattleSystem_SinglePlayer: End Unit Spawning.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at SpawnUnit() " + ex.Message);
                return false;
            }
        }

        public Vector3 GetActualPosition(_2DCoord _coord)
        {
            try
            {
                int tileNum = _coord.Y * CoreValues.SIZE_OF_A_SIDE_OF_BOARD + _coord.X;
                Transform transform_tileMask = GameObject.Find("Tile" + tileNum.ToString()).transform.Find("Mask" + tileNum.ToString());

                float adjustmentValueY = 0.01f;

                float x = transform_tileMask.position.x;
                float y = transform_tileMask.position.y + adjustmentValueY;
                float z = transform_tileMask.position.z;

                return new Vector3(x, y, z);
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at GetActualPosition() " + ex.Message);
                return new Vector3(-1, -1, -1);
            }
        }

        public void MoveUnit(_2DCoord _destination)
        {
            try
            {
                PlayerOnBoard pob = BattleSystemCore.CurrentTurnPlayer;
                UnitInstance unit = pob.AlliedUnits[pob.SelectedUnitIndex];

                BattleSystemCore.MoveUnit(unit, _destination);

                PlayerController_SinglePlayer playerController = PlayerControllers.First(x => x.PlayerData == pob);
                playerController.SyncProperties();

                UpdateMovableArea(playerController.PlayerId, pob.SelectedUnitIndex);
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at MoveUnit() " + ex.Message);
            }
        }

        public void Attack(List<_2DCoord> _targetCoords)
        {
            try
            {
                PlayerOnBoard pob = BattleSystemCore.CurrentTurnPlayer;
                UnitInstance unit = pob.AlliedUnits[pob.SelectedUnitIndex];

                BattleSystemCore.RequestAttack(unit, _targetCoords);

                PlayerController_SinglePlayer playerController = PlayerControllers.First(x => x.PlayerData == pob);
                playerController.SyncProperties();

                UpdateAttackTargetableArea(playerController.PlayerId, pob.SelectedUnitIndex);
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at Attack() " + ex.Message);
            }
        }

        public void UseSkill(ActiveSkill _skill, List<_2DCoord> _targetCoords, List<_2DCoord> _secondaryTargetCoords = null) { UseSkill(_skill.BaseInfo.Name, _targetCoords, _secondaryTargetCoords); }
        public void UseSkill(string _skillName, List<_2DCoord> _targetCoords, List<_2DCoord> _secondaryTargetCoords = null)
        {
            try
            {
                PlayerOnBoard pob = BattleSystemCore.CurrentTurnPlayer;
                UnitInstance unit = pob.AlliedUnits[pob.SelectedUnitIndex];

                CostRequiringSkill skill = unit.Skills.OfType<CostRequiringSkill>().First(x => x.BaseInfo.Name == _skillName);

                BattleSystemCore.RequestSkillUse(unit, skill, _targetCoords, _secondaryTargetCoords);

                PlayerController_SinglePlayer playerController = PlayerControllers.First(x => x.PlayerData == pob);
                playerController.SyncProperties();

                UpdateSkillTargetableArea(playerController.PlayerId, pob.SelectedUnitIndex, _skillName);
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at Attack() " + ex.Message);
            }
        }

        //public void UseUltimateSkill(string _skillName, List<_2DCoord> _targetCoords)
        //{
        //
        //
        //
        //}

        //public void RequestSkillCost(int _playerId, int _unitIndex, string _skillName)
        //{
        //    int playerIndex = _playerId - 1; // _playerId is one-based and playerIndex needs to be zero-based

        //    PlayerOnBoard pob = BattleSystemCore.Field.Players[playerIndex];
        //    UnitInstance unit = pob.AlliedUnits[_unitIndex];

        //    int requiredSP = (unit.BaseInfo.Skills.First(x => x.Name == _skillName) as CostRequiringSkill).SPCost;
        //}

        //private void SyncUnitStatus_All()
        //{
        //    try
        //    {
        //        foreach (GameObject Unit in Player1Units)
        //        {
        //            UnitController_SinglePlayer UnitController = Unit.GetComponent<UnitController_SinglePlayer>();

        //            UnitInstance unit = BattleSystemCore.Field.Players[0].AlliedUnits[UnitController.Unit_PrivateId];

        //            //UnitController.Rpc_SyncStatus(Calculator.MaxHP(unit), unit.RemainingHP, unit.isAlive);
        //        }

        //        foreach (GameObject Unit in Player2Units)
        //        {
        //            UnitController_SinglePlayer UnitController = Unit.GetComponent<UnitController_SinglePlayer>();

        //            UnitInstance unit = BattleSystemCore.Field.Players[1].AlliedUnits[UnitController.Unit_PrivateId];

        //            //UnitController.Rpc_SyncStatus(Calculator.MaxHP(unit), unit.RemainingHP, unit.isAlive);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Log("BattleSystem_SinglePlayer: at SyncUnitStatus_All() " + ex.Message);
        //    }
        //}

        public void ChangeUnit(int _playerId, int _unitIndex)
        {
            try
            {
                if (m_animationController.LockUI)
                    return;

                int internalPlayerId = _playerId - 1;

                PlayerOnBoard pob = BattleSystemCore.Field.Players[internalPlayerId];

                if (_unitIndex != pob.SelectedUnitIndex)
                {
                    BattleSystemCore.ChangeSelectedUnit(pob, _unitIndex);
                    Debug.Log("Selected Unit has been changed");

                    PlayerControllers[internalPlayerId].SyncProperties();

                    Debug.Log("Unit Selection Info was Synced!");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at ChangeUnit() " + ex.Message);
            }
        }

        public void UpdateMovableArea(int _playerId, int _unitIndex)
        {
            try
            {
                int playerIndex = _playerId - 1; // _playerId is one-based and playerIndex needs to be zero-based

                PlayerOnBoard pob = BattleSystemCore.Field.Players[playerIndex];
                UnitInstance unit = pob.AlliedUnits[_unitIndex];

                m_tileMaskManager.MaxNumOfTargets = 1;
                m_tileMaskManager.UpdateTargetArea(BattleSystemCore.GetMovableAndSelectableArea(unit));
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at UpdateMovableArea() " + ex.Message + ex.InnerException);
            }
        }

        public void UpdateAttackTargetableArea(int _playerId, int _unitIndex)
        {
            try
            {
                int playerIndex = _playerId - 1; // _playerId is one-based and playerIndex needs to be zero-based

                PlayerOnBoard pob = BattleSystemCore.Field.Players[playerIndex];
                UnitInstance unit = pob.AlliedUnits[_unitIndex];

                m_tileMaskManager.MaxNumOfTargets = Calculator.MaxNumOfTargets(unit, GameDataContainer.Instance.BasicAttackSkill, BattleSystemCore.GetAttackTargetableArea(unit), BattleSystemCore);
                m_tileMaskManager.UpdateTargetArea(BattleSystemCore.GetAttackTargetableAndSelectableArea(unit));
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at UpdateAttackTargetableArea() " + ex.Message);
            }
        }

        public void UpdateSkillTargetableArea(int _playerId, int _unitIndex, string _skillName)
        {
            try
            {
                int playerIndex = _playerId - 1; // _playerId is one-based and playerIndex needs to be zero-based

                PlayerOnBoard pob = BattleSystemCore.Field.Players[playerIndex];
                UnitInstance unit = pob.AlliedUnits[_unitIndex];

                var skill = unit.Skills.OfType<ActiveSkill>().First(x => x.BaseInfo.Name == _skillName);

                m_tileMaskManager.MaxNumOfTargets = Calculator.MaxNumOfTargets(unit, skill, BattleSystemCore.GetSkillTargetableArea(unit, skill), BattleSystemCore);

                if (skill is CostRequiringSkill)
                    m_tileMaskManager.UpdateTargetArea(BattleSystemCore.GetSkillTargetableAndSelectableArea(unit, skill as CostRequiringSkill));
                else if (skill is UltimateSkill)
                    m_tileMaskManager.UpdateTargetArea(BattleSystemCore.GetSkillTargetableAndSelectableArea(unit, skill as UltimateSkill));
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at UpdateSkillTargetableArea() " + ex.Message);
            }
        }

        public void UpdateTargetableAreaToNull() { m_tileMaskManager.UpdateTargetArea(null); }

        public bool SetPlayOrder()
        {
            try
            {
                MTRandom.RandInit();
                int randNum = MTRandom.GetRandInt(1, 10); //Randomly select which Player will start playing first
                if (randNum > 5)
                {
                    //Invert the player order in list
                    {
                        GameObject tmp = GOs_Player[0];
                        GOs_Player[0] = GOs_Player[1];
                        GOs_Player[1] = tmp;
                    }

                    {
                        PlayerController_SinglePlayer tmp = PlayerControllers[0];
                        PlayerControllers[0] = PlayerControllers[1];
                        PlayerControllers[1] = tmp;
                    }
                }
                //else if (randNum <= 5)
                //Do nothing;

                PlayerControllers[0].SyncProperties(1); // Giving it an Id of 1 so that it knows it is Player 1. Starts first
                PlayerControllers[1].SyncProperties(2); // Giving it an Id of 2 so that it knows it is Player 2. Starts after Player 1

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_SinglePlayer: at SetPlayOrder() " + ex.Message);
                return false;
            }
        }
    }
}