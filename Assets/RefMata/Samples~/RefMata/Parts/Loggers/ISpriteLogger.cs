using UnityEngine;

namespace RefMata.Samples
{
    public interface ISpriteLogger
    {
        float Size { get; }
        void Log(Transform parent);
    }
}
