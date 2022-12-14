using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public class Gacha
    {
        public Gacha(int _id, string _title, eGachaClassification _gachaClassification, List<GachaObjectInfo> _gachaObjectInfos, ValuePerRarity _defaultDispensationValues, AlternativeDispensationInfo _alternativeDispensationInfo, List<DispensationOption> _dispensationOptions, byte[] _bannerImageAsBytes, byte[] _gachaSceneBackgroundImageAsBytes, int _levelOfObjects)
        {
            Id = _id;
            Title = _title.CoalesceNullAndReturnCopyOptionally(true);

            GachaClassification = _gachaClassification;

            m_gachaObjectInfos = _gachaObjectInfos.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            DefaultDispensationValues = _defaultDispensationValues;

            AlternativeDispensationInfo = _alternativeDispensationInfo;

            m_dispensationOptions = _dispensationOptions.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            BannerImageAsBytes = _bannerImageAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
            BackgroundImageAsBytes = _gachaSceneBackgroundImageAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            LevelOfObjects = _levelOfObjects;
        }

        #region Properties
        public int Id { get; }
        public string Title { get; }

        public eGachaClassification GachaClassification { get; }
        public IList<GachaObjectInfo> GachaObjectInfos { get { return m_gachaObjectInfos.AsReadOnly(); } }

        public ValuePerRarity DefaultDispensationValues { get; }
        public AlternativeDispensationInfo AlternativeDispensationInfo { get; }

        public IList<DispensationOption> DispensationOptions { get { return m_dispensationOptions.AsReadOnly(); } }

        public byte[] BannerImageAsBytes { get; }
        public byte[] BackgroundImageAsBytes { get; }

        public int LevelOfObjects { get; }
        #endregion

        #region Private Read-only Fields
        private readonly List<GachaObjectInfo> m_gachaObjectInfos;
        private readonly List<DispensationOption> m_dispensationOptions;
        #endregion
    }

    public struct GachaObjectInfo
    {
        public IRarityMeasurable Object;
        public int RelativeOccurenceValue;
    }

    public struct ValuePerRarity
    {
        public int Legendary;
        public int Epic;
        public int Rare;
        public int Uncommon;
        public int Common;

        public int TotalValue { get { return Common + Uncommon + Rare + Epic + Legendary; } }
        public eRarity OccurrenceValueToRarity(int _value)
        {
            if (_value < 1 || _value > TotalValue)
                return default;

            if (_value > TotalValue - Legendary)
                return eRarity.Legendary;
            else if (_value > TotalValue - Legendary - Epic)
                return eRarity.Epic;
            else if (_value > Common + Uncommon)
                return eRarity.Rare;
            else if (_value > Common)
                return eRarity.Uncommon;
            else
                return eRarity.Common;
        }
    }

    public struct AlternativeDispensationInfo
    {
        public int ApplyAtXthDispensation;
        public ValuePerRarity RatioPerRarity;
    }

    public class DispensationOption
    {
        public DispensationOption(int _id, eCostType _costType, int _costItemId, int _costValue, int _timesToDispense, bool _isNumberOfAttemptsPerDay, int _remainingAttempts = default)
        {
            Id = _id;
            CostType = _costType;
            CostItemId = _costItemId;
            CostValue = _costValue;
            TimesToDispense = _timesToDispense;
            IsNumberOfAttemptsPerDay = _isNumberOfAttemptsPerDay;
            RemainingAttempts = _remainingAttempts;
        }

        #region Properties
        public int Id { get; }
        public eCostType CostType { get; }
        public int CostItemId { get; } // Used in case CostType == eCostType.Item
        public int CostValue { get; }
        public int TimesToDispense { get; }
        public bool IsNumberOfAttemptsPerDay { get; } // Used to specify whether the number of remaining attempts will reset everyday (in case RemainingAttempts is not infinite)

        public int RemainingAttempts { get; set; } // If -1, then it is infinite
        #endregion
    }

    public enum eGachaClassification
    {
        Unit,
        Weapon,
        Armour,
        Accessory,
        SkillItem,
        SkillMaterial,
        ItemMaterial,
        EquipmentMaterial,
        EvolutionMaterial,
        WeaponEnhancementMaterial,
        UnitEnhancementMaterial,
        SkillEnhancementMaterial
    }
}
