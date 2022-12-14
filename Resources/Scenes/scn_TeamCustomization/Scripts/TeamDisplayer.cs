using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using EEANWorks.Games.Unity.Engine;
using UnityEngine.Events;
using EEANWorks.Games.TBSG._01.Unity.Data;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(InfoPanelManager_Unit))]
    [RequireComponent(typeof(InfoPanelManager_Weapon))]
    [RequireComponent(typeof(InfoPanelManager_Armour))]
    [RequireComponent(typeof(InfoPanelManager_Accessory))]
    public class TeamDisplayer : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_teamMenuPanelPrefab;
        [SerializeField]
        private Transform m_menuPanelsContainerTransform;
        [SerializeField]
        private int m_numOfFramesToEndShifting;
        #endregion

        #region Private Fields
        private InfoPanelManager_Unit m_infoPanelManager_unit;
        private InfoPanelManager_Weapon m_infoPanelManager_weapon;
        private InfoPanelManager_Armour m_infoPanelManager_armour;
        private InfoPanelManager_Accessory m_infoPanelManager_accessory;

        private Sprite m_emptyUnitSlotSprite;

        private static readonly Vector2 m_movementVector = new Vector2(1, 0);

        private static int m_selectedTeamIndex = 0;
        private static int m_selectedMemberIndex = 0;
        private static bool m_changeMember = false;

        private RectTransform m_rt_menusPanel;

        private int m_numOfMenus;

        private List<GameObject> m_gos_menu;
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
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_isInitialized = false;
            m_isInitializing = false;

            m_infoPanelManager_unit = this.GetComponent<InfoPanelManager_Unit>();
            m_infoPanelManager_weapon = this.GetComponent<InfoPanelManager_Weapon>();
            m_infoPanelManager_armour = this.GetComponent<InfoPanelManager_Armour>();
            m_infoPanelManager_accessory = this.GetComponent<InfoPanelManager_Accessory>();

            m_emptyUnitSlotSprite = SpriteContainer.Instance.EmptyObjectSprite_Set;

            m_rt_menusPanel = m_menuPanelsContainerTransform.GetComponent<RectTransform>();

            m_gos_menu = new List<GameObject>();
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

            List<Team> teams = GameDataContainer.Instance.Player.Teams;

            // Update team if member change has been requested by player
            if (m_changeMember && m_selectedTeamIndex <= teams.Count)
            {
                if (teams[m_selectedTeamIndex].Members[m_selectedMemberIndex] != GameDataContainer.Instance.SelectedUnit) // If a different unit has been selected
                {
                    LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                    yield return this.StartCoroutineRepetitionUntilTrue(ChangeMember(looperAndCoroutineLinker), looperAndCoroutineLinker);
                }

                m_changeMember = false;
            }

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

            m_isInitializing = false;
            m_isInitialized = true;
        }

        IEnumerator ChangeMember(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "ChangeMember"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
                    {"teamIndex", m_selectedTeamIndex.ToString()},
                    {"memberIndex", m_selectedMemberIndex.ToString()},
                    {"targetUnitId", GameDataContainer.Instance.SelectedUnit.UniqueId.ToString()}
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
                else if (response == "error")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error", "Someting went wrong!\nPlease try again.", "OK");
                else // Member Changed Successfully!
                {
                    // Apply the changes to the game
                    Team selectedTeam = GameDataContainer.Instance.SelectedTeam;
                    Unit selectedMember = selectedTeam.Members[m_selectedMemberIndex];
                    Unit targetUnit = GameDataContainer.Instance.SelectedUnit;

                    if (selectedTeam.Members.Contains(targetUnit)) // If targetUnit is also a member of the same team
                    {
                        int memberIndex_targetUnit = Array.IndexOf(selectedTeam.Members, targetUnit);
                        selectedTeam.Members[memberIndex_targetUnit] = selectedMember;
                    }

                    selectedTeam.Members[m_selectedMemberIndex] = targetUnit;
                }

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        private Transform InstantiatePage(int _teamIndex)
        {
            GameObject go_menu = Instantiate(m_teamMenuPanelPrefab, m_menuPanelsContainerTransform);
            go_menu.name = "Page:" + (_teamIndex + 1).ToString();

            Transform transform_menu = go_menu.transform;

            Text text_pageNumber = transform_menu.Find("Text@PageNumber").GetComponent<Text>();
            text_pageNumber.fontSize = 75;
            text_pageNumber.text = (_teamIndex + 1).ToString() + "/" + m_numOfMenus.ToString();

            return transform_menu;
        }

        private void InitializePage(Transform _transform_menu, int _teamIndex, bool _insertAsFirstPage)
        {
            RectTransform rt_menu = _transform_menu as RectTransform;
            if (m_rt_menusPanel.childCount == 1) // If this is the first page
            {
                rt_menu.anchorMin = Vector2.zero;
                rt_menu.anchorMax = Vector2.one;
            }
            else
            {
                if (_insertAsFirstPage)
                {
                    RectTransform rt_leftmostMenu = m_rt_menusPanel.GetChild(0) as RectTransform;

                    rt_menu.SetAsFirstSibling(); // Set as the first child

                    // Adjust menu position to the left end
                    rt_menu.anchorMin = rt_leftmostMenu.anchorMin - Vector2.right;
                    rt_menu.anchorMax = rt_leftmostMenu.anchorMax - Vector2.right;
                }
                else
                {
                    int lastIndexExcludingThis = m_rt_menusPanel.childCount - 2;
                    RectTransform rt_rightmostMenu = m_rt_menusPanel.GetChild(lastIndexExcludingThis) as RectTransform;

                    // Adjust menu position to the right end
                    rt_menu.anchorMin = rt_rightmostMenu.anchorMin + Vector2.right;
                    rt_menu.anchorMax = rt_rightmostMenu.anchorMax + Vector2.right;
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
                UnityAction buttonClickAction = new UnityAction(delegate
                {
                    m_selectedTeamIndex = _teamIndex;
                    m_selectedMemberIndex = memberIndex;
                    m_changeMember = true;
                    GameDataContainer.Instance.SelectedTeam = team;
                    GameDataContainer.Instance.SelectedUnit = member;
                    GameDataContainer.Instance.UnitSelectionMode = eUnitSelectionMode.Member;
                    SceneConnector.GoToScene("scn_UnitList", true);
                });
                UnityAction buttonLongPressAction = (member != null) ? new UnityAction(() => m_infoPanelManager_unit.InstantiateInfoPanel(member)) : null;
                goFormatter_unitButton.Format(member, null, buttonClickAction, buttonLongPressAction);
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

                GameObjectFormattingFunctions.FormatUnitEquipmentButtonsAsChangeable(member, transform_mainWeaponButton, transform_subWeaponButton, transform_armourButton, transform_accessoryButton, m_infoPanelManager_weapon, m_infoPanelManager_armour, m_infoPanelManager_accessory, (member == null));
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
                    goFormatter_objectButton.Format(tmp_item, null, null,
                        () =>
                        {
                            m_selectedTeamIndex = _teamIndex;
                            m_selectedMemberIndex = j;
                            GameDataContainer.Instance.SelectedTeam = team;
                            GameDataContainer.Instance.ItemSelectionMode = eItemSelectionMode.ItemSet;
                            SceneConnector.GoToScene("scn_ItemList", true);
                        });
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
                    goFormatter_objectButton.Image_Object.sprite = SpriteContainer.Instance.EmptyObjectSprite_Set;
                }
            }

            if (_insertAsFirstPage)
            {
                // Insert game object and each component to its corresponding list as the first item
                m_gos_menu.Insert(0, _transform_menu.gameObject);
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
                m_gos_menu.Add(_transform_menu.gameObject);
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
                foreach (RectTransform child in m_menuPanelsContainerTransform)
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
                    foreach (RectTransform child in m_menuPanelsContainerTransform)
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
            GameObject go_endmostMenu = _leftMost ? m_gos_menu[0] : m_gos_menu.Last();

            if (_leftMost)
            {
                m_gos_menu.RemoveAt(0);
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
                m_gos_menu.RemoveLast();
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
    }
}