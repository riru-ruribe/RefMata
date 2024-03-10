using UnityEngine;
using UnityEngine.UI;

namespace RefMata.Samples
{
    abstract class SpriteLoggerBase : ISpriteLogger
    {
        [SerializeField] Sprite sprite = default;
        public abstract float Size { get; }
        public void Log(Transform parent)
        {
            if (sprite != null)
            {
                var go = new GameObject();
                go.name = sprite.name;
                go.transform.SetParent(parent);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one * Size;
                go.AddComponent<Image>().sprite = sprite;
            }
        }
        public SpriteLoggerBase(Sprite sprite)
        {
            this.sprite = sprite;
        }
    }
}
