using HarmonyLib;
using InsanityLib.Documentation;
using InsanityLib.Extensions;
using InsanityLib.PathResolvers;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
        //TODO collapsible sections maybe?
        //TODO maybe anchor/goto tag for in doc navigation
        switch (tagToken.Name)
        {
            //TODO maybe add comparison logic?
            case "if":
                tagToken.Attributes?.TryGetValue("condition", out path);
                if(!ResolvePath(capi, "if", path, out resolved)) return;
                
                renderChildred = resolved.IsTruthy();
                break;

            case "value":
                if (ResolvePath(capi, "value", tagToken.ContentText, out resolved))
                {
                    elems.Add(new RichTextComponent(capi, resolved?.ToString() ?? "null", fontStack.Peek()));
                }
                else return;
                break;
            //TODO `foreach`/`for` to handle arrays/dictionaries resolved by a path resolver

            case "lang":
                var args = new List<object?>();
                
                if(tagToken.Attributes?.Count is int argCount && argCount > 0)
                {
                    var lookup = tagToken.Attributes.GetAlternateLookup<ReadOnlySpan<char>>();
                    Span<char> buffer = stackalloc char[3];

                    for(var i = 0; i < argCount; i++)
                    {
                        if (!i.TryFormat(buffer, out int charsWritten) || !lookup.TryGetValue(buffer[..charsWritten], out var arg)) break;
                        
                        if (long.TryParse(arg, out var argAsLong))
                        {
                            args.Add(argAsLong);
                        }
                        else if (double.TryParse(arg, out var argAsDouble))
                        {
                            args.Add(argAsDouble);
                        }
                        else if (Resolver.TryResolve(arg, capi, out var argAsResolvedObj))
                        {
                            args.Add(argAsResolvedObj);
                        }
                        else args.Add(arg);
                    }
                }
                tokens = VtmlParser.Tokenize(capi.Logger, Lang.Get(tagToken.ContentText, [.. args]));

                VtmlUtil.Richtextify(capi, tokens, ref elems, fontStack, didClickLink);
                break;
            
            //TODO maybe one for writing documentation on an entire class (and maybe even subclass?)
            case "doc":
                string? typeName = null;
                string? memberName = null;
                MemberInfo? member = null;
                try
                {
                    if(tagToken.Attributes is null || !tagToken.Attributes.TryGetValue("type", out typeName) || AccessTools.TypeByName(typeName) is not Type type)
                    {
                        capi.Logger.Warning("[InsanityLib] Encountered a <doc> tag with unresolved type: {0}", typeName ?? "not specified");
                        return;
                    }

                    if(tagToken.Attributes.TryGetValue("member", out memberName))
                    {
                        var possibleMembers = type.GetMember(memberName);
                        if(possibleMembers.Length == 1) member = possibleMembers[0];
                        else if(possibleMembers.Length > 2)
                        {
                            member = possibleMembers.OrderByDescending(ReflectionExtensions.IsPublic).First();
                        }
                        else
                        {
                            capi.Logger.Warning("[InsanityLib] Encountered a <doc> tag with unresolved member on type '{0}': {1}", typeName, memberName);
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

    private static void TraverseRecursive(VtmlTagToken token, Action<VtmlToken> action)
    {
        action(token);
        var stack = new Stack<VtmlTagToken>(); 
        stack.Push(token);

        while(stack.TryPop(out var currentToken))
        {
            if(currentToken.ChildElements is not { Count: > 0}) continue;
            foreach(var childToken in currentToken.ChildElements)
            {
                action(childToken);
                if(childToken is VtmlTagToken childTagToken) stack.Push(childTagToken);
            }
        }
    }

    private static bool ResolvePath(ICoreClientAPI capi, string tag, string? path, out object? resolved)
    {
        if(Resolver.TryResolve(path, capi, out resolved)) return true;

        capi.Logger.Warning("[InsanityLib] Encountered a '{0}' tag with path that could not be resolved: {1}", tag, path);

        return false;
    }
}