using EEANWorks;
using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using EEANWorks.Games.Unity.Engine;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(InfoPanelManager_Unit))]
    public class OwnedUnitsDisplayer : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Transform m_contentsTransform;
        [SerializeField]
        private Transform m_selectionEndingPanelTransform;
        [SerializeField]
        private Transform m_sortModePanelTransform;
        [SerializeField]
        private Transform m_filterModePanelTransform;
        #endregion

        #region Private Fields
        private const int MAX_NUM_OF_UNITS_TO_SELECT = 20;

        private GameObject m_unitButtonPrefab;

        private Dropdown[] m_dropdowns_sort;
        private Toggle[] m_toggles_sort;

        private RangeSlider m_rangeSlider_level;
        private ImageToggle[] m_toggles_rarity;
        private ImageToggle[] m_toggles_element;
        private Toggle m_toggle_showUnitsWithNoElement;

        private Text m_text_applySortModeButton;
        private Text m_text_applyFilterModeButton;

        private Text m_text_selectionCount;
        private Button m_button_selectionEnding;

        private List<Unit> m_units;

        private List<Unit> m_unitsToDisplay;
        private List<KeySelectorAndSortType<Unit>> m_sortConditions;

        private InfoPanelManager_Unit m_infoPanelManager_unit;
        private Dictionary<Unit, Transform> m_transforms_unitButton;
        private List<Image> m_images_rarityTexture;
        private List<AdvancedButton> m_buttons_unit;

        private DynamicGridLayoutGroup m_dynamicGridLayoutGroup;

        private List<Unit> m_selectedUnits;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_unitButtonPrefab = SharedAssetContainer.Instance.ObjectButtonPrefab;

            m_sortConditions = new List<KeySelectorAndSortType<Unit>>();

            m_selectedUnits = new List<Unit>();

            m_transforms_unitButton = new Dictionary<Unit, Transform>();
            m_images_rarityTexture = new List<Image>();
            m_buttons_unit = new List<AdvancedButton>();

            m_infoPanelManager_unit = this.GetComponent<InfoPanelManager_Unit>();

            // Initialize values for sort mode panel
            Transform transform_priorities = m_sortModePanelTransform.Find("Priorities");
            m_dropdowns_sort = new Dropdown[transform_priorities.childCount];
            m_toggles_sort = new Toggle[transform_priorities.childCount];
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (var unitSortMode in Enum.GetNames(typeof(eUnitSortMode)))
            {
                options.Add(new Dropdown.OptionData(unitSortMode));
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
                    m_dropdowns_sort[i].value = (int)(eUnitSortMode.Element) - 1; // Subtracting 1 because the first option has been removed
                else if (i == 1)
                {
                    m_dropdowns_sort[i].value = (int)(eUnitSortMode.Rarity);
                    m_toggles_sort[i].isOn = true;
                }
                else if (i == 2)
                {
                    m_dropdowns_sort[i].value = (int)(eUnitSortMode.Id);
                    m_toggles_sort[i].isOn = false;
                }

                UpdateSortToggle(m_dropdowns_sort[i], m_toggles_sort[i]);
            }
            m_text_applySortModeButton = m_sortModePanelTransform.Find("Button@Apply").Find("Text").GetComponent<Text>();
            m_sortModePanelTransform.gameObject.SetActive(false);

            // Initialize values for filter mode panel
            m_rangeSlider_level = m_filterModePanelTransform.Find("Level").Find("RangeSlider").GetComponent<RangeSlider>();
            m_toggles_rarity = new ImageToggle[Enum.GetNames(typeof(eRarity)).Length];
            Transform transform_rarityToggles = m_filterModePanelTransform.Find("Rarity").Find("Toggles");
            for (int i = 0; i < m_toggles_rarity.Length; i++)
            {
                int rarityValue = (i + 1) * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES;
                m_toggles_rarity[i] = transform_rarityToggles.Find("ImageToggle@" + ((eRarity)rarityValue).ToString()).GetComponent<ImageToggle>();
            }
            Transform transform_element = m_filterModePanelTransform.Find("Element");
            Transform transform_elementToggles = transform_element.Find("Toggles");
            m_toggles_element = new ImageToggle[Enum.GetNames(typeof(eElement)).Length - 1]; // Exclude eElement.None
            for (int i = 0; i < m_toggles_element.Length; i++)
            {
                m_toggles_element[i] = transform_elementToggles.Find("ImageToggle@" + ((eElement)(i + 1)).ToString()).GetComponent<ImageToggle>();
            }
            m_toggle_showUnitsWithNoElement = transform_element.Find("Toggle@ShowUnitsWithNoElement").GetComponent<Toggle>();
            m_text_applyFilterModeButton = m_filterModePanelTransform.Find("Button@Apply").Find("Text").GetComponent<Text>();
            m_filterModePanelTransform.gameObject.SetActive(false);

            // Initialize values for selection ending panel (used when required multiple selection)
            m_text_selectionCount = m_selectionEndingPanelTransform.Find("Text@SelectionCount").GetComponent<Text>();
            m_button_selectionEnding = m_selectionEndingPanelTransform.Find("Button@SelectionEnding").GetComponent<Button>();

            // Set units to display
            m_dynamicGridLayoutGroup = m_contentsTransform.GetComponent<DynamicGridLayoutGroup>();
            m_dynamicGridLayoutGroup.RefreshLayoutOnTransformChildrenChanged = false; // Required so that the layout is not refreshed every time that a unit button is instantiated in a loop

            m_units = new List<Unit>();

            switch (GameDataContainer.Instance.UnitSelectionMode)
            {
                default: // case eUnitSelectionMethod.List
                    {
                        m_units = GameDataContainer.Instance.Player.UnitsOwned;
                        m_selectionEndingPanelTransform.gameObject.SetActive(false);
                    }
                    break;

                case eUnitSelectionMode.Member:
                    {
                        if (GameDataContainer.Instance.SelectedUnit == null) // If true, the selected member slot is not the first one
                            m_units = GameDataContainer.Instance.Player.UnitsOwned.Except(GameDataContainer.Instance.SelectedTeam.Members[0]); // Do not include the first member in the selected team
                        else
                            m_units = GameDataContainer.Instance.Player.UnitsOwned.Except(GameDataContainer.Instance.SelectedUnit);
                        m_selectionEndingPanelTransform.gameObject.SetActive(false);
                    }
                    break;

                case eUnitSelectionMode.EnhancingUnit:
                    {
                        m_units = GameDataContainer.Instance.Player.UnitsOwned.Where(x => x != GameDataContainer.Instance.SelectedUnit 
                                                                                        && !Calculator.IsMaxLevel(x)).ToList();
                        m_selectionEndingPanelTransform.gameObject.SetActive(false);
                    }
                    break;

                case eUnitSelectionMode.SkillEnhancingUnit:
                    {
                        m_units = GameDataContainer.Instance.Player.UnitsOwned.Where(x => x.Skills.Any(y => y.Level < CoreValues.MAX_SKILL_LEVEL)).ToList();
                        m_selectionEndingPanelTransform.gameObject.SetActive(false);
                    }
                    break;

                case eUnitSelectionMode.SkillEnhancementMaterials:
                    {
                        m_units = GameDataContainer.Instance.SelectedSkill != null ?
                                                                GameDataContainer.Instance.Player.UnitsOwned.Where(x => !x.IsLocked
                                                                                    && x.Skills.Contains(GameDataContainer.Instance.SelectedSkill)).ToList() :
                                                                GameDataContainer.Instance.Player.UnitsOwned.Where(x => !x.IsLocked
                                                                                    && x.Skills.ContainsAny(GameDataContainer.Instance.SelectedUnit.Skills)).ToList();
                        m_selectionEndingPanelTransform.gameObject.SetActive(true);
                        m_button_selectionEnding.onClick.AddListener(() => SelectMultiple());
                    }
                    break;

                case eUnitSelectionMode.SkillInheritor:
                    {
                        m_units = GameDataContainer.Instance.Player.UnitsOwned.Where(x => x.BaseInfo.Rarity == eRarity.Legendary
                                                                && Calculator.IsMaxLevel(x)
                                                                && x.Skills.Any(y => y.Level == CoreValues.MAX_SKILL_LEVEL)
                                                                && !GameDataContainer.Instance.Player.Teams.Any(y => y.Members.Contains(x))).ToList();
                        m_selectionEndingPanelTransform.gameObject.SetActive(false);
                    }
                    break;

                case eUnitSelectionMode.SkillInheritee:
                    {
                        m_units = GameDataContainer.Instance.Player.UnitsOwned.Where(x => x.SkillInheritor == null
                                                                                && !x.Skills.Contains(GameDataContainer.Instance.SelectedSkill)).ToList();
                        m_selectionEndingPanelTransform.gameObject.SetActive(false);
                    }
                    break;
            }

            m_unitsToDisplay = new List<Unit>(m_units);

            InstantiateUnits();
            Sort();
        }

        private void InstantiateUnits()
        {
            m_transforms_unitButton.Clear();
            m_images_rarityTexture.Clear();
            m_buttons_unit.Clear();
            m_contentsTransform.ClearChildren();

            foreach (Unit unit in m_unitsToDisplay)
            {
                GameObject go_unitButton = Instantiate(m_unitButtonPrefab, m_contentsTransform);
                m_transforms_unitButton.Add(unit, go_unitButton.GetComponent<RectTransform>());

                GameObjectFormatter_ObjectButton goFormatter_objectButton = go_unitButton.GetComponent<GameObjectFormatter_ObjectButton>();
                goFormatter_objectButton.Format(unit);

                Text text_value = goFormatter_objectButton.Text_Value;
                eUnitSortMode primarySortMode = m_dropdowns_sort[0].options[m_dropdowns_sort[0].value].text.ToCorrespondingEnumValue<eUnitSortMode>();
                switch (primarySortMode)
                {
                    case eUnitSortMode.Level:
                        text_value.text = Calculator.Level(unit).ToString();
                        break;
                    case eUnitSortMode.MaxHP:
                        text_value.text = Calculator.MaxHP(unit).ToString();
                        break;
                    case eUnitSortMode.Phy_Str:
                        text_value.text = Calculator.PhysicalStrength(unit).ToString();
                        break;
                    case eUnitSortMode.Phy_Res:
                        text_value.text = Calculator.PhysicalResistance(unit).ToString();
                        break;
                    case eUnitSortMode.Mag_Str:
                        text_value.text = Calculator.MagicalStrength(unit).ToString();
                        break;
                    case eUnitSortMode.Mag_Res:
                        text_value.text = Calculator.MagicalResistance(unit).ToString();
                        break;
                    case eUnitSortMode.Vitality:
                        text_value.text = Calculator.Vitality(unit).ToString();
                        break;
                    default:
                        text_value.text = "";
                        break;
                }

                m_images_rarityTexture.Add(goFormatter_objectButton.Image_RarityTexture);
                m_buttons_unit.Add(goFormatter_objectButton.Button_Object);
            }

            switch (GameDataContainer.Instance.UnitSelectionMode)
            {
                default: // case eUnitSelectionMethod.List
                    {
                        for (int i = 0; i < m_unitsToDisplay.Count; i++)
                        {
                            Unit unit = m_unitsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                            m_buttons_unit[i].OnClick.AddListener(() => m_infoPanelManager_unit.InstantiateInfoPanel(unit));
                            m_buttons_unit[i].interactable = true;
                        }
                    }
                    break;

                case eUnitSelectionMode.Member:
                case eUnitSelectionMode.EvolvingUnit:
                case eUnitSelectionMode.EnhancingUnit:
                case eUnitSelectionMode.SkillEnhancingUnit:
                case eUnitSelectionMode.SkillInheritor:
                    {
                        for (int i = 0; i < m_unitsToDisplay.Count; i++)
                        {
                            Unit unit = m_unitsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                            m_buttons_unit[i].OnClick.AddListener(() => Select(unit));
                            m_buttons_unit[i].EnableLongPressAsDefault();
                            m_buttons_unit[i].OnLongPress.AddListener(() => m_infoPanelManager_unit.InstantiateInfoPanel(unit, true));
                            m_buttons_unit[i].interactable = true;
                        }
                    }
                    break;

                case eUnitSelectionMode.SkillInheritee:
                    {
                        for (int i = 0; i < m_unitsToDisplay.Count; i++)
                        {
                            Unit unit = m_unitsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                            m_buttons_unit[i].OnClick.AddListener(() => Select(unit, true));
                            m_buttons_unit[i].EnableLongPressAsDefault();
                            m_buttons_unit[i].OnLongPress.AddListener(() => m_infoPanelManager_unit.InstantiateInfoPanel(unit, true));
                            m_buttons_unit[i].interactable = true;
                        }
                    }
                    break;

                case eUnitSelectionMode.SkillEnhancementMaterials:
                    {
                        for (int i = 0; i < m_unitsToDisplay.Count; i++)
                        {
                            Unit unit = m_unitsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                            Image image_rarityTexture = m_images_rarityTexture[i];
                            m_buttons_unit[i].OnClick.AddListener(() => ToggleSelection(unit, image_rarityTexture));
                            m_buttons_unit[i].EnableLongPressAsDefault();
                            m_buttons_unit[i].OnLongPress.AddListener(() => m_infoPanelManager_unit.InstantiateInfoPanel(unit, true));
                            m_buttons_unit[i].interactable = true;
                        }
                    }
                    break;
            }
        }

        private void UpdateSortToggle(Dropdown _dropdown, Toggle _toggle)
        {
            eUnitSortMode sortMode = _dropdown.options[_dropdown.value].text.ToCorrespondingEnumValue<eUnitSortMode>();
            switch (sortMode)
            {
                case eUnitSortMode.None:
                case eUnitSortMode.Element:
                    _toggle.gameObject.SetActive(false);
                    break;
                default:
                    _toggle.gameObject.SetActive(true);
                    break;
            }
        }

        private void Select(Unit _unit, bool _isUnit2 = false)
        {
            if (!_isUnit2)
                GameDataContainer.Instance.SelectedUnit = _unit;
            else
                GameDataContainer.Instance.SelectedUnit2 = _unit;

            SceneConnector.GoToPreviousScene();
        }
        private void SelectMultiple()
        {
            GameDataContainer.Instance.SelectedUnits = m_selectedUnits;

            SceneConnector.GoToPreviousScene();
        }

        private void ToggleSelection(Unit _unit, Image _image_rarityTexture)
        {
            if (!m_selectedUnits.Contains(_unit))
            {
                if (m_selectedUnits.Count >= MAX_NUM_OF_UNITS_TO_SELECT)
                    return;

                m_selectedUnits.Add(_unit);
                _image_rarityTexture.color = Color.red;
            }
            else
            {
                m_selectedUnits.Remove(_unit);
                _image_rarityTexture.color = Color.white;
            }

            m_text_selectionCount.text = m_selectedUnits.Count.ToString() + "/" + MAX_NUM_OF_UNITS_TO_SELECT.ToString();
        }

        public void OpenSortModePanel() { m_sortModePanelTransform.gameObject.SetActive(true); }

        public void OpenFilterModePanel() { m_filterModePanelTransform.gameObject.SetActive(true); }

        public void CloseSortModePanel() { m_sortModePanelTransform.gameObject.SetActive(false); }

        public void CloseFilterModePanel() { m_filterModePanelTransform.gameObject.SetActive(false); }

        public void Sort()
        {
            StartCoroutine(SortUnits(false, true));
        }
        IEnumerator SortUnits(bool _usePreviousConditions = false, bool _closeSortModePanel = false)
        {
            if (_closeSortModePanel)
            {
                m_text_applyFilterModeButton.text = "Applying...";
                yield return null;
            }

            if (!_usePreviousConditions) // Set all the conditions specified through the UI
            {
                m_sortConditions.Clear();
                for (int i = 0; i < m_dropdowns_sort.Length; i++)
                {
                    eUnitSortMode sortMode = m_dropdowns_sort[i].options[m_dropdowns_sort[i].value].text.ToCorrespondingEnumValue<eUnitSortMode>();
                    switch (sortMode)
                    {
                        default: // case eUnitSortMode.None
                            break;
                        case eUnitSortMode.Id:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.BaseInfo.Id, eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.BaseInfo.Id, eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Rarity:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.BaseInfo.Rarity, eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.BaseInfo.Rarity, eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Element:
                            {
                                m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.BaseInfo.Elements[0], eSortType.Ascending));
                                m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.BaseInfo.Elements[1], eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Date:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.UniqueId, eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => x.UniqueId, eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Level:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.Level(x), eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.Level(x), eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.MaxHP:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.MaxHP(x), eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.MaxHP(x), eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Phy_Str:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.PhysicalStrength(x), eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.PhysicalStrength(x), eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Phy_Res:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.PhysicalResistance(x), eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.PhysicalResistance(x), eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Mag_Str:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.MagicalStrength(x), eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.MagicalStrength(x), eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Mag_Res:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.MagicalResistance(x), eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.MagicalResistance(x), eSortType.Ascending));
                            }
                            break;
                        case eUnitSortMode.Vitality:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.Vitality(x), eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Unit>(x => Calculator.Vitality(x), eSortType.Ascending));
                            }
                            break;
                    }

                    yield return null;
                }
            }

            m_unitsToDisplay = m_unitsToDisplay.OrderByMultipleConditions(m_sortConditions);

            ReorderUnitButtons();

            if (_closeSortModePanel)
            {
                CloseSortModePanel();
                m_text_applyFilterModeButton.text = "Apply";
            }
        }

        private void ReorderUnitButtons()
        {
            int index = 0;
            foreach (Unit unit in m_unitsToDisplay)
            {
                m_transforms_unitButton[unit].SetSiblingIndex(index);

                index++;
            }

            m_dynamicGridLayoutGroup.RefreshLayout(index);
        }

        public void Filter() { StartCoroutine(FilterUnits()); }
        IEnumerator FilterUnits()
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

            List<eElement> selectedElements = new List<eElement>();
            for (int i = 0; i < m_toggles_element.Length; i++)
            {
                if (m_toggles_element[i].IsOn)
                    selectedElements.Add((eElement)(i + 1));
            }
            if (selectedElements.Count == 0 && !m_toggle_showUnitsWithNoElement.isOn) // If no element is selected and units without element are set not to be shown, treat it as if all elements were selected
            {
                selectedElements.AddRange(Enum.GetValues(typeof(eElement)).OfType<eElement>());
                selectedElements.Remove(eElement.None);
            }

            m_unitsToDisplay = m_units.Where(x => selectedRarities.Contains(x.BaseInfo.Rarity))
                                        .Where(x => (selectedElements.ContainsAny(x.BaseInfo.Elements) || ((m_toggle_showUnitsWithNoElement.isOn ? x.BaseInfo.Elements.ContainsOnly(eElement.None) : false))))
                                        .Where(x => { int level = Calculator.Level(x); return level >= minLevel && level <= maxLevel; }).ToList();

            yield return StartCoroutine(SortUnits(true));

            CloseFilterModePanel();
            m_text_applyFilterModeButton.text = "Apply";
        }
    }

    public enum eUnitSelectionMode
    {
        List,
        Member,
        EvolvingUnit,
        EnhancingUnit,
        SkillEnhancingUnit,
        SkillEnhancementMaterials,
        SkillInheritor,
        SkillInheritee
    }

    public enum eUnitSortMode
    {
        None,
        Id,
        Rarity,
        Element,
        Date,
        Level,
        MaxHP,
        Phy_Str,
        Phy_Res,
        Mag_Str,
        Mag_Res,
        Vitality
    }
}