using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public abstract class Item : IRarityMeasurable
    {
        public Item(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice)
        {
            Id = _id;

            Name = _name.CoalesceNullAndReturnCopyOptionally(true);

            IconAsBytes = _iconAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            Rarity = _rarity;

            SellingPrice = _sellingPrice;
        }

        #region Properties
        public int Id { get; }
        public string Name { get; }
        public byte[] IconAsBytes { get; }

        public eRarity Rarity { get; }

        public int SellingPrice { get; }
        #endregion
    }

    // Used during battle
    public class BattleItem : Item, IRarityMeasurable
    {
        public BattleItem(int _id, string _name, byte[] _iconAsByteArray, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsByteArray, _rarity, _sellingPrice)
        {
        }
    }

    // Can be used to execute a skill
    public class SkillItem : BattleItem, IRarityMeasurable
    {
        public SkillItem(int _id, string _name, byte[] _iconAsByteArray, eRarity _rarity, int _sellingPrice,
            ActiveSkill _skill) : base(_id, _name, _iconAsByteArray, _rarity, _sellingPrice)
        {
            Skill = _skill.CoalesceNullAndReturnDeepCopyOptionally(true) as ActiveSkill;
        }

        #region Properties
        public ActiveSkill Skill { get; }
        #endregion
    }

    // Required for Skills' item cost
    public class SkillMaterial : BattleItem, IRarityMeasurable
    {
        public SkillMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
        }
    }

    // Required to craft Items
    public class ItemMaterial : Item, IRarityMeasurable
    {
        public ItemMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
        }
    }

    // Required to forge Equipments
    public class EquipmentMaterial : Item, IRarityMeasurable
    {
        public EquipmentMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
        }
    }

    // Required to evolve Units
    public class EvolutionMaterial : Item, IRarityMeasurable
    {
        public EvolutionMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
        }
    }

    // Required to enhance some objects
    public class EnhancementMaterial : Item, IRarityMeasurable
    {
        public EnhancementMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice,
            int _enhancementValue) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
            EnhancementValue = _enhancementValue;
        }

        #region Properties
        public int EnhancementValue { get; }
        #endregion
    }

    // Used to increase the level of Levelable Weapons
    public class WeaponEnhancementMaterial : EnhancementMaterial, IRarityMeasurable
    {
        public WeaponEnhancementMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice, int _expToApply,
            List<eWeaponClassification> _targetingWeaponClassifications) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice, _expToApply)
        {
            m_targetingWeaponClassifications = _targetingWeaponClassifications.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        #region Properties
        public IList<eWeaponClassification> TargetingWeaponClassifications { get { return m_targetingWeaponClassifications.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<eWeaponClassification> m_targetingWeaponClassifications; // All classifications will be targeted if this contains no item
        #endregion
    }

    // Used to increase the level of Units
    public class UnitEnhancementMaterial : EnhancementMaterial, IRarityMeasurable
    {
        public UnitEnhancementMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice, int _expToApply,
            List<eElement> _bonusElements) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice, _expToApply)
        {
            m_bonusElements = _bonusElements.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        #region Properties
        public IList<eElement> BonusElements { get { return m_bonusElements.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<eElement> m_bonusElements;
        #endregion
    }

    // Used to increase the level of Skills
    public class SkillEnhancementMaterial : EnhancementMaterial, IRarityMeasurable
    {
        public SkillEnhancementMaterial(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice, int _levelsToEnhance,
            List<eRarity> _targetingRarities, List<eElement> _targetingElements, List<string> _targetingLabels) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice, _levelsToEnhance)
        {
            m_targetingRarities = _targetingRarities.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
            m_targetingElements = _targetingElements.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
            m_targetingLabels = _targetingLabels.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
        }

        #region Properties
        public IList<eRarity> TargetingRarities { get { return m_targetingRarities.AsReadOnly(); } }
        public IList<eElement> TargetingElements { get { return m_targetingElements.AsReadOnly(); } }
        public IList<string> TargetingLabels { get { return m_targetingLabels.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<eRarity> m_targetingRarities; // All rarities will be targeted if this contains no item
        private readonly List<eElement> m_targetingElements; // All elements will be targeted if this contains no item
        private readonly List<string> m_targetingLabels; // All labels will be targeted if this contains no item
        #endregion
    }

    // Required for some Gachas, instead of gems or gold
    public class GachaCostItem : Item, IRarityMeasurable
    {
        public GachaCostItem(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
        }
    }

    // Required to obtain some Equipments
    public class EquipmentTradingItem : Item, IRarityMeasurable
    {
        public EquipmentTradingItem(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
        }
    }

    // Required to obtain some Units
    public class UnitTradingItem : Item, IRarityMeasurable
    {
        public UnitTradingItem(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, int _sellingPrice) : base(_id, _name, _iconAsBytes, _rarity, _sellingPrice)
        {
        }
    }
}
