using UnityEngine;

namespace RefMata
{
    /// <summary>
    /// prepared this component to avoid individual implementations dirty.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RefMataHook : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("RefMata")]
        void OnValidate()
        {
            if (Application.isPlaying) return;
            var hookables = gameObject.GetComponents<IRefMataHookable>();
            if (hookables?.Length > 0)
            {
                foreach (var hookable in hookables)
                {
                    hookable.RunOnValidate();
                }
            }
        }
#endif
    }
}
