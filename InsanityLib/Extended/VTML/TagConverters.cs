using HarmonyLib;
using InsanityLib.Documentation;
using InsanityLib.Extensions;
using InsanityLib.PathResolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Extended.VTML;

[HarmonyPatch]
public static class TagConverters
{
    // Sadly TagConverters just doesn't pass enough for handling childtokens

    [HarmonyTargetMethod]
    public static MethodBase TargetMethod() => AccessTools.Method(typeof(VtmlUtil), nameof(VtmlUtil.Richtextify), [typeof(ICoreClientAPI), typeof(VtmlToken), typeof(List<RichTextComponentBase>).MakeByRefType(), typeof(Stack<CairoFont>), typeof(Action<LinkTextComponent>)]);

    [HarmonyPrefix]
    public static void CheckState(ref List<RichTextComponentBase> elems, out int __state) => __state = elems.Count;

    [HarmonyPostfix]
    public static void AppendExtraComplexTags(ICoreClientAPI capi, VtmlToken token, ref List<RichTextComponentBase> elems, Stack<CairoFont> fontStack, Action<LinkTextComponent> didClickLink, int __state)
    {
        if(elems.Count != __state || token is not VtmlTagToken tagToken) return; // Token was already handled
        
        VtmlToken[] tokens;
        bool renderChildred = false;
        string? path = null;
        object? resolved = null;
        switch (tagToken.Name)
        {
            case "if":
                
                tagToken.Attributes?.TryGetValue("condition", out path);
                if (string.IsNullOrEmpty(path))
                {
                    capi.Logger.Warning("[InsanityLib] Language file includes an <if> tag without 'condition' attribute, content will always be visible");
                    renderChildred = true;
                    break;
                }
                
                if(!Resolver.TryResolve(path, capi, out resolved))
                {
                    capi.Logger.Warning("[InsanityLib] Language file includes an <if> tag with unresolved confition: {0}", path);
                    return;
                }
                renderChildred = resolved.IsTruthy();
                break;

            case "value":
                path = tagToken.ContentText.Trim();
                if (string.IsNullOrEmpty(path))
                {
                    capi.Logger.Warning("[InsanityLib] Language file includes a <value> tag without content");
                    return;
                }
                
                if(!Resolver.TryResolve(path, capi, out resolved))
                {
                    capi.Logger.Warning("[InsanityLib] Language file includes a <value> tag with unresolved content: {0}", path);
                    return;
                }
                else elems.Add(new RichTextComponent(capi, resolved?.ToString() ?? "null", fontStack.Peek()));
                break;

            case "lang":
                var args = new List<object?>();

                int argCount = tagToken.Attributes?.Count ?? 0;
                for(var i = 0; i < argCount; i++)
                {
                    if(tagToken.Attributes!.TryGetValue(i.ToString(), out var arg))
                    {
                        if(long.TryParse(arg, out var argAsLong))
                        {
                            args.Add(argAsLong);
                        }
                        else if(double.TryParse(arg, out var argAsDouble))
                        {
                            args.Add(argAsDouble);
                        }
                        else if(Resolver.TryResolve(arg, capi, out var argAsResolvedObj))
                        {
                            args.Add(argAsResolvedObj);
                        }
                        else args.Add(arg);
                    }
                    else break;
                }
                tokens = VtmlParser.Tokenize(capi.Logger, Lang.Get(tagToken.ContentText, [.. args]));

                VtmlUtil.Richtextify(capi, tokens, ref elems, fontStack, didClickLink);
                break;
            case "doc":
                string? typeName = null;
                string? memberName = null;
                MemberInfo? member = null;
                try
                {
                    if(tagToken.Attributes is null || !tagToken.Attributes.TryGetValue("type", out typeName) || AccessTools.TypeByName(typeName) is not Type type)
                    {
                        capi.Logger.Warning("[InsanityLib] Language file included a <doc> tag with unresolved type: {0}", typeName ?? "not specified");
                        return;
                    }

                    if(tagToken.Attributes.TryGetValue("member", out memberName))
                    {
                        var possibleMembers = type.GetMember(memberName);
                        if(possibleMembers.Length == 1) member = possibleMembers[0];
                        else if(possibleMembers.Length > 2)
                        {
                            member =possibleMembers.OrderByDescending(ReflectionExtensions.IsPublic).First();
                        }
                        else
                        {
                            capi.Logger.Warning("[InsanityLib] Language file included a <doc> tag with unresolved member on type '{0}': {1}", typeName, memberName);
                            return;
                        }
                    }
                    else member = type;

                    var doc = DocumentationUtil.GetDocumentationContext(member)!;
                    tokens = VtmlParser.Tokenize(capi.Logger, doc.GetDescription());
                    VtmlUtil.Richtextify(capi, tokens, ref elems, fontStack, didClickLink);
                }
                catch(Exception ex)
                {
                    capi.Logger.Error("[InsanityLib] An error occured trying to resolve a <doc> tag: {0}", ex);
                    return;
                }
                break;

            default: return;
        }

        if (renderChildred)
        {
            foreach(var child in tagToken.ChildElements)
            {
                VtmlUtil.Richtextify(capi, child, ref elems, fontStack, didClickLink);
            }
        }
    }
}
