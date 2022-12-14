using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public class ApplicationInitializer : MonoBehaviour
    {
        #region Serialized Fields
        public List<Sprite> GenderIcons;
        public List<Sprite> ElementIcons;
        public List<Sprite> RaritySprites;
        public List<Sprite> WeaponClassificationIcons;
        public List<Sprite> ArmourClassificationIcons;
        public List<Sprite> AccessoryClassificationIcons;
        public List<Sprite> Element1FrameSprites;
        public List<Sprite> Element2FrameSprites;
        public List<Sprite> SkillIconBaseSprites;
        public List<Sprite> CapsuleSprites;
        public List<Sprite> TileSprites;

        public Sprite ObjectIconFrameSprite_Unit;
        public Sprite ObjectIconFrameSprite_NonUnit;
        public Sprite EmptySkillSlotIcon;
        public Sprite EmptyObjectSprite_NotSet;
        public Sprite EmptyObjectSprite_Set;
        public Sprite GemSprite;
        public Sprite GoldSprite;

        public Sprite NovelEmotionIcon_Amused;
        public Sprite NovelEmotionIcon_SpacedOut;
        public Sprite NovelEmotionIcon_Sleeping;
        public Sprite NovelEmotionIcon_Noticed;
        public Sprite NovelEmotionIcon_Shocked;
        public Sprite NovelEmotionIcon_Fun;
        public Sprite NovelEmotionIcon_Glitter;
        public Sprite NovelEmotionIcon_Flowers;
        public Sprite NovelEmotionIcon_Furious;
        public Sprite NovelEmotionIcon_Irritated;
        public Sprite NovelEmotionIcon_Uneasy;
        public Sprite NovelEmotionIcon_Silence;
        public Sprite NovelEmotionIcon_Idea;
        public Sprite NovelEmotionIcon_Yes;
        public Sprite NovelEmotionIcon_No;
        public Sprite NovelEmotionIcon_Heart;
        public Sprite NovelEmotionIcon_BrokenHeart;
        public Sprite NovelEmotionIcon_Exclamation;
        public Sprite NovelEmotionIcon_Question;
        public Sprite NovelEmotionIcon_ExclamationAndQuestion;
        public Sprite NovelEmotionIcon_Injured;
        public Sprite NovelEmotionIcon_Gloomy;
        public Sprite NovelEmotionIcon_Angry;
        public Sprite NovelEmotionIcon_Sweat;
        public Sprite NovelEmotionIcon_Droplets;
        public Sprite NovelEmotionIcon_Trembling;

        public List<Material> RarityMaterials;

        public GameObject BasicAttackActivationEffectPrefab;
        public GameObject BasicAttackHitEffectPrefab;
        public List<GameObject> SkillActivationEffectPrefabs;
        public List<GameObject> EffectGenerationPointPrefabs;
        public List<GameObject> ProjectilePrefabs;
        public List<GameObject> LaserEffectPrefabs;
        public List<GameObject> HitEffectPrefabs;
        public List<GameObject> AttachmentEffectPrefabs;

        public GameObject UnitStatusPrefab;
        public GameObject StatusEffectIconPrefab;
        public GameObject ActionUI_MoveButtonPrefab;
        public GameObject ActionUI_AttackButtonPrefab;
        public GameObject ActionUI_ItemButtonPrefab;
        public GameObject ActionUI_SkillButtonPrefab;
        public GameObject DamageTextPrefab;
        public Material SelectedUnitChipFrameMaterial;
        public Material StoneMaterial_Active;
        public Material StoneMaterial_Inactive;
        public Material TileMaterial_Normal;
        public Material TileMaterial_Blue;
        public Material TileMaterial_Red;
        public Material TileMaterial_Green;
        public Material TileMaterial_Ocher;
        public Material TileMaterial_Purple;
        public Material TileMaterial_Yellow;
        public Material TileMaterial_Heal;
        public Material TargetingTileMaterial_Default;
        public Material TargetingTileMaterial_NonSelectable;
        public Material TargetingTileMaterial_Movement_Selectable;
        public Material TargetingTileMaterial_Attack_Selectable;
        public Material TargetingTileMaterial_Skill_Selectable;
        public Material TargetingTileMaterial_Movement_Selected;
        public Material TargetingTileMaterial_Attack_Selected;
        public Material TargetingTileMaterial_Skill_Selected;

        public GameObject UnitInfoPanelPrefab;
        public GameObject UnitActionRangesInfoPanelPrefab;
        public GameObject UnitEquipmentsInfoPanelPrefab;
        public GameObject UnitSkillsInfoPanelPrefab;
        public GameObject WeaponInfoPanelPrefab;
        public GameObject ArmourInfoPanelPrefab;
        public GameObject AccessoryPanelInfoPrefab;
        public GameObject ItemInfoPanelPrefab;
        public GameObject SkillInfoPanelPrefab;
        public GameObject InfoPanelUnitLabelPrefab;
        public GameObject InfoPanelSkillInfoButtonPrefab;

        public GameObject ImagePrefab;
        public GameObject ObjectButtonPrefab;
        public GameObject ItemButtonPrefab;

        public GameObject PopUpWindowPrefab;
        public GameObject PopUpWindowButtonPrefab;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            //UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1.2f;
            GameDataContainer.Instance.ResetInitializationFlag();

            List<Sprite> novelEmotionIcons = new List<Sprite>
            {
                NovelEmotionIcon_Amused,
                NovelEmotionIcon_SpacedOut,
                NovelEmotionIcon_Sleeping,
                NovelEmotionIcon_Noticed,
                NovelEmotionIcon_Shocked,
                NovelEmotionIcon_Fun,
                NovelEmotionIcon_Glitter,
                NovelEmotionIcon_Flowers,
                NovelEmotionIcon_Furious,
                NovelEmotionIcon_Irritated,
                NovelEmotionIcon_Uneasy,
                NovelEmotionIcon_Silence,
                NovelEmotionIcon_Idea,
                NovelEmotionIcon_Yes,
                NovelEmotionIcon_No,
                NovelEmotionIcon_Heart,
                NovelEmotionIcon_BrokenHeart,
                NovelEmotionIcon_Exclamation,
                NovelEmotionIcon_Question,
                NovelEmotionIcon_ExclamationAndQuestion,
                NovelEmotionIcon_Injured,
                NovelEmotionIcon_Gloomy,
                NovelEmotionIcon_Angry,
                NovelEmotionIcon_Sweat,
                NovelEmotionIcon_Droplets,
                NovelEmotionIcon_Trembling
            };

            SpriteContainer.Instance.Initialize(GenderIcons, ElementIcons, RaritySprites, WeaponClassificationIcons, ArmourClassificationIcons, AccessoryClassificationIcons, Element1FrameSprites, Element2FrameSprites, SkillIconBaseSprites, CapsuleSprites, TileSprites, ObjectIconFrameSprite_Unit, ObjectIconFrameSprite_NonUnit, EmptySkillSlotIcon, EmptyObjectSprite_NotSet, EmptyObjectSprite_Set, GemSprite, GoldSprite, novelEmotionIcons);
            MaterialContainer.Instance.Initialize(RarityMaterials);
            EffectPrefabContainer.Instance.Initialize(BasicAttackActivationEffectPrefab, BasicAttackHitEffectPrefab, SkillActivationEffectPrefabs, EffectGenerationPointPrefabs, ProjectilePrefabs, LaserEffectPrefabs, HitEffectPrefabs, AttachmentEffectPrefabs);
            BattleSceneAssetContainer.Instance.Initialize(UnitStatusPrefab, StatusEffectIconPrefab, ActionUI_MoveButtonPrefab, ActionUI_AttackButtonPrefab, ActionUI_ItemButtonPrefab, ActionUI_SkillButtonPrefab, DamageTextPrefab, SelectedUnitChipFrameMaterial, StoneMaterial_Active, StoneMaterial_Inactive, TileMaterial_Normal, TileMaterial_Blue, TileMaterial_Red, TileMaterial_Green, TileMaterial_Ocher, TileMaterial_Purple, TileMaterial_Yellow, TileMaterial_Heal, TargetingTileMaterial_Default, TargetingTileMaterial_NonSelectable, TargetingTileMaterial_Movement_Selectable, TargetingTileMaterial_Attack_Selectable, TargetingTileMaterial_Skill_Selectable, TargetingTileMaterial_Movement_Selected, TargetingTileMaterial_Attack_Selected, TargetingTileMaterial_Skill_Selected);
            InfoPanelPrefabContainer.Instance.Initialize(UnitInfoPanelPrefab, UnitActionRangesInfoPanelPrefab, UnitEquipmentsInfoPanelPrefab, UnitSkillsInfoPanelPrefab, WeaponInfoPanelPrefab, ArmourInfoPanelPrefab, AccessoryPanelInfoPrefab, ItemInfoPanelPrefab, SkillInfoPanelPrefab, InfoPanelUnitLabelPrefab, InfoPanelSkillInfoButtonPrefab);
            SharedAssetContainer.Instance.Initialize(ImagePrefab, ObjectButtonPrefab, ItemButtonPrefab);
            PopUpWindowManager.Instance.Initialize(PopUpWindowPrefab, PopUpWindowButtonPrefab);

            Debug.Log("Application's resources have been initialized!");
        }
    }
}
