using InsanityLib.Util.Versioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProperVersion;
using System;

namespace InsanityLib.Extended.Json;

public sealed class VersionedConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Versioned<>);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not IVersionedWrapper wrapper)
        {
            writer.WriteNull();
            return;
        }

        var content = JObject.FromObject(wrapper.ContentAsObj, serializer);

        if (content.ContainsKey("Version")) throw new JsonSerializationException("Versioned content cannot contain a 'Version' property.");

        content.AddFirst(new JProperty("Version", JToken.FromObject(wrapper.Version, serializer)));
        content.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);

        var version = obj["Version"]?.ToObject<SemVer>(serializer);
        obj.Remove("Version");

        var contentType = objectType.GetGenericArguments()[0];
        var content = obj.ToObject(contentType, serializer) ?? throw new JsonSerializationException($"Unable to deserialize content as {contentType}.");

        var result = (IVersionedWrapper)Activator.CreateInstance(objectType)!;

        if (version is not null) result.Version = version;

        objectType.GetProperty(nameof(Versioned<>.Content))!.SetValue(result, content);

        return result;
    }

    public override bool CanWrite => true;
    public override bool CanRead => true;
}