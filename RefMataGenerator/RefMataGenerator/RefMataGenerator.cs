using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefMataGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class RefMataGenerator : IIncrementalGenerator
{
    const string Ns = "RefMata";
    const string AtrRef = "RefMatableAttribute";
    const string AtrRefDisp = Ns + "." + AtrRef;
    const string AtrRefMe = "RefMataMeAttribute";
    const string AtrRefMeDisp = Ns + "." + AtrRefMe;
    const string AtrRefChild = "RefMataChildAttribute";
    const string AtrRefChildDisp = Ns + "." + AtrRefChild;
    const string AtrRefParent = "RefMataParentAttribute";
    const string AtrRefParentDisp = Ns + "." + AtrRefParent;
    const string AtrRefLoad = "RefMataLoadAttribute";
    const string AtrRefLoadDisp = Ns + "." + AtrRefLoad;
    const string AtrUsingNs = "UsingNamespaceAttribute";
    const string AtrUsingNsDisp = Ns + "." + AtrUsingNs;
    const string AtrSearch = "SearchInFoldersAttribute";
    const string AtrSearchDisp = Ns + "." + AtrSearch;

    const string GenericApi = "System.Collections.Generic";
    const string SerializeReferenceDisp = "UnityEngine.SerializeReference";
    const string RequireComponentDisp = "UnityEngine.RequireComponent";
    const string CmpDisp = "UnityEngine.Component";
    const string SoDisp = "UnityEngine.ScriptableObject";
    const string IReferenceable = "IRefMataReferenceable";
    const string IHookable = "IRefMataHookable";

    [Flags]
    enum RefMataKinds
    {
        Me = 1 << 0,
        Child = 1 << 1,
        Parent = 1 << 2,
        Load = 1 << 3,
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource("RefMata.g.cs", PostAtr()));

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            AtrRefDisp,
            static (node, token) => node is ClassDeclarationSyntax,
            static (context, token) => context
        );
        context.RegisterSourceOutput(source, Emit);
    }

    static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;
        var typeNode = (TypeDeclarationSyntax)source.TargetNode;
        var className = typeSymbol.Name;

        var accessibility = typeSymbol.DeclaredAccessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            _ => null,
        };
        if (string.IsNullOrEmpty(accessibility))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0001, typeNode.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        if (!typeNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0002, typeNode.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        var isComponent = IsParent(typeSymbol, CmpDisp);
        var isScriptable = IsParent(typeSymbol, SoDisp);
        var isInterface = typeSymbol.Interfaces.Length > 0;
        if (!isComponent && !isScriptable && !isInterface)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0003, typeNode.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        var isAlreadyOnValidate = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(x => x.Name == "OnValidate");
        if (isAlreadyOnValidate)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.W0001, typeNode.Identifier.GetLocation(), typeSymbol.Name));
        }

        var labelHashSet = new HashSet<string>();
        var nsSb = new StringBuilder();
        var fieldSb = new StringBuilder();
        var implSb = new StringBuilder();
        var loadSb = new StringBuilder();
        var kindSb = new StringBuilder();

        foreach (var atr in typeSymbol.GetAttributes())
        {
            RefMataLabel(atr!, labelHashSet);
            RequireComponent(atr!, fieldSb, implSb);
            UsingNamespace(atr!, nsSb);
        }

        var kindHashSet = new HashSet<RefMataKinds>();

        if (fieldSb.Length > 0) // RequireComponent 
            kindHashSet.Add(RefMataKinds.Me);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IFieldSymbol field) continue;

            if (field.Type.ToString().Contains(GenericApi))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0004, typeNode.Identifier.GetLocation(), typeSymbol.Name));
                return;
            }

            bool isMe = false, isChild = false, isParent = false, isLoad = false, isSerializeReference = false;
            int atrCnt = 0;
            string inactive = "", filter = "", orderRule = "", searchInFolders = "";
            foreach (var atr in member.GetAttributes())
            {
                var disp = atr?.AttributeClass?.ToDisplayString();
                if (disp == AtrRefMeDisp)
                {
                    isMe = true; atrCnt++;
                }
                else if (disp == AtrRefChildDisp)
                {
                    isChild = true; atrCnt++; inactive = IncludeInactive(atr!); orderRule = OrderRuleRawStr(atr!);
                }
                else if (disp == AtrRefParentDisp)
                {
                    isParent = true; atrCnt++; inactive = IncludeInactive(atr!); orderRule = OrderRuleRawStr(atr!);
                }
                else if (disp == AtrRefLoadDisp)
                {
                    isLoad = true; atrCnt++; filter = Filter(atr!); orderRule = OrderRuleRawStr(atr!);
                }
                else if (disp == AtrSearchDisp)
                {
                    searchInFolders = SearchInFolders(atr!);
                }
                else if (disp == SerializeReferenceDisp)
                {
                    isSerializeReference = true;
                }
            }
            if (!isSerializeReference && atrCnt > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0005, typeNode.Identifier.GetLocation(), typeSymbol.Name));
                return;
            }

            if (implSb.Length > 0) implSb.AppendLine();
            var pre = isComponent ? "" : "root.";
            var isAry = field.Type.TypeKind == TypeKind.Array;
            var t = field.Type.ToString().Replace("[]", "");
            if (isMe)
            {
                kindHashSet.Add(RefMataKinds.Me);
                implSb.Append($$"""        {{member.Name}} = {{pre}}GetComponent{{(isAry ? "s" : "")}}<{{t}}>();""");
            }
            else if (isChild)
            {
                kindHashSet.Add(RefMataKinds.Child);
                if (!string.IsNullOrEmpty(orderRule))
                {
                    orderRule = $".{orderRule}.{(isAry ? "ToArray" : "FirstOrDefault")}()";
                    isAry = true;
                }
                implSb.Append($$"""        {{member.Name}} = {{pre}}GetComponent{{(isAry ? "s" : "")}}InChildren<{{t}}>({{inactive}}){{orderRule}};""");
            }
            else if (isParent)
            {
                kindHashSet.Add(RefMataKinds.Parent);
                if (!string.IsNullOrEmpty(orderRule))
                {
                    orderRule = $".{orderRule}.{(isAry ? "ToArray" : "FirstOrDefault")}()";
                    isAry = true;
                }
                implSb.Append($$"""        {{member.Name}} = {{pre}}GetComponent{{(isAry ? "s" : "")}}InParent<{{t}}>({{inactive}}){{orderRule}};""");
            }
            else if (isLoad)
            {
                orderRule = LoadProcSuffix(field.Type.TypeKind == TypeKind.Array, t, orderRule);
                var tmp = $$"""
    {{member.Name}} = AssetDatabase.FindAssets("{{filter}}"{{searchInFolders}})
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            {{orderRule}};
""";
                kindHashSet.Add(RefMataKinds.Load);
                implSb.Append(tmp);
                if (loadSb.Length > 0) loadSb.AppendLine();
                loadSb.Append(tmp);
            }
            else if (isSerializeReference) // NOTE: prioritize other attributes.
            {
                kindHashSet.Add(RefMataKinds.Load); // FIXME: このままでも良いかも
                implSb.Append($$"""        ({{member.Name}} as {{IReferenceable}})?.RunOnValidate(this);""");
                if (loadSb.Length > 0) loadSb.AppendLine();
                loadSb.Append($$"""        ({{member.Name}} as {{IReferenceable}})?.RunLoad();""");
            }
        }

        if (kindHashSet.Count <= 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0006, typeNode.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        if (isScriptable && kindHashSet.Where(x => x != RefMataKinds.Load).Count() > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0007, typeNode.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        var isNamespace = !typeSymbol.ContainingNamespace.IsGlobalNamespace;

        if (isInterface && !isComponent) // pure interface
        {
            context.AddSource($"{className}.RefMata.g.cs", $$"""
// <auto-generated/>
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using {{Ns}};{{nsSb}}
#endif
{{(isNamespace ? $$"""namespace {{typeSymbol.ContainingNamespace}} {""" : "")}}
{{accessibility}} partial class {{className}}
    #if UNITY_EDITOR
    : {{IReferenceable}}
    #endif
{
{{fieldSb}}
    #if UNITY_EDITOR
    void {{IReferenceable}}.RunOnValidate(Component root)
    {
{{implSb}}
    }
    void {{IReferenceable}}.RunLoad()
    {
{{loadSb}}
    }
    #endif
}
{{(isNamespace ? "}" : "")}}
""");
            return;
        }

        context.AddSource($"{className}.RefMata.g.cs", $$"""
// <auto-generated/>
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using {{Ns}};{{nsSb}}
#endif
{{(isNamespace ? $$"""namespace {{typeSymbol.ContainingNamespace}} {""" : "")}}
{{accessibility}} partial class {{className}}
    #if UNITY_EDITOR
    : {{IHookable}}
    #endif
{
{{fieldSb}}
    #if UNITY_EDITOR
    RefMataKinds {{IHookable}}.Kinds => {{string.Join(" | ", kindHashSet.Select(x => $"RefMataKinds.{x}"))}};
    readonly HashSet<string> labels = new HashSet<string>
    {
        {{string.Join(" , ", labelHashSet.Select(x => $"\"{x}\""))}}
    };
    HashSet<string> {{IHookable}}.Labels => labels;
{{OnValidateMethod(isScriptable, isAlreadyOnValidate)}}
    void {{IHookable}}.RunOnValidate()
    {
{{implSb}}
    }
    void {{IHookable}}.RunLoad()
    {
{{loadSb}}
    }
    #endif
}
{{(isNamespace ? "}" : "")}}
""");
    }

    static bool IsParent(INamedTypeSymbol symbol, string disp)
    {
        var type = symbol.BaseType;
        while (type != null)
        {
            if (type.ToString() == disp) return true;
            type = type.BaseType;
        }
        return false;
    }

    static void RefMataLabel(AttributeData atr, HashSet<string> labelHashSet)
    {
        if (atr?.AttributeClass?.ToDisplayString() == AtrRefDisp &&
            atr.ConstructorArguments.Length > 0)
        {
            foreach (var arg in atr.ConstructorArguments[0].Values)
            {
                labelHashSet.Add($"RefMata{arg.Value?.ToString()}");
            }
        }
    }

    static void RequireComponent(AttributeData atr, StringBuilder fieldSb, StringBuilder implSb)
    {
        if (atr?.AttributeClass?.ToDisplayString() == RequireComponentDisp &&
            atr.ConstructorArguments.Length > 0)
        {
            foreach (var arg in atr.ConstructorArguments)
            {
                var typeName = arg.Value?.ToString();
                var className = typeName.Contains('.') ? typeName?.Split('.').Last() : typeName;
                className = ToFirstLower(className!);

                if (fieldSb.Length > 0) fieldSb.AppendLine();
                fieldSb.Append($"    [SerializeField] {typeName} {className}Required = default;");

                if (implSb.Length > 0) implSb.AppendLine();
                implSb.Append($"        {className}Required = GetComponent<{typeName}>();");
            }
        }
    }

    static void UsingNamespace(AttributeData atr, StringBuilder nsSb)
    {
        if (atr?.AttributeClass?.ToDisplayString() == AtrUsingNsDisp &&
            atr.ConstructorArguments.Length > 0)
        {
            foreach (var arg in atr.ConstructorArguments[0].Values)
            {
                if (arg.Value?.ToString() is { } v)
                {
                    nsSb.AppendLine();
                    nsSb.Append($"using {v};");
                }
            }
        }
    }

    static string IncludeInactive(AttributeData atr)
    {
        if (atr?.ConstructorArguments.Length > 0 &&
            atr.ConstructorArguments[0].Value?.ToString() is { } v)
        {
            return ToFirstLower(v);
        }
        return "";
    }

    static string Filter(AttributeData atr)
    {
        if (atr?.ConstructorArguments.Length > 0 &&
            atr.ConstructorArguments[0].Value?.ToString() is { } v)
        {
            return v;
        }
        return "";
    }

    static string OrderRuleRawStr(AttributeData atr)
    {
        if (atr?.ConstructorArguments.Length == 2 &&
            atr.ConstructorArguments[1].Value?.ToString() is { } v)
        {
            return v;
        }
        return "";
    }

    static string SearchInFolders(AttributeData atr)
    {
        if (atr?.ConstructorArguments.Length > 0)
        {
            var sb = new StringBuilder();
            foreach (var arg in atr.ConstructorArguments[0].Values)
            {
                if (arg.Value?.ToString() is { } v)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append("\"");
                    sb.Append(v);
                    sb.Append("\"");
                }
            }
            if (sb.Length > 0)
            {
                return $$""", new[] {{{sb}}}""";
            }
        }
        return "";
    }

    static string LoadProcSuffix(bool isAry, string t, string orderRuleRawStr)
    {
        if (isAry)
        {
            if (string.IsNullOrEmpty(orderRuleRawStr))
                return $".OfType<{t}>().ToArray()";
            else
                return $".{orderRuleRawStr}.OfType<{t}>().ToArray()";
        }
        else
        {
            if (string.IsNullOrEmpty(orderRuleRawStr))
                return $".OfType<{t}>().FirstOrDefault()";
            else
                return $".{orderRuleRawStr}.OfType<{t}>().FirstOrDefault()";
        }
    }

    static string OnValidateMethod(bool isScriptable, bool isAlreadyOnValidate)
    {
        if (isScriptable)
        {
            return $$"""
    [ContextMenu(nameof(RefMata))]
    void RefMata()
    {
        (this as {{IHookable}}).RunLoad();
    }
""";
        }
        return $$"""
    [ContextMenu("RefMata")]
    void OnValidate{{(isAlreadyOnValidate ? "Gen" : "")}}()
    {
        if (GetComponent<RefMataHook>() == null)
            gameObject.AddComponent<RefMataHook>();
    }
""";
    }

    static string PostAtr()
    {
        return $$"""
// <auto-generated/>
using System;
namespace {{Ns}}
{
    /// <summary>
    /// when attached to script, '{{IHookable}}' is automatically implemented and called in 'OnValidate'.<br/>
    /// if 'OnValidate' is implemented, 'OnValidateGen' method will be generated instead.<br/>
    /// be careful as label output is $"RefMata{labelSuffix}".
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class {{AtrRef}} : Attribute
    {
        public {{AtrRef}}() { }
        public {{AtrRef}}(params string[] labelSuffixes) { }
    }

    /// <summary>
    /// component attached to 'MonoBehaviour' can be automatically referenced.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class {{AtrRefMe}} : Attribute
    {
        public {{AtrRefMe}}() { }
    }

    /// <summary>
    /// child components attached to 'MonoBehaviour' can be automatically referenced.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class {{AtrRefChild}} : Attribute
    {
        public {{AtrRefChild}}() { }
        public {{AtrRefChild}}(bool includeInactive) { }
        public {{AtrRefChild}}(bool includeInactive, string orderRuleRawStr) { }
    }

    /// <summary>
    /// parent components attached to 'MonoBehaviour' can be automatically referenced.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class {{AtrRefParent}} : Attribute
    {
        public {{AtrRefParent}}() { }
        public {{AtrRefParent}}(bool includeInactive) { }
        public {{AtrRefParent}}(bool includeInactive, string orderRuleRawStr) { }
    }

    /// <summary>
    /// assets under 'Assets/' folder can be automatically referenced.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class {{AtrRefLoad}} : Attribute
    {
        public {{AtrRefLoad}}(string filter) { }
        public {{AtrRefLoad}}(string filter, string orderRuleRawStr) { }
    }

    /// <summary>
    /// add namespaces to automatically created sources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class {{AtrUsingNs}} : Attribute
    {
        public {{AtrUsingNs}}(params string[] namespaces) { }
    }

    /// <summary>
    /// arguments to pass to 'AssetDatabase.FindAssets'.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class {{AtrSearch}} : Attribute
    {
        public {{AtrSearch}}(params string[] folders) { }
    }
}
""";
    }

    static string ToFirstLower(string s)
    {
        return char.ToLower(s[0]) + s.Substring(1);
    }
}
