using System;
using System.Collections.Generic;
using UnityEngine;

namespace RefMata.Samples
{
    [RefMatable("Colors", "NumSprites")]
    [UsingNamespace("System", "System.Text.RegularExpressions")]
    public partial class RefMataAdvancedSampleComponent : MonoBehaviour
    {
#if EXIST_COLOR_OBJECT
        [Serializable]
        sealed class ClrAndMat
        {
            public Color Clr;
            public Material Mat;
            public ClrAndMat(IEnumerable<UnityEngine.Object> objects)
            {
                foreach (var o in objects)
                {
                    if (o is ColorObject.ColorObject clo) Clr = clo;
                    else if (o is Material mat) Mat = mat;
                }
            }
        }

        [RefMataLoad("t:ColorObject,t:Material",
            "GroupBy(x => int.Parse(Regex.Replace(x.name, @\".{1,}[^0-9]\", \"\")))." +
            "Select(x => new ClrAndMat(x))")]
        [SearchInFolders("Assets/Samples/RefMata")]
        [SerializeField] ClrAndMat[] cms = default;
#endif

        [RefMataLoad("t:MonoScript,t:Sprite",
            "Where(x => x is MonoScript or Sprite)." +
            "Where(x => x.name.Contains(\"logger\", StringComparison.OrdinalIgnoreCase))." +
            "GroupBy(x => int.TryParse(Regex.Replace(x.name, @\".{1,}[^0-9]\", \"\"), out int i) ? i : -1)." +
            "Where(x => x.Key > 0)." +
            "Select(x => Activator.CreateInstance(x.OfType<MonoScript>().First().GetClass(), args: x.OfType<Sprite>().First()))")]
        [SearchInFolders("Assets/Samples/RefMata")]
        [SerializeReference] ISpriteLogger[] loggers = default;

#if UNITY_EDITOR
        [ContextMenu("Log")]
        void Log()
        {
            foreach (var logger in loggers) logger.Log(transform);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
