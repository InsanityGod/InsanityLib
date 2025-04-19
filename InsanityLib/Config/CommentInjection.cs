using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using InsanityLib.Util;

namespace InsanityLib.Config
{
    public class JsonWithCommentsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType.IsComplexClassType();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            IEnumerable<MemberInfo> props = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            IEnumerable<MemberInfo> fields = value.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !f.IsStatic);

            foreach (var member in props.Union(fields))
            {
                var doc = member.GetDocumentationContext();
                var description = doc.GetDescription();

                if (!string.IsNullOrEmpty(description))
                {
                    writer.WriteComment(description);
                }

                writer.WritePropertyName(member.Name);
                var propValue = member.GetValue(value);
                serializer.Serialize(writer, propValue);
            }

            writer.WriteEndObject();
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException("Reading JSON with comments is not supported.");
    }

    public class CommentInjectingContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            // Original properties
            var properties = base.CreateProperties(type, memberSerialization).ToList();
    
            var commentProps = new List<JsonProperty>();
    
            foreach (var prop in properties)
            {
                var member = type.GetMember(prop.UnderlyingName)[0];
                var description = member.GetCustomAttribute<DescriptionAttribute>();
    
                if (description != null)
                {
                    var commentProp = new JsonProperty
                    {
                        PropertyName = $"_{prop.PropertyName}_comment",
                        PropertyType = typeof(string),
                        Readable = true,
                        Writable = false,
                        ValueProvider = new StaticValueProvider(description.Description),
                        DeclaringType = type
                    };
    
                    commentProps.Add(commentProp);
                }
            }
    
            // Inject comment properties *before* real ones for better readability (optional)
            commentProps.AddRange(properties);
            return commentProps;
        }
    
        private class StaticValueProvider : IValueProvider
        {
            private readonly object value;
            public StaticValueProvider(object value) => this.value = value;
            public object GetValue(object target) => value;
            public void SetValue(object target, object value) { /* no-op */ }
        }
    }
}
