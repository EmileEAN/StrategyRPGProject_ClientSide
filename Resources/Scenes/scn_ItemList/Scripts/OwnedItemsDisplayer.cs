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
    [RequireComponent(typeof(InfoPanelManager_Item))]
    public class OwnedItemsDisplayer : MonoBehaviour
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
        private const int MAX_NUM_OF_ITEMS_TO_SELECT = 20;

        private GameObject m_itemButtonPrefab;

        private Dropdown[] m_dropdowns_sort;
        private Toggle[] m_toggles_sort;

        private RangeSlider m_rangeSlider_price;
        private ImageToggle[] m_toggles_rarity;

        private Text m_text_applySortModeButton;
        private Text m_text_applyFilterModeButton;

        private Text m_text_selectionCount;
        private Button m_button_selectionEnding;

        private Button m_button_skillItem;
        private Button m_button_skillMaterial;
        private Button m_button_itemMaterial;
        private Button m_button_equipmentMaterial;
        private Button m_button_evolutionMaterial;
        private Button m_button_weaponEnhancementMaterial;
        private Button m_button_unitEnhancementMaterial;
        private Button m_button_skillEnhancementMaterial;
        private Button m_button_gachaCostItem;
        private Button m_button_equipmentTradingItem;
        private Button m_button_unitTradingItem;

        private Dictionary<Item, int> m_items;

        private eItemClassification m_itemClassification;
        private List<Item> m_itemsToDisplay;
        private List<KeySelectorAndSortType<Item>> m_sortConditions;

        private InfoPanelManager_Item m_infoPanelManager_item;
        private Dictionary<Item, Transform> m_transforms_itemButton;
        private List<AdvancedButton> m_buttons_item;
        private List<Image> m_images_rarityTexture;
        private List<Text> m_texts_value;
        private List<Button> m_buttons_plus;
        private List<Button> m_buttons_minus;

        private DynamicGridLayoutGroup m_dynamicGridLayoutGroup;

        private Dictionary<Item, int> m_selectedItems;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_itemButtonPrefab = SharedAssetContainer.Instance.ItemButtonPrefab;

            m_sortConditions = new List<KeySelectorAndSortType<Item>>();

            m_selectedItems = new Dictionary<Item, int>();

            m_transforms_itemButton = new Dictionary<Item, Transform>();
            m_buttons_item = new List<AdvancedButton>();
            m_images_rarityTexture = new List<Image>();
            m_texts_value = new List<Text>();
            m_buttons_plus = new List<Button>();
            m_buttons_minus = new List<Button>();

            m_infoPanelManager_item = this.GetComponent<InfoPanelManager_Item>();

            // Initialize values for sort mode panel
            Transform transform_priorities = m_sortModePanelTransform.Find("Priorities");
            m_dropdowns_sort = new Dropdown[transform_priorities.childCount];
            m_toggles_sort = new Toggle[transform_priorities.childCount];
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (var itemSortMode in Enum.GetNames(typeof(eItemSortMode)))
            {
                options.Add(new Dropdown.OptionData(itemSortMode));
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
                    m_dropdowns_sort[i].value = (int)(eItemSortMode.Rarity) - 1; // Subtracting 1 because the first option has been removed
                    m_toggles_sort[i].isOn = true;
                }
                else if (i == 1)
                {
                    m_dropdowns_sort[i].value = (int)(eItemSortMode.Id);
                    m_toggles_sort[i].isOn = false;
                }

                UpdateSortToggle(m_dropdowns_sort[i], m_toggles_sort[i]);
            }
            m_text_applySortModeButton = m_sortModePanelTransform.Find("Button@Apply").Find("Text").GetComponent<Text>();
            m_sortModePanelTransform.gameObject.SetActive(false);

            // Initialize values for filter mode panel
            m_rangeSlider_price = m_filterModePanelTransform.Find("SellingPrice").Find("RangeSlider").GetComponent<RangeSlider>();
            m_toggles_rarity = new ImageToggle[Enum.GetNames(typeof(eRarity)).Length];
            Transform transform_rarityToggles = m_filterModePanelTransform.Find("Rarity").Find("Toggles");
            for (int i = 0; i < m_toggles_rarity.Length; i++)
            {
                int rarityValue = (i + 1) * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES;
                m_toggles_rarity[i] = transform_rarityToggles.Find("ImageToggle@" + ((eRarity)rarityValue).ToString()).GetComponent<ImageToggle>();
            }
            m_text_applyFilterModeButton = m_filterModePanelTransform.Find("Button@Apply").Find("Text").GetComponent<Text>();
            m_filterModePanelTransform.gameObject.SetActive(false);

            // Initialize values for selection ending panel (used when required multiple selection)
            m_text_selectionCount = m_selectionEndingPanelTransform.Find("Text@SelectionCount").GetComponent<Text>();
            m_button_selectionEnding = m_selectionEndingPanelTransform.Find("Button@SelectionEnding").GetComponent<Button>();

            // Initialize values for side bar panel (used in list mode)
            m_button_skillItem = m_sideBarTransform.Find("Button@SkillItem").GetComponent<Button>();
            m_button_skillMaterial = m_sideBarTransform.Find("Button@SkillMaterial").GetComponent<Button>();
            m_button_itemMaterial = m_sideBarTransform.Find("Button@ItemMaterial").GetComponent<Button>();
            m_button_equipmentMaterial = m_sideBarTransform.Find("Button@EquipmentMaterial").GetComponent<Button>();
            m_button_evolutionMaterial = m_sideBarTransform.Find("Button@EvolutionMaterial").GetComponent<Button>();
            m_button_weaponEnhancementMaterial = m_sideBarTransform.Find("Button@WeaponEnhancementMaterial").GetComponent<Button>();
            m_button_unitEnhancementMaterial = m_sideBarTransform.Find("Button@UnitEnhancementMaterial").GetComponent<Button>();
            m_button_skillEnhancementMaterial = m_sideBarTransform.Find("Button@SkillEnhancementMaterial").GetComponent<Button>();
            m_button_gachaCostItem = m_sideBarTransform.Find("Button@GachaCostItem").GetComponent<Button>();
            m_button_equipmentTradingItem = m_sideBarTransform.Find("Button@EquipmentTradingItem").GetComponent<Button>();
            m_button_unitTradingItem = m_sideBarTransform.Find("Button@UnitTradingItem").GetComponent<Button>();

            // Set items to display
            m_dynamicGridLayoutGroup = m_contentsTransform.GetComponent<DynamicGridLayoutGroup>();
            m_dynamicGridLayoutGroup.RefreshLayoutOnTransformChildrenChanged = false; // Required so that the layout is not refreshed every time that a item button is instantiated in a loop

            m_items = new Dictionary<Item, int>();

            switch (GameDataContainer.Instance.ItemSelectionMode)
            {
                default: // case eItemSelectionMode.List
                    {
                        m_items = GameDataContainer.Instance.Player.ItemsOwned;

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(true);

                        m_button_skillItem.onClick.AddListener(delegate { m_itemClassification = eItemClassification.SkillItem; Filter(); SetInteractableForAllSideBarButtons(true); m_button_skillItem.interactable = false; });
                        m_button_skillMaterial.onClick.AddListener(delegate { m_itemClassification = eItemClassification.SkillMaterial; Filter(); SetInteractableForAllSideBarButtons(true); m_button_skillMaterial.interactable = false; });
                        m_button_itemMaterial.onClick.AddListener(delegate { m_itemClassification = eItemClassification.ItemMaterial; Filter(); SetInteractableForAllSideBarButtons(true); m_button_itemMaterial.interactable = false; });
                        m_button_equipmentMaterial.onClick.AddListener(delegate { m_itemClassification = eItemClassification.EquipmentMaterial; Filter(); SetInteractableForAllSideBarButtons(true); m_button_equipmentMaterial.interactable = false; });
                        m_button_evolutionMaterial.onClick.AddListener(delegate { m_itemClassification = eItemClassification.EvolutionMaterial; Filter(); SetInteractableForAllSideBarButtons(true); m_button_evolutionMaterial.interactable = false; });
                        m_button_weaponEnhancementMaterial.onClick.AddListener(delegate { m_itemClassification = eItemClassification.WeaponEnhancementMaterial; Filter(); SetInteractableForAllSideBarButtons(true); m_button_weaponEnhancementMaterial.interactable = false; });
                        m_button_unitEnhancementMaterial.onClick.AddListener(delegate { m_itemClassification = eItemClassification.UnitEnhancementMaterial; Filter(); SetInteractableForAllSideBarButtons(true); m_button_unitEnhancementMaterial.interactable = false; });
                        m_button_skillEnhancementMaterial.onClick.AddListener(delegate { m_itemClassification = eItemClassification.SkillEnhancementMaterial; Filter(); SetInteractableForAllSideBarButtons(true); m_button_skillEnhancementMaterial.interactable = false; });
                        m_button_gachaCostItem.onClick.AddListener(delegate { m_itemClassification = eItemClassification.GachaCostItem; Filter(); SetInteractableForAllSideBarButtons(true); m_button_gachaCostItem.interactable = false; });
                        m_button_equipmentTradingItem.onClick.AddListener(delegate { m_itemClassification = eItemClassification.EquipmentTradingItem; Filter(); SetInteractableForAllSideBarButtons(true); m_button_equipmentTradingItem.interactable = false; });
                        m_button_unitTradingItem.onClick.AddListener(delegate { m_itemClassification = eItemClassification.UnitTradingItem; Filter(); SetInteractableForAllSideBarButtons(true); m_button_unitTradingItem.interactable = false; });

                        m_button_skillItem.interactable = false;
                        m_itemClassification = eItemClassification.SkillItem;
                    }
                    break;

                case eItemSelectionMode.UnitEnhancementMaterials:
                    {
                        m_items = GameDataContainer.Instance.Player.ItemsOwned.Where(x => x.Key is UnitEnhancementMaterial).ToDictionary(x => x.Key, x => x.Value);
                        m_text_selectionCount.gameObject.SetActive(true);
                        m_button_selectionEnding.gameObject.SetActive(true);
                        m_button_selectionEnding.onClick.AddListener(() => SelectMultiple());

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;

                case eItemSelectionMode.ItemSet:
                    {
                        m_items = GameDataContainer.Instance.Player.ItemsOwned.Where(x => x.Key is BattleItem).ToDictionary(x => x.Key, x => x.Value);

                        m_selectionEndingPanelTransform.gameObject.SetActive(false);

                        SetInteractableForAllSideBarButtons(false);
                    }
                    break;
            }

            m_itemsToDisplay = m_items.Keys.ToList();

            InstantiateItems();
            Sort();
        }

        private void InstantiateItems()
        {
            m_transforms_itemButton.Clear();
            m_buttons_item.Clear();
            m_images_rarityTexture.Clear();
            m_texts_value.Clear();
            m_buttons_plus.Clear();
            m_buttons_minus.Clear();
            m_contentsTransform.ClearChildren();

            foreach (var quantityPerItem in m_items)
            {
                Item item = quantityPerItem.Key;

                GameObject go_itemButton = Instantiate(m_itemButtonPrefab, m_contentsTransform);
                m_transforms_itemButton.Add(item, go_itemButton.GetComponent<RectTransform>());

                GameObjectFormatter_ItemButton goFormatter_itemButton = go_itemButton.GetComponent<GameObjectFormatter_ItemButton>();
                goFormatter_itemButton.Format(item);

                Text text_value = goFormatter_itemButton.Text_Value;
                eItemSortMode primarySortMode = m_dropdowns_sort[0].options[m_dropdowns_sort[0].value].text.ToCorrespondingEnumValue<eItemSortMode>();
                switch (primarySortMode)
                {
                    case eItemSortMode.Price:
                        text_value.text = item.SellingPrice.ToString();
                        break;
                    default:
                        text_value.text = "";
                        break;
                }

                m_buttons_item.Add(goFormatter_itemButton.Button_Object);
                m_images_rarityTexture.Add(goFormatter_itemButton.Image_RarityTexture);
                m_texts_value.Add(text_value);
                m_buttons_plus.Add(goFormatter_itemButton.Button_Plus);
                m_buttons_minus.Add(goFormatter_itemButton.Button_Minus);
            }

            switch (GameDataContainer.Instance.ItemSelectionMode)
            {
                default: // case eItemSelectionMethod.List
                    {
                        for (int i = 0; i < m_itemsToDisplay.Count; i++)
                        {
                            Item item = m_itemsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                            m_buttons_item[i].OnClick.AddListener(() => m_infoPanelManager_item.InstantiateInfoPanel(item));

                            m_buttons_plus[i].gameObject.SetActive(false);
                            m_buttons_minus[i].gameObject.SetActive(false);
                        }
                    }
                    break;

                case eItemSelectionMode.UnitEnhancementMaterials:
                    {
                        for (int i = 0; i < m_itemsToDisplay.Count; i++)
                        {
                            Item item = m_itemsToDisplay[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                            Image image_rarityTexture = m_images_rarityTexture[i]; // Used to prevent the UnityAction from throwing IndexOutOfRangeException
                            Button button_plus = m_buttons_plus[i];
                            Text text_value = m_texts_value[i];
                            Button button_minus = m_buttons_minus[i];
                            m_buttons_item[i].OnClick.AddListener(() => m_infoPanelManager_item.InstantiateInfoPanel(item));
                            m_buttons_item[i].interactable = true;

                            m_buttons_plus[i].onClick.AddListener(() => AddOneToSelection(item, image_rarityTexture, button_plus, text_value, button_minus));
                            m_buttons_plus[i].interactable = true;

                            m_buttons_minus[i].onClick.AddListener(() => RemoveOneFromSelection(item, image_rarityTexture, button_minus, text_value, button_plus));
                            m_buttons_minus[i].interactable = true;
                        }
                    }
                    break;

                    //case eItemSelectionMode.ItemSet:
                    //    break;
            }
        }

        private void SetInteractableForAllSideBarButtons(bool _interactable)
        {
            m_button_skillItem.interactable = _interactable;
            m_button_skillMaterial.interactable = _interactable;
            m_button_itemMaterial.interactable = _interactable;
            m_button_equipmentMaterial.interactable = _interactable;
            m_button_evolutionMaterial.interactable = _interactable;
            m_button_weaponEnhancementMaterial.interactable = _interactable;
            m_button_unitEnhancementMaterial.interactable = _interactable;
            m_button_skillEnhancementMaterial.interactable = _interactable;
            m_button_gachaCostItem.interactable = _interactable;
            m_button_equipmentTradingItem.interactable = _interactable;
            m_button_unitTradingItem.interactable = _interactable;
        }

        private void UpdateSortToggle(Dropdown _dropdown, Toggle _toggle)
        {
            eItemSortMode sortMode = _dropdown.options[_dropdown.value].text.ToCorrespondingEnumValue<eItemSortMode>();
            switch (sortMode)
            {
                case eItemSortMode.None:
                    _toggle.gameObject.SetActive(false);
                    break;
                default:
                    _toggle.gameObject.SetActive(true);
                    break;
            }
        }

        private void Select(Item _item)
        {
            GameDataContainer.Instance.SelectedItem = _item;

            SceneConnector.GoToPreviousScene();
        }
        private void SelectMultiple()
        {
            GameDataContainer.Instance.SelectedItems = m_selectedItems;

            SceneConnector.GoToPreviousScene();
        }

        private void AddOneToSelection(Item _item, Image _image_rarityTexture, Button _plusButton, Text _valueText, Button _minusButton)
        {
            int quantitySelected;

            if (!m_selectedItems.ContainsKey(_item))
            {
                m_selectedItems.Add(_item, 1);
                quantitySelected = 1;
            }
            else
            {
                int quantityOwned = m_items[_item];
                quantitySelected = m_selectedItems[_item];

                if (quantitySelected < quantityOwned) // There are more items owned than the amount selected
                {
                    m_selectedItems[_item]++;
                    quantitySelected++;
                }

                if (quantitySelected == quantityOwned)
                    _plusButton.interactable = false;
            }

            _image_rarityTexture.color = Color.red;
            _valueText.text = quantitySelected.ToString();
            _minusButton.interactable = true;

            m_text_selectionCount.text = m_selectedItems.Count.ToString() + "/" + MAX_NUM_OF_ITEMS_TO_SELECT.ToString();
        }

        private void RemoveOneFromSelection(Item _item, Image _image_rarityTexture, Button _minusButton, Text _valueText, Button _plusButton)
        {
            if (m_selectedItems.ContainsKey(_item))
            {
                int quantitySelected = m_selectedItems[_item];

                if (quantitySelected > 0)
                {
                    m_selectedItems[_item]--;
                    quantitySelected--;
                }

                if (quantitySelected == 0)
                {
                    _minusButton.interactable = false;
                    _image_rarityTexture.color = Color.white;
                    _valueText.text = "";
                }
                else
                    _valueText.text = quantitySelected.ToString();

                _plusButton.interactable = true;

                m_text_selectionCount.text = m_selectedItems.Count.ToString() + "/" + MAX_NUM_OF_ITEMS_TO_SELECT.ToString();
            }
        }

        public void OpenSortModePanel() { m_sortModePanelTransform.gameObject.SetActive(true); }

        public void OpenFilterModePanel() { m_filterModePanelTransform.gameObject.SetActive(true); }

        public void CloseSortModePanel() { m_sortModePanelTransform.gameObject.SetActive(false); }

        public void CloseFilterModePanel() { m_filterModePanelTransform.gameObject.SetActive(false); }

        public void Sort()
        {
            StartCoroutine(SortItems(false, true));
        }
        IEnumerator SortItems(bool _usePreviousConditions = false, bool _closeSortModePanel = false)
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
                    eItemSortMode sortMode = m_dropdowns_sort[i].options[m_dropdowns_sort[i].value].text.ToCorrespondingEnumValue<eItemSortMode>();
                    switch (sortMode)
                    {
                        default: // case eItemSortMode.None
                            break;
                        case eItemSortMode.Id:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Item>(x => x.Id, eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Item>(x => x.Id, eSortType.Ascending));
                            }
                            break;
                        case eItemSortMode.Rarity:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Item>(x => x.Rarity, eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Item>(x => x.Rarity, eSortType.Ascending));
                            }
                            break;
                        case eItemSortMode.Price:
                            {
                                if (m_toggles_sort[i].isOn)
                                    m_sortConditions.Add(new KeySelectorAndSortType<Item>(x => x.SellingPrice, eSortType.Descending));
                                else
                                    m_sortConditions.Add(new KeySelectorAndSortType<Item>(x => x.SellingPrice, eSortType.Ascending));
                            }
                            break;
                    }

                    yield return null;
                }
            }

            m_itemsToDisplay = m_itemsToDisplay.OrderByMultipleConditions(m_sortConditions);

            int? minSellingPrice = m_itemsToDisplay.OrderBy(x => x.SellingPrice).FirstOrDefault()?.SellingPrice;
            int? maxSellingPrice = m_itemsToDisplay.OrderByDescending(x => x.SellingPrice).FirstOrDefault()?.SellingPrice;

            m_rangeSlider_price.LowerValue = minSellingPrice ?? 0;
            m_rangeSlider_price.HigherValue = maxSellingPrice ?? 1000000000;

            ReorderItemButtons();

            if (_closeSortModePanel)
            {
                CloseSortModePanel();
                m_text_applyFilterModeButton.text = "Apply";
            }
        }

        private void ReorderItemButtons()
        {
            int index = 0;
            foreach (Item item in m_itemsToDisplay)
            {
                m_transforms_itemButton[item].SetSiblingIndex(index);

                index++;
            }

            m_dynamicGridLayoutGroup.RefreshLayout(index);
        }

        public void Filter() { StartCoroutine(FilterItems()); }
        IEnumerator FilterItems()
        {
            m_text_applyFilterModeButton.text = "Applying...";
            yield return null;

            int minSellingPrice = Mathf.RoundToInt(m_rangeSlider_price.LowerValue);
            int maxSellingPrice = Mathf.RoundToInt(m_rangeSlider_price.HigherValue);

            List<eRarity> selectedRarities = new List<eRarity>();
            for (int i = 0; i < m_toggles_rarity.Length; i++)
            {
                int rarityValue = (i + 1) * CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES;

                if (m_toggles_rarity[i].IsOn)
                    selectedRarities.Add((eRarity)rarityValue);
            }

            m_itemsToDisplay = m_items.Where(x => x.Key.SellingPrice >= minSellingPrice
                                                && x.Key.SellingPrice <= maxSellingPrice
                                                && selectedRarities.Contains(x.Key.Rarity))
                                                .ToDictionary(x => x.Key, x => x.Value).Keys.ToList();

            switch (m_itemClassification)
            {
                default: // case eItemClassification.SkillItem
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is SkillItem).ToList();
                    break;
                case eItemClassification.SkillMaterial:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is SkillMaterial).ToList();
                    break;
                case eItemClassification.ItemMaterial:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is ItemMaterial).ToList();
                    break;
                case eItemClassification.EquipmentMaterial:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is EquipmentMaterial).ToList();
                    break;
                case eItemClassification.EvolutionMaterial:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is EvolutionMaterial).ToList();
                    break;
                case eItemClassification.WeaponEnhancementMaterial:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is WeaponEnhancementMaterial).ToList();
                    break;
                case eItemClassification.UnitEnhancementMaterial:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is UnitEnhancementMaterial).ToList();
                    break;
                case eItemClassification.SkillEnhancementMaterial:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is SkillEnhancementMaterial).ToList();
                    break;
                case eItemClassification.GachaCostItem:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is GachaCostItem).ToList();
                    break;
                case eItemClassification.EquipmentTradingItem:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is EquipmentTradingItem).ToList();
                    break;
                case eItemClassification.UnitTradingItem:
                    m_itemsToDisplay = m_itemsToDisplay.Where(x => x is UnitTradingItem).ToList();
                    break;
            }

            yield return StartCoroutine(SortItems(true));

            CloseFilterModePanel();
            m_text_applyFilterModeButton.text = "Apply";
        }
    }

    public enum eItemSelectionMode
    {
        List,
        UnitEnhancementMaterials,
        ItemSet
    }

    public enum eItemSortMode
    {
        None,
        Id,
        Rarity,
        Price
    }

    public enum eItemClassification
    {
        SkillItem,
        SkillMaterial,
        ItemMaterial,
        EquipmentMaterial,
        EvolutionMaterial,
        WeaponEnhancementMaterial,
        UnitEnhancementMaterial,
        SkillEnhancementMaterial,
        GachaCostItem,
        EquipmentTradingItem,
        UnitTradingItem
    }
}