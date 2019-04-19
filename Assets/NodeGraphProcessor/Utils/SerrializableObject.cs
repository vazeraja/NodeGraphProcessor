using System;
using UnityEngine;
using System.Globalization;

namespace GraphProcessor
{
    // Warning: this class only support the serialization of UnityObject and primitive
    [System.Serializable]
    public class SerializableObject : ISerializationCallbackReceiver
    {
        [System.Serializable]
        class ObjectWrapper
        {
            public UnityEngine.Object value;
        }

        public string serializedType;
        public string serializedValue;

        public object value;

        public SerializableObject(object value)
        {
            this.value = value;
        }

        public void OnAfterDeserialize()
        {
            if (String.IsNullOrEmpty(serializedType))
            {
                Debug.LogError("Can't deserialize the object from null type");
                return;
            }

            Type type = Type.GetType(serializedType);

            if (type.IsPrimitive)
            {
                if (string.IsNullOrEmpty(serializedValue))
                    value = Activator.CreateInstance(type);
                else
                    value = Convert.ChangeType(serializedValue, type, CultureInfo.InvariantCulture);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                ObjectWrapper obj = new ObjectWrapper();
                JsonUtility.FromJsonOverwrite(serializedValue, obj);
                value = obj.value;
            }
            else if (type == typeof(string))
                value = serializedValue.Substring(1, serializedValue.Length - 2).Replace("\\\"", "\"");
            else
            {
                try {
                    value = Activator.CreateInstance(type);
                    JsonUtility.FromJsonOverwrite(serializedValue, value);
                } catch {
                    Debug.LogError("Can't serialize type " + serializedType);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            if (value == null)
                return ;

            serializedType = value.GetType().AssemblyQualifiedName;
            Debug.Log("Serialized type: " + serializedType);

            if (value.GetType().IsPrimitive)
                serializedValue = value.ToString();
            else if (value is UnityEngine.Object) //type is a unity object
            {
                if ((value as UnityEngine.Object) == null)
                    return ;

                ObjectWrapper wrapper = new ObjectWrapper { value = value as UnityEngine.Object };
                serializedValue = JsonUtility.ToJson(wrapper);
            }
            else if (value is string)
                serializedValue = "\"" + ((string)value).Replace("\"", "\\\"") + "\"";
            else
            {
                try {
                    serializedValue = JsonUtility.ToJson(value);
                } catch {
                    Debug.LogError("Can't serialize type " + serializedType);
                }
            }
        }
    }
}