using EEANWorks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EEANWorks.Games.TBSG._01
{
    #region Data
    public abstract class SkillData
    {
        public SkillData(int _id, string _name, byte[] _iconAsBytes, List<StatusEffectData> _statusEffectsData, int _skillActivationAnimationId)
        {
            Id = _id;

            Name = _name.CoalesceNullAndReturnCopyOptionally(true);

            IconAsBytes = _iconAsBytes; // Getting a reference to a byte[] within GameDataContainer

            m_statusEffectsData = _statusEffectsData.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer

            SkillActivationAnimationId = _skillActivationAnimationId;
        }

        #region Properties
        public int Id { get; }
        public string Name { get; }

        public byte[] IconAsBytes { get; }

        public virtual IList<StatusEffectData> StatusEffectsData { get { return m_statusEffectsData.AsReadOnly(); } }

        public int SkillActivationAnimationId { get; }
        #endregion

        #region Private Read-only Fields
        private readonly List<StatusEffectData> m_statusEffectsData;
        #endregion

        #region Public Methods
        public Skill ToSkill(int _level)
        {
            if (this == null)
                return null;
            else
            {
                if (this is OrdinarySkillData) { return new OrdinarySkill(this as OrdinarySkillData, _level); }
                else if (this is CounterSkillData) { return new CounterSkill(this as CounterSkillData, _level); }
                else if (this is UltimateSkillData) { return new UltimateSkill(this as UltimateSkillData, _level); }
                else /*(this is PassiveSkillData)*/ { return new PassiveSkill(this as PassiveSkillData, _level); }
            }
        }

        public virtual string ToFormattedString(int _level)
        {
            string result = "<b>[Status Effects]</b>"
                            + "\n";
            foreach (StatusEffectData statusEffectData in m_statusEffectsData)
            {
                result += statusEffectData.ToFormattedString(_level)
                        + "\n";
            }

            return result;
        }
        #endregion
    }

    public abstract class ActiveSkillData : SkillData
    {
        public ActiveSkillData(int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId,
            Tag _maxNumberOfTargets, Effect _effect) : base(_id, _name, _iconAsBytes, _temporalStatusEffectsData.CoalesceNullAndReturnCopyOptionally(eCopyType.None).Cast<StatusEffectData>().ToList(), _skillActivationAnimationId)
        {
            MaxNumberOfTargets = _maxNumberOfTargets; // Getting a reference to an object stored within GameDataContainer

            Effect = _effect; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public Tag MaxNumberOfTargets { get; } // Tag value must be positive integer

        public Effect Effect { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0)
        {
            return "<b>[Max Number of Targets]</b>"
                + "\n"
                + MaxNumberOfTargets.ToFormattedString(_level)
                + "\n\n"
                + base.ToFormattedString(_level)
                + "\n\n"
                + Effect.ToFormattedString(_level);
        }
        #endregion
    }

    public abstract class CostRequiringSkillData : ActiveSkillData
    {
        public CostRequiringSkillData(int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId, Tag _maxNumberOfTargets, Effect _effect,
            int _spCost, Dictionary<SkillMaterial, int> _itemCosts) : base(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, _maxNumberOfTargets, _effect)
        {
            if (_spCost < CoreValues.MIN_SP_COST)
                _spCost = CoreValues.MIN_SP_COST;
            else if (_spCost > CoreValues.MAX_SP)
                _spCost = CoreValues.MAX_SP;

            SPCost = _spCost;

            m_itemCosts = _itemCosts.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep); // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public int SPCost { get; }
        public ReadOnlyDictionary<SkillMaterial, int> ItemCosts { get { return m_itemCosts.AsReadOnly(); } } // Key => Item Id, Value => Quantity
        #endregion

        #region Private Read-only Fields
        private readonly Dictionary<SkillMaterial, int> m_itemCosts;
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level); }
        #endregion
    }

    public class OrdinarySkillData : CostRequiringSkillData
    {
        public OrdinarySkillData(int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId, Tag _maxNumberOfTargets, Effect _effect, int _spCost, Dictionary<SkillMaterial, int> _itemCosts) : base(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, _maxNumberOfTargets, _effect, _spCost, _itemCosts)
        {
        }

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level); }
        #endregion
    }

    public class CounterSkillData : CostRequiringSkillData
    {
        public CounterSkillData(int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId, Tag _maxNumberOfTargets, Effect _effect, int _spCost, Dictionary<SkillMaterial, int> _itemCosts,
            eEventTriggerTiming _activationTiming, ComplexCondition _activationCondition) : base(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, _maxNumberOfTargets, _effect, _spCost, _itemCosts)
        {
            ActivationTiming = _activationTiming;
            ActivationCondition = _activationCondition.CoalesceNullAndReturnDeepCopyOptionally(true);
        }

        #region Properties
        public eEventTriggerTiming ActivationTiming { get; }
        public ComplexCondition ActivationCondition { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0)
        {
            return "<b>[Activation Timing]</b>"
                    + "\n"
                    + ActivationTiming.ToString()
                    + "\n\n"
                    + "<b>[Activation Condition]</b>"
                    + "\n"
                    + ((ActivationCondition.ConditionSets.Count != 0) ? ("If " + ActivationCondition.ToFormattedString(_level)) : "None")
                    + "\n\n"
                    + base.ToFormattedString(_level);
        }
        #endregion
    }

    public class UltimateSkillData : ActiveSkillData
    {
        public UltimateSkillData(int _id, string _name, byte[] _iconAsBytes, List<BackgroundStatusEffectData> _temporalStatusEffectsData, int _skillActivationAnimationId, Tag _maxNumberOfTargets, Effect _effect) : base(_id, _name, _iconAsBytes, _temporalStatusEffectsData, _skillActivationAnimationId, _maxNumberOfTargets, _effect)
        {
        }

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level); }
        #endregion
    }

    public class PassiveSkillData : SkillData
    {
        public PassiveSkillData(int _id, string _name, byte[] _iconAsBytes, List<StatusEffectData> _statusEffectsData, int _skillActivationAnimationId,
            eTargetUnitClassification _targetClassification, ComplexCondition _activationCondition) : base(_id, _name, _iconAsBytes, _statusEffectsData, _skillActivationAnimationId)
        {
            TargetClassification = _targetClassification;
            ActivationCondition = _activationCondition.CoalesceNullAndReturnDeepCopyOptionally(true);
        }

        #region Properties
        public eTargetUnitClassification TargetClassification { get; }
        public ComplexCondition ActivationCondition { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0)
        {
            return "<b>[Target Unit Classification]</b>" 
                    + "\n"
                    + TargetClassification.ToString()
                    + "\n\n"
                    + "<b>[Activation Condition]</b>"
                    + "\n"
                    + ((ActivationCondition.ConditionSets.Count != 0) ? ("If " + ActivationCondition.ToFormattedString(_level)) : "None")
                    + "\n\n"
                    + base.ToFormattedString(_level);
        }
        #endregion
    }
    #endregion

    #region Actual Skills
    public abstract class Skill : IDeepCopyable<Skill>
    {
        public Skill(SkillData _skillData, int _level)
        {
            BaseInfo = _skillData; // Getting a reference to an object stored within GameDataContainer

            Level = _level;
        }

        #region Properties
        public SkillData BaseInfo { get; } 

        public int Level { get; set; } // To be updated externally
        #endregion

        #region Public Methods
        Skill IDeepCopyable<Skill>.DeepCopy() { return (Skill)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected virtual object DeepCopyInternally() { return (Skill)this.MemberwiseClone(); }
        #endregion
    }

    public abstract class ActiveSkill : Skill, IDeepCopyable<ActiveSkill>
    {
        public ActiveSkill(ActiveSkillData _skillData, int _level) : base(_skillData, _level)
        {
        }

        #region Properties
        public new ActiveSkillData BaseInfo => base.BaseInfo as ActiveSkillData;
        #endregion

        #region Public Methods
        ActiveSkill IDeepCopyable<ActiveSkill>.DeepCopy() { return (ActiveSkill)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (ActiveSkill)base.DeepCopyInternally(); }
        #endregion
    }

    public abstract class CostRequiringSkill : ActiveSkill, IDeepCopyable<CostRequiringSkill>
    {
        public CostRequiringSkill(CostRequiringSkillData _skillData, int _level) : base(_skillData, _level)
        {
        }

        #region Properties
        public new CostRequiringSkillData BaseInfo => base.BaseInfo as CostRequiringSkillData;
        #endregion

        #region Public Methods
        CostRequiringSkill IDeepCopyable<CostRequiringSkill>.DeepCopy() { return (CostRequiringSkill)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (CostRequiringSkill)base.DeepCopyInternally(); }
        #endregion
    }

    public class OrdinarySkill : CostRequiringSkill, IDeepCopyable<OrdinarySkill>
    {
        public OrdinarySkill(OrdinarySkillData _skillData, int _level) : base(_skillData, _level)
        {
        }

        #region Properties
        public new OrdinarySkillData BaseInfo => base.BaseInfo as OrdinarySkillData;
        #endregion

        #region Public Methods
        OrdinarySkill IDeepCopyable<OrdinarySkill>.DeepCopy() { return (OrdinarySkill)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (OrdinarySkill)base.DeepCopyInternally(); }
        #endregion
    }

    public class CounterSkill : CostRequiringSkill, IDeepCopyable<CounterSkill>
    {
        public CounterSkill(CounterSkillData _skillData, int _level) : base(_skillData, _level)
        {
        }

        #region Properties
        public new CounterSkillData BaseInfo => base.BaseInfo as CounterSkillData;
        #endregion

        #region Public Methods
        CounterSkill IDeepCopyable<CounterSkill>.DeepCopy() { return (CounterSkill)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (CounterSkill)base.DeepCopyInternally(); }
        #endregion
    }

    public class UltimateSkill : ActiveSkill, IDeepCopyable<UltimateSkill>
    {
        public UltimateSkill(UltimateSkillData _skillData, int _level) : base(_skillData, _level)
        {
        }

        #region Properties
        public new UltimateSkillData BaseInfo => base.BaseInfo as UltimateSkillData;
        #endregion

        #region Public Methods
        UltimateSkill IDeepCopyable<UltimateSkill>.DeepCopy() { return (UltimateSkill)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (UltimateSkill)base.DeepCopyInternally(); }
        #endregion
    }

    public class PassiveSkill : Skill, IDeepCopyable<PassiveSkill>
    {
        public PassiveSkill(PassiveSkillData _skillData, int _level) : base(_skillData, _level)
        {
        }

        #region Properties
        public new PassiveSkillData BaseInfo => base.BaseInfo as PassiveSkillData;
        #endregion

        #region Public Methods
        PassiveSkill IDeepCopyable<PassiveSkill>.DeepCopy() { return (PassiveSkill)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (PassiveSkill)base.DeepCopyInternally(); }
        #endregion
    }
    #endregion
}
