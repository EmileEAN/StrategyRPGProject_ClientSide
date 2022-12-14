using EEANWorks.Games.TBSG._01.Unity.Connection;
using EEANWorks.Games.TBSG._01.Unity.SceneSpecific;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EEANWorks.Games.TBSG._01.Data
{
    public sealed class GameDataContainer
    {
        private static GameDataContainer m_instance;

        public static GameDataContainer Instance { get { return m_instance ?? (m_instance = new GameDataContainer()); } }

        private GameDataContainer()
        {
            MTRandom.RandInit();
            m_pass = MTRandom.GetRandString(20, true, true, true);
            GameDataLoader.PassForGameDataContainerTransaction = m_pass;
        }

        #region Properties
        public string SessionId { get; private set; }

        public Player Player { get; private set; }
        public Player CPU { get; set; } //Used in single player mode

        public IList<UnitData> UnitEncyclopedia { get { return m_unitEncyclopedia.AsReadOnly(); } }
        public IList<WeaponData> WeaponEncyclopedia { get { return m_weaponEncyclopedia.AsReadOnly(); } }
        public IList<ArmourData> ArmourEncyclopedia { get { return m_armourEncyclopedia.AsReadOnly(); } }
        public IList<AccessoryData> AccessoryEncyclopedia { get { return m_accessoryEncyclopedia.AsReadOnly(); } }
        public IList<Item> ItemEncyclopedia { get { return m_itemEncyclopedia.AsReadOnly(); } }

        public IList<SkillData> Skills { get { return m_skills.AsReadOnly(); } }
        public IList<Effect> Effects { get { return m_effects.AsReadOnly(); } }
        public IList<StatusEffectData> StatusEffects { get { return m_statusEffects.AsReadOnly(); } }

        public ReadOnlyDictionary<int, AnimationInfo> AnimationInfos { get { return m_animationInfos.AsReadOnly(); } }

        public IList<WeaponRecipe> WeaponRecipes { get { return m_weaponRecipes.AsReadOnly(); } }
        public IList<ArmourRecipe> ArmourRecipes { get { return m_armourRecipes.AsReadOnly(); } }
        public IList<AccessoryRecipe> AccessoryRecipes { get { return m_accessoryRecipes.AsReadOnly(); } }
        public IList<ItemRecipe> ItemRecipes { get { return m_itemRecipes.AsReadOnly(); } }

        public ReadOnlyDictionary<int, TileSet> TileSets { get { return m_tileSets.AsReadOnly(); } }

        public IList<MainStory> MainStories { get { return m_mainStories.AsReadOnly(); } }
        public IList<EventStory> EventStories { get { return m_eventStories.AsReadOnly(); } }

        public IList<Gacha> Gachas { get { return m_gachas.AsReadOnly(); } }

        public Gacha GachaRolled
        {
            get
            {
                Gacha copy = m_gachaRolled;
                m_gachaRolled = null;
                return copy;
            }

            set { m_gachaRolled = value; }
        }
        public DispensationOption DispensationOptionSelected
        {
            get
            {
                DispensationOption copy = m_dispensationOptionSelected;
                m_dispensationOptionSelected = default;
                return copy;
            }

            set { m_dispensationOptionSelected = value; }
        }
        public IList<object> GachaResultObjects
        {
            get
            {
                if (m_gachaResultObjects != null)
                {
                    List<object> copy = new List<object>(m_gachaResultObjects);
                    m_gachaResultObjects = null;
                    return copy.AsReadOnly();
                }

                return null;
            }

            set { m_gachaResultObjects = new List<object>(value); }
        }

        public eUnitSelectionMode UnitSelectionMode { get; set; }
        public Unit SelectedUnit { get; set; }
        public Unit SelectedUnit2 { get; set; }
        public List<Unit> SelectedUnits { get; set; }
        public eSkillSelectionMode SkillSelectionMode { get; set; }
        public Skill SelectedSkill { get; set; }
        public eEquipmentSelectionMode EquipmentSelectionMode { get; set; }
        public Weapon SelectedWeapon { get; set; }
        public Armour SelectedArmour { get; set; }
        public Accessory SelectedAccessory { get; set; }
        public eItemSelectionMode ItemSelectionMode { get; set; }
        public Item SelectedItem { get; set; }
        public Dictionary<Item, int> SelectedItems { get; set; }
        public Team SelectedTeam { get; set; }
        public bool ShowMainStories { get; set; }
        public StoryEpisode EpisodeToPlay { get; set; }
        public eEpisodePhase EpisodePhase { get; set; }
        public Dungeon DungeonToPlay { get; set; }
        public List<List<FloorInstanceMemberInfo>> FloorInstanceInfos { get; set; }

        public OrdinarySkill BasicAttackSkill { get; private set; }
        #endregion

        #region Private Fields
        private string m_pass; //Used to limit access to the functions below

        private List<UnitData> m_unitEncyclopedia;
        private List<WeaponData> m_weaponEncyclopedia;
        private List<ArmourData> m_armourEncyclopedia;
        private List<AccessoryData> m_accessoryEncyclopedia;
        private List<Item> m_itemEncyclopedia;

        private List<SkillData> m_skills;
        private List<Effect> m_effects;
        private List<StatusEffectData> m_statusEffects;

        private Dictionary<int, AnimationInfo> m_animationInfos;

        private List<WeaponRecipe> m_weaponRecipes;
        private List<ArmourRecipe> m_armourRecipes;
        private List<AccessoryRecipe> m_accessoryRecipes;
        private List<ItemRecipe> m_itemRecipes;

        private Dictionary<int, TileSet> m_tileSets;

        //private Dictionary<string, byte[]> m_novelCharacterSpriteSheets;

        private List<MainStory> m_mainStories;
        private List<EventStory> m_eventStories;

        private List<Gacha> m_gachas;

        // The variables below will be used for the GachaSelection and GachaResult scenes to communicate with each other.
        // The values will be initialized each time they are read; and, thus, the values can be read only once.
        private Gacha m_gachaRolled;
        private DispensationOption m_dispensationOptionSelected;
        private List<object> m_gachaResultObjects;

        private bool m_isInitialized = false;
        #endregion

        #region Public Functions
        public bool Initialize(string _pass, string _sessionId, Player _player, Dictionary<int, AnimationInfo> _animationInfos, List<StatusEffectData> _statusEffects, List<Effect> _effects, List<SkillData> _skills, List<Item> _items, List<AccessoryData> _accessories, List<ArmourData> _armours, List<WeaponData> _weapons, List<UnitData> _units, List<ItemRecipe> _itemRecipes, List<AccessoryRecipe> _accessoryRecipes, List<ArmourRecipe> _armourRecipes, List<WeaponRecipe> _weaponRecipes, Dictionary<int, TileSet> _tileSets, List<MainStory> _mainStories, List<EventStory> _eventStories, List<Gacha> _gachas)
        {
            if (_pass != m_pass)
                return false;

            if (_player == null
                || _animationInfos == null
                || _statusEffects == null
                || _effects == null
                || _skills == null
                || _items == null
                || _accessories == null
                || _armours == null
                || _weapons == null
                || _units == null
                || _itemRecipes == null
                || _accessoryRecipes == null
                || _armourRecipes == null
                || _weaponRecipes == null
                || _tileSets == null
                || _mainStories == null
                || _eventStories == null
                || _gachas == null)
            {
                return false;
            }

            if (!m_isInitialized)
            {
                SessionId = _sessionId;

                Player = _player;
                CPU = new Player(0, "CPU", null, null, null, null, null, null, null, 0, 0);

                m_gachaRolled = null;
                m_dispensationOptionSelected = default;
                m_gachaResultObjects = null;

                UnitSelectionMode = default;
                SelectedUnit = null;
                SelectedUnit2 = null;
                SelectedUnits = new List<Unit>();
                SkillSelectionMode = default;
                SelectedSkill = null;
                EquipmentSelectionMode = default;
                SelectedWeapon = null;
                SelectedArmour = null;
                SelectedAccessory = null;
                ItemSelectionMode = default;
                SelectedItem = null;
                SelectedItems = new Dictionary<Item, int>();
                
                m_statusEffects = new List<StatusEffectData>(_statusEffects);
                m_effects = new List<Effect>(_effects);
                m_skills = new List<SkillData>(_skills);

                m_animationInfos = new Dictionary<int, AnimationInfo>(_animationInfos);

                m_itemEncyclopedia = new List<Item>(_items);
                m_accessoryEncyclopedia = new List<AccessoryData>(_accessories);
                m_armourEncyclopedia = new List<ArmourData>(_armours);
                m_weaponEncyclopedia = new List<WeaponData>(_weapons);
                m_unitEncyclopedia = new List<UnitData>(_units);

                m_itemRecipes = new List<ItemRecipe>(_itemRecipes);
                m_accessoryRecipes = new List<AccessoryRecipe>(_accessoryRecipes);
                m_armourRecipes = new List<ArmourRecipe>(_armourRecipes);
                m_weaponRecipes = new List<WeaponRecipe>(_weaponRecipes);

                m_tileSets = new Dictionary<int, TileSet>(_tileSets);

                m_mainStories = new List<MainStory>(_mainStories);
                m_eventStories = new List<EventStory>(_eventStories);

                m_gachas = new List<Gacha>(_gachas);

                BasicAttackSkill = new OrdinarySkill(m_skills.First(x => x.Id == -1) as OrdinarySkillData, 0);

                m_isInitialized = true;
            }

            return m_isInitialized;
        }

        public void UpdateGachas(string _pass, List<Gacha> _gachas)
        {
            if (_pass == m_pass)
                m_gachas = _gachas;
        }

        /// <summary>
        /// Should be called only when the application starts up, or when the session has expired and player has returned to Title.
        /// </summary>
        public void ResetInitializationFlag() { m_isInitialized = false; }
        #endregion
    }

    /// <summary>
    /// Wrapper class to store read-only list of tile types within a dictionary
    /// </summary>
    public class TileSet
    {
        public TileSet(List<eTileType> _tileTypes)
        {
            m_tileTypes = _tileTypes.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        #region Properties
        public IList<eTileType> TileTypes { get { return m_tileTypes.AsReadOnly(); } }
        #endregion

        #region Private Fields
        private List<eTileType> m_tileTypes;
        #endregion
    }

    public enum eEpisodePhase
    {
        PreBattleScene,
        Battle,
        PostBattleScene
    }
}