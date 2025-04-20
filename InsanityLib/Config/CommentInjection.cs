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

namespace InsanityLib.Config
{
    public class JsonWithCommentsConverter : JsonConverter
    {
        public static DefaultContractResolver DefaultResolver { get; } = new DefaultContractResolver();
        public override bool CanConvert(Type objectType) => 
            objectType.IsComplexClassType()
            && !typeof(Array).IsAssignableFrom(objectType) 
            && !typeof(IEnumerable).IsAssignableFrom(objectType)
            && DefaultResolver.ResolveContract(objectType).GetType() != typeof(JsonStringContract);

        //ATTEMPT 1
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
                //var t = new DefaultContractResolver();
                //var f = t.ResolveContract(member.GetPrimaryType());
                WritePropertyNameWithDescription(writer, member);
                //var contract = serializer.ContractResolver?.ResolveContract(member.GetPrimaryType());
                var propValue = member.GetValue(value);
                serializer.Serialize(writer, propValue);
            }

            writer.WriteEndObject();
        }

        private void WritePropertyNameWithDescription(JsonWriter writer, MemberInfo member)
        {
            //TODO
            var writerTraverse = Traverse.Create(writer);
            var currentPosition = writerTraverse.Field("_currentPosition");
            var currentState = writerTraverse.Field("_currentState");
            var currentPositionObj = writerTraverse.Field("_currentPosition").GetValue();
            currentPositionObj.GetType().GetField("PropertyName", AccessTools.all).SetValue(currentPositionObj, member.Name);
            
            var tokenBeingWritten = JsonToken.PropertyName;

            var stateArray = writerTraverse.Field("StateArray");
            var newState = ((Array)stateArray.GetValue<Array>().GetValue((int)tokenBeingWritten)).GetValue((int)currentState.GetValue());

            if ((int)newState == 9) throw new JsonWriterException($"Token {tokenBeingWritten} in state {currentState.GetValue()} would result in an invalid JSON object.");

            var currentStateInt = (int)currentState.GetValue();
            if ((currentStateInt == 3 || currentStateInt == 5 || currentStateInt == 7))
            {
                writerTraverse.Method("WriteValueDelimiter").GetValue();
            }

            //internal enum State
            //{
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
            //}

            var writeIndentSpace = writerTraverse.Method("WriteIndentSpace");
            var writeIndent = writerTraverse.Method("WriteIndent");

            var doc = member.GetDocumentationContext();
            var description = doc.GetExtendedDescription();
            if(currentStateInt != 2 && !string.IsNullOrEmpty(description)) writer.WriteWhitespace(Environment.NewLine);

            //if (currentStateInt == 1)
            //{
            //    writeIndentSpace.GetValue();
            //}
            
            //if(currentStateInt != 0)
            //{
                writeIndent.GetValue();
            //}
    
            if (!string.IsNullOrEmpty(description))
            {
                //var lines = description.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                //for (int i = 0; i < lines.Length; i++)
                //{
                //    writer.WriteRaw($"// {lines[i].Trim()}");
                //    //if(i != lines.Length - 2)
                //    //writer.WriteWhitespace(Environment.NewLine);
                //}

                foreach (var line in description.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
                {
                    writer.WriteRaw($"// {line.Trim()}");
                    writeIndent.GetValue();
                }
                //writer.WriteComment(description);
                //writer.WriteWhitespace(Environment.NewLine);
                //writeIndent.GetValue();
            }

            // don't indent a property when it is the first token to be written (i.e. at the start)
            //if ((currentStateInt == 5 || currentStateInt == 4 || currentStateInt == 7 || currentStateInt == 6)
            //    || currentStateInt != 0) //(&& tokenBeingWritten == JsonToken.PropertyName)
            //{
            //    writeIndent.GetValue();
            //}
            
            currentState.SetValue(newState.Cast(currentState.GetValueType()));
            //
            //
            //writer.WritePropertyName(member.Name);

            //_currentPosition.PropertyName = name;
            //AutoComplete(JsonToken.PropertyName);


            writerTraverse.Method("WriteEscapedString", member.Name, true).GetValue();
            //WriteEscapedString(name, _quoteName);

            writer.WriteRaw(":");
            //_writer.Write(':');
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException("Reading JSON with comments is not supported.");
    }
}
