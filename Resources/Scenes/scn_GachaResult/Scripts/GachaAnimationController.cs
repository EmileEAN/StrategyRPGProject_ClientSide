using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using EEANWorks.Games.Unity.Engine;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(InfoPanelManager_Unit))]
    [RequireComponent(typeof(InfoPanelManager_Weapon))]
    [RequireComponent(typeof(InfoPanelManager_Armour))]
    [RequireComponent(typeof(InfoPanelManager_Accessory))]
    [RequireComponent(typeof(InfoPanelManager_Item))]
    [RequireComponent(typeof(InfoPanelManager_Unit))]
    [RequireComponent(typeof(GachaRollRequester))]
    public class GachaAnimationController : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_capsuleWithObjectPrefab;

        [SerializeField]
        private int m_numOfFramesForNormalAnimationLoop;
        [SerializeField]
        private Color m_initialColorForEpicAnimation;
        [SerializeField]
        private Color m_finalColorForEpicAnimation;
        [SerializeField]
        private int m_numOfFramesForEpicAnimation;
        [SerializeField]
        private int m_numOfFramesBetweenEpicAnimation;
        [SerializeField]
        private int m_rGBShiftPerFrame_LegendaryAnimation;
        [SerializeField]
        private int m_rGBShiftBetweenLetters_LegendaryAnimation;

        [SerializeField]
        private List<Vector3IntPair> m_rotationOfHandleAndFramesToEndRotation;

        [SerializeField]
        private int m_numOfFramesForCapsuleToFall;

        [SerializeField]
        private int m_numOfIdleFramesBeforeShrinkingCapsule;

        [SerializeField]
        private int m_numOfFramesForCapsuleShrinking;
        [SerializeField]
        private float m_relativeSizeOfCapsuleAfterShrinking;

        [SerializeField]
        private float m_secondsBetweenCapsuleOpening;

        [SerializeField]
        private float m_secondsBeforeShowingOptionsMenu;

        [SerializeField]
        private GameObject m_startButtonGO;

        [SerializeField]
        private Transform m_gachaMachineTransform;

        [SerializeField]
        private Transform m_threeObjectsLeftTransform;
        [SerializeField]
        private Transform m_twoObjectsLeftTransform;
        [SerializeField]
        private Transform m_threeObjectsRightTransform;
        [SerializeField]
        private Transform m_twoObjectsRightTransform;

        [SerializeField]
        private GameObject m_optionsPanelGO;
        #endregion

        #region Private Fields
        private Gacha m_gacha;
        private DispensationOption m_dispensationOption;
        private IList<object> m_gachaResultObjects;

        private InfoPanelManager_Unit m_infoPanelManager_unit;
        private InfoPanelManager_Weapon m_infoPanelManager_weapon;
        private InfoPanelManager_Armour m_infoPanelManager_armour;
        private InfoPanelManager_Accessory m_infoPanelManager_accessory;
        private InfoPanelManager_Item m_infoPanelManager_item;
        private GachaRollRequester m_gachaRollRequester;

        private Text m_text_startButton;
        private string m_startButtonDefaultString;
        private Color32 m_color_firstLetterOfStartButton;
        private float m_startButtonTransparencyShiftPerFrame;
        private bool m_startButtonTransparencyTowardsZero;
        private int m_remainingFrames_epicAnimation;
        private int m_remainingFrames_epicAnimationInterval;

        private int m_currentDispensationIndex;
        private bool m_animatingDispensation;

        private RectTransform m_rt_handle;
        private bool m_animatingHandleRotation;
        private List<Vector3> m_list_rotationPerFrame_handle;
        private int m_remainingFrames_handleRotation;
        private int m_rotationsCompleted;

        private Transform m_transform_capsulePath;
        private RectTransform m_rt_capsuleReferencePoint_top;
        private RectTransform m_rt_capsuleReferencePoint_bottom;
        private bool m_animatingCapsuleFalling;
        private Vector2 m_capsuleMovementPerFrame;
        private int m_remainingFrames_capsuleFalling;
        private List<Transform> m_transforms_capsule;
        private List<RectTransform> m_rts_capsule;
        private List<Image> m_images_capsule;
        private List<AdvancedButton> m_buttons_object;

        private int m_remainingFrames_idleBeforeCapsuleShrinking;

        private bool m_animatingCapsuleShrinking;
        private float m_capsuleShrinkingMultiplierPerFrame;
        private int m_remainingFrames_capsuleShrinking;

        private bool m_openCapsules;

        private bool m_isThereAnyEpicObject;
        private bool m_isThereAnyLegendaryObject;
        private bool m_animatingStartButton_normal;
        private bool m_animatingStartButton_epic;
        private bool m_animatingStartButton_legendary;

        private bool m_isInitialized;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_gacha = GameDataContainer.Instance.GachaRolled;
            m_dispensationOption = GameDataContainer.Instance.DispensationOptionSelected;
            m_gachaResultObjects = GameDataContainer.Instance.GachaResultObjects;

            m_infoPanelManager_unit = this.GetComponent<InfoPanelManager_Unit>();
            m_infoPanelManager_weapon = this.GetComponent<InfoPanelManager_Weapon>();
            m_infoPanelManager_armour = this.GetComponent<InfoPanelManager_Armour>();
            m_infoPanelManager_accessory = this.GetComponent<InfoPanelManager_Accessory>();
            m_infoPanelManager_item = this.GetComponent<InfoPanelManager_Item>();
            m_gachaRollRequester = this.GetComponent<GachaRollRequester>();
        }

        // FixedUpdate is called once per frame
        void FixedUpdate()
        {
            if (!m_isInitialized)
                Initialize();
            else
            {
                if (!m_animatingDispensation)
                {
                    if (m_animatingStartButton_normal)
                        AnimateStartButton_Normal();
                    else if (m_animatingStartButton_epic)
                        AnimateStartButton_Epic();
                    else if (m_animatingStartButton_legendary)
                        AnimateStartButton_Legendary();
                }
                else
                {
                    if (m_animatingHandleRotation)
                        AnimateHandleRotation();

                    if (m_animatingCapsuleFalling)
                        AnimateCapsuleFalling();

                    if (m_animatingCapsuleShrinking)
                        AnimateCapsuleShrinking();

                    if (m_openCapsules)
                        StartCoroutine(OpenCapsules());
                }
            }
        }

        private void Initialize()
        {
            if (m_isInitialized || !DynamicGridLayoutGroup.AreAllInitialized)
                return;

            try
            {
                m_currentDispensationIndex = 0;
                m_animatingDispensation = false;

                m_text_startButton = m_startButtonGO.transform.Find("Text").GetComponent<Text>();
                //----------------------------------
                #region Platform-based Code
#if UNITY_STANDALONE
                m_startButtonDefaultString = "Click";
#endif
#if UNITY_IOS || UNITY_ANDROID
            m_startButtonDefaultString = "Tap";
#endif
                #endregion
                //----------------------------------
                m_startButtonTransparencyShiftPerFrame = 2f / m_numOfFramesForNormalAnimationLoop;
                m_startButtonTransparencyTowardsZero = true;
                m_remainingFrames_epicAnimation = 0;
                m_remainingFrames_epicAnimationInterval = 0;
                m_startButtonGO.SetActive(false);

                m_optionsPanelGO.SetActive(false);

                // Set values for handle animation
                m_rt_handle = m_gachaMachineTransform.Find("Panel@Handle").Find("Image@Handle").GetComponent<RectTransform>();
                m_animatingHandleRotation = false;
                m_list_rotationPerFrame_handle = new List<Vector3>();
                foreach (Vector3IntPair rotationAndNumOfFrames in m_rotationOfHandleAndFramesToEndRotation)
                {
                    m_list_rotationPerFrame_handle.Add(rotationAndNumOfFrames.Key / rotationAndNumOfFrames.Value);
                }
                m_remainingFrames_handleRotation = 0;
                m_rotationsCompleted = 0;

                // Set values for capsule animation
                m_transform_capsulePath = m_gachaMachineTransform.Find("CapsulePath");
                m_rt_capsuleReferencePoint_top = m_transform_capsulePath.Find("Top").Find("ReferencePoint").GetComponent<RectTransform>();
                m_rt_capsuleReferencePoint_bottom = m_transform_capsulePath.Find("Bottom").Find("ReferencePoint").GetComponent<RectTransform>();
                m_animatingCapsuleFalling = false;
                Vector2 capsuleMovementDistance = (m_rt_capsuleReferencePoint_bottom.anchorMin - m_rt_capsuleReferencePoint_top.anchorMin) * new Vector2(0, 1);
                m_capsuleMovementPerFrame = capsuleMovementDistance / m_numOfFramesForCapsuleToFall;
                m_remainingFrames_capsuleFalling = 0;
                m_transforms_capsule = new List<Transform>();
                m_rts_capsule = new List<RectTransform>();
                m_images_capsule = new List<Image>();
                m_buttons_object = new List<AdvancedButton>();

                m_remainingFrames_idleBeforeCapsuleShrinking = 0;

                m_animatingCapsuleShrinking = false;
                m_capsuleShrinkingMultiplierPerFrame = (float)(Math.Pow(m_relativeSizeOfCapsuleAfterShrinking, 1f / m_numOfFramesForCapsuleShrinking));
                m_remainingFrames_capsuleShrinking = 0;

                m_openCapsules = false;

                // Instantiate Capsules With Objects
                foreach (object gachaResultObject in m_gachaResultObjects)
                {
                    Transform transform_capsule;
                    transform_capsule = Instantiate(m_capsuleWithObjectPrefab, m_transform_capsulePath).transform;
                    m_transforms_capsule.Add(transform_capsule);
                    RectTransform rt_capsule = transform_capsule.GetComponent<RectTransform>();
                    m_rts_capsule.Add(rt_capsule);

                    // Set the initial position of the capsule
                    rt_capsule.anchorMin = m_rt_capsuleReferencePoint_top.anchorMin;
                    rt_capsule.anchorMax = m_rt_capsuleReferencePoint_top.anchorMax;

                    Image image_capsule = transform_capsule.Find("Image@Capsule").GetComponent<Image>();
                    m_images_capsule.Add(image_capsule);
                    eRarity rarity = default;

                    Transform transform_objectButton = transform_capsule.Find("Button@Object");
                    RectTransform rt_objectButton = transform_objectButton.GetComponent<RectTransform>();
                    Vector2 resizingVector = new Vector2(0.495f, 0.495f);
                    rt_objectButton.anchorMin = resizingVector;
                    rt_objectButton.anchorMax = Vector2.one - resizingVector;
                    GameObjectFormatter_ObjectButton goFormatter_objectButton = transform_objectButton.GetComponent<GameObjectFormatter_ObjectButton>();
                    m_buttons_object.Add(goFormatter_objectButton.Button_Object);

                    switch (m_gacha.GachaClassification)
                    {
                        default: // case eGachaClassification.Unit
                            {
                                Unit unit = gachaResultObject as Unit;
                                rarity = unit.BaseInfo.Rarity;
                                goFormatter_objectButton.Format(unit, null, null, () => m_infoPanelManager_unit.InstantiateInfoPanel(unit, true));
                            }
                            break;
                        case eGachaClassification.Weapon:
                            {
                                Weapon weapon = gachaResultObject as Weapon;
                                rarity = weapon.BaseInfo.Rarity;
                                goFormatter_objectButton.Format(weapon, null, null, () => m_infoPanelManager_weapon.InstantiateInfoPanel(weapon));
                            }
                            break;
                        case eGachaClassification.Armour:
                            {
                                Armour armour = gachaResultObject as Armour;
                                rarity = armour.BaseInfo.Rarity;
                                goFormatter_objectButton.Format(armour, null, null, () => m_infoPanelManager_armour.InstantiateInfoPanel(armour));
                            }
                            break;
                        case eGachaClassification.Accessory:
                            {
                                Accessory accessory = gachaResultObject as Accessory;
                                rarity = accessory.BaseInfo.Rarity;
                                goFormatter_objectButton.Format(accessory, null, null, () => m_infoPanelManager_accessory.InstantiateInfoPanel(accessory));
                            }
                            break;
                        case eGachaClassification.SkillItem:
                        case eGachaClassification.SkillMaterial:
                        case eGachaClassification.ItemMaterial:
                        case eGachaClassification.EquipmentMaterial:
                        case eGachaClassification.EvolutionMaterial:
                        case eGachaClassification.WeaponEnhancementMaterial:
                        case eGachaClassification.UnitEnhancementMaterial:
                        case eGachaClassification.SkillEnhancementMaterial:
                            {
                                Item item = gachaResultObject as Item;
                                rarity = item.Rarity;
                                goFormatter_objectButton.Format(item, null, null, () => m_infoPanelManager_item.InstantiateInfoPanel(item));
                            }
                            break;
                    }

                    // Set interactable to false while the capsule is closed
                    goFormatter_objectButton.Button_Object.interactable = false;

                    if (rarity == eRarity.Epic)
                        m_isThereAnyEpicObject = true;
                    else if (rarity == eRarity.Legendary)
                    {
                        m_isThereAnyEpicObject = true;
                        m_isThereAnyLegendaryObject = true;
                    }

                    int rarityIndex = (Convert.ToInt32(rarity) / CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES) - 1;
                    image_capsule.sprite = SpriteContainer.Instance.CapsuleSprites[rarityIndex];
                }

                m_startButtonGO.SetActive(true);
                AnimateStartButton();

                m_isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private void AnimateStartButton()
        {
            if (m_isThereAnyLegendaryObject) // Initialize start button with legendary animation settings
            {
                m_animatingStartButton_legendary = true;

                m_color_firstLetterOfStartButton = Color.cyan;
                AnimateStartButton_Legendary(); // Initialize
            }
            else if (m_isThereAnyEpicObject) // Initialize start button with epic animation
            {
                m_animatingStartButton_epic = true;

                m_remainingFrames_epicAnimation = m_numOfFramesForEpicAnimation;
                m_remainingFrames_epicAnimationInterval = 0;
                AnimateStartButton_Epic(); // Initialize
            }
            else // Initialize start button with normal animation
            {
                m_animatingStartButton_normal = true;

                AnimateStartButton_Normal(); // Initialize
            }
        }

        private void AnimateStartButton_Normal()
        {
            Color color = m_text_startButton.color;

            if (m_startButtonTransparencyTowardsZero) // color.a -> 0
            {
                if (color.a == 0)
                {
                    m_startButtonTransparencyTowardsZero = false;
                    color.a += m_startButtonTransparencyShiftPerFrame;
                }
                else
                {
                    color.a -= m_startButtonTransparencyShiftPerFrame;
                    if (color.a < 0)
                        color.a = 0;
                }
            }
            else // color.a -> 1
            {
                if (color.a == 1)
                {
                    m_startButtonTransparencyTowardsZero = true;
                    color.a -= m_startButtonTransparencyShiftPerFrame;
                }
                else
                {
                    color.a += m_startButtonTransparencyShiftPerFrame;
                    if (color.a > 1)
                        color.a = 1;
                }
            }

            m_text_startButton.color = color;
        }

        private void AnimateStartButton_Epic()
        {
            if (m_remainingFrames_epicAnimationInterval > 0)
                m_remainingFrames_epicAnimationInterval--;
            else if (m_remainingFrames_epicAnimationInterval == 0 && m_remainingFrames_epicAnimation == 0)
                m_remainingFrames_epicAnimation = m_numOfFramesForEpicAnimation;

            if (m_remainingFrames_epicAnimation > 0)
            {
                // Set different color for each letter
                string text = string.Empty;
                for (int i = 0; i < m_startButtonDefaultString.Length; i++)
                {
                    Color32 color = m_color_firstLetterOfStartButton.RGBShift(ePointToPointGradient.Loop, m_initialColorForEpicAnimation, m_finalColorForEpicAnimation, m_startButtonDefaultString.Length, 1);

                    text += m_startButtonDefaultString[i].IntoColorTag(color);

                    if (i == 1)
                        m_color_firstLetterOfStartButton = color; // Update color for next call
                }

                m_text_startButton.text = text; // Update text

                m_remainingFrames_epicAnimation--;

                if (m_remainingFrames_epicAnimation == 0)
                    m_remainingFrames_epicAnimationInterval = m_numOfFramesBetweenEpicAnimation;
            }
        }

        private void AnimateStartButton_Legendary()
        {
            // Set different color for each letter.
            string text = string.Empty;
            for (int i = 0; i < m_startButtonDefaultString.Length; i++)
            {
                Color32 color = m_color_firstLetterOfStartButton.RGBShift(eFixedGradient._1536Colors, m_rGBShiftBetweenLetters_LegendaryAnimation * i, false); // Move color shifting from left to right

                text += m_startButtonDefaultString[i].IntoColorTag(color);
            }

            m_text_startButton.text = text; // Update text

            m_color_firstLetterOfStartButton = m_color_firstLetterOfStartButton.RGBShift(eFixedGradient._1536Colors, m_rGBShiftPerFrame_LegendaryAnimation, true); // Update color for next call
        }

        public void AnimateDispensation()
        {
            if (!m_animatingDispensation)
            {
                m_startButtonGO.SetActive(false);
                m_animatingStartButton_normal = m_animatingStartButton_epic = m_animatingStartButton_legendary = false;

                m_animatingDispensation = true;
                m_animatingHandleRotation = true; // Set first animation
                m_rotationsCompleted = 0;

                if (m_rotationOfHandleAndFramesToEndRotation.Count > 0)
                    m_remainingFrames_handleRotation = m_rotationOfHandleAndFramesToEndRotation[0].Value; // Set the number of remaining frames to match the number of frames required for the first rotation
            }
        }

        private void AnimateHandleRotation()
        {
            if (m_remainingFrames_handleRotation > 0)
            {
                int currentRotationIndex = m_rotationsCompleted;
                m_rt_handle.Rotate(m_list_rotationPerFrame_handle[currentRotationIndex]); // Rotate based on the values for currently specified rotation

                m_remainingFrames_handleRotation--;
            }

            if (m_remainingFrames_handleRotation == 0)
            {
                m_rotationsCompleted++;
                if (m_rotationsCompleted == m_list_rotationPerFrame_handle.Count) // All rotations have been completed
                {
                    m_animatingHandleRotation = false; // Terminate handle rotation animation

                    m_remainingFrames_capsuleFalling = m_numOfFramesForCapsuleToFall;
                    m_animatingCapsuleFalling = true; // Start next animation
                }
                else // There is at least one unexecuted rotation specified in the list
                {
                    int nextRotationIndex = m_rotationsCompleted;
                    m_remainingFrames_handleRotation = m_rotationOfHandleAndFramesToEndRotation[nextRotationIndex].Value;
                }
            }
        }

        private void AnimateCapsuleFalling()
        {
            if (m_remainingFrames_capsuleFalling > 0)
            {
                m_rts_capsule[m_currentDispensationIndex].anchorMin += m_capsuleMovementPerFrame;
                m_rts_capsule[m_currentDispensationIndex].anchorMax += m_capsuleMovementPerFrame;

                m_remainingFrames_capsuleFalling--;
            }

            if (m_remainingFrames_capsuleFalling == 0)
            {
                m_animatingCapsuleFalling = false; // Terminate capsule falling animation

                m_remainingFrames_idleBeforeCapsuleShrinking = m_numOfIdleFramesBeforeShrinkingCapsule;
                m_remainingFrames_capsuleShrinking = m_numOfFramesForCapsuleShrinking;
                m_animatingCapsuleShrinking = true; // Start next animation
            }
        }

        private void AnimateCapsuleShrinking()
        {
            if (m_remainingFrames_idleBeforeCapsuleShrinking > 0)
            {
                m_remainingFrames_idleBeforeCapsuleShrinking--;
                return;
            }

            if (m_remainingFrames_capsuleShrinking > 0)
            {
                RectTransform rt = m_rts_capsule[m_currentDispensationIndex];

                Vector2 size = rt.anchorMax - rt.anchorMin;
                Vector2 newSize = size * m_capsuleShrinkingMultiplierPerFrame;

                Vector2 paddingHalf = (size - newSize) / 2; // Get half of the available padding in order to maintain the capsule centered
                rt.anchorMin += paddingHalf;
                rt.anchorMax -= paddingHalf;

                m_remainingFrames_capsuleShrinking--;
            }

            if (m_remainingFrames_capsuleShrinking == 0)
            {
                m_animatingCapsuleShrinking = false; // Terminate capsule shrinking animation

                MoveCapsuleToSidePanel();

                m_currentDispensationIndex++;

                //Specify next animation
                if (m_currentDispensationIndex == m_gachaResultObjects.Count)
                    m_openCapsules = true;
                else
                {
                    m_remainingFrames_capsuleFalling = m_numOfFramesForCapsuleToFall; // Reset values for animation
                    m_animatingCapsuleFalling = true; // Start next animation
                }
            }
        }

        private void MoveCapsuleToSidePanel()
        {
            Transform capsule = m_transforms_capsule[m_currentDispensationIndex];

            int currentDispensationNum = m_currentDispensationIndex + 1;
            if (currentDispensationNum == 1 || currentDispensationNum == 3 || currentDispensationNum == 5)
                capsule.SetParent(m_threeObjectsLeftTransform);
            else if (currentDispensationNum == 2 || currentDispensationNum == 4)
                capsule.SetParent(m_twoObjectsLeftTransform);
            else if (currentDispensationNum == 6 || currentDispensationNum == 8 || currentDispensationNum == 10)
                capsule.SetParent(m_threeObjectsRightTransform);
            else if (currentDispensationNum == 7 || currentDispensationNum == 9)
                capsule.SetParent(m_twoObjectsRightTransform);
        }

        IEnumerator OpenCapsules()
        {
            m_openCapsules = false; // Prevent multiple calls

            for (int i = 0; i < m_images_capsule.Count; i++)
            {
                yield return new WaitForSecondsRealtime(m_secondsBetweenCapsuleOpening);

                Color color = m_images_capsule[i].color;
                color.a = 0;
                m_images_capsule[i].color = color; // Make the capsule image transparent

                Transform objectButton = m_transforms_capsule[i].Find("Button@Object");
                RectTransform rt = objectButton.GetComponent<RectTransform>(); // Resize the rect transform to match the parent size
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
            }

            foreach (AdvancedButton button in m_buttons_object)
            {
                button.interactable = true;
            }

            yield return new WaitForSecondsRealtime(m_secondsBeforeShowingOptionsMenu);

            m_animatingDispensation = false;

            PrepareAndDisplayOptionsPanel();
        }

        private void PrepareAndDisplayOptionsPanel()
        {
            //Debug.Log("PrepareAndDisplayOptionsPanel() called!");

            Button reRollButton = m_optionsPanelGO.transform.Find("Button@ReRoll").GetComponent<Button>();
            reRollButton.onClick.RemoveAllListeners();
            reRollButton.onClick.AddListener(() => GachaConfirmationPopUpCreator.CreatePopUp(m_gacha, m_gachaRollRequester, m_dispensationOption));

            m_optionsPanelGO.SetActive(true);
        }
    }
}