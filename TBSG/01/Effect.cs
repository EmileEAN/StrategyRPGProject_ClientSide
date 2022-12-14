using EEANWorks;
using System.Collections.Generic;
using System.Linq;

namespace EEANWorks.Games.TBSG._01
{
    public interface IComplexTargetSelectionEffect
    {
    }

    public abstract class Effect
    {
        public Effect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo)
        {
            Id = _id;

            ActivationCondition = _activationCondition.CoalesceNullAndReturnDeepCopyOptionally(true);

            TimesToApply = _timesToApply; // Getting a reference to an object stored within GameDataContainer

            SuccessRate = _successRate; // Getting a reference to an object stored within GameDataContainer

            DiffusionDistance = _diffusionDistance; // Getting a reference to an object stored within GameDataContainer

            m_secondaryEffects = _secondaryEffects.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer

            AnimationInfo = _animationInfo; // Getting a reference to an object stored within GameDataContainer

            m_isSecondaryEffectListModifiable = true;
        }

        #region Properties
        public int Id { get; }

        public ComplexCondition ActivationCondition { get; }

        public Tag TimesToApply { get; } //Tag value must be positive integer
        public Tag SuccessRate { get; } //Tag value must be between 0 and 1

        public Tag DiffusionDistance { get; } //Tag value must be 0 or a positive integer

        public IList<Effect> SecondaryEffects
        {
            get
            {
                if (m_isSecondaryEffectListModifiable)
                    return m_secondaryEffects;
                else
                    return m_secondaryEffects.AsReadOnly();
            }
        }

        public AnimationInfo AnimationInfo { get; } // Will be null only for the basic damage effect
        #endregion

        #region Private Fields
        private List<Effect> m_secondaryEffects; // The eComplexTargetSelectionEffectTargetType value may not always be used

        private bool m_isSecondaryEffectListModifiable;
        #endregion

        #region Public Methods
        public virtual string ToFormattedString(int _level)
        {
            int effectCount = 1;

            return "<b>[Effect]</b>"
                    + "\n"
                    + ((ActivationCondition.ConditionSets.Count != 0) ? ("If " + ActivationCondition.ToFormattedString(_level) + ", with ") : "With ")
                    + "a success rate of " + SuccessRate.ToFormattedString(_level) + ", "
                    + "execute the following effect " + TimesToApply.ToFormattedString(_level) + " times: ";
        }

        public void DisableModification() { m_isSecondaryEffectListModifiable = false; }
        #endregion
    }

    public abstract class UnitTargetingEffect : Effect
    {
        public UnitTargetingEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo,
            eTargetUnitClassification _targetClassification) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo)
        {
            TargetClassification = _targetClassification;
        }

        #region Properties
        public eTargetUnitClassification TargetClassification { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level)
        {
            return "<b>[Target Unit Classification]</b>"
                    + "\n"
                    + TargetClassification.ToString()
                    + "\n\n"
                    + base.ToFormattedString(_level);
        }
        #endregion
    }

    public class UnitTargetingEffectsWrapperEffect : UnitTargetingEffect // Mock effect to wrap multiple effects as the main effect
    {
        public UnitTargetingEffectsWrapperEffect(int _id, ComplexCondition _activationCondition, List<UnitTargetingEffect> _secondaryEffects, eTargetUnitClassification _targetClassification) : base(_id, _activationCondition, null, null, null, _secondaryEffects.Cast<Effect>().ToList(), default, _targetClassification)
        {
        }

        #region Public Methods
        public override string ToFormattedString(int _level)
        {
            return base.ToFormattedString(_level);
        }
        #endregion
    }

    public class DamageEffect : UnitTargetingEffect
    {
        public DamageEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification,
            eAttackClassification _attackClassification, Tag _value, bool _isFixedValue, eElement _element) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification)
        {
            AttackClassification = _attackClassification;

            Value = _value; // Getting a reference to an object stored within GameDataContainer

            IsFixedValue = _isFixedValue;
            Element = _element;
        }
        public DamageEffect(DrainEffect _drainEffect) : base(_drainEffect.Id, _drainEffect.ActivationCondition, _drainEffect.TimesToApply, _drainEffect.SuccessRate, _drainEffect.DiffusionDistance, _drainEffect.SecondaryEffects.ToList(), _drainEffect.AnimationInfo, _drainEffect.TargetClassification)
        {
            AttackClassification = _drainEffect.AttackClassification;

            Value = _drainEffect.Value; // Getting a reference to an object stored within GameDataContainer

            IsFixedValue = _drainEffect.IsFixedValue;
            Element = _drainEffect.Element;
        }

        #region Properties
        public eAttackClassification AttackClassification { get; }
        public Tag Value { get; }
        public bool IsFixedValue { get; }
        public eElement Element { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level)
        {
            return base.ToFormattedString(_level) + "<color=red>Deal " + Value.ToFormattedString(_level) + (IsFixedValue ? "(fixed) " : "") +  ((AttackClassification == eAttackClassification.Physic) ? " physical " : " magical ") + "<" + Element.ToString() + " element>" + "damage</color>";
        }
        #endregion
    }

    //public class Summon : Effect
    //{
    //    //    public string ServantName { get; set; }
    //    //    public int AmountToSummon { get; set; }
    //}

    //public class Transform : Effect
    //{
    //    //    public string CharacterName { get; set; }
    //    //    public int Duration { get; set; }
    //}

    public class HealEffect : UnitTargetingEffect
    {
        public HealEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification,
            Tag _value, bool _isFixedValue) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification)
        {
            Value = _value; // Getting a reference to an object stored within GameDataContainer

            IsFixedValue = _isFixedValue;
        }

        #region Properties
        public Tag Value { get; }
        public bool IsFixedValue { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level)
        {
            return base.ToFormattedString(_level) + "<color=green>Restore " + Value.ToFormattedString(_level) + (IsFixedValue ? "(fixed) " : "") + "HP</color>.";
        }
        #endregion
    }

    public class StatusEffectAttachmentEffect : UnitTargetingEffect
    {
        public StatusEffectAttachmentEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification,
            StatusEffectData _dataOfStatusEffectToAttach) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification)
        {
            DataOfStatusEffectToAttach = _dataOfStatusEffectToAttach; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public StatusEffectData DataOfStatusEffectToAttach { get; }
        #endregion

        #region Public Methods
        public override string ToFormattedString(int _level)
        {
            return base.ToFormattedString(_level) + "Attach the following status effect to the target: " + DataOfStatusEffectToAttach.ToFormattedString(_level);
        }
        #endregion
    }

    //public class UnitSwapEffect : UnitTargetingEffect, IDeepCopyable<UnitSwapEffect>
    //{
    //    public UnitSwapEffect(int _id, ComplexCondition _activationCondition, Tag _successRate, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification) : base(_id, _activationCondition, Tag.One, _successRate, null, _secondaryEffects, _animationInfo, _targetClassification)
    //    {
    //    }

    //    #region Properties
    //    #endregion

    //    #region Public Methods
    //    UnitSwapEffect IDeepCopyable<UnitSwapEffect>.DeepCopy() { return (UnitSwapEffect)DeepCopyInternally(); }
    //    #endregion

    //    #region Protected Methods
    //    protected override object DeepCopyInternally()
    //    {
    //        UnitSwapEffect copy = (UnitSwapEffect)base.DeepCopyInternally();

    //        return copy;
    //    }
    //    #endregion
    //}

    public class DrainEffect : UnitTargetingEffect, IComplexTargetSelectionEffect
    {
        public DrainEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo, eTargetUnitClassification _targetClassification,
            Tag _maxNumberOfSecondaryTargets, eAttackClassification _attackClassification, Tag _value, bool _isFixedValue, eElement _element, Tag _drainingEfficiency, SimpleAnimationInfo _healAnimationInfo) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo, _targetClassification)
        {
            MaxNumberOfSecondaryTargets = _maxNumberOfSecondaryTargets; // Getting a reference to an object stored within GameDataContainer

            AttackClassification = _attackClassification;

            Value = _value; // Getting a reference to an object stored within GameDataContainer

            IsFixedValue = _isFixedValue;
            Element = _element;

            DrainingEfficiency = _drainingEfficiency; // Getting a reference to an object stored within GameDataContainer

            HealAnimationInfo = _healAnimationInfo; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public Tag MaxNumberOfSecondaryTargets { get; }

        public eAttackClassification AttackClassification { get; }
        public Tag Value { get; }
        public bool IsFixedValue { get; }
        public eElement Element { get; }

        public Tag DrainingEfficiency { get; }

        public SimpleAnimationInfo HealAnimationInfo { get; }
        #endregion
    }

    public abstract class TileTargetingEffect : Effect
    {
        public TileTargetingEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo)
        {
        }

        #region Public Methods
        public override string ToFormattedString(int _level)
        {
            return base.ToFormattedString(_level);
        }
        #endregion
    }

    public class MovementEffect : TileTargetingEffect
    {
        public MovementEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, List<Effect> _secondaryEffects, MovementAnimationInfo _animationInfo) : base (_id, _activationCondition, _timesToApply, Tag.One, Tag.Zero, _secondaryEffects, _animationInfo)
        {
        }

        #region Public Methods
        public override string ToFormattedString(int _level)
        {
            return base.ToFormattedString(_level) + "Move to selected tile.";
        }
        #endregion
    }

    //public class TileTrapEffect : TileTargetingEffect, IDeepCopyable<TileTrapEffect>
    //{
    //    public TileTrapEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo,
    //        List<Effect> _effectsToAttach) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo)
    //    {
    //        m_effectsToAttach = _effectsToAttach.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
    //    }

    //    #region Properties
    //    public IList<Effect> EffectsToAttach { get { return m_effectsToAttach.AsReadOnly(); } }
    //    #endregion

    //    #region Private Fields
    //    private List<Effect> m_effectsToAttach;
    //    #endregion

    //    #region Public Methods
    //    TileTrapEffect IDeepCopyable<TileTrapEffect>.DeepCopy() { return (TileTrapEffect)DeepCopyInternally(); }
    //    #endregion

    //    #region Protected Methods
    //    protected override object DeepCopyInternally()
    //    {
    //        TileTrapEffect copy = (TileTrapEffect)base.DeepCopyInternally();

    //        copy.m_effectsToAttach = m_effectsToAttach.DeepCopy();

    //        return copy;
    //    }
    //    #endregion
    //}

    public class TileSwapEffect : TileTargetingEffect
    {
        public TileSwapEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo)
        {
        }
    }

    //public abstract class TargetlessEffect : Effect, IDeepCopyable<TargetlessEffect>
    //{
    //    public TargetlessEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo)
    //    {
    //    }

    //    #region Public Methods
    //    TargetlessEffect IDeepCopyable<TargetlessEffect>.DeepCopy() { return (TargetlessEffect)DeepCopyInternally(); }
    //    #endregion

    //    #region Protected Methods
    //    protected override object DeepCopyInternally()
    //    {
    //        TargetlessEffect copy = (TargetlessEffect)base.DeepCopyInternally();

    //        return copy;
    //    }
    //    #endregion
    //}

    //public class ItemCreationEffect : TargetlessEffect, IDeepCopyable<ItemCreationEffect>
    //{
    //    public ItemCreationEffect(int _id, ComplexCondition _activationCondition, Tag _timesToApply, Tag _successRate, Tag _diffusionDistance, List<Effect> _secondaryEffects, AnimationInfo _animationInfo) : base(_id, _activationCondition, _timesToApply, _successRate, _diffusionDistance, _secondaryEffects, _animationInfo)
    //    {
    //    }

    //    #region Public Methods
    //    ItemCreationEffect IDeepCopyable<ItemCreationEffect>.DeepCopy() { return (ItemCreationEffect)DeepCopyInternally(); }
    //    #endregion

    //    #region Protected Methods
    //    protected override object DeepCopyInternally()
    //    {
    //        ItemCreationEffect copy = (ItemCreationEffect)base.DeepCopyInternally();

    //        return copy;
    //    }
    //    #endregion

    //    //    public eItemType ProductType { get; set; }
    //    //    public string ProductName { get; set; }
    //    //    public int AmountToCreate { get; set; }
    //}

    //public class Cure : Effect
    //{
    //    public eAilmentType TargetAilment { get; set; }
    //    public int Value { get; set; }
    //}
}
