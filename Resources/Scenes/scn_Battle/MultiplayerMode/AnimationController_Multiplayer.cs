using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine;
using EEANWorks.Games.Unity.Graphics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class AnimationController_Multiplayer : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Vector3 m_effectGenerationPointHeight;
        [SerializeField]
        private int m_projectileForce;
        [SerializeField]
        private RectTransform m_matchResultPanel;
        [SerializeField]
        private int m_secondsTillSceneChange;
        #endregion

        #region Properties
        public bool LockUI { get { return m_playingAnimation; } }
        public bool IsInitialized { get; private set; }
        #endregion

        #region Private Fields
        private UnityBattleSystem_Multiplayer m_mainScript;
        private TileMapManager_Multiplayer m_tileMapManager;

        private SPDisplayer_Multiplayer m_spDisplayer;
        private TeamStatusDisplayer_Multiplayer m_teamStatusDisplayer;

        private Text m_text_central;

        private Image m_image_activatedSkillOrStatusEffectNamePanel;
        private ImageColorBlender m_imageColorBlender_activatedSkillOrStatusEffectNamePanel;
        private Text m_text_activatedSkillOrStatusEffectName;

        private int m_indexOfLastLog;

        private bool m_playingAnimation;

        private GameObject m_go_skillActivationEffect;
        private GameObject m_go_effectGenerationPoint;
        private int m_latestEffectGenerationPointId;
        private bool m_effectGenerationPointChanged;
        private GameObject m_go_attachmentEffect;

        private Transform m_transform_actorUnitChip;

        private List<Transform> m_transforms_defeatedUnitChip;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            IsInitialized = false;

            m_indexOfLastLog = -1;

            m_playingAnimation = false;

            m_go_skillActivationEffect = null;
            m_go_effectGenerationPoint = null;
            m_latestEffectGenerationPointId = default;
            m_go_attachmentEffect = null;

            m_transform_actorUnitChip = null;

            m_transforms_defeatedUnitChip = new List<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsInitialized)
                Initialize();

            if (IsInitialized)
            {
                if (m_indexOfLastLog < m_mainScript.EventLogs.Count - 1
                    && !m_playingAnimation)
                {
                    StartCoroutine(AnimateLogEvent(m_indexOfLastLog + 1));
                }
            }
        }

        private void Initialize()
        {
            if (m_mainScript == null)
                m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();
            if (m_mainScript == null)
                return;

            GameObject go_gameBoard = GameObject.FindGameObjectWithTag("GameBoard");

            if (m_tileMapManager == null)
                m_tileMapManager = go_gameBoard?.GetComponent<TileMapManager_Multiplayer>();
            if (m_tileMapManager == null)
                return;

            if (m_spDisplayer == null)
                m_spDisplayer = go_gameBoard?.GetComponent<SPDisplayer_Multiplayer>();
            if (m_spDisplayer == null)
                return;

            GameObject go_canvas = GameObject.FindGameObjectWithTag("Canvas");

            if (m_teamStatusDisplayer == null)
                m_teamStatusDisplayer = go_canvas?.transform.Find("TeamStatus")?.GetComponent<TeamStatusDisplayer_Multiplayer>();
            if (m_teamStatusDisplayer == null)
                return;

            if (m_text_central == null)
                m_text_central = go_canvas?.transform.Find("Text@Central").GetComponent<Text>();
            if (m_text_central == null)
                return;

            Transform transform_activatedSkillOrStatusEffectNamePanel = go_canvas?.transform.Find("Panel@ActivatedSkillOrStatusEffectName");

            if (m_image_activatedSkillOrStatusEffectNamePanel == null)
                m_image_activatedSkillOrStatusEffectNamePanel = transform_activatedSkillOrStatusEffectNamePanel?.GetComponent<Image>();
            if (m_image_activatedSkillOrStatusEffectNamePanel == null)
                return;

            if (m_imageColorBlender_activatedSkillOrStatusEffectNamePanel == null)
                m_imageColorBlender_activatedSkillOrStatusEffectNamePanel = transform_activatedSkillOrStatusEffectNamePanel?.GetComponent<ImageColorBlender>();
            if (m_imageColorBlender_activatedSkillOrStatusEffectNamePanel == null)
                return;

            if (m_text_activatedSkillOrStatusEffectName == null)
                m_text_activatedSkillOrStatusEffectName = m_imageColorBlender_activatedSkillOrStatusEffectNamePanel.transform.Find("Text@ActivatedSkillOrStatusEffectName").GetComponent<Text>();
            if (m_text_activatedSkillOrStatusEffectName == null)
                return;

            if (m_tileMapManager.IsInitialized)
                IsInitialized = true;
        }

        IEnumerator AnimateLogEvent(int _indexOfLog)
        {
            m_playingAnimation = true;

            EventLog log = m_mainScript.EventLogs[_indexOfLog];

            if (!(log is EffectTrialLog))
                m_latestEffectGenerationPointId = default;

            if (log is AutomaticEventLog)
            {
                if (log is TurnChangeEventLog)
                {
                    var detailedLog = log as TurnChangeEventLog;

                    string message = (detailedLog.TurnInitiatingPlayerId == m_mainScript.PlayerController.PlayerId) ? "Your" : "Opponent's";

                    decimal nextEventTurn = detailedLog.EventTurn + 0.5m;
                    int playerTurn = (detailedLog.TurnInitiatingPlayerId == 1) ? nextEventTurn.Ceiling() : nextEventTurn.Floor();

                    message += " Turn\n<" + playerTurn + ">";

                    m_text_central.text = message;

                    Color color = m_text_central.color;
                    color.a = 1f;
                    m_text_central.color = color;

                    // Update SP graphic for both players
                    m_spDisplayer.UpdateSPGraphic(detailedLog.TurnEndingPlayerId - 1, detailedLog.RemainingSPForTurnEndingPlayer);
                    m_spDisplayer.UpdateSPGraphic(detailedLog.TurnInitiatingPlayerId - 1, detailedLog.RemainingSPForTurnInitiatingPlayer);

                    yield return StartCoroutine(VanishCentralText(3f));
                }
                else if (log is EffectTrialLog)
                {
                    var effectLog = log as EffectTrialLog;

                    if (effectLog.DidActivate)
                    {
                        if (effectLog.AnimationInfo is ProjectileAnimationInfo || effectLog.AnimationInfo is LaserAnimationInfo)
                        {
                            int effectGenerationPointId;
                            if (effectLog.AnimationInfo is ProjectileAnimationInfo)
                                effectGenerationPointId = (effectLog.AnimationInfo as ProjectileAnimationInfo).ProjectileGenerationPointId;
                            else //if (effectLog.AnimationInfo is LaserAnimationInfo)
                                effectGenerationPointId = (effectLog.AnimationInfo as LaserAnimationInfo).LaserGenerationPointId;

                            if (effectGenerationPointId != m_latestEffectGenerationPointId)
                            {
                                int effectGenerationPointPrefabIndex = effectGenerationPointId - 1;
                                m_go_effectGenerationPoint = Instantiate(EffectPrefabContainer.Instance.EffectGenerationPointPrefabs[effectGenerationPointPrefabIndex], m_transform_actorUnitChip.position + m_effectGenerationPointHeight, Quaternion.identity);

                                yield return new WaitForSecondsRealtime(2f);

                                m_latestEffectGenerationPointId = effectGenerationPointId;
                            }
                        }

                        if (log is EffectTrialLog_UnitTargetingEffect)
                        {
                            var unitTargetingEffectLog = log as EffectTrialLog_UnitTargetingEffect;

                            Transform transform_targetUnitChip = m_mainScript.GOs_Unit[unitTargetingEffectLog.TargetId].transform;
                            MeshCollider meshCollider_targetUnitChip = null;
                            if (effectLog.AnimationInfo is ProjectileAnimationInfo || effectLog.AnimationInfo is LaserAnimationInfo)
                            {
                                meshCollider_targetUnitChip = transform_targetUnitChip.Find("Frame").GetComponent<MeshCollider>();
                                meshCollider_targetUnitChip.enabled = true; // Activate target unit chip's collider temporarily
                            }

                            if (effectLog.AnimationInfo == null) // Meaning that it is the basic damage effect
                            {
                                Instantiate(EffectPrefabContainer.Instance.BasicAttackHitEffectPrefab, transform_targetUnitChip.position, Quaternion.identity);
                                yield return new WaitForSecondsRealtime(1f);
                            }
                            else if (effectLog.AnimationInfo is SimpleAnimationInfo)
                            {
                                SimpleAnimationInfo simpleAnimationInfo = (effectLog.AnimationInfo as SimpleAnimationInfo);
                                Instantiate(EffectPrefabContainer.Instance.HitEffectPrefabs[simpleAnimationInfo.HitEffectId - 1], transform_targetUnitChip.position, Quaternion.identity);
                                yield return new WaitForSecondsRealtime(1f);
                            }
                            else if (effectLog.AnimationInfo is ProjectileAnimationInfo)
                            {
                                ProjectileAnimationInfo projectileAnimationInfo = (effectLog.AnimationInfo as ProjectileAnimationInfo);
                                ProjectileLauncher projectileLauncher = m_go_effectGenerationPoint.GetComponent<ProjectileLauncher>();
                                projectileLauncher.ProjectilePrefab = EffectPrefabContainer.Instance.ProjectilePrefabs[projectileAnimationInfo.ProjectileId - 1];
                                projectileLauncher.GenerateProjectiles(1, EffectPrefabContainer.Instance.HitEffectPrefabs[projectileAnimationInfo.HitEffectId - 1]);
                                projectileLauncher.ProjectileForce = m_projectileForce;
                                projectileLauncher.LaunchProjectile(transform_targetUnitChip);
                                yield return new WaitForSecondsRealtime(1f);
                            }
                            else if (effectLog.AnimationInfo is LaserAnimationInfo)
                            {
                                LaserAnimationInfo laserAnimationInfo = (effectLog.AnimationInfo as LaserAnimationInfo);
                                LaserLauncher laserLauncher = m_go_effectGenerationPoint.GetComponent<LaserLauncher>();
                                laserLauncher.LaserEffectPrefab = EffectPrefabContainer.Instance.LaserEffectPrefabs[laserAnimationInfo.LaserEffectId - 1];
                                laserLauncher.GenerateLasers(1);
                                laserLauncher.LaunchLaser(transform_targetUnitChip);
                                yield return new WaitForSecondsRealtime(1f);
                                Instantiate(EffectPrefabContainer.Instance.HitEffectPrefabs[laserAnimationInfo.HitEffectId - 1], transform_targetUnitChip.position, Quaternion.identity);
                            }
                            else if (effectLog.AnimationInfo is MovementAnimationInfo)
                            {
                                MovementAnimationInfo movementAnimationInfo = (effectLog.AnimationInfo as MovementAnimationInfo);
                                m_go_attachmentEffect = Instantiate(EffectPrefabContainer.Instance.AttachmentEffectPrefabs[movementAnimationInfo.AttachmentEffectId - 1], transform_targetUnitChip.Find("Frame"));
                                Destroy(m_go_skillActivationEffect);
                                m_go_skillActivationEffect = null;
                                yield return new WaitForSecondsRealtime(1f);
                            }

                            if (meshCollider_targetUnitChip != null)
                                meshCollider_targetUnitChip.enabled = false; // Deactivate target unit chip's collider

                            if (log is EffectTrialLog_DamageEffect)
                            {
                                var detailedLog = log as EffectTrialLog_DamageEffect;

                                if (detailedLog.DidSucceed)
                                    yield return StartCoroutine(DamagedAnimation(transform_targetUnitChip));

                                Transform transform_hpBar = transform_targetUnitChip?.transform.Find("HPBar");
                                if (transform_hpBar == null)
                                    yield break;

                                if (transform_hpBar.parent.parent.name == "Player")
                                {
                                    HPBarController_PlayerUnit hpBarController = transform_hpBar?.GetComponent<HPBarController_PlayerUnit>();
                                    hpBarController.TryUpdateHP(detailedLog.Value * -1, detailedLog.DidSucceed, detailedLog.WasCritical, detailedLog.RemainingHPAfterModification);
                                }
                                else if (transform_hpBar.parent.parent.name == "Opponent")
                                {
                                    HPBarController_OpponentUnit hpBarController = transform_hpBar?.GetComponent<HPBarController_OpponentUnit>();
                                    hpBarController.TryUpdateHP(detailedLog.Value * -1, detailedLog.DidSucceed, detailedLog.WasCritical, detailedLog.RemainingHPAfterModification);
                                }

                                if (detailedLog.RemainingHPAfterModification == 0 & !m_transforms_defeatedUnitChip.Contains(transform_targetUnitChip)) // If the unit's HP is zero and its unit chip has not been added to transforms_defeatedUnitChip
                                    m_transforms_defeatedUnitChip.Add(transform_targetUnitChip);
                            }
                            else if (log is EffectTrialLog_HealEffect)
                            {
                                var detailedLog = log as EffectTrialLog_HealEffect;

                                //Instantiate(EffectPrefabContainer.Instance.SimpleEffectPrefabs[detailedLog.TargetId], position, Quaternion.identity);

                                yield return new WaitForSecondsRealtime(2f);

                                Transform transform_hpBar = transform_targetUnitChip?.transform.Find("HPBar");
                                if (transform_hpBar == null)
                                    yield break;

                                HPBarController_SinglePlayer hpBarController = transform_hpBar?.GetComponent<HPBarController_SinglePlayer>();
                                hpBarController.TryUpdateHP(detailedLog.Value * -1, detailedLog.DidSucceed, detailedLog.WasCritical, detailedLog.RemainingHPAfterModification);
                            }
                            else if (log is EffectTrialLog_StatusEffectAttachmentEffect)
                            {
                                var detailedLog = log as EffectTrialLog_StatusEffectAttachmentEffect;

                                //Instantiate(EffectPrefabContainer.Instance.SimpleEffectPrefabs[detailedLog.AnimationInfo - 1], position, Quaternion.identity);

                                yield return new WaitForSecondsRealtime(4f);
                            }
                        }
                        else if (log is EffectTrialLog_TileTargetingEffect)
                        {
                            var tileTargetingEffectLog = log as EffectTrialLog_TileTargetingEffect;

                            if (log is EffectTrialLog_MovementEffect)
                            {
                                var detailedLog = log as EffectTrialLog_MovementEffect;

                                Vector3 sliding = m_mainScript.GetActualPosition(tileTargetingEffectLog.TargetCoord);

                                yield return StartCoroutine(m_transform_actorUnitChip.MoveInSeconds(sliding, false, 0.3f));

                                Destroy(m_go_attachmentEffect);
                                m_go_attachmentEffect = null;
                            }
                        }
                    }
                    //Instantiate(EffectParticlePrefabs[effectLog.AnimationId], position, Quaternion.identity);

                    yield return new WaitForSeconds(1);
                }
            }
            else if (log is ActionLog)
            {
                var actionLog = log as ActionLog;

                m_transform_actorUnitChip = m_mainScript.GOs_Unit[actionLog.ActorId].transform;

                if (log is ActionLog_Move)
                {
                    var detailedLog = log as ActionLog_Move;

                    Vector3 elevation = new Vector3(0, 5, 0);
                    Vector3 sliding = m_mainScript.GetActualPosition(detailedLog.EventualCoord) + elevation;

                    yield return StartCoroutine(m_transform_actorUnitChip.MoveInSeconds(elevation, true, 0.5f));
                    yield return StartCoroutine(m_transform_actorUnitChip.MoveInSeconds(sliding, false, 0.5f));
                    yield return StartCoroutine(m_transform_actorUnitChip.MoveInSeconds(-elevation, true, 0.05f));
                }
                else if (log is ActionLog_Attack)
                {
                    m_go_skillActivationEffect = Instantiate(EffectPrefabContainer.Instance.BasicAttackActivationEffectPrefab, m_transform_actorUnitChip.position, Quaternion.identity);
                    yield return new WaitForSecondsRealtime(0.75f);
                }
                else if (log is ActionLog_Skill)
                {
                    var detailedLog = log as ActionLog_Skill;

                    ShowActivatedSkillOrStatusEffectName(detailedLog.SkillName);
                    yield return null; // Pause method for one frame so that the UI remains with the alpha value of 1 during this frame
                    yield return StartCoroutine(VanishActivatedSkillOrStatusEffectName(1.5f));

                    int actorOwnerPlayerIndex = m_transform_actorUnitChip.parent.GetComponent<PlayerController_SinglePlayer>().PlayerId - 1;

                    m_spDisplayer.UpdateSPGraphic(actorOwnerPlayerIndex, detailedLog.RemainingSPAfterUsingSkill); // Update SP graphic before the actual animation
                    m_go_skillActivationEffect = Instantiate(EffectPrefabContainer.Instance.SkillActivationEffectPrefabs[detailedLog.AnimationId - 1], m_transform_actorUnitChip.position, Quaternion.identity);
                    yield return new WaitForSecondsRealtime(0.75f);
                }
            }

            m_teamStatusDisplayer.TryUpdateTeamStatus();

            if (m_go_skillActivationEffect != null || m_go_effectGenerationPoint != null)
            {
                int numOfLogs = m_mainScript.EventLogs.Count;
                bool containsEffectTrialLogForCurrentlyActiveSkill = false; // Variable to identify whether any effect trial log corresponding to the previously activated skill exist
                bool willGenerationPointPrefabChange = true;
                if (_indexOfLog != numOfLogs - 1) // If it is not the last log
                {
                    EventLog nextLog = m_mainScript.EventLogs[_indexOfLog + 1];

                    containsEffectTrialLogForCurrentlyActiveSkill = nextLog is EffectTrialLog; // If next log is an effect trial log, it means that the current skill has not ended
                    if (containsEffectTrialLogForCurrentlyActiveSkill)
                    {
                        EffectTrialLog proceedingEffectTrialLog = nextLog as EffectTrialLog;

                        int nextEffectGenerationPointId = default;
                        if (proceedingEffectTrialLog.AnimationInfo is ProjectileAnimationInfo)
                            nextEffectGenerationPointId = (proceedingEffectTrialLog.AnimationInfo as ProjectileAnimationInfo).ProjectileGenerationPointId;
                        else if (proceedingEffectTrialLog.AnimationInfo is LaserAnimationInfo)
                            nextEffectGenerationPointId = (proceedingEffectTrialLog.AnimationInfo as LaserAnimationInfo).LaserGenerationPointId;

                        willGenerationPointPrefabChange = (m_latestEffectGenerationPointId == nextEffectGenerationPointId);
                    }
                }

                if (!containsEffectTrialLogForCurrentlyActiveSkill) // If the effect animation was the last one for the skill
                {
                    if (m_transforms_defeatedUnitChip.Count > 0) // If any unit has been defeated
                    {
                        foreach (Transform transform_unitChip in m_transforms_defeatedUnitChip)
                        {
                            yield return StartCoroutine(DefeatedAnimation(transform_unitChip)); // Animate defeat
                        }

                        m_transforms_defeatedUnitChip.Clear(); // There are no more units required to animate its defeat
                    }

                    if (m_go_skillActivationEffect != null)
                    {
                        Destroy(m_go_skillActivationEffect);
                        m_go_skillActivationEffect = null;
                    }

                    if (m_go_effectGenerationPoint != null && willGenerationPointPrefabChange)
                    {
                        Destroy(m_go_effectGenerationPoint);
                        m_go_effectGenerationPoint = null;
                    }
                }
            }

            m_indexOfLastLog = _indexOfLog;

            m_playingAnimation = false;
        }

        IEnumerator VanishCentralText(float _seconds)
        {
            Color color = m_text_central.color;

            float timeElapsed = 0f;

            while (timeElapsed < _seconds)
            {
                color.a = 1f - (timeElapsed / _seconds);
                m_text_central.color = color;

                Debug.Log("Time Elapsed: " + timeElapsed.ToString() + "; Text Alpha: " + m_text_central.color.a.ToString());

                timeElapsed += Time.deltaTime;

                yield return null;
            }

            color.a = 0;
            m_text_central.color = color;
        }

        private void ShowActivatedSkillOrStatusEffectName(string _skillName)
        {
            m_text_activatedSkillOrStatusEffectName.text = _skillName;
            SkillData skillData = GameDataContainer.Instance.Skills.First(x => x.Name == _skillName);

            m_imageColorBlender_activatedSkillOrStatusEffectNamePanel.Color = (skillData is OrdinarySkillData) ? Color.red : (skillData is CounterSkillData) ? Color.yellow : Color.magenta; // Magenta if UltimateSkillData

            Color color_text = m_text_activatedSkillOrStatusEffectName.color;
            Color color_image = m_image_activatedSkillOrStatusEffectNamePanel.color;

            // Make the UI visible
            color_text.a = 1f;
            color_image.a = 1f;

            m_image_activatedSkillOrStatusEffectNamePanel.color = color_image;
            m_text_activatedSkillOrStatusEffectName.color = color_text;
        }

        IEnumerator VanishActivatedSkillOrStatusEffectName(float _seconds)
        {
            Color color_text = m_text_activatedSkillOrStatusEffectName.color;
            Color color_image = m_image_activatedSkillOrStatusEffectNamePanel.color;

            float timeElapsed = 0f;

            while (timeElapsed < _seconds)
            {
                color_text.a = 1f - (timeElapsed / _seconds);
                color_image.a = 1f - (timeElapsed / _seconds);

                m_image_activatedSkillOrStatusEffectNamePanel.color = color_image;
                m_text_activatedSkillOrStatusEffectName.color = color_text;

                //Debug.Log("Time Elapsed: " + timeElapsed.ToString() + "; Text Alpha: " + m_text_central.color.a.ToString());

                timeElapsed += Time.deltaTime;

                yield return null;
            }

            color_image.a = 0;
            color_text.a = 0;

            m_image_activatedSkillOrStatusEffectNamePanel.color = color_image;
            m_text_activatedSkillOrStatusEffectName.color = color_text;
        }

        IEnumerator DamagedAnimation(Transform _unitChip)
        {
            MeshRenderer[] meshRenderers = _unitChip.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < 3; i++)
            {
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    meshRenderer.enabled = false;
                }

                yield return new WaitForSecondsRealtime(0.1f);

                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    meshRenderer.enabled = true;
                }

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        IEnumerator DefeatedAnimation(Transform _unitChip)
        {
            bool isOwnerPlayer1 = _unitChip.parent.GetComponent<PlayerController_SinglePlayer>().PlayerId == 1;
            Vector3 tile1Position = m_mainScript.GetActualPosition(new _2DCoord(0, 0));
            Vector3 tile2Position = m_mainScript.GetActualPosition(new _2DCoord(0, 1));
            float distanceBetweenTwoTiles = Vector3.Distance(tile1Position, tile2Position);

            Vector3 motionReferencePoint = Vector3.zero;
            if (isOwnerPlayer1)
            {
                Vector3 nearestTile = m_mainScript.GetActualPosition(new _2DCoord(CoreValues.SIZE_OF_A_SIDE_OF_BOARD.Middle() - 1, 0));

                motionReferencePoint = new Vector3(_unitChip.position.x, _unitChip.position.y, nearestTile.z - distanceBetweenTwoTiles * 2.5f);
            }
            else
            {
                Vector3 nearestTile = m_mainScript.GetActualPosition(new _2DCoord(CoreValues.SIZE_OF_A_SIDE_OF_BOARD.Middle() - 1, CoreValues.SIZE_OF_A_SIDE_OF_BOARD - 1));

                motionReferencePoint = new Vector3(_unitChip.position.x, _unitChip.position.y, nearestTile.z + distanceBetweenTwoTiles * 2.5f);
            }

            float motionVertex_z = (motionReferencePoint.z + _unitChip.position.z) / 2;
            Vector3 motionVertex = new Vector3(motionReferencePoint.x, motionReferencePoint.y + 20f, motionVertex_z);

            StartCoroutine(_unitChip.ParabolicMotionInSeconds(eAxis.Y, motionVertex, false, 2.5f, 2f));
            yield return new WaitForSecondsRealtime(0.1f);
            yield return StartCoroutine(_unitChip.RotateInSeconds(new Vector3(-360 * 10, 0, 0), 1.5f));
        }

        public IEnumerator FadeMatchResultPanelIn(bool _isPlayerWinner)
        {
            while (m_playingAnimation
                || m_indexOfLastLog < m_mainScript.EventLogs.Count - 1) // While any other animation is being processed or is to be processed
            {
                yield return null; // Wait
            }

            m_playingAnimation = true;

            yield return new WaitForSecondsRealtime(2f);

            Text text_result = m_matchResultPanel.Find("Text@Result").GetComponent<Text>();
            text_result.text = _isPlayerWinner ? "VICTORY" : "DEFEAT";
            text_result.color = _isPlayerWinner ? Color.yellow : Color.grey;

            Text text_secondsTillSceneChange = m_matchResultPanel.Find("Text@SecondsTillSceneChange").GetComponent<Text>();
            text_secondsTillSceneChange.text = "Rewards in ";

            m_matchResultPanel.gameObject.SetActive(true);

            for (int i = m_secondsTillSceneChange; i > 0; i--)
            {
                text_secondsTillSceneChange.text = "Rewards in " + i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }

            m_playingAnimation = false;
            SceneConnector.GoToScene("scn_DungeonRewards");
        }
    }
}