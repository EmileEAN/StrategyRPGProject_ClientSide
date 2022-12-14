using System;
using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public static class StringExtension
    {
        public static Type ToCorrespondingEnumType(this string _string)
        {
            if (_string == null || _string == "")
                return default;

            try
            {
                string nameSpace = "EEANWorks.Games.TBSG._01";

                return Type.GetType(nameSpace + "." + _string);
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static object ToCorrespondingEnumValue(this string _string, string _enumTypeString)
        {
            if (_string == null || _string == "" 
                || _enumTypeString == null || _enumTypeString == "")
            {
                return null;
            }

            try
            {
                Type enumType = _enumTypeString.ToCorrespondingEnumType();

                if (enumType.IsEnum)
                {
                    foreach (var e in Enum.GetValues(enumType))
                    {
                        if (String.Compare(_string, e.ToString(), StringComparison.OrdinalIgnoreCase) == 0) //...if both strings are equal (ignoring the case)
                            return e;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class IntExtension
    {
        public static _2DCoord To2DCoord(this int _index)
        {
            return _index.To2DCoord(CoreValues.SIZE_OF_A_SIDE_OF_BOARD, CoreValues.SIZE_OF_A_SIDE_OF_BOARD);
        }
    }

    public static class _2DCoordExtension
    {
        public static int ToIndex(this _2DCoord _coord) { return _coord.ToIndex(CoreValues.SIZE_OF_A_SIDE_OF_BOARD); }
    }

    public static class NullPreventionAssignmentMethods
    {
        public static PlayerOnBoard CoalesceNull(this PlayerOnBoard _this)
        {
            if (_this != null)
                return _this;
            else
                return new PlayerOnBoard(null, default(int), default);
        }

        public static Unit CoalesceNullAndReturnDeepCopyOptionally(this Unit _this, bool _returnDeepCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnDeepCopyInstead)
                    return (_this as IDeepCopyable<Unit>).DeepCopy();
                else
                    return _this;
            }
            else
                return new Unit(null, default, string.Empty, default, default, default(List<Skill>));
        }

        public static UnitInstance CoalesceNullAndReturnDeepCopyOptionally(this UnitInstance _this, bool _returnDeepCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnDeepCopyInstead)
                    return (_this as IDeepCopyable<UnitInstance>).DeepCopy();
                else
                    return _this;
            }
            else
                return new UnitInstance(new Unit(null, default, string.Empty, default, default, default(List<Skill>)), null);
        }

        public static Skill CoalesceNullAndReturnDeepCopyOptionally(this Skill _this, bool _returnDeepCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnDeepCopyInstead)
                    return (_this as IDeepCopyable<Skill>).DeepCopy();

                return _this;
            }
            else
            {
                if (_this is OrdinarySkill) { return new OrdinarySkill(null, default); }
                else if (_this is CounterSkill) { return new CounterSkill(null, default); }
                else if (_this is UltimateSkill) { return new UltimateSkill(null, default); }
                else /*(_this is PassiveSkill)*/ { return new PassiveSkill(null, default); }
            }
        }

        public static StatusEffect CoalesceNullAndReturnDeepCopyOptionally(this StatusEffect _this, bool _returnDeepCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnDeepCopyInstead)
                    return (_this as IDeepCopyable<StatusEffect>).DeepCopy();

                return _this;
            }
            else
            {
                if (_this is BuffStatusEffect) { return new BuffStatusEffect(null); }
                else if (_this is DebuffStatusEffect) { return new DebuffStatusEffect(null); }
                else if (_this is TargetRangeModStatusEffect) { return new TargetRangeModStatusEffect(null); }
                else if (_this is DamageStatusEffect) { return new DamageStatusEffect(null, null); }
                else /*(_this is HealStatusEffect)*/ { return new HealStatusEffect(null, null); }
            }
        }

        public static Duration CoalesceNullAndReturnDeepCopyOptionally(this Duration _this, bool _returnDeepCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnDeepCopyInstead)
                    return _this.DeepCopy();
                else
                    return _this;
            }
            else
                return new Duration(default, default, null);
        }

        public static ComplexCondition CoalesceNullAndReturnDeepCopyOptionally(this ComplexCondition _this, bool _returnDeepCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnDeepCopyInstead)
                    return _this.DeepCopy();
                else
                    return _this;
            }
            else
                return new ComplexCondition(null);
        }
    }
}
