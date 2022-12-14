using System.Collections.Generic;
using System.Linq;

namespace EEANWorks.Games.TBSG._01
{
    public abstract class EquipmentData : IRarityMeasurable
    {
        public EquipmentData(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData)
        {
            Id = _id;

            Name = _name.CoalesceNullAndReturnCopyOptionally(true);

            IconAsBytes = _iconAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            Rarity = _rarity;

            m_statusEffectsData = _statusEffectsData.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer
        }

        #region Properties
        public int Id { get; }
        public string Name { get; }

        public byte[] IconAsBytes { get; }

        public eRarity Rarity { get; }

        public IList<StatusEffectData> StatusEffectsData { get { return m_statusEffectsData.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<StatusEffectData> m_statusEffectsData;
        #endregion
    }

    public class WeaponData : EquipmentData, IRarityMeasurable
    {
        public WeaponData(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData,
                eWeaponType _weaponType, List<eWeaponClassification> _weaponClassifications, Skill _mainWeaponSkill,
                    List<WeaponData> _targetWeaponInCaseTypeIsTransformable = null) : base(_id, _name, _iconAsBytes, _rarity, _statusEffectsData)
        {
            WeaponType = _weaponType;

            m_weaponClassifications = _weaponClassifications.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            MainWeaponSkill = _mainWeaponSkill;

            m_transformableWeapons = _targetWeaponInCaseTypeIsTransformable.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer

            m_isTransformableWeaponsListModifiable = true;
        }

        #region Properties
        public eWeaponType WeaponType { get; }

        public IList<eWeaponClassification> WeaponClassifications { get { return m_weaponClassifications.AsReadOnly(); } }

        public Skill MainWeaponSkill { get; } // Can be null.

        //used if WeaponType == eWeaponType.TRANSFORMABLE
        public IList<WeaponData> TransformableWeapons
        {
            get
            {
                if (m_isTransformableWeaponsListModifiable)
                    return m_transformableWeapons;
                else
                    return m_transformableWeapons.AsReadOnly();
            }
        }
        #endregion

        #region Private Fields
        private List<eWeaponClassification> m_weaponClassifications;

        private List<WeaponData> m_transformableWeapons; //Store the reference to the original object

        private bool m_isTransformableWeaponsListModifiable;
        #endregion

        #region Public Methods
        public void DisableModification() { m_isTransformableWeaponsListModifiable = false; }
        #endregion
    }

    public abstract class Weapon
    {
        public Weapon(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData, eWeaponType _weaponType, List<eWeaponClassification> _weaponClassifications, Skill _mainWeaponSkill,
                        int _uniqueId, bool _isLocked)
        {
            BaseInfo = new WeaponData(_id, _name, _iconAsBytes, _rarity, _statusEffectsData, _weaponType, _weaponClassifications, _mainWeaponSkill);

            UniqueId = _uniqueId;

            IsLocked = _isLocked;
        }
        public Weapon(WeaponData _weaponData, int _uniqueId, bool _isLocked)
        {
            BaseInfo = _weaponData; // Getting a reference to an object stored within GameDataContainer

            UniqueId = _uniqueId;

            IsLocked = _isLocked;
        }

        #region Properties
        public WeaponData BaseInfo { get; } 

        public int UniqueId { get; }

        public bool IsLocked { get; set; }
        #endregion
    }

    public class OrdinaryWeapon : Weapon
    {
        public OrdinaryWeapon(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData, List<eWeaponClassification> _weaponClassifications, Skill _mainWeaponSkill, int _uniqueId, bool _isLocked) : base(_id, _name, _iconAsBytes, _rarity, _statusEffectsData, eWeaponType.Ordinary, _weaponClassifications, _mainWeaponSkill, _uniqueId, _isLocked)
        {
        }
        public OrdinaryWeapon(WeaponData _weaponData, int _uniqueId, bool _isLocked) : base(_weaponData, _uniqueId, _isLocked)
        {
        }
    }

    public class LevelableWeapon : Weapon
    {
        public LevelableWeapon(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData, List<eWeaponClassification> _weaponClassifications, Skill _mainWeaponSkill, int _uniqueId, bool _isLocked,
            int _accumulatedExperience) : base(_id, _name, _iconAsBytes, _rarity, _statusEffectsData, eWeaponType.Levelable, _weaponClassifications, _mainWeaponSkill, _uniqueId, _isLocked)
        {
            if (_accumulatedExperience > 0)
                AccumulatedExperience = _accumulatedExperience;
            else
                AccumulatedExperience = 0;
        }
        public LevelableWeapon(WeaponData _weaponData, int _uniqueId, bool _isLocked, int _accumulatedExperience) : base(_weaponData, _uniqueId, _isLocked)
        {
            if (_accumulatedExperience > 0)
                AccumulatedExperience = _accumulatedExperience;
            else
                AccumulatedExperience = 0;
        }

        #region Properties
        public int AccumulatedExperience { get; private set; }
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
        #endregion
    }

    public class TransformableWeapon : Weapon
    {
        public TransformableWeapon(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData, List<eWeaponClassification> _weaponClassifications, Skill _mainWeaponSkill, int _uniqueId, bool _isLocked,
            List<TransformableWeapon> _targetWeapons) : base(_id, _name, _iconAsBytes, _rarity, _statusEffectsData, eWeaponType.Transformable, _weaponClassifications, _mainWeaponSkill, _uniqueId, _isLocked)
        {
            m_transformableWeapons = _targetWeapons.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }
        public TransformableWeapon(WeaponData _weaponData, int _uniqueId, bool _isLocked) : base(_weaponData, _uniqueId, _isLocked)
        {
            List<TransformableWeapon> weaponPool = new List<TransformableWeapon> { this };

            m_transformableWeapons = new List<TransformableWeapon>();
            foreach (WeaponData weaponData in _weaponData.TransformableWeapons)
            {
                m_transformableWeapons.Add(new TransformableWeapon(weaponData, UniqueId, _isLocked, weaponPool));
            }
        }
        private TransformableWeapon(WeaponData _weaponData, int _uniqueId, bool _isLocked,
            List<TransformableWeapon> _weaponsGenerated) : base(_weaponData, _uniqueId, _isLocked)
        {
            m_transformableWeapons = new List<TransformableWeapon>();
            foreach (WeaponData weaponData in _weaponData.TransformableWeapons)
            {
                if (!_weaponsGenerated.Any(x => x.BaseInfo.Id == weaponData.Id))
                    m_transformableWeapons.Add(new TransformableWeapon(weaponData, UniqueId, _isLocked, _weaponsGenerated));
                else
                    m_transformableWeapons.Add(_weaponsGenerated.First(x => x.BaseInfo.Id == weaponData.Id));
            }
        }

        #region Properties
        public IList<TransformableWeapon> TransformableWeapons { get { return m_transformableWeapons.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<TransformableWeapon> m_transformableWeapons;
        #endregion
    }

    public class LevelableTransformableWeapon : Weapon
    {
        public LevelableTransformableWeapon(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData, List<eWeaponClassification> _weaponClassifications, Skill _mainWeaponSkill, int _uniqueId, bool _isLocked,
            int _accumulatedExperience, List<LevelableTransformableWeapon> _targetWeapons) : base(_id, _name, _iconAsBytes, _rarity, _statusEffectsData, eWeaponType.Levelable, _weaponClassifications, _mainWeaponSkill, _uniqueId, _isLocked)
        {
            if (_accumulatedExperience > 0)
                AccumulatedExperience = _accumulatedExperience;
            else
                AccumulatedExperience = 0;

            m_transformableWeapons = _targetWeapons.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        public LevelableTransformableWeapon(WeaponData _weaponData, int _uniqueId, bool _isLocked,
            int _accumulatedExperience) : base(_weaponData, _uniqueId, _isLocked)
        {
            if (_accumulatedExperience > 0)
                AccumulatedExperience = _accumulatedExperience;
            else
                AccumulatedExperience = 0;

            List<LevelableTransformableWeapon> weaponPool = new List<LevelableTransformableWeapon>() { this };

            m_transformableWeapons = new List<LevelableTransformableWeapon>();
            foreach (WeaponData weaponData in _weaponData.TransformableWeapons)
            {
                m_transformableWeapons.Add(new LevelableTransformableWeapon(weaponData, UniqueId, _isLocked, _accumulatedExperience, weaponPool));
            }
        }

        private LevelableTransformableWeapon(WeaponData _weaponData, int _uniqueId, bool _isLocked,
            int _accumulatedExperience, List<LevelableTransformableWeapon> _weaponsGenerated) : base(_weaponData, _uniqueId, _isLocked)
        {
            if (_accumulatedExperience > 0)
                AccumulatedExperience = _accumulatedExperience;
            else
                AccumulatedExperience = 0;

            m_transformableWeapons = new List<LevelableTransformableWeapon>();
            foreach (WeaponData weaponData in _weaponData.TransformableWeapons)
            {
                if (!_weaponsGenerated.Any(x => x.BaseInfo.Id == weaponData.Id))
                    m_transformableWeapons.Add(new LevelableTransformableWeapon(weaponData, UniqueId, _isLocked, _accumulatedExperience, _weaponsGenerated));
                else
                    m_transformableWeapons.Add(_weaponsGenerated.First(x => x.BaseInfo.Id == weaponData.Id));
            }
        }

        #region Properties
        public int AccumulatedExperience { get; private set; }

        public IList<LevelableTransformableWeapon> TransformableWeapons { get { return m_transformableWeapons.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<LevelableTransformableWeapon> m_transformableWeapons;
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
        #endregion
    }

    public class ArmourData : EquipmentData, IRarityMeasurable
    {
        public ArmourData(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData,
            eArmourClassification _armourClassification, eGender _targetGender) : base(_id, _name, _iconAsBytes, _rarity, _statusEffectsData)
        {
            ArmourClassification = _armourClassification;
            TargetGender = _targetGender;
        }

        #region Properties
        public eArmourClassification ArmourClassification { get; }

        public eGender TargetGender { get; }
        #endregion
    }

    public class Armour
    {
        public Armour(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData, eArmourClassification _armourClassification, eGender _targetGender,
                        int _uniqueId, bool _isLocked)
        {
            BaseInfo = new ArmourData(_id, _name, _iconAsBytes, _rarity, _statusEffectsData, _armourClassification, _targetGender);

            UniqueId = _uniqueId;

            IsLocked = _isLocked;
        }
        public Armour(ArmourData _armourData, int _uniqueId, bool _isLocked)
        {
            BaseInfo = _armourData; // Getting a reference to an object stored within GameDataContainer

            UniqueId = _uniqueId;

            IsLocked = _isLocked;
        }

        #region Properties
        public ArmourData BaseInfo { get; } 

        public int UniqueId { get; }

        public bool IsLocked { get; set; }
        #endregion
    }

    public class AccessoryData : EquipmentData, IRarityMeasurable
    {
        public AccessoryData(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData,
            eAccessoryClassification _accessoryClassification, eGender _targetGender) : base(_id, _name, _iconAsBytes, _rarity, _statusEffectsData)
        {
            AccessoryClassification = _accessoryClassification;
            TargetGender = _targetGender;
        }

        #region Properties
        public eAccessoryClassification AccessoryClassification { get; }

        public eGender TargetGender { get; }
        #endregion
    }

    public class Accessory
    {
        public Accessory(int _id, string _name, byte[] _iconAsBytes, eRarity _rarity, List<StatusEffectData> _statusEffectsData, eAccessoryClassification _accessoryClassification, eGender _targetGender,
                         int _uniqueId, bool _isLocked)
        {
            BaseInfo = new AccessoryData(_id, _name, _iconAsBytes, _rarity, _statusEffectsData, _accessoryClassification, _targetGender);

            UniqueId = _uniqueId;

            IsLocked = _isLocked;
        }
        public Accessory(AccessoryData _accessoryData, int _uniqueId, bool _isLocked)
        {
            BaseInfo = _accessoryData; // Getting a reference to an object stored within GameDataContainer

            UniqueId = _uniqueId;

            IsLocked = _isLocked;
        }

        #region Properties
        public AccessoryData BaseInfo { get; } 

        public int UniqueId { get; }

        public bool IsLocked { get; set; }
        #endregion
    }
}
