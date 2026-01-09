using InsanityLib.Extended.Enums;
using InsanityLib.Util;
using InsanityLib.Util.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Vintagestory.API.Config;

namespace InsanityLib.Documentation;

public class MemberDocumentationContext(AssemblyDocumentationContext assemblyDocumentationContext, MemberInfo member) : IInitialize
{
    public readonly AssemblyDocumentationContext AssemblyDocumentationContext = assemblyDocumentationContext;
    
    public readonly MemberInfo Member = member;

    public XmlNode MemberNode { get; internal set; }

    public bool HasXmlDocumentation => MemberNode is not null;

    public void Initialize()
    {
        if (!AssemblyDocumentationContext.HasXmlDocumentation) return;

        var memberName = Member.GetDocumentationMemberName();
        if (!string.IsNullOrEmpty(memberName))
        {
            MemberNode = AssemblyDocumentationContext.Document.SelectSingleNode($"/doc/members/member[@name='{memberName}']");
        }
    }

    public bool TryGetFromLang(EDocumentationType type, out string result)
    {
        var languageString = Member.GetLangKey(type);
        var descriptionFromLang = Lang.Get(languageString);
        if(descriptionFromLang != languageString && !string.IsNullOrWhiteSpace(descriptionFromLang))
        {
            result = descriptionFromLang.CleanWhiteSpaces();
            return true;
        }

        result = string.Empty;
        return false;
    }

    public string GetDisplayName() => Member.GetHumanReadableName();

    public string GetDescription()
    {
        if(TryGetFromLang(EDocumentationType.Description, out var descriptionFromLang)) return descriptionFromLang;

        if (HasXmlDocumentation && MemberNode.SelectSingleNode("summary") is XmlNode summaryNode)
        {
            var descriptionFromXml = summaryNode.InnerText;
            if(!string.IsNullOrWhiteSpace(descriptionFromXml)) return descriptionFromXml.CleanWhiteSpaces();
        }
        
        var descriptionFromAttr = Member.GetCustomAttribute<DescriptionAttribute>()?.Description;
        if(!string.IsNullOrWhiteSpace(descriptionFromAttr)) return descriptionFromAttr.CleanWhiteSpaces();

        return string.Empty;
    }

    public string[] GetExamples()
    {
        if(TryGetFromLang(EDocumentationType.Example, out var descriptionFromLang)) return descriptionFromLang.Split("\n");

        if (!HasXmlDocumentation || MemberNode.SelectNodes("example") is not XmlNodeList exampleNodes) return [];

        var exampleStrings = new List<string>();

        foreach (XmlNode exampleNode in exampleNodes)
        {
            var exampleStr = exampleNode.InnerText.CleanWhiteSpaces();
            if (!string.IsNullOrWhiteSpace(exampleStr)) exampleStrings.Add(exampleStr);
        }

        return [.. exampleStrings];
    }

    public string GetReturn()
    {
        if(TryGetFromLang(EDocumentationType.Returns, out var descriptionFromLang)) return descriptionFromLang;

        if (HasXmlDocumentation && MemberNode.SelectSingleNode("returns") is XmlNode returnNode)
        {
            var returnStr = returnNode.InnerText.CleanWhiteSpaces();
            if(!string.IsNullOrEmpty(returnStr)) return returnStr;
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
            if (isEnumFlag) description.Append("Valid Values (Combination):");
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
                    description.AppendLine( $"Compare: {compareAttr.OtherProperty} ({compareAttr.ErrorMessage})");
                    break;
            }

            //TODO interface for custom attribute messages
        }

        return description.ToString().CleanWhiteSpaces();
    }
}
