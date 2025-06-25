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
        public bool HasXmlDocumentation => MemberNode is not null;

        public string GetDescription()
        {
            if (HasXmlDocumentation)
            {
                var summaryNode = MemberNode.SelectSingleNode("summary");
                if (summaryNode is not null)
                {
                    var summaryStr = summaryNode.InnerText.CleanWhiteSpaces();
                    if(!string.IsNullOrEmpty(summaryStr)) return summaryStr;
                }
            }

            var attr = Member.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description.CleanWhiteSpaces() ?? string.Empty;
        }

        public string[] GetExamples()
        {
            if (HasXmlDocumentation)
            {
                var exampleNodes = MemberNode.SelectNodes("example");
                if (exampleNodes is not null)
                {
                    var exampleStrings = new List<string>();

                    foreach (XmlNode exampleNode in exampleNodes)
                    {
                        var exampleStr = exampleNode.InnerText.CleanWhiteSpaces();
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
                if (returnNode is not null)
                {
                    var returnStr = returnNode.InnerText.CleanWhiteSpaces();
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
            if(defaultAttr is not null) description.AppendLine($"Default: {defaultAttr.Value}");

            var validatorAttributes = Member.GetCustomAttributes<ValidationAttribute>().ToArray();

            var primaryType = Member.GetPrimaryType();
            if (primaryType.IsEnum)
            {
                var isEnumFlag = primaryType.GetCustomAttribute<FlagsAttribute>() is not null;
                if (isEnumFlag) description.Append("Valid Values (Combination): ");
                else description.Append("Valid Values: ");
                var parser = new EnumNameValueMapping(primaryType);
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

            return description.ToString().CleanWhiteSpaces();
        }
    }
}
