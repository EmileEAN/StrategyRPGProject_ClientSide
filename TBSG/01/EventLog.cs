using System;
using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public abstract class EventLog
    {
        public EventLog(decimal _eventTurn)
        {
            EventTurn = _eventTurn;
        }

        #region Properties
        public decimal EventTurn { get; }
        #endregion
    }

    public abstract class ActionLog : EventLog
    {
        public ActionLog(decimal _actionTurn, int _actorId, string _actorName, string _actorNickname) : base(_actionTurn)
        {
            ActorId = _actorId;
            ActorName = _actorName.CoalesceNullAndReturnCopyOptionally(true);
            ActorNickname = _actorNickname.CoalesceNullAndReturnCopyOptionally(true);
        }

        #region Properties
        public int ActorId { get; }
        public string ActorName { get; }
        public string ActorNickname { get; }
        #endregion
    }

    public abstract class AutomaticEventLog : EventLog
    {
        public AutomaticEventLog(decimal _eventTurn) : base(_eventTurn)
        {
        }
    }

    public class TurnChangeEventLog : AutomaticEventLog
    {
        public TurnChangeEventLog(decimal _eventTurn,
            int _turnEndingPlayerId, int _turnInitiatingPlayerId, int _remainingSPForTurnEndingPlayer, int _remainingSPForTurnInitiatingPlayer) : base(_eventTurn)
        {
            TurnEndingPlayerId = _turnEndingPlayerId;
            TurnInitiatingPlayerId = _turnInitiatingPlayerId;

            RemainingSPForTurnEndingPlayer = _remainingSPForTurnEndingPlayer;
            RemainingSPForTurnInitiatingPlayer = _remainingSPForTurnInitiatingPlayer;
        }

        #region Properties
        public int TurnEndingPlayerId { get; }
        public int TurnInitiatingPlayerId { get; }
        
        public int RemainingSPForTurnEndingPlayer { get; }
        public int RemainingSPForTurnInitiatingPlayer { get; }
        #endregion
    }

    //public class PassiveSkillLog : AutomaticEventLog
    //{


    //    #region Properties
    //    public UnitInstance SkillOwner { get; private set; }
    //    #endregion
    //}

    //public class TileTrapLog : AutomaticEventLog
    //{

    //}

    public abstract class StatusEffectLog : AutomaticEventLog
    {
        public StatusEffectLog(decimal _eventTurn,
            int _effectHolderId, string _effectHolderName, string _effectHolderNickname) : base(_eventTurn)
        {
            EffectHolderId = _effectHolderId;
            EffectHolderName = _effectHolderName.CoalesceNullAndReturnCopyOptionally(true);
            EffectHolderNickname = _effectHolderNickname.CoalesceNullAndReturnCopyOptionally(true);
        }

        #region Properties
        public int EffectHolderId { get; }
        public string EffectHolderName { get; }
        public string EffectHolderNickname { get; }
        #endregion
    }

    public class StatusEffectLog_HPModification : StatusEffectLog
    {
        public StatusEffectLog_HPModification(decimal _eventTurn, int _effectHolderId, string _effectHolderName, string _effectHolderNickname,
            bool _isPositive, int _value, int _remainingHPAfterModification) : base(_eventTurn, _effectHolderId, _effectHolderName, _effectHolderNickname)
        {
            IsPositive = _isPositive;
            Value = _value;
            RemainingHPAfterModification = _remainingHPAfterModification;
        }

        #region Properties
        public bool IsPositive { get; } // If true, Value represents the amout healed. If false, Value represents the damage dealt. Both healing and damaging value can be zero.
        public int Value { get; }
        public int RemainingHPAfterModification { get; }
        #endregion
    }

    public class ActionLog_Move : ActionLog
    {
        public ActionLog_Move(decimal _actionTurn, int _actorId, string _actorName, string _actorNickname,
            _2DCoord _initialCoord, _2DCoord _eventualCoord) : base(_actionTurn, _actorId, _actorName, _actorNickname)
        {
            InitialCoord = _initialCoord.CoalesceNullAndReturnCopyOptionally(true);
            EventualCoord = _eventualCoord.CoalesceNullAndReturnCopyOptionally(true);
        }

        #region Properties
        public _2DCoord InitialCoord { get; }
        public _2DCoord EventualCoord { get; }
        #endregion
    }

    public class ActionLog_Attack : ActionLog
    {
        public ActionLog_Attack(decimal _actionTurn, int _actorId, string _actorName, string _actorNickname,
            int _actorLocationTileIndex, int _targetId, int _targetLocationTileIndex) : base(_actionTurn, _actorId, _actorName, _actorNickname)
        {
            ActorLocationTileIndex = _actorLocationTileIndex;
            TargetId = _targetId;
            TargetLocationTileIndex = _targetLocationTileIndex;
        }

        #region Properties
        public int ActorLocationTileIndex { get; }
        public int TargetId { get; }
        public int TargetLocationTileIndex { get; }
        #endregion
    }

    public abstract class ActionLog_Skill : ActionLog
    {
        public ActionLog_Skill(decimal _actionTurn, int _actorId, string _actorName, string _actorNickname,
            string _skillName, int _actorLocationTileIndex, int _remainingSPAfterUsingSkill, int _animationId) : base(_actionTurn, _actorId, _actorName, _actorNickname)
        {
            SkillName = _skillName.CoalesceNullAndReturnCopyOptionally(true);

            ActorLocationTileIndex = _actorLocationTileIndex;

            RemainingSPAfterUsingSkill = _remainingSPAfterUsingSkill;

            AnimationId = _animationId;
        }

        #region Properties
        public string SkillName { get; }

        public int ActorLocationTileIndex { get; }

        public int RemainingSPAfterUsingSkill { get; }

        public int AnimationId { get; }
        #endregion
    }

    public class ActionLog_UnitTargetingSkill : ActionLog_Skill
    {
        public ActionLog_UnitTargetingSkill(decimal _actionTurn, int _actorId, string _actorName, string _actorNickname, string _skillName, int _actorLocationTileIndex, int _remainingSPAfterUsingSkill, int _animationId,
            List<Tuple<string, string, string>> _targetsName_Nickname_OwnerName, List<Tuple<string, string, string>> _secondaryTargetsName_Nickname_OwnerName) : base(_actionTurn, _actorId, _actorName, _actorNickname, _skillName, _actorLocationTileIndex, _remainingSPAfterUsingSkill, _animationId)
        {
            m_targetsName_Nickname_OwnerName = _targetsName_Nickname_OwnerName.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
            m_secondaryTargetsName_Nickname_OwnerName = _secondaryTargetsName_Nickname_OwnerName.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
        }

        #region Properties
        public IList<Tuple<string, string, string>> TargetsName_Nickname_OwnerName { get { return m_targetsName_Nickname_OwnerName.AsReadOnly(); } }
        public IList<Tuple<string, string, string>> SecondaryTargetsName_Nickname_OwnerName { get { return m_secondaryTargetsName_Nickname_OwnerName.AsReadOnly(); } }
        #endregion

        #region Private Fields
        private List<Tuple<string, string, string>> m_targetsName_Nickname_OwnerName;
        private List<Tuple<string, string, string>> m_secondaryTargetsName_Nickname_OwnerName;
        #endregion
    }

    public class ActionLog_TileTargetingSkill : ActionLog_Skill
    {
        public ActionLog_TileTargetingSkill(decimal _actionTurn, int _actorId, string _actorName, string _actorNickname, string _skillName, int _actorLocationTileIndex, int _remainingSPAfterUsingSkill, int _animationId,
            List<_2DCoord> _targetCoords, List<_2DCoord> _secondaryTargetCoords) : base(_actionTurn, _actorId, _actorName, _actorNickname, _skillName, _actorLocationTileIndex, _remainingSPAfterUsingSkill, _animationId)
        {
            m_targetCoords = _targetCoords.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
            m_secondaryTargetCoords = _secondaryTargetCoords.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);
        }

        #region Properties
        public IList<_2DCoord> TargetCoords { get { return m_targetCoords.AsReadOnly(); } }
        public IList<_2DCoord> SecondaryTargetCoords { get { return m_secondaryTargetCoords.AsReadOnly(); } }
        #endregion

        #region Private Fields
        private List<_2DCoord> m_targetCoords;
        private List<_2DCoord> m_secondaryTargetCoords;
        #endregion
    }

    public abstract class EffectTrialLog : AutomaticEventLog
    {
        public EffectTrialLog(decimal _eventTurn,
            AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed) : base(_eventTurn)
        {
            AnimationInfo = _animationInfo;
            IsDiffused = _isDiffused;

            DidActivate = _didActivate;
            DidSucceed = _didSucceed;
        }

        #region Properties
        public AnimationInfo AnimationInfo { get; }
        public bool IsDiffused { get; }

        public bool DidActivate { get; }
        public bool DidSucceed { get; }
        #endregion
    }

    public abstract class EffectTrialLog_UnitTargetingEffect : EffectTrialLog
    {
        public EffectTrialLog_UnitTargetingEffect(decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed,
            int _targetId, string _targetName, string _targetNickname, int _targetLocationTileIndex) : base(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed)
        {
            TargetId = _targetId;
            TargetName = _targetName.CoalesceNullAndReturnCopyOptionally(true);
            TargetNickname = _targetNickname.CoalesceNullAndReturnCopyOptionally(true);
            TargetLocationTileIndex = _targetLocationTileIndex;
        }

        #region Properties
        public int TargetId { get; }
        public string TargetName { get; }
        public string TargetNickname { get; }
        public int TargetLocationTileIndex { get; }
        #endregion
    }

    public class EffectTrialLog_DamageEffect : EffectTrialLog_UnitTargetingEffect
    {
        public EffectTrialLog_DamageEffect(decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed, int _targetId, string _targetName, string _targetNickname, int _targetLocationTileIndex,
            bool _wasImmune, bool _wasCritical, eEffectiveness _effectiveness, int _value, int _remainingHPAfterModification) : base(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, _targetId, _targetName, _targetNickname, _targetLocationTileIndex)
        {
            WasImmune = _wasImmune;
            WasCritical = _wasCritical;
            Effectiveness = _effectiveness;

            Value = _value;
            RemainingHPAfterModification = _remainingHPAfterModification;
        }

        #region Properties
        public bool WasImmune { get; }
        public bool WasCritical { get; }
        public eEffectiveness Effectiveness { get; }

        public int Value { get; }
        public int RemainingHPAfterModification { get; }
        #endregion
    }

    public class EffectTrialLog_HealEffect : EffectTrialLog_UnitTargetingEffect
    {
        public EffectTrialLog_HealEffect(decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed, int _targetId, string _targetName, string _targetNickname, int _targetLocationTileIndex,
            bool _wasCritical, int _value, int _remainingHPAfterModification) : base(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, _targetId, _targetName, _targetNickname, _targetLocationTileIndex)
        {
            WasCritical = _wasCritical;

            Value = _value;
            RemainingHPAfterModification = _remainingHPAfterModification;
        }

        #region Properties
        public bool WasCritical { get; }

        public int Value { get; }
        public int RemainingHPAfterModification { get; }
        #endregion
    }

    public class EffectTrialLog_StatusEffectAttachmentEffect : EffectTrialLog_UnitTargetingEffect
    {
        public EffectTrialLog_StatusEffectAttachmentEffect(decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed, int _targetId, string _targetName, string _targetNickname, int _targetLocationTileIndex,
            int _attachedStatusEffectId) : base(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed, _targetId, _targetName, _targetNickname, _targetLocationTileIndex)
        {
            AttachedStatusEffectId = _attachedStatusEffectId;
        }

        #region Properties
        public int AttachedStatusEffectId { get; }
        #endregion
    }

    public abstract class EffectTrialLog_TileTargetingEffect : EffectTrialLog
    {
        public EffectTrialLog_TileTargetingEffect(decimal _eventTurn, AnimationInfo _animationInfo, bool _isDiffused, bool _didActivate, bool _didSucceed,
            _2DCoord _targetCoord) : base(_eventTurn, _animationInfo, _isDiffused, _didActivate, _didSucceed)
        {
            TargetCoord = _targetCoord.CoalesceNullAndReturnCopyOptionally(true);
        }

        #region Properties
        public _2DCoord TargetCoord { get; }
        #endregion
    }

    public class EffectTrialLog_MovementEffect : EffectTrialLog_TileTargetingEffect
    {
        public EffectTrialLog_MovementEffect(decimal _eventTurn, MovementAnimationInfo _animationInfo, bool _didActivate, _2DCoord _targetCoord,
            _2DCoord _initialCoord) : base(_eventTurn, _animationInfo, false, _didActivate, true, _targetCoord)
        {
            InitialCoord = _initialCoord.CoalesceNullAndReturnCopyOptionally(true);
        }

        #region Properties
        public _2DCoord InitialCoord { get; }
        #endregion
    }
}
