using EEANWorks;
using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
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
    [RequireComponent(typeof(InfoPanelManager_Weapon))]
    [RequireComponent(typeof(InfoPanelManager_Armour))]
    [RequireComponent(typeof(InfoPanelManager_Accessory))]
    public class OwnedEquipmentsDisplayer : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Transform m_contentsTransform;
        [SerializeField]
        private Transform m_sideBarTransform;
        [SerializeField]
        private Transform m_selectionEndingPanelTransform;
        [SerializeField]
        private Transform m_sortModePanelTransform;
        [SerializeField]
        private Transform m_filterModePanelTransform;
        #endregion

        #region Private Fields
        private const int MAX_NUM_OF_EQUIPMENTS_TO_SELECT = 20;
        private const string EQUIPPED_TEXT = "<color=green>E</color>";

        private GameObject m_equipmentButtonPrefab;

        private Dropdown[] m_dropdowns_sort;
        private Toggle[] m_toggles_sort;

        private ImageToggle[] m_toggles_rarity;
        private Transform m_transform_weaponType;
        private Toggle[] m_toggles_weaponType;
        private Transform m_transform_level;
        private RangeSlider m_rangeSlider_level;

        private Text m_text_applySortModeButton;
        private Text m_text_applyFilterModeButton;

        private Text m_text_selectionCount;
        private Button m_button_selectionEnding;

        private Button m_button_weapon;
        private Button m_button_armour;
        private Button m_button_accessory;

        private List<Weapon> m_weapons;
        private List<Armour> m_armours;
        private List<Accessory> m_accessories;

        private eEquipmentClassification m_equipmentClassification;
        private List<Weapon> m_weaponsToDisplay;
        private List<Armour> m_armoursToDisplay;
        private List<Accessory> m_accessoriesToDisplay;
        private List<KeySelectorAndSortType<Weapon>> m_sortConditions_weapon;
        private List<KeySelectorAndSortType<Armour>> m_sortConditions_armour;
        private List<KeySelectorAndSortType<Accessory>> m_sortConditions_accessory;

        private InfoPanelManager_Weapon m_infoPanelManager_weapon;
        private InfoPanelManager_Armour m_infoPanelManager_armour;
        private InfoPanelManager_Accessory m_infoPanelManager_accessory;
        private Dictionary<Weapon, Transform> m_transforms_weaponButton;
        private Dictionary<Armour, Transform> m_transforms_armourButton;
        private Dictionary<Accessory, Transform> m_transforms_accessoryButton;
        private List<AdvancedButton> m_buttons_equipment;
        private List<Image> m_images_rarityTexture;
        private List<Text> m_texts_value;

        private DynamicGridLayoutGroup m_dynamicGridLayoutGroup;

        //private Dictionary<Item, int> m_selectedItems;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_equipmentButtonPrefab = SharedAssetContainer.Instance.ObjectButtonPrefab;

            m_sortConditions_weapon = new List<KeySelectorAndSortType<Weapon>>();
            m_sortConditions_armour = new List<KeySelectorAndSortType<Armour>>();
            m_sortConditions_accessory = new List<KeySelectorAndSortType<Accessory>>();

            m_transforms_weaponButton = new Dictionary<Weapon, Transform>();
            m_transforms_armourButton = new Dictionary<Armour, Transform>();
            m_transforms_accessoryButton = new Dictionary<Accessory, Transform>();
            m_buttons_equipment = new List<AdvancedButton>();
            m_images_rarityTexture = new List<Image>();
            m_texts_value = new List<Text>();

            m_infoPanelManager_weapon = this.GetComponent<InfoPanelManager_Weapon>();
            m_infoPanelManager_armour = this.GetComponent<InfoPanelManager_Armour>();
            m_infoPanelManager_accessory = this.GetComponent<InfoPanelManager_Accessory>();

            // Initialize values for sort mode panel
            Transform transform_priorities = m_sortModePanelTransform.Find("Priorities");
            m_dropdowns_sort = new Dropdown[transform_priorities.childCount];
            m_toggles_sort = new Toggle[transform_priorities.childCount];
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (var equipmentSortMode in Enum.GetNames(typeof(eEquipmentSortMode)))
            {
                options.Add(new Dropdown.OptionData(equipmentSortMode));
            }
            for (int i = 0; i < transform_priorities.childCount; i++)
            {
                Transform transform_priority = transform_priorities.Find("Priority" + (i + 1).ToString());

                List<Dropdown.OptionData> optionsCopy = new List<Dropdown.OptionData>(options);

                m_dropdowns_sort[i] = transform_priority.Find("Dropdown").GetComponent<Dropdown>();
                if (i == 0)
                    optionsCopy.Remove(options.First()); // Remove the None option from the first dropdown
                m_dropdowns_sort[i].options = optionsCopy;

                m_toggles_sort[i] = transform_priority.Find("Toggle").GetComponent<Toggle>();
                m_toggles_sort[i].gameObject.SetActive(false);

                int index = i; // Used to prevent the delegate from throwing IndexOutOfRangeException
                m_dropdowns_sort[i].onValueChanged.AddListener(delegate { UpdateSortToggle(m_dropdowns_sort[index], m_toggles_sort[index]); });

                // Set the default sort criteria
                if (i == 0)
                {
                    m_dropdowns_sort[i].value = (int)(eEquipmentSortMode.Rarity) - 1; // Subtracting 1 because the first option has been removed
                    m_toggles_sort[i].isOn = true;
                }
                else if (i == 1)
                {
                    m_dropdowns_sort[i].value = (int)(eEquipmentSortMode.Id);
                    m_toggles_sort[i].isOn = false;
                }

                UpdateSortToggle(m_dropdowns_sort[i], m_toggles_sort[i]);
            }
            m_text_applySortModeButton = m_sortModePanelTransform.Find("Button@Apply").Find("Text").GetComponent<Text>();
            m_sortModePanelTransform.gameObject.SetActive(false);

            // Initialize values for filter mode panel
            m_toggles_rarity = new ImageToggle[Enum.GetNames(typeof(eRarity)).Length];
            Transform transform_rarityToggles = m_filterModePanelTransform.Find("Rarity").Find("Toggles");
            for (int i = 0; i < m_toggles_rarity.Length; i++)
            {
                int rarityValue = (i + 1) * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES;
                m_toggles_rarity[i] = transform_rarityToggles.Find("ImageToggle@" + ((eRarity)rarityValue).ToString()).GetComponent<ImageToggle>();
            }
            m_toggles_weaponType = new Toggle[Enum.GetNames(typeof(eWeaponType)).Length - 1];
            m_transform_weaponType = m_filterModePanelTransform.Find("WeaponType");
            Transform transform_weaponTypeToggles = m_transform_weaponType.Find("Toggles");
            for (int i = 0; i < m_toggles_weaponType.Length; i++)
            {
                m_toggles_weaponType[i] = transform_weaponTypeToggles.Find("Toggle@" + ((eWeaponType)i).ToString()).GetComponent<Toggle>();
            }
            m_transform_level = m_filterModePanelTransform.Find("Level");
            m_rangeSlider_level = m_transform_level.Find("RangeSlider").GetComponent<RangeSlider>();
            m_text_applyFilterModeButton = m_filterModePanelTransform.Find("Button@Apply").Find("Text").GetComponent<Text>();
            m_filterModePanelTransform.gameObject.SetActive(false);

            // Initialize values for selection ending panel (used when required multiple selection)
            m_text_selectionCount = m_selectionEndingPanelTransform.Find("Text@SelectionCount").GetComponent<Text>();
            m_button_selectionEnding = m_selectionEndingPanelTransform.Find("Button@SelectionEnding").GetComponent<Button>();

            // Initialize values for side bar panel (used in list mode)
            m_button_weapon = m_sideBarTransform.Find("Button@Weapon").GetComponent<Button>();
            m_button_armour = m_sideBarTransform.Find("Button@Armour").GetComponent<Button>();
            m_button_accessory = m_sideBarTransform.Find("Button@Accessory").GetComponent<Button>();

            // Set equipments to display
            m_dynamicGridLayoutGroup = m_contentsTransform.GetComponent<DynamicGridLayoutGroup>();
            m_dynamicGridLayoutGroup.RefreshLayoutOnTransformChildrenChanged = false; // Required so that the layout is not refreshed every time that a equipment button is instantiated in a loop

            m_weapons = new List<Weapon>();
            m_armours = new List<Armour>();
            m_accessories = new List<Accessory>();

            Player player = GameDataContainer.Instance.Player;
            switch (GameDataContainer.Instance.EquipmentSelectionMode)
            {
                default: // case eEquipmentSelectionMode.List
                    {
                        m_weapons = player.WeaponsOwned;
                        m_armours = player.ArmoursOwned;
                        m_accessories = player.AccessoriesOwned;

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(true);

                        m_button_weapon.onClick.AddListener(delegate { m_equipmentClassification = eEquipmentClassification.Weapon; RefreshFilterModePanel(); m_weaponsToDisplay = new List<Weapon>(m_weapons); InstantiateEquipments(); Filter(); SetInteractableForAllSideBarButtons(true); m_button_weapon.interactable = false; });
                        m_button_armour.onClick.AddListener(delegate { m_equipmentClassification = eEquipmentClassification.Armour; RefreshFilterModePanel(); m_armoursToDisplay = new List<Armour>(m_armours); InstantiateEquipments(); Filter(); SetInteractableForAllSideBarButtons(true); m_button_armour.interactable = false; });
                        m_button_accessory.onClick.AddListener(delegate { m_equipmentClassification = eEquipmentClassification.Accessory; RefreshFilterModePanel(); m_accessoriesToDisplay = new List<Accessory>(m_accessories); InstantiateEquipments(); Filter(); SetInteractableForAllSideBarButtons(true); m_button_accessory.interactable = false; });

                        m_button_weapon.interactable = false;
                        m_equipmentClassification = eEquipmentClassification.Weapon;
                    }
                    break;

                case eEquipmentSelectionMode.UnitMainWeapon:
                    {
                        Unit selectedUnit = GameDataContainer.Instance.SelectedUnit;
                        m_weapons = player.WeaponsOwned.Where(x => x != selectedUnit.MainWeapon).Where(x => selectedUnit.BaseInfo.EquipableWeaponClassifications.ContainsAny(x.BaseInfo.WeaponClassifications)).ToList();

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eEquipmentSelectionMode.UnitSubWeapon:
                    {
                        Unit selectedUnit = GameDataContainer.Instance.SelectedUnit;
                        m_weapons = player.WeaponsOwned.Where(x => x != selectedUnit.SubWeapon).Where(x => selectedUnit.BaseInfo.EquipableWeaponClassifications.ContainsAny(x.BaseInfo.WeaponClassifications)).ToList();

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eEquipmentSelectionMode.UnitArmour:
                    {
                        Unit selectedUnit = GameDataContainer.Instance.SelectedUnit;
                        m_armours = player.ArmoursOwned.Where(x => x != selectedUnit.Armour).Where(x => selectedUnit.BaseInfo.EquipableArmourClassifications.Contains(x.BaseInfo.ArmourClassification)).ToList();

                        m_transform_weaponType.gameObject.SetActive(false);
                        m_transform_level.gameObject.SetActive(false);

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eEquipmentSelectionMode.UnitAccessory:
                    {
                        Unit selectedUnit = GameDataContainer.Instance.SelectedUnit;
                        m_accessories = player.AccessoriesOwned.Where(x => x != selectedUnit.Accessory).Where(x => selectedUnit.BaseInfo.EquipableAccessoryClassifications.Contains(x.BaseInfo.AccessoryClassification)).ToList();

                        m_transform_weaponType.gameObject.SetActive(false);
                        m_transform_level.gameObject.SetActive(false);

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eEquipmentSelectionMode.UpgradingWeapon:
                    {
                        m_weapons = player.WeaponsOwned.Where(x => GameDataContainer.Instance.WeaponRecipes.Any(y => y.WeaponToUpgrade == x.BaseInfo)).ToList();

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eEquipmentSelectionMode.UpgradingArmour:
                    {
                        m_armours = player.ArmoursOwned.Where(x => GameDataContainer.Instance.ArmourRecipes.Any(y => y.ArmourToUpgrade == x.BaseInfo)).ToList(); ;

                        m_transform_weaponType.gameObject.SetActive(false);
                        m_transform_level.gameObject.SetActive(false);

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eEquipmentSelectionMode.UpgradingAccessory:
                    {
                        m_accessories = player.AccessoriesOwned.Where(x => GameDataContainer.Instance.AccessoryRecipes.Any(y => y.AccessoryToUpgrade == x.BaseInfo)).ToList(); ;

                        m_transform_weaponType.gameObject.SetActive(false);
                        m_transform_level.gameObject.SetActive(false);

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eEquipmentSelectionMode.EnhancingLevelableWeapon:
                    {
                        m_weapons = player.WeaponsOwned.OfType<LevelableWeapon>().Where(x => !Calculator.IsMaxLevel(x)).Cast<Weapon>().ToList();
                        m_weapons.AddRange(player.WeaponsOwned.OfType<LevelableTransformableWeapon>().Where(x => !Calculator.IsMaxLevel(x)).Cast<Weapon>().ToList());

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;
            }

            m_weaponsToDisplay = new List<Weapon>(m_weapons);
            m_armoursToDisplay = new List<Armour>(m_armours);
            m_accessoriesToDisplay = new List<Accessory>(m_accessories);

            InstantiateEquipments();
            Sort();
        }

        private void InstantiateEquipments()
        {
            m_transforms_weaponButton.Clear();
            m_transforms_armourButton.Clear();
            m_transforms_accessoryButton.Clear();
            m_buttons_equipment.Clear();
            m_images_rarityTexture.Clear();
            m_texts_value.Clear();
            m_contentsTransform.ClearChildren();

            eEquipmentSelectionMode equipmentSelectionMode = GameDataContainer.Instance.EquipmentSelectionMode;

            switch (m_equipmentClassification)
            {
                default: // case eEquipmentClassification.Weapon
                    {
                        foreach (Weapon weapon in m_weaponsToDisplay)
                        {
                            GameObject go_equipmentButton = Instantiate(m_equipmentButtonPrefab, m_contentsTransform);
                            m_transforms_weaponButton.Add(weapon, go_equipmentButton.GetComponent<RectTransform>());

                            GameObjectFormatter_ObjectButton goFormatter_objectButton = go_equipmentButton.GetComponent<GameObjectFormatter_ObjectButton>();
                            goFormatter_objectButton.Format(weapon);

                            Text text_value = goFormatter_objectButton.Text_Value;
                            eEquipmentSortMode primarySortMode = m_dropdowns_sort[0].options[m_dropdowns_sort[0].value].text.ToCorrespondingEnumValue<eEquipmentSortMode>();
                            if (equipmentSelectionMode == eEquipmentSelectionMode.UnitMainWeapon || equipmentSelectionMode == eEquipmentSelectionMode.UnitSubWeapon)
                            {
                                text_value.text = (GameDataContainer.Instance.Player.UnitsOwned.Any(x => x.MainWeapon == weapon || x.SubWeapon == weapon)) ? EQUIPPED_TEXT : "";
                            }
                            else
                            {
                                switch (primarySortMode)
                                {
                                    case eEquipmentSortMode.Level:
                                        text_value.text = Calculator.Level(weapon).ToString();
                                        break;
                                    default:
                                        text_value.text = "";
                                        break;
                                }
                            }

                            m_buttons_equipment.Add(goFormatter_objectButton.Button_Object);
                            m_images_rarityTexture.Add(goFormatter_objectButton.Image_RarityTexture);
                            m_texts_value.Add(text_value);
                        }

                        switch (equipmentSelectionMode)
                        {
                            default: // case eEquipmentSelectionMode.List
                                {
                                    for (int i = 0; i < m_weaponsToDisplay.Count; i++)
                                    {
                                        Weapon weapon = m_weaponsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                                        m_buttons_equipment[i].OnClick.AddListener(() => m_infoPanelManager_weapon.InstantiateInfoPanel(weapon));
                                        m_buttons_equipment[i].interactable = true;
                                    }
                                }
                                break;

                            case eEquipmentSelectionMode.UnitMainWeapon:
                            case eEquipmentSelectionMode.UnitSubWeapon:
                            case eEquipmentSelectionMode.UpgradingWeapon:
                            case eEquipmentSelectionMode.EnhancingLevelableWeapon:
                                {
                                    for (int i = 0; i < m_weaponsToDisplay.Count; i++)
                                    {
                                        Weapon weapon = m_weaponsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                                        m_buttons_equipment[i].OnClick.AddListener(() => Select(weapon));
                                        m_buttons_equipment[i].EnableLongPressAsDefault();
                                        m_buttons_equipment[i].OnLongPress.AddListener(() => m_infoPanelManager_weapon.InstantiateInfoPanel(weapon));
                                        m_buttons_equipment[i].interactable = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case eEquipmentClassification.Armour:
                    {
                        foreach (Armour armour in m_armoursToDisplay)
                        {
                            GameObject go_equipmentButton = Instantiate(m_equipmentButtonPrefab, m_contentsTransform);
                            m_transforms_armourButton.Add(armour, go_equipmentButton.GetComponent<RectTransform>());

                            GameObjectFormatter_ObjectButton goFormatter_objectButton = go_equipmentButton.GetComponent<GameObjectFormatter_ObjectButton>();
                            goFormatter_objectButton.Format(armour);

                            Text text_value = goFormatter_objectButton.Text_Value;
                            eEquipmentSortMode primarySortMode = m_dropdowns_sort[0].options[m_dropdowns_sort[0].value].text.ToCorrespondingEnumValue<eEquipmentSortMode>();
                            if (equipmentSelectionMode == eEquipmentSelectionMode.UnitArmour)
                                text_value.text = EQUIPPED_TEXT;
                            else
                            {
                                switch (primarySortMode)
                                {
                                    default:
                                        text_value.text = "";
                                        break;
                                }
                            }

                            m_buttons_equipment.Add(goFormatter_objectButton.Button_Object);
                            m_images_rarityTexture.Add(goFormatter_objectButton.Image_RarityTexture);
                            m_texts_value.Add(text_value);
                        }

                        switch (equipmentSelectionMode)
                        {
                            default: // case eEquipmentSelectionMode.List
                                {
                                    for (int i = 0; i < m_armoursToDisplay.Count; i++)
                                    {
                                        Armour armour = m_armoursToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                                        m_buttons_equipment[i].OnClick.AddListener(() => m_infoPanelManager_armour.InstantiateInfoPanel(armour));
                                        m_buttons_equipment[i].interactable = true;
                                    }
                                }
                                break;

                            case eEquipmentSelectionMode.UnitArmour:
                            case eEquipmentSelectionMode.UpgradingArmour:
                                {
                                    for (int i = 0; i < m_armoursToDisplay.Count; i++)
                                    {
                                        Armour armour = m_armoursToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                                        m_buttons_equipment[i].OnClick.AddListener(() => Select(armour));
                                        m_buttons_equipment[i].EnableLongPressAsDefault();
                                        m_buttons_equipment[i].OnLongPress.AddListener(() => m_infoPanelManager_armour.InstantiateInfoPanel(armour));
                                        m_buttons_equipment[i].interactable = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case eEquipmentClassification.Accessory:
                    {
                        foreach (Accessory accessory in m_accessoriesToDisplay)
                        {
                            GameObject go_equipmentButton = Instantiate(m_equipmentButtonPrefab, m_contentsTransform);
                            m_transforms_accessoryButton.Add(accessory, go_equipmentButton.GetComponent<RectTransform>());

                            GameObjectFormatter_ObjectButton goFormatter_objectButton = go_equipmentButton.GetComponent<GameObjectFormatter_ObjectButton>();
                            goFormatter_objectButton.Format(accessory);

                            Text text_value = goFormatter_objectButton.Text_Value;
                            eEquipmentSortMode primarySortMode = m_dropdowns_sort[0].options[m_dropdowns_sort[0].value].text.ToCorrespondingEnumValue<eEquipmentSortMode>();
                            if (equipmentSelectionMode == eEquipmentSelectionMode.UnitAccessory)
                                text_value.text = EQUIPPED_TEXT;
                            else
                            {
                                switch (primarySortMode)
                                {
                                    default:
                                        text_value.text = "";
                                        break;
                                }
                            }

                            m_buttons_equipment.Add(goFormatter_objectButton.Button_Object);
                            m_images_rarityTexture.Add(goFormatter_objectButton.Image_RarityTexture);
                            m_texts_value.Add(text_value);
                        }

                        switch (equipmentSelectionMode)
                        {
                            default: // case eEquipmentSelectionMode.List
                                {
                                    for (int i = 0; i < m_accessoriesToDisplay.Count; i++)
                                    {
                                        Accessory accessory = m_accessoriesToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                                        m_buttons_equipment[i].OnClick.AddListener(() => m_infoPanelManager_accessory.InstantiateInfoPanel(accessory));
                                        m_buttons_equipment[i].interactable = true;
                                    }
                                }
                                break;

                            case eEquipmentSelectionMode.UnitArmour:
                            case eEquipmentSelectionMode.UpgradingArmour:
                                {
                                    for (int i = 0; i < m_accessoriesToDisplay.Count; i++)
                                    {
                                        Accessory accessory = m_accessoriesToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                                        m_buttons_equipment[i].OnClick.AddListener(() => Select(accessory));
                                        m_buttons_equipment[i].EnableLongPressAsDefault();
                                        m_buttons_equipment[i].OnLongPress.AddListener(() => m_infoPanelManager_accessory.InstantiateInfoPanel(accessory));
                                        m_buttons_equipment[i].interactable = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private void SetInteractableForAllSideBarButtons(bool _interactable)
        {
            m_button_weapon.interactable = _interactable;
            m_button_armour.interactable = _interactable;
            m_button_accessory.interactable = _interactable;
        }

        private void UpdateSortToggle(Dropdown _dropdown, Toggle _toggle)
        {
            eEquipmentSortMode sortMode = _dropdown.options[_dropdown.value].text.ToCorrespondingEnumValue<eEquipmentSortMode>();
            switch (sortMode)
            {
                case eEquipmentSortMode.None:
                    _toggle.gameObject.SetActive(false);
                    break;
                default:
                    _toggle.gameObject.SetActive(true);
                    break;
            }
        }

        private void Select(Weapon _weapon) { StartCoroutine(SelectWeapon(_weapon)); }
        private IEnumerator SelectWeapon(Weapon _weapon)
        {
            GameDataContainer.Instance.SelectedWeapon = _weapon;

            eEquipmentSelectionMode equipmentSelectionMode = GameDataContainer.Instance.EquipmentSelectionMode;
            if (equipmentSelectionMode == eEquipmentSelectionMode.UnitMainWeapon || equipmentSelectionMode == eEquipmentSelectionMode.UnitSubWeapon)
                yield return StartCoroutine(TryChangeEquipment());

            SceneConnector.GoToPreviousScene();
        }
        private void Select(Armour _armour) { StartCoroutine(SelectArmour(_armour)); }
        private IEnumerator SelectArmour(Armour _armour)
        {
            GameDataContainer.Instance.SelectedArmour = _armour;

            if (GameDataContainer.Instance.EquipmentSelectionMode == eEquipmentSelectionMode.UnitArmour)
                yield return StartCoroutine(TryChangeEquipment());

            SceneConnector.GoToPreviousScene();
        }
        private void Select(Accessory _accessory) { StartCoroutine(SelectAccessory(_accessory)); }
        private IEnumerator SelectAccessory(Accessory _accessory)
        {
            GameDataContainer.Instance.SelectedAccessory = _accessory;

            if (GameDataContainer.Instance.EquipmentSelectionMode == eEquipmentSelectionMode.UnitAccessory)
                yield return StartCoroutine(TryChangeEquipment());

            SceneConnector.GoToPreviousScene();
        }

        IEnumerator TryChangeEquipment()
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(ChangeEquipment(looperAndCoroutineLinker), looperAndCoroutineLinker);
        }

        public void OpenSortModePanel() { m_sortModePanelTransform.gameObject.SetActive(true); }

        public void OpenFilterModePanel() { m_filterModePanelTransform.gameObject.SetActive(true); }

        public void CloseSortModePanel() { m_sortModePanelTransform.gameObject.SetActive(false); }

        public void CloseFilterModePanel() { m_filterModePanelTransform.gameObject.SetActive(false); }

        public void Sort()
        {
            StartCoroutine(SortEquipments(false, true));
        }
        IEnumerator SortEquipments(bool _usePreviousConditions = false, bool _closeSortModePanel = false)
        {
            if (_closeSortModePanel)
            {
                m_text_applyFilterModeButton.text = "Applying...";
                yield return null;
            }

            if (!_usePreviousConditions) // Set all the conditions specified through the UI
            {
                switch (m_equipmentClassification)
                {
                    default: // case eEquipmentClassification.Weapon
                        {
                            m_sortConditions_weapon.Clear();
                            for (int i = 0; i < m_dropdowns_sort.Length; i++)
                            {
                                eEquipmentSortMode sortMode = m_dropdowns_sort[i].options[m_dropdowns_sort[i].value].text.ToCorrespondingEnumValue<eEquipmentSortMode>();
                                switch (sortMode)
                                {
                                    default: // case eEquipmentSortMode.None
                                        break;
                                    case eEquipmentSortMode.Id:
                                        {
                                            if (m_toggles_sort[i].isOn)
                                                m_sortConditions_weapon.Add(new KeySelectorAndSortType<Weapon>(x => x.BaseInfo.Id, eSortType.Descending));
                                            else
                                                m_sortConditions_weapon.Add(new KeySelectorAndSortType<Weapon>(x => x.BaseInfo.Id, eSortType.Ascending));
                                        }
                                        break;
                                    case eEquipmentSortMode.Rarity:
                                        {
                                            if (m_toggles_sort[i].isOn)
                                                m_sortConditions_weapon.Add(new KeySelectorAndSortType<Weapon>(x => x.BaseInfo.Rarity, eSortType.Descending));
                                            else
                                                m_sortConditions_weapon.Add(new KeySelectorAndSortType<Weapon>(x => x.BaseInfo.Rarity, eSortType.Ascending));
                                        }
                                        break;
                                    case eEquipmentSortMode.Level:
                                        {
                                            if (m_toggles_sort[i].isOn)
                                                m_sortConditions_weapon.Add(new KeySelectorAndSortType<Weapon>(x => Calculator.Level(x), eSortType.Descending));
                                            else
                                                m_sortConditions_weapon.Add(new KeySelectorAndSortType<Weapon>(x => Calculator.Level(x), eSortType.Ascending));
                                        }
                                        break;
                                }

                                yield return null;
                            }
                        }
                        break;

                    case eEquipmentClassification.Armour:
                        {
                            m_sortConditions_armour.Clear();
                            for (int i = 0; i < m_dropdowns_sort.Length; i++)
                            {
                                eEquipmentSortMode sortMode = m_dropdowns_sort[i].options[m_dropdowns_sort[i].value].text.ToCorrespondingEnumValue<eEquipmentSortMode>();
                                switch (sortMode)
                                {
                                    default: // case eEquipmentSortMode.None
                                        break;
                                    case eEquipmentSortMode.Id:
                                        {
                                            if (m_toggles_sort[i].isOn)
                                                m_sortConditions_armour.Add(new KeySelectorAndSortType<Armour>(x => x.BaseInfo.Id, eSortType.Descending));
                                            else
                                                m_sortConditions_armour.Add(new KeySelectorAndSortType<Armour>(x => x.BaseInfo.Id, eSortType.Ascending));
                                        }
                                        break;
                                    case eEquipmentSortMode.Rarity:
                                        {
                                            if (m_toggles_sort[i].isOn)
                                                m_sortConditions_armour.Add(new KeySelectorAndSortType<Armour>(x => x.BaseInfo.Rarity, eSortType.Descending));
                                            else
                                                m_sortConditions_armour.Add(new KeySelectorAndSortType<Armour>(x => x.BaseInfo.Rarity, eSortType.Ascending));
                                        }
                                        break;
                                }

                                yield return null;
                            }
                        }
                        break;

                    case eEquipmentClassification.Accessory:
                        {
                            m_sortConditions_accessory.Clear();
                            for (int i = 0; i < m_dropdowns_sort.Length; i++)
                            {
                                eEquipmentSortMode sortMode = m_dropdowns_sort[i].options[m_dropdowns_sort[i].value].text.ToCorrespondingEnumValue<eEquipmentSortMode>();
                                switch (sortMode)
                                {
                                    default: // case eEquipmentSortMode.None
                                        break;
                                    case eEquipmentSortMode.Id:
                                        {
                                            if (m_toggles_sort[i].isOn)
                                                m_sortConditions_accessory.Add(new KeySelectorAndSortType<Accessory>(x => x.BaseInfo.Id, eSortType.Descending));
                                            else
                                                m_sortConditions_accessory.Add(new KeySelectorAndSortType<Accessory>(x => x.BaseInfo.Id, eSortType.Ascending));
                                        }
                                        break;
                                    case eEquipmentSortMode.Rarity:
                                        {
                                            if (m_toggles_sort[i].isOn)
                                                m_sortConditions_accessory.Add(new KeySelectorAndSortType<Accessory>(x => x.BaseInfo.Rarity, eSortType.Descending));
                                            else
                                                m_sortConditions_accessory.Add(new KeySelectorAndSortType<Accessory>(x => x.BaseInfo.Rarity, eSortType.Ascending));
                                        }
                                        break;
                                }

                                yield return null;
                            }
                        }
                        break;
                }
            }

            switch (m_equipmentClassification)
            {
                default: // case eEquipmentClassification.Weapon
                    m_weaponsToDisplay = m_weaponsToDisplay.OrderByMultipleConditions(m_sortConditions_weapon);
                    break;
                case eEquipmentClassification.Armour:
                    m_armoursToDisplay = m_armoursToDisplay.OrderByMultipleConditions(m_sortConditions_armour);
                    break;
                case eEquipmentClassification.Accessory:
                    m_accessoriesToDisplay = m_accessoriesToDisplay.OrderByMultipleConditions(m_sortConditions_accessory);
                    break;
            }

            ReorderEquipmentButtons();

            if (_closeSortModePanel)
            {
                CloseSortModePanel();
                m_text_applyFilterModeButton.text = "Apply";
            }
        }

        private void ReorderEquipmentButtons()
        {
            int index = 0;
            switch (m_equipmentClassification)
            {
                default: // case eEquipmentClassification.Weapon
                    {
                        foreach (Weapon weapon in m_weaponsToDisplay)
                        {
                            m_transforms_weaponButton[weapon].SetSiblingIndex(index);

                            index++;
                        }
                    }
                    break;
                case eEquipmentClassification.Armour:
                    {
                        foreach (Armour armour in m_armoursToDisplay)
                        {
                            m_transforms_armourButton[armour].SetSiblingIndex(index);

                            index++;
                        }
                    }
                    break;
                case eEquipmentClassification.Accessory:
                    {
                        foreach (Accessory accessory in m_accessoriesToDisplay)
                        {
                            m_transforms_accessoryButton[accessory].SetSiblingIndex(index);

                            index++;
                        }
                    }
                    break;
            }

            m_dynamicGridLayoutGroup.RefreshLayout(index);
        }

        public void Filter() { StartCoroutine(FilterEquipments()); }
        IEnumerator FilterEquipments()
        {
            m_text_applyFilterModeButton.text = "Applying...";
            yield return null;

            int minLevel = Mathf.RoundToInt(m_rangeSlider_level.LowerValue);
            int maxLevel = Mathf.RoundToInt(m_rangeSlider_level.HigherValue);

            List<eRarity> selectedRarities = new List<eRarity>();
            for (int i = 0; i < m_toggles_rarity.Length; i++)
            {
                int rarityValue = (i + 1) * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES;

                if (m_toggles_rarity[i].IsOn)
                    selectedRarities.Add((eRarity)rarityValue);
            }
            if (selectedRarities.Count == 0) // If no rarity is selected, treat it as if all were selected
                selectedRarities.AddRange(Enum.GetValues(typeof(eRarity)).OfType<eRarity>());

            switch (m_equipmentClassification)
            {
                default: // case eEquipmentClassification.Weapon
                    {
                        m_weaponsToDisplay = m_weapons.Where(x => selectedRarities.Contains(x.BaseInfo.Rarity))
                                                        .Where(x =>
                                                        {
                                                            int level = Calculator.Level(x);
                                                            return (x is LevelableWeapon || x is LevelableTransformableWeapon) ? (level >= minLevel && level <= maxLevel) : true;
                                                        }).ToList();

                        if (!m_toggles_weaponType[0].isOn) // If ordinary weapons should be hidden
                            m_weaponsToDisplay = m_weaponsToDisplay.Where(x => !(x is OrdinaryWeapon)).ToList();

                        if (!m_toggles_weaponType[1].isOn) // If levelable weapons should be hidden
                            m_weaponsToDisplay = m_weaponsToDisplay.Where(x => !(x is LevelableWeapon)).ToList();

                        if (!m_toggles_weaponType[2].isOn) // If transformable weapons should be hidden
                            m_weaponsToDisplay = m_weaponsToDisplay.Where(x => !(x is TransformableWeapon)).ToList();

                        if (!m_toggles_weaponType[1].isOn && !m_toggles_weaponType[2].isOn) // If levelable and transformable weapons should be hidden
                            m_weaponsToDisplay = m_weaponsToDisplay.Where(x => !(x is LevelableTransformableWeapon)).ToList();
                    }
                    break;
                case eEquipmentClassification.Armour:
                    m_armoursToDisplay = m_armours.Where(x => selectedRarities.Contains(x.BaseInfo.Rarity)).ToList();
                    break;
                case eEquipmentClassification.Accessory:
                    m_accessoriesToDisplay = m_accessories.Where(x => selectedRarities.Contains(x.BaseInfo.Rarity)).ToList();
                    break;
            }

            yield return StartCoroutine(SortEquipments(true));

            CloseFilterModePanel();
            m_text_applyFilterModeButton.text = "Apply";
        }

        public void RefreshFilterModePanel()
        {
            switch (m_equipmentClassification)
            {
                default: // case eEquipmentClassification.Weapon
                    {
                        m_transform_weaponType.gameObject.SetActive(true);

                        if (m_toggles_weaponType[1].isOn) // If levelable weapons is not selected
                            m_transform_level.gameObject.SetActive(true);
                        else
                            m_transform_level.gameObject.SetActive(false);
                    }
                    break;

                case eEquipmentClassification.Armour:
                case eEquipmentClassification.Accessory:
                    {
                        m_transform_weaponType.gameObject.SetActive(false);
                        m_transform_level.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        IEnumerator ChangeEquipment(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            string targetEquipmentType = string.Empty;
            eEquipmentSelectionMode equipmentSelectionMode = GameDataContainer.Instance.EquipmentSelectionMode;
            switch (equipmentSelectionMode)
            {
                case eEquipmentSelectionMode.UnitMainWeapon:
                case eEquipmentSelectionMode.UnitSubWeapon:
                case eEquipmentSelectionMode.UnitArmour:
                case eEquipmentSelectionMode.UnitAccessory:
                    targetEquipmentType = equipmentSelectionMode.ToString();
                    break;

                default:
                    yield break;
            }

            int targetEquipmentId = default;
            switch (m_equipmentClassification)
            {
                default: // case eEquipmentClassification.Weapon
                    targetEquipmentId = GameDataContainer.Instance.SelectedWeapon.UniqueId;
                    break;
                case eEquipmentClassification.Armour:
                    targetEquipmentId = GameDataContainer.Instance.SelectedArmour.UniqueId;
                    break;
                case eEquipmentClassification.Accessory:
                    targetEquipmentId = GameDataContainer.Instance.SelectedAccessory.UniqueId;
                    break;
            }

            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "ChangeEquipment"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
                    {"targetUnitId", GameDataContainer.Instance.SelectedUnit.UniqueId.ToString()},
                    {"targetEquipmentType", targetEquipmentType},
                    {"targetEquipmentId", targetEquipmentId.ToString()}
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
                else // Equipment Changed Successfully!
                {
                    // Apply the changes to the game
                    Unit targetUnit = GameDataContainer.Instance.SelectedUnit;
                    Player player = GameDataContainer.Instance.Player;

                    switch (equipmentSelectionMode)
                    {
                        case eEquipmentSelectionMode.UnitMainWeapon:
                            {
                                Weapon selectedWeapon = GameDataContainer.Instance.SelectedWeapon;
                                if (targetUnit.SubWeapon == selectedWeapon)
                                {
                                    Weapon tmp = targetUnit.MainWeapon;
                                    targetUnit.MainWeapon = selectedWeapon;
                                    targetUnit.SubWeapon = tmp;
                                }
                                else if (player.UnitsOwned.Any(x => x != targetUnit && (x.MainWeapon == selectedWeapon || x.SubWeapon == selectedWeapon)))
                                {
                                    Unit holderUnit = player.UnitsOwned.First(x => x.MainWeapon == selectedWeapon || x.SubWeapon == selectedWeapon);
                                    bool isHolderUnitsMainWeapon = (holderUnit.MainWeapon == selectedWeapon);

                                    Weapon tmp = targetUnit.MainWeapon;
                                    if (isHolderUnitsMainWeapon)
                                    {
                                        targetUnit.MainWeapon = holderUnit.MainWeapon;
                                        holderUnit.MainWeapon = (holderUnit.BaseInfo.EquipableWeaponClassifications.ContainsAny(tmp.BaseInfo.WeaponClassifications)) ? tmp : null;
                                    }
                                    else
                                    {
                                        targetUnit.MainWeapon = holderUnit.SubWeapon;
                                        holderUnit.SubWeapon = (holderUnit.BaseInfo.EquipableWeaponClassifications.ContainsAny(tmp.BaseInfo.WeaponClassifications)) ? tmp : null;
                                    }
                                }
                                else
                                    targetUnit.MainWeapon = selectedWeapon;
                            }
                            break;

                        case eEquipmentSelectionMode.UnitSubWeapon:
                            {
                                Weapon selectedWeapon = GameDataContainer.Instance.SelectedWeapon;
                                if (targetUnit.MainWeapon == selectedWeapon)
                                {
                                    Weapon tmp = targetUnit.SubWeapon;
                                    targetUnit.SubWeapon = selectedWeapon;
                                    targetUnit.MainWeapon = tmp;
                                }
                                else if (player.UnitsOwned.Any(x => x != targetUnit && (x.SubWeapon == selectedWeapon || x.MainWeapon == selectedWeapon)))
                                {
                                    Unit holderUnit = player.UnitsOwned.First(x => x.SubWeapon == selectedWeapon || x.MainWeapon == selectedWeapon);
                                    bool isHolderUnitsMainWeapon = (holderUnit.MainWeapon == selectedWeapon);

                                    Weapon tmp = targetUnit.SubWeapon;
                                    if (isHolderUnitsMainWeapon)
                                    {
                                        targetUnit.SubWeapon = holderUnit.MainWeapon;
                                        holderUnit.MainWeapon = (holderUnit.BaseInfo.EquipableWeaponClassifications.ContainsAny(tmp.BaseInfo.WeaponClassifications)) ? tmp : null;
                                    }
                                    else
                                    {
                                        targetUnit.SubWeapon = holderUnit.SubWeapon;
                                        holderUnit.SubWeapon = (holderUnit.BaseInfo.EquipableWeaponClassifications.ContainsAny(tmp.BaseInfo.WeaponClassifications)) ? tmp : null;
                                    }
                                }
                                else
                                    targetUnit.SubWeapon = selectedWeapon;
                            }
                            break;

                        case eEquipmentSelectionMode.UnitArmour:
                            {
                                Armour selectedArmour = GameDataContainer.Instance.SelectedArmour;
                                if (player.UnitsOwned.Any(x => x != targetUnit && x.Armour == selectedArmour))
                                {
                                    Unit holderUnit = player.UnitsOwned.First(x => x.Armour == selectedArmour);

                                    Armour tmp = targetUnit.Armour;
                                    targetUnit.Armour = holderUnit.Armour;
                                    holderUnit.Armour = (holderUnit.BaseInfo.EquipableArmourClassifications.Contains(tmp.BaseInfo.ArmourClassification)) ? tmp : null;
                                }
                                else
                                    targetUnit.Armour = selectedArmour;
                            }
                            break;

                        case eEquipmentSelectionMode.UnitAccessory:
                            {
                                Accessory selectedAccessory = GameDataContainer.Instance.SelectedAccessory;
                                if (player.UnitsOwned.Any(x => x != targetUnit && x.Accessory == selectedAccessory))
                                {
                                    Unit holderUnit = player.UnitsOwned.First(x => x.Accessory == selectedAccessory);

                                    Accessory tmp = targetUnit.Accessory;
                                    targetUnit.Accessory = holderUnit.Accessory;
                                    holderUnit.Accessory = (holderUnit.BaseInfo.EquipableAccessoryClassifications.Contains(tmp.BaseInfo.AccessoryClassification)) ? tmp : null; ;
                                }
                                else
                                    targetUnit.Accessory = selectedAccessory;
                            }
                            break;

                        default:
                            yield break;
                    }

                    _looperAndCoroutineLinker.SetTerminateLoopToTrue();
                }
            }
        }
    }

    public enum eEquipmentSelectionMode
    {
        List,
        UnitMainWeapon,
        UnitSubWeapon,
        UnitArmour,
        UnitAccessory,
        UpgradingWeapon,
        UpgradingArmour,
        UpgradingAccessory,
        EnhancingLevelableWeapon,
    }

    public enum eEquipmentSortMode
    {
        None,
        Id,
        Rarity,
        Level
    }

    public enum eEquipmentClassification
    {
        Weapon,
        Armour,
        Accessory
    }
}