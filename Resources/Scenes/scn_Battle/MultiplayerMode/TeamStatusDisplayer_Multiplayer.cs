using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using EEANWorks.Games.Unity.Engine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class TeamStatusDisplayer_Multiplayer : MonoBehaviour
    {
        #region Private Fields
        private InfoPanelManager_Unit m_unitInfoPanelManager;

        private List<Button> m_buttons_unit;
        private List<Image> m_images_unit;
        private List<Image> m_images_rarityTexture;
        private List<Button> m_buttons_info;
        private List<Transform> m_transforms_statusEffect;
        private List<Slider> m_sliders_hpBar;
        private List<Text> m_texts_hp;

        private UnityBattleSystem_Multiplayer m_mainScript;

        private List<PlayerUnitController> m_unitControllers;

        private bool m_isInitialized;

        private int m_currentSelectedUnitIndex;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_isInitialized = false;

            m_unitInfoPanelManager = this.transform.root.GetComponent<InfoPanelManager_Unit>();

            m_buttons_unit = new List<Button>();
            m_images_unit = new List<Image>();
            m_images_rarityTexture = new List<Image>();
            m_buttons_info = new List<Button>();
            m_transforms_statusEffect = new List<Transform>();
            m_sliders_hpBar = new List<Slider>();
            m_texts_hp = new List<Text>();

            m_unitControllers = new List<PlayerUnitController>();

            m_currentSelectedUnitIndex = -1;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();

            if (m_isInitialized)
            {
                PlayerController_Multiplayer playerController = m_mainScript.PlayerController;
                if (playerController.PlayerData.SelectedUnitIndex != m_currentSelectedUnitIndex)
                {
                    m_currentSelectedUnitIndex = playerController.PlayerData.SelectedUnitIndex;
                    UpdateHighlight();
                }

            }
        }

        private void Initialize()
        {
            try
            {
                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();
                if (m_mainScript == null)
                    return;

                if (m_mainScript.IsInitialized)
                {
                    int playerId = m_mainScript.PlayerController.PlayerId;
                    List<GameObject> gos_unit = m_mainScript.GOs_PlayerUnit;
                    m_unitControllers.Clear();
                    for (int i = 0; i < gos_unit.Count; i++)
                    {
                        PlayerUnitController unitController = gos_unit[i].GetComponent<PlayerUnitController>();
                        m_unitControllers.Add(unitController);

                        GameObject go_unitStatus = Instantiate(BattleSceneAssetContainer.Instance.UnitStatusPrefab, this.transform);
                        go_unitStatus.name = "UnitStatus@" + (i + 1).ToString();

                        UnitInstance unit = unitController.UnitReference;

                        Transform transform_unitStatus = go_unitStatus.transform;
                        GameObjectFormatter_ObjectButton goFormatter_unitButton = transform_unitStatus.Find("UnitButtonContainer").Find("Button@Object").GetComponent<GameObjectFormatter_ObjectButton>();
                        goFormatter_unitButton.Format(unit, null, () => m_mainScript.Request_ChangeUnit(unitController.PrivateIndex), () => m_unitInfoPanelManager.InstantiateInfoPanel(unit, true));
                        m_images_rarityTexture.Add(goFormatter_unitButton.Image_RarityTexture);

                        m_transforms_statusEffect.Add(transform_unitStatus.Find("StatusEffect"));

                        m_sliders_hpBar.Add(transform_unitStatus.Find("Slider@HPBar").GetComponent<Slider>());

                        m_texts_hp.Add(transform_unitStatus.Find("Text@HP").GetComponent<Text>());
                    }

                    TryUpdateTeamStatus();

                    Debug.Log("Team Status Panel initialized successfully!");
                    m_isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("TeamStatusDisplayer.Initialize(): " + ex.Message);
            }
        }

        public void TryUpdateTeamStatus()
        {
            UpdateHPInfo();
            UpdateStatusEffects();
        }

        private void UpdateStatusEffects()
        {
            for (int i = 0; i < m_unitControllers.Count; i++)
            {
                m_transforms_statusEffect[i].ClearChildren();
                foreach (StatusEffect statusEffect in m_unitControllers[i].UnitReference.StatusEffects)
                {
                    GameObject go_statusEffectIcon = Instantiate(BattleSceneAssetContainer.Instance.StatusEffectIconPrefab, m_transforms_statusEffect[i]);
                    go_statusEffectIcon.GetComponent<Image>().sprite = SpriteContainer.Instance.GetStatusEffectIcon(statusEffect);
                    go_statusEffectIcon.transform.Find("Text@TimesRemaining").GetComponent<Text>().text = (statusEffect.Duration.ActivationTimes != 0) ? statusEffect.Duration.ActivationTimes.ToString() : string.Empty;
                    go_statusEffectIcon.transform.Find("Text@TurnsRemaining").GetComponent<Text>().text = (statusEffect.Duration.Turns != 0m) ? statusEffect.Duration.Turns.ToString() : string.Empty;
                }
            }
        }

        private void UpdateHPInfo()
        {
            try
            {
                for (int i = 0; i < this.transform.childCount; i++)
                {
                    int remainingHP = m_unitControllers[i].UnitReference.RemainingHP;
                    int maxHP = Calculator.MaxHP(m_unitControllers[i].UnitReference);

                    m_sliders_hpBar[i].value = (float)remainingHP / maxHP;
                    m_texts_hp[i].text = remainingHP.ToString() + " / " + maxHP.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("TeamStatusDisplayer.RefreshTeamStatus(): " + ex.Message);
            }
        }

        private void UpdateHighlight()
        {
            if (m_currentSelectedUnitIndex > m_images_rarityTexture.Count)
                return;

            ResetHighlight();

            if (m_currentSelectedUnitIndex >= 0)
                m_images_rarityTexture[m_currentSelectedUnitIndex].color = Color.red;
        }

        private void ResetHighlight()
        {
            foreach (Image image_rarityFrame in m_images_rarityTexture)
            {
                image_rarityFrame.color = Color.white;
            }
        }
    }
}