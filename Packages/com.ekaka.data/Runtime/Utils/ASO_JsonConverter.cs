using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.Utils
{
    public class ASO_JsonConverter<T> : JsonConverter where T : AddressableScriptableObject
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            T addressableScriptableObject = (T) value;

            //serialize Asset Guid instead of the scriptable object
            serializer.Serialize(writer, addressableScriptableObject.AssetGuid);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            //use addressable to load the scriptable object from AssetGuid
            return Addressables.LoadAssetAsync<T>(reader.Value).WaitForCompletion();
        }
    }
}
