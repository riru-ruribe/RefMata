using UnityEngine;

namespace RefMata.Samples
{
    sealed class SpriteLogger3 : SpriteLoggerBase
    {
        public override float Size => 1f;
        public SpriteLogger3(Sprite sprite) : base(sprite) { }
    }
}
