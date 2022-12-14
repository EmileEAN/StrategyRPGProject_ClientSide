using EEANWorks.Games.Unity.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class SpriteContainer
    {
        private static SpriteContainer m_instance;

        public static SpriteContainer Instance { get { return m_instance ?? (m_instance = new SpriteContainer()); } }

        private SpriteContainer()
        {
            m_isInitialized = false;
            m_areIconsDownloaded = false;
        }

        #region Properties
        // These sprites will be set through the ApplicationInitializer
        public IList<Sprite> SkillIconBaseSprites { get { return m_skillIconBaseSprites.AsReadOnly(); } }
        public IList<Sprite> CapsuleSprites { get { return m_capsuleSprites.AsReadOnly(); } }

        public Sprite ObjectIconFrameSprite_Unit { get; private set; }
        public Sprite ObjectIconFrameSprite_NonUnit { get; private set; }
        public Sprite EmptySkillSlotIcon { get; private set; }
        public Sprite EmptyObjectSprite_NotSet { get; private set; }
        public Sprite EmptyObjectSprite_Set { get; private set; }
        public Sprite GemSprite { get; private set; }
        public Sprite GoldSprite { get; private set; }

        public ReadOnlyDictionary<string, NovelCharacterSpriteSet> CharacterName_NovelCharacterSpriteSet { get { return m_characterName_novelCharacterSpriteSet.AsReadOnly(); } }

        // These icons will be downloaded from the server
        public ReadOnlyDictionary<UnitData, Sprite> UnitIcons { get { return m_unitIcons.AsReadOnly(); } }
        public ReadOnlyDictionary<WeaponData, Sprite> WeaponIcons { get { return m_weaponIcons.AsReadOnly(); } }
        public ReadOnlyDictionary<ArmourData, Sprite> ArmourIcons { get { return m_armourIcons.AsReadOnly(); } }
        public ReadOnlyDictionary<AccessoryData, Sprite> AccessoryIcons { get { return m_accessoryIcons.AsReadOnly(); } }
        public ReadOnlyDictionary<Item, Sprite> ItemIcons { get { return m_itemIcons.AsReadOnly(); } }

        public ReadOnlyDictionary<Gacha, Sprite> GachaBannerImages { get { return m_gachaBannerImages.AsReadOnly(); } }
        public ReadOnlyDictionary<Gacha, Sprite> GachaBackgroundImages { get { return m_gachaBackgroundImages.AsReadOnly(); } }
        public ReadOnlyDictionary<Story, Sprite> StoryBannerImages { get { return m_storyBannerImages.AsReadOnly(); } }
        public ReadOnlyDictionary<StoryArc, Sprite> StoryArcBannerImages { get { return m_storyArcBannerImages.AsReadOnly(); } }
        public ReadOnlyDictionary<StoryChapter, Sprite> StoryChapterBannerImages { get { return m_storyChapterBannerImages.AsReadOnly(); } }
        #endregion

        #region Private Fields
        private bool m_isInitialized;
        private bool m_areIconsDownloaded;

        // The sprites below will be set through the ApplicationInitializer
        private List<Sprite> m_genderIcons;
        private List<Sprite> m_elementIcons;
        private List<Sprite> m_raritySprites;
        private List<Sprite> m_weaponClassificationIcons;
        private List<Sprite> m_armourClassificationIcons;
        private List<Sprite> m_accessoryClassificationIcons;
        private List<Sprite> m_element1FrameSprites;
        private List<Sprite> m_element2FrameSprites;
        private List<Sprite> m_skillIconBaseSprites;
        private List<Sprite> m_capsuleSprites;
        private List<Sprite> m_tileSprites;

        private List<Sprite> m_novelEmotionIcons;
        private Dictionary<string, NovelCharacterSpriteSet> m_characterName_novelCharacterSpriteSet;

        // The icons below will be downloaded from the server
        private Dictionary<UnitData, Sprite> m_unitIcons;
        private Dictionary<WeaponData, Sprite> m_weaponIcons;
        private Dictionary<ArmourData, Sprite> m_armourIcons;
        private Dictionary<AccessoryData, Sprite> m_accessoryIcons;
        private Dictionary<Item, Sprite> m_itemIcons;

        private Dictionary<byte[], Sprite> m_skillIcons;
        private Dictionary<byte[], Sprite> m_statusEffectIcons;
        private Dictionary<byte[], Sprite> m_chapterMapImages;
        private Dictionary<byte[], int> m_chapterMapChipSizes;
        private Dictionary<byte[], Sprite> m_episodeLocationIcons;
        private Dictionary<byte[], Sprite> m_novelBackgroundImages;

        private Dictionary<Gacha, Sprite> m_gachaBannerImages;
        private Dictionary<Gacha, Sprite> m_gachaBackgroundImages;
        private Dictionary<Story, Sprite> m_storyBannerImages;
        private Dictionary<StoryArc, Sprite> m_storyArcBannerImages;
        private Dictionary<StoryChapter, Sprite> m_storyChapterBannerImages;
        #endregion

        #region Public Methods
        public void Initialize(List<Sprite> _genderIcons, List<Sprite> _elementIcons, List<Sprite> _raritySprites, List<Sprite> _weaponClassificationIcons, List<Sprite> _armourClassificationIcons, List<Sprite> _accessoryClassificationIcons, List<Sprite> _element1FrameSprites, List<Sprite> _element2FrameSprites, List<Sprite> _skillIconBaseSprites, List<Sprite> _capsuleSprites, List<Sprite> _tileSprites, Sprite _objectIconFrameSprite_unit, Sprite _objectIconFrameSprite_nonUnit, Sprite _emptySkillSlotIcon, Sprite _emptyObjectSprite_notSet, Sprite _emptyObjectSprite_set, Sprite _gemSprite, Sprite _goldSprite, List<Sprite> _novelEmotionIcons)
        {
            if (m_isInitialized)
                return;

            if (_genderIcons == null
                || _elementIcons == null
                || _raritySprites == null
                || _weaponClassificationIcons == null
                || _armourClassificationIcons == null
                || _accessoryClassificationIcons == null
                || _element1FrameSprites == null
                || _element2FrameSprites == null
                || _skillIconBaseSprites == null
                || _capsuleSprites == null
                || _tileSprites == null
                || _objectIconFrameSprite_unit == null
                || _objectIconFrameSprite_nonUnit == null
                || _emptySkillSlotIcon == null
                || _emptyObjectSprite_notSet == null
                || _emptyObjectSprite_set == null
                || _gemSprite == null
                || _goldSprite == null
                || _novelEmotionIcons == null)
            {
                return;
            }

            m_genderIcons = _genderIcons;
            m_elementIcons = _elementIcons;
            m_raritySprites = _raritySprites;
            m_weaponClassificationIcons = _weaponClassificationIcons;
            m_armourClassificationIcons = _armourClassificationIcons;
            m_accessoryClassificationIcons = _accessoryClassificationIcons;
            m_element1FrameSprites = _element1FrameSprites;
            m_element2FrameSprites = _element2FrameSprites;
            m_skillIconBaseSprites = _skillIconBaseSprites;
            m_capsuleSprites = _capsuleSprites;
            m_tileSprites = _tileSprites;
            ObjectIconFrameSprite_Unit = _objectIconFrameSprite_unit;
            ObjectIconFrameSprite_NonUnit = _objectIconFrameSprite_nonUnit;
            EmptySkillSlotIcon = _emptySkillSlotIcon;
            EmptyObjectSprite_NotSet = _emptyObjectSprite_notSet;
            EmptyObjectSprite_Set = _emptyObjectSprite_set;
            GemSprite = _gemSprite;
            GoldSprite = _goldSprite;
            m_novelEmotionIcons = _novelEmotionIcons;

            m_isInitialized = true;
        }

        public void SetIcons(Dictionary<UnitData, Sprite> _unitIcons, Dictionary<WeaponData, Sprite> _weaponIcons, Dictionary<ArmourData, Sprite> _armourIcons, Dictionary<AccessoryData, Sprite> _accessoryIcons, Dictionary<Item, Sprite> _itemIcons, Dictionary<int, byte[]> _skillIcons, Dictionary<int, byte[]> _statusEffectIcons, Dictionary<string, byte[]> _chapterMapImages, Dictionary<byte[], int> _chapterMapChipeSizes, Dictionary<int, byte[]> _episodeLocationIcons, Dictionary<int, byte[]> _novelBackgroundImages, Dictionary<Gacha, Sprite> _gachaBannerImages, Dictionary<Gacha, Sprite> _gachaBackgroundImages, Dictionary<Story, Sprite> _storyBannerImages, Dictionary<StoryArc, Sprite> _storyArcBannerImages, Dictionary<StoryChapter, Sprite> _storyChapterBannerImages, Dictionary<string, byte[]> _characterName_spriteSheetAsBytes)
        {
            if (m_areIconsDownloaded)
                return;

            if (_unitIcons == null
                || _weaponIcons == null
                || _armourIcons == null
                || _accessoryIcons == null
                || _itemIcons == null
                || _skillIcons == null
                || _statusEffectIcons == null
                || _chapterMapImages == null
                || _chapterMapChipeSizes == null
                || _episodeLocationIcons == null
                || _novelBackgroundImages == null
                || _gachaBannerImages == null
                || _gachaBackgroundImages == null
                || _storyBannerImages == null
                || _storyArcBannerImages == null
                || _storyChapterBannerImages == null
                || _characterName_spriteSheetAsBytes == null)
            {
                return;
            }

            m_unitIcons = _unitIcons;
            m_weaponIcons = _weaponIcons;
            m_armourIcons = _armourIcons;
            m_accessoryIcons = _accessoryIcons;
            m_itemIcons = _itemIcons;

            FilterMode filterMode = FilterMode.Point;

            m_skillIcons = new Dictionary<byte[], Sprite>();
            foreach (var entry in _skillIcons)
            {
                m_skillIcons.Add(entry.Value, ImageConverter.ByteArrayToSprite(entry.Value, filterMode));
            }

            m_statusEffectIcons = new Dictionary<byte[], Sprite>();
            foreach (var entry in _statusEffectIcons)
            {
                m_statusEffectIcons.Add(entry.Value, ImageConverter.ByteArrayToSprite(entry.Value, filterMode));
            }

            m_chapterMapImages = new Dictionary<byte[], Sprite>();
            foreach (var entry in _chapterMapImages)
            {
                m_chapterMapImages.Add(entry.Value, ImageConverter.ByteArrayToSprite(entry.Value, filterMode));
            }

            m_chapterMapChipSizes = new Dictionary<byte[], int>(_chapterMapChipeSizes);

            m_episodeLocationIcons = new Dictionary<byte[], Sprite>();
            foreach (var entry in _episodeLocationIcons)
            {
                m_episodeLocationIcons.Add(entry.Value, ImageConverter.ByteArrayToSprite(entry.Value, filterMode));
            }

            m_novelBackgroundImages = new Dictionary<byte[], Sprite>();
            foreach (var entry in _novelBackgroundImages)
            {
                m_novelBackgroundImages.Add(entry.Value, ImageConverter.ByteArrayToSprite(entry.Value, filterMode));
            }

            m_characterName_novelCharacterSpriteSet = new Dictionary<string, NovelCharacterSpriteSet>();
            foreach (var entry in _characterName_spriteSheetAsBytes)
            {
                m_characterName_novelCharacterSpriteSet.Add(entry.Key, new NovelCharacterSpriteSet(entry.Value));
            }

            m_gachaBannerImages = _gachaBannerImages;
            m_gachaBackgroundImages = _gachaBackgroundImages;
            m_storyBannerImages = _storyBannerImages;
            m_storyArcBannerImages = _storyArcBannerImages;
            m_storyChapterBannerImages = _storyChapterBannerImages;

            m_areIconsDownloaded = true;
        }

        public Sprite GetGenderIcon(Unit _unit) { return GetGenderIcon(_unit.BaseInfo.Gender); }
        public Sprite GetGenderIcon(eGender _gender)
        {
            int genderIndex = Convert.ToInt32(_gender);
            return (m_genderIcons.Count > genderIndex) ? m_genderIcons[genderIndex] : null;
        }

        public List<Sprite> ElementIconsForUnit(Unit _unit) { return ElementIconsForUnit(_unit.BaseInfo); }
        public List<Sprite> ElementIconsForUnit(UnitData _unit)
        {
            int element1Index = Convert.ToInt32(_unit.Elements[0]);
            Sprite sprite_element1Icon = m_elementIcons[element1Index];

            int element2Index = Convert.ToInt32(_unit.Elements[1]);
            Sprite sprite_element2Icon;
            if (element2Index == 0 && element1Index != 0) // index == 0 means eElement.None
                sprite_element2Icon = m_elementIcons[element1Index];
            else
                sprite_element2Icon = m_elementIcons[element2Index];

            return new List<Sprite> { sprite_element1Icon, sprite_element2Icon };
        }
        public Sprite GetElementIcon(eElement _element)
        {
            int elementIndex = Convert.ToInt32(_element);
            return (m_elementIcons.Count > elementIndex) ? m_elementIcons[elementIndex] : null;
        }

        public Sprite GetRaritySprite(Unit _unit) { return GetRaritySprite(_unit.BaseInfo); }
        public Sprite GetRaritySprite(Weapon _weapon) { return GetRaritySprite(_weapon.BaseInfo); }
        public Sprite GetRaritySprite(Armour _armour) { return GetRaritySprite(_armour.BaseInfo); }
        public Sprite GetRaritySprite(Accessory _accessory) { return GetRaritySprite(_accessory.BaseInfo); }
        public Sprite GetRaritySprite(IRarityMeasurable _rarityMeasurable) { return GetRaritySprite(_rarityMeasurable.Rarity); }
        public Sprite GetRaritySprite(eRarity _rarity)
        {
            int rarityIndex = (Convert.ToInt32(_rarity) / CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES) - 1;
            return (m_raritySprites.Count > rarityIndex) ? m_raritySprites[rarityIndex] : null;
        }

        public Sprite GetWeaponClassificationIcon(eWeaponClassification _classification)
        {
            int weaponClassificationIndex = Convert.ToInt32(_classification);
            return (m_weaponClassificationIcons.Count > weaponClassificationIndex) ? m_weaponClassificationIcons[weaponClassificationIndex] : null;
        }

        public Sprite GetArmourClassificationIcon(eArmourClassification _classification)
        {
            int armourClassificationIndex = Convert.ToInt32(_classification);
            return (m_armourClassificationIcons.Count > armourClassificationIndex) ? m_armourClassificationIcons[armourClassificationIndex] : null;
        }

        public Sprite GetAccessoryClassificationIcon(eAccessoryClassification _classification)
        {
            int accessoryClassificationIndex = Convert.ToInt32(_classification);
            return (m_accessoryClassificationIcons.Count > accessoryClassificationIndex) ? m_accessoryClassificationIcons[accessoryClassificationIndex] : null;
        }

        public List<Sprite> ElementFrameSpritesForUnit(UnitData _unit)
        {
            int element1Index = Convert.ToInt32(_unit.Elements[0]);
            Sprite sprite_element1Frame = m_element1FrameSprites[element1Index];

            int element2Index = Convert.ToInt32(_unit.Elements[1]);
            Sprite sprite_element2Frame;
            if (element2Index == 0 && element1Index != 0) // index == 0 means eElement.None
                sprite_element2Frame = m_element2FrameSprites[element1Index];
            else
                sprite_element2Frame = m_element2FrameSprites[element2Index];

            return new List<Sprite> { sprite_element1Frame, sprite_element2Frame };
        }

        public Sprite GetTileSprite(eTileType _tileType)
        {
            int tileTypeIndex = Convert.ToInt32(_tileType);
            return (m_tileSprites.Count > tileTypeIndex) ? m_tileSprites[tileTypeIndex] : null; 
        }

        public Sprite GetNovelEmotionIcon(eNovelEmotionIconType _emotionIconType)
        {
            int emotionIconTypeIndex = Convert.ToInt32(_emotionIconType);
            return (m_novelEmotionIcons.Count > emotionIconTypeIndex) ? m_novelEmotionIcons[emotionIconTypeIndex] : null;
        }

        public Sprite GetSkillIcon(Skill _skill) { return GetSkillIcon(_skill.BaseInfo); }
        public Sprite GetSkillIcon(SkillData _skill) { return m_skillIcons[_skill.IconAsBytes]; }
        public Sprite GetStatusEffectIcon(StatusEffect _statusEffect) { return GetStatusEffectIcon(_statusEffect.BaseInfo); }
        public Sprite GetStatusEffectIcon(StatusEffectData _statusEffect) { return m_statusEffectIcons[_statusEffect.IconAsBytes]; }
        public Sprite GetChapterMapImage(StoryChapter _chapter) { return m_chapterMapImages[_chapter.MapImageAsBytes]; }
        public int GetChapterMapChipSize(StoryChapter _chapter) { return m_chapterMapChipSizes[_chapter.MapImageAsBytes]; }
        public Sprite GetEpisodeLocationIcon(StoryEpisode _episode) { return m_episodeLocationIcons[_episode.LocationIconAsBytes]; }
        public Sprite GetNovelBackgroundImage(NovelSubscene _subscene) { return m_novelBackgroundImages[_subscene.BackgroundImageAsBytes]; }

        /// <summary>
        /// Should be called only when the application starts up, or when the session has expired and player has returned to Title.
        /// </summary>
        public void ResetIconsDownloadFlag() { m_areIconsDownloaded = false; }
        #endregion
    }

    public class NovelCharacterSpriteSet
    {
        public NovelCharacterSpriteSet(byte[] _spriteSheet)
        {
            List<Sprite> sprites = ImageConverter.ByteArrayToMultipleSprites(_spriteSheet, 10, 6, FilterMode.Point);

            // sprites[0] to [9] are other types of sprites
            List<Sprite> sprites_face = sprites.GetRange(10, 20);
            List<Sprite> sprites_mouth = sprites.GetRange(30, 30);

            TearsSprite = sprites[2];
            WaterfallTearsSprite = sprites[3];
            ShySprite = sprites[4];
            EmbarrassmentSprite = sprites[5];
            RedNoseSprite = sprites[6];
            BlackFaceMaskSprite = sprites[7];

            BaseFrontSprite = sprites[1];

            MouthSprites = new NovelCharacterMouthSprites(sprites_mouth);

            FaceSprites = new NovelCharacterFaceSprites(sprites_face);

            BlueFaceMaskSprite = sprites[8];
            RedFaceMaskSprite = sprites[9];

            BaseBackSprite = sprites[0];
        }

        #region Properties
        // The sprites below are grouped and ordered based on the layer order
        // Topmost layer
        public Sprite TearsSprite { get; }
        public Sprite WaterfallTearsSprite { get; }
        public Sprite ShySprite { get; }
        public Sprite EmbarrassmentSprite { get; }
        public Sprite RedNoseSprite { get; }
        public Sprite BlackFaceMaskSprite { get; }

        public Sprite BaseFrontSprite { get; }

        public NovelCharacterMouthSprites MouthSprites { get; }

        public NovelCharacterFaceSprites FaceSprites { get; }

        public Sprite BlueFaceMaskSprite { get; }
        public Sprite RedFaceMaskSprite { get; }

        // Bottommost layer
        public Sprite BaseBackSprite { get; }
        #endregion
    }

    public class NovelCharacterMouthSprites
    {
        public NovelCharacterMouthSprites(List<Sprite> _sprites)
        {
            m_sprites = _sprites.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        #region Private Fields
        private List<Sprite> m_sprites;
        #endregion

        #region Public Methods
        public Sprite GetSprite(eNovelCharacterMouthType _mouthType)
        {
            int mouthTypeIndex = Convert.ToInt32(_mouthType);
            return (m_sprites.Count > mouthTypeIndex) ? m_sprites[mouthTypeIndex] : null;
        }
        #endregion
    }

    public class NovelCharacterFaceSprites
    {
        public NovelCharacterFaceSprites(List<Sprite> _sprites)
        {
            m_sprites = _sprites.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        #region Private Fields
        private List<Sprite> m_sprites;
        #endregion

        #region Public Methods
        public Sprite GetSprite(eNovelCharacterFaceType _faceType)
        {
            int faceTypeIndex = Convert.ToInt32(_faceType);
            return (m_sprites.Count > faceTypeIndex) ? m_sprites[faceTypeIndex] : null;
        }
        #endregion
    }
}
