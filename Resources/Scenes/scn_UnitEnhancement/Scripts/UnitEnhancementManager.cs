using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using EEANWorks.Games.Unity.Engine;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(InfoPanelManager_Unit))]
    [RequireComponent(typeof(InfoPanelManager_Item))]
    public class UnitEnhancementManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Button m_skipAnimationButton;

        [SerializeField]
        private GameObjectFormatter_ObjectButton m_goFormatter_unitButton;
        [SerializeField]
        private GameObjectFormatter_ValueChangeTexts m_goFormatter_ValueChangeTexts_Level;


        [SerializeField]
        private Slider m_expBarSlider;
        [SerializeField]
        private Text m_currentExpText;
        [SerializeField]
        private Text m_requiredExpForLevelUpText;
        [SerializeField]
        private float m_timeToCompleteExpUIAnimationForMaxLevel;

        [SerializeField]
        private GameObjectFormatter_ValueChangeTexts m_goFormatter_ValueChangeTexts_MaxHP;
        [SerializeField]
        private GameObjectFormatter_ValueChangeTexts m_goFormatter_ValueChangeTexts_PhyStr;
        [SerializeField]
        private GameObjectFormatter_ValueChangeTexts m_goFormatter_ValueChangeTexts_PhyRes;
        [SerializeField]
        private GameObjectFormatter_ValueChangeTexts m_goFormatter_ValueChangeTexts_MagStr;
        [SerializeField]
        private GameObjectFormatter_ValueChangeTexts m_goFormatter_ValueChangeTexts_MagRes;
        [SerializeField]
        private GameObjectFormatter_ValueChangeTexts m_goFormatter_ValueChangeTexts_Vit;


        [SerializeField]
        private Transform m_materialsContainerTransform;
        [SerializeField]
        private Button m_selectMaterialsButton;

        [SerializeField]
        private Text m_acquiringExpText;
        [SerializeField]
        private Text m_exceedingExpText;
        [SerializeField]
        private Text m_requiredGoldText;

        [SerializeField]
        private Button m_enhanceButton;
        #endregion

        #region Private Fields
        private InfoPanelManager_Unit m_infoPanelManager_unit;
        private InfoPanelManager_Item m_infoPanelManager_item;

        private Outline m_outline_exceedingExpText;

        private Unit m_selectedUnit;
        private Dictionary<Item, int> m_selectedMaterials;

        private int m_initialLevel;
        private int m_totalExp;
        private int m_eventualLevel;
        private int m_maxLevel;

        private int m_remainingExp;
        private int m_eventualExpForLevel;

        private int m_currentExp;
        private int m_requiredExpForLevelUp;

        private float m_expUIAnimationTimePerExp;

        private bool m_processingRequest;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_skipAnimationButton.interactable = false;
            m_skipAnimationButton.onClick.AddListener(() => { SkipAnimation(); });

            m_infoPanelManager_unit = this.GetComponent<InfoPanelManager_Unit>();
            m_infoPanelManager_item = this.GetComponent<InfoPanelManager_Item>();

            m_outline_exceedingExpText = m_exceedingExpText.GetComponent<Outline>();

            m_selectedUnit = GameDataContainer.Instance.SelectedUnit;
            m_selectedMaterials = GameDataContainer.Instance.SelectedItems;

            m_expUIAnimationTimePerExp = m_timeToCompleteExpUIAnimationForMaxLevel / Calculator.RequiredExperienceForLevelUp(Calculator.MaxLevelForRarity(eRarity.Legendary) - 1);

            m_processingRequest = false;

            Initialize();
        }

        private void Initialize()
        {
            bool isUnitSelected = m_selectedUnit != null;

            m_enhanceButton.interactable = isUnitSelected;

            UnityAction buttonClickAction = new UnityAction(delegate
            {
                GameDataContainer.Instance.UnitSelectionMode = eUnitSelectionMode.EnhancingUnit;
                SceneConnector.GoToScene("scn_UnitList", true);
            });
            UnityAction buttonLongPressAction = isUnitSelected ? new UnityAction(() => m_infoPanelManager_unit.InstantiateInfoPanel(m_selectedUnit)) : null;
            m_goFormatter_unitButton.Format(m_selectedUnit, "", buttonClickAction, buttonLongPressAction);

            int acquiringExp = 0;
            decimal bonusElementMultiplier = CoreValues.MULTIPLIER_FOR_BONUS_ELEMENT_MATCHING_UNIT;

            GameObject m_go_objectButton = SharedAssetContainer.Instance.ObjectButtonPrefab;
            int materialCount = 0;
            foreach (var itemQuantity in m_selectedMaterials)
            {
                for (int i = 0; i < itemQuantity.Value; i++)
                {
                    GameObject go_materialButton = Instantiate(m_go_objectButton, m_materialsContainerTransform);
                    GameObjectFormatter_ObjectButton goFormatter_materialButton = go_materialButton.GetComponent<GameObjectFormatter_ObjectButton>();
                    UnitEnhancementMaterial tmp_material = itemQuantity.Key as UnitEnhancementMaterial; // Variable to avoid reference error within lambda expression
                    goFormatter_materialButton.Format(itemQuantity.Key, "", () => m_infoPanelManager_item.InstantiateInfoPanel(tmp_material));

                    acquiringExp += tmp_material.BonusElements.ContainsAny(m_selectedUnit?.BaseInfo.Elements) ? Convert.ToInt32(Math.Round(tmp_material.EnhancementValue * bonusElementMultiplier, MidpointRounding.AwayFromZero)) : tmp_material.EnhancementValue;

                    materialCount++;
                }
            }

            m_initialLevel = isUnitSelected ? Calculator.Level(m_selectedUnit) : 0;
            m_totalExp = m_selectedUnit.AccumulatedExperience + acquiringExp;
            m_eventualLevel = isUnitSelected ? Calculator.Level(m_totalExp) : 0;
            m_maxLevel = Calculator.MaxLevelForRarity(m_selectedUnit.BaseInfo);
            m_goFormatter_ValueChangeTexts_Level.Text_InitialValue.text = isUnitSelected ? m_initialLevel.ToString() : "-";
            m_goFormatter_ValueChangeTexts_Level.Text_EventualValue.text = isUnitSelected ? m_eventualLevel.ToString() : "-";

            m_currentExp = Calculator.LevelExperience(m_selectedUnit);
            m_requiredExpForLevelUp = Calculator.RequiredExperienceForLevelUp(m_selectedUnit);
            m_currentExpText.text = m_currentExp.ToString();
            m_requiredExpForLevelUpText.text = m_requiredExpForLevelUp.ToString();

            m_goFormatter_ValueChangeTexts_MaxHP.Format(isUnitSelected ? Calculator.MaxHP(m_selectedUnit).ToString() : "-",
                                                        isUnitSelected ? Calculator.MaxHP(m_selectedUnit.BaseInfo, m_totalExp).ToString() : "-");
            m_goFormatter_ValueChangeTexts_PhyStr.Format(isUnitSelected ? Calculator.PhysicalStrength(m_selectedUnit).ToString() : "-", 
                                                        isUnitSelected ? Calculator.PhysicalStrength(m_selectedUnit.BaseInfo, m_totalExp).ToString() : "-");
            m_goFormatter_ValueChangeTexts_PhyRes.Format(isUnitSelected ? Calculator.PhysicalResistance(m_selectedUnit).ToString() : "-",
                                                        isUnitSelected ? Calculator.PhysicalResistance(m_selectedUnit.BaseInfo, m_totalExp).ToString() : "-");
            m_goFormatter_ValueChangeTexts_MagStr.Format(isUnitSelected ? Calculator.MagicalStrength(m_selectedUnit).ToString() : "-",
                                                        isUnitSelected ? Calculator.MagicalStrength(m_selectedUnit.BaseInfo, m_totalExp).ToString() : "-");
            m_goFormatter_ValueChangeTexts_MagRes.Format(isUnitSelected ? Calculator.MagicalResistance(m_selectedUnit).ToString() : "-",                                                                        isUnitSelected ? Calculator.MagicalResistance(m_selectedUnit.BaseInfo, m_totalExp).ToString() : "-");
            m_goFormatter_ValueChangeTexts_Vit.Format(isUnitSelected ? Calculator.Vitality(m_selectedUnit).ToString() : "-",
                                                      isUnitSelected ? Calculator.Vitality(m_selectedUnit.BaseInfo, m_totalExp).ToString() : "-");

            m_acquiringExpText.text = acquiringExp.ToString();
            int maxAccumulatedExp = isUnitSelected ? Calculator.MaxAccumulatedExperienceForRarity(m_selectedUnit.BaseInfo.Rarity) : 0;
            int exceedingExp = isUnitSelected ? ((m_selectedUnit.AccumulatedExperience + acquiringExp) - maxAccumulatedExp) : 0;
            m_exceedingExpText.text = (exceedingExp > 0) ? "<color=red>" + exceedingExp.ToString() + "</color>" : "0";
            m_outline_exceedingExpText.enabled = exceedingExp > 0;
            m_requiredGoldText.text = isUnitSelected ? (m_initialLevel * CoreValues.OBJECT_ENHANCEMENT_COST_MULTIPLIER * materialCount).ToString() : "0";
        }

        #region Public Methods
        public void SelectMaterials()
        {
            GameDataContainer.Instance.ItemSelectionMode = eItemSelectionMode.UnitEnhancementMaterials;
            SceneConnector.GoToScene("scn_ItemList", true);
        }

        public void Request_UnitEnhance()
        {
            StartCoroutine(TryEnhanceUnit());
        }
        #endregion

        #region Private Mehods
        private void SkipAnimation()
        {
            m_skipAnimationButton.interactable = false;

            StopAllCoroutines();

            m_goFormatter_ValueChangeTexts_Level.SetEventualToInitial();
            m_goFormatter_ValueChangeTexts_MaxHP.SetEventualToInitial();
            m_goFormatter_ValueChangeTexts_PhyStr.SetEventualToInitial();
            m_goFormatter_ValueChangeTexts_PhyRes.SetEventualToInitial();
            m_goFormatter_ValueChangeTexts_MagStr.SetEventualToInitial();
            m_goFormatter_ValueChangeTexts_MagRes.SetEventualToInitial();
            m_goFormatter_ValueChangeTexts_Vit.SetEventualToInitial();

            if (m_eventualLevel == m_maxLevel) // If m_eventual is the max level for rarity
            {
                m_currentExpText.text = m_requiredExpForLevelUpText.text = "-";
                m_expBarSlider.value = m_expBarSlider.maxValue;

                m_selectMaterialsButton.interactable = false;
            }
            else
            {
                m_currentExp = m_totalExp - Calculator.MinimumAccumulatedExperienceRequired(m_eventualLevel);
                m_requiredExpForLevelUp = Calculator.RequiredExperienceForLevelUp(m_eventualLevel);

                m_currentExpText.text = m_currentExp.ToString();
                m_requiredExpForLevelUpText.text = m_requiredExpForLevelUp.ToString();

                m_expBarSlider.value = m_expBarSlider.maxValue * ((float)m_currentExp / m_requiredExpForLevelUp);
            }

            m_acquiringExpText.text = "0";
            m_exceedingExpText.text = "0";
            m_outline_exceedingExpText.enabled = false;
            m_requiredGoldText.text = "0";
        }

        IEnumerator TryEnhanceUnit()
        {
            if (m_processingRequest)
                yield break;

            m_processingRequest = true;

            m_skipAnimationButton.interactable = true;

            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(EnhanceUnit(looperAndCoroutineLinker), looperAndCoroutineLinker);

            m_skipAnimationButton.interactable = false;

            m_processingRequest = false;
        }

        IEnumerator EnhanceUnit(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            string materialsQuantityString = string.Empty;
            foreach (var itemQuantity in GameDataContainer.Instance.SelectedItems)
            {
                materialsQuantityString += itemQuantity.Key.Id.ToString() + "," + itemQuantity.Value.ToString();
                materialsQuantityString += "/"; // Separator
            }
            materialsQuantityString = materialsQuantityString.RemoveLast(); // Remove last '/'

            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "EnhanceUnit"},
                    {"sessionId", GameDataContainer.Instance.SessionId.ToString()},
                    {"unitId", GameDataContainer.Instance.SelectedUnit.UniqueId.ToString()},
                    {"materialsQuantity", materialsQuantityString}
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

                if (response != "error")
                {
                    int expToAdd = Convert.ToInt32(response);
                    m_remainingExp = expToAdd;
                    m_selectedUnit.GainExperience(expToAdd); // Apply exp change to the unit

                    GameDataContainer.Instance.Player.ItemsOwned.SubtractAll(m_selectedMaterials); // Subtract quantity of each material spent
                    m_materialsContainerTransform.ClearChildren(); // Remove all selected material game objects

                    m_enhanceButton.interactable = false; // Do not allow second execution

                    yield return null; // Return once, so that the modification to the game objects become visible

                    yield return StartCoroutine(ProcessExpAnimation());

                    if (m_eventualLevel == m_maxLevel) // If the unit reached its max level
                        m_selectMaterialsButton.interactable = false; // Disable material selection button

                    m_acquiringExpText.text = "0";
                    m_exceedingExpText.text = "0";
                    m_outline_exceedingExpText.enabled = false;
                    m_requiredGoldText.text = "0";
                }
                else
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Something went wrong!\nPlease try again.", "OK");

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        IEnumerator ProcessExpAnimation()
        {
            for (int lv = m_initialLevel; lv <= m_eventualLevel; lv++) // For each level
            {
                if (lv != m_initialLevel)
                {
                    // Update Texts to match new values
                    m_goFormatter_ValueChangeTexts_Level.Text_InitialValue.text = lv.ToString();
                    m_goFormatter_ValueChangeTexts_MaxHP.Text_InitialValue.text = Calculator.MaxHPAtLevel(m_selectedUnit, lv).ToString();
                    m_goFormatter_ValueChangeTexts_PhyStr.Text_InitialValue.text = Calculator.PhysicalStrengthAtLevel(m_selectedUnit, lv).ToString();
                    m_goFormatter_ValueChangeTexts_PhyRes.Text_InitialValue.text = Calculator.PhysicalResistanceAtLevel(m_selectedUnit, lv).ToString();
                    m_goFormatter_ValueChangeTexts_MagStr.Text_InitialValue.text = Calculator.MagicalStrengthAtLevel(m_selectedUnit, lv).ToString();
                    m_goFormatter_ValueChangeTexts_MagRes.Text_InitialValue.text = Calculator.MagicalResistanceAtLevel(m_selectedUnit, lv).ToString();
                    m_goFormatter_ValueChangeTexts_Vit.Text_InitialValue.text = Calculator.VitalityAtLevel(m_selectedUnit, lv).ToString();

                    if (lv == m_maxLevel) // If lv is the max level
                    {
                        m_currentExpText.text = m_requiredExpForLevelUpText.text = "-";
                        yield break;
                    }

                    m_currentExp = 0;
                    m_requiredExpForLevelUp = Calculator.RequiredExperienceForLevelUp(lv); // Update required exp value based on the level
                    m_requiredExpForLevelUpText.text = m_requiredExpForLevelUp.ToString();

                    yield return null; // Make update visible
                }

                yield return StartCoroutine(AnimateExpUI());
            }
        }

        IEnumerator AnimateExpUI()
        {
            int expTillLevelUp = m_requiredExpForLevelUp - m_currentExp;
            m_eventualExpForLevel = (m_remainingExp >= expTillLevelUp) ? m_requiredExpForLevelUp : (m_currentExp + m_remainingExp);
            // Execute coroutines simultaneously
            Coroutine coroutine_numberCount = StartCoroutine(m_currentExpText.CountNumber(m_currentExp, m_eventualExpForLevel, expTillLevelUp * m_expUIAnimationTimePerExp, 0));
            Coroutine coroutine_sliderAnimation = StartCoroutine(AnimateSlider());

            // Wait until both have finished
            yield return coroutine_numberCount;
            yield return coroutine_sliderAnimation;

            m_remainingExp -= expTillLevelUp; // It will become negative if level up stops (meaning that m_remainingExp is less than the amount required to level up)
        }

        IEnumerator AnimateSlider()
        {
            while (m_currentExpText.text != m_eventualExpForLevel.ToString()) // While m_currentExpText.CountNumber() does not end
            {
                m_currentExp = Convert.ToInt32(m_currentExpText.text);
                m_expBarSlider.value = m_expBarSlider.maxValue * ((float)m_currentExp / m_requiredExpForLevelUp); // Update slider value

                yield return null;
            }

            m_currentExp = Convert.ToInt32(m_currentExpText.text);
            m_expBarSlider.value = m_expBarSlider.maxValue * ((float)m_currentExp / m_requiredExpForLevelUp); // Update slider value once again
        }
        #endregion
    }
}
