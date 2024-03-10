using UnityEngine;

namespace RefMata.Samples
{
    [CreateAssetMenu(menuName = "RefMata/Samples/RefMataBasicSampleSO", fileName = "RefMataBasicSampleSO")]
    [RefMatable("NumSprites")]
    [UsingNamespace("System.Text.RegularExpressions")]
    public partial class RefMataBasicSampleSO : ScriptableObject
    {
        [RefMataLoad("t:Sprite", "Where(x => x is Sprite && Regex.IsMatch(x.name, @\"" + @"^refmata_num_\d{1,}" + "\"))")]
        [SerializeField] Sprite[] s1 = default;
    }
}
