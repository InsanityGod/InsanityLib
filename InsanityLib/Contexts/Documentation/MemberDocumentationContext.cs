using InsanityLib.Enums;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InsanityLib.Contexts.Documentation
{
    public class MemberDocumentationContext
    {
        public AssemblyDocumentationContext AssemblyDocumentationContext { get; init; }
        public MemberInfo Member { get; init; }
        public XmlNode MemberNode { get; internal set; }
        public bool HasXmlDocumentation => MemberNode != null;

        public string GetDescription()
        {
            if (HasXmlDocumentation)
            {
                var summaryNode = MemberNode.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    var summaryStr = summaryNode.InnerText.Trim(Naming.TrimCharacters);
                    if(!string.IsNullOrEmpty(summaryStr)) return summaryStr;
                }
            }

            var attr = Member.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description.Trim(Naming.TrimCharacters) ?? string.Empty;
        }

        public string[] GetExamples()
        {
            if (HasXmlDocumentation)
            {
                var exampleNodes = MemberNode.SelectNodes("example");
                if (exampleNodes != null)
                {
                    var exampleStrings = new List<string>();

                    foreach (XmlNode exampleNode in exampleNodes)
                    {
                        var exampleStr = exampleNode.InnerText.Trim(Naming.TrimCharacters);
                        if (!string.IsNullOrEmpty(exampleStr)) exampleStrings.Add(exampleStr);
                    }

                    return exampleStrings.ToArray();
                }
            }

            return Array.Empty<string>();
        }

        public string GetReturn()
        {
            if (HasXmlDocumentation)
            {
                var returnNode = MemberNode.SelectSingleNode("returns");
                if (returnNode != null)
                {
                    var returnStr = returnNode.InnerText.Trim(Naming.TrimCharacters);
                    if(!string.IsNullOrEmpty(returnStr)) return returnStr;
                }
            }
            return string.Empty;
        }

        public string GetExtendedDescription()
        {
            if(Member is not FieldInfo && Member is not PropertyInfo) return GetDescription(); //TODO extended description for other memers
            
            var description = new StringBuilder(GetDescription());
            description.AppendLine();

            var defaultAttr = Member.GetCustomAttribute<DefaultValueAttribute>();
            if(defaultAttr != null) description.AppendLine($"Default: {defaultAttr.Value}");

            var validatorAttributes = Member.GetCustomAttributes<ValidationAttribute>().ToArray();

            var primaryTye = Member.GetPrimaryType();
            if (primaryTye.IsEnum)
            {
                var isEnumFlag = primaryTye.GetCustomAttribute<FlagsAttribute>() != null;
                if (isEnumFlag) description.Append("Valid Values (Combination): ");
                else description.Append("Valid Values: ");
                var parser = new EnumNameValueMapping(primaryTye);
                description.AppendLine(parser.GetDescriptionStrings());
                if(validatorAttributes.Length > 0) description.AppendLine();
            }

            foreach(var attr in validatorAttributes)
            {
                //TODO Sort these so ordering is consistent
                switch (attr)
                {
                    case RequiredAttribute requiredAttr:
                        description.AppendLine($"Required: {requiredAttr.ErrorMessage}");
                        break;

                    case RangeAttribute rangeAttr:
                        description.AppendLine($"Range: {rangeAttr.Minimum} ~ {rangeAttr.Maximum}");
                        break;

                    case StringLengthAttribute stringLengthAttr:
                        description.AppendLine($"String Length: {stringLengthAttr.MinimumLength} ~ {stringLengthAttr.MaximumLength}");
                        break;

                    case MinLengthAttribute minLengthAttr:
                        description.AppendLine($"Min Length: {minLengthAttr.Length}");
                        break;

                    case MaxLengthAttribute maxLengthAttr:
                        description.AppendLine($"Max Length: {maxLengthAttr.Length}");
                        break;

                    case RegularExpressionAttribute regexAttr:
                        description.AppendLine($"Regex: {regexAttr.Pattern}");
                        break;

                    case CompareAttribute compareAttr:
                        description.AppendLine($"Compare: {compareAttr.OtherProperty} ({compareAttr.ErrorMessage})");
                        break;
                }

                //TODO interface for custom attribute messages
            }

            return description.ToString().Trim(Naming.TrimCharacters);
        }
    }
}
