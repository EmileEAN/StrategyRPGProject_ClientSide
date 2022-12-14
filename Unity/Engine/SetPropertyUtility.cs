using System;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine
{
    internal static class SetPropertyUtility
    {
        public static bool SetColor(ref Color _currentValue, Color _newValue)
        {
            if (_currentValue.r == _newValue.r && _currentValue.g == _newValue.g && _currentValue.b == _newValue.b && _currentValue.a == _newValue.a)
                return false;

            _currentValue = _newValue;
            return true;
        }

        public static bool SetEquatableStruct<T>(ref T _currentValue, T _newValue) where T : IEquatable<T>
        {
            if (_currentValue.Equals(_newValue))
                return false;

            _currentValue = _newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T _currentValue, T _newValue) where T : struct
        {
            if (_currentValue.Equals(_newValue))
                return false;

            _currentValue = _newValue;
            return true;
        }

        public static bool SetClass<T>(ref T _currentValue, T _newValue) where T : class
        {
            if ((_currentValue == null && _newValue == null) || (_currentValue != null && _currentValue.Equals(_newValue)))
                return false;

            _currentValue = _newValue;
            return true;
        }
    }
}
