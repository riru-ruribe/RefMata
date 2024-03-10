using UnityEngine;

namespace RefMata.Samples
{
    sealed class SpriteLogger2 : SpriteLoggerBase
    {
        public override float Size => 0.75f;
        public SpriteLogger2(Sprite sprite) : base(sprite) { }
    }
}
