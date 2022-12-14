using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(InfoPanelManager_Unit))]
    [RequireComponent(typeof(InfoPanelManager_Weapon))]
    [RequireComponent(typeof(InfoPanelManager_Armour))]
    [RequireComponent(typeof(InfoPanelManager_Accessory))]
    [RequireComponent(typeof(InfoPanelManager_Item))]
    [RequireComponent(typeof(InfoPanelManager_DungeonDrop))]
    public class DungeonLobbyManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_teamMenuPanelContainerPrefab;
        [SerializeField]
        private int m_numOfFramesToEndShifting;
        [SerializeField]
        private Transform m_tileInfosContainerPanelTransform;
        [SerializeField]
        private Transform m_unitInfosContainerPanelTransform;
        #endregion

        #region Private Fields
        private GameObject m_imagePrefab;

        private InfoPanelManager_Unit m_infoPanelManager_unit;
        private InfoPanelManager_Weapon m_infoPanelManager_weapon;
        private InfoPanelManager_Armour m_infoPanelManager_armour;
        private InfoPanelManager_Accessory m_infoPanelManager_accessory;
        private InfoPanelManager_Item m_infoPanelManager_item;
        private InfoPanelManager_DungeonDrop m_infoPanelManager_dungeonDrop;

        private Sprite m_emptyUnitSlotSprite;

        private static readonly Vector2 m_movementVector = new Vector2(1, 0);

        private static int m_selectedTeamIndex = 0;
        private static int m_selectedMemberIndex;

        private Transform m_transform_canvas;
        private Transform m_transform_menuPanels;
        private RectTransform m_rt_menusPanel;

        private int m_numOfMenus;

        private List<GameObject> m_gos_menuContainer;
        private List<Button> m_buttons_backward;
        private List<Button> m_buttons_forward;
        private List<Button[]> m_unitButtonsPerMenu;
        private List<Button[]> m_mainWeaponButtonsPerMenu;
        private List<Button[]> m_subWeaponButtonsPerMenu;
        private List<Button[]> m_armourButtonsPerMenu;
        private List<Button[]> m_accessoryButtonsPerMenu;
        private List<Button[]> m_itemButtonsPerMenu;

        private bool m_isInitialized;
        private bool m_isInitializing;

        private bool m_shiftingPage;
        private Vector2 m_movementPerFrame;

        private bool m_processingRequest;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_isInitialized = false;
            m_isInitializing = false;

            m_imagePrefab = SharedAssetContainer.Instance.ImagePrefab;

            m_infoPanelManager_unit = this.GetComponent<InfoPanelManager_Unit>();
            m_infoPanelManager_weapon = this.GetComponent<InfoPanelManager_Weapon>();
            m_infoPanelManager_armour = this.GetComponent<InfoPanelManager_Armour>();
            m_infoPanelManager_accessory = this.GetComponent<InfoPanelManager_Accessory>();
            m_infoPanelManager_item = this.GetComponent<InfoPanelManager_Item>();
            m_infoPanelManager_dungeonDrop = this.GetComponent<InfoPanelManager_DungeonDrop>();

            m_emptyUnitSlotSprite = SpriteContainer.Instance.EmptyObjectSprite_NotSet;

            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_transform_menuPanels = m_transform_canvas.Find("Panel@Background").Find("TeamSelectionSection").Find("MenusParentContainer").Find("Menus");
            m_rt_menusPanel = m_transform_menuPanels.GetComponent<RectTransform>();

            m_gos_menuContainer = new List<GameObject>();
            m_buttons_backward = new List<Button>();
            m_buttons_forward = new List<Button>();
            m_unitButtonsPerMenu = new List<Button[]>();
            m_mainWeaponButtonsPerMenu = new List<Button[]>();
            m_subWeaponButtonsPerMenu = new List<Button[]>();
            m_armourButtonsPerMenu = new List<Button[]>();
            m_accessoryButtonsPerMenu = new List<Button[]>();
            m_itemButtonsPerMenu = new List<Button[]>();

            m_shiftingPage = false;
            m_movementPerFrame = new Vector2((m_numOfFramesToEndShifting > 0) ? (1f / m_numOfFramesToEndShifting) : 1f, 0);

            m_processingRequest = false;
        }

        // Updates every frame
        void FixedUpdate()
        {
            if (!m_isInitialized && !m_isInitializing)
                StartCoroutine(Initialize());
        }

        void OnValidate()
        {
            m_movementPerFrame = new Vector2((m_numOfFramesToEndShifting > 0) ? (1f / m_numOfFramesToEndShifting) : 1f, 0);
        }

        IEnumerator Initialize()
        {
            if (m_isInitialized)
                yield break;

            m_isInitializing = true;

            // Initialize team selection section
            {
                List<Team> teams = GameDataContainer.Instance.Player.Teams;

                m_numOfMenus = teams.Count;
                if (m_numOfMenus < 1)
                {
                    m_isInitializing = false;
                    m_isInitialized = true;
                    yield break;
                }

                for (int i = 0; i < teams.Count; i++)
                {
                    InitializePage(InstantiatePage(i), i, false);
                }

                if (m_numOfMenus != 1)
                    DuplicateEndmostMenu(false);

                if (m_selectedTeamIndex > 0)
                    QuickShiftPage(true, m_selectedTeamIndex); // Show the last team that had been selected

                DisableAllUI();
                EnableSelectedMenuUI();
            }

            // Initialize dungeon info section
            {
                Dungeon dungeon = GameDataContainer.Instance.DungeonToPlay;

                List<eTileType> tileTypes = new List<eTileType>();
                List<UnitData> units = new List<UnitData>();

                foreach (Floor floor in dungeon.Floors)
                {
                    tileTypes.AddRange(GameDataContainer.Instance.TileSets[floor.TileSetId].TileTypes.Distinct());

                    foreach (DungeonUnitInfo dungeonUnitInfo in floor.DungeonUnitInfos)
                    {
                        if (!units.Any(x => x == dungeonUnitInfo.UnitData)) // If the unit has not been added
                            units.Add(dungeonUnitInfo.UnitData);
                    }
                }
                tileTypes = tileTypes.Distinct().OrderBy(x => Convert.ToInt32(x)).ToList(); // Get an ordered list that contains only one instance of each tile type

                foreach (eTileType tileType in tileTypes)
                {
                    Image image = Instantiate(m_imagePrefab, m_tileInfosContainerPanelTransform).GetComponent<Image>();
                    image.sprite = SpriteContainer.Instance.GetTileSprite(tileType);
                }

                foreach (UnitData unit in units)
                {
                    GameObjectFormatter_ObjectButton goFormatter_objectButton = Instantiate(SharedAssetContainer.Instance.ObjectButtonPrefab, m_unitInfosContainerPanelTransform).GetComponent<GameObjectFormatter_ObjectButton>();
                    goFormatter_objectButton.Format(unit);
                }
            }

            m_isInitializing = false;
            m_isInitialized = true;
        }

        private Transform InstantiatePage(int _teamIndex)
        {
            GameObject go_menuContainer = Instantiate(m_teamMenuPanelContainerPrefab, m_transform_menuPanels);
            go_menuContainer.name = "Page:" + (_teamIndex + 1).ToString();

            Transform transform_menu = go_menuContainer.transform.GetChild(0);

            Text text_pageNumber = transform_menu.Find("Text@PageNumber").GetComponent<Text>();
            text_pageNumber.fontSize = 75;
            text_pageNumber.text = (_teamIndex + 1).ToString() + "/" + m_numOfMenus.ToString();

            return transform_menu;
        }

        private void InitializePage(Transform _transform_menu, int _teamIndex, bool _insertAsFirstPage)
        {
            RectTransform rt_menuContainer = _transform_menu.parent as RectTransform;
            if (m_rt_menusPanel.childCount == 1) // If this is the first page
            {
                rt_menuContainer.anchorMin = Vector2.zero;
                rt_menuContainer.anchorMax = Vector2.one;
            }
            else
            {
                if (_insertAsFirstPage)
                {
                    RectTransform rt_leftmostMenu = m_rt_menusPanel.GetChild(0) as RectTransform;

                    rt_menuContainer.SetAsFirstSibling(); // Set as the first child

                    // Adjust menu position to the left end
                    rt_menuContainer.anchorMin = rt_leftmostMenu.anchorMin - Vector2.right;
                    rt_menuContainer.anchorMax = rt_leftmostMenu.anchorMax - Vector2.right;
                }
                else
                {
                    int lastIndexExcludingThis = m_rt_menusPanel.childCount - 2;
                    RectTransform rt_rightmostMenu = m_rt_menusPanel.GetChild(lastIndexExcludingThis) as RectTransform;

                    // Adjust menu position to the right end
                    rt_menuContainer.anchorMin = rt_rightmostMenu.anchorMin + Vector2.right;
                    rt_menuContainer.anchorMax = rt_rightmostMenu.anchorMax + Vector2.right;
                }
            }

            Team team = GameDataContainer.Instance.Player.Teams[_teamIndex];

            Button button_pageShift_backward = _transform_menu.Find("Button@Backward").GetComponent<Button>();
            button_pageShift_backward.onClick.AddListener(() => ShiftPage(false, 1));

            Button button_pageShift_forward = _transform_menu.Find("Button@Forward").GetComponent<Button>();
            button_pageShift_forward.onClick.AddListener(() => ShiftPage(true, 1));

            Transform transform_membersParent = _transform_menu.Find("Members");

            int numOfMembers = CoreValues.MAX_MEMBERS_PER_TEAM;
            Button[] buttons_unit = new Button[numOfMembers];
            Button[] buttons_mainWeapon = new Button[numOfMembers];
            Button[] buttons_subWeapon = new Button[numOfMembers];
            Button[] buttons_armour = new Button[numOfMembers];
            Button[] buttons_accessory = new Button[numOfMembers];
            for (int i = 0; i < numOfMembers; i++)
            {
                Transform transform_memberContainer = transform_membersParent.Find("MemberContainer" + (i + 1).ToString());

                Transform transform_unitButton = transform_memberContainer.Find("Panel@Unit").Find("UnitButtonContainer").Find("Button@Unit");
                buttons_unit[i] = transform_unitButton.GetComponent<Button>();

                GameObjectFormatter_ObjectButton goFormatter_unitButton = transform_unitButton.GetComponent<GameObjectFormatter_ObjectButton>();

                Unit member = team.Members[i];
                int memberIndex = i; // Using new variable to avoid reference error within delegate
                UnityAction buttonClickAction = (member != null) ? new UnityAction(() => m_infoPanelManager_unit.InstantiateInfoPanel(member)) : null;
                goFormatter_unitButton.Format(member, null, buttonClickAction);
                if (member == null)
                    goFormatter_unitButton.Image_Object.sprite = m_emptyUnitSlotSprite;

                Transform transform_equipmentButtonsParent = transform_memberContainer.Find("Panel@Equipment").Find("EquipmentButtons");

                Transform transform_mainWeaponButton = transform_equipmentButtonsParent.Find("Button@MainWeapon");
                Transform transform_subWeaponButton = transform_equipmentButtonsParent.Find("Button@SubWeapon");
                Transform transform_armourButton = transform_equipmentButtonsParent.Find("Button@Armour");
                Transform transform_accessoryButton = transform_equipmentButtonsParent.Find("Button@Accessory");

                buttons_mainWeapon[i] = transform_mainWeaponButton.GetComponent<Button>();
                buttons_subWeapon[i] = transform_subWeaponButton.GetComponent<Button>();
                buttons_armour[i] = transform_armourButton.GetComponent<Button>();
                buttons_accessory[i] = transform_accessoryButton.GetComponent<Button>();

                GameObjectFormattingFunctions.FormatUnitEquipmentButtonsAsNonChangeable(member, transform_mainWeaponButton, transform_subWeaponButton, transform_armourButton, transform_accessoryButton, m_infoPanelManager_weapon, m_infoPanelManager_armour, m_infoPanelManager_accessory);
            }

            Transform transform_itemButtonsParent = _transform_menu.Find("Panel@ItemSet")?.Find("Items");

            List<KeySelectorAndSortType<BattleItem>> sortConditions = new List<KeySelectorAndSortType<BattleItem>>
            {
                new KeySelectorAndSortType<BattleItem>(x => Convert.ToInt32(x.Rarity), eSortType.Descending),
                new KeySelectorAndSortType<BattleItem>(x => Convert.ToInt32(x.Id), eSortType.Ascending)
            };

            Button[] buttons_item = new Button[CoreValues.MAX_ITEMS_PER_TEAM];
            int itemCount = 0;
            foreach (BattleItem item in team.ItemSet.QuantityPerItem.Keys.ToList().OrderByMultipleConditions(sortConditions))
            {
                int quantity = team.ItemSet.QuantityPerItem[item];
                for (int j = 0; j < quantity; j++)
                {
                    itemCount++;
                    Transform transform_itemButton = transform_itemButtonsParent.Find("Button@Item" + itemCount.ToString());
                    buttons_item[itemCount - 1] = transform_itemButton.GetComponent<Button>();

                    GameObjectFormatter_ObjectButton goFormatter_objectButton = transform_itemButton.GetComponent<GameObjectFormatter_ObjectButton>();
                    BattleItem tmp_item = item; // Variable to avoid error within lambda expression
                    UnityAction buttonClickAction = new UnityAction(() => m_infoPanelManager_item.InstantiateInfoPanel(item));
                    goFormatter_objectButton.Format(item, null, buttonClickAction);
                }
            }
            if (itemCount != buttons_item.Length) // If the quantity of items in the item set is not equal to the limit
            {
                // Get and store the button components for those that do not have any item assigned
                int remainingItemSlots = buttons_item.Length - itemCount;
                for (int j = 0; j < remainingItemSlots; j++)
                {
                    itemCount++;
                    Transform transform_itemButton = transform_itemButtonsParent.Find("Button@Item" + itemCount.ToString());
                    buttons_item[itemCount - 1] = transform_itemButton.GetComponent<Button>();

                    GameObjectFormatter_ObjectButton goFormatter_objectButton = transform_itemButton.GetComponent<GameObjectFormatter_ObjectButton>();
                    goFormatter_objectButton.Format(null);
                    goFormatter_objectButton.Image_Object.sprite = SpriteContainer.Instance.EmptyObjectSprite_NotSet;
                }
            }

            if (_insertAsFirstPage)
            {
                // Insert game object and each component to its corresponding list as the first item
                m_gos_menuContainer.Insert(0, _transform_menu.parent.gameObject);
                m_buttons_backward.Insert(0, button_pageShift_backward);
                m_buttons_forward.Insert(0, button_pageShift_forward);
                m_unitButtonsPerMenu.Insert(0, buttons_unit);
                m_mainWeaponButtonsPerMenu.Insert(0, buttons_mainWeapon);
                m_subWeaponButtonsPerMenu.Insert(0, buttons_subWeapon);
                m_armourButtonsPerMenu.Insert(0, buttons_armour);
                m_accessoryButtonsPerMenu.Insert(0, buttons_accessory);
                m_itemButtonsPerMenu.Insert(0, buttons_item);
            }
            else
            {
                // Add game object and each component to its corresponding list
                m_gos_menuContainer.Add(_transform_menu.parent.gameObject);
                m_buttons_backward.Add(button_pageShift_backward);
                m_buttons_forward.Add(button_pageShift_forward);
                m_unitButtonsPerMenu.Add(buttons_unit);
                m_mainWeaponButtonsPerMenu.Add(buttons_mainWeapon);
                m_subWeaponButtonsPerMenu.Add(buttons_subWeapon);
                m_armourButtonsPerMenu.Add(buttons_armour);
                m_accessoryButtonsPerMenu.Add(buttons_accessory);
                m_itemButtonsPerMenu.Add(buttons_item);
            }
        }

        private void ShiftPage(bool _shiftForward, int _timesToShift)
        {
            if (m_rt_menusPanel.childCount == 1) // If there is only one menu
                return;

            if (!m_shiftingPage)
            {
                if (m_numOfFramesToEndShifting < 1)
                    QuickShiftPage(_shiftForward, 1);
                else
                {
                    StartCoroutine(MoveMenus(_shiftForward, _timesToShift));
                }
            }
        }
        IEnumerator MoveMenus(bool _shiftForward, int _timesToShift)
        {
            if (m_rt_menusPanel.childCount == 1) // If there is only one menu
                yield break;

            if (m_shiftingPage)
                yield break;

            m_shiftingPage = true;

            DisableSelectedMenuUI();

            if (_shiftForward)
                DuplicateMenus(1, 1 + (_timesToShift - 1), false);
            else
            {
                int penultimateMenuIndex = m_rt_menusPanel.childCount - 2;
                DuplicateMenus(penultimateMenuIndex, penultimateMenuIndex - (_timesToShift - 1), true);
            }

            Vector2 actualMovement = _shiftForward ? (-1 * m_movementPerFrame) : m_movementPerFrame;
            for (int i = 0; i < m_numOfFramesToEndShifting; i++)
            {
                // Move each menu
                foreach (RectTransform child in m_transform_menuPanels)
                {
                    child.anchorMin += actualMovement * _timesToShift;
                    child.anchorMax += actualMovement * _timesToShift;
                }

                yield return null;
            }

            for (int i = 0; i < _timesToShift; i++)
            {
                RemoveEndmostMenu(_shiftForward);
            }

            int selectedMenuPanelIndex = _shiftForward ? 2 : 0;
            m_selectedTeamIndex = Convert.ToInt32(m_rt_menusPanel.GetChild(selectedMenuPanelIndex).name.Remove("Page:")) - 1; // Update selected team index

            EnableSelectedMenuUI();

            m_shiftingPage = false; // Ending shifting
        }

        private void QuickShiftPage(bool _shiftForward, int _timesToShift)
        {
            if (!m_shiftingPage)
            {
                m_shiftingPage = true;

                Vector2 actualMovement = _shiftForward ? (-1 * Vector2.right) : Vector2.right;
                for (int i = 0; i < _timesToShift; i++)
                {
                    // Move each menu
                    foreach (RectTransform child in m_transform_menuPanels)
                    {
                        child.anchorMin += actualMovement;
                        child.anchorMax += actualMovement;
                    }

                    int selectedMenuPanelIndex = _shiftForward ? 2 : 0;
                    m_selectedTeamIndex = Convert.ToInt32(m_rt_menusPanel.GetChild(selectedMenuPanelIndex).name.Remove("Page:")) - 1; // Update selected team index

                    RemoveEndmostMenu(_shiftForward);
                    DuplicateEndmostMenu(_shiftForward);
                }

                m_shiftingPage = false;
            }
        }

        /// <summary>
        /// Duplicate the leftmost/rightmost menu (the first/last element in the list) and add to the end/beginning of the list.
        /// </summary>
        private void DuplicateEndmostMenu(bool _leftMost)
        {
            int lastIndex = m_numOfMenus - 1;
            int teamIndex = (m_selectedTeamIndex == 0) ? lastIndex : (m_selectedTeamIndex - 1);
            InitializePage(InstantiatePage(teamIndex), teamIndex, !_leftMost);
        }
        private void DuplicateMenus(int _startIndex, int _endIndex, bool _insertAsFirstPage)
        {
            if (_startIndex < 0
                || _endIndex < 0
                || _startIndex >= m_rt_menusPanel.childCount
                || _endIndex >= m_rt_menusPanel.childCount)
            {
                return;
            }

            if (_startIndex > _endIndex)
            {
                for (int i = _endIndex; i >= _startIndex; i--)
                {
                    int teamIndex = Convert.ToInt32(m_rt_menusPanel.GetChild(i).name.Remove("Page:")) - 1;

                    InitializePage(InstantiatePage(teamIndex), teamIndex, _insertAsFirstPage);
                }
            }
            else
            {
                for (int i = _startIndex; i <= _endIndex; i++)
                {
                    int teamIndex = Convert.ToInt32(m_rt_menusPanel.GetChild(i).name.Remove("Page:")) - 1;

                    InitializePage(InstantiatePage(teamIndex), teamIndex, _insertAsFirstPage);
                }
            }
        }

        /// <summary>
        /// Remove the leftmost/rightmost menu, which is a duplicate of the rightmost/leftmost menu.
        /// </summary>
        private void RemoveEndmostMenu(bool _leftMost)
        {
            GameObject go_endmostMenu = _leftMost ? m_gos_menuContainer[0] : m_gos_menuContainer.Last();

            if (_leftMost)
            {
                m_gos_menuContainer.RemoveAt(0);
                m_buttons_backward.RemoveAt(0);
                m_buttons_forward.RemoveAt(0);
                m_unitButtonsPerMenu.RemoveAt(0);
                m_mainWeaponButtonsPerMenu.RemoveAt(0);
                m_subWeaponButtonsPerMenu.RemoveAt(0);
                m_armourButtonsPerMenu.RemoveAt(0);
                m_accessoryButtonsPerMenu.RemoveAt(0);
                m_itemButtonsPerMenu.RemoveAt(0);
            }
            else
            {
                m_gos_menuContainer.RemoveLast();
                m_buttons_backward.RemoveLast();
                m_buttons_forward.RemoveLast();
                m_unitButtonsPerMenu.RemoveLast();
                m_mainWeaponButtonsPerMenu.RemoveLast();
                m_subWeaponButtonsPerMenu.RemoveLast();
                m_armourButtonsPerMenu.RemoveLast();
                m_accessoryButtonsPerMenu.RemoveLast();
                m_itemButtonsPerMenu.RemoveLast();
            }

            go_endmostMenu.transform.parent = null;
            Destroy(go_endmostMenu);
        }

        private void EnableSelectedMenuUI()
        {
            int selectedMenuIndex = (m_numOfMenus == 1) ? 0 : 1;
            m_buttons_backward[selectedMenuIndex].interactable = (m_numOfMenus == 1) ? false : true;
            m_buttons_forward[selectedMenuIndex].interactable = (m_numOfMenus == 1) ? false : true;
        }

        private void DisableSelectedMenuUI()
        {
            int selectedMenuIndex = (m_numOfMenus == 1) ? 0 : 1;
            m_buttons_backward[selectedMenuIndex].interactable = false;
            m_buttons_forward[selectedMenuIndex].interactable = false;
        }

        private void DisableAllUI()
        {
            foreach (Button backwardButton in m_buttons_backward) { backwardButton.interactable = false; }
            foreach (Button forwardButton in m_buttons_forward) { forwardButton.interactable = false; }
        }

        public void OpenDungeonDropInfoPanel()
        {
            m_infoPanelManager_dungeonDrop.InstantiateInfoPanel(GameDataContainer.Instance.DungeonToPlay);
        }

        public void Request_EnterDungeon()
        {
            StartCoroutine(TryEnterDungeon());
        }

        IEnumerator TryEnterDungeon()
        {
            if (m_processingRequest)
                yield break;

            m_processingRequest = true;

            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(EnterDungeon(looperAndCoroutineLinker), looperAndCoroutineLinker);

            m_processingRequest = false;
        }

        IEnumerator EnterDungeon(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "EnterDungeon"},
                    {"sessionId", GameDataContainer.Instance.SessionId.ToString()},
                    {"dungeonName", GameDataContainer.Instance.DungeonToPlay.Name},
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

                if (response == "sessionExpired")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Session Error!", CoreValues.SESSION_ERROR_MESSAGE, "Return To Title", () => SceneConnector.GoToScene("scn_Title"));
                else if (response == "notExists")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "The dungeon does not exist!", "Return To Battle Mode Selection", () => SceneConnector.GoToScene("scn_BattleMode"));
                else
                {
                    try
                    {
                        List<List<FloorInstanceMemberInfo>> floorInstanceInfos = new List<List<FloorInstanceMemberInfo>>();

                        string[] floorInstanceInfoStrings = response.Split('F');
                        foreach (string floorInstanceInfoString in floorInstanceInfoStrings)
                        {
                            List<FloorInstanceMemberInfo> floorInstanceInfo = new List<FloorInstanceMemberInfo>();

                            string[] memberInfoStrings = floorInstanceInfoString.Split('M');
                            foreach (string memberInfoString in memberInfoStrings)
                            {
                                string[] valueStrings = memberInfoString.Split(',');

                                int unitId = Convert.ToInt32(valueStrings[0]);
                                UnitData unitData = GameDataContainer.Instance.UnitEncyclopedia[unitId];
                                int unitLevel = Convert.ToInt32(valueStrings[1]);
                                bool drops = Convert.ToBoolean(valueStrings[2]);
                                int numberOfDroppingItems = Convert.ToInt32(valueStrings[3]);

                                floorInstanceInfo.Add(new FloorInstanceMemberInfo(unitData, unitLevel, drops, numberOfDroppingItems));
                            }

                            floorInstanceInfos.Add(floorInstanceInfo);
                        }

                        if (floorInstanceInfos.Count > 0 && floorInstanceInfos.Any(x => x.Count > 0)) // If at least one floor instance info with at least one opponent unit has been loaded
                        {
                            GameDataContainer.Instance.FloorInstanceInfos = floorInstanceInfos;
                            SceneConnector.GoToScene("scn_SinglePlayerBattle");
                        }
                        else
                            response = "failed";
                    }
                    catch (Exception)
                    {
                        response = "failed";
                    }
                }
                if (response == "failed")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Something went wrong!\nPlease try again.", "OK");

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }
    }
}
