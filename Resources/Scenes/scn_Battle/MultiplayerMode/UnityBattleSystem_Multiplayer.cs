using static EEANWorks.DictionaryExtension;
using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.Unity.Engine;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(AnimationController_Multiplayer))]
    public class UnityBattleSystem_Multiplayer : MonoBehaviour
    {
        #region Serialized Field
        public GameObject PlayerUnitPrefab;
        public GameObject OpponentUnitPrefab;
        #endregion

        #region Properties
        public IList<EventLog> EventLogs { get { return m_eventLogs.AsReadOnly(); } }

        public GameObject GO_Player { get; private set; }
        public GameObject GO_Opponent { get; private set; }

        public List<GameObject> GOs_Unit { get; private set; }
        public List<GameObject> GOs_PlayerUnit { get; private set; }
        public List<GameObject> GOs_OpponentUnit { get; private set; }

        public PlayerController_Multiplayer PlayerController { get; private set; }
        public PlayerOnBoard PlayerData { get; private set; }

        public string Opponent_Name { get; private set; }
        public List<OpponentUnitInfo> OpponentUnitInfos { get; private set; }

        public eTileType[,] TileMap { get; private set; }

        public bool IsInitialized { get; private set; }
        #endregion

        #region Private Fields
        private AnimationController_Multiplayer m_animationController;

        private TileMaskManager_Multiplayer m_tileMaskManager;
        private GameObject m_go_gameBoardSet;
        private Transform m_transform_baseTile;
        private Vector2 m_tileSize;

        private List<EventLog> m_eventLogs;

        private Dictionary<int, _2DCoord> m_initialUnitPositions;
        private Dictionary<int, int> m_playerUnitIdAndIndexMapping;

        private Dictionary<_2DCoord, bool> m_targetableAndSelectableArea;
        private int m_maxNumOfTargets;

        private bool m_hasOpponentInfoBeenLoaded;

        private bool m_isGettingMatchEndStatus;

        private bool m_isGettingMissingEventLogs;

        private bool m_arePlayersSet;
        private bool m_isPlayerDataSet;
        private bool m_isOpponentDataSet;
        private bool m_isTileMapSet;
        private bool m_areUnitsSet;
        private bool m_isInitializing;

        private bool m_processingSceneExit;

        private bool m_isMatchEnd;
        private bool m_isPlayerWinner;
        #endregion

        // Use this for Initialization of simple properties
        void Awake()
        {
            m_hasOpponentInfoBeenLoaded = true;

            m_isGettingMatchEndStatus = false;

            m_isGettingMissingEventLogs = false;

            IsInitialized = false;
            m_arePlayersSet = false;
            m_isPlayerDataSet = false;
            m_isOpponentDataSet = false;
            m_isTileMapSet = false;
            m_areUnitsSet = false;
            m_isInitializing = false;

            m_isMatchEnd = false;
            m_isPlayerWinner = false;

            m_processingSceneExit = false;

            m_animationController = this.GetComponent<AnimationController_Multiplayer>();
            m_go_gameBoardSet = GameObject.FindGameObjectWithTag("GameBoard");
            m_tileMaskManager = m_go_gameBoardSet.GetComponent<TileMaskManager_Multiplayer>();
            m_transform_baseTile = m_go_gameBoardSet.transform.Find("Tile0");
            MeshRenderer meshRenderer = m_transform_baseTile.GetComponent<MeshRenderer>();
            m_tileSize = new Vector2(meshRenderer.bounds.size.x, meshRenderer.bounds.size.z);

            m_eventLogs = new List<EventLog>();

            m_initialUnitPositions = new Dictionary<int, _2DCoord>();
            m_playerUnitIdAndIndexMapping = new Dictionary<int, int>();
            m_targetableAndSelectableArea = new Dictionary<_2DCoord, bool>();

            GOs_Unit = new List<GameObject>();
            GOs_PlayerUnit = new List<GameObject>();
            GOs_OpponentUnit = new List<GameObject>();

            OpponentUnitInfos = new List<OpponentUnitInfo>();
        }

        // Use this for detailed initialization
        // Update() is being used to make sure that all Player Instances are created before the detailed initialization
        void Update()
        {
            if (!IsInitialized && !m_isInitializing)
                Initialize();

            if (IsInitialized)
            {
                if (m_isMatchEnd && !m_processingSceneExit)
                    SceneExit();
            }
        }

        // Called once per frame
        private void FixedUpdate()
        {
            if (!m_isMatchEnd && !m_isGettingMissingEventLogs)
            {
                m_isGettingMissingEventLogs = true;
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                this.StartCoroutineRepetitionUntilTrue(GetMissingEventLogs(looperAndCoroutineLinker), looperAndCoroutineLinker);
            }

            if (!m_isMatchEnd && !m_isGettingMatchEndStatus)
            {
                m_isGettingMatchEndStatus = true;
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                this.StartCoroutineRepetitionUntilTrue(GetMatchEndStatus(looperAndCoroutineLinker), looperAndCoroutineLinker);
            }
        }

        private void Initialize()
        {
            try
            {
                Debug.Log("BattleSystem_Multiplayer: Start Initialization.");

                if (!m_arePlayersSet)
                {
                    if (GO_Player == null)
                        GO_Player = GameObject.Find("Player");
                    if (GO_Player == null)
                        return;

                    if (GO_Opponent == null)
                        GO_Opponent = GameObject.Find("Opponent");
                    if (GO_Opponent == null)
                        return;

                    PlayerController = GO_Player.GetComponent<PlayerController_Multiplayer>();
                    if (PlayerController == null || !PlayerController.IsInitialized)
                        return;

                    m_arePlayersSet = true;
                }

                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                this.StartCoroutineRepetitionUntilTrue(GetMatchInfoAndInitialize(looperAndCoroutineLinker), looperAndCoroutineLinker);
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_Multiplayer: at Initialize()" + ex.Message);
            }
        }

        IEnumerator GetMatchInfoAndInitialize(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            m_isInitializing = true;

            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetMatchInfo"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            Debug.Log("WebRequest Sent");

            string errorMsg = string.Empty;

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                string sectionString = string.Empty;

                #region Load Player Data
                Dictionary<int, int> units_maxHP = new Dictionary<int, int>();
                if (!m_isPlayerDataSet)
                {
                    // Get instance of PlayerOnBoard
                    sectionString = response.GetTagPortionWithoutOpeningAndClosingTags("Player");
                    PlayerData = ResponseStringToPlayerOnBoard(sectionString);
                    PlayerController.IsMyTurn = PlayerData.IsPlayer1; // Sync/Initialize the turn status to the PlayerController (If IsPlayer1, then, the player starts the match.)

                    // Clear the dictionary to link each player unit's Id and Index
                    m_playerUnitIdAndIndexMapping.Clear();

                    // Get the data of player's units and map the unique Ids to the unit indexes
                    sectionString = response.GetTagPortionWithoutOpeningAndClosingTags("Units");
                    while (sectionString != "")
                    {
                        string playerUnitString = string.Empty;
                        sectionString = sectionString.DetachTagPortion("Unit", ref playerUnitString);

                        int id = playerUnitString.GetTagPortionValue<int>("UniqueId");

                        int index = playerUnitString.GetTagPortionValue<int>("Index");

                        int maxHP = playerUnitString.GetTagPortionValue<int>("MaxHP");

                        m_playerUnitIdAndIndexMapping.Add(id, index);
                        units_maxHP.Add(id, maxHP);
                    }

                    if (!PlayerController.IsInitialized)
                        yield break;

                    m_isPlayerDataSet = true;
                }
                #endregion

                #region Load Opponent Data
                if (!m_isOpponentDataSet)
                {
                    sectionString = response.GetTagPortionWithoutOpeningAndClosingTags("Opponent");

                    // Get the name of opponent and set it to Opponent_Name variable
                    Opponent_Name = sectionString.GetTagPortionValue<string>("Name");

                    // Clear OpponentUnitInfos list
                    OpponentUnitInfos.Clear();

                    // Get the data of opponent's units and store them into OpponentUnitInfos list
                    sectionString = sectionString.GetTagPortionWithoutOpeningAndClosingTags("Units");
                    while (sectionString != "")
                    {
                        string opponentUnitString = string.Empty;
                        sectionString = sectionString.DetachTagPortion("Unit", ref opponentUnitString);

                        OpponentUnitInfos.Add(ResponseStringToOpponentPlayerInfo(opponentUnitString));
                    }

                    m_isOpponentDataSet = true;
                }
                #endregion

                #region Load Tile Map
                if (!m_isTileMapSet)
                {
                    // Initialize TileMap array
                    TileMap = new eTileType[CoreValues.SIZE_OF_A_SIDE_OF_BOARD, CoreValues.SIZE_OF_A_SIDE_OF_BOARD];

                    // Clear m_initialUnitPositions dictionary
                    m_initialUnitPositions.Clear();

                    // Get the tile map data and store the data into TileMap array
                    // Additionally, get and set the initail position of each unit
                    sectionString = response.GetTagPortionWithoutOpeningAndClosingTags("Sockets");
                    while (sectionString != "")
                    {
                        string socketString = string.Empty;
                        sectionString = sectionString.DetachTagPortion("Socket", ref socketString);

                        LoadTileTypeAndUnitForSocket(socketString);
                    }

                    m_isTileMapSet = true;
                }
                #endregion

                if (!m_areUnitsSet)
                {
                    // Clear the list of unit chips
                    GOs_Unit.Clear();

                    // Remove existing unit chips from the Unity world
                    GO_Player.transform.ClearChildren();
                    GO_Opponent.transform.ClearChildren();

                    // Instantiate unit chips for player and opponent into Unity world
                    if (PlayerController.PlayerId == 1) // If player is Player 1
                    {
                        SpawnPlayerUnits(units_maxHP); // Spawn player units first
                        SpawnOpponentUnits(); // ...Then, spawn opponent units
                    }
                    else // If opponent is Player 1
                    {
                        SpawnOpponentUnits(); // Spawn opponent units first
                        SpawnPlayerUnits(units_maxHP); // ...Then, spawn player units
                    }

                    m_areUnitsSet = true;
                }

                if (m_arePlayersSet
                    && m_isPlayerDataSet
                    && m_isOpponentDataSet
                    && m_isTileMapSet)
                {
                    IsInitialized = true;
                    m_isInitializing = false;
                }

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }

            if (errorMsg != string.Empty)
                PopUpWindowManager.Instance.CreateSimplePopUp("Error!", errorMsg, "OK");
        }

        private PlayerOnBoard ResponseStringToPlayerOnBoard(string _response)
        {
            bool isPlayer1 = _response.GetTagPortionValue<bool>("IsPlayer1");

            int maxSP = _response.GetTagPortionValue<int>("MaxSP");

            PlayerOnBoard pob = new PlayerOnBoard(GameDataContainer.Instance.Player, 0, isPlayer1)
            {
                MaxSP = maxSP,
                RemainingSP = maxSP
            };

            return pob;
        }

        private OpponentUnitInfo ResponseStringToOpponentPlayerInfo(string _response)
        {
            int index = _response.GetTagPortionValue<int>("Index");

            int id = _response.GetTagPortionValue<int>("Id");

            int maxHP = _response.GetTagPortionValue<int>("MaxHP");

            int remainingHP = _response.GetTagPortionValue<int>("RemainingHP");

            return new OpponentUnitInfo { UnitIndex = index, Id = id, MaxHP = maxHP, RemainingHP = remainingHP };
        }

        private void LoadTileTypeAndUnitForSocket(string _response)
        {
            int x = _response.GetTagPortionValue<int>("XCoord");

            int y = _response.GetTagPortionValue<int>("YCoord");

            eTileType tileType = _response.GetTagPortionValue<eTileType>("TileType");

            // Set tile type for coordinate
            TileMap[x, y] = tileType;

            // Store the position of corresponding unit into m_initialUnitPositions list if the socket contains a unit
            if (_response.Contains("UnitIndex"))
            {
                int unitIndex = _response.GetTagPortionValue<int>("UnitIndex");

                m_initialUnitPositions.Add(unitIndex, new _2DCoord(x, y));
            }
        }

        public IEnumerator UpdatePlayerInfo(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetPlayerInfo"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                bool isMyTurn = response.GetTagPortionValue<bool>("IsMyTurn");

                bool hasMoved = response.GetTagPortionValue<bool>("HasMoved");

                bool hasAttacked = response.GetTagPortionValue<bool>("HasAttacked");

                bool hasUsedUltimateSkill = response.GetTagPortionValue<bool>("HasUsedUltimateSkill");

                int maxSP = response.GetTagPortionValue<int>("MaxSP");

                int remainingSP = response.GetTagPortionValue<int>("RemainingSP");

                int selectedAlliedUnitId = response.GetTagPortionValue<int>("SelectedAlliedUnitId");

                PlayerController.IsMyTurn = isMyTurn;
                PlayerData.Moved = hasMoved;
                PlayerData.Attacked = hasAttacked;
                if (hasUsedUltimateSkill) { PlayerData.SetUsedUltimateSkillToTrue(); }
                PlayerData.MaxSP = maxSP;
                PlayerData.RemainingSP = remainingSP;
                PlayerData.SelectedUnitIndex = selectedAlliedUnitId;

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public void Request_ChangeTurn()
        {
            if (!m_animationController.IsInitialized || m_animationController.LockUI)
                return;

            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            this.StartCoroutineRepetitionUntilTrue(ChangeTurn(looperAndCoroutineLinker), looperAndCoroutineLinker);
        }
        IEnumerator ChangeTurn(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "ChangeTurn"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            string errorMsg = string.Empty;

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                yield return this.StartCoroutineRepetitionUntilTrue(UpdatePlayerInfo(looperAndCoroutineLinker), looperAndCoroutineLinker);

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }

            if (errorMsg != string.Empty)
                PopUpWindowManager.Instance.CreateSimplePopUp("Error!", errorMsg, "OK");
        }

        public void Request_Concede()
        {
            if (!m_animationController.IsInitialized || m_animationController.LockUI)
                return;

            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            this.StartCoroutineRepetitionUntilTrue(Concede(looperAndCoroutineLinker), looperAndCoroutineLinker);
        }
        IEnumerator Concede(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "Concede"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
        }

        private void SceneExit()
        {
            m_processingSceneExit = true;

            StartCoroutine(ProcessSceneExit());
        }
        IEnumerator ProcessSceneExit()
        {
            yield return StartCoroutine(m_animationController.FadeMatchResultPanelIn(m_isPlayerWinner));

            m_processingSceneExit = false;
        }
        IEnumerator GetMatchEndStatus(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetMatchEndStatus"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                m_isMatchEnd = response.GetTagPortionValue<bool>("IsMatchEnd");

                if (m_isMatchEnd)
                    m_isPlayerWinner = response.GetTagPortionValue<bool>("IsPlayerWinner");

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        private void SpawnPlayerUnits(Dictionary<int, int> _units_maxHP)
        {
            foreach (UnitInstance unit in PlayerData.AlliedUnits)
            {
                int maxHP = _units_maxHP.First(x => x.Key == unit.UniqueId).Value;
                SpawnPlayerUnit(unit, maxHP);
            }
        }
        private bool SpawnPlayerUnit(UnitInstance _unit, int _maxHP)
        {
            try
            {
                Debug.Log("BattleSystem_Multiplayer: Start Player Unit Spawning.");

                GameObject unit = Instantiate(PlayerUnitPrefab, Vector3.zero, Quaternion.identity, this.transform.root);

                int unitIndex = m_playerUnitIdAndIndexMapping[_unit.UniqueId];
                unit.name = unitIndex.ToString(); //Set the unit index as the name of the game object for easy reference
                _2DCoord coord = m_initialUnitPositions[unitIndex]; // Get location as logical 2D Coordinate (x, y) --- this is not Unity coordinate
                unit.transform.position = GetActualPosition(coord); // Get and Set the actual position in the Unity world
                if (PlayerController.PlayerId != 1)
                    unit.transform.Rotate(0f, 180f, 0f); //Rotate 180 degrees to adjust player 2's units' direction facing.

                //Return false if not succeeded
                if (!unit.GetComponent<PlayerUnitController>()
                    .SetInitializationData(unitIndex,
                                            PlayerData.AlliedUnits.IndexOf(_unit),
                                            PlayerData.IsPlayer1 ? 1 : 2,
                                            _unit, _maxHP))
                {
                    Destroy(unit); // destroy not to leave the unsynced instance in unity world
                    return false;
                }

                GOs_PlayerUnit.Add(unit);

                GOs_Unit.Add(unit);

                Debug.Log("BattleSystem_Multiplayer: End Player Unit Spawning.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_Multiplayer: at SpawnUnit() " + ex.Message);
                return false;
            }
        }

        private void SpawnOpponentUnits()
        {
            foreach (OpponentUnitInfo unit in OpponentUnitInfos)
            {
                SpawnOpponentUnit(unit);
            }
        }
        private bool SpawnOpponentUnit(OpponentUnitInfo _unit)
        {
            try
            {
                Debug.Log("BattleSystem_Multiplayer: Start Opponent Unit Spawning.");

                GameObject unit = Instantiate(OpponentUnitPrefab, Vector3.zero, Quaternion.identity, this.transform.root);

                unit.name = _unit.UnitIndex.ToString(); //Set the unit index as the name of the game object for easy reference
                _2DCoord coord = m_initialUnitPositions[_unit.UnitIndex]; // Get location as logical 2D Coordinate (x, y) --- this is not Unity coordinate
                unit.transform.position = GetActualPosition(coord); // Get and Set the actual position in the Unity world
                if (PlayerController.PlayerId == 1) // If Player's id is 1, then, opponent's id is 2
                    unit.transform.Rotate(0f, 180f, 0f); //Rotate 180 degrees to adjust player 2's units' direction facing.

                //Return false if not succeeded
                if (!unit.GetComponent<OpponentUnitController>()
                    .SetInitializationData(_unit.UnitIndex,
                                            PlayerData.IsPlayer1 ? 2 : 1,
                                            GameDataContainer.Instance.UnitEncyclopedia.Where(x => x.Id == _unit.Id).First(),
                                            _unit.MaxHP,
                                            _unit.RemainingHP))
                {
                    Destroy(unit); // destroy not to leave the unsynced instance in unity world
                    return false;
                }

                GOs_OpponentUnit.Add(unit);

                GOs_Unit.Add(unit);

                Debug.Log("BattleSystem_Multiplayer: End Opponent Unit Spawning.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_Multiplayer: at SpawnUnit() " + ex.Message);
                return false;
            }
        }

        public Vector3 GetActualPosition(_2DCoord _coord)
        {
            try
            {
                int tileNum = _coord.Y * CoreValues.SIZE_OF_A_SIDE_OF_BOARD + _coord.X;
                Transform tileMask = GameObject.Find("Tile" + tileNum.ToString()).transform.Find("Mask" + tileNum.ToString());

                float adjustmentValueY = 0.01f;

                float x = tileMask.position.x;
                float y = tileMask.position.y + adjustmentValueY;
                float z = tileMask.position.z;

                return new Vector3(x, y, z);
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_Multiplayer: at GetActualPosition() " + ex.Message);
                return new Vector3(-1, -1, -1);
            }
        }

        public IEnumerator MoveUnit(LooperAndCoroutineLinker _looperAndCoroutineLinker, _2DCoord _destination)
        {
            string destinationString = "(" + _destination.X.ToString() + "," + _destination.Y.ToString() + ")";

            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "MoveUnit"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
            {"destination", destinationString}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                yield return this.StartCoroutineRepetitionUntilTrue(UpdatePlayerInfo(looperAndCoroutineLinker), looperAndCoroutineLinker);

                yield return StartCoroutine(UpdateMovableArea());

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public IEnumerator Attack(LooperAndCoroutineLinker _looperAndCoroutineLinker, List<_2DCoord> _targetCoords)
        {
            string targetCoordsString = string.Empty;
            foreach (_2DCoord coord in _targetCoords)
            {
                targetCoordsString += "(" + coord.X.ToString() + "," + coord.Y.ToString() + ");";
            }

            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "Attack"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
            {"targetCoords", targetCoordsString}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                yield return this.StartCoroutineRepetitionUntilTrue(UpdatePlayerInfo(looperAndCoroutineLinker), looperAndCoroutineLinker);

                yield return StartCoroutine(UpdateAttackTargetableArea());

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public IEnumerator UseSkill(LooperAndCoroutineLinker _looperAndCoroutineLinker, string _skillName, List<_2DCoord> _targetCoords, List<_2DCoord> _secondaryTargetCoords)
        {
            string targetCoordsString = string.Empty;
            foreach (_2DCoord coord in _targetCoords)
            {
                targetCoordsString += "(" + coord.X.ToString() + "," + coord.Y.ToString() + ");";
            }

            string secondaryTargetCoordsString = string.Empty;
            foreach (_2DCoord coord in _secondaryTargetCoords)
            {
                secondaryTargetCoordsString += "(" + coord.X.ToString() + "," + coord.Y.ToString() + ");";
            }

            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "UseSkill"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
            {"skillName", _skillName},
            {"targetCoords", targetCoordsString},
            {"secondaryTargetCoords", secondaryTargetCoordsString}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                yield return this.StartCoroutineRepetitionUntilTrue(UpdatePlayerInfo(looperAndCoroutineLinker), looperAndCoroutineLinker);

                yield return StartCoroutine(UpdateSkillTargetableArea(_skillName));

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        //public void UseUltimateSkill(string _skillName, List<_2DCoord> _targetCoords)
        //{
        //
        //
        //
        //}



        public void Request_ChangeUnit(int _unitIndex)
        {
            try
            {
                if (m_animationController.LockUI)
                    return;

                if (_unitIndex != PlayerData.SelectedUnitIndex)
                    StartCoroutine(ChangeUnitAndSyncProperties(_unitIndex));
            }
            catch (Exception ex)
            {
                Debug.Log("BattleSystem_Multiplayer: at ChangeUnit() " + ex.Message);
            }
        }
        IEnumerator ChangeUnitAndSyncProperties(int _unitIndex)
        {
            //Immediately apply visual changes (If the modification is wrong, it will be adjusted based on the information returned from the server).
            PlayerData.SelectedUnitIndex = _unitIndex;

            LooperAndCoroutineLinker looperAndCoroutineLinker1 = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(ChangeUnit(looperAndCoroutineLinker1, _unitIndex), looperAndCoroutineLinker1);
            Debug.Log("Selected Unit has been changed");

            LooperAndCoroutineLinker looperAndCoroutineLinker2 = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(UpdatePlayerInfo(looperAndCoroutineLinker2), looperAndCoroutineLinker2);
            Debug.Log("Unit Selection Info was Synced!");
        }
        IEnumerator ChangeUnit(LooperAndCoroutineLinker _looperAndCoroutineLinker, int _unitIndex)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "ChangeUnit"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
            {"unitIndex", _unitIndex.ToString() }
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
        }

        public IEnumerator UpdateMovableArea()
        {
            float startingTime = Time.realtimeSinceStartup;
            Debug.Log("UpdateMovableArea() Started!");

            m_maxNumOfTargets = 1;
            m_tileMaskManager.MaxNumOfTargets = m_maxNumOfTargets;

            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(GetMovableAndSelectableArea(looperAndCoroutineLinker), looperAndCoroutineLinker);
            m_tileMaskManager.UpdateTargetArea(m_targetableAndSelectableArea);

            Debug.Log("UpdateMovableArea() Ended! " + (Time.realtimeSinceStartup - startingTime).ToString() + "seconds");
        }
        IEnumerator GetMovableAndSelectableArea(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetMovableAndSelectableArea"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                m_targetableAndSelectableArea = ResponseStringToTargetableAndSelectableArea(response);

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public IEnumerator UpdateAttackTargetableArea()
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker1 = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(GetMaxNumOfTargets(looperAndCoroutineLinker1, GameDataContainer.Instance.BasicAttackSkill.BaseInfo.Name), looperAndCoroutineLinker1);
            m_tileMaskManager.MaxNumOfTargets = m_maxNumOfTargets;

            LooperAndCoroutineLinker looperAndCoroutineLinker2 = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(GetAttackTargetableAndSelectableArea(looperAndCoroutineLinker2), looperAndCoroutineLinker2);
            m_tileMaskManager.UpdateTargetArea(m_targetableAndSelectableArea);
        }
        IEnumerator GetAttackTargetableAndSelectableArea(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetAttackTargetableAndSelectableArea"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                m_targetableAndSelectableArea = ResponseStringToTargetableAndSelectableArea(response);

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public IEnumerator UpdateSkillTargetableArea(string _skillName)
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker1 = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(GetMaxNumOfTargets(looperAndCoroutineLinker1, _skillName), looperAndCoroutineLinker1);
            m_tileMaskManager.MaxNumOfTargets = m_maxNumOfTargets;

            LooperAndCoroutineLinker looperAndCoroutineLinker2 = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(GetSkillTargetableAndSelectableArea(looperAndCoroutineLinker2, _skillName), looperAndCoroutineLinker2);
            m_tileMaskManager.UpdateTargetArea(m_targetableAndSelectableArea);
        }
        IEnumerator GetSkillTargetableAndSelectableArea(LooperAndCoroutineLinker _looperAndCoroutineLinker, string _skillName)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetSkillTargetableAndSelectableArea"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
            {"skillName",  _skillName}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                m_targetableAndSelectableArea = ResponseStringToTargetableAndSelectableArea(response);

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public void UpdateTargetableAreaToNull() { m_tileMaskManager.UpdateTargetArea(null); }

        private Dictionary<_2DCoord, bool> ResponseStringToTargetableAndSelectableArea(string _response)
        {
            //float startingTime = Time.realtimeSinceStartup;
            //Debug.Log("ResponseStringToTargetableAndSelectableArea() Started!");

            Dictionary<_2DCoord, bool> result = new Dictionary<_2DCoord, bool>();

            try
            {
                string sectionString = string.Empty;

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Coords");
                while (sectionString != "")
                {
                    string coordString = "";
                    sectionString = sectionString.DetachTagPortion("Coord", ref coordString);

                    int x = coordString.GetTagPortionValue<int>("X");
                    int y = coordString.GetTagPortionValue<int>("Y");

                    bool isSelectable = coordString.GetTagPortionValue<bool>("IsSelectable");

                    result.Add(new _2DCoord(x, y), isSelectable);
                }

                //Debug.Log("ResponseStringToTargetableAndSelectableArea() Ended! " + (Time.realtimeSinceStartup - startingTime).ToString() + "seconds");

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        IEnumerator GetMaxNumOfTargets(LooperAndCoroutineLinker _looperAndCoroutineLinker, string _skillName)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetMaxNumOfTargets"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
            {"skillName", _skillName}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                m_maxNumOfTargets = Convert.ToInt32(response);

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        IEnumerator GetMissingEventLogs(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
        {
            {"subject", "GetMissingEventLogs"},
            {"playerId", GameDataContainer.Instance.Player.Id.ToString()},
            {"latestLogIndex", (m_eventLogs.Count - 1).ToString()}
        };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            string errorMsg = string.Empty;

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                m_eventLogs.AddRange(ResponseStringToEventLogs(response));

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }

            m_isGettingMissingEventLogs = false;
        }

        public List<EventLog> ResponseStringToEventLogs(string _response)
        {
            try
            {
                List<EventLog> result = new List<EventLog>();
                string response = string.Copy(_response);

                while (response != "")
                {
                    string eventLogString = string.Empty;
                    //Automatic Event Log
                    if (response.StartsWith("<TurnChangeEventLog>"))
                        response = response.DetachTagPortion("TurnChangeEventLog", ref eventLogString);
                    else if (response.StartsWith("<EffectTrialLog_DamageEffect>"))
                        response = response.DetachTagPortion("EffectTrialLog_DamageEffect", ref eventLogString);
                    else if (response.StartsWith("<EffectTrialLog_HealEffect>"))
                        response = response.DetachTagPortion("EffectTrialLog_HealEffect", ref eventLogString);
                    else if (response.StartsWith("<EffectTrialLog_StatusEffectAttachmentEffect>"))
                        response = response.DetachTagPortion("EffectTrialLog_StatusEffectAttachmentEffect", ref eventLogString);
                    else if (response.StartsWith("<EffectTrialLog_MovementEffect>"))
                        response = response.DetachTagPortion("EffectTrialLog_MovementEffect", ref eventLogString);
                    else if (response.StartsWith("<StatusEffectLog_HPModification>"))
                        response = response.DetachTagPortion("StatusEffectLog_HPModification", ref eventLogString);
                    //Action Log
                    else if (response.StartsWith("<ActionLog_Move>"))
                        response = response.DetachTagPortion("ActionLog_Move", ref eventLogString);
                    else if (response.StartsWith("<ActionLog_Attack>"))
                        response = response.DetachTagPortion("ActionLog_Attack", ref eventLogString);
                    else if (response.StartsWith("<ActionLog_UnitTargetingSkill>"))
                        response = response.DetachTagPortion("ActionLog_UnitTargetingSkill", ref eventLogString);
                    else if (response.StartsWith("<ActionLog_TileTargetingSkill>"))
                        response = response.DetachTagPortion("ActionLog_TileTargetingSkill", ref eventLogString);

                    result.Add(StringToEventLog(eventLogString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private EventLog StringToEventLog(string _string)
        {
            try
            {
                string sectionString = string.Empty;

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("EventTurn");
                decimal eventTurn = Convert.ToDecimal(sectionString);

                if (_string.StartsWith("<TurnChangeEventLog>")
                    || _string.StartsWith("<EffectTrialLog_DamageEffect>")
                    || _string.StartsWith("<EffectTrialLog_HealEffect>")
                    || _string.StartsWith("<EffectTrialLog_StatusEffectAttachmentEffect>")
                    || _string.StartsWith("<EffectTrialLog_MovementEffect>")
                    || _string.StartsWith("<StatusEffectLog_HPModification>"))
                {
                    return StringToAutomaticEventLog(_string, eventTurn);
                }
                else if (_string.StartsWith("<ActionLog_Move>")
                    || _string.StartsWith("<ActionLog_Attack>")
                    || _string.StartsWith("<ActionLog_UnitTargetingSkill>")
                    || _string.StartsWith("<ActionLog_TileTargetingSkill>"))
                {
                    return StringToActionLog(_string, eventTurn);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private AutomaticEventLog StringToAutomaticEventLog(string _string, decimal _eventTurn)
        {
            try
            {
                if (_string.StartsWith("<TurnChangeEventLog>"))
                    return StringToTurnChangeEventLog(_string, _eventTurn);
                else if (_string.StartsWith("<EffectTrialLog_DamageEffect>")
                    || _string.StartsWith("<EffectTrialLog_HealEffect>")
                    || _string.StartsWith("<EffectTrialLog_StatusEffectAttachmentEffect>")
                    || _string.StartsWith("<EffectTrialLog_MovementEffect>"))
                {
                    return StringToEffectTrialLog(_string, _eventTurn);
                }
                else if (_string.StartsWith("<StatusEffectLog_HPModification>"))
                {
                    return StringToStatusEffectLog(_string, _eventTurn);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private AutomaticEventLog StringToTurnChangeEventLog(string _string, decimal _eventTurn)
        {
            try
            {
                int turnEndingPlayerId = _string.GetTagPortionValue<int>("TurnEndingPlayerId");
                int turnInitiatingPlayerId = _string.GetTagPortionValue<int>("TurnInitiatingPlayerId");

                int remainingSPForTurnEndingPlayer = _string.GetTagPortionValue<int>("RemainingSPForTurnEndingPlayer");
                int remainingSPForTurnInitiatingPlayer = _string.GetTagPortionValue<int>("RemainingSPForTurnInitiatingPlayer");

                return new TurnChangeEventLog(_eventTurn, turnEndingPlayerId, turnInitiatingPlayerId, remainingSPForTurnEndingPlayer, remainingSPForTurnInitiatingPlayer);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private AutomaticEventLog StringToEffectTrialLog(string _string, decimal _eventTurn)
        {
            try
            {
                int animationInfoId = _string.GetTagPortionValue<int>("AnimationInfoId");
                AnimationInfo animationInfo = GameDataContainer.Instance.AnimationInfos[animationInfoId];
                bool isDiffused = _string.GetTagPortionValue<bool>("IsDiffused");

                bool didActivate = _string.GetTagPortionValue<bool>("DidActivate");
                bool didSucceed = _string.GetTagPortionValue<bool>("DidSucceed");

                if (_string.StartsWith("<EffectTrialLog_DamageEffect>")
                    || _string.StartsWith("<EffectTrialLog_HealEffect>")
                    || _string.StartsWith("<EffectTrialLog_StatusEffectAttachmentEffect>"))
                {
                    return StringToUnitTargetingEffectTrialLog(_string, _eventTurn, animationInfo, isDiffused, didActivate, didSucceed);
                }
                else if (_string.StartsWith("<EffectTrialLog_MovementEffect>"))
                {
                    return StringToTileTargetingEffectTrialLog(_string, _eventTurn, animationInfo, isDiffused, didActivate, didSucceed);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private EffectTrialLog_UnitTargetingEffect StringToUnitTargetingEffectTrialLog(string _string, decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed)
        {
            try
            {
                int targetId = _string.GetTagPortionValue<int>("TargetId");

                string targetName = _string.GetTagPortionValue<string>("TargetName");

                string targetNickname = _string.GetTagPortionValue<string>("TargetNickname");

                int targetLocationTileIndex = _string.GetTagPortionValue<int>("TargetLocationTileIndex");

                if (_string.StartsWith("<EffectTrialLog_DamageEffect>"))
                    return StringToDamageEffectTrialLog(_string, _eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, targetId, targetName, targetNickname, targetLocationTileIndex);
                else if (_string.StartsWith("<EffectTrialLog_HealEffect>"))
                    return StringToHealEffectTrialLog(_string, _eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, targetId, targetName, targetNickname, targetLocationTileIndex);
                else if (_string.StartsWith("<EffectTrialLog_StatusEffectAttachmentEffect>"))
                    return StringToStatusEffectAttachmentEffectTrialLog(_string, _eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, targetId, targetName, targetNickname, targetLocationTileIndex);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private EffectTrialLog_DamageEffect StringToDamageEffectTrialLog(string _string, decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed, int _targetId, string _targetName, string _targetNickname, int _targetLocationTileIndex)
        {
            try
            {
                bool wasImmune = _string.GetTagPortionValue<bool>("WasImmune");

                bool wasCritical = _string.GetTagPortionValue<bool>("WasCritical");

                eEffectiveness effectiveness = _string.GetTagPortionValue<eEffectiveness>("Effectiveness");

                int value = _string.GetTagPortionValue<int>("Value");

                int remainingHPAfterModification = _string.GetTagPortionValue<int>("RemainingHPAfterModification");

                return new EffectTrialLog_DamageEffect(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, _targetId, _targetName, _targetNickname, _targetLocationTileIndex, wasImmune, wasCritical, effectiveness, value, remainingHPAfterModification);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private EffectTrialLog_HealEffect StringToHealEffectTrialLog(string _string, decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed, int _targetId, string _targetName, string _targetNickname, int _targetLocationTileIndex)
        {
            try
            {
                bool wasCritical = _string.GetTagPortionValue<bool>("WasCritical");

                int value = _string.GetTagPortionValue<int>("Value");

                int remainingHPAfterModification = _string.GetTagPortionValue<int>("RemainingHPAfterModification");

                return new EffectTrialLog_HealEffect(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, _targetId, _targetName, _targetNickname, _targetLocationTileIndex, wasCritical, value, remainingHPAfterModification);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private EffectTrialLog_StatusEffectAttachmentEffect StringToStatusEffectAttachmentEffectTrialLog(string _string, decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed, int _targetId, string _targetName, string _targetNickname, int _targetLocationTileIndex)
        {
            try
            {
                int attachedStatusEffectId = _string.GetTagPortionValue<int>("AttachedStatusEffectId");

                return new EffectTrialLog_StatusEffectAttachmentEffect(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, _targetId, _targetName, _targetNickname, _targetLocationTileIndex, attachedStatusEffectId);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private EffectTrialLog_TileTargetingEffect StringToTileTargetingEffectTrialLog(string _string, decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed)
        {
            try
            {
                string sectionString = _string.GetTagPortion("TargetCoord");
                int x = sectionString.GetTagPortionValue<int>("X");
                int y = sectionString.GetTagPortionValue<int>("Y");
                _2DCoord targetCoord = new _2DCoord(x, y);

                if (_string.StartsWith("<EffectTrialLog_MovementEffect>"))
                    return StringToMovementEffectTrialLog(_string, _eventTurn, _animationInfo as MovementAnimationInfo, _didActivate, targetCoord);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private EffectTrialLog_MovementEffect StringToMovementEffectTrialLog(string _string, decimal _eventTurn, MovementAnimationInfo _animationInfo, bool _didActivate, _2DCoord _targetCoord)
        {
            try
            {
                string sectionString = _string.GetTagPortion("InitialCoord");
                int x = sectionString.GetTagPortionValue<int>("X");
                int y = sectionString.GetTagPortionValue<int>("Y");
                _2DCoord initialCoord = new _2DCoord(x, y);

                return new EffectTrialLog_MovementEffect(_eventTurn, _animationInfo, _didActivate, _targetCoord, initialCoord);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private StatusEffectLog StringToStatusEffectLog(string _string, decimal _eventTurn)
        {
            try
            {
                int effectHolderId = _string.GetTagPortionValue<int>("EffectHolderId");

                string effectHolderName = _string.GetTagPortionValue<string>("EffectHolderName");

                string effectHolderNickname = _string.GetTagPortionValue<string>("EffectHolderNickname");

                if (_string.StartsWith("<StatusEffectLog_HPModification>"))
                    return StringToHPModificationStatusEffectLog(_string, _eventTurn, effectHolderId, effectHolderName, effectHolderNickname);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private StatusEffectLog_HPModification StringToHPModificationStatusEffectLog(string _string, decimal _eventTurn, int _effectHolderId, string _effectHolderName, string _effectHolderNickname)
        {
            try
            {
                bool isPositive = _string.GetTagPortionValue<bool>("IsPositive");

                int value = _string.GetTagPortionValue<int>("Value");

                int remainingHPAfterModification = _string.GetTagPortionValue<int>("RemainingHPAfterModification");

                return new StatusEffectLog_HPModification(_eventTurn, _effectHolderId, _effectHolderName, _effectHolderNickname, isPositive, value, remainingHPAfterModification);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ActionLog StringToActionLog(string _string, decimal _actionTurn)
        {
            try
            {
                int actorId = _string.GetTagPortionValue<int>("ActorId");

                string actorName = _string.GetTagPortionValue<string>("ActorName");

                string actorNickname = _string.GetTagPortionValue<string>("ActorNickname");

                if (_string.StartsWith("<ActionLog_Move>"))
                    return StringToMoveActionLog(_string, _actionTurn, actorId, actorName, actorNickname);
                else if (_string.StartsWith("<ActionLog_Attack>"))
                    return StringToAttackActionLog(_string, _actionTurn, actorId, actorName, actorNickname);
                else if (_string.StartsWith("<ActionLog_UnitTargetingSkill>")
                    || _string.StartsWith("<ActionLog_TileTargetingSkill>"))
                    return StringToSkillActionLog(_string, _actionTurn, actorId, actorName, actorNickname);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ActionLog_Move StringToMoveActionLog(string _string, decimal _actionTurn, int _actorId, string _actorName, string _actorNickname)
        {
            try
            {
                string sectionString = string.Empty;

                string initialCoordString = _string.GetTagPortion("InitialCoord");
                int initialCoordX = initialCoordString.GetTagPortionValue<int>("X");
                int initialCoordY = initialCoordString.GetTagPortionValue<int>("Y");
                _2DCoord initialCoord = new _2DCoord(initialCoordX, initialCoordY);

                string eventualCoordString = _string.GetTagPortion("EventualCoord");
                int eventualCoordX = eventualCoordString.GetTagPortionValue<int>("X");
                int eventualCoordY = eventualCoordString.GetTagPortionValue<int>("Y");
                _2DCoord eventualCoord = new _2DCoord(eventualCoordX, eventualCoordY);

                return new ActionLog_Move(_actionTurn, _actorId, _actorName, _actorNickname, initialCoord, eventualCoord);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ActionLog_Attack StringToAttackActionLog(string _string, decimal _actionTurn, int _actorId, string _actorName, string _actorNickname)
        {
            try
            {
                int actorLocationTileIndex = _string.GetTagPortionValue<int>("ActorLocationTileIndex");

                int targetId = _string.GetTagPortionValue<int>("TargetId");

                int targetLocationTileIndex = _string.GetTagPortionValue<int>("TargetLocationTileIndex");

                return new ActionLog_Attack(_actionTurn, _actorId, _actorName, _actorNickname, actorLocationTileIndex, targetId, targetLocationTileIndex);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ActionLog_Skill StringToSkillActionLog(string _string, decimal _actionTurn, int _actorId, string _actorName, string _actorNickname)
        {
            try
            {
                string skillName = _string.GetTagPortionValue<string>("SkillName");

                int actorLocationTileIndex = _string.GetTagPortionValue<int>("ActorLocationTileIndex");

                int remainingSPAfterUsingSkill = _string.GetTagPortionValue<int>("RemainingSPAfterUsingSkill");

                int animationId = _string.GetTagPortionValue<int>("AnimationId");

                if (_string.StartsWith("<ActionLog_UnitTargetingSkill>"))
                    return StringToUnitTargetingSkillActionLog(_string, _actionTurn, _actorId, _actorName, _actorNickname, skillName, actorLocationTileIndex, remainingSPAfterUsingSkill, animationId);
                if (_string.StartsWith("<ActionLog_TileTargetingSkill>"))
                    return StringToTileTargetingSkillActionLog(_string, _actionTurn, _actorId, _actorName, _actorNickname, skillName, actorLocationTileIndex, remainingSPAfterUsingSkill, animationId);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ActionLog_UnitTargetingSkill StringToUnitTargetingSkillActionLog(string _string, decimal _actionTurn, int _actorId, string _actorName, string _actorNickname, string _skillName, int _actorLocationTileIndex, int _remainingSPAfterUsingSkill, int _animationId)
        {
            try
            {
                string sectionString = string.Empty;

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Targets");
                List<Tuple<string, string, string>> targetsName_Nickname_OwnerName = new List<Tuple<string, string, string>>();
                while (sectionString != "")
                {
                    string targetString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Target", ref targetString);

                    string name = targetString.GetTagPortionValue<string>("Name");
                    string nickname = targetString.GetTagPortionValue<string>("Nickname");
                    string ownerName = targetString.GetTagPortionValue<string>("OwnerName");

                    targetsName_Nickname_OwnerName.Add(new Tuple<string, string, string>(name, nickname, ownerName));
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("SecondaryTargets");
                List<Tuple<string, string, string>> secondaryTargetsName_Nickname_OwnerName = new List<Tuple<string, string, string>>();
                while (sectionString != "")
                {
                    string targetString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Target", ref targetString);

                    string name = targetString.GetTagPortionValue<string>("Name");
                    string nickname = targetString.GetTagPortionValue<string>("Nickname");
                    string ownerName = targetString.GetTagPortionValue<string>("OwnerName");

                    targetsName_Nickname_OwnerName.Add(new Tuple<string, string, string>(name, nickname, ownerName));
                }

                return new ActionLog_UnitTargetingSkill(_actionTurn, _actorId, _actorName, _actorNickname, _skillName, _actorLocationTileIndex, _remainingSPAfterUsingSkill, _animationId, targetsName_Nickname_OwnerName, secondaryTargetsName_Nickname_OwnerName);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ActionLog_TileTargetingSkill StringToTileTargetingSkillActionLog(string _string, decimal _actionTurn, int _actorId, string _actorName, string _actorNickname, string _skillName, int _actorLocationTileIndex, int _remainingSPAfterUsingSkill, int _animationId)
        {
            try
            {
                string sectionString = string.Empty;

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Coords");
                List<_2DCoord> targetCoords = new List<_2DCoord>();
                while (sectionString != "")
                {
                    string coordString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Coord", ref coordString);

                    int x = coordString.GetTagPortionValue<int>("X");
                    int y = coordString.GetTagPortionValue<int>("Y");

                    targetCoords.Add(new _2DCoord(x, y));
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("SecondaryCoords");
                List<_2DCoord> secondaryTargetCoords = new List<_2DCoord>();
                while (sectionString != "")
                {
                    string coordString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Coord", ref coordString);

                    int x = coordString.GetTagPortionValue<int>("X");
                    int y = coordString.GetTagPortionValue<int>("Y");

                    targetCoords.Add(new _2DCoord(x, y));
                }

                return new ActionLog_TileTargetingSkill(_actionTurn, _actorId, _actorName, _actorNickname, _skillName, _actorLocationTileIndex, _remainingSPAfterUsingSkill, _animationId, targetCoords, secondaryTargetCoords);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }
    }

    public struct OpponentUnitInfo
    {
        public int UnitIndex;
        public int Id;
        public int MaxHP;
        public int RemainingHP;
    }
}