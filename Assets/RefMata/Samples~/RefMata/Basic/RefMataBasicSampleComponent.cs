using UnityEngine;
using UnityEngine.UI;

namespace RefMata.Samples
{
    [RequireComponent(typeof(RectTransform))]
    [RefMatable("Colors", "NumSprites")]
    [UsingNamespace("System.Text.RegularExpressions")]
    public partial class RefMataBasicSampleComponent : MonoBehaviour
    {
        [SerializeField, RefMataMe] Image image = default;

        [RefMataChild(true, "Where(x => x.name.Contains(\"hoge\"))")]
        [SerializeField] Text[] t1 = default;

        [RefMataChild(true, "Where(x => x.name.Contains(\"hoge\"))")]
        [SerializeField] Text t2 = default;

        [RefMataParent(true)]
        [SerializeField] Canvas canvas = default;

        [RefMataLoad("t:Sprite", "Where(x => x is Sprite && Regex.IsMatch(x.name, @\"" + @"^refmata_num_\d{1,}" + "\"))")]
        [SerializeField] Sprite[] s1 = default;

        [RefMataLoad("t:Sprite", "Where(x => x is Sprite && x.name.StartsWith(\"refmata_\"))")]
        [SerializeField] Sprite s2 = default;

        [RefMataLoad("t:Material")]
        [SearchInFolders("Assets/Samples/RefMata")]
        [SerializeField] Material[] m2 = default;

#if EXIST_COLOR_OBJECT
        [RefMataLoad("t:ColorObject", "Select(x => (Color)((ColorObject.ColorObject)x))")]
        [SerializeField] Color[] c1 = default;
#endif

        void Start()
        {
            Debug.Log(image.name);
            Debug.Log(rectTransformRequired.name);
        }
    }
}
