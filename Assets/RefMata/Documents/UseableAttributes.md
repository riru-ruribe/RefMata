## useable attributes

#### ◆ RefMata.RefMatable
```C#
/// <summary>
/// when attached to script, 'IRefMataHookable' is automatically implemented and called in 'OnValidate'.
/// if 'OnValidate' is implemented, 'OnValidateGen' method will be generated instead.
/// be careful as label output is $"RefMata{labelSuffix}".
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class RefMatableAttribute : Attribute
{
    public RefMatableAttribute() { }
    public RefMatableAttribute(params string[] labelSuffixes) { }
}
```

#### ◆ RefMata.RefMataMe
```C#
/// <summary>
/// component attached to 'MonoBehaviour' can be automatically referenced.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class RefMataMeAttribute : Attribute
{
    public RefMataMeAttribute() { }
}
```

#### ◆ RefMata.RefMataChild
```C#
/// <summary>
/// child components attached to 'MonoBehaviour' can be automatically referenced.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class RefMataChildAttribute : Attribute
{
    public RefMataChildAttribute() { }
    public RefMataChildAttribute(bool includeInactive) { }
    public RefMataChildAttribute(bool includeInactive, string orderRuleRawStr) { }
}
```

#### ◆ RefMata.RefMataParent
```C#
/// <summary>
/// parent components attached to 'MonoBehaviour' can be automatically referenced.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class RefMataParentAttribute : Attribute
{
    public RefMataParentAttribute() { }
    public RefMataParentAttribute(bool includeInactive) { }
    public RefMataParentAttribute(bool includeInactive, string orderRuleRawStr) { }
}
```

#### ◆ RefMata.RefMataLoad
```C#
/// <summary>
/// assets under 'Assets/' folder can be automatically referenced.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class RefMataLoadAttribute : Attribute
{
    public RefMataLoadAttribute(string filter) { }
    public RefMataLoadAttribute(string filter, string orderRuleRawStr) { }
}
```

#### ◆ RefMata.UsingNamespace
```C#
/// <summary>
/// add namespaces to automatically created sources.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class UsingNamespaceAttribute : Attribute
{
    public UsingNamespaceAttribute(params string[] namespaces) { }
}
```

#### ◆ RefMata.SearchInFolders
```C#
/// <summary>
/// arguments to pass to 'AssetDatabase.FindAssets'.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class SearchInFoldersAttribute : Attribute
{
    public SearchInFoldersAttribute(params string[] folders) { }
}
```
