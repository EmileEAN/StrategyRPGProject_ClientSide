using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Graphics
{
    public static class ImageConverter
    {
        public static Texture2D ByteArrayToTexture(byte[] _bytes, FilterMode _filterMode, bool _alphaIsTransparency = true)
        {
            Texture2D texture = new Texture2D(1, 1) // Create a new Texture2D of whatever size
            {
                alphaIsTransparency = _alphaIsTransparency
            }; 
            texture.LoadImage(_bytes); // The image will be loaded to the texture with the correct/original image size
            texture.filterMode = _filterMode;

            return texture;
        }

        public static Sprite ByteArrayToSprite(byte[] _bytes, FilterMode _filterMode)
        {
            Texture2D texture = ByteArrayToTexture(_bytes, _filterMode);
            texture.filterMode = _filterMode;

            return TextureToSprite(texture);
        }

        public static List<Sprite> SpriteToMultipleSprites(Sprite _sprite, int _numberOfColumns, int _numberOfRows)
        {
            List<Sprite> sprites = new List<Sprite>();

            Texture2D texture = _sprite.texture;

            int w = texture.width;
            int h = texture.height;
            int blockWidth = w / _numberOfColumns;
            int blockHeight = h / _numberOfRows;

            for (int y = _numberOfRows - 1; y >= 0; y--)
            {
                for (int x = 0; x < _numberOfColumns; x++)
                {
                    sprites.Add(Sprite.Create(texture, new Rect(x * blockWidth, y * h / _numberOfRows, blockWidth, blockHeight), Vector2.zero));
                }
            }

            return sprites;
        }

        public static List<Sprite> ByteArrayToMultipleSprites(byte[] _bytes, int _numberOfColumns, int _numberOfRows, FilterMode _filterMode)
        {
            List<Sprite> sprites = new List<Sprite>();

            Texture2D texture = ByteArrayToTexture(_bytes, _filterMode);

            int w = texture.width;
            int h = texture.height;
            int blockWidth = w / _numberOfColumns;
            int blockHeight = h / _numberOfRows;

            for (int y = _numberOfRows - 1; y >= 0; y--)
            {
                for (int x = 0; x < _numberOfColumns; x++)
                {
                    sprites.Add(Sprite.Create(texture, new Rect(x * blockWidth, y * h / _numberOfRows, blockWidth, blockHeight), Vector2.zero));
                }
            }

            return sprites;
        }

        public static Sprite TextureToSprite(Texture2D _texture, Sprite _baseSprite = null)
        {
            if (_baseSprite == null)
                return Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), Vector2.zero);
            else
                return Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), Vector2.zero, _baseSprite.pixelsPerUnit, 1, default, _baseSprite.border);
        }
    }
}
