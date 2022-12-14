using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    #region Data
    public abstract class StatusEffectData
    {
        public StatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes)
        {
            Id = _id;

            Duration = _duration; // Getting a reference to an object stored within GameDataContainer

            ActivationTurnClassification = _activationTurnClassification;

            ActivationCondition = _activationCondition.CoalesceNullAndReturnDeepCopyOptionally(true);

            IconAsBytes = _iconAsBytes; // Getting a reference to a byte[] within GameDataContainer
        }

        #region Properties
        public int Id { get; }

        public DurationData Duration { get; }
        public eActivationTurnClassification ActivationTurnClassification { get; }
        public ComplexCondition ActivationCondition { get; }

        public byte[] IconAsBytes { get; }
        #endregion

        #region Public Methods
        public virtual string ToFormattedString(int _level)
        {
            string durationString = Duration.ToFormattedString(_level);
            return "<-" + ActivationTurnClassification.ToString()  + "-> " + durationString + ((ActivationCondition.ConditionSets.Count != 0) ? (((durationString != "") ? "if " : "If ") + ActivationCondition.ToFormattedString(_level) + ", ") : "");
        }
        #endregion
    }

    #region Background
    public abstract class BackgroundStatusEffectData : StatusEffectData
    {
        public BackgroundStatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes) : base(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes)
        {
        }

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level); }
        #endregion
    }

    public class BuffStatusEffectData : BackgroundStatusEffectData
    {
        public BuffStatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes,
            eStatusType _targetStatusType, Tag _value, bool _isSum) : base(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes)
        {
            TargetStatusType = _targetStatusType;

            Value = _value; // Getting a reference to an object stored within GameDataContainer

            IsSum = _isSum;
        }

        #region Properties
        public eStatusType TargetStatusType { get; }
        public Tag Value { get; }
        public bool IsSum { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level) + "<color=green>" + (IsSum ? "+" : "x") + Value.ToFormattedString(_level) + " </color>" + TargetStatusType.ToString(); }
        #endregion
    }

    public class DebuffStatusEffectData : BackgroundStatusEffectData
    {
        public DebuffStatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes,
            eStatusType _targetStatusType, Tag _value, bool _isSum) : base(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes)
        {
            TargetStatusType = _targetStatusType;

            Value = _value; // Getting a reference to an object stored within GameDataContainer

            IsSum = _isSum;
        }

        #region Properties
        public eStatusType TargetStatusType { get; }
        public Tag Value { get; }
        public bool IsSum { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level) + "<color=red>" + (IsSum ? "-" : "÷") + Value.ToFormattedString(_level) + " </color>" + TargetStatusType.ToString(); }
        #endregion
    }

    public class TargetRangeModStatusEffectData : BackgroundStatusEffectData
    {
        public TargetRangeModStatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes,
            bool _isMovementRangeClassification, eTargetRangeClassification _targetRangeClassification, eModificationMethod _modificationMethod) : base(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes)
        {
            IsMovementRangeClassification = _isMovementRangeClassification;
            TargetRangeClassification = _targetRangeClassification;
            ModificationMethod = _modificationMethod;
        }

        #region Properties
        public bool IsMovementRangeClassification { get; }
        public eTargetRangeClassification TargetRangeClassification { get; }
        public eModificationMethod ModificationMethod { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level) + "<color=purple>" + ModificationMethod.ToString() + TargetRangeClassification.ToString() + " on " + (IsMovementRangeClassification ? " Movement" : " Non-movement Action") + "Range</color>"; }
        #endregion
    }
    #endregion

    #region Foreground
    public abstract class ForegroundStatusEffectData : StatusEffectData
    {
        public ForegroundStatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, ComplexCondition _activationCondition, byte[] _iconAsBytes,
            eEventTriggerTiming _eventTriggerTiming, SimpleAnimationInfo _animationInfo) : base(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes)
        {
            EventTriggerTiming = _eventTriggerTiming;
            AnimationInfo = _animationInfo; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public eEventTriggerTiming EventTriggerTiming { get; }
        public SimpleAnimationInfo AnimationInfo { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return "<-" + EventTriggerTiming.ToString() + "-> " + base.ToFormattedString(_level); }
        #endregion
    }

    public class DamageStatusEffectData : ForegroundStatusEffectData
    {
        public DamageStatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, eEventTriggerTiming _eventTriggerTiming, ComplexCondition _activationCondition, byte[] _iconAsBytes, SimpleAnimationInfo _animationInfo,
            Tag _value) : base(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes, _eventTriggerTiming, _animationInfo)
        {
            Value = _value; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public Tag Value { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level) + "<color=red>Deal " + Value.ToFormattedString(_level) + "damage</color>."; }
        #endregion
    }

    public class HealStatusEffectData : ForegroundStatusEffectData
    {
        public HealStatusEffectData(int _id, DurationData _duration, eActivationTurnClassification _activationTurnClassification, eEventTriggerTiming _eventTriggerTiming, ComplexCondition _activationCondition, byte[] _iconAsBytes, SimpleAnimationInfo _animationInfo,
            Tag _value) : base(_id, _duration, _activationTurnClassification, _activationCondition, _iconAsBytes, _eventTriggerTiming, _animationInfo)
        {
            Value = _value; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public Tag Value { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level = 0) { return base.ToFormattedString(_level) + "<color=green>Restore " + Value.ToFormattedString(_level) + "HP</color>."; }
        #endregion
    }
    #endregion

    public sealed class DurationData
    {
        public DurationData(Tag _activationTimes, Tag _turns, ComplexCondition _whileConditions)
        {
            ActivationTimes = _activationTimes; // Getting a reference to an object stored within GameDataContainer
            Turns = _turns; // Getting a reference to an object stored within GameDataContainer
            WhileCondition = _whileConditions.CoalesceNullAndReturnDeepCopyOptionally(true);
        }

        //Can use either one or combine properties
        #region Properties
        public Tag ActivationTimes { get; }
        public Tag Turns { get; } //Decimal number. 0.5 represents a player turn. 1 represents a turn of each player.
        public ComplexCondition WhileCondition { get; }
        #endregion

        #region Public Methods
        public string ToFormattedString(int _level)
        {
            string result = "<color=purple>";

            bool hasActivationTimes = ActivationTimes != Tag.Zero;
            bool hasTurns = Turns != Tag.Zero;
            bool hasWhileCondition = WhileCondition.ConditionSets.Count != 0;
            if (hasActivationTimes || hasTurns || hasWhileCondition)
            {
                if (hasActivationTimes)
                    result += "For " + ActivationTimes.ToFormattedString(_level) + " times";

                if (hasTurns)
                    result += (hasActivationTimes ? "/" : "") + "For " + Turns.ToFormattedString(_level) + " turns";

                if (hasWhileCondition)
                    result += ((hasActivationTimes || hasTurns) ? "/" : "") + "While " + WhileCondition.ToFormattedString(_level);

                result += ", ";
            }

            result += "</color>";

            return result;
        }
        #endregion
    }
    #endregion

    #region Actual Status Effects
    public abstract class StatusEffect : IDeepCopyable<StatusEffect> //Poison, Confusion, Strength down, etc. This will be attached directly to Units
    {
        public StatusEffect(StatusEffectData _data, UnitInstance _effectApplier, int _originSkillLevel, int _equipmentLevel, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect)
        {
            BaseInfo = _data; // Getting a reference to an object stored within GameDataContainer

            ActivationCondition = _data.ActivationCondition.CoalesceNullAndReturnDeepCopyOptionally(true);
            EffectApplier = _effectApplier; // Getting a reference

            OriginSkillLevel = _originSkillLevel;
            EquipmentLevel = _equipmentLevel;

            Duration = new Duration(_data.Duration, _system, this, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
        }

        #region Properties
        public StatusEffectData BaseInfo { get; }
        public Duration Duration { get; private set; }
        public ComplexCondition ActivationCondition { get; private set; }
        public UnitInstance EffectApplier { get; } 
        public int OriginSkillLevel { get; } // Level of the skill that generated or that contains this status effect
        public int EquipmentLevel { get; } // Level of the equipment that contains this status effect
        #endregion

        #region Public Methods
        StatusEffect IDeepCopyable<StatusEffect>.DeepCopy() { return (StatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected virtual object DeepCopyInternally()
        {
            StatusEffect copy = (StatusEffect)this.MemberwiseClone();

            copy.Duration = Duration.DeepCopy();
            copy.ActivationCondition = ActivationCondition.DeepCopy();

            return copy;
        }
        #endregion
    }

    #region Background
    public abstract class BackgroundStatusEffect : StatusEffect, IDeepCopyable<BackgroundStatusEffect>
    {
        public BackgroundStatusEffect(BackgroundStatusEffectData _data, UnitInstance _effectApplier, int _originSkillLevel, int _equipmentLevel, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect) : base(_data, _effectApplier, _originSkillLevel, _equipmentLevel, _system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect)
        {
            ActivationTurnClassification = _data.ActivationTurnClassification;
        }

        #region Properties
        public eActivationTurnClassification ActivationTurnClassification { get; }
        #endregion

        #region Public Methods
        BackgroundStatusEffect IDeepCopyable<BackgroundStatusEffect>.DeepCopy() { return (BackgroundStatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (BackgroundStatusEffect)base.DeepCopyInternally(); }
        #endregion
    }

    public class BuffStatusEffect : BackgroundStatusEffect, IDeepCopyable<BuffStatusEffect>
    {
        public BuffStatusEffect(BuffStatusEffectData _data, UnitInstance _effectApplier = null, int _originSkillLevel = 0, int _equipmentLevel = 0) : base(_data, _effectApplier, _originSkillLevel, _equipmentLevel, null, null, null, null, null, null, null, default)
        {
            TargetStatusType = _data.TargetStatusType;
            Value = _data.Value; // Getting a reference to an object stored within GameDataContainer
            IsSum = _data.IsSum;
        }

        public BuffStatusEffect(BuffStatusEffectData _data, UnitInstance _effectApplier, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, int _originSkillLevel, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null) : base(_data, _effectApplier, _originSkillLevel, 0, _system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect)
        {
            TargetStatusType = _data.TargetStatusType;
            Value = _data.Value; // Getting a reference to an object stored within GameDataContainer
            IsSum = _data.IsSum;
        }

        #region Properties
        public eStatusType TargetStatusType { get; }
        public Tag Value { get; }
        public bool IsSum { get; }
        #endregion

        #region Public Methods
        BuffStatusEffect IDeepCopyable<BuffStatusEffect>.DeepCopy() { return (BuffStatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (BuffStatusEffect)base.DeepCopyInternally(); }
        #endregion
    }

    public class DebuffStatusEffect : BackgroundStatusEffect, IDeepCopyable<DebuffStatusEffect>
    {
        public DebuffStatusEffect(DebuffStatusEffectData _data, UnitInstance _effectApplier = null, int _originSkillLevel = 0, int _equipmentLevel = 0) : base(_data, _effectApplier, _originSkillLevel, _equipmentLevel, null, null, null, null, null, null, null, default)
        {
            TargetStatusType = _data.TargetStatusType;
            Value = _data.Value; // Getting a reference to an object stored within GameDataContainer
            IsSum = _data.IsSum;
        }

        public DebuffStatusEffect(DebuffStatusEffectData _data, UnitInstance _effectApplier, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, int _originSkillLevel, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null) : base(_data, _effectApplier, _originSkillLevel, 0, _system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect)
        {
            TargetStatusType = _data.TargetStatusType;
            Value = _data.Value; // Getting a reference to an object stored within GameDataContainer
            IsSum = _data.IsSum;
        }

        #region Properties
        public eStatusType TargetStatusType { get; }
        public Tag Value { get; }
        public bool IsSum { get; }
        #endregion

        #region Public Methods
        DebuffStatusEffect IDeepCopyable<DebuffStatusEffect>.DeepCopy() { return (DebuffStatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (DebuffStatusEffect)base.DeepCopyInternally(); }
        #endregion
    }

    public class TargetRangeModStatusEffect : BackgroundStatusEffect, IDeepCopyable<TargetRangeModStatusEffect>
    {
        public TargetRangeModStatusEffect(TargetRangeModStatusEffectData _data, UnitInstance _effectApplier = null, int _originSkillLevel = 0, int _equipmentLevel = 0) : base(_data, _effectApplier, _originSkillLevel, _equipmentLevel, null, null, null, null, null, null, null, default)
        {
            IsMovementRangeClassification = _data.IsMovementRangeClassification;
            TargetRangeClassification = _data.TargetRangeClassification;
            ModificationMethod = _data.ModificationMethod;
        }

        public TargetRangeModStatusEffect(TargetRangeModStatusEffectData _data, UnitInstance _effectApplier, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, int _originSkillLevel, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null) : base(_data, _effectApplier, _originSkillLevel, 0, _system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect)
        {
            IsMovementRangeClassification = _data.IsMovementRangeClassification;
            TargetRangeClassification = _data.TargetRangeClassification;
            ModificationMethod = _data.ModificationMethod;
        }

        #region Properties
        public bool IsMovementRangeClassification { get; }
        public eTargetRangeClassification TargetRangeClassification { get; }
        public eModificationMethod ModificationMethod { get; }
        #endregion

        #region Public Methods
        TargetRangeModStatusEffect IDeepCopyable<TargetRangeModStatusEffect>.DeepCopy() { return (TargetRangeModStatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (TargetRangeModStatusEffect)base.DeepCopyInternally(); }
        #endregion
    }
    #endregion

    #region Foreground
    public abstract class ForegroundStatusEffect : StatusEffect, IDeepCopyable<ForegroundStatusEffect>
    {
        public ForegroundStatusEffect(ForegroundStatusEffectData _data, UnitInstance _effectApplier, int _originSkillLevel, int _equipmentLevel, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect) : base(_data, _effectApplier, _originSkillLevel, _equipmentLevel, _system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect)
        {
            ActivationTurnClassification = _data.ActivationTurnClassification;
            EventTriggerTiming = _data.EventTriggerTiming;
            AnimationInfo = _data.AnimationInfo;
        }

        #region Properties
        public eActivationTurnClassification ActivationTurnClassification { get; }
        public eEventTriggerTiming EventTriggerTiming { get; }
        public SimpleAnimationInfo AnimationInfo { get; }
        #endregion

        #region Public Methods
        ForegroundStatusEffect IDeepCopyable<ForegroundStatusEffect>.DeepCopy() { return (ForegroundStatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (ForegroundStatusEffect)base.DeepCopyInternally(); }
        #endregion
    }

    public class DamageStatusEffect : ForegroundStatusEffect, IDeepCopyable<DamageStatusEffect>
    {
        public DamageStatusEffect(DamageStatusEffectData _data, UnitInstance _effectApplier, int _originSkillLevel = 0, int _equipmentLevel = 0) : base(_data, _effectApplier, _originSkillLevel, _equipmentLevel, null, null, null, null, null, null, null, default)
        {
            Damage = _data.Value; // Getting a reference to an object stored within GameDataContainer
        }

        public DamageStatusEffect(DamageStatusEffectData _data, UnitInstance _effectApplier, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, int _originSkillLevel, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null) : base(_data, _effectApplier, _originSkillLevel, 0, _system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect)
        {
            Damage = _data.Value; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public Tag Damage { get; }
        #endregion

        #region Public Methods
        DamageStatusEffect IDeepCopyable<DamageStatusEffect>.DeepCopy() { return (DamageStatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (DamageStatusEffect)base.DeepCopyInternally(); }
        #endregion
    }

    public class HealStatusEffect : ForegroundStatusEffect, IDeepCopyable<HealStatusEffect>
    {
        public HealStatusEffect(HealStatusEffectData _data, UnitInstance _effectApplier, int _originSkillLevel = 0, int _equipmentLevel = 0) : base(_data, _effectApplier, _originSkillLevel, _equipmentLevel, null, null, null, null, null, null, null, default)
        {
            HPAmount = _data.Value; // Getting a reference to an object stored within GameDataContainer
        }

        public HealStatusEffect(HealStatusEffectData _data, UnitInstance _effectApplier, BattleSystemCore _system, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, int _originSkillLevel, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null) : base(_data, _effectApplier, _originSkillLevel, 0, _system, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect)
        {
            HPAmount = _data.Value; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public Tag HPAmount { get; }
        #endregion

        #region Public Methods
        HealStatusEffect IDeepCopyable<HealStatusEffect>.DeepCopy() { return (HealStatusEffect)DeepCopyInternally(); }
        #endregion

        #region Protected Methods
        protected override object DeepCopyInternally() { return (HealStatusEffect)base.DeepCopyInternally(); }
        #endregion
    }

    public sealed class Duration : IDeepCopyable<Duration>
    {
        public Duration(int _activationTimes, decimal _turns, ComplexCondition _whileConditions)
        {
            ActivationTimes = _activationTimes;
            Turns = _turns;
            WhileCondition = _whileConditions.CoalesceNullAndReturnDeepCopyOptionally(true);
        }

        public Duration(DurationData _data, BattleSystemCore _system, StatusEffect _statusEffect, UnitInstance _effectUser, ActiveSkill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect)
        {
            DurationData tmp_data = _data; // Getting a reference to an object stored within GameDataContainer

            ActivationTimes = tmp_data.ActivationTimes.ToValue<int>(_system, null, _statusEffect, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
            Turns = tmp_data.Turns.ToValue<decimal>(_system, null, _statusEffect, _effectUser, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect);
            WhileCondition = _data.WhileCondition.CoalesceNullAndReturnDeepCopyOptionally(true);
        }

        //Can use either one or combine properties
        #region Properties
        public int ActivationTimes { get; set; }
        public decimal Turns { get; set; } //Decimal number. 0.5 represents a player turn. 1 represents a turn of each player.
        public ComplexCondition WhileCondition { get; private set; }
        #endregion

        #region Public Mmethods
        public Duration DeepCopy()
        {
            Duration copy = (Duration)this.MemberwiseClone();

            copy.WhileCondition = WhileCondition.DeepCopy();

            return copy;
        }
        #endregion
    }
    #endregion
    #endregion
}
