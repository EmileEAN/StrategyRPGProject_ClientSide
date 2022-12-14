using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using EEANWorks.Games.Unity.Engine;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using EEANWorks.Games.Unity.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class ActionUIManager_Multiplayer : MonoBehaviour
    {
        #region Properties
        public eActionType CurrentActionSelected { get; private set; }
        #endregion

        #region Private Fields
        private UnityBattleSystem_Multiplayer m_mainScript;
        private PlayerController_Multiplayer m_playerController;
        private TileMaskManager_Multiplayer m_tileMaskManager; //m_tileMaskManager checks for this.IsInitialized in order to initialize itself.
        private AnimationController_Multiplayer m_animationController;
        private InfoPanelManager_Skill m_infoPanelManager_skill;

        private Transform m_transform_nonUSkillActions;
        private Button m_button_move;
        private Button m_button_attack;
        private Button m_button_item;
        private List<Button> m_buttons_skill;
        private List<GameObject> m_gos_skillDetainlInfoPopUp;

        private List<_2DCoord> m_selectedPrimaryCoords;
        private bool m_arePrimaryCoordsSelected;

        private Button m_button_confirmation;
        private Text m_text_confirmationButton;

        private readonly Color m_color_button_bright = new Color32(236, 192, 172, 255);
        private readonly Color m_color_button_color = new Color32(186, 135, 112, 255);
        private readonly Color m_color_button_default = Color.white;
        private readonly Color m_color_button_transparent = new Color(0, 0, 0, 0);

        private List<PlayerUnitController> m_unitControllers;
        private int m_currentUnitId;
        private string m_nameOfSelectedSkill;

        private bool m_isInitialized;
        private bool m_isUIOn;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            CurrentActionSelected = eActionType.None;
            m_nameOfSelectedSkill = string.Empty;

            m_isInitialized = false;
            m_isUIOn = false;
            m_currentUnitId = -1;
            m_unitControllers = new List<PlayerUnitController>();

            m_transform_nonUSkillActions = this.transform.Find("NonUSkillActions");

            m_buttons_skill = new List<Button>();
            m_gos_skillDetainlInfoPopUp = new List<GameObject>();

            m_arePrimaryCoordsSelected = false;

            m_button_confirmation = this.transform.Find("ActionConfirmation").Find("Button@ConfirmAction").GetComponent<Button>();
            m_text_confirmationButton = m_button_confirmation.transform.Find("Text").GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();

            if (m_isInitialized)
            {
                if ((m_playerController.IsMyTurn && !m_isUIOn)
                    || (!m_playerController.IsMyTurn && m_isUIOn))
                {
                    CurrentActionSelected = eActionType.None;
                    ResetUI(m_playerController.IsMyTurn);
                    StartCoroutine(DisplayTargetAreaForAction(CurrentActionSelected));
                }
                else if (m_playerController.IsMyTurn && m_currentUnitId != m_playerController.PlayerData.SelectedUnitIndex)
                {
                    m_currentUnitId = m_playerController.PlayerData.SelectedUnitIndex;
                    ResetUI(m_playerController.IsMyTurn);

                    if (CurrentActionSelected == eActionType.Skill)
                        Request_ChangeAction(eActionType.None);
                    else
                        StartCoroutine(DisplayTargetAreaForAction(CurrentActionSelected));
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
                    if (m_playerController == null)
                        m_playerController = m_mainScript.PlayerController;
                    if (m_playerController == null)
                        return;

                    if (m_tileMaskManager == null)
                        m_tileMaskManager = GameObject.FindGameObjectWithTag("GameBoard").GetComponent<TileMaskManager_Multiplayer>();
                    if (m_tileMaskManager == null)
                        return;

                    if (m_animationController == null)
                        m_animationController = this.transform.root.GetComponent<AnimationController_Multiplayer>();
                    if (m_animationController == null)
                        return;

                    if (m_infoPanelManager_skill == null)
                        m_infoPanelManager_skill = this.transform.root.GetComponent<InfoPanelManager_Skill>();
                    if (m_infoPanelManager_skill == null)
                        return;

                    m_unitControllers.Clear();
                    List<GameObject> units = m_mainScript.GOs_PlayerUnit;
                    foreach (GameObject unit in units)
                    {
                        m_unitControllers.Add(unit.GetComponent<PlayerUnitController>());
                    }

                    if (m_unitControllers.Count != units.Count)
                        return;

                    Debug.Log("ActionUIManager: Initialized successfully!");
                    m_isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.Initialize() : " + ex.Message);
            }
        }

        public void Request_ChangeAction(eActionType _action, string _skillName = "")
        {
            float startingTime = Time.realtimeSinceStartup;
            Debug.Log("Request_ChangeAction() Started!");

            StartCoroutine(ChangeAction(_action, _skillName));

            Debug.Log("Request_ChangeAction() Ended! " + (Time.realtimeSinceStartup - startingTime).ToString() + "seconds");
        }
        IEnumerator ChangeAction(eActionType _action, string _skillName = "")
        {
            if (m_animationController.LockUI)
                yield break;

            if (CurrentActionSelected != _action
                || m_nameOfSelectedSkill != _skillName)
            {
                Debug.Log("ActionSelected: " + CurrentActionSelected.ToString() + " -> " + _action.ToString() + ((m_nameOfSelectedSkill != _skillName) ? " [" + m_nameOfSelectedSkill + " -> " + _skillName.ToString() + "]" : string.Empty));
                CurrentActionSelected = _action;
                m_nameOfSelectedSkill = _skillName;
                yield return StartCoroutine(DisplayTargetAreaForAction(CurrentActionSelected, _skillName));
                UpdateUI();
            }
        }

        public void Request_ConfirmAction() { StartCoroutine(ConfirmAction()); }
        IEnumerator ConfirmAction()
        {
            if (!m_tileMaskManager.IsInitialized)
                yield break;

            if (!m_animationController.IsInitialized || m_animationController.LockUI)
                yield break;

            List<_2DCoord> selectedCoords = m_tileMaskManager.SelectedCoords();

            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            switch (CurrentActionSelected)
            {

                case eActionType.Move:
                    yield return this.StartCoroutineRepetitionUntilTrue(m_mainScript.MoveUnit(looperAndCoroutineLinker, selectedCoords.First()), looperAndCoroutineLinker);
                    UpdateUI();
                    break;
                case eActionType.Attack:
                    yield return this.StartCoroutineRepetitionUntilTrue(m_mainScript.Attack(looperAndCoroutineLinker, selectedCoords), looperAndCoroutineLinker);
                    UpdateUI();
                    break;
                case eActionType.Skill:
                    {
                        if (GameDataContainer.Instance.Skills.OfType<ActiveSkillData>().First(x => x.Name == m_nameOfSelectedSkill).Effect is IComplexTargetSelectionEffect)
                        {
                            if (!m_arePrimaryCoordsSelected)
                            {
                                m_selectedPrimaryCoords = selectedCoords;
                                m_arePrimaryCoordsSelected = true;
                                m_tileMaskManager.DeselectAll();
                                m_text_confirmationButton.text = "Execute";
                            }
                            else
                            {
                                yield return this.StartCoroutineRepetitionUntilTrue(m_mainScript.UseSkill(looperAndCoroutineLinker, m_nameOfSelectedSkill, m_selectedPrimaryCoords, selectedCoords), looperAndCoroutineLinker);
                                m_selectedPrimaryCoords.Clear();
                                m_arePrimaryCoordsSelected = false;
                            }
                        }
                        else
                        {
                            yield return this.StartCoroutineRepetitionUntilTrue(m_mainScript.UseSkill(looperAndCoroutineLinker, m_nameOfSelectedSkill, selectedCoords, new List<_2DCoord>()), looperAndCoroutineLinker);
                            UpdateUI();
                        }
                    }
                    break;
                default:
                    break;
            }

            m_tileMaskManager.DisplayTargetArea(); // Display target area updated after executing an action
        }

        IEnumerator DisplayTargetAreaForAction(eActionType _action, string _skillName = "")
        {
            float startingTime = Time.realtimeSinceStartup;
            Debug.Log("DisplayTargetAreaForAction() Started!");

            m_mainScript.UpdateTargetableAreaToNull();
            yield return null;

            switch (_action)
            {
                case eActionType.Move:
                    m_nameOfSelectedSkill = string.Empty;
                    yield return StartCoroutine(m_mainScript.UpdateMovableArea());
                    break;
                case eActionType.Attack:
                    m_nameOfSelectedSkill = string.Empty;
                    yield return StartCoroutine(m_mainScript.UpdateAttackTargetableArea());
                    break;
                case eActionType.Skill:
                    m_nameOfSelectedSkill = _skillName;
                    yield return StartCoroutine(m_mainScript.UpdateSkillTargetableArea(_skillName));
                    break;
                default: // No area to display
                    break;
            }

            m_tileMaskManager.DisplayTargetArea();

            Debug.Log("DisplayTargetAreaForAction() Ended! " + (Time.realtimeSinceStartup - startingTime).ToString() + "seconds");
        }

        private void ResetUI(bool _isMyTurn)
        {
            try
            {
                m_transform_nonUSkillActions.ClearChildren();
                foreach (GameObject go_skillDetailInfoPopUp in m_gos_skillDetainlInfoPopUp)
                {
                    Destroy(go_skillDetailInfoPopUp);
                }

                if (_isMyTurn)
                {
                    GameObject moveButton = Instantiate(BattleSceneAssetContainer.Instance.MoveButtonPrefab, m_transform_nonUSkillActions);
                    m_button_move = moveButton.GetComponent<Button>();
                    m_button_move.onClick.AddListener(() => Request_ChangeAction(eActionType.Move));

                    GameObject attackButton = Instantiate(BattleSceneAssetContainer.Instance.AttackButtonPrefab, m_transform_nonUSkillActions);
                    m_button_attack = attackButton.GetComponent<Button>();
                    m_button_attack.onClick.AddListener(() => Request_ChangeAction(eActionType.Attack));

                    GameObject itemButton = Instantiate(BattleSceneAssetContainer.Instance.ItemButtonPrefab, m_transform_nonUSkillActions);
                    m_button_item = itemButton.GetComponent<Button>();
                    m_button_item.onClick.AddListener(() => Request_ChangeAction(eActionType.Item));

                    if (m_currentUnitId >= 0 && m_currentUnitId < m_unitControllers.Count)
                    {
                        m_buttons_skill.Clear();
                        foreach (OrdinarySkill skill in m_unitControllers[m_currentUnitId].UnitReference.Skills.OfType<OrdinarySkill>())
                        {
                            GameObject go_skillButton = Instantiate(BattleSceneAssetContainer.Instance.SkillButtonPrefab, m_transform_nonUSkillActions);
                            go_skillButton.name = skill.BaseInfo.Name;

                            DetailInfoPopUpController detailInfoPopUpController = go_skillButton.GetComponent<DetailInfoPopUpController>();
                            GameObject go_skillDetailInfoPopUp = detailInfoPopUpController.GO_detailInfoPopUp;
                            go_skillDetailInfoPopUp.transform.Find("Text@SkillName").GetComponent<Text>().text = skill.BaseInfo.Name;
                            go_skillDetailInfoPopUp.transform.Find("Text@RequiredSP").GetComponent<Text>().text = "SP REQ: " + skill.BaseInfo.SPCost.ToString();
                            go_skillDetailInfoPopUp.transform.Find("Button@Info").GetComponent<Button>().onClick.AddListener(() => m_infoPanelManager_skill.InstantiateInfoPanel(skill));
                            m_gos_skillDetainlInfoPopUp.Add(go_skillDetailInfoPopUp);

                            Button button_skill = go_skillButton.GetComponent<Button>();
                            m_buttons_skill.Add(button_skill);
                            button_skill.onClick.AddListener(() => Request_ChangeAction(eActionType.Skill, skill.BaseInfo.Name));

                            button_skill.transform.Find("Image@Icon").GetComponent<ImageColorBlender>().Sprite = SpriteContainer.Instance.GetSkillIcon(skill.BaseInfo);
                        }
                    }

                    UpdateUI();

                    if (!m_isUIOn)
                        m_isUIOn = true;
                }
                else
                    m_isUIOn = false;
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.ResetUI() : " + ex.Message);
            }
        }

        private void UpdateUI()
        {
            try
            {
                UpdateMoveButton();
                UpdateAttackButton();
                UpdateItemButton();
                UpdateSkillButtons();
                UpdateConfirmationButton();
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.UpdateUI() : " + ex.Message);
            }
        }

        private void UpdateMoveButton()
        {
            try
            {
                if (m_currentUnitId < 0
                    || m_playerController.PlayerData.Moved
                    /*|| unit.movementBind*/)
                {
                    m_button_move.image.color = m_color_button_default;
                }
                else if (CurrentActionSelected == eActionType.Move)
                    m_button_move.image.color = m_color_button_bright;
                else
                    m_button_move.image.color = m_color_button_color;

                if (m_currentUnitId >= 0
                    && m_currentUnitId < (m_playerController.PlayerData).AlliedUnits.Count
                    && !m_playerController.PlayerData.Moved
                    && CurrentActionSelected != eActionType.Move)
                {
                    m_button_move.interactable = true;
                }
                else
                    m_button_move.interactable = false;

                /* if (unit.movementBind)
                 *      m_moveButton.Find("Image@BindMark").GetComponent<Image>().color = m_buttonColor_default;
                 * else
                 */
                m_button_move.transform.Find("Image@BindMark").GetComponent<Image>().color = m_color_button_transparent;

                if ((1 == 0 /*unit.moveBind*/
                        || m_playerController.PlayerData.Moved)
                    && CurrentActionSelected == eActionType.Move)
                {
                    Request_ChangeAction(eActionType.None);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.UpdateMoveButton() : " + ex.Message);
            }
        }

        private void UpdateAttackButton()
        {
            try
            {
                if (m_currentUnitId < 0
                    || m_playerController.PlayerData.Attacked
                    /*|| unit.attackBind*/)
                {
                    m_button_attack.image.color = m_color_button_default;
                }
                else if (CurrentActionSelected == eActionType.Attack)
                    m_button_attack.image.color = m_color_button_bright;
                else
                    m_button_attack.image.color = m_color_button_color;

                if (m_currentUnitId >= 0
                    && m_currentUnitId < (m_playerController.PlayerData).AlliedUnits.Count
                    && !m_playerController.PlayerData.Attacked
                    && CurrentActionSelected != eActionType.Attack)
                {
                    m_button_attack.interactable = true;
                }
                else
                    m_button_attack.interactable = false;

                /* if (unit.movementBind)
                 *      m_attackButton.Find("Image@BindMark").GetComponent<Image>().color = m_buttonColor_default;
                 * else
                 */
                m_button_attack.transform.Find("Image@BindMark").GetComponent<Image>().color = m_color_button_transparent;

                if ((1 == 0 /*|| unit.attackBind*/
                        || m_playerController.PlayerData.Attacked)
                    && CurrentActionSelected == eActionType.Attack)
                {
                    Request_ChangeAction(eActionType.None);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.UpdateAttackButton() : " + ex.Message);
            }
        }

        private void UpdateItemButton()
        {
            try
            {
                if (true/*unit.itemBind*/)
                {
                    m_button_item.interactable = false;
                    m_button_item.image.color = m_color_button_default;
                    //m_itemButton.Find("Image@BindMark").GetComponent<Image>().color = m_buttonColor_default;
                }
                else
                {
                    m_button_item.interactable = true;
                    m_button_item.transform.Find("Image@BindMark").GetComponent<Image>().color = m_color_button_transparent;


                    if (CurrentActionSelected == eActionType.Item)
                        m_button_item.image.color = m_color_button_bright;
                    else
                        m_button_item.image.color = m_color_button_color;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.UpdateItemButton() : " + ex.Message);
            }
        }

        private void UpdateSkillButtons()
        {
            try
            {
                foreach (Button button_skill in m_buttons_skill)
                {
                    string skillName = button_skill.name;

                    bool areResourcesEnoughForSkillExecution = m_unitControllers[m_currentUnitId].UnitReference.AreResourcesEnoughForSkillExecution(skillName);

                    if (1 == 0/*unit.skillBind*/
                        || !areResourcesEnoughForSkillExecution)
                    {
                        button_skill.image.color = m_color_button_default;
                        //skillButton.Find("Image@BindMark").GetComponent<Image>().color = m_buttonColor_default;
                    }
                    else
                    {
                        button_skill.transform.Find("Image@BindMark").GetComponent<Image>().color = m_color_button_transparent;

                        if (CurrentActionSelected == eActionType.Skill && m_nameOfSelectedSkill == skillName)
                            button_skill.image.color = m_color_button_bright;
                        else
                            button_skill.image.color = m_color_button_color;
                    }


                    Transform transform_iconImage = button_skill.transform.Find("Image@Icon");
                    ImageColorBlender imageColorBlender_icon = transform_iconImage.GetComponent<ImageColorBlender>();
                    Image image_icon = transform_iconImage.GetComponent<Image>();
                    if (1 == 1/*!unit.skillBind*/
                         && areResourcesEnoughForSkillExecution
                         && (CurrentActionSelected != eActionType.Skill
                                || (CurrentActionSelected == eActionType.Skill && m_nameOfSelectedSkill != skillName)))
                    {
                        imageColorBlender_icon.BlendingMethod = eBlendingMethod.None;
                        button_skill.interactable = true;
                    }
                    else
                    {
                        imageColorBlender_icon.BlendingMethod = eBlendingMethod.Saturation;
                        image_icon.color = Color.white; // Any color that has 0 saturation is ok
                        button_skill.interactable = false;
                    }

                    if ((1 == 0 /*|| unit.skillBind*/
                            || !areResourcesEnoughForSkillExecution)
                        && (CurrentActionSelected == eActionType.Skill && m_nameOfSelectedSkill == skillName))
                    {
                        Request_ChangeAction(eActionType.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.UpdateSkillButton() : " + ex.Message);
            }
        }

        public void UpdateConfirmationButton()
        {
            try
            {
                if (!m_tileMaskManager.IsInitialized)
                {
                    m_button_confirmation.interactable = false;
                    return;
                }

                switch (CurrentActionSelected)
                {
                    case eActionType.Move:
                        m_text_confirmationButton.text = "Move";
                        break;

                    case eActionType.Attack:
                        m_text_confirmationButton.text = "Attack";
                        break;

                    case eActionType.Item:
                        m_text_confirmationButton.text = "Use";
                        break;

                    case eActionType.Skill:
                    case eActionType.USkill:
                        {
                            if (GameDataContainer.Instance.Skills.OfType<ActiveSkillData>().First(x => x.Name == m_nameOfSelectedSkill).Effect is IComplexTargetSelectionEffect)
                            {
                                m_text_confirmationButton.text = "Select";
                            }
                            else
                                m_text_confirmationButton.text = "Selected (" + m_tileMaskManager.NumOfTilesSelected().ToString() + "/" + m_tileMaskManager.MaxNumOfTargets.ToString() + ")\nExecute";
                        }
                        break;

                    default:
                        break;
                }

                if (CurrentActionSelected == eActionType.None
                    || m_tileMaskManager.NumOfTilesSelected() <= 0)
                {
                    m_button_confirmation.interactable = false;
                }
                else
                    m_button_confirmation.interactable = true;

                Debug.Log("Confirmation Button Updated.");
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.UpdateConfirmationButton() : " + ex.Message);
            }
        }
    }
}