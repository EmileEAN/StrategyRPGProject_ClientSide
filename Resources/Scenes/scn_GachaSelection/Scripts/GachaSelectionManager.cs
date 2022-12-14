using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Connection;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine.UI;
using EEANWorks.Games.Unity.Graphics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(GachaRollRequester))]
    [RequireComponent(typeof(InfoPanelManager_Gacha))]
    public class GachaSelectionManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_gachaSelectionMenuPanelPrefab;
        [SerializeField]
        private GameObject m_rollButtonPrefab;
        [SerializeField]
        private int m_numOfFramesToEndShifting;
        #endregion

        #region Private Fields
        private static readonly Vector2 m_movementVector = new Vector2(1, 0);

        private GameUpdateManager m_gameUpdateManager;
        private GachaRollRequester m_gachaRollRequester;
        private InfoPanelManager_Gacha m_infoPanelManger_gacha;

        private static int m_selectedGachaIndex = 0;

        private Transform m_transform_menuPanels;
        private RectTransform m_rt_menusPanel;

        private int m_numOfMenus;

        private List<GameObject> m_gos_menu;
        private List<Button> m_buttons_info;
        private List<Button> m_buttons_backward;
        private List<Button> m_buttons_forward;
        private List<List<Button>> m_rollButtonsPerMenu;

        private bool m_isInitialized;
        private bool m_isInitializing;

        private bool m_shiftingPage;
        private int m_timesToShiftPage;
        private bool m_shiftForward;
        private int m_remainingFramesForShifting;
        private Vector2 m_movementPerFrame;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_isInitialized = false;
            m_isInitializing = false;

            m_gameUpdateManager = this.GetComponent<GameUpdateManager>();
            m_gachaRollRequester = this.GetComponent<GachaRollRequester>();
            m_infoPanelManger_gacha = this.GetComponent<InfoPanelManager_Gacha>();

            m_transform_menuPanels = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Panel@Menus");
            m_rt_menusPanel = m_transform_menuPanels.GetComponent<RectTransform>();

            m_gos_menu = new List<GameObject>();
            m_buttons_info = new List<Button>();
            m_buttons_backward = new List<Button>();
            m_buttons_forward = new List<Button>();
            m_rollButtonsPerMenu = new List<List<Button>>();

            m_shiftingPage = false;
            m_timesToShiftPage = 1;
            m_remainingFramesForShifting = 0;
            m_movementPerFrame = new Vector2(1f / m_numOfFramesToEndShifting, 0);
        }

        // Updates every frame
        void FixedUpdate()
        {
            if (!m_isInitialized && !m_isInitializing)
                StartCoroutine(Initialize());
            else
            {
                if (m_shiftingPage)
                {
                    if (m_remainingFramesForShifting == m_numOfFramesToEndShifting) // Shifting has just started
                        DisableSelectedMenuUI();

                    // Move the panel
                    if (m_shiftForward)
                    {
                        foreach (RectTransform child in m_transform_menuPanels)
                        {
                            child.anchorMin -= m_movementPerFrame * m_timesToShiftPage;
                            child.anchorMax -= m_movementPerFrame * m_timesToShiftPage;
                        }
                    }
                    else
                    {
                        foreach (RectTransform child in m_transform_menuPanels)
                        {
                            child.anchorMin += m_movementPerFrame;
                            child.anchorMax += m_movementPerFrame;
                        }
                    }

                    m_remainingFramesForShifting--;

                    if (m_remainingFramesForShifting == 0) // Moving the panel has just ended
                    {
                        RemoveEndmostMenu(m_shiftForward);
                        DuplicateEndmostMenu(m_shiftForward);

                        EnableSelectedMenuUI();

                        m_shiftingPage = false; // Ending shifting
                    }
                }
            }
        }

        void OnValidate()
        {
            m_movementPerFrame = new Vector2((m_numOfFramesToEndShifting > 0) ? (1f / m_numOfFramesToEndShifting) : 1f, 0);
        }

        IEnumerator Initialize()
        {
            if (m_isInitialized)
                yield break;

            if (m_gameUpdateManager.IsWorking)
                yield break;

            m_isInitializing = true;

            m_numOfMenus = GameDataContainer.Instance.Gachas.Count;
            if (m_numOfMenus < 1)
            {
                m_isInitializing = false;
                m_isInitialized = true;
                yield break;
            }

            for (int i = 0; i < GameDataContainer.Instance.Gachas.Count; i++)
            {
                GameObject go_menu = Instantiate(m_gachaSelectionMenuPanelPrefab, m_transform_menuPanels);
                go_menu.name = "Page:" + (i + 1).ToString();

                Transform transform_menu = go_menu.transform;

                Image image_background = transform_menu.GetComponent<Image>();
                image_background.sprite = ImageConverter.ByteArrayToSprite(GameDataContainer.Instance.Gachas[i].BackgroundImageAsBytes, FilterMode.Point);

                InitializePage(transform_menu, i, false);
            }

            if (m_numOfMenus != 1)
                DuplicateEndmostMenu(false);

            DisableAllUI();
            EnableSelectedMenuUI();

            if (m_selectedGachaIndex > 1)
                QuickShiftPage(true, m_selectedGachaIndex - 1); // Show the last team that had been selected

            m_isInitializing = false;
            m_isInitialized = true;
        }

        private void InitializePage(Transform _transform_menu, int _gachaIndex, bool _insertAsFirstPage)
        {
            RectTransform rt_menu = _transform_menu as RectTransform;
            if (_insertAsFirstPage)
            {
                _transform_menu.SetAsFirstSibling(); // Set as the first child

                // Adjust menu position to the left end
                rt_menu.anchorMin = new Vector2(-1, 0);
                rt_menu.anchorMax = new Vector2(0, 1);
            }
            else
            {
                int indexDistance = (_gachaIndex >= m_selectedGachaIndex) ? (_gachaIndex - m_selectedGachaIndex) : ((_gachaIndex + m_numOfMenus) - m_selectedGachaIndex);
                // Adjust menu position based on the team index
                rt_menu.anchorMin = new Vector2(indexDistance, 0);
                rt_menu.anchorMax = new Vector2(indexDistance + 1, 1);
            }

            Gacha gacha = GameDataContainer.Instance.Gachas[_gachaIndex];

            Button button_info = _transform_menu.Find("Button@Info").GetComponent<Button>();
            //button_info.onClick.AddListener(() => m_infoPanelManger_gacha.InstantiateInfoPanel(pageCounter));

            Button button_pageShift_backward = _transform_menu.Find("Button@Backward").GetComponent<Button>();
            button_pageShift_backward.onClick.AddListener(() => ShiftPage(false));

            Button button_pageShift_forward = _transform_menu.Find("Button@Forward").GetComponent<Button>();
            button_pageShift_forward.onClick.AddListener(() => ShiftPage(true));

            Transform transform_rollButtonsParent = _transform_menu.Find("RollButtons");
            DynamicGridLayoutGroup dynamicGridLayoutGroup_rollButtonsParent = transform_rollButtonsParent.GetComponent<DynamicGridLayoutGroup>();
            dynamicGridLayoutGroup_rollButtonsParent.FixedNumOfElementsPerRow = true;
            dynamicGridLayoutGroup_rollButtonsParent.ElementsPerRow = gacha.DispensationOptions.Count;
            dynamicGridLayoutGroup_rollButtonsParent.AutomaticElementWidth = true;

            List<Button> buttons_roll = new List<Button>();
            foreach (DispensationOption dispensationOption in gacha.DispensationOptions)
            {
                Transform transform_rollButtonParent = Instantiate(m_rollButtonPrefab, transform_rollButtonsParent).transform;

                Transform transform_rollButton = transform_rollButtonParent.Find("Button@Roll");
                Button button_roll = transform_rollButton.GetComponent<Button>();
                button_roll.onClick.AddListener(() => GachaConfirmationPopUpCreator.CreatePopUp(gacha, m_gachaRollRequester, dispensationOption));
                Text text_timesToRoll = transform_rollButton.Find("Text@TimesToRoll").GetComponent<Text>();
                string timesToRollText = dispensationOption.TimesToDispense.ToString();
                timesToRollText += (timesToRollText == "1") ? " Shot" : " Shots";
                text_timesToRoll.text = timesToRollText;
                Text text_costValue = transform_rollButton.Find("Text@CostValue").GetComponent<Text>();
                text_costValue.text = "x" + dispensationOption.CostValue.ToString();
                Image image_costType = transform_rollButton.Find("Image@CostType").GetComponent<Image>();
                switch (dispensationOption.CostType)
                {
                    default: // case eCostType.Gem
                        image_costType.sprite = SpriteContainer.Instance.GemSprite;
                        break;
                    case eCostType.Gold:
                        image_costType.sprite = SpriteContainer.Instance.GoldSprite;
                        break;
                    case eCostType.Item:
                        {
                            int costItemId = dispensationOption.CostItemId;
                            Item costItem = GameDataContainer.Instance.ItemEncyclopedia.First(x => x.Id == costItemId);
                            image_costType.sprite = SpriteContainer.Instance.ItemIcons[costItem];
                        }
                        break;
                }

                Transform transform_extraInfoPanel = transform_rollButtonParent.Find("Panel@ExtraInfo");
                if (dispensationOption.RemainingAttempts == -1) // Meaning infinite
                    transform_extraInfoPanel.gameObject.SetActive(false);
                else
                {
                    Text text_limitationType = transform_extraInfoPanel.Find("Text@LimitationType").GetComponent<Text>();
                    text_limitationType.text = dispensationOption.IsNumberOfAttemptsPerDay ? "Daily!" : "Limited";

                    Text text_remainingAttempts = transform_extraInfoPanel.Find("Text@RemainingAttempts").GetComponent<Text>();
                    text_remainingAttempts.text = "x " + dispensationOption.RemainingAttempts.ToString() + " Left";

                    transform_extraInfoPanel.gameObject.SetActive(true);
                }

                buttons_roll.Add(button_roll);
            }

            if (_insertAsFirstPage)
            {
                // Insert game object and each component to its corresponding list as the first item
                m_gos_menu.Insert(0, _transform_menu.gameObject);
                m_buttons_info.Insert(0, button_info);
                m_buttons_backward.Insert(0, button_pageShift_backward);
                m_buttons_forward.Insert(0, button_pageShift_forward);
                m_rollButtonsPerMenu.Insert(0, buttons_roll);
            }
            else
            {
                // Add game object and each component to its corresponding list
                m_gos_menu.Add(_transform_menu.gameObject);
                m_buttons_info.Add(button_info);
                m_buttons_backward.Add(button_pageShift_backward);
                m_buttons_forward.Add(button_pageShift_forward);
                m_rollButtonsPerMenu.Add(buttons_roll);
            }
        }

        private void ShiftPage(bool _shiftForward)
        {
            if (!m_shiftingPage)
            {
                if (m_numOfFramesToEndShifting < 1)
                    QuickShiftPage(_shiftForward, 1);
                else
                {
                    m_shiftingPage = true;
                    m_timesToShiftPage = 1;
                    m_shiftForward = _shiftForward;
                    m_remainingFramesForShifting = m_numOfFramesToEndShifting;
                }
            }
        }

        private void QuickShiftPage(bool _shiftForward, int _timesToShift)
        {
            if (!m_shiftingPage)
            {
                m_shiftingPage = true;
                m_timesToShiftPage = _timesToShift;
                m_shiftForward = _shiftForward;
                m_remainingFramesForShifting = 1;
            }
        }

        /// <summary>
        /// Duplicate the leftmost/rightmost menu (the first/last element in the list) and add to the end/beginning of the list.
        /// </summary>
        private void DuplicateEndmostMenu(bool _leftMost)
        {
            GameObject go_duplicatedMenu = Instantiate((_leftMost ? m_gos_menu[0] : m_gos_menu.Last()), m_transform_menuPanels);
            int lastIndex = m_numOfMenus - 1;
            int gachaIndex = (m_selectedGachaIndex == 0) ? lastIndex : (m_selectedGachaIndex - 1);
            InitializePage(go_duplicatedMenu.transform, gachaIndex, !_leftMost);
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
                m_buttons_info.RemoveAt(0);
                m_buttons_backward.RemoveAt(0);
                m_buttons_forward.RemoveAt(0);
                m_rollButtonsPerMenu.RemoveAt(0);
            }
            else
            {
                m_gos_menu.RemoveLast();
                m_buttons_info.RemoveLast();
                m_buttons_backward.RemoveLast();
                m_buttons_forward.RemoveLast();
                m_rollButtonsPerMenu.RemoveLast();
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