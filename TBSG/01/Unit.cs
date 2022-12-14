using EEANWorks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EEANWorks.Games.TBSG._01
{
    public class UnitData : IRarityMeasurable
    {

        /// <summary>
        /// Ctor
        /// </summary>
        public UnitData(int _id, string _name, byte[] _iconAsBytes, eGender _gender, eRarity _rarity, eTargetRangeClassification _movementRangeClassification, eTargetRangeClassification _nonMovementActionRangeClassification, List<eElement> _elements,
                             List<eWeaponClassification> _equipableWeaponClassifications, List<eArmourClassification> _equipableArmourClassifications, List<eAccessoryClassification> _equipableAccessoryClassifications,
                                int _maxLvHP, int _maxLvPhyStr, int _maxLvPhyRes, int _maxLvMagStr, int _maxLvMagRes, int _maxLvVit, List<SkillData> _skills, List<string> _labels, string _description, List<UnitEvolutionRecipe> _progressiveEvolutionRecipes = null, UnitEvolutionRecipe _retrogressiveEvolutionRecipe = null)
        {
            Id = _id;

            Name = _name.CoalesceNullAndReturnCopyOptionally(true);

            IconAsBytes = _iconAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            // Assign Gender
            Gender = _gender;

            // Assign Rarity
            Rarity = _rarity;

            MovementRangeClassification = _movementRangeClassification;
            NonMovementActionRangeClassification = _nonMovementActionRangeClassification;

            /*-----------------
            Assign Elements
            -----------------*/
            m_elements = new eElement[2];

            List<eElement> elementsWithoutRedundancy = new List<eElement>(); // Store every value of eElement type within _elements, without including any duplicated value.
            if (_elements != null)
            {
                foreach (eElement element in _elements)
                {
                    if (element != eElement.None && !elementsWithoutRedundancy.Contains(element)) // If elementsWithoutRedundancy does not already contain the given value
                        elementsWithoutRedundancy.Add(element); // Add the value to the list
                }
            }

            elementsWithoutRedundancy = elementsWithoutRedundancy.OrderBy(x => (int)x).ToList(); // Sort the list based on the order of values in the eElement enum

            // Assign argument values
            for (int i = 1; i <= m_elements.Length; i++)
            {
                if (elementsWithoutRedundancy.Count < i) // If elementsWithoutRedundancy contains less items than i
                    m_elements[i - 1] = eElement.None; // Set eElement.None
                else
                    m_elements[i - 1] = _elements[i - 1]; // Set the corresponding value in elementsWithoutRedundancy
            }

            m_equipableWeaponClassifications = _equipableWeaponClassifications.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
            m_equipableArmourClassifications = _equipableArmourClassifications.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
            m_equipableAccessoryClassifications = _equipableAccessoryClassifications.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);


            // Assign Status Values at MAX Level
            MaxLevel_HP = _maxLvHP;
            MaxLevel_PhysicalStrength = _maxLvPhyStr;
            MaxLevel_PhysicalResistance = _maxLvPhyRes;
            MaxLevel_MagicalStrength = _maxLvMagStr;
            MaxLevel_MagicalResistance = _maxLvMagRes;
            MaxLevel_Vitality = _maxLvVit;

            // Assign Skills
            m_skills = _skills.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer

            // Assign Labels
            m_labels = _labels.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer

            Description = _description.CoalesceNullAndReturnCopyOptionally(true);

            m_progressiveEvolutionRecipes = _progressiveEvolutionRecipes.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
            m_retrogressiveEvolutionRecipe = _retrogressiveEvolutionRecipe;

            m_areRecipesModifiable = true;
        }

        #region Properties
        //Base Properties
        public int Id { get; }
        public string Name { get; }

        // Icon
        public byte[] IconAsBytes { get; }

        public eGender Gender { get; }

        // Rarity of the UnitBase
        public eRarity Rarity { get; }

        // Types
        public eTargetRangeClassification MovementRangeClassification { get; }
        public eTargetRangeClassification NonMovementActionRangeClassification { get; }
        public IList<eElement> Elements { get { return Array.AsReadOnly(m_elements); } }

        // Equipable
        public IList<eWeaponClassification> EquipableWeaponClassifications { get { return m_equipableWeaponClassifications.AsReadOnly(); } }
        public IList<eArmourClassification> EquipableArmourClassifications { get { return m_equipableArmourClassifications.AsReadOnly(); } }
        public IList<eAccessoryClassification> EquipableAccessoryClassifications { get { return m_equipableAccessoryClassifications.AsReadOnly(); } }

        // Status Values at MAX Level
        public int MaxLevel_HP { get; }
        public int MaxLevel_PhysicalStrength { get; }
        public int MaxLevel_PhysicalResistance { get; }
        public int MaxLevel_MagicalStrength { get; }
        public int MaxLevel_MagicalResistance { get; }
        public int MaxLevel_Vitality { get; }

        // Skills
        public IList<SkillData> Skills { get { return m_skills.AsReadOnly(); } }

        // Labels
        public IList<string> Labels { get { return m_labels.AsReadOnly(); } }

        // Explanation Text
        public string Description { get; }

        public IList<UnitEvolutionRecipe> ProgressiveEvolutionRecipes
        {
            get
            {
                if (m_areRecipesModifiable)
                    return m_progressiveEvolutionRecipes;
                else
                    return m_progressiveEvolutionRecipes.AsReadOnly();
            }
        }
        public UnitEvolutionRecipe RetrogressiveEvolutionRecipe
        {
            get { return m_retrogressiveEvolutionRecipe; }
            set
            {
                if (m_areRecipesModifiable)
                    m_retrogressiveEvolutionRecipe = value;
            }
        }
        #endregion

        #region Private Fields
        private List<UnitEvolutionRecipe> m_progressiveEvolutionRecipes;
        private UnitEvolutionRecipe m_retrogressiveEvolutionRecipe;

        private bool m_areRecipesModifiable;
        #endregion

        #region Private Read-only Fields
        private readonly eElement[] m_elements; // All can be eElement.None

        private readonly List<eWeaponClassification> m_equipableWeaponClassifications;
        private readonly List<eArmourClassification> m_equipableArmourClassifications;
        private readonly List<eAccessoryClassification> m_equipableAccessoryClassifications;

        private readonly List<SkillData> m_skills;

        private readonly List<string> m_labels;
        #endregion

        #region Public Methods
        public void DisableModification() { m_areRecipesModifiable = false; }
        #endregion
    }

    public class Unit : IDeepCopyable<Unit>
    {
        public Unit(int _baseId, int _uniqueId, string _name, byte[] _iconAsBytes, eGender _gender, eRarity _rarity,
            string _nickname, eTargetRangeClassification _movementRangeClassification, eTargetRangeClassification _nonMovementActionRangeClassification, List<eElement> _elements,
            List<eWeaponClassification> _equipableWeaponClassifications, List<eArmourClassification> _equipableArmourClassifications, List<eAccessoryClassification> _equipableAccessoryClassifications,
            int _accumulatedExp, int _maxLvHP, int _maxLvPhyStr, int _maxLvPhyRes, int _maxLvMagStr, int _maxLvMagRes, int _maxLvVit, bool _isLocked, 
            List<SkillData> _skills, Dictionary<int, int> _skillLevels, List<string> _labels, string _description,
            List<UnitEvolutionRecipe> _progressiveEvolutionRecipes, UnitEvolutionRecipe _retrogressiveEvolutionRecipe = null,
            Weapon _mainWeapon = null, Weapon _subWeapon = null, Armour _armour = null, Accessory _accessory = null)
        {
            BaseInfo = new UnitData(_baseId, _name, _iconAsBytes, _gender, _rarity,
                _movementRangeClassification, _nonMovementActionRangeClassification, _elements,
                _equipableWeaponClassifications, _equipableArmourClassifications, _equipableAccessoryClassifications,
                _maxLvHP, _maxLvPhyStr, _maxLvPhyRes, _maxLvMagStr, _maxLvMagRes, _maxLvVit,
                _skills, _labels, _description, _progressiveEvolutionRecipes, _retrogressiveEvolutionRecipe);

            UniqueId = _uniqueId;

            Nickname = _nickname.CoalesceNullAndReturnCopyOptionally(true);

            AccumulatedExperience = _accumulatedExp;

            IsLocked = _isLocked;

            m_skills = new List<Skill>();
            foreach (SkillData skillData in _skills)
            {
                if (!_skillLevels.TryGetValue(skillData.Id, out int level))
                    level = 1;

                if (skillData is OrdinarySkillData)
                    m_skills.Add(new OrdinarySkill(skillData as OrdinarySkillData, level));
                else if (skillData is CounterSkillData)
                    m_skills.Add(new CounterSkill(skillData as CounterSkillData, level));
                else if (skillData is UltimateSkillData)
                    m_skills.Add(new UltimateSkill(skillData as UltimateSkillData, level));
                else // skillData is PassiveSkillData
                    m_skills.Add(new PassiveSkill(skillData as PassiveSkillData, level));
            }

            MainWeapon = _mainWeapon;
            SubWeapon = _subWeapon;
            Armour = _armour;
            Accessory = _accessory;

            SkillInheritor = null;
            InheritingSkillId = 0;
        }
        public Unit(UnitData _unitData, int _uniqueId, string _nickname, int _accumulatedExp, bool _isLocked, Dictionary<int, int> _skillLevels = null, Weapon _mainWeapon = null, Weapon _subWeapon = null, Armour _armour = null, Accessory _accessory = null)
        {
            BaseInfo = _unitData; // Getting a reference to an object stored within GameDataContainer

            UniqueId = _uniqueId;

            Nickname = _nickname.CoalesceNullAndReturnCopyOptionally(true);

            AccumulatedExperience = _accumulatedExp;

            IsLocked = _isLocked;

            m_skills = new List<Skill>();
            foreach (SkillData skillData in _unitData.Skills)
            {
                int level = 1;
                if (_skillLevels != null)
                    _skillLevels.TryGetValue(skillData.Id, out level);

                if (skillData is OrdinarySkillData)
                    m_skills.Add(new OrdinarySkill(skillData as OrdinarySkillData, level));
                else if (skillData is CounterSkillData)
                    m_skills.Add(new CounterSkill(skillData as CounterSkillData, level));
                else if (skillData is UltimateSkillData)
                    m_skills.Add(new UltimateSkill(skillData as UltimateSkillData, level));
                else // skillData is PassiveSkillData
                    m_skills.Add(new PassiveSkill(skillData as PassiveSkillData, level));
            }

            MainWeapon = _mainWeapon;
            SubWeapon = _subWeapon;
            Armour = _armour;
            Accessory = _accessory;

            SkillInheritor = null;
            InheritingSkillId = 0;
        }
        public Unit(UnitData _unitData, int _uniqueId, string _nickname, int _accumulatedExp, bool _isLocked, List<Skill> _skills, Weapon _mainWeapon = null, Weapon _subWeapon = null, Armour _armour = null, Accessory _accessory = null)
        {
            BaseInfo = _unitData; // Getting a reference to an object stored within GameDataContainer

            UniqueId = _uniqueId;

            Nickname = _nickname.CoalesceNullAndReturnCopyOptionally(true);

            AccumulatedExperience = _accumulatedExp;

            IsLocked = _isLocked;

            m_skills = _skills.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            MainWeapon = _mainWeapon;
            SubWeapon = _subWeapon;
            Armour = _armour;
            Accessory = _accessory;

            SkillInheritor = null;
            InheritingSkillId = 0;
        }

        #region Properties
        public UnitData BaseInfo { get; } // All Units with same baseInfo will reference the same instance of UnitData

        public int UniqueId { get; }

        // User Defined Nickname
        public string Nickname { get; set; }
         
        // Basic Status
        public int AccumulatedExperience { get; private set; }

        public bool IsLocked { get; set; }

        public IList<Skill> Skills { get { return m_skills.AsReadOnly(); } }

        public Weapon MainWeapon { get; set; } // Store actual reference to the Weapon
        public Weapon SubWeapon { get; set; } // Store actual reference to the Weapon
        public Armour Armour { get; set; } // Store actual reference to the Armour
        public Accessory Accessory { get; set; } // Store actual reference to the Accessory

        // The actual values for the two properties below will not be set through the constructor
        public Unit SkillInheritor { get; set; } // Store actual reference to the Unit
        public int InheritingSkillId { get; set; }
        #endregion

        #region Private Fields
        private List<Skill> m_skills;
        #endregion

        #region Public Methods
        public void GainExperience(int _exp)
        {
            if (_exp <= 0 || AccumulatedExperience == int.MaxValue)
                return;

            if (AccumulatedExperience + _exp >= int.MaxValue)
                AccumulatedExperience = int.MaxValue;
            else
                AccumulatedExperience += _exp;
        } 

        Unit IDeepCopyable<Unit>.DeepCopy() { return (Unit)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected virtual object DeepCopyInternally()
        {
            Unit copy = (Unit)this.MemberwiseClone();

            copy.Nickname = string.Copy(Nickname);

            return copy;
        }
        #endregion
    }

    public sealed class UnitInstance : Unit, IDeepCopyable<UnitInstance>
    {
        /// <summary>
        /// Ctor 1
        /// PreCondition: _iconAsByteArray, _spriteAsByteArray, _owner, _armour, _accessory, _ownerInstance have been initialized successfully; _specieTypes.Count > 0;
        /// _equipableWeaponClassifications.Count > 0; _equipableArmourClassifications.Count > 0; _equipableAccessoryClassifications.Count > 0; 
        /// _level > 0; _maxLvHP > 0; _maxLvPhyStr > 0; _maxLvPhyRes > 0; _maxLvMagStr > 0; _maxLvMarRes > 0; _maxLvVit > 0; _max_skills.Count > 0;
        /// PostCondition: Will be initialized successfully.
        /// </summary>
        public UnitInstance(int _baseId, int _uniqueId, byte[] _iconAsByteArray,
            string _name, eGender _gender, eRarity _rarity, string _nickname,
            eTargetRangeClassification _movementRangeClassification, eTargetRangeClassification _nonMovementActionRangeClassification, List<eElement> _elements, List<eWeaponClassification> _equipableWeaponClassifications, List<eArmourClassification> _equipableArmourClassifications, List<eAccessoryClassification> _equipableAccessoryClassifications,
            int _accumulatedExp, int _maxLvHP, int _maxLvPhyStr, int _maxLvPhyRes, int _maxLvMagStr, int _maxLvMagRes, int _maxLvVit, List<SkillData> _skills, Dictionary<int, int> _skillLevels, List<string> _labels, string _description, List<UnitEvolutionRecipe> _progressiveEvolutionRecipes, UnitEvolutionRecipe _retrogressiveEvolutionRecipe,
            Weapon _mainWeapon, Weapon _subWeapon, Armour _armour, Accessory _accessory, PlayerOnBoard _ownerInstance) : base(_baseId, _uniqueId, _name, _iconAsByteArray, _gender, _rarity, _nickname, _movementRangeClassification, _nonMovementActionRangeClassification, _elements, _equipableWeaponClassifications, _equipableArmourClassifications, _equipableAccessoryClassifications, _accumulatedExp, _maxLvHP, _maxLvPhyStr, _maxLvPhyRes, _maxLvMagStr, _maxLvMagRes, _maxLvVit, false, _skills, _skillLevels, _labels, _description, _progressiveEvolutionRecipes, _retrogressiveEvolutionRecipe, _mainWeapon, _subWeapon, _armour, _accessory)
        {
            IsAlive = true;
            RemainingHP = Calculator.MaxHP(this);
            StatusEffects = new List<StatusEffect>();

            InheritedSkill = null;

            MainWeapon = _mainWeapon; // Getting a reference
            SubWeapon = _subWeapon; // Getting a reference
            Armour = _armour; // Getting a reference
            Accessory = _accessory; // Getting a reference

            OwnerInstance = _ownerInstance.CoalesceNull();
        }

        /// <summary>
        /// Ctor 2
        /// PreCondition: _unitBase, _armour, _accessory, and _ownerInstance have been initialized successfully; _weapons.Count > 0;
        /// PostCondition: Will be initialized successfully.
        /// </summary>
        public UnitInstance(Unit _baseUnit, PlayerOnBoard _ownerInstance) : base(_baseUnit.BaseInfo, _baseUnit.UniqueId, _baseUnit.Nickname, _baseUnit.AccumulatedExperience, false, _baseUnit.Skills.ToList())
        {
            IsAlive = true;
            RemainingHP = Calculator.MaxHP(this);
            StatusEffects = new List<StatusEffect>();

            InheritedSkill = _baseUnit.SkillInheritor?.Skills.First(x => x.BaseInfo.Id == _baseUnit.InheritingSkillId);

            MainWeapon = _baseUnit.MainWeapon; // Getting a reference
            SubWeapon = _baseUnit.SubWeapon; // Getting a reference
            Armour = _baseUnit.Armour; // Getting a reference
            Accessory = _baseUnit.Accessory; // Getting a reference

            OwnerInstance = _ownerInstance.CoalesceNull();
        }

        #region Properies
        public bool IsAlive { get; set; }

        public int RemainingHP { get; set; }

        public List<StatusEffect> StatusEffects { get; private set; }

        public Skill InheritedSkill { get; } // Store reference to the original instance of Skill. Can be null.

        public new Weapon MainWeapon { get; set; }
        public new Weapon SubWeapon { get; set; }
        public new Armour Armour { get; set; }
        public new Accessory Accessory { get; set; }

        public PlayerOnBoard OwnerInstance { get; } // Get reference to the actual object
        #endregion

        #region Public Methods
        public bool IsMainWeaponSkill(Skill _skill) { return Object.ReferenceEquals(MainWeapon.BaseInfo.MainWeaponSkill, _skill); }

        public bool AreResourcesEnoughForSkillExecution(CostRequiringSkill _skill)
        {
            try
            {
                CostRequiringSkill skill = Skills.OfType<CostRequiringSkill>().First(x => Object.ReferenceEquals(x, _skill));
                if (skill == null 
                    && MainWeapon.BaseInfo.MainWeaponSkill != null 
                    && MainWeapon.BaseInfo.MainWeaponSkill is CostRequiringSkill
                    && Object.ReferenceEquals(MainWeapon.BaseInfo.MainWeaponSkill, _skill))
                {
                    skill = MainWeapon.BaseInfo.MainWeaponSkill as CostRequiringSkill;
                }
                if (skill == null)
                    return false;

                if (skill.BaseInfo.SPCost <= OwnerInstance.RemainingSP && OwnerInstance.HasRequiredItems(skill.BaseInfo.ItemCosts))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool AreResourcesEnoughForSkillExecution(string _skillName)
        {
            try
            {
                CostRequiringSkill skill = Skills.OfType<CostRequiringSkill>().First(x => x.BaseInfo.Name == _skillName);
                if (skill == null
                    && MainWeapon.BaseInfo.MainWeaponSkill != null
                    && MainWeapon.BaseInfo.MainWeaponSkill is CostRequiringSkill
                    && MainWeapon.BaseInfo.MainWeaponSkill.BaseInfo.Name == _skillName)
                {
                    skill = MainWeapon.BaseInfo.MainWeaponSkill as CostRequiringSkill;
                }
                if (skill == null)
                    return false;

                if (skill.BaseInfo.SPCost <= OwnerInstance.RemainingSP && OwnerInstance.HasRequiredItems(skill.BaseInfo.ItemCosts))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        UnitInstance IDeepCopyable<UnitInstance>.DeepCopy() { return (UnitInstance)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally()
        {
            UnitInstance copy = (UnitInstance)base.DeepCopyInternally();

            copy.StatusEffects = StatusEffects.DeepCopy();

            return copy;
        }
        #endregion
    }
}
