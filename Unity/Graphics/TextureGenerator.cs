using System;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Graphics
{
    [RequireComponent(typeof(Image))]
    public class TextureGenerator : MonoBehaviour
    {
        #region Serialized Fields
        public eTextureType TextureType;
        #endregion

        #region Private Fields
        private Image m_image;

        private Color32 m_brushColor;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_brushColor = Color.white;
            m_image = this.GetComponent<Image>();

            GenerateTexture();
        }

        private void GenerateTexture()
        {
            switch (TextureType)
            {
                case eTextureType.Gradient:
                    GenerateGradientTexture();
                    break;
                case eTextureType.MonotoneNoised:
                    GenerateMonotoneNoisedTexture();
                    break;
                default:
                    break;
            }
        }

        private void GenerateGradientTexture()
        {
            int width = 256 * 6;
            Texture2D texture = new Texture2D(width, 1, TextureFormat.RGB24, false);

            m_brushColor = Color.red;

            Color[] colorPerColumn = texture.GetPixels();
            for (int i = 0; i < texture.width; i++)
            {
                colorPerColumn[i] = m_brushColor;

                int remainingColorShift = 1;

                while (remainingColorShift > 0)
                {
                    if (m_brushColor.r == 255)
                    {
                        if (m_brushColor.b > 0)
                            m_brushColor.b--;
                        else if (m_brushColor.b == 0 && m_brushColor.g != 255)
                            m_brushColor.g++;
                        else if (m_brushColor.g == 255)
                            m_brushColor.r--;
                    }
                    else if (m_brushColor.g == 255)
                    {
                        if (m_brushColor.r > 0)
                            m_brushColor.r--;
                        else if (m_brushColor.r == 0 && m_brushColor.b != 255)
                            m_brushColor.b++;
                        else if (m_brushColor.b == 255)
                            m_brushColor.g--;
                    }
                    else // if (m_brushColor.b == 255)
                    {
                        if (m_brushColor.g > 0)
                            m_brushColor.g--;
                        else if (m_brushColor.g == 0 && m_brushColor.r != 255)
                            m_brushColor.r++;
                        else if (m_brushColor.r == 255)
                            m_brushColor.b--;
                    }

                    remainingColorShift--;
                }
            }

            texture.SetPixels(colorPerColumn);
            texture.Apply();

            m_image.sprite = ImageConverter.TextureToSprite(texture);
        }

        private void GenerateMonotoneNoisedTexture()
        {
            int width = 256;
            Texture2D texture = new Texture2D(width, 1, TextureFormat.RGB24, false);

            MTRandom.RandInit();

            Color[] colorPerColumn = texture.GetPixels();
            for (int i = 0; i < texture.width; i++)
            {
                byte colorValue = Convert.ToByte(MTRandom.GetRandInt(0, 255));
                m_brushColor = new Color32(colorValue, colorValue, colorValue, 255);
                colorPerColumn[i] = m_brushColor;
            }

            texture.SetPixels(colorPerColumn);
            texture.Apply();

            m_image.sprite = ImageConverter.TextureToSprite(texture);
        }

        public enum eTextureType
        {
            Gradient,
            MonotoneNoised
        }
    }
}