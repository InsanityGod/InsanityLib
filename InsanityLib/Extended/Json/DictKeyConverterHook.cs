using InsanityLib.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace InsanityLib.Extended.Json;

public sealed class DictKeyConverterHook : JsonConverter
{
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => typeof(IDictionary).IsAssignableFrom(objectType) && objectType.FindGenericInterfaceDefinition(typeof(IDictionary<,>)) is { } interace && interace.GetGenericArguments()[0] != typeof(string);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var dictType = objectType.FindGenericInterfaceDefinition(typeof(IDictionary<,>))!.GetGenericArguments();
        var keyType = dictType[0];
        var valueType = dictType[1];

        var dict = (IDictionary)Activator.CreateInstance(objectType)!;

        var jo = JObject.Load(reader);

        foreach (var prop in jo.Properties())
        {
            dict.Add(
                JValue.CreateString(prop.Name).ToObject(keyType, serializer)!,
                prop.Value.ToObject(valueType, serializer)
            );
        }

        return dict;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
