using EEANWorks;
using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine.UI;
using EEANWorks.Games.Unity.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;

namespace EEANWorks.Games.TBSG._01.Unity.Connection
{
    // This class is not static nor sealed, becuase it has been created in order not to retain the values across the whole game. 
    // The GameDataContainer class is used to store game values, which include some of the values obtained below.
    public class GameDataLoader
    {
        public GameDataLoader()
        {
            m_labelDictionary = new Dictionary<int, string>();

            m_statusEffectIconDictionary = new Dictionary<int, byte[]>();
            m_skillIconDictionary = new Dictionary<int, byte[]>();
            m_novelCharacterSpriteSheetDictionary = new Dictionary<string, byte[]>();
            m_novelBackgroundImageDictionary = new Dictionary<int, byte[]>();
            m_episodeLocationIconDictionary = new Dictionary<int, byte[]>();
            m_chapterMapImageDictionary = new Dictionary<string, byte[]>();
            m_chapterMapChipSizeDictionary = new Dictionary<byte[], int>();

            m_tagDictionary = new Dictionary<int, Tag>();
            m_conditionDictionary = new Dictionary<int, Condition>();
            m_conditionSetDictionary = new Dictionary<int, ConditionSet>();

            m_animationInfoDictionary = new Dictionary<int, AnimationInfo>();

            m_durationDictionary = new Dictionary<int, DurationData>();
            m_statusEffectDataList = new List<StatusEffectData>();
            m_effectList = new List<Effect>();
            m_itemCostDictionary = new Dictionary<int, Tuple<SkillMaterial, int>>();
            m_skillList = new List<SkillData>();

            m_itemList = new List<Item>();
            m_accessoryDataList = new List<AccessoryData>();
            m_armourDataList = new List<ArmourData>();
            m_weaponDataList = new List<WeaponData>();
            m_unitDataList = new List<UnitData>();

            m_itemRecipeList = new List<ItemRecipe>();
            m_accessoryRecipeList = new List<AccessoryRecipe>();
            m_armourRecipeList = new List<ArmourRecipe>();
            m_weaponRecipeList = new List<WeaponRecipe>();

            m_tileSetDictionary = new Dictionary<int, TileSet>();

            m_novelCharacterSpritePartsIndicationDictionary = new Dictionary<int, NovelCharacterSpritePartsIndication>();
            m_novelLineDictionary = new Dictionary<int, NovelLine>();
            m_mainStoryList = new List<MainStory>();
            m_eventStoryList = new List<EventStory>();

            m_gachaList = new List<Gacha>();

            m_effectId_secondaryEffectId = new Dictionary<int, int>();
            m_weaponId_transformableWeaponId = new Dictionary<int, int>();
            m_unitId_progressiveEvolutionRecipeBase = new Dictionary<int, UnitEvolutionRecipeBase>();
            m_unitId_retrogressiveEvolutionRecipeBase = new Dictionary<int, UnitEvolutionRecipeBase>();
            m_playableUnitId_inheritorUnitId_inheritingSkillId = new List<Tuple<int, int, int>>();

            GameDataContainer mock = GameDataContainer.Instance; //Ensure that the constructor of container is called before calling any of its methods
        }

        #region Properties
        public bool IsPlayerLoaded { get { return m_player != null; } }

        public static string PassForGameDataContainerTransaction { private get; set; }
        #endregion

        #region Private Fields
        private Dictionary<int, String> m_labelDictionary;

        private Dictionary<int, byte[]> m_statusEffectIconDictionary;
        private Dictionary<int, byte[]> m_skillIconDictionary;
        private Dictionary<string, byte[]> m_novelCharacterSpriteSheetDictionary;
        private Dictionary<int, byte[]> m_novelBackgroundImageDictionary;
        private Dictionary<int, byte[]> m_episodeLocationIconDictionary;
        private Dictionary<string, byte[]> m_chapterMapImageDictionary;
        private Dictionary<byte[], int> m_chapterMapChipSizeDictionary;

        private Dictionary<int, Tag> m_tagDictionary;
        private Dictionary<int, Condition> m_conditionDictionary;
        private Dictionary<int, ConditionSet> m_conditionSetDictionary;

        private Dictionary<int, AnimationInfo> m_animationInfoDictionary;

        private Dictionary<int, DurationData> m_durationDictionary;
        private List<StatusEffectData> m_statusEffectDataList;
        private List<Effect> m_effectList;
        private Dictionary<int, Tuple<SkillMaterial, int>> m_itemCostDictionary;
        private List<SkillData> m_skillList;

        private List<Item> m_itemList;
        private List<AccessoryData> m_accessoryDataList;
        private List<ArmourData> m_armourDataList;
        private List<WeaponData> m_weaponDataList;
        private List<UnitData> m_unitDataList;

        private List<ItemRecipe> m_itemRecipeList;
        private List<AccessoryRecipe> m_accessoryRecipeList;
        private List<ArmourRecipe> m_armourRecipeList;
        private List<WeaponRecipe> m_weaponRecipeList;

        private Dictionary<int, TileSet> m_tileSetDictionary;

        private Dictionary<int, NovelCharacterSpritePartsIndication> m_novelCharacterSpritePartsIndicationDictionary;
        private Dictionary<int, NovelLine> m_novelLineDictionary;
        private List<MainStory> m_mainStoryList;
        private List<EventStory> m_eventStoryList;

        private List<Gacha> m_gachaList;

        private Player m_player;

        private Dictionary<int, int> m_effectId_secondaryEffectId;
        private Dictionary<int, int> m_weaponId_transformableWeaponId;
        private Dictionary<int, UnitEvolutionRecipeBase> m_unitId_progressiveEvolutionRecipeBase;
        private Dictionary<int, UnitEvolutionRecipeBase> m_unitId_retrogressiveEvolutionRecipeBase;
        private List<Tuple<int, int, int>> m_playableUnitId_inheritorUnitId_inheritingSkillId;
        #endregion

        public IEnumerator LoadCoreGameData(LooperAndCoroutineLinker _looperAndCoroutineLinker, string _userName, string _password, string _sessionId)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "LoadCoreGameData"},
                    {"userName", _userName},
                    {"password", _password},
                    {"sessionId", _sessionId}
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
 
                string sectionString = string.Empty;

                sectionString = response.GetTagPortion("Labels");
                m_labelDictionary.Clear();
                m_labelDictionary = ResponseStringToLabelDictionary(sectionString);

                sectionString = response.GetTagPortion("StatusEffectIcons");
                m_statusEffectIconDictionary.Clear();
                m_statusEffectIconDictionary = ResponseStringToStatusEffectIconDictionary(sectionString);

                sectionString = response.GetTagPortion("SkillIcons");
                m_skillIconDictionary.Clear();
                m_skillIconDictionary = ResponseStringToSkillIconDictionary(sectionString);

                sectionString = response.GetTagPortion("ChapterMapImages");
                m_chapterMapImageDictionary.Clear();
                m_chapterMapChipSizeDictionary.Clear();
                m_chapterMapImageDictionary = ResponseStringToChapterMapImageDictionary(sectionString);

                sectionString = response.GetTagPortion("EpisodeLocationIcons");
                m_episodeLocationIconDictionary.Clear();
                m_episodeLocationIconDictionary = ResponseStringToEpisodeLocationIconDictionary(sectionString);

                sectionString = response.GetTagPortion("NovelBackgroundImages");
                m_novelBackgroundImageDictionary.Clear();
                m_novelBackgroundImageDictionary = ResponseStringToNovelBackgroundImageDictionary(sectionString);

                sectionString = response.GetTagPortion("NovelCharacterSpriteSheets");
                m_novelCharacterSpriteSheetDictionary.Clear();
                m_novelCharacterSpriteSheetDictionary = ResponseStringToNovelCharacterSpriteSheetDictionary(sectionString);

                sectionString = response.GetTagPortion("Tags");
                m_tagDictionary.Clear();
                m_tagDictionary = ResponseStringToTagDictionary(sectionString);

                sectionString = response.GetTagPortion("Conditions");
                m_conditionDictionary.Clear();
                m_conditionDictionary = ResponseStringToConditionDictionary(sectionString);

                sectionString = response.GetTagPortion("ConditionSets");
                m_conditionSetDictionary.Clear();
                m_conditionSetDictionary = ResponseStringToConditionSetDictionary(sectionString);

                sectionString = response.GetTagPortion("AnimationInfos");
                m_animationInfoDictionary.Clear();
                m_animationInfoDictionary = ResponseStringToAnimationInfoDictionary(sectionString);

                sectionString = response.GetTagPortion("Durations");
                m_durationDictionary.Clear();
                m_durationDictionary = ResponseStringToDurationDictionary(sectionString);

                sectionString = response.GetTagPortion("StatusEffects");
                m_statusEffectDataList.Clear();
                m_statusEffectDataList = ResponseStringToStatusEffectDataList(sectionString);

                sectionString = response.GetTagPortion("Effects");
                m_effectList.Clear();
                m_effectList = ResponseStringToEffectList(sectionString);

                sectionString = response.GetTagPortion("ItemCosts");
                m_itemCostDictionary.Clear();
                m_itemCostDictionary = ResponseStringToItemCostDictionary(sectionString);

                sectionString = response.GetTagPortion("Skills");
                m_skillList.Clear();
                m_skillList = ResponseStringToSkillDataList(sectionString);

                sectionString = response.GetTagPortion("Items");
                m_itemList.Clear();
                m_itemList = ResponseStringToItemList(sectionString);

                sectionString = response.GetTagPortion("Accessories");
                m_accessoryDataList.Clear();
                m_accessoryDataList = ResponseStringToAccessoryDataList(sectionString);

                sectionString = response.GetTagPortion("Armours");
                m_armourDataList.Clear();
                m_armourDataList = ResponseStringToArmourDataList(sectionString);

                sectionString = response.GetTagPortion("Weapons");
                m_weaponDataList.Clear();
                m_weaponDataList = ResponseStringToWeaponDataList(sectionString);

                sectionString = response.GetTagPortion("Units");
                m_unitDataList.Clear();
                m_unitDataList = ResponseStringToUnitDataList(sectionString);

                sectionString = response.GetTagPortion("ItemRecipes");
                m_itemRecipeList.Clear();
                m_itemRecipeList = ResponseStringToItemRecipeList(sectionString);

                sectionString = response.GetTagPortion("AccessoryRecipes");
                m_accessoryRecipeList.Clear();
                m_accessoryRecipeList = ResponseStringToAccessoryRecipeList(sectionString);

                sectionString = response.GetTagPortion("ArmourRecipes");
                m_armourRecipeList.Clear();
                m_armourRecipeList = ResponseStringToArmourRecipeList(sectionString);

                sectionString = response.GetTagPortion("WeaponRecipes");
                m_weaponRecipeList.Clear();
                m_weaponRecipeList = ResponseStringToWeaponRecipeList(sectionString);

                sectionString = response.GetTagPortion("TileSets");
                m_tileSetDictionary.Clear();
                m_tileSetDictionary = ResponseStringToTileSetDictionary(sectionString);

                sectionString = response.GetTagPortion("NovelCharacterSpritePartsIndications");
                m_novelCharacterSpritePartsIndicationDictionary.Clear();
                m_novelCharacterSpritePartsIndicationDictionary = ResponseStringToNovelCharacterSpritePartsIndicationDictionary(sectionString);

                sectionString = response.GetTagPortion("NovelLines");
                m_novelLineDictionary.Clear();
                m_novelLineDictionary = ResponseStringToNovelLineDictionary(sectionString);

                sectionString = response.GetTagPortion("MainStories");
                m_mainStoryList.Clear();
                m_mainStoryList = ResponseStringToStoryList<MainStory>(sectionString, true);

                sectionString = response.GetTagPortion("EventStories");
                m_eventStoryList.Clear();
                m_eventStoryList = ResponseStringToStoryList<EventStory>(sectionString, false);

                sectionString = response.GetTagPortion("Gachas");
                m_gachaList.Clear();
                m_gachaList = ResponseStringToGachaList(sectionString);

                sectionString = response.GetTagPortion("GachaDispensationOptionsRemainingAttempts");
                LoadRemainingAttemptsPerGachaDispensationOption(sectionString);

                LoadSecondaryEffects();
                LoadTransformableWeaponsData();
                LoadUnitEvolutionRecipes();

                sectionString = response.GetTagPortion("Player");
                m_player = ResponseStringToPlayer(sectionString);

                LoadSkillInheritors(m_player.UnitsOwned);

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public IEnumerator LoadGachaData(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "LoadGachaData"},
                    {"sessionId", GameDataContainer.Instance.SessionId}
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                {
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
                }
            }
            else
            {
                string response = uwr.downloadHandler.text;

                if (response.Contains("loadingError"))
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Gacha data could not be loaded successfully...", "Return", () => SceneConnector.GoToPreviousScene(), true);
                else
                {
                    string sectionString = string.Empty;

                    sectionString = response.GetTagPortion("Gachas");
                    m_gachaList.Clear();
                    m_gachaList = ResponseStringToGachaList(sectionString);

                    sectionString = response.GetTagPortion("GachaDispensationOptionsRemainingAttempts");
                    LoadRemainingAttemptsPerGachaDispensationOption(sectionString);

                    GameDataContainer.Instance.UpdateGachas(PassForGameDataContainerTransaction, m_gachaList);
                }

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public bool SaveLoadedData(string _sessionId)
        {
            TransferIconsToSpriteContainer();

            return GameDataContainer.Instance.Initialize(PassForGameDataContainerTransaction, _sessionId, m_player, m_animationInfoDictionary, m_statusEffectDataList, m_effectList, m_skillList, m_itemList, m_accessoryDataList, m_armourDataList, m_weaponDataList, m_unitDataList, m_itemRecipeList, m_accessoryRecipeList, m_armourRecipeList, m_weaponRecipeList, m_tileSetDictionary, m_mainStoryList, m_eventStoryList, m_gachaList);
        }

        private void TransferIconsToSpriteContainer()
        {
            FilterMode filterMode = FilterMode.Point;

            Dictionary<UnitData, Sprite> unitIcons = new Dictionary<UnitData, Sprite>();
            foreach (UnitData unitData in m_unitDataList)
            {
                unitIcons.Add(unitData, ImageConverter.ByteArrayToSprite(unitData.IconAsBytes, filterMode));
            }

            Dictionary<WeaponData, Sprite> weaponIcons = new Dictionary<WeaponData, Sprite>();
            foreach (WeaponData weaponData in m_weaponDataList)
            {
                weaponIcons.Add(weaponData, ImageConverter.ByteArrayToSprite(weaponData.IconAsBytes, filterMode));
            }

            Dictionary<ArmourData, Sprite> armourIcons = new Dictionary<ArmourData, Sprite>();
            foreach (ArmourData armourData in m_armourDataList)
            {
                armourIcons.Add(armourData, ImageConverter.ByteArrayToSprite(armourData.IconAsBytes, filterMode));
            }

            Dictionary<AccessoryData, Sprite> accessoryIcons = new Dictionary<AccessoryData, Sprite>();
            foreach (AccessoryData accessoryData in m_accessoryDataList)
            {
                accessoryIcons.Add(accessoryData, ImageConverter.ByteArrayToSprite(accessoryData.IconAsBytes, filterMode));
            }

            Dictionary<Item, Sprite> itemIcons = new Dictionary<Item, Sprite>();
            foreach (Item item in m_itemList)
            {
                itemIcons.Add(item, ImageConverter.ByteArrayToSprite(item.IconAsBytes, filterMode));
            }

            Dictionary<Gacha, Sprite> gachaBannerImages = new Dictionary<Gacha, Sprite>();
            Dictionary<Gacha, Sprite> gachaBackgroundImages = new Dictionary<Gacha, Sprite>();
            foreach (Gacha gacha in m_gachaList)
            {
                gachaBannerImages.Add(gacha, ImageConverter.ByteArrayToSprite(gacha.BackgroundImageAsBytes, filterMode));
                gachaBackgroundImages.Add(gacha, ImageConverter.ByteArrayToSprite(gacha.BannerImageAsBytes, filterMode));
            }

            Dictionary<Story, Sprite> storyBannerImages = new Dictionary<Story, Sprite>();
            Dictionary<StoryArc, Sprite> storyArcBannerImages = new Dictionary<StoryArc, Sprite>();
            Dictionary<StoryChapter, Sprite> storyChapterBannerImages = new Dictionary<StoryChapter, Sprite>();
            List<StoryArc> storyArcs = new List<StoryArc>();
            foreach (Story story in m_mainStoryList.Cast<Story>().Concat(m_eventStoryList.Cast<Story>()))
            {
                storyBannerImages.Add(story, ImageConverter.ByteArrayToSprite(story.BannerImageAsBytes, filterMode));
                foreach (StoryArc arc in story.Arcs)
                {
                    storyArcBannerImages.Add(arc, ImageConverter.ByteArrayToSprite(arc.BannerImageAsBytes, filterMode));
                    foreach (StoryChapter chapter in arc.Chapters)
                    {
                        storyChapterBannerImages.Add(chapter, ImageConverter.ByteArrayToSprite(chapter.BannerImageAsBytes, filterMode));
                    }
                }
            }

            SpriteContainer.Instance.SetIcons(unitIcons, weaponIcons, armourIcons, accessoryIcons, itemIcons, m_skillIconDictionary, m_statusEffectIconDictionary, m_chapterMapImageDictionary, m_chapterMapChipSizeDictionary, m_episodeLocationIconDictionary, m_novelBackgroundImageDictionary, gachaBannerImages, gachaBackgroundImages, storyBannerImages, storyArcBannerImages, storyChapterBannerImages, m_novelCharacterSpriteSheetDictionary);
        }

        private Player ResponseStringToPlayer(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _response.GetTagPortionValue<int>("Id");

                string playerName = _response.GetTagPortionValue<string>("PlayerName");

                int gemsOwned = _response.GetTagPortionValue<int>("GemsOwned");

                int goldOwned = _response.GetTagPortionValue<int>("GoldOwned");

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("WeaponsOwned");
                List<Weapon> weaponsOwned = new List<Weapon>();
                while (sectionString != "")
                {
                    string weaponString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Weapon", ref weaponString);

                    weaponsOwned.Add(StringToWeapon(weaponString));
                }

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ArmoursOwned");
                List<Armour> armoursOwned = new List<Armour>();
                while (sectionString != "")
                {
                    string armourString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Armour", ref armourString);

                    armoursOwned.Add(StringToArmour(armourString));
                }

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("AccessoriesOwned");
                List<Accessory> accessoriesOwned = new List<Accessory>();
                while (sectionString != "")
                {
                    string accessoryString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Accessory", ref accessoryString);

                    accessoriesOwned.Add(StringToAccessory(accessoryString));
                }

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("UnitsOwned");
                List<Unit> unitsOwned = new List<Unit>();
                while (sectionString != "")
                {
                    string unitString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Unit", ref unitString);

                    unitsOwned.Add(StringToUnit(unitString, weaponsOwned, armoursOwned, accessoriesOwned));
                }

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ItemsOwned");
                Dictionary<Item, int> itemsOwned = new Dictionary<Item, int>();
                while (sectionString != "")
                {
                    string itemString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Item", ref itemString);

                    KeyValuePair<Item, int> quantityPerItem = StringToItemOwned(itemString);
                    itemsOwned.Add(quantityPerItem.Key, quantityPerItem.Value);
                }

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ItemSets");
                List<ItemSet> itemSets = new List<ItemSet>();
                while (sectionString != "")
                {
                    string itemSetString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ItemSet", ref itemSetString);

                    itemSets.Add(StringToItemSet(itemSetString));
                }

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Teams");
                List<Team> teams = new List<Team>();
                while (sectionString != "")
                {
                    string teamString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Team", ref teamString);

                    teams.Add(StringToTeam(teamString, unitsOwned, itemSets));
                }

                return new Player(id, playerName, unitsOwned, weaponsOwned, armoursOwned, accessoriesOwned, itemsOwned, itemSets, teams, gemsOwned, goldOwned);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Unit StringToUnit(string _string, List<Weapon> _weaponsOwned, List<Armour> _armoursOwned, List<Accessory> _accessoriesOwned)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int uniqueId = _string.GetTagPortionValue<int>("UniqueId");

                int baseUnitId = _string.GetTagPortionValue<int>("BaseUnitId");
                UnitData baseUnitData = m_unitDataList.First(x => x.Id == baseUnitId);

                int accumulatedExperience = _string.GetTagPortionValue<int>("AccumulatedExperience");

                string nickname = _string.GetTagPortionValue<string>("Nickname");

                bool isLocked = _string.GetTagPortionValue<bool>("IsLocked");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Skills");
                List<Skill> skills = new List<Skill>();
                while (sectionString != "")
                {
                    string skillString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Skill", ref skillString);

                    int skillId = skillString.GetTagPortionValue<int>("SkillId");

                    int skillLevel = skillString.GetTagPortionValue<int>("SkillLevel");

                    SkillData skillData = m_skillList.First(x => x.Id == skillId);

                    if (skillData is OrdinarySkillData)
                        skills.Add(new OrdinarySkill(skillData as OrdinarySkillData, skillLevel));
                    else if (skillData is CounterSkillData)
                        skills.Add(new CounterSkill(skillData as CounterSkillData, skillLevel));
                    else if (skillData is UltimateSkillData)
                        skills.Add(new UltimateSkill(skillData as UltimateSkillData, skillLevel));
                    else if (skillData is PassiveSkillData)
                        skills.Add(new PassiveSkill(skillData as PassiveSkillData, skillLevel));
                }

                int mainWeaponId = _string.GetTagPortionValue<int>("MainWeaponId");
                Weapon mainWeapon = _weaponsOwned.FirstOrDefault(x => x.UniqueId == mainWeaponId);
                int subWeaponId = _string.GetTagPortionValue<int>("SubWeaponId");
                Weapon subWeapon = _weaponsOwned.FirstOrDefault(x => x.UniqueId == subWeaponId);
                int armourId = _string.GetTagPortionValue<int>("ArmourId");
                Armour armour = _armoursOwned.FirstOrDefault(x => x.UniqueId == armourId);
                int accessoryId = _string.GetTagPortionValue<int>("AccessoryId");
                Accessory accessory = _accessoriesOwned.FirstOrDefault(x => x.UniqueId == accessoryId);

                int skillInheritorUnitId = _string.GetTagPortionValue<int>("SkillInheritorUnitId");
                int inheritingSkillId = _string.GetTagPortionValue<int>("InheritingSkillId");
                m_playableUnitId_inheritorUnitId_inheritingSkillId.Add(new Tuple<int, int, int>(uniqueId, skillInheritorUnitId, inheritingSkillId));

                return new Unit(baseUnitData, uniqueId, nickname, accumulatedExperience, isLocked, skills, mainWeapon, subWeapon, armour, accessory);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Weapon StringToWeapon(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                int uniqueId = _string.GetTagPortionValue<int>("UniqueId");

                int baseWeaponId = _string.GetTagPortionValue<int>("BaseWeaponId");
                WeaponData baseWeaponData = m_weaponDataList.First(x => x.Id == baseWeaponId);

                bool isLocked = _string.GetTagPortionValue<bool>("IsLocked");

                int accumulatedExperience = 0;
                if (_string.Contains("AccumulatedExperience"))
                    accumulatedExperience = _string.GetTagPortionValue<int>("AccumulatedExperience");

                switch (baseWeaponData.WeaponType)
                {
                    default: //case eWeaponType.Ordinary
                        return new OrdinaryWeapon(baseWeaponData, uniqueId, isLocked);
                    case eWeaponType.Levelable:
                        return new LevelableWeapon(baseWeaponData, uniqueId, isLocked, accumulatedExperience);
                    case eWeaponType.Transformable:
                        return new TransformableWeapon(baseWeaponData, uniqueId, isLocked);
                    case eWeaponType.LevelableTransformable:
                        return new LevelableTransformableWeapon(baseWeaponData, uniqueId, isLocked, accumulatedExperience);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Armour StringToArmour(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                int uniqueId = _string.GetTagPortionValue<int>("UniqueId");

                int baseArmourId = _string.GetTagPortionValue<int>("BaseArmourId");
                ArmourData baseArmourData = m_armourDataList.First(x => x.Id == baseArmourId);

                bool isLocked = _string.GetTagPortionValue<bool>("IsLocked");

                return new Armour(baseArmourData, uniqueId, isLocked);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Accessory StringToAccessory(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                int uniqueId = _string.GetTagPortionValue<int>("UniqueId");

                int baseAccessoryId = _string.GetTagPortionValue<int>("BaseAccessoryId");
                AccessoryData baseAccessoryData = m_accessoryDataList.First(x => x.Id == baseAccessoryId);

                bool isLocked = _string.GetTagPortionValue<bool>("IsLocked");

                return new Accessory(baseAccessoryData, uniqueId, isLocked);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private KeyValuePair<Item, int> StringToItemOwned(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                int id = _string.GetTagPortionValue<int>("ItemId");
                Item item = m_itemList.First(x => x.Id == id);

                int quantity = _string.GetTagPortionValue<int>("Quantity");

                return new KeyValuePair<Item, int>(item, quantity);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return new KeyValuePair<Item, int>();
            }
        }

        private ItemSet StringToItemSet(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Items");
                Dictionary<BattleItem, int> quantityPerItem = new Dictionary<BattleItem, int>();
                while (sectionString != "")
                {
                    string itemString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Item", ref itemString);

                    int itemId = itemString.GetTagPortionValue<int>("ItemId");
                    BattleItem item = m_itemList.First(x => x.Id == itemId) as BattleItem;

                    int quantity = itemString.GetTagPortionValue<int>("Quantity");

                    quantityPerItem.Add(item, quantity);
                }

                return new ItemSet(id, quantityPerItem);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return new ItemSet(0, new Dictionary<BattleItem, int>());
            }
        }

        private Team StringToTeam(string _string, List<Unit> _unitsOwned, List<ItemSet> _itemSets)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("MemberIds");
                List<Unit> members = new List<Unit>();
                while (sectionString != "")
                {
                    int memberId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("MemberId", ref memberId);

                    Unit member = (memberId != 0) ? _unitsOwned.First(x => x.UniqueId == memberId) : null;

                    members.Add(member);
                }

                ItemSet itemSet = null;
                if (_string.Contains("ItemSetId"))
                {
                    int itemSetId = _string.GetTagPortionValue<int>("ItemSetId");
                    itemSet = _itemSets.FirstOrDefault(x => x.Id == itemSetId);
                }

                return new Team(members, itemSet);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<UnitData> ResponseStringToUnitDataList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<UnitData> result = new List<UnitData>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Units");

                while (sectionString != "")
                {
                    string unitDataString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("UnitData", ref unitDataString);

                    result.Add(StringToUnitData(unitDataString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private UnitData StringToUnitData(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string name = _string.GetTagPortionValue<string>("Name");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("IconAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                eGender gender = _string.GetTagPortionValue<eGender>("Gender");

                eRarity rarity = _string.GetTagPortionValue<eRarity>("Rarity");

                eTargetRangeClassification movementRangeClassification = _string.GetTagPortionValue<eTargetRangeClassification>("MovementRangeClassification");

                eTargetRangeClassification nonMovementActionRangeClassification = _string.GetTagPortionValue<eTargetRangeClassification>("NonMovementActionRangeClassification");

                eElement element1 = _string.GetTagPortionValue<eElement>("Element1");
                eElement element2 = _string.GetTagPortionValue<eElement>("Element2");
                List<eElement> elements = new List<eElement> { element1, element2 };


                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("EquipableWeaponClassifications");
                List<eWeaponClassification> equipableWeaponClassifications = new List<eWeaponClassification>();
                while (sectionString != "")
                {
                    eWeaponClassification equipableWeaponClassification = default;
                    sectionString = sectionString.DetachTagPortionAsValue("WeaponClassification", ref equipableWeaponClassification);

                    equipableWeaponClassifications.Add(equipableWeaponClassification);
                }


                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("EquipableArmourClassifications");
                List<eArmourClassification> equipableArmourClassifications = new List<eArmourClassification>();
                while (sectionString != "")
                {
                    eArmourClassification equipableArmourClassification = default;
                    sectionString = sectionString.DetachTagPortionAsValue("ArmourClassification", ref equipableArmourClassification);

                    equipableArmourClassifications.Add(equipableArmourClassification);
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("EquipableAccessoryClassifications");
                List<eAccessoryClassification> equipableAccessoryClassifications = new List<eAccessoryClassification>();
                while (sectionString != "")
                {
                    eAccessoryClassification equipableAccessoryClassification = default;
                    sectionString = sectionString.DetachTagPortionAsValue("AccessoryClassification", ref equipableAccessoryClassification);

                    equipableAccessoryClassifications.Add(equipableAccessoryClassification);
                }

                int maxLevelHp = _string.GetTagPortionValue<int>("MaxLevelHP");

                int maxLevelPhysicalStrength = _string.GetTagPortionValue<int>("MaxLevelPhysicalStrength");

                int maxLevelPhysicalResistance = _string.GetTagPortionValue<int>("MaxLevelPhysicalResistance");

                int maxLevelMagicalStrength = _string.GetTagPortionValue<int>("MaxLevelMagicalStrength");

                int maxLevelMagicalResistance = _string.GetTagPortionValue<int>("MaxLevelMagicalResistance");

                int maxLevelVitality = _string.GetTagPortionValue<int>("MaxLevelVitality");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("SkillIds");
                List<SkillData> skills = new List<SkillData>();
                while (sectionString != "")
                {
                    int skillId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("SkillId", ref skillId);

                    skills.Add(m_skillList.First(x => x.Id == skillId));
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("LabelIds");
                List<string> labels = new List<string>();
                while (sectionString != "")
                {
                    int labelId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("LabelId", ref labelId);

                    labels.Add(m_labelDictionary[labelId]);
                }

                string description = _string.GetTagPortionValue<string>("Description");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("ProgressiveEvolutionRecipes");
                while (sectionString != "")
                {
                    string progressiveEvolutionRecipeString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ProgressiveEvolutionRecipe", ref progressiveEvolutionRecipeString);

                    UnitEvolutionRecipeBase progressiveEvolutionRecipeBase = StringToUnitEvolutionRecipeBase(progressiveEvolutionRecipeString);
                    m_unitId_progressiveEvolutionRecipeBase.Add(id, progressiveEvolutionRecipeBase);
                }

                if (_string.Contains("RetrogressiveEvolutionRecipe"))
                {
                    sectionString = _string.GetTagPortion("RetrogressiveEvolutionRecipe");

                    UnitEvolutionRecipeBase retrogressiveEvolutionRecipeBase = StringToUnitEvolutionRecipeBase(sectionString);
                    m_unitId_retrogressiveEvolutionRecipeBase.Add(id, retrogressiveEvolutionRecipeBase);
                }

                return new UnitData(id, name, iconAsBytes, gender, rarity, movementRangeClassification, nonMovementActionRangeClassification, elements, equipableWeaponClassifications, equipableArmourClassifications, equipableAccessoryClassifications, maxLevelHp, maxLevelPhysicalStrength, maxLevelPhysicalResistance, maxLevelMagicalStrength, maxLevelMagicalResistance, maxLevelVitality, skills, labels, description);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private UnitEvolutionRecipeBase StringToUnitEvolutionRecipeBase(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = String.Empty;

                int afterEvolutionUnitId = _string.GetTagPortionValue<int>("AfterEvolutionUnitId");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("MaterialIds");
                List<int> materialIds = new List<int>();
                while (sectionString != "")
                {
                    int materialId = default;
                    sectionString = sectionString.DetachTagPortionAsValue<int>("MaterialIds", ref materialId);

                    materialIds.Add(materialId);
                }
                if (materialIds.Count != CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE)
                    return null;

                int cost = _string.GetTagPortionValue<int>("Cost");

                return new UnitEvolutionRecipeBase(afterEvolutionUnitId, materialIds[0], materialIds[1], materialIds[2], materialIds[4], materialIds[5], cost);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<Gacha> ResponseStringToGachaList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<Gacha> result = new List<Gacha>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Gachas");

                while (sectionString != "")
                {
                    string gachaString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Gacha", ref gachaString);

                    result.Add(StringToGacha(gachaString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Gacha StringToGacha(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string title = _string.GetTagPortionValue<string>("Title");

                eGachaClassification gachaClassification = _string.GetTagPortionValue<eGachaClassification>("GachaClassification");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("GachaObjectInfos");
                List<GachaObjectInfo> gachaObjectInfos = new List<GachaObjectInfo>();
                while (sectionString != "")
                {
                    string gachaObjectInfoString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("GachaObjectInfo", ref gachaObjectInfoString);

                    gachaObjectInfos.Add(StringToGachaObjectInfo(gachaObjectInfoString, gachaClassification));
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("DefaultDispensationValues");
                ValuePerRarity defaultDispensationValues = StringToValuePerRarity(sectionString);

                AlternativeDispensationInfo alternativeDispensationInfo = default;
                if (_string.Contains("AlternativeDispensationInfo"))
                {
                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("AlternativeDispensationInfo");

                    int applyAtXthDispensation = sectionString.GetTagPortionValue<int>("ApplyAtXthDispensation");

                    String ratioPerRarityString = sectionString.GetTagPortionWithoutOpeningAndClosingTags("RatioPerRarity");
                    ValuePerRarity ratioPerRarity = StringToValuePerRarity(ratioPerRarityString);

                    alternativeDispensationInfo = new AlternativeDispensationInfo { ApplyAtXthDispensation = applyAtXthDispensation, RatioPerRarity = ratioPerRarity };
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("DispensationOptions");
                List<DispensationOption> dispensationOptions = new List<DispensationOption>();
                while (sectionString != "")
                {
                    string dispensationOptionString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("DispensationOption", ref dispensationOptionString);

                    dispensationOptions.Add(StringToDispensationOption(dispensationOptionString));
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("BannerImageAsBytes");
                byte[] bannerImageAsBytes = Convert.FromBase64String(sectionString);

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("BackgroundImageAsBytes");
                byte[] backgroundImageAsBytes = Convert.FromBase64String(sectionString);

                int levelOfObjects = sectionString.Contains("LevelOfObjects") ? _string.GetTagPortionValue<int>("LevelOfObjects") : default;

                return new Gacha(id, title, gachaClassification, gachaObjectInfos, defaultDispensationValues, alternativeDispensationInfo, dispensationOptions, bannerImageAsBytes, backgroundImageAsBytes, levelOfObjects);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private GachaObjectInfo StringToGachaObjectInfo(string _string, eGachaClassification _gachaClassification)
        {
            if (_string == null)
                return default;

            try
            {
                int objectId = _string.GetTagPortionValue<int>("ObjectId");

                IRarityMeasurable obj;
                switch (_gachaClassification)
                {
                    default: // case eGachaClassification.Unit
                        obj = m_unitDataList.First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.Weapon:
                        obj = m_weaponDataList.First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.Armour:
                        obj = m_armourDataList.First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.Accessory:
                        obj = m_accessoryDataList.First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.SkillItem:
                    case eGachaClassification.SkillMaterial:
                    case eGachaClassification.ItemMaterial:
                    case eGachaClassification.EquipmentMaterial:
                    case eGachaClassification.EvolutionMaterial:
                    case eGachaClassification.WeaponEnhancementMaterial:
                    case eGachaClassification.UnitEnhancementMaterial:
                    case eGachaClassification.SkillEnhancementMaterial:
                        obj = m_itemList.First(x => x.Id == objectId);
                        break;
                }

                int relativeOccurrenceValue = _string.GetTagPortionValue<int>("RelativeOccurrenceValue");

                return new GachaObjectInfo { Object = obj, RelativeOccurenceValue = relativeOccurrenceValue };
            }
            catch (Exception ex)
            {
                //Debug.Log(ex.Message);
                return new GachaObjectInfo { Object = null, RelativeOccurenceValue = default };
            }
        }

        private DispensationOption StringToDispensationOption(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                int id = _string.GetTagPortionValue<int>("Id");

                eCostType costType = _string.GetTagPortionValue<eCostType>("CostType");

                int costItemId = _string.GetTagPortionValue<int>("CostItemId");

                int costValue = _string.GetTagPortionValue<int>("CostValue");

                int timesToDispense = _string.GetTagPortionValue<int>("TimesToDispense");

                bool isNumberOfAttemptsPerDay = _string.GetTagPortionValue<bool>("IsNumberOfAttemptsPerDay");

                return new DispensationOption(id, costType, costItemId, costValue, timesToDispense, isNumberOfAttemptsPerDay);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return new DispensationOption(default, default, default, default, default, default);
            }
        }

        private void LoadRemainingAttemptsPerGachaDispensationOption(string _string)
        {
            try
            {
                string sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("GachaDispensationOptionsRemainingAttempts");
                while (sectionString != "")
                {
                    string entryString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Entry", ref entryString);

                    int gachaId = entryString.GetTagPortionValue<int>("GachaId");
                    Gacha gacha = m_gachaList.First(x => x.Id == gachaId);

                    int dispensationOptionId = entryString.GetTagPortionValue<int>("DispensationOptionId");
                    DispensationOption dispensationOption = gacha.DispensationOptions.First(x => x.Id == dispensationOptionId);

                    int remainingAttempts = entryString.GetTagPortionValue<int>("RemainingAttempts");

                    dispensationOption.RemainingAttempts = remainingAttempts;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private ValuePerRarity StringToValuePerRarity(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                string sectionString = string.Empty;

                int common = _string.GetTagPortionValue<int>("Common");
                int uncommon = _string.GetTagPortionValue<int>("Uncommon");
                int rare = _string.GetTagPortionValue<int>("Rare");
                int epic = _string.GetTagPortionValue<int>("Epic");
                int legendary = _string.GetTagPortionValue<int>("Legendary");

                return new ValuePerRarity { Common = common, Uncommon = uncommon, Rare = rare, Epic = epic, Legendary = legendary };
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return default;
            }
        }

        private Dictionary<int, TileSet> ResponseStringToTileSetDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, TileSet> result = new Dictionary<int, TileSet>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("TileSets");

                while (sectionString != "")
                {
                    string tileSetString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("TileSet", ref tileSetString);

                    Tuple<int, TileSet> tileSetId_tileSet = StringToTileSet(tileSetString);
                    result.Add(tileSetId_tileSet.Item1, tileSetId_tileSet.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, TileSet> StringToTileSet(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                int tileOccurrence_normal = _string.GetTagPortionValue<int>("Normal");
                int tileOccurrence_blue = _string.GetTagPortionValue<int>("Blue");
                int tileOccurrence_red = _string.GetTagPortionValue<int>("Red");
                int tileOccurrence_green = _string.GetTagPortionValue<int>("Green");
                int tileOccurrence_ocher = _string.GetTagPortionValue<int>("Ocher");
                int tileOccurrence_purple = _string.GetTagPortionValue<int>("Purple");
                int tileOccurrence_yellow = _string.GetTagPortionValue<int>("Yellow");
                int tileOccurrence_heal = _string.GetTagPortionValue<int>("Heal");

                List<eTileType> tileSet = new List<eTileType>();

                for (int i = 0; i < tileOccurrence_normal; i++) { tileSet.Add(eTileType.Normal); }
                for (int i = 0; i < tileOccurrence_blue; i++) { tileSet.Add(eTileType.Blue); }
                for (int i = 0; i < tileOccurrence_red; i++) { tileSet.Add(eTileType.Red); }
                for (int i = 0; i < tileOccurrence_green; i++) { tileSet.Add(eTileType.Green); }
                for (int i = 0; i < tileOccurrence_ocher; i++) { tileSet.Add(eTileType.Ocher); }
                for (int i = 0; i < tileOccurrence_purple; i++) { tileSet.Add(eTileType.Purple); }
                for (int i = 0; i < tileOccurrence_yellow; i++) { tileSet.Add(eTileType.Yellow); }
                for (int i = 0; i < tileOccurrence_heal; i++) { tileSet.Add(eTileType.Heal); }

                return new Tuple<int, TileSet>(id, new TileSet(tileSet));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<T> ResponseStringToStoryList<T>(string _response, bool _areMainStories) where T : Story
        {
            if (_response == null)
                return null;

            try
            {
                List<T> result = new List<T>();

                string outerTagTitle = _areMainStories ? "MainStories" : "EventStories";
                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags(outerTagTitle);

                while (sectionString != "")
                {
                    string storyString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Story", ref storyString);

                    result.Add(StringToStory(storyString, !_areMainStories) as T);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Story StringToStory(string _string, bool _isEventStory)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");
                string title = _string.GetTagPortionValue<string>("Title");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("BannerImageAsBytes");
                byte[] bannerImageAsBytes = Convert.FromBase64String(sectionString);

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Arcs");
                List<StoryArc> arcs = new List<StoryArc>();
                while (sectionString != "")
                {
                    string arcString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Arc", ref arcString);

                    arcs.Add(StringToStoryArc(arcString));
                }

                if (_isEventStory)
                    return new EventStory(id, title, bannerImageAsBytes, arcs);
                else
                    return new MainStory(id, title, bannerImageAsBytes, arcs);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private StoryArc StringToStoryArc(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                string title = _string.GetTagPortionValue<string>("Title");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("BannerImageAsBytes");
                byte[] bannerImageAsBytes = Convert.FromBase64String(sectionString);

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Chapters");
                List<StoryChapter> chapters = new List<StoryChapter>();
                while (sectionString != "")
                {
                    string chapterString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Chapter", ref chapterString);

                    chapters.Add(StringToStoryChapter(chapterString));
                }

                return new StoryArc(title, bannerImageAsBytes, chapters);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private StoryChapter StringToStoryChapter(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                string title = _string.GetTagPortionValue<string>("Title");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("BannerImageAsBytes");
                byte[] bannerImageAsBytes = Convert.FromBase64String(sectionString);

                string mapName = _string.GetTagPortionValue<string>("MapName");
                byte[] mapImageAsBytes = m_chapterMapImageDictionary[mapName];

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Episodes");
                List<StoryEpisode> episodes = new List<StoryEpisode>();
                while (sectionString != "")
                {
                    string episodeString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Episode", ref episodeString);

                    episodes.Add(StringToStoryEpisode(episodeString));
                }

                return new StoryChapter(title, bannerImageAsBytes, mapImageAsBytes, episodes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private StoryEpisode StringToStoryEpisode(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                string title = _string.GetTagPortionValue<string>("Title");

                string locationName = _string.GetTagPortionValue<string>("LocationName");

                int locationIconId = _string.GetTagPortionValue<int>("LocationIconId");
                byte[] locationIconAsBytes = m_episodeLocationIconDictionary[locationIconId];

                int chapterMapCoordX = _string.GetTagPortionValue<int>("ChapterMapCoordX");
                int chapterMapCoordY = _string.GetTagPortionValue<int>("ChapterMapCoordY");
                _2DCoord chapterMapCoord = new _2DCoord(chapterMapCoordX, chapterMapCoordY);

                int locationIconSize = _string.GetTagPortionValue<int>("LocationIconSize");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("NovelSceneBeforeBattle");
                NovelScene novelSceneBeforeBattle = StringToNovelScene(sectionString);

                Dungeon dungeon = null;
                if (_string.Contains("Dungeon"))
                {
                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Dungeon");
                    dungeon = StringToDungeon(sectionString);
                }

                NovelScene? novelSceneAfterBattle = null;
                if (_string.Contains("NovelSceneAfterBattle"))
                {
                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("NovelSceneAfterBattle");
                    novelSceneAfterBattle = StringToNovelScene(sectionString);
                }

                return new StoryEpisode(title, locationName, locationIconAsBytes, chapterMapCoord, locationIconSize, novelSceneBeforeBattle, dungeon, novelSceneAfterBattle);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private NovelScene StringToNovelScene(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                string sectionString = string.Empty;

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Subscenes");
                List<NovelSubscene> subscenes = new List<NovelSubscene>();
                while (sectionString != "")
                {
                    string subsceneString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Subscene", ref subsceneString);

                    subscenes.Add(StringToNovelSubscene(subsceneString));
                }

                return new NovelScene(subscenes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return default;
            }
        }

        private NovelSubscene StringToNovelSubscene(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                string sectionString = string.Empty;

                int backgroundImageId = _string.GetTagPortionValue<int>("BackgroundImageId");
                byte[] backgroundImageAsBytes = m_novelBackgroundImageDictionary[backgroundImageId];

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("NovelLineIds");
                List<NovelLine> lines = new List<NovelLine>();
                while (sectionString != "")
                {
                    int lineId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("NovelLineId", ref lineId);
                    NovelLine line = m_novelLineDictionary[lineId];

                    lines.Add(line);
                }

                return new NovelSubscene(backgroundImageAsBytes, lines);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return default;
            }
        }

        private Dictionary<int, NovelCharacterSpritePartsIndication> ResponseStringToNovelCharacterSpritePartsIndicationDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, NovelCharacterSpritePartsIndication> result = new Dictionary<int, NovelCharacterSpritePartsIndication>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("NovelCharacterSpritePartsIndications");

                while (sectionString != "")
                {
                    string indicationString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("NovelCharacterSpritePartsIndication", ref indicationString);

                    Tuple<int, NovelCharacterSpritePartsIndication> id_indication = StringToNovelCharacterSpritePartsIndication(indicationString);
                    result.Add(id_indication.Item1, id_indication.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, NovelCharacterSpritePartsIndication> StringToNovelCharacterSpritePartsIndication(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string characterName = _string.GetTagPortionValue<string>("CharacterName");

                bool showTearsSprite = _string.GetTagPortionValue<bool>("ShowTearsSprite");
                bool showWaterfallTearsSprite = _string.GetTagPortionValue<bool>("ShowWaterfallTearsSprite");
                bool showShySprite = _string.GetTagPortionValue<bool>("ShowShySprite");
                bool showEmbarrassmentSprite = _string.GetTagPortionValue<bool>("ShowEmbarrassmentSprite");
                bool showRedNoseSprite = _string.GetTagPortionValue<bool>("ShowRedNoseSprite");
                bool showBlackFaceMaskSprite = _string.GetTagPortionValue<bool>("ShowBlackFaceMaskSprite");
                bool showBlueFaceMaskSprite = _string.GetTagPortionValue<bool>("ShowBlueFaceMaskSprite");
                bool showRedFaceMaskSprite = _string.GetTagPortionValue<bool>("ShowRedFaceMaskSprite");

                eNovelCharacterFaceType faceType = _string.GetTagPortionValue<eNovelCharacterFaceType>("FaceType");
                eNovelCharacterMouthType mouthType = _string.GetTagPortionValue<eNovelCharacterMouthType>("MouthType");
                eNovelEmotionIconType? emotionIconType = null;
                if (_string.Contains("EmotionIconType"))
                    emotionIconType = _string.GetTagPortionValue<eNovelEmotionIconType>("EmotionIconType");

                return new Tuple<int, NovelCharacterSpritePartsIndication>(id, new NovelCharacterSpritePartsIndication(characterName, showTearsSprite, showWaterfallTearsSprite, showShySprite, showEmbarrassmentSprite, showRedNoseSprite, showBlackFaceMaskSprite, showBlueFaceMaskSprite, showRedFaceMaskSprite, faceType, mouthType, emotionIconType));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return default;
            }
        }

        private Dictionary<int, NovelLine> ResponseStringToNovelLineDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, NovelLine> result = new Dictionary<int, NovelLine>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("NovelLines");

                while (sectionString != "")
                {
                    string novelLineString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("NovelLine", ref novelLineString);

                    Tuple<int, NovelLine> id_novelLine = StringToNovelLine(novelLineString);
                    result.Add(id_novelLine.Item1, id_novelLine.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, NovelLine> StringToNovelLine(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string line = _string.GetTagPortionValue<string>("Line");

                eNovelCharacterPosition characterPosition = _string.GetTagPortionValue<eNovelCharacterPosition>("CharacterPosition");

                NovelCharacterSpritePartsIndication? characterSpritePartsIndication = null;
                if (_string.Contains("CharacterSpritePartsIndicationId"))
                {
                    int characterSpritePartsIndicationId = _string.GetTagPortionValue<int>("CharacterSpritePartsIndicationId");
                    characterSpritePartsIndication = m_novelCharacterSpritePartsIndicationDictionary[characterSpritePartsIndicationId];
                }

                return new Tuple<int, NovelLine>(id, new NovelLine(line, characterPosition, characterSpritePartsIndication));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return default;
            }
        }

        private Dungeon StringToDungeon(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                string name = _string.GetTagPortionValue<string>("Name");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("Floors");
                List<Floor> floors = new List<Floor>();
                while (sectionString != "")
                {
                    string floorString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Floor", ref floorString);

                    floors.Add(StringToFloor(floorString));
                }

                return new Dungeon(name, floors);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Floor StringToFloor(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int tileSetId = _string.GetTagPortionValue<int>("TileSetId");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("DungeonUnitInfos");
                List<DungeonUnitInfo> dungeonUnitInfos = new List<DungeonUnitInfo>();
                while (sectionString != "")
                {
                    string dungeonUnitInfoString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("DungeonUnitInfo", ref dungeonUnitInfoString);

                    dungeonUnitInfos.Add(StringToDungeonUnitInfo(dungeonUnitInfoString));
                }

                return new Floor(tileSetId, dungeonUnitInfos);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private DungeonUnitInfo StringToDungeonUnitInfo(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int unitId = _string.GetTagPortionValue<int>("UnitId");
                UnitData unitData = m_unitDataList.First(x => x.Id == unitId);

                int minLevel = _string.GetTagPortionValue<int>("MinLevel");
                int maxLevel = _string.GetTagPortionValue<int>("MaxLevel");

                decimal dropRate = _string.GetTagPortionValue<decimal>("DropRate");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("DropItemInfos");
                List<DropItemInfo> dropItemInfos = new List<DropItemInfo>();
                while (sectionString != "")
                {
                    string dropItemInfoString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("DropItemInfo", ref dropItemInfoString);

                    dropItemInfos.Add(StringToDropItemInfo(dropItemInfoString));
                }

                return new DungeonUnitInfo(unitData, minLevel, maxLevel, dropRate, dropItemInfos);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private DropItemInfo StringToDropItemInfo(string _string)
        {
            if (_string == null)
                return default;

            try
            {
                string sectionString = string.Empty;

                int itemId = _string.GetTagPortionValue<int>("ItemId");
                Item item = m_itemList.First(x => x.Id == itemId);

                decimal dropRate = _string.GetTagPortionValue<decimal>("DropRate");

                return new DropItemInfo(item, dropRate);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return default;
            }
        }

        private List<WeaponData> ResponseStringToWeaponDataList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<WeaponData> result = new List<WeaponData>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Weapons");

                while (sectionString != "")
                {
                    string weaponDataString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("WeaponData", ref weaponDataString);

                    result.Add(StringToWeaponData(weaponDataString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private WeaponData StringToWeaponData(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string name = _string.GetTagPortionValue<string>("Name");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("IconAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                eRarity rarity = _string.GetTagPortionValue<eRarity>("Rarity");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("StatusEffectDataIds");
                List<StatusEffectData> statusEffectsData = new List<StatusEffectData>();
                while (sectionString != "")
                {
                    int statusEffectDataId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("StatusEffectDataId", ref statusEffectDataId);

                    statusEffectsData.Add(m_statusEffectDataList.First(x => x.Id == statusEffectDataId));
                }

                eWeaponType weaponType = _string.GetTagPortionValue<eWeaponType>("WeaponType");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("WeaponClassifications");
                List<eWeaponClassification> weaponClassifications = new List<eWeaponClassification>();
                while (sectionString != "")
                {
                    eWeaponClassification weaponClassification = default;
                    sectionString = sectionString.DetachTagPortionAsValue("WeaponClassification", ref weaponClassification);

                    weaponClassifications.Add(weaponClassification);
                }

                Skill mainWeaponSkill = null;
                if (_string.Contains("MainWeaponSkillId"))
                {
                    int mainWeaponSkillId = _string.GetTagPortionValue<int>("MainWeaponSkillId");
                    SkillData mainWeaponSkillData = m_skillList.First(x => x.Id == mainWeaponSkillId);
                    int mainWeaponSkillLevel = _string.GetTagPortionValue<int>("MainWeaponSkillLevel");
                    if (mainWeaponSkillData is OrdinarySkillData)
                        mainWeaponSkill = new OrdinarySkill(mainWeaponSkillData as OrdinarySkillData, mainWeaponSkillLevel);
                    else if (mainWeaponSkillData is CounterSkillData)
                        mainWeaponSkill = new CounterSkill(mainWeaponSkillData as CounterSkillData, mainWeaponSkillLevel);
                    else if (mainWeaponSkillData is UltimateSkillData)
                        mainWeaponSkill = new UltimateSkill(mainWeaponSkillData as UltimateSkillData, mainWeaponSkillLevel);
                    else if (mainWeaponSkillData is PassiveSkillData)
                        mainWeaponSkill = new PassiveSkill(mainWeaponSkillData as PassiveSkillData, mainWeaponSkillLevel);
                }

                if (weaponType == eWeaponType.Transformable || weaponType == eWeaponType.LevelableTransformable)
                {
                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("TargetWeaponsInCaseTypeIsTransformable");
                    while (sectionString != "")
                    {
                        int targetWeaponDataId = default;
                        sectionString = sectionString.DetachTagPortionAsValue("WeaponId", ref targetWeaponDataId);

                        m_weaponId_transformableWeaponId.Add(id, targetWeaponDataId);
                    }
                }

                return new WeaponData(id, name, iconAsBytes, rarity, statusEffectsData, weaponType, weaponClassifications, mainWeaponSkill, new List<WeaponData>());
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<ArmourData> ResponseStringToArmourDataList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<ArmourData> result = new List<ArmourData>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Armours");

                while (sectionString != "")
                {
                    string armourDataString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ArmourData", ref armourDataString);

                    result.Add(StringToArmourData(armourDataString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ArmourData StringToArmourData(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string name = _string.GetTagPortionValue<string>("Name");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("IconAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                eRarity rarity = _string.GetTagPortionValue<eRarity>("Rarity");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("StatusEffectDataIds");
                List<StatusEffectData> statusEffectsData = new List<StatusEffectData>();
                while (sectionString != "")
                {
                    int statusEffectDataId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("StatusEffectDataId", ref statusEffectDataId);

                    statusEffectsData.Add(m_statusEffectDataList.First(x => x.Id == statusEffectDataId));
                }

                eArmourClassification armourClassification = _string.GetTagPortionValue<eArmourClassification>("ArmourClassification");

                eGender targetGender = _string.GetTagPortionValue<eGender>("TargetGender");

                return new ArmourData(id, name, iconAsBytes, rarity, statusEffectsData, armourClassification, targetGender);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<AccessoryData> ResponseStringToAccessoryDataList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<AccessoryData> result = new List<AccessoryData>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Accessories");

                while (sectionString != "")
                {
                    string accessoryDataString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ArmourData", ref accessoryDataString);

                    result.Add(StringToAccessoryData(accessoryDataString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private AccessoryData StringToAccessoryData(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string name = _string.GetTagPortionValue<string>("Name");

                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                eRarity rarity = _string.GetTagPortionValue<eRarity>("Rarity");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("StatusEffectDataIds");
                List<StatusEffectData> statusEffectsData = new List<StatusEffectData>();
                while (sectionString != "")
                {
                    int statusEffectDataId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("StatusEffectDataId", ref statusEffectDataId);

                    statusEffectsData.Add(m_statusEffectDataList.First(x => x.Id == statusEffectDataId));
                }

                eAccessoryClassification accessoryClassification = _string.GetTagPortionValue<eAccessoryClassification>("AccessoryClassification");

                eGender targetGender = _string.GetTagPortionValue<eGender>("TargetGender");

                return new AccessoryData(id, name, iconAsBytes, rarity, statusEffectsData, accessoryClassification, targetGender);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<Item> ResponseStringToItemList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<Item> result = new List<Item>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Items");

                while (sectionString != "")
                {
                    string itemString = string.Empty;

                    if (sectionString.StartsWith("<SkillItem>"))
                        sectionString = sectionString.DetachTagPortion("SkillItem", ref itemString);
                    else if (sectionString.StartsWith("<SkillMaterial>"))
                        sectionString = sectionString.DetachTagPortion("SkillMaterial", ref itemString);
                    else if (sectionString.StartsWith("<ItemMaterial>"))
                        sectionString = sectionString.DetachTagPortion("ItemMaterial", ref itemString);
                    else if (sectionString.StartsWith("<EquipmentMaterial>"))
                        sectionString = sectionString.DetachTagPortion("EquipmentMaterial", ref itemString);
                    else if (sectionString.StartsWith("<EvolutionMaterial>"))
                        sectionString = sectionString.DetachTagPortion("EvolutionMaterial", ref itemString);
                    else if (sectionString.StartsWith("<WeaponEnhancementMaterial>"))
                        sectionString = sectionString.DetachTagPortion("WeaponEnhancementMaterial", ref itemString);
                    else if (sectionString.StartsWith("<UnitEnhancementMaterial>"))
                        sectionString = sectionString.DetachTagPortion("UnitEnhancementMaterial", ref itemString);
                    else if (sectionString.StartsWith("<SkillEnhancementMaterial>"))
                        sectionString = sectionString.DetachTagPortion("SkillEnhancementMaterial", ref itemString);
                    else if (sectionString.StartsWith("<GachaCostItem>"))
                        sectionString = sectionString.DetachTagPortion("GachaCostItem", ref itemString);
                    else if (sectionString.StartsWith("<EquipmentTradingItem>"))
                        sectionString = sectionString.DetachTagPortion("EquipmentTradingItem", ref itemString);
                    else if (sectionString.StartsWith("<UnitTradingItem>"))
                        sectionString = sectionString.DetachTagPortion("UnitTradingItem", ref itemString);

                    result.Add(StringToItem(itemString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Item StringToItem(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string name = _string.GetTagPortionValue<string>("Name");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("IconAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                eRarity rarity = _string.GetTagPortionValue<eRarity>("Rarity");

                int sellingPrice = _string.GetTagPortionValue<int>("SellingPrice");

                if (_string.StartsWith("<SkillItem>"))
                {
                    int skillId = _string.GetTagPortionValue<int>("SkillId");
                    SkillData skillData = m_skillList.First(x => x.Id == skillId);
                    int skillLevel = _string.GetTagPortionValue<int>("SkillLevel");
                    ActiveSkill skill = null;
                    if (skillData is OrdinarySkillData)
                        skill = new OrdinarySkill(skillData as OrdinarySkillData, skillLevel);
                    else if (skillData is CounterSkillData)
                        skill = new CounterSkill(skillData as CounterSkillData, skillLevel);
                    else /*if (skillData is UltimateSkillData)*/
                        skill = new UltimateSkill(skillData as UltimateSkillData, skillLevel);

                    return new SkillItem(id, name, iconAsBytes, rarity, sellingPrice, skill);
                }
                else if (_string.StartsWith("<SkillMaterial>"))
                    return new SkillMaterial(id, name, iconAsBytes, rarity, sellingPrice);
                else if (_string.StartsWith("<ItemMaterial>"))
                    return new ItemMaterial(id, name, iconAsBytes, rarity, sellingPrice);
                else if (_string.StartsWith("<EquipmentMaterial>"))
                    return new EquipmentMaterial(id, name, iconAsBytes, rarity, sellingPrice);
                else if (_string.StartsWith("<EvolutionMaterial>"))
                    return new EvolutionMaterial(id, name, iconAsBytes, rarity, sellingPrice);
                else if (_string.StartsWith("<WeaponEnhancementMaterial>"))
                {
                    int expToAppy = _string.GetTagPortionValue<int>("EnhancementValue");

                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("TargetingWeaponClassifications");
                    List<eWeaponClassification> targetingWeaponClassifications = new List<eWeaponClassification>();
                    while (sectionString != "")
                    {
                        eWeaponClassification classification = default;
                        sectionString = sectionString.DetachTagPortionAsValue("Classification", ref classification);

                        targetingWeaponClassifications.Add(classification);
                    }

                    return new WeaponEnhancementMaterial(id, name, iconAsBytes, rarity, sellingPrice, expToAppy, targetingWeaponClassifications);
                }
                else if (_string.StartsWith("<UnitEnhancementMaterial>"))
                {
                    int expToAppy = _string.GetTagPortionValue<int>("EnhancementValue");

                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("BonusElements");
                    List<eElement> bonusElements = new List<eElement>();
                    while (sectionString != "")
                    {
                        eElement element = default;
                        sectionString = sectionString.DetachTagPortionAsValue("Element", ref element);

                        bonusElements.Add(element);
                    }

                    return new UnitEnhancementMaterial(id, name, iconAsBytes, rarity, sellingPrice, expToAppy, bonusElements);
                }
                else if (_string.StartsWith("<SkillEnhancementMaterial>"))
                {
                    int levelsToEnhance = _string.GetTagPortionValue<int>("EnhancementValue");

                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("TargetingRarities");
                    List<eRarity> targetingRarities = new List<eRarity>();
                    while (sectionString != "")
                    {
                        eRarity targetingRarity = default;
                        sectionString = sectionString.DetachTagPortionAsValue("Rarity", ref targetingRarity);

                        targetingRarities.Add(targetingRarity);
                    }

                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("TargetingElements");
                    List<eElement> targetingElements = new List<eElement>();
                    while (sectionString != "")
                    {
                        eElement element = default;
                        sectionString = sectionString.DetachTagPortionAsValue("Element", ref element);

                        targetingElements.Add(element);
                    }

                    sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("TargetingLabelIds");
                    List<string> targetingLabels = new List<string>();
                    while (sectionString != "")
                    {
                        int labelId = default;
                        sectionString = sectionString.DetachTagPortionAsValue("LabelId", ref labelId);

                        targetingLabels.Add(m_labelDictionary[labelId]);
                    }

                    return new SkillEnhancementMaterial(id, name, iconAsBytes, rarity, sellingPrice, levelsToEnhance, targetingRarities, targetingElements, targetingLabels);
                }
                else if (_string.StartsWith("<GachaCostItem>"))
                    return new GachaCostItem(id, name, iconAsBytes, rarity, sellingPrice);
                else if (_string.StartsWith("<EquipmentTradingItem>"))
                    return new EquipmentTradingItem(id, name, iconAsBytes, rarity, sellingPrice);
                else if (_string.StartsWith("<UnitTradingItem>"))
                    return new UnitTradingItem(id, name, iconAsBytes, rarity, sellingPrice);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<WeaponRecipe> ResponseStringToWeaponRecipeList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<WeaponRecipe> result = new List<WeaponRecipe>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("WeaponRecipes");

                while (sectionString != "")
                {
                    string weaponRecipeString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("WeaponRecipe", ref weaponRecipeString);

                    result.Add(StringToWeaponRecipe(weaponRecipeString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private WeaponRecipe StringToWeaponRecipe(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int productId = _string.GetTagPortionValue<int>("ProductId");
                WeaponData productWeaponData = m_weaponDataList.First(x => x.Id == productId);

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("MaterialIds");
                List<EquipmentMaterial> materials = new List<EquipmentMaterial>();
                while (sectionString != "")
                {
                    int materialId = default;
                    sectionString = sectionString.DetachTagPortionAsValue<int>("MaterialId", ref materialId);

                    materials.Add(m_itemList.OfType<EquipmentMaterial>().First(x => x.Id == materialId));
                }

                WeaponData upgradingWeaponData = null;
                if (_string.Contains("UpgradingWeaponId"))
                {
                    int upgradingWeaponId = _string.GetTagPortionValue<int>("UpgradingWeaponId");
                    upgradingWeaponData = m_weaponDataList.First(x => x.Id == upgradingWeaponId);
                }

                int cost = _string.GetTagPortionValue<int>("Cost");

                return new WeaponRecipe(productWeaponData, materials, cost, upgradingWeaponData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<ArmourRecipe> ResponseStringToArmourRecipeList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<ArmourRecipe> result = new List<ArmourRecipe>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ArmourRecipes");

                while (sectionString != "")
                {
                    string armourRecipeString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ArmourRecipe", ref armourRecipeString);

                    result.Add(StringToArmourRecipe(armourRecipeString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ArmourRecipe StringToArmourRecipe(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int productId = _string.GetTagPortionValue<int>("ProductId");
                ArmourData productArmourData = m_armourDataList.First(x => x.Id == productId);

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("MaterialIds");
                List<EquipmentMaterial> materials = new List<EquipmentMaterial>();
                while (sectionString != "")
                {
                    int materialId = default;
                    sectionString = sectionString.DetachTagPortionAsValue<int>("MaterialId", ref materialId);

                    materials.Add(m_itemList.OfType<EquipmentMaterial>().First(x => x.Id == materialId));
                }

                ArmourData upgradingArmourData = null;
                if (_string.Contains("UpgradingArmourId"))
                {
                    int upgradingArmourId = _string.GetTagPortionValue<int>("UpgradingArmourId");
                    upgradingArmourData = m_armourDataList.First(x => x.Id == upgradingArmourId);
                }

                int cost = _string.GetTagPortionValue<int>("Cost");

                return new ArmourRecipe(productArmourData, materials, cost, upgradingArmourData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<AccessoryRecipe> ResponseStringToAccessoryRecipeList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<AccessoryRecipe> result = new List<AccessoryRecipe>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("AccessoryRecipes");

                while (sectionString != "")
                {
                    string accessoryRecipeString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("AccessoryRecipe", ref accessoryRecipeString);

                    result.Add(StringToAccessoryRecipe(accessoryRecipeString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private AccessoryRecipe StringToAccessoryRecipe(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int productId = _string.GetTagPortionValue<int>("ProductId");
                AccessoryData productAccessoryData = m_accessoryDataList.First(x => x.Id == productId);

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("MaterialIds");
                List<EquipmentMaterial> materials = new List<EquipmentMaterial>();
                while (sectionString != "")
                {
                    int materialId = default;
                    sectionString = sectionString.DetachTagPortionAsValue<int>("MaterialId", ref materialId);

                    materials.Add(m_itemList.OfType<EquipmentMaterial>().First(x => x.Id == materialId));
                }

                AccessoryData upgradingAccessoryData = null;
                if (_string.Contains("UpgradingAccessoryId"))
                {
                    int upgradingAccessoryId = _string.GetTagPortionValue<int>("UpgradingAccessoryId");
                    upgradingAccessoryData = m_accessoryDataList.First(x => x.Id == upgradingAccessoryId);
                }

                int cost = _string.GetTagPortionValue<int>("Cost");

                return new AccessoryRecipe(productAccessoryData, materials, cost, upgradingAccessoryData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<ItemRecipe> ResponseStringToItemRecipeList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<ItemRecipe> result = new List<ItemRecipe>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ItemRecipes");

                while (sectionString != "")
                {
                    string itemRecipeString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ItemRecipe", ref itemRecipeString);

                    result.Add(StringToItemRecipe(itemRecipeString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ItemRecipe StringToItemRecipe(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int productId = _string.GetTagPortionValue<int>("ProductId");
                Item productItemData = m_itemList.First(x => x.Id == productId);

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("MaterialIds");
                List<ItemMaterial> materials = new List<ItemMaterial>();
                while (sectionString != "")
                {
                    int materialId = default;
                    sectionString = sectionString.DetachTagPortionAsValue<int>("MaterialId", ref materialId);

                    materials.Add(m_itemList.OfType<ItemMaterial>().First(x => x.Id == materialId));
                }

                Item upgradingItemData = null;
                if (_string.Contains("UpgradingItemId"))
                {
                    int upgradingItemId = _string.GetTagPortionValue<int>("UpgradingItemId");
                    upgradingItemData = m_itemList.First(x => x.Id == upgradingItemId);
                }

                int cost = _string.GetTagPortionValue<int>("Cost");

                return new ItemRecipe(productItemData, materials, cost, upgradingItemData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, string> ResponseStringToLabelDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, string> result = new Dictionary<int, string>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Labels");

                while (sectionString != "")
                {
                    string labelString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Label", ref labelString);

                    Tuple<int, string> id_label = StringToLabel(labelString);
                    result.Add(id_label.Item1, id_label.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, string> StringToLabel(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string label = _string.GetTagPortionValue<string>("String");

                return new Tuple<int, string>(id, label);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, byte[]> ResponseStringToStatusEffectIconDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("StatusEffectIcons");

                while (sectionString != "")
                {
                    string statusEffectIconString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("StatusEffectIcon", ref statusEffectIconString);

                    Tuple<int, byte[]> id_statusEffectIcon = StringToStatusEffectIcon(statusEffectIconString);
                    result.Add(id_statusEffectIcon.Item1, id_statusEffectIcon.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, byte[]> StringToStatusEffectIcon(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("IconAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                return new Tuple<int, byte[]>(id, iconAsBytes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, byte[]> ResponseStringToSkillIconDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("SkillIcons");

                while (sectionString != "")
                {
                    string statusEffectIconString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("SkillIcon", ref statusEffectIconString);

                    Tuple<int, byte[]> id_skillIcon = StringToSkillIcon(statusEffectIconString);
                    result.Add(id_skillIcon.Item1, id_skillIcon.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, byte[]> StringToSkillIcon(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("IconAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                return new Tuple<int, byte[]>(id, iconAsBytes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<string, byte[]> ResponseStringToChapterMapImageDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ChapterMapImages");

                while (sectionString != "")
                {
                    string chapterMapImageString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ChapterMapImage", ref chapterMapImageString);

                    Tuple<string, byte[], int> mapName_chapterMapImage_mapChipSize = StringToChapterMapImage(chapterMapImageString);

                    m_chapterMapChipSizeDictionary.Add(mapName_chapterMapImage_mapChipSize.Item2, mapName_chapterMapImage_mapChipSize.Item3);
                    result.Add(mapName_chapterMapImage_mapChipSize.Item1, mapName_chapterMapImage_mapChipSize.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<string, byte[], int> StringToChapterMapImage(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                string mapName = _string.GetTagPortionValue<string>("MapName");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("ImageAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                int mapChipSize = _string.GetTagPortionValue<int>("MapChipSize");

                return new Tuple<string, byte[], int>(mapName, iconAsBytes, mapChipSize);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, byte[]> ResponseStringToEpisodeLocationIconDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("EpisodeLocationIcons");

                while (sectionString != "")
                {
                    string statusEffectIconString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("EpisodeLocationIcon", ref statusEffectIconString);

                    Tuple<int, byte[]> id_episodeLocationIcon = StringToEpisodeLocationIcon(statusEffectIconString);
                    result.Add(id_episodeLocationIcon.Item1, id_episodeLocationIcon.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, byte[]> StringToEpisodeLocationIcon(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("IconAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                return new Tuple<int, byte[]>(id, iconAsBytes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, byte[]> ResponseStringToNovelBackgroundImageDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("NovelBackgroundImages");

                while (sectionString != "")
                {
                    string statusEffectImageString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("NovelBackgroundImage", ref statusEffectImageString);

                    Tuple<int, byte[]> id_novelBackgroundImage = StringToNovelBackgroundImage(statusEffectImageString);
                    result.Add(id_novelBackgroundImage.Item1, id_novelBackgroundImage.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, byte[]> StringToNovelBackgroundImage(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("ImageAsBytes");
                byte[] iconAsBytes = Convert.FromBase64String(sectionString);

                return new Tuple<int, byte[]>(id, iconAsBytes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<string, byte[]> ResponseStringToNovelCharacterSpriteSheetDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("NovelCharacterSpriteSheets");

                while (sectionString != "")
                {
                    string novelCharacterSpriteSheetString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("NovelCharacterSpriteSheet", ref novelCharacterSpriteSheetString);

                    Tuple<string, byte[]> characterName_spriteSheet = StringToNovelCharacterSpriteSheet(novelCharacterSpriteSheetString);
                    result.Add(characterName_spriteSheet.Item1, characterName_spriteSheet.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<string, byte[]> StringToNovelCharacterSpriteSheet(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                string characterName = _string.GetTagPortionValue<string>("CharacterName");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("SpriteSheetAsBytes");
                byte[] spriteSheetAsBytes = Convert.FromBase64String(sectionString);

                return new Tuple<string, byte[]>(characterName, spriteSheetAsBytes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, Tag> ResponseStringToTagDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, Tag> result = new Dictionary<int, Tag>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Tags");

                while (sectionString != "")
                {
                    string tagString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Tag", ref tagString);

                    Tuple<int, Tag> id_tag = StringToTag(tagString);
                    result.Add(id_tag.Item1, id_tag.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, Tag> StringToTag(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("CompleteTagString");
                Tag tag = Tag.NewTag(sectionString);

                return new Tuple<int, Tag>(id, tag);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, Condition> ResponseStringToConditionDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, Condition> result = new Dictionary<int, Condition>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Conditions");

                while (sectionString != "")
                {
                    string conditionString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Condition", ref conditionString);

                    Tuple<int, Condition> id_condition = StringToCondition(conditionString);
                    result.Add(id_condition.Item1, id_condition.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, Condition> StringToCondition(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                int tagAId = _string.GetTagPortionValue<int>("TagAId");
                Tag tagA = m_tagDictionary[tagAId];

                eRelationType relationType = _string.GetTagPortionValue<eRelationType>("RelationType");

                int tagBId = _string.GetTagPortionValue<int>("TagBId");
                Tag tagB = m_tagDictionary[tagBId];

                return new Tuple<int, Condition>(id, new Condition(tagA, relationType, tagB));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, ConditionSet> ResponseStringToConditionSetDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, ConditionSet> result = new Dictionary<int, ConditionSet>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ConditionSets");

                while (sectionString != "")
                {
                    string conditionSetString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ConditionSet", ref conditionSetString);

                    Tuple<int, ConditionSet> id_conditionSet = StringToConditionSet(conditionSetString);
                    result.Add(id_conditionSet.Item1, id_conditionSet.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, ConditionSet> StringToConditionSet(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                int id = _string.GetTagPortionValue<int>("Id");

                List<Condition> conditions = new List<Condition>();
                string sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("ConditionIds");
                while (sectionString != "")
                {
                    string conditionIdString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ConditionId", ref conditionIdString);

                    int conditionId = conditionIdString.GetTagPortionValue<int>("ConditionId");
                    Condition condition = m_conditionDictionary[conditionId];

                    conditions.Add(condition);
                }

                return new Tuple<int, ConditionSet>(id, new ConditionSet(conditions));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, AnimationInfo> ResponseStringToAnimationInfoDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, AnimationInfo> result = new Dictionary<int, AnimationInfo>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("AnimationInfos");

                while (sectionString != "")
                {
                    string animationInfoString = string.Empty;

                    if (sectionString.StartsWith("<SimpleAnimationInfo>"))
                        sectionString = sectionString.DetachTagPortion("SimpleAnimationInfo", ref animationInfoString);
                    else if (sectionString.StartsWith("<ProjectileAnimationInfo>"))
                        sectionString = sectionString.DetachTagPortion("ProjectileAnimationInfo", ref animationInfoString);
                    else if (sectionString.StartsWith("<LaserAnimationInfo>"))
                        sectionString = sectionString.DetachTagPortion("LaserAnimationInfo", ref animationInfoString);
                    else if (sectionString.StartsWith("<MovementAnimationInfo>"))
                        sectionString = sectionString.DetachTagPortion("MovementAnimationInfo", ref animationInfoString);
                    else
                        return result;

                    Tuple<int, AnimationInfo> id_animationInfo = StringToAnimationInfo(animationInfoString);
                    result.Add(id_animationInfo.Item1, id_animationInfo.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, AnimationInfo> StringToAnimationInfo(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                AnimationInfo animationInfo = null;

                int id = _string.GetTagPortionValue<int>("Id");

                if (_string.StartsWith("<SimpleAnimationInfo>"))
                {
                    int hitEffectId = _string.GetTagPortionValue<int>("HitEffectId");

                    animationInfo = new SimpleAnimationInfo(hitEffectId);
                }
                else if (_string.StartsWith("<ProjectileAnimationInfo>"))
                {
                    int projectileGenerationPointId = _string.GetTagPortionValue<int>("ProjectileGenerationPointId"); 
                    int projectileId = _string.GetTagPortionValue<int>("ProjectileId");
                    int hitEffectId = _string.GetTagPortionValue<int>("HitEffectId");

                    animationInfo = new ProjectileAnimationInfo(projectileGenerationPointId, projectileId, hitEffectId);
                }
                else if (_string.StartsWith("<LaserAnimationInfo>"))
                {
                    int laserGenerationPointId = _string.GetTagPortionValue<int>("LaserGenerationPointId");
                    int laserEffectId = _string.GetTagPortionValue<int>("LaserEffectId");
                    int hitEffectId = _string.GetTagPortionValue<int>("HitEffectId");

                    animationInfo = new LaserAnimationInfo(laserGenerationPointId, laserEffectId, hitEffectId);
                }
                else if (_string.StartsWith("<MovementAnimationInfo>"))
                {
                    int attachmentEffectId = _string.GetTagPortionValue<int>("AttachmentEffectId");

                    animationInfo = new MovementAnimationInfo(attachmentEffectId);
                }
                else
                    return null;

                return new Tuple<int, AnimationInfo>(id, animationInfo);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, DurationData> ResponseStringToDurationDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, DurationData> result = new Dictionary<int, DurationData>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Durations");

                while (sectionString != "")
                {
                    string durationString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Duration", ref durationString);

                    Tuple<int, DurationData> id_duration = StringToDurationData(durationString);
                    result.Add(id_duration.Item1, id_duration.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, DurationData> StringToDurationData(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                int activationTimesTagId = _string.GetTagPortionValue<int>("ActivationTimesTagId");
                Tag activationTimes = m_tagDictionary.GetValueOrElse(activationTimesTagId, Tag.Zero);

                int turnTagId = _string.GetTagPortionValue<int>("TurnsTagId");
                Tag turns = m_tagDictionary.GetValueOrElse(turnTagId, Tag.Zero);

                sectionString = _string.GetTagPortion("WhileCondition");
                ComplexCondition whileCondition = StringToComplexCondition(sectionString, "WhileCondition");

                return new Tuple<int, DurationData>(id, new DurationData(activationTimes, turns, whileCondition));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<StatusEffectData> ResponseStringToStatusEffectDataList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<StatusEffectData> result = new List<StatusEffectData>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("StatusEffects");

                while (sectionString != "")
                {
                    string statusEffectDataString = string.Empty;

                    if (sectionString.StartsWith("<BuffStatusEffectData>"))
                        sectionString = sectionString.DetachTagPortion("BuffStatusEffectData", ref statusEffectDataString);
                    else if (sectionString.StartsWith("<DebuffStatusEffectData>"))
                        sectionString = sectionString.DetachTagPortion("DebuffStatusEffectData", ref statusEffectDataString);
                    else if (sectionString.StartsWith("<TargetRangeModStatusEffectData>"))
                        sectionString = sectionString.DetachTagPortion("TargetRangeModStatusEffectData", ref statusEffectDataString);
                    else if (sectionString.StartsWith("<DamageStatusEffectData>"))
                        sectionString = sectionString.DetachTagPortion("DamageStatusEffectData", ref statusEffectDataString);
                    else if (sectionString.StartsWith("<HealStatusEffectData>"))
                        sectionString = sectionString.DetachTagPortion("HealStatusEffectData", ref statusEffectDataString);

                    result.Add(StringToStatusEffectData(statusEffectDataString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private StatusEffectData StringToStatusEffectData(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                int durationId = _string.GetTagPortionValue<int>("DurationId");
                DurationData durationData = m_durationDictionary[durationId];

                sectionString = _string.GetTagPortion("ActivationCondition");
                ComplexCondition activationCondition = StringToComplexCondition(sectionString, "ActivationCondition");

                int iconId = _string.GetTagPortionValue<int>("IconId");
                byte[] iconAsBytes = m_statusEffectIconDictionary[iconId];

                if (_string.StartsWith("<BuffStatusEffectData>")
                    || _string.StartsWith("<DebuffStatusEffectData>")
                    || _string.StartsWith("<TargetRangeModStatusEffectData>"))
                {
                    return StringToBackgroundStatusEffectData(_string, id, durationData, activationCondition, iconAsBytes);
                }
                else if (_string.StartsWith("<DamageStatusEffectData>")
                    || _string.StartsWith("<HealStatusEffectData>"))
                {
                    return StringToForegroundStatusEffectData(_string, id, durationData, activationCondition, iconAsBytes);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private BackgroundStatusEffectData StringToBackgroundStatusEffectData(string _string, int _id, DurationData _duration, ComplexCondition _activationCondition, byte[] _iconAsBytes)
        {
            if (_string == null)
                return null;

            try
            {
                eActivationTurnClassification activationTurnClassification = _string.GetTagPortionValue<eActivationTurnClassification>("ActivationTurnClassification");

                if (_string.StartsWith("<BuffStatusEffectData>"))
                    return StringToBuffStatusEffectData(_string, _id, _duration, activationTurnClassification, _activationCondition, _iconAsBytes);
                else if (_string.StartsWith("<DebuffStatusEffectData>"))
                    return StringToDebuffStatusEffectData(_string, _id, _duration, activationTurnClassification, _activationCondition, _iconAsBytes);
                else if (_string.StartsWith("<TargetRangeModStatusEffectData>"))
                    return StringToTargetRangeModStatusEffectData(_string, _id, _duration, activationTurnClassification, _activationCondition, _iconAsBytes);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private BuffStatusEffectData StringToBuffStatusEffectData(string _string, int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                eStatusType targetStatusType = _string.GetTagPortionValue<eStatusType>("TargetStatusType");

                int valueTagId = _string.GetTagPortionValue<int>("ValueTagId");
                Tag value = m_tagDictionary[valueTagId];

                bool isSum = _string.GetTagPortionValue<bool>("IsSum");

                return new BuffStatusEffectData(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes, targetStatusType, value, isSum);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private DebuffStatusEffectData StringToDebuffStatusEffectData(string _string, int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                eStatusType targetStatusType = _string.GetTagPortionValue<eStatusType>("TargetStatusType");

                int valueTagId = _string.GetTagPortionValue<int>("ValueTagId");
                Tag value = m_tagDictionary[valueTagId];

                bool isSum = _string.GetTagPortionValue<bool>("IsSum");

                return new DebuffStatusEffectData(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes, targetStatusType, value, isSum);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private TargetRangeModStatusEffectData StringToTargetRangeModStatusEffectData(string _string, int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes)
        {
            if (_string == null)
                return null;

            try
            {
                bool isMovementRangeClassification = _string.GetTagPortionValue<bool>("IsMovementRangeClassification");

                eTargetRangeClassification targetRangeClassification = _string.GetTagPortionValue<eTargetRangeClassification>("TargetRangeClassification");

                eModificationMethod modificationMethod = _string.GetTagPortionValue<eModificationMethod>("ModificationMethod");

                return new TargetRangeModStatusEffectData(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes, isMovementRangeClassification, targetRangeClassification, modificationMethod);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ForegroundStatusEffectData StringToForegroundStatusEffectData(string _string, int _id, DurationData _duration, ComplexCondition _activationCondition, byte[] _iconAsBytes)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                eActivationTurnClassification activationTurnClassification = _string.GetTagPortionValue<eActivationTurnClassification>("ActivationTurnClassification");

                eEventTriggerTiming eventTriggerTiming = _string.GetTagPortionValue<eEventTriggerTiming>("EventTriggerTiming");

                int animationInfoId = _string.GetTagPortionValue<int>("AnimationInfoId");
                SimpleAnimationInfo animationInfo = m_animationInfoDictionary[animationInfoId] as SimpleAnimationInfo;

                if (_string.StartsWith("<DamageStatusEffectData>"))
                    return StringToDamageStatusEffectData(_string, _id, _duration, activationTurnClassification, eventTriggerTiming, _activationCondition, _iconAsBytes, animationInfo);
                else if (_string.StartsWith("<HealStatusEffectData>"))
                    return StringToHealStatusEffectData(_string, _id, _duration, activationTurnClassification, eventTriggerTiming, _activationCondition, _iconAsBytes, animationInfo);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private DamageStatusEffectData StringToDamageStatusEffectData(string _string, int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, eEventTriggerTiming _eventTriggerTiming, ComplexCondition _activationCondition, byte[] _iconAsBytes, SimpleAnimationInfo _animationInfo)
        {
            if (_string == null)
                return null;

            try
            {
                int valueTagId = _string.GetTagPortionValue<int>("ValueTagid");
                Tag value = m_tagDictionary[valueTagId];

                return new DamageStatusEffectData(_id, _duration, _activationTurnClassification, _eventTriggerTiming, _activationCondition, _iconAsBytes, _animationInfo, value);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private HealStatusEffectData StringToHealStatusEffectData(string _string, int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, eEventTriggerTiming _eventTriggerTiming, ComplexCondition _activationCondition, byte[] _iconAsBytes, SimpleAnimationInfo _animationInfo)
        {
            if (_string == null)
                return null;

            try
            {
                int valueTagId = _string.GetTagPortionValue<int>("ValueTagId");
                Tag value = m_tagDictionary[valueTagId];

                return new HealStatusEffectData(_id, _duration, _activationTurnClassification, _eventTriggerTiming, _activationCondition, _iconAsBytes, _animationInfo, value);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<Effect> ResponseStringToEffectList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<Effect> result = new List<Effect>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Effects");

                while (sectionString != "")
                {
                    string effectString = string.Empty;

                    if (sectionString.StartsWith("<UnitTargetingEffectsWrapperEffect>"))
                        sectionString = sectionString.DetachTagPortion("UnitTargetingEffectsWrapperEffect", ref effectString);
                    else if (sectionString.StartsWith("<DamageEffect>"))
                        sectionString = sectionString.DetachTagPortion("DamageEffect", ref effectString);
                    else if (sectionString.StartsWith("<DrainEffect>"))
                        sectionString = sectionString.DetachTagPortion("DrainEffect", ref effectString);
                    else if (sectionString.StartsWith("<HealEffect>"))
                        sectionString = sectionString.DetachTagPortion("HealEffect", ref effectString);
                    else if (sectionString.StartsWith("<StatusEffectAttachmentEffect>"))
                        sectionString = sectionString.DetachTagPortion("StatusEffectAttachmentEffect", ref effectString);
                    else if (sectionString.StartsWith("<MovementEffect>"))
                        sectionString = sectionString.DetachTagPortion("MovementEffect", ref effectString);

                    result.Add(StringToEffect(effectString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Effect StringToEffect(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                sectionString = _string.GetTagPortion("ActivationCondition");
                ComplexCondition activationCondition = StringToComplexCondition(sectionString, "ActivationCondition");

                Tag timesToApply = null;
                Tag successRate = null;
                Tag diffusionDistance = null;
                if (!_string.StartsWith("<UnitTargetingEffectsWrapperEffect>"))
                {
                    int timesToApplyTagId = _string.GetTagPortionValue<int>("TimesToApplyTagId");
                    timesToApply = m_tagDictionary[timesToApplyTagId];

                    if (!_string.StartsWith("<MovementEffect>"))
                    {
                        int successRateTagId = _string.GetTagPortionValue<int>("SuccessRateTagId");
                        successRate = m_tagDictionary[successRateTagId];

                        int diffusionDistanceTagId = _string.GetTagPortionValue<int>("DiffusionDistanceTagId");
                        diffusionDistance = m_tagDictionary[diffusionDistanceTagId];
                    }
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("SecondaryEffectIds");
                while (sectionString != "")
                {
                    int secondaryEffectId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("SecondaryEffectId", ref secondaryEffectId);

                    m_effectId_secondaryEffectId.Add(id, secondaryEffectId);
                }

                int animationInfoId = default;
                {
                    if (_string.Contains("<AnimationInfoId>"))
                        animationInfoId = _string.GetTagPortionValue<int>("AnimationInfoId");
                }
                AnimationInfo animationInfo = (animationInfoId != default) ? m_animationInfoDictionary[animationInfoId] : null;

                if (_string.StartsWith("<UnitTargetingEffectsWrapperEffect>")
                    || _string.StartsWith("<DamageEffect>")
                    || _string.StartsWith("<HealEffect>")
                    || _string.StartsWith("<StatusEffectAttachmentEffect>"))
                {
                    return StringToUnitTargetingEffect(_string, id, activationCondition, timesToApply, successRate, diffusionDistance, new List<Effect>(), animationInfo);
                }
                else if (_string.StartsWith("<MovementEffect>"))
                {
                    return StringToTileTargetingEffect(_string, id, activationCondition, timesToApply, successRate, diffusionDistance, new List<Effect>(), animationInfo);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private UnitTargetingEffect StringToUnitTargetingEffect(string _string, int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo)
        {
            if (_string == null)
                return null;

            try
            {
                eTargetUnitClassification targetClassification = _string.GetTagPortionValue<eTargetUnitClassification>("TargetClassification");

                if (_string.StartsWith("<UnitTargetingEffectsWrapperEffect>"))
                    return new UnitTargetingEffectsWrapperEffect(_id, _activationCondition, _secondaryEffects.OfType<UnitTargetingEffect>().ToList(), targetClassification);
                else if (_string.StartsWith("<DamageEffect>"))
                    return StringToDamageEffect(_string, _id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, targetClassification);
                else if (_string.StartsWith("<DrainEffect>"))
                    return StringToDrainEffect(_string, _id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, targetClassification);
                else if (_string.StartsWith("<HealEffect>"))
                    return StringToHealEffect(_string, _id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, targetClassification);
                else if (_string.StartsWith("<StatusEffectAttachmentEffect>"))
                    return StringToStatusEffectAttachmentEffect(_string, _id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, targetClassification);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private DamageEffect StringToDamageEffect(string _string, int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                eAttackClassification attackClassification = _string.GetTagPortionValue<eAttackClassification>("AttackClassification");

                int valueTagId = _string.GetTagPortionValue<int>("ValueTagId");
                Tag value = m_tagDictionary[valueTagId];

                bool isFixedValue = _string.GetTagPortionValue<bool>("IsFixedValue");

                eElement element = _string.GetTagPortionValue<eElement>("Element");

                return new DamageEffect(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification, attackClassification, value, isFixedValue, element);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private DrainEffect StringToDrainEffect(string _string, int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int maxNumOfSecondaryTargetsTagId = _string.GetTagPortionValue<int>("MaxNumberOfSecondaryEffectsTagId");
                Tag maxNumOfSecondaryTargets = m_tagDictionary[maxNumOfSecondaryTargetsTagId];

                eAttackClassification attackClassification = _string.GetTagPortionValue<eAttackClassification>("AttackClassification");

                int valueTagId = _string.GetTagPortionValue<int>("ValueTagId");
                Tag value = m_tagDictionary[valueTagId];

                bool isFixedValue = _string.GetTagPortionValue<bool>("IsFixedValue");

                eElement element = _string.GetTagPortionValue<eElement>("Element");

                int drainingEfficiencyTagId = _string.GetTagPortionValue<int>("DrainingEfficiencyTagId");
                Tag drainingEfficiency = m_tagDictionary[drainingEfficiencyTagId];

                int healAnimationInfoId = _string.GetTagPortionValue<int>("HealAnimationInfoId");
                SimpleAnimationInfo healAnimationInfo = m_animationInfoDictionary[healAnimationInfoId] as SimpleAnimationInfo;

                return new DrainEffect(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification, maxNumOfSecondaryTargets, attackClassification, value, isFixedValue, element, drainingEfficiency, healAnimationInfo);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private HealEffect StringToHealEffect(string _string, int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int valueTagId = _string.GetTagPortionValue<int>("ValueTagId");
                Tag value = m_tagDictionary[valueTagId];

                bool isFixedValue = _string.GetTagPortionValue<bool>("IsFixedValue");

                return new HealEffect(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification, value, isFixedValue);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private StatusEffectAttachmentEffect StringToStatusEffectAttachmentEffect(string _string, int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification)
        {
            if (_string == null)
                return null;

            try
            {
                int statusEffectDataId = _string.GetTagPortionValue<int>("StatusEffectDataId");
                StatusEffectData dataOfStatusEffectToAttach = m_statusEffectDataList.First(x => x.Id == statusEffectDataId);

                return new StatusEffectAttachmentEffect(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification, dataOfStatusEffectToAttach);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private TileTargetingEffect StringToTileTargetingEffect(string _string, int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo)
        {
            if (_string == null)
                return null;

            try
            {
                if (_string.StartsWith("<MovementEffect>"))
                    return StringToMovementEffect(_string, _id, _activationCondition, _timesToApply, _secondaryEffects, _animationInfo as MovementAnimationInfo);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private MovementEffect StringToMovementEffect(string _string, int _id, ComplexCondition _activationCondition, Tag _timesToApply, List<Effect> _secondaryEffects, MovementAnimationInfo _animationInfo)
        {
            if (_string == null)
                return null;

            try
            {
                return new MovementEffect(_id, _activationCondition, _timesToApply, _secondaryEffects, _animationInfo);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ComplexCondition StringToComplexCondition(string _string, string _tagTitle)
        {
            if (_string == null)
                return null;

            try
            {
                List<ConditionSet> conditionSets = new List<ConditionSet>();

                string sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags(_tagTitle);

                while (sectionString != "")
                {
                    int conditionSetId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("ConditionSetId", ref conditionSetId);
                    ConditionSet conditionSet = m_conditionSetDictionary[conditionSetId];

                    conditionSets.Add(conditionSet);
                }

                return new ComplexCondition(conditionSets);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Dictionary<int, Tuple<SkillMaterial, int>> ResponseStringToItemCostDictionary(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                Dictionary<int, Tuple<SkillMaterial, int>> result = new Dictionary<int, Tuple<SkillMaterial, int>>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("ItemCosts");

                while (sectionString != "")
                {
                    string itemCostString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("ItemCost", ref itemCostString);

                    Tuple<int, Tuple<SkillMaterial, int>> id_itemCost = StringToItemCost(itemCostString);
                    result.Add(id_itemCost.Item1, id_itemCost.Item2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private Tuple<int, Tuple<SkillMaterial, int>> StringToItemCost(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                int id = _string.GetTagPortionValue<int>("Id");

                int itemId = _string.GetTagPortionValue<int>("ItemId");
                SkillMaterial item = m_itemList.First(x => x.Id == itemId) as SkillMaterial;

                int quantity = _string.GetTagPortionValue<int>("Quantity");

                return new Tuple<int, Tuple<SkillMaterial, int>>(id, new Tuple<SkillMaterial, int>(item, quantity));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private List<SkillData> ResponseStringToSkillDataList(string _response)
        {
            if (_response == null)
                return null;

            try
            {
                List<SkillData> result = new List<SkillData>();

                string sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Skills");

                while (sectionString != "")
                {
                    string skillString = string.Empty;

                    if (sectionString.StartsWith("<OrdinarySkillData>"))
                        sectionString = sectionString.DetachTagPortion("OrdinarySkillData", ref skillString);
                    else if (sectionString.StartsWith("<CounterSkillData>"))
                        sectionString = sectionString.DetachTagPortion("CounterSkillData", ref skillString);
                    else if (sectionString.StartsWith("<UltimateSkillData>"))
                        sectionString = sectionString.DetachTagPortion("UltimateSkillData", ref skillString);
                    else if (sectionString.StartsWith("<PassiveSkillData>"))
                        sectionString = sectionString.DetachTagPortion("PassiveSkillData", ref skillString);

                    result.Add(StringToSkillData(skillString));
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private SkillData StringToSkillData(string _string)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int id = _string.GetTagPortionValue<int>("Id");

                string name = _string.GetTagPortionValue<string>("Name");

                byte[] iconAsBytes = new byte[0];
                if (id != -1) // Meaning that it is not the basic attack, which does not have an icon
                {
                    int iconId = _string.GetTagPortionValue<int>("IconId");
                    iconAsBytes = m_skillIconDictionary[iconId];
                }

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("TemporalStatusEffectDataIds");
                List<StatusEffectData> temporalStatusEffectsData = new List<StatusEffectData>();
                while (sectionString != "")
                {
                    int temporalStatusEffectDataId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("StatusEffectDataId", ref temporalStatusEffectDataId);

                    temporalStatusEffectsData.Add(m_statusEffectDataList.First(x => x.Id == temporalStatusEffectDataId));
                }

                int skillActivationAnimationId = _string.GetTagPortionValue<int>("SkillActivationAnimationId");

                if (_string.StartsWith("<OrdinarySkillData>")
                    || _string.StartsWith("<CounterSkillData>")
                    || _string.StartsWith("<UltimateSkillData>"))
                {
                    return StringToActiveSkillData(_string, id, name, iconAsBytes, temporalStatusEffectsData.OfType<BackgroundStatusEffectData>().ToList(), skillActivationAnimationId);
                }
                else if (_string.StartsWith("PassiveSkillData"))
                    return StringToPassiveSkillData(_string, id, name, iconAsBytes, temporalStatusEffectsData, skillActivationAnimationId);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private ActiveSkillData StringToActiveSkillData(string _string, int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;


                int maxNumberOfTargetsTagId = _string.GetTagPortionValue<int>("MaxNumberOfTargetsTagId");
                Tag maxNumberOfTargets = m_tagDictionary[maxNumberOfTargetsTagId];

                int effectId = _string.GetTagPortionValue<int>("EffectId");
                Effect effect = m_effectList.First(x => x.Id == effectId);

                if (_string.StartsWith("<OrdinarySkillData>"))
                    return StringToOrdinarySkillData(_string, _id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, maxNumberOfTargets, effect);
                else if (_string.StartsWith("<CounterSkillData>"))
                    return StringToCounterSkillData(_string, _id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, maxNumberOfTargets, effect);
                else if (_string.StartsWith("<UltimateSkillData>"))
                    return StringToUltimateSkillData(_string, _id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, maxNumberOfTargets, effect);

                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private OrdinarySkillData StringToOrdinarySkillData(string _string, int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId, Tag _maxNumberOfTargets, Effect _effect)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int spCost = _string.GetTagPortionValue<int>("SPCost");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("ItemCostIds");
                Dictionary<SkillMaterial, int> itemCosts = new Dictionary<SkillMaterial, int>();
                while (sectionString != "")
                {
                    int itemCostId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("ItemCostId", ref itemCostId);
                    Tuple<SkillMaterial, int> itemCost = m_itemCostDictionary[itemCostId];

                    itemCosts.Add(itemCost.Item1, itemCost.Item2);
                }

                return new OrdinarySkillData(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, _maxNumberOfTargets, _effect, spCost, itemCosts);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private CounterSkillData StringToCounterSkillData(string _string, int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId, Tag _maxNumberOfTargets, Effect _effect)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                int spCost = _string.GetTagPortionValue<int>("SPCost");

                sectionString = _string.GetTagPortionWithoutOpeningAndClosingTags("ItemCostIds");
                Dictionary<SkillMaterial, int> itemCosts = new Dictionary<SkillMaterial, int>();
                while (sectionString != "")
                {
                    int itemCostId = default;
                    sectionString = sectionString.DetachTagPortionAsValue("ItemCostId", ref itemCostId);
                    Tuple<SkillMaterial, int> itemCost = m_itemCostDictionary[itemCostId];

                    itemCosts.Add(itemCost.Item1, itemCost.Item2);
                }

                eEventTriggerTiming eventTriggerTiming = _string.GetTagPortionValue<eEventTriggerTiming>("EventTriggerTiming");

                sectionString = _string.GetTagPortion("ActivationCondition");
                ComplexCondition activationCondition = StringToComplexCondition(sectionString, "ActivationCondition");

                return new CounterSkillData(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, _maxNumberOfTargets, _effect, spCost, itemCosts, eventTriggerTiming, activationCondition);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private UltimateSkillData StringToUltimateSkillData(string _string, int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId, Tag _maxNumberOfTargets, Effect _effect)
        {
            if (_string == null)
                return null;

            try
            {
                return new UltimateSkillData(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, _maxNumberOfTargets, _effect);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private PassiveSkillData StringToPassiveSkillData(string _string, int _id, string _name, byte[] _iconAsBytes, List<StatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId)
        {
            if (_string == null)
                return null;

            try
            {
                string sectionString = string.Empty;

                eTargetUnitClassification targetClassification = _string.GetTagPortionValue<eTargetUnitClassification>("TargetClassification");

                sectionString = _string.GetTagPortion("ActivationCondition");
                ComplexCondition activationCondition = StringToComplexCondition(sectionString, "ActivationCondition");

                return new PassiveSkillData(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, targetClassification, activationCondition);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private void LoadSecondaryEffects()
        {
            try
            {
                foreach (var entry in m_effectId_secondaryEffectId)
                {
                    m_effectList.First(x => x.Id == entry.Key).SecondaryEffects.Add(m_effectList.First(x => x.Id == entry.Value));
                }

                m_effectList.ForEach(x => x.DisableModification());
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private void LoadTransformableWeaponsData()
        {
            try
            {
                foreach (var entry in m_weaponId_transformableWeaponId)
                {
                    m_weaponDataList.First(x => x.Id == entry.Key).TransformableWeapons.Add(m_weaponDataList.First(x => x.Id == entry.Value));
                }

                m_weaponDataList.ForEach(x => x.DisableModification());
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private void LoadUnitEvolutionRecipes()
        {
            try
            {
                foreach (var entry in m_unitId_progressiveEvolutionRecipeBase)
                {
                    UnitData unitData = m_unitDataList.First(x => x.Id == entry.Value.AfterEvolutionUnitId);

                    List<EvolutionMaterial> materials = new List<EvolutionMaterial>();
                    for (int i = 0; i < CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE; i++)
                    {
                        materials.Add(m_itemList.OfType<EvolutionMaterial>().First(x => x.Id == entry.Value.MaterialIds[i]));
                    }

                    UnitEvolutionRecipe progressiveEvolutionRecipe = new UnitEvolutionRecipe(unitData, materials, entry.Value.Cost);

                    m_unitDataList.First(x => x.Id == entry.Key).ProgressiveEvolutionRecipes.Add(progressiveEvolutionRecipe);
                }

                foreach (var entry in m_unitId_retrogressiveEvolutionRecipeBase)
                {
                    UnitData unitData = m_unitDataList.First(x => x.Id == entry.Value.AfterEvolutionUnitId);

                    List<EvolutionMaterial> materials = new List<EvolutionMaterial>();
                    for (int i = 0; i < CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE; i++)
                    {
                        materials.Add(m_itemList.OfType<EvolutionMaterial>().First(x => x.Id == entry.Value.MaterialIds[i]));
                    }

                    UnitEvolutionRecipe retrogressiveEvolutionRecipe = new UnitEvolutionRecipe(unitData, materials, entry.Value.Cost);

                    m_unitDataList.First(x => x.Id == entry.Key).RetrogressiveEvolutionRecipe = retrogressiveEvolutionRecipe;
                }

                m_unitDataList.ForEach(x => x.DisableModification());
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private void LoadSkillInheritors(List<Unit> _units)
        {
            try
            {
                foreach (var item in m_playableUnitId_inheritorUnitId_inheritingSkillId)
                {
                    Unit unit = _units.First(x => x.UniqueId == item.Item1);

                    unit.SkillInheritor = _units.FirstOrDefault(x => x.UniqueId == item.Item2);
                    unit.InheritingSkillId = item.Item3;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private class UnitEvolutionRecipeBase
        {
            public UnitEvolutionRecipeBase(int _afterEvolutionUnitId, int _materialId1, int _materialId2, int _materialId3, int _materialId4, int _materialId5, int _cost)
            {
                AfterEvolutionUnitId = _afterEvolutionUnitId;

                MaterialIds = new int[5];
                MaterialIds[0] = _materialId1;
                MaterialIds[1] = _materialId2;
                MaterialIds[2] = _materialId3;
                MaterialIds[3] = _materialId4;
                MaterialIds[4] = _materialId5;

                Cost = _cost;
            }

            public int AfterEvolutionUnitId { get; }
            public int[] MaterialIds { get; }
            public int Cost { get; }
        }
    }
}
