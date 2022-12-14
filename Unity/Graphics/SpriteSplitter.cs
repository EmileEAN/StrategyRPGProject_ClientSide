using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Graphics
{
    static class SpriteSplitter
    {
        public static List<Sprite> SpriteToMultipleSprites(Sprite _sprite, int _numberOfColumns, int _numberOfRows)
        {
            List<Sprite> sprites = new List<Sprite>();

            Texture2D texture = _sprite.texture;

            int w = texture.width;
            int h = texture.height;
            int blockWidth = w / _numberOfColumns;
            int blockHeight = h / _numberOfRows;

            Color[] c;
            for (int y = _numberOfRows; y >= 1; y--)
            {
                for (int x = 1; x <= _numberOfColumns; x++)
                {
                    c = texture.GetPixels((x - 1) * blockWidth, (y - 1) * blockHeight, blockWidth, blockHeight);
                    Texture2D txtr = new Texture2D(blockWidth, blockHeight);
                    txtr.SetPixels(c);
                    txtr.Apply();
                    sprites.Add(Sprite.Create(txtr, new Rect(0, 0, blockWidth, blockHeight), new Vector2()));
                }
            }

            return sprites;
        }
    }
}