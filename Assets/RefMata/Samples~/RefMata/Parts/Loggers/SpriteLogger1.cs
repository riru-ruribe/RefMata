using UnityEngine;

namespace RefMata.Samples
{
    sealed class SpriteLogger1 : SpriteLoggerBase
    {
        public override float Size => 0.5f;
        public SpriteLogger1(Sprite sprite) : base(sprite) { }
    }
}
