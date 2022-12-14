using System;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine
{
    [Serializable]
    public class Vector3IntPair : SerializableKeyValuePair<Vector3, int>
    {
        public Vector3IntPair(Vector3 _key, int _value) : base(_key, _value) { }
    }

    [Serializable]
    public class Vector2FloatPair : SerializableKeyValuePair<Vector2, float>
    {
        public Vector2FloatPair(Vector2 _key, float _value) : base(_key, _value) { }
    }

    [Serializable]
    public class Vector3FloatPair : SerializableKeyValuePair<Vector3, float>
    {
        public Vector3FloatPair(Vector3 _key, float _value) : base(_key, _value) { }
    }

    [Serializable]
    public class SerializableKeyValuePair<K, V>
    {
        public K Key;
        public V Value;

        public SerializableKeyValuePair(K _key, V _value)
        {
            Key = _key;
            Value = _value;
        }
        public SerializableKeyValuePair(SerializableKeyValuePair<K, V> _keyValuePair)
        {
            Key = _keyValuePair.Key;
            Value = _keyValuePair.Value;
        }
    }
}
