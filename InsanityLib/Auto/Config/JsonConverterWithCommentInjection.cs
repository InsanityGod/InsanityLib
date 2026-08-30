using System;
using System.Linq;
using InsanityLib.Documentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace InsanityLib.Auto.Config;

public sealed class JsonConverterWithCommentInjection(IContractResolver resolver) : JsonConverter
{
    public override bool CanConvert(Type objectType) => resolver.ResolveContract(objectType) is JsonObjectContract;

    public override void WriteJson(JsonWriter writer,object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        if (writer is not JsonTextWriter textWriter) throw new JsonSerializationException($"{nameof(JsonConverterWithCommentInjection)} requires a {nameof(JsonTextWriter)}.");

        JsonObjectContract contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

        writer.WriteStartObject();

        foreach (var property in contract.Properties)
        {
            if (property.Ignored || !property.Readable) continue;

            WritePropertyNameWithDescription(textWriter, property);

            var propertyValue = property.ValueProvider?.GetValue(value);

            serializer.Serialize(writer, propertyValue);
        }

        writer.WriteEndObject();
    }

    private static void WritePropertyNameWithDescription(JsonTextWriter writer, JsonProperty property)
    {
        var member = property.DeclaringType?
            .GetMember(property.UnderlyingName ?? property.PropertyName!)
            .FirstOrDefault();

        var description = member?
            .GetDocumentationContext()?
            .GetExtendedDescription();

        writer.InternalWritePropertyName(property.PropertyName!);

        if (!string.IsNullOrEmpty(description))
        {
            foreach (var line in description.Split(["\r\n", "\n", "\r"], StringSplitOptions.None))
            {
                writer.WriteIndent();
                writer.WriteRaw($"// ");
                writer.WriteRaw(line.Trim());
            }
        }
        writer.WriteIndent();

        writer.WriteEscapedString(property.PropertyName!, writer.QuoteName);

        writer.WriteRaw(":");
    }

    public override bool CanRead => false;

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotSupportedException("Reading JSON with comments is not supported.");
}
