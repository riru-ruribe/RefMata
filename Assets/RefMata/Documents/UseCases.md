## use cases

#### ◆ simple sample
```C#
[RefMatable]
public partial class Script : MonoBehaviour
{
    [SerializeField, RefMataMe] Image image;
    [SerializeField, RefMataChild] Text[] texts;
}
```

#### ◆ 'RequireComponent' also automatically define and reference field
```C#
[RefMatable, RequireComponent(typeof(RectTransform))]
public partial class Script : MonoBehaviour
{
    void Start()
    {
        Debug.Log(rectTransformRequired.name);
    }
}
```

#### ◆ sample of 'orderRuleRawStr'
```C#
[RefMatable]
[UsingNamespace("System.Text.RegularExpressions")]
public partial class Script : MonoBehaviour
{
    /*
        GetComponentsInChildren<T>(true)
            .--can write this part freely--
            .ToArray();
    */
    [RefMataChild(true, "Where(x => x.name.Contains(\"hoge\"))")]
    public Text[] t1;

    /*
        GetComponentsInChildren<T>(true)
            .--can write this part freely--
            .FirstOrDefault();
    */
    [RefMataChild(true, "Where(x => x.name.Contains(\"hoge\"))")]
    public Text t2;

    /*
        GetComponentsInParent<T>(true)
            .--can write this part freely--
            .ToArray();
    */
    [RefMataParent(true, "Where(x => x.name.Contains(\"fuga\"))")]
    public Text[] t3;

    /*
        AssetDatabase.FindAssets(filter)
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .--can write this part freely--
            .OfType<T>()
            .FirstOrDefault();
    */
    [RefMataLoad("t:Sprite", "Where(x => x is Sprite && x.name.StartsWith(\"hoge\"))")]
    public Sprite s1;

    /*
        AssetDatabase.FindAssets(filter, new[] { "Assets/Fuga" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .--can write this part freely--
            .OfType<T>()
            .ToArray();
    */
    [RefMataLoad("t:Sprite", "Where(x => x is Sprite && Regex.IsMatch(x.name, @\"" + @"^fuga\d{1,}" + "\"))")]
    [SearchInFolders("Assets/Fuga")]
    public Sprite[] s2;
}
```

#### ◆ sample of 'SerializeReference'
```C#
[RefMatable]
partial class XXX : IXXX
{
    [SerializeField, RefMataMe] Image image;
}

[RefMatable]
[UsingNamespace("System.Text.RegularExpressions")]
public partial class Script : MonoBehaviour
{
    [SerializeReference] IXXX x;

    // combinations are also possible.
    // in that case you have to call 'IRefMataReferenceable' yourself (if necessary). 
    [RefMataLoad("t:MonoScript", 
        "Where(x => x.GetClass().GetInterfaces().Contains(typeof(IXXX)))." +
        "Select(x => Activator.CreateInstance(x.GetClass())" +
        "Select(x => { (x as IRefMataReferenceable)?.RunOnValidate(this); return x; })")]
    [SerializeReference] IXXX[] y;
}
```

#### ◆ IRefMataHookable
can also hook processing by implementing 'IRefMataHookable' individually without using 'RefMatable' attribute.
