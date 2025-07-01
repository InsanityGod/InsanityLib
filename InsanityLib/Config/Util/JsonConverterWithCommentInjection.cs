using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using InsanityLib.Util;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using HarmonyLib;
using System.Globalization;
using System.Collections;
using Vintagestory.API.Common;
using Vintagestory.Common;
using ProtoBuf;

namespace InsanityLib.Config.Util
{
    public class JsonConverterWithCommentInjection : JsonConverter
    {
        public static DefaultContractResolver DefaultResolver { get; } = new DefaultContractResolver();
        public override bool CanConvert(Type objectType) =>
            objectType.IsComplexClassType()
            && !typeof(Array).IsAssignableFrom(objectType)
            && !typeof(IEnumerable).IsAssignableFrom(objectType)
            && DefaultResolver.ResolveContract(objectType).GetType() != typeof(JsonStringContract); //Ignore classes that would normally be saved as a string (like AssetLocations)

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            IEnumerable<MemberInfo> props = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            IEnumerable<MemberInfo> fields = value.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                ;//.Where(f => !f.IsStatic); //TODO test

            foreach (var member in props.Union(fields))
            {
                WritePropertyNameWithDescription(writer, member);
                var propValue = member.GetValue(value);
                serializer.Serialize(writer, propValue);
            }

            writer.WriteEndObject();
        }

        private static void WritePropertyNameWithDescription(JsonWriter writer, MemberInfo member)
        {
            var writerTraverse = Traverse.Create(writer);
            var currentState = writerTraverse.Field("_currentState");
            var currentPositionObj = writerTraverse.Field("_currentPosition").GetValue();
            currentPositionObj.GetType().GetField("PropertyName", AccessTools.all).SetValue(currentPositionObj, member.Name);

            var tokenBeingWritten = JsonToken.PropertyName;

            var stateArray = writerTraverse.Field("StateArray");
            var newState = ((Array)stateArray.GetValue<Array>().GetValue((int)tokenBeingWritten)).GetValue((int)currentState.GetValue());

            if ((int)newState == 9) throw new JsonWriterException($"Token {tokenBeingWritten} in state {currentState.GetValue()} would result in an invalid JSON object.");

            var currentStateInt = (int)currentState.GetValue();
            if (currentStateInt == 3 || currentStateInt == 5 || currentStateInt == 7)
            {
                writerTraverse.Method("WriteValueDelimiter").GetValue();
            }

            //    Start = 0,
            //    Property = 1,
            //    ObjectStart = 2,
            //    Object = 3,
            //    ArrayStart = 4,
            //    Array = 5,
            //    ConstructorStart = 6,
            //    Constructor = 7,
            //    Closed = 8,
            //    Error = 9

            var writeIndent = writerTraverse.Method("WriteIndent");

            var doc = member.GetDocumentationContext();
            var description = doc.GetExtendedDescription();
            if (currentStateInt != 2 && !string.IsNullOrEmpty(description)) writer.WriteWhitespace(Environment.NewLine);

            writeIndent.GetValue();

            if (!string.IsNullOrEmpty(description))
            {
                foreach (var line in description.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
                {
                    writer.WriteRaw($"// {line.Trim()}");
                    writeIndent.GetValue();
                }
            }

            currentState.SetValue(newState.Cast(currentState.GetValueType()));

            writerTraverse.Method("WriteEscapedString", member.Name, true).GetValue();

            writer.WriteRaw(":");
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException("Reading JSON with comments is not supported.");
    }
}
