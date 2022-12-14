using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EEANWorks
{
    public static class ObjectExtension
    {
        public static bool IsNumeric(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.IsNumeric();
        }

        public static bool IsStruct(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.Is_struct();
        }

        public static bool IsArray(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.IsArray;
        }

        public static bool IsReadOnlyCollection(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.IsReadOnlyCollection();
        }

        public static bool IsList(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.IsList();
        }

        public static bool IsReadOnlyDictionary(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.IsReadOnlyDictionary();
        }

        public static bool IsDictionary(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.IsDictionary();
        }

        public static bool IsIDeepCopyable(this object _object)
        {
            if (_object == null)
                return false;

            Type type = _object.GetType();
            return type.IsIDeepCopyable();
        }

        public static T ToT<T>(this object _object)
        {
            if (_object == null)
                return default;

            if (_object is T)
                return (T)_object;
            else if (_object.IsArray())
            {
                Type tType = typeof(T).GetInternalTypesOfCollection()[0];

                MethodInfo methodInfo_ToArray = typeof(ObjectExtension).GetMethod("ToArray").MakeGenericMethod(tType);

                return methodInfo_ToArray.Invoke(null, new[] { _object }).ToT<T>();
            }
            else if (_object.IsList())
            {
                Type tType = typeof(T).GetInternalTypesOfCollection()[0];

                MethodInfo methodInfo_ToList = typeof(ObjectExtension).GetMethod("ToList").MakeGenericMethod(tType);

                return methodInfo_ToList.Invoke(null, new[] { _object }).ToT<T>();
            }
            else if (_object.IsDictionary())
            {
                List<Type> internalTypes = typeof(T).GetInternalTypesOfCollection(); //Gets the types of the elements in the dictionary. For instance, int and List<string> will be returned for Dictionary<int, List<string>>
                Type keyType = internalTypes[0];
                Type valueType = internalTypes[1];

                MethodInfo methodInfo_ToDictionary = typeof(ObjectExtension).GetMethod("ToDictionary").MakeGenericMethod(keyType, valueType);

                return methodInfo_ToDictionary.Invoke(null, new[] { _object }).ToT<T>();
            }
            else if (_object is string && typeof(T).IsEnum)
                return ((string)_object).ToCorrespondingEnumValue<T>();
            else
            {
                try
                {
                    return (T)Convert.ChangeType(_object, typeof(T));
                }
                catch (Exception ex)
                {
                    return default;
                }
            }
        }

        public static T[] ToArray<T>(this object _object)
        {
            if (_object == null || !_object.IsArray())
                return null;

            T[] array = (T[])_object;

            T[] result = new T[array.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array[i].ToT<T>();
            }

            return result;
        }

        public static List<T> ToList<T>(this object _object)
        {
            List<T> result = new List<T>();

            if (_object == null || (!_object.IsList() && !_object.IsReadOnlyCollection()))
                return null;

            IList list = (IList)_object;

            //Type tType = typeof(T);

            foreach (var item in list)
            {
                result.Add(item.ToT<T>());
            }

            return result;
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this object _object)
        {
            Dictionary<K, V> result = new Dictionary<K, V>();

            if (_object == null || (!_object.IsList() && !_object.IsReadOnlyDictionary()))
                return null;

            IDictionary dic = (IDictionary)_object;

            foreach (DictionaryEntry entry in dic)
            {
                result.Add(entry.Key.ToT<K>(), entry.Value.ToT<V>());
            }

            return result;
        }

        /// <summary>
        /// Checkes whether _value implements IDeepCopyable<>. Returns the results of _value.DeepCopy() if true. Returns null if false.
        /// </summary>
        public static object DeepCopy(this object _object)
        {
            if (_object == null)
                return null;

            Type type = _object.GetType();
            if (_object.IsIDeepCopyable())
                return type.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDeepCopyable<>)).GetMethod("DeepCopy").Invoke(_object, null);

            return null;
        }
    }

    public static class TypeExtension
    {
        public static bool IsNumeric(this Type _type)
        {
            switch(Type.GetTypeCode(_type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool Is_struct(this Type _type) { return (_type != null && _type.IsValueType && !_type.IsNumeric() && !_type.IsEnum && !(Type.GetTypeCode(_type) == TypeCode.Boolean) && !(Type.GetTypeCode(_type) == TypeCode.Char)); }

        public static bool IsReadOnlyCollection(this Type _type) { return _type != null && ((_type.IsGenericType) ? _type.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>) : false); }

        public static bool IsList(this Type _type) { return _type != null && ((_type.IsGenericType) ? _type.GetGenericTypeDefinition() == typeof(List<>) : false); }

        public static bool IsReadOnlyDictionary(this Type _type) { return _type != null && ((_type.IsGenericType) ? _type.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>) : false); }

        public static bool IsDictionary(this Type _type) { return _type != null && ((_type.IsGenericType) ? _type.GetGenericTypeDefinition() == typeof(Dictionary<,>) : false); }

        public static bool IsIDeepCopyable(this Type _type) { return _type != null && _type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDeepCopyable<>)); }

        public static List<Type> GetInternalTypesOfCollection(this Type _someCollectionType)
        {
            List<Type> result = new List<Type>();

            if (_someCollectionType == null)
                return result;

            List<string> internalTypeStrings = new List<string>();

            string typeString = string.Copy(_someCollectionType.ToString());

            if (typeString.EndsWith("[]")) //It is an array which has a different string format
                typeString = typeString.Remove(typeString.Length - 2);
            else
            {
                // Remove string from index 0 to the index with the first occurnce of '['
                for (int i = 0; i < typeString.Length; i++)
                {
                    if (typeString[i] == '[')
                    {
                        typeString = typeString.Remove(0, i + 1);
                        break;
                    }
                }

                // Remove the last string which should be ']'
                typeString = typeString.Remove(typeString.Length - 1);
            }

            while (typeString != string.Empty)
            {
                int openBracketsCount = 0;
                for (int i = 0; i < typeString.Length; i++)
                {
                    if (openBracketsCount == 0)
                    {
                        if (typeString[i] == ',')
                        {
                            internalTypeStrings.Add(typeString.Substring(0, i));
                            typeString = typeString.Remove(0, i + 1);
                            break;
                        }

                        if (typeString[i] == '[')
                            openBracketsCount++;
                    }
                    else
                    {
                        if (typeString[i] == '[')
                            openBracketsCount++;

                        if (typeString[i] == ']')
                            openBracketsCount--;
                    }

                    if (i == typeString.Length - 1)
                    {
                        internalTypeStrings.Add(typeString);
                        typeString = string.Empty;
                    }
                }
            }

            foreach (string internalTypeString in internalTypeStrings)
            {
                result.Add(Type.GetType(internalTypeString));
            }

            return result;
        }
    }

    public static class StringExtension
    {
        public static string Remove(this string _string, string _targetString) { return _string?.Replace(_targetString, ""); }

        public static string RemoveLast(this string _string) { return _string.Remove(_string.Length - 1, 1); }

        public static string RemoveEscapeSequences(this string _string)
        {
            if (_string == null)
                return null;

            string result = _string;

            string[] escapeSequences = { "\a", "\b", "\f", "\n", "\r", "\t", "\v", "\'", "\"", "\\" };

            while (result.ContainsAny(escapeSequences))
            {
                foreach(string escapeSequence in escapeSequences)
                {
                    result = result.Remove(escapeSequence);
                }
            }

            return result;
        }

        public static bool ContainsAny(this string _string, string[] _strings)
        {
            if (_string == null)
                return false;

            foreach(string s in _strings)
            {
                if (_string.Contains(s))
                    return true;
            }

            return false;
        }

        public static bool ContainsAll(this string _string, string[] _strings)
        {
            if (_string == null)
                return false;

            foreach (string s in _strings)
            {
                if (!_string.Contains(s))
                    return false;
            }

            return true;
        }

        public static string DetachPortion(this string _string, string _beginning, string _ending, ref string _detachedPortion)
        {
            if (_string == null || _beginning == null || _ending == null)
                return _string;

            string copy = String.Copy(_string);

            int beginningIndex = copy.IndexOf(_beginning);
            if (beginningIndex < 0)
            {
                _detachedPortion = string.Empty;
                return _string;
            }

            int endingIndex = copy.IndexOfLastChar(_ending);
            if (endingIndex < 0)
            {
                _detachedPortion = string.Empty;
                return _string;
            }

            int portionLength = endingIndex - beginningIndex + 1;

            _detachedPortion = copy.Substring(beginningIndex, portionLength);

            return copy.Remove(beginningIndex, portionLength);
        }


        /// <summary>
        /// Returns the _string without the first portion of string that starts with <_tagTitle> and ends with </_tagTitle>. _detachedPortion will be assigned with the previously mentioned portion.
        /// </summary>
        public static string DetachTagPortion(this string _string, string _tagTitle, ref string _detachedPortion)
        {
            return _string.DetachPortion("<" + _tagTitle + ">", "</" + _tagTitle + ">", ref _detachedPortion);
        }

        public static string DetachTagPortionAsValue<T>(this string _string, string _tagTitle, ref T _value)
        {
            string detachedPortion = string.Empty;
            string result = _string.DetachTagPortion(_tagTitle, ref detachedPortion);

            _value = detachedPortion.RemoveOpeningAndClosingTags(_tagTitle).ToT<T>();

            return result;
        }

        /// <summary>
        /// Returns the first portion of string that starts with <_tagTitle> and ends with </_tagTitle>. The opening and closing tags are included.
        /// </summary>
        public static string GetTagPortion(this string _string, string _tagTitle)
        {
            string detachedPortion = string.Empty;
            _string.DetachTagPortion(_tagTitle, ref detachedPortion);

            return detachedPortion;
        }

        /// <summary>
        /// Returns the first portion of string that is between <_tagTitle> and </_tagTitle>. The opening and closing tags are not included.
        /// </summary>
        public static string GetTagPortionWithoutOpeningAndClosingTags(this string _string, string _tagTitle)
        {
            string tagPortion = _string.GetTagPortion(_tagTitle);

            return tagPortion.RemoveOpeningAndClosingTags(_tagTitle);
        }

        public static T GetTagPortionValue<T>(this string _string, string _tagTitle)
        {
            return _string.GetTagPortionWithoutOpeningAndClosingTags(_tagTitle).ToT<T>();
        }

        private static string RemoveOpeningAndClosingTags(this string _string, string _tagTitle)
        {
            string openingTag = "<" + _tagTitle + ">";
            string closingTag = "</" + _tagTitle + ">";

            _string = _string.Remove(0, openingTag.Length);
            _string = _string.Remove(_string.Length - closingTag.Length, closingTag.Length);

            return _string;
        }

        public static int IndexOfLastChar(this string _string, string _target)
        {
            if (_string == null || _target == null)
                return default;

            return _string.IndexOf(_target) + _target.Length - 1;
        }

        public static object ToCorrespondingEnumValue(this string _string, Type _enumType)
        {
            if (_string == null || _string == "")
                return null;

            if (_enumType != null && _enumType.IsEnum)
            {
                foreach (var e in Enum.GetValues(_enumType))
                {
                    if (String.Compare(_string, e.ToString(), StringComparison.OrdinalIgnoreCase) == 0) //...if both strings are equal (ignoring the case)
                        return e;
                }
            }

            return null;
        }

        public static object ToCorrespondingEnumValue(this string _string, string _enumTypeString, string _nameSpaceWhereEnumsAreDefined)
        {
            if (_string == null || _string == ""
                || _enumTypeString == null || _enumTypeString == ""
                || _nameSpaceWhereEnumsAreDefined == null || _nameSpaceWhereEnumsAreDefined == "")
            {
                return null;
            }

            try
            {
                Type enumType = Type.GetType(_nameSpaceWhereEnumsAreDefined + "." + _string);

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

        public static T ToCorrespondingEnumValue<T>(this string _string)
        {
            if (_string == null || _string == "")
                return default;

            if (typeof(T).IsEnum)
            {
                foreach (T e in Enum.GetValues(typeof(T)))
                {
                    if (String.Compare(_string, e.ToString(), StringComparison.OrdinalIgnoreCase) == 0) //...if both strings are equal (ignoring the case)
                        return e;
                }
            }

            return default;
        }
    }

    public static class IntExtension
    {
        public static bool IsOdd(this int _int) { return _int % 2 == 1; }
        public static bool IsEven(this int _int) { return _int % 2 == 0; }

        public static int Middle(this int _int, bool _returnLowerIfEven = true)
        {
            if (_int.IsEven() && _returnLowerIfEven)
                return _int / 2;
            else
                return (_int / 2) + 1;
        }

        public static string ToOrdinal(this int _int)
        {
            if (_int < 0)
                return "";

            string intString = _int.ToString();

            int remainder = _int % 100;
            if (remainder >= 11 && remainder <= 13)
                return intString + "th";

            switch (_int % 10)
            {
                case 1:
                    return intString + "st";
                case 2:
                    return intString + "nd";
                case 3:
                    return intString + "rd";
                default:
                    return intString + "th";
            }
        }

        public static _2DCoord To2DCoord(this int _index, int _horizontalLength, int _verticalLength)
        {
            if (_index < 0)
                return null;

            return new _2DCoord(_index % _horizontalLength, _index / _verticalLength);
        }
    }

    public static class DoubleExtension
    {
        public static double Clamp(this double _double, double _min, double _max) { return (_double < _min) ? _min : ((_double > _max) ? _max : _double); }
        public static double Clamp01(this double _double) { return _double.Clamp(0d, 1d); }
    }

    public static class DecimalExtension
    {
        public static int Ceiling(this decimal _decimal) { return Convert.ToInt32(decimal.Ceiling(_decimal)); }

        public static int Floor(this decimal _decimal) { return Convert.ToInt32(decimal.Floor(_decimal)); }

        public static decimal Pow(this decimal _decimal, decimal _y)
        {
            decimal result = 1.0m;

            for (int i = 0; i < _y; i++)
            {
                result *= _decimal;
            }

            return result;
        }
        public static decimal Sqrt(this decimal _decimal) { return _decimal.Pow(0.5m); }

        public static decimal Reciprocal(this decimal _decimal)
        {
            decimal result = _decimal;

            result = 1.0m / result;

            return result;
        }

        public static decimal Clamp01(this decimal _decimal) { return (_decimal < 0m) ? 0m : ((_decimal > 1m) ? 1m : _decimal); }
    }

    public static class _2DCoordExtension
    {
        public static int ToIndex(this _2DCoord _coord, int _horizontalLength) { return _horizontalLength * _coord.Y + _coord.X; }
    }

    public static class ArrayExtension
    {
        /// <summary>
        /// Returns empty Array if T is a non-IDeepCopyable class or a struct
        /// </summary>
        public static T[] DeepCopy<T>(this T[] _array)
        {
            if (_array == null)
                return null;

            T[] copy = new T[0];

            object[] objectCopy = new object[0];

            if (_array.Length != 0)
            {
                copy = new T[_array.Length];

                Type type = _array[0].GetType();

                if (type.IsArray)
                {
                    for (int i = 0; i < _array.Length; i++) { objectCopy[i] = (_array[i].ToArray<object>()).DeepCopy(); }
                }
                else if (type.IsList())
                {
                    for (int i = 0; i < _array.Length; i++) { objectCopy[i] = (_array[i].ToList<object>()).DeepCopy(); }
                }
                else if (type.IsDictionary())
                {
                    for (int i = 0; i < _array.Length; i++) { objectCopy[i] = _array[i].ToDictionary<object, object>().DeepCopy(); ; }
                }
                else if (type == typeof(string))
                {
                    for (int i = 0; i < _array.Length; i++) { objectCopy[i] = string.Copy(_array[i] as string); }
                }
                else if (type.IsIDeepCopyable())
                {
                    for (int i = 0; i < _array.Length; i++) { objectCopy[i] = _array[i].DeepCopy(); }
                }
                else if (type.IsValueType) //It actually is a shallow copy
                {
                    copy = (T[])_array.Clone(); //Set value directly to copy variable, instead of copy variable
                }
            }

            if (copy.Count() > 0)
                copy = copy.ToArray<T>();

            return copy;
        }
    }

    public struct KeySelectorAndSortType<T>
    {
        public KeySelectorAndSortType(Func<T, object> _keySelector, eSortType _sortType)
        {
            KeySelector = _keySelector;
            SortType = _sortType;
        }

        public Func<T, object> KeySelector;
        public eSortType SortType;
    }

    public static class ListExtension
    {
        public static List<T> Except<T>(this List<T> _list, T _object) { return (_list == null || _list.Count < 1) ? _list : _list.Where(x => !x.Equals(_object)).ToList(); }

        public static List<T> OrderByMultipleConditions<T>(this List<T> _list, List<KeySelectorAndSortType<T>> _conditions)
        {
            if (_list == null || _list.Count < 1 
                || _conditions == null || _conditions.Count < 1)
            {
                return _list;
            }

            /*
             * listsPerKeyValue will store the sublists of _list that will be created. 
             * Each sublist will contain the same value for the given key each time a sorting process is applied. 
             * The sublists will be subdivided again and again until all sorting conditions have been applied. 
             * The eventual sublists will be merged altogether, in the order, to form a single list with sorted values.
             */
            List<List<T>> listsPerKeyValue = new List<List<T>> { new List<T>(_list) };

            for (int i = 0; i < _conditions.Count; i++)
            {
                List<List<T>> lists = new List<List<T>>();
                foreach (List<T> subList in listsPerKeyValue)
                {
                    IEnumerable<T> orderedList = subList.OrderByCondition(_conditions[i]);
                    lists.AddRange(orderedList.SubdivideBy(_conditions[i].KeySelector));
                }

                listsPerKeyValue = lists; // Update listsPerKeyValue
            }

            List<T> result = new List<T>();
            foreach (List<T> subList in listsPerKeyValue) // Merge the sub-lists
            {
                result.AddRange(subList);
            }

            return result;
        }

        public static void RemoveLast<T>(this List<T> _list) { if (_list != null && _list.Count > 0) { _list.RemoveAt(_list.Count - 1); } }

        public static List<List<T>> GetAllCombinations<T>(this List<T> _list)
        {
            List<List<T>> result = new List<List<T>>();

            for (int i = 1; i <= _list.Count; i++)
            {
                result.AddRange(_list.GetCombinations(i));
            }

            return result;
        }
        public static List<List<T>> GetCombinations<T>(this List<T> _list, int _minNumOfElementsToGet, int _maxNumOfElementsToGet)
        {
            List<List<T>> result = new List<List<T>>();

            if (_minNumOfElementsToGet < 1 || _maxNumOfElementsToGet > _list.Count)
                return new List<List<T>>();

            for (int i = _minNumOfElementsToGet; i <= _maxNumOfElementsToGet; i++)
            {
                result.AddRange(_list.GetCombinations(i));
            }

            return result;
        }
        private static List<List<T>> GetCombinations<T>(this List<T> _list, int _numOfElementsToGet)
        {
            if (_numOfElementsToGet < 1 || _numOfElementsToGet > _list.Count)
                return new List<List<T>>();

            List<List<T>> result = new List<List<T>>();
            if (_numOfElementsToGet == 1)
            {
                foreach (T item in _list)
                {
                    result.Add(new List<T>() { item });
                }
            }
            else
            {
                for (int i = 0; i < _list.Count - _numOfElementsToGet + 1; i++)
                {
                    T firstElement = _list[i];
                    List<T> restOfElements = new List<T>(_list);
                    restOfElements.RemoveRange(0, i + 1);

                    List<List<T>> innerCommbinations = restOfElements.GetCombinations(_numOfElementsToGet - 1);
                    foreach (List<T> innerCombination in innerCommbinations)
                    {
                        innerCombination.Insert(0, firstElement);
                        result.Add(innerCombination);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a shallow copy if T is a non-IDeepCopyable class or a struct
        /// </summary>
        public static List<T> DeepCopy<T>(this List<T> _list)
        {
            if (_list == null)
                return null;

            List<object> copy = new List<object>();

            if (_list.Count != 0)
            {
                Type type = _list[0].GetType();

                if (type.IsList())
                    _list.ForEach(x => copy.Add((x.ToList<object>()).DeepCopy()));
                else if (type.IsArray)
                    _list.ForEach(x => copy.Add((x.ToArray<object>()).DeepCopy()));
                else if (type.IsDictionary())
                    _list.ForEach(x => copy.Add(x.ToDictionary<object, object>().DeepCopy()));
                else if (type == typeof(string))
                    _list.ForEach(x => copy.Add(string.Copy((x as string))));
                else if (type.IsIDeepCopyable())
                    _list.ForEach(x => copy.Add(x.DeepCopy()));
                else if (type.IsValueType && !type.Is_struct())
                    _list.ForEach(x => copy.Add(x)); //It actually is a shallow copy
            }

            return copy.ToList<T>();
        }
    }

    public static class IEnumerableExtension
    {
        /// <summary>
        /// Please use OrderBy() or OrderByDescending() if you know the direction to which the _enumerable must be sorted.<para/>
        /// This method has been implemented to increase the readability of some other extension methods.
        /// </summary>
        public static IEnumerable<T> OrderByCondition<T>(this IEnumerable<T> _enumerable, KeySelectorAndSortType<T> _condition)
        {
            if (_enumerable == null || _condition.KeySelector == null)
                return _enumerable;

            if (_condition.SortType == eSortType.Ascending)
                return _enumerable.OrderBy(_condition.KeySelector);
            else
                return _enumerable.OrderByDescending(_condition.KeySelector);
        }

        public static List<List<T>> SubdivideBy<T>(this IEnumerable<T> _enumerable, Func<T, object> _keySelector)
        {
            if (_enumerable == null)
                return null;

            IEnumerable<T> enumerable = new List<T>(_enumerable);

            List<List<T>> listsPerKeyValue = new List<List<T>>();
            if (_keySelector == null)
            {
                listsPerKeyValue.Add(_enumerable.ToList());
                return listsPerKeyValue;
            }

            bool firstLoop = true;

            object previousKeyValue = _keySelector(enumerable.FirstOrDefault()); //Initialize with the key value for the first item in the list
            foreach (T item in enumerable)
            {
                object currentKeyValue = _keySelector(item);

                if (firstLoop || !currentKeyValue.Equals(previousKeyValue))
                {
                    listsPerKeyValue.Add(new List<T>());

                    if (firstLoop)
                        firstLoop = false;
                }

                listsPerKeyValue.Last().Add(item);

                previousKeyValue = currentKeyValue;
            }

            return listsPerKeyValue;
        }
    }

    public static class IListExtension
    {
        public static bool ContainsAny<T>(this IList<T> _list, IList<T> _targetList)
        {
            foreach (T item in _targetList)
            {
                if (_list.Contains(item))
                    return true;
            }

            return false;
        }

        public static bool ContainsAll<T>(this IList<T> _list, IList<T> _targetList)
        {
            foreach (T item in _targetList)
            {
                if (!_list.Contains(item))
                    return false;
            }

            return true;
        }

        public static bool ContainsOnly<T>(this IList<T> _list, T _value) { return !_list.Any(x => !x.Equals(_value)); }
    }

    public static class DictionaryExtension
    {
        public static ReadOnlyDictionary<K, V> AsReadOnly<K, V>(this Dictionary<K, V> _dictionary) { return new ReadOnlyDictionary<K, V>(_dictionary); }

        public static V GetValueOrElse<K, V>(this Dictionary<K, V> _dictionary, K _key, V _value)
        {
            if (_dictionary == null)
                return _value;

            return _dictionary.ContainsKey(_key) ? _dictionary[_key] : _value;
        }

        public static K GetFirstKey<K, V>(this Dictionary<K, V> _dictionary, V _value) { return _dictionary.First(x => x.Value.Equals(_value)).Key; }
        
        public static K GetFirstKeyOrDefault<K, V>(this Dictionary<K, V> _dictionary, V _value)
        {
            if (_dictionary == null)
                return default;

            return _dictionary.FirstOrDefault(x => x.Value.Equals(_value)).Key;
        }

        public static List<K> GetKeysWithValue<K, V>(this Dictionary<K, V> _dictionary, V _value)
        {
            if (_dictionary == null)
                return null;

            List<K> keys = new List<K>();
            foreach (var entry in _dictionary)
            {
                if (entry.Value.Equals(_value))
                    keys.Add(entry.Key);
            }

            return keys;
        }

        public static void SumAll<K>(this Dictionary<K, int> _dictionary, Dictionary<K, int> _target)
        {
            foreach (var entry in _target)
            {
                _dictionary.Sum(entry.Key, entry.Value);
            }
        }
        public static void Sum<K>(this Dictionary<K, int> _dictionary, K _key, int _value)
        {
            if (_dictionary == null)
                return;

            if (_dictionary.ContainsKey(_key))
                _dictionary.Add(_key, _value);
            else
                _dictionary[_key] += _value;
        }

        public static void SubtractAll<K>(this Dictionary<K, int> _dictionary, Dictionary<K, int> _target)
        {
            foreach (var entry in _target)
            {
                _dictionary.Subtract(entry.Key, entry.Value);
            }
        }
        public static void Subtract<K>(this Dictionary<K, int> _dictionary, K _key, int _value) { _dictionary.Sum(_key, -1 *_value); }

        /// <summary>
        /// Returns empty Dictionary if K or V is a non-IDeepCopyable class or a struct
        /// </summary>
        public static Dictionary<K, V> DeepCopy<K, V>(this Dictionary<K, V> _dictionary)
        {
            if (_dictionary == null)
                return null;

            Dictionary<object, object> copy = new Dictionary<object, object>();

            if (_dictionary.Count != 0)
            {
                // In case K and V are object, K and V might not be the actual types.
                // Therefore, get directly the types from _dictionary's key and value
                Type tType = _dictionary.Keys.First().GetType();
                Type uType = _dictionary.Values.First().GetType();

                foreach (var entry in _dictionary)
                {
                    object key = null;
                    object value = null;

                    if (tType.IsDictionary())
                        key = entry.Key.ToDictionary<object, object>().DeepCopy();
                    else if (tType.IsArray)
                        key = (entry.Key.ToArray<object>()).DeepCopy();
                    else if (tType.IsList())
                        key = (entry.Key.ToList<object>()).DeepCopy();
                    else if (tType == typeof(string))
                        key = String.Copy(entry.Key as string);
                    else if (tType.IsIDeepCopyable())
                        key = entry.Key.DeepCopy();
                    else if (tType.IsValueType && !tType.Is_struct())
                        key = entry.Key; //It actually is a shallow copy

                    if (uType.IsDictionary())
                        value = entry.Value.ToDictionary<object, object>().DeepCopy();
                    else if (uType.IsArray)
                        value = (entry.Value.ToArray<object>()).DeepCopy();
                    else if (uType.IsList())
                        value = (entry.Value.ToList<object>()).DeepCopy();
                    else if (uType == typeof(string))
                        value = String.Copy(entry.Value as string);
                    else if (uType.IsIDeepCopyable())
                        value = entry.Value.DeepCopy();
                    else if (uType.IsValueType && !uType.Is_struct())
                        value = entry.Value; //It actually is a shallow copy

                    copy.Add(key, value);
                }
            }

            return copy.ToDictionary(x => x.Key.ToT<K>(), x => x.Value.ToT<V>());
        }
    }

    public static class ReadonlyDictionaryExtension
    {
        public static K GetFirstKey<K, V>(this ReadOnlyDictionary<K, V> _dictionary, V _value) { return _dictionary.First(x => x.Value.Equals(_value)).Key; }

        public static K GetFirstKeyOrDefault<K, V>(this ReadOnlyDictionary<K, V> _dictionary, V _value)
        {
            if (_dictionary == null)
                return default;

            return _dictionary.FirstOrDefault(x => x.Value.Equals(_value)).Key;
        }

        public static List<K> GetKeysWithValue<K, V>(this ReadOnlyDictionary<K, V> _dictionary, V _value)
        {
            if (_dictionary == null)
                return null;

            List<K> keys = new List<K>();
            foreach (var entry in _dictionary)
            {
                if (entry.Value.Equals(_value))
                    keys.Add(entry.Key);
            }

            return keys;
        }
    }

    public static class NullPreventionAssignmentMethods
    {
        public static string CoalesceNullAndReturnCopyOptionally(this string _this, bool _returnCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnCopyInstead)
                    return string.Copy(_this);
                else
                    return _this;
            }
            else
                return string.Empty;
        }

        public static _2DCoord CoalesceNullAndReturnCopyOptionally(this _2DCoord _this, bool _returnCopyInstead = false)
        {
            if (_this != null)
            {
                if (_returnCopyInstead)
                    return _this.DeepCopy();
                else
                    return _this;
            }
            else
                return new _2DCoord(default, default);
        }

        public static T[] CoalesceNullAndReturnCopyOptionally<T>(this T[] _this, eCopyType _copyType = eCopyType.None)
        {
            if (_this != null)
            {
                switch (_copyType)
                {
                    case eCopyType.Shallow:
                        return (T[])_this.Clone();
                    case eCopyType.Deep:
                        return _this.DeepCopy();
                    default:
                        return _this;
                }
            }
            else
                return new T[0];
        }

        public static List<T> CoalesceNullAndReturnCopyOptionally<T>(this List<T> _this, eCopyType _copyType = eCopyType.None)
        {
            if (_this != null)
            {
                switch (_copyType)
                {
                    case eCopyType.Shallow:
                        return new List<T>(_this);
                    case eCopyType.Deep:
                        return _this.DeepCopy();
                    default:
                        return _this;
                }
            }
            else
                return new List<T>();
        }

        public static Dictionary<K, V> CoalesceNullAndReturnCopyOptionally<K, V>(this Dictionary<K, V> _this, eCopyType _copyType = eCopyType.None)
        {
            if (_this != null)
            {
                switch (_copyType)
                {
                    case eCopyType.Shallow:
                        return new Dictionary<K, V>(_this);
                    case eCopyType.Deep:
                        return _this.DeepCopy();
                    default:
                        return _this;
                }
            }
            else
                return new Dictionary<K, V>();
        }
    }

    public enum eCopyType
    {
        None,
        Shallow,
        Deep
    }
}