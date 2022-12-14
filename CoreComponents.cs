using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEANWorks
{
    public interface IDeepCopyable<T>
    {
        T DeepCopy();
    }

    public static class CoreFunctions
    {
        #region Public Methods
        public static bool Compare(object _a, eRelationType _relation, object _b)
        {
            var comparableA = _a as IComparable;
            var comparableB = _b as IComparable;

            try
            {
                if (comparableA != null && comparableB != null)
                {
                    return Compare(comparableA, _relation, comparableB);
                }
                else
                {
                    switch (_relation)
                    {
                        case eRelationType.EqualTo: return _a.Equals(_b);
                        case eRelationType.NotEqualTo: return !_a.Equals(_b);
                        default: return false; //Other comparisons cannot be applied to a non-IComparable type
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool IsSuccess(decimal _probability)
        {
            if (_probability <= 0.0m)
                return false;
            else if (_probability >= 1.0m)
                return true;
            else
            {
                int maxDecimalPoints = 6;
                int multiplier = Convert.ToInt32(Math.Pow(10, maxDecimalPoints));

                int rangeNumber = Convert.ToInt32(Math.Round(_probability * multiplier, MidpointRounding.AwayFromZero));

                MTRandom.RandInit();
                int randomNumber = MTRandom.GetRandInt(1, multiplier);

                if (randomNumber < rangeNumber)
                    return true;
                else
                    return false;
            }
        }

        //_a and _b will be calculated as decimal
        public static object Sum(object _a, object _b)
        {
            try
            {
                decimal a = Convert.ToDecimal(_a);
                decimal b = Convert.ToDecimal(_b);

                return a + b;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static object Subtract(object _a, object _b)
        {
            try
            {
                decimal a = Convert.ToDecimal(_a);
                decimal b = Convert.ToDecimal(_b);

                return a - b;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static object Multiply(object _a, object _b)
        {
            try
            {
                decimal a = Convert.ToDecimal(_a);
                decimal b = Convert.ToDecimal(_b);

                return a * b;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static object Divide(object _a, object _b)
        {
            try
            {
                decimal a = Convert.ToDecimal(_a);
                decimal b = Convert.ToDecimal(_b);

                return a / b;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Private Methods
        private static bool Compare<T>(T _a, eRelationType _relation, T _b) where T : IComparable
        {
            switch (_relation)
            {
                case eRelationType.EqualTo:
                    return _a.CompareTo(_b) == 0;
                case eRelationType.NotEqualTo:
                    return _a.CompareTo(_b) != 0;
                case eRelationType.GreaterThan:
                    return _a.CompareTo(_b) > 0;
                case eRelationType.LessThan:
                    return _a.CompareTo(_b) < 0;
                case eRelationType.GreaterThanOrEqualTo:
                    return _a.CompareTo(_b) >= 0;
                case eRelationType.LessThanOrEqualTo:
                    return _a.CompareTo(_b) <= 0;
                default:
                    return false;
            }
        }
        #endregion
    }

    public enum eRelationType
    {
        EqualTo,
        NotEqualTo,
        GreaterThan,
        LessThan,
        GreaterThanOrEqualTo,
        LessThanOrEqualTo
    }

    public enum eSortType
    {
        Descending,
        Ascending
    }

    public enum e3DCoordValueSwapMethod
    {
        RotateToRight,
        RotateToLeft,
        XAndY,
        YAndZ,
        ZAndX
    }

    public struct DelegateWithParameters<T>
    {
        public DelegateWithParameters(Delegate _method, params T[] _params)
        {
            Method = _method;
            Params = _params;
        }

        public readonly Delegate Method;
        public readonly T[] Params;
    }
}
