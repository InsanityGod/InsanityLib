using InsanityLib.Extensions;
using InsanityLib.Util;
using Newtonsoft.Json;
using System;

namespace InsanityLib.Extended.Enums;

public class ExtendedEnumJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => (Nullable.GetUnderlyingType(objectType) ?? objectType).IsEnum;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var enumType = value.GetType();
        enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;

        var intValue = value.AutoConvert<int>();
        var extendedValue = ExtendedEnumExtensions.TryToString(enumType, intValue);
        if(extendedValue is not null) writer.WriteValue(extendedValue);
        else writer.WriteValue(value.ToString());
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var enumType = Nullable.GetUnderlyingType(objectType) ?? objectType;

        if (reader.TokenType == JsonToken.Null) return null;

        var stringValue = reader.Value!.ToString()!;

        var extendedResult = ExtendedEnumExtensions.TryParse(enumType, stringValue);
        if(extendedResult is not null) return extendedResult;
        

        return Enum.Parse(enumType, stringValue, ignoreCase: true);
    }
}