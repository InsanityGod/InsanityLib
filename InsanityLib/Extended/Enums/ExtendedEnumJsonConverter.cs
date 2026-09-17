using InsanityLib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

        if (reader.TokenType == JsonToken.StartObject && enumType.IsDefined(typeof(FlagsAttribute), inherit: false))
        {
            var existingValueAsNumber = existingValue?.AutoConvert<int>() ?? 0; //TODO maybe see about using ulong instead in general

            if (serializer.Deserialize<Dictionary<string, bool>>(reader) is { } flags)
            {
                foreach (var (key, enabled) in flags)
                {
                    // Ignore unknown enum values.
                    if (ExtendedEnumExtensions.TryParse(enumType, key) is not int flagValue) continue;

                    existingValueAsNumber = enabled
                        ? existingValueAsNumber | flagValue
                        : existingValueAsNumber & ~flagValue;
                }
            }

            return Enum.ToObject(enumType, existingValueAsNumber);
        }

        var stringValue = reader.Value!.ToString()!;

        var extendedResult = ExtendedEnumExtensions.TryParse(enumType, stringValue);
        if (extendedResult is not null) return extendedResult;

        return Enum.Parse(enumType, stringValue, ignoreCase: true);
    }
}