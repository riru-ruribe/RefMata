## auto implementation details

#### ◆ case of inherit 'Component'
```C#
[RequireComponent(typeof(RectTransform))]
[RefMatable("Hoge")]
public partial class Script : MonoBehaviour
{
    [SerializeField, RefMataMe] Image image;
    [SerializeField, RefMataChild] Text[] texts;
    [SerializeField, RefMataLoad("t:Sprite")] Sprite[] sprites;
    [SerializeReference] IXXX x;
}

// ↓ auto implement

public partial class Script
    #if UNITY_EDITOR
    : IRefMataHookable
    #endif
{
    [SerializeField] RectTransform rectTransformRequired = default;
    #if UNITY_EDITOR
    RefMataKinds IRefMataHookable.Kinds => RefMataKinds.Me | RefMataKinds.Child | RefMataKinds.Load;
    readonly HashSet<string> labels = new HashSet<string>
    {
        "RefMataHoge"
    };
    HashSet<string> IRefMataHookable.Labels => labels;
    [ContextMenu("RefMata")]
    void OnValidate()
    {
        // prepared this component to avoid individual implementations dirty.
        if (GetComponent<RefMataHook>() == null)
            gameObject.AddComponent<RefMataHook>();
    }
    void IRefMataHookable.RunOnValidate()
    {
        rectTransformRequired = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        texts = GetComponentsInChildren<Text>();
        sprites = AssetDatabase.FindAssets("t:Sprite")
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Sprite>()
            .ToArray();
        (x as IRefMataReferenceable)?.RunOnValidate(this);
    }
    void IRefMataHookable.RunLoad()
    {
        sprites = AssetDatabase.FindAssets("t:Sprite")
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Sprite>()
            .ToArray();
        (x as IRefMataReferenceable)?.RunLoad();
    }
    #endif
}
```

#### ◆ case of inherit 'ScriptableObject'
```C#
[RefMatable("Fuga")]
public partial class Script : ScriptableObject
{
    [SerializeField, RefMataLoad("t:Sprite")] Sprite[] sprites;
}

// ↓ auto implement

public partial class Script
    #if UNITY_EDITOR
    : IRefMataHookable
    #endif
{
    #if UNITY_EDITOR
    RefMataKinds IRefMataHookable.Kinds => RefMataKinds.Load;
    readonly HashSet<string> labels = new HashSet<string>
    {
        "RefMataFuga"
    };
    HashSet<string> IRefMataHookable.Labels => labels;
    [ContextMenu("RefMata")]
    void RefMata()
    {
        (this as IRefMataHookable).RunLoad();
    }
    void IRefMataHookable.RunOnValidate()
    {
        sprites = AssetDatabase.FindAssets("t:Sprite")
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Sprite>()
            .ToArray();
    }
    void IRefMataHookable.RunLoad()
    {
        sprites = AssetDatabase.FindAssets("t:Sprite")
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Sprite>()
            .ToArray();
    }
    #endif
}
```

#### ◆ case of inherit interface

```C#
[RefMatable]
public partial class Script : IXXX
{
    [SerializeField, RefMataMe] Image image;
    [SerializeField, RefMataChild] Text[] texts;
    [SerializeField, RefMataLoad("t:Sprite")] Sprite[] sprites;
}

// ↓ auto implement

public partial class Script
    #if UNITY_EDITOR
    : IRefMataReferenceable
    #endif
{
    #if UNITY_EDITOR
    void IRefMataReferenceable.RunOnValidate(Component root)
    {
        image = GetComponent<Image>();
        texts = GetComponentsInChildren<Text>();
        sprites = AssetDatabase.FindAssets("t:Sprite")
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Sprite>()
            .ToArray();
    }
    void IRefMataReferenceable.RunLoad()
    {
        sprites = AssetDatabase.FindAssets("t:Sprite")
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Sprite>()
            .ToArray();
    }
    #endif
}
```
