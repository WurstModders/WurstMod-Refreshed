using UnityEngine;

namespace WurstModRefreshed
{
    public static class Extensions
    {
        public static Sprite ToSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100f, 0, SpriteMeshType.Tight);
        }
    }
}