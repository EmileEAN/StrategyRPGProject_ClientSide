using EEANWorks.Games.Unity.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Graphics
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class ImageColorBlender : MonoBehaviour
    {

        #region Serialized Fields
        [SerializeField]
        private bool m_executeScriptAlways = true;

        [Space(10)]

        [SerializeField]
        private Sprite m_sprite;
        [SerializeField]
        private eBlendingMethod m_blendingMethod;
        [SerializeField]
        private Color m_color = Color.white;
        #endregion

        #region Properties
        public Sprite Sprite { get { return m_sprite; } set { if (SetPropertyUtility.SetClass(ref m_sprite, value)) { BlendColor(); } } }
        public eBlendingMethod BlendingMethod { get { return m_blendingMethod; } set { if (SetPropertyUtility.SetStruct(ref m_blendingMethod, value)) { BlendColor(); } } }
        public Color Color { get { return m_color; } set { if (SetPropertyUtility.SetColor(ref m_color, value)) { BlendColor(); } } }
        #endregion

        #region Private Fields
        private Image m_image;

        private float m_r;
        private float m_g;
        private float m_b;
        private float m_a;
        #endregion

        private void OnValidate()
        {
            if (!Application.isPlaying && !m_executeScriptAlways)
                return;

            if (m_image != null && m_sprite != null)
                BlendColor();
            else if (m_sprite == null)
            {
                //Debug.Log(this.gameObject.name + "'s ImageColorBlender -> You need to assign a Sprite!");
                return;
            }
        }

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_image = this.GetComponent<Image>();
            BlendColor();
        }

        private void BlendColor()
        {
            if (!Application.isPlaying && !m_executeScriptAlways)
                return;

            if (m_image == null)
                return;

            if (m_sprite == null)
            {
                //Debug.Log(this.gameObject.name + "'s ImageColorBlender -> You need to assign a Sprite!");
                return;
            }

            //Debug.Log("Blending Color!");

            m_r = m_color.r;
            m_g = m_color.g;
            m_b = m_color.b;
            m_a = m_color.a;

            switch (m_blendingMethod)
            {
                default: // case eBlendingMethod.None:
                    m_image.sprite = m_sprite;
                    break;

                case eBlendingMethod.Normal:
                    Normal();
                    break;
                case eBlendingMethod.Multiply:
                    Multiply();
                    break;
                case eBlendingMethod.Add:
                    Add();
                    break;
                case eBlendingMethod.Subtract:
                    Subtract();
                    break;
                case eBlendingMethod.Overlay:
                    Overlay();
                    break;
                case eBlendingMethod.Screen:
                    Screen();
                    break;
                case eBlendingMethod.Lighten:
                    Lighten();
                    break;
                case eBlendingMethod.Darken:
                    Darken();
                    break;
                case eBlendingMethod.Difference:
                    Difference();
                    break;
                case eBlendingMethod.Dodge:
                    Dodge();
                    break;
                case eBlendingMethod.Burn:
                    Burn();
                    break;
                case eBlendingMethod.SoftLight:
                    SoftLight();
                    break;
                case eBlendingMethod.HardLight:
                    HardLight();
                    break;
                case eBlendingMethod.Hue:
                    Hue();
                    break;
                case eBlendingMethod.Saturation:
                    Saturation();
                    break;
                case eBlendingMethod.Color:
                    Color_();
                    break;
                case eBlendingMethod.Luminosity:
                    Luminosity();
                    break;
            }
        }

        private void Normal()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = ApplyAlphaBlending(pixels[i].r, m_r);
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, m_g);
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, m_b);
            }

            SetTexture(targetTexture, pixels);
        }

        private void Multiply()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = ApplyAlphaBlending(pixels[i].r, pixels[i].r * m_r);
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, pixels[i].g * m_g);
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, pixels[i].b * m_b);
            }

            SetTexture(targetTexture, pixels);
        }

        private void Add()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = ApplyAlphaBlending(pixels[i].r, pixels[i].r + m_r);
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, pixels[i].g + m_g);
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, pixels[i].b + m_b);
            }

            SetTexture(targetTexture, pixels);
        }

        private void Subtract()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, (r > m_r) ? r - m_r : 0);
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, (g > m_g) ? g - m_g : 0);
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, (b > m_b) ? b - m_b : 0);
            }

            SetTexture(targetTexture, pixels);
        }

        private void Overlay()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, (r < 0.5f) ? (2 * r * m_r) : (1 - 2 * (1 - r) * (1 - m_r)));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, (g < 0.5f) ? (2 * g * m_g) : (1 - 2 * (1 - g) * (1 - m_g)));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, (b < 0.5f) ? (2 * b * m_b) : (1 - 2 * (1 - b) * (1 - m_b)));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Screen()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, 1 - ((1 - m_r) * (1 - r)));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, 1 - ((1 - m_g) * (1 - g)));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, 1 - ((1 - m_b) * (1 - b)));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Lighten()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, Mathf.Max(r, m_r));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, Mathf.Max(g, m_g));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, Mathf.Max(b, m_b));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Darken()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, Mathf.Min(r, m_r));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, Mathf.Min(g, m_g));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, Mathf.Min(b, m_b));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Difference()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, Mathf.Abs(r - m_r));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, Mathf.Abs(g - m_g));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, Mathf.Abs(b - m_b));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Dodge()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, pixels[i].r / (1 - m_r));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, pixels[i].g / (1 - m_g));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, pixels[i].b / (1 - m_b));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Burn()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, 1 - ((1 - r) / m_r));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, 1 - ((1 - g) / m_g));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, 1 - ((1 - b) / m_b));
            }

            SetTexture(targetTexture, pixels);
        }

        private void SoftLight()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, (m_r < 0.5f) ? ((2 * r * m_r) + (Mathf.Pow(r, 2) * (1 - 2 * m_r))) : ((2 * r * (1 - m_r)) + (Mathf.Pow(r, 0.5f) * (2 * m_r - 1))));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, (m_g < 0.5f) ? ((2 * g * m_g) + (Mathf.Pow(g, 2) * (1 - 2 * m_g))) : ((2 * g * (1 - m_g)) + (Mathf.Pow(g, 0.5f) * (2 * m_g - 1))));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, (m_b < 0.5f) ? ((2 * b * m_b) + (Mathf.Pow(b, 2) * (1 - 2 * m_b))) : ((2 * b * (1 - m_b)) + (Mathf.Pow(b, 0.5f) * (2 * m_b - 1))));
            }

            SetTexture(targetTexture, pixels);
        }

        private void HardLight()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r;
                float g = pixels[i].g;
                float b = pixels[i].b;

                pixels[i].r = ApplyAlphaBlending(pixels[i].r, (m_r <= 0.5f) ? (2 * m_r * r) : (1 - 2 * (1 - m_r) * (1 - r)));
                pixels[i].g = ApplyAlphaBlending(pixels[i].g, (m_g <= 0.5f) ? (2 * m_g * g) : (1 - 2 * (1 - m_g) * (1 - g)));
                pixels[i].b = ApplyAlphaBlending(pixels[i].b, (m_b <= 0.5f) ? (2 * m_b * b) : (1 - 2 * (1 - m_b) * (1 - b)));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Hue()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float h = Hue(m_color);
                float s = Saturation(pixels[i]);
                float l = Lightness(pixels[i]);

                pixels[i] = ApplyAlphaBlending(pixels[i], HSLToRGB(h, s, l));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Saturation()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float h = Hue(pixels[i]);
                float s = Saturation(m_color);
                float l = Lightness(pixels[i]);

                pixels[i] = ApplyAlphaBlending(pixels[i], HSLToRGB(h, s, l));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Color_()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float h = Hue(m_color);
                float s = Saturation(m_color);
                float l = Lightness(pixels[i]);

                pixels[i] = ApplyAlphaBlending(pixels[i], HSLToRGB(h, s, l));
            }

            SetTexture(targetTexture, pixels);
        }

        private void Luminosity()
        {
            Texture2D targetTexture = GetCopyTexture();

            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float h = Hue(pixels[i]);
                float s = Saturation(pixels[i]);
                float l = Lightness(m_color);

                pixels[i] = ApplyAlphaBlending(pixels[i], HSLToRGB(h, s, l));
            }

            SetTexture(targetTexture, pixels);
        }

        private float Hue(Color _color) // H of HSY, HSL, HSV; Range: 0 <= H < 360. This is the type of color.
        {
            float r = _color.r;
            float g = _color.g;
            float b = _color.b;

            float max = Mathf.Max(r, g, b);
            float min = Mathf.Min(r, g, b);


            float hue;
            if ((max - min) == 0) hue = 0;
            else if (r == max) hue = 60 * ((g - b) / (max - min));
            else if (g == max) hue = 60 * (2 + (b - r) / (max - min));
            else /*if (b == max)*/ hue = 60 * (4 + (r - g) / (max - min));

            if (hue < 0)
                hue += 360;

            return hue;
        }
        private float Saturation(Color _color) // S of HSY, HSL, HSV; Range: 0 <= S <= 1. This is how far the color is from the grayscale zone (in gray scale zone == 0, in colorful zone == 1).
        {
            float r = _color.r;
            float g = _color.g;
            float b = _color.b;

            float max = Mathf.Max(r, g, b);
            float min = Mathf.Min(r, g, b);

            if (max + min == 0 || 2 - max - min == 0) return 0; // If max and min are both 0 or both 1, return 0;
            else return ((max + min) / 2 < 0.5f) ? ((max - min) / (max + min)) : ((max - min) / (2 - max - min));
        }
        private float Lightness(Color _color) // L of HSL; Range: 0 <= L <= 1. This is how far the color is from black within the grayscale zone (black == 0, white == 1).
        {
            float r = _color.r;
            float g = _color.g;
            float b = _color.b;

            float max = Mathf.Max(r, g, b);
            float min = Mathf.Min(r, g, b);

            return (max + min) / 2;
        }

        private float ApplyAlphaBlending(float _old, float _new) { return m_a * _new + (1 - m_a) * _old; }
        private Color ApplyAlphaBlending(Color _old, Color _new)
        {
            Color result = new Color();

            result.a = ApplyAlphaBlending(_old.r, _new.r);
            result.a = ApplyAlphaBlending(_old.g, _new.g);
            result.a = ApplyAlphaBlending(_old.b, _new.b);

            return result;
        }

        private Color HSLToRGB(float _hue, float _saturation, float _lightness)
        {
            if (_hue < 0 || _hue >= 360
                || _saturation < 0 || _saturation > 1
                || _lightness < 0 || _lightness > 1)
            {
                return Color.white;
            }

            float r, g, b;
            if (_saturation == 0) // It means that the color is in the grayscale
                r = g = b = _lightness;
            else
            {
                float v1, v2;
                float floatHue = _hue / 360f;

                v2 = (_lightness < 0.5f) ? (_lightness * (1 + _saturation)) : ((_lightness + _saturation) - (_lightness * _saturation));
                v1 = 2 * _lightness - v2;

                r = HueToRGB(v1, v2, floatHue + (1f / 3));
                g = HueToRGB(v1, v2, floatHue);
                b = HueToRGB(v1, v2, floatHue - (1f / 3));
            }

            return new Color(r, g, b);
        }

        private float HueToRGB(float _v1, float _v2, float _vH)
        {
            if (_vH < 0)
                _vH++;
            else if (_vH > 1)
                _vH--;

            if (_vH * 6 < 1)
                return (_v1 + (_v2 - _v1) * 6 * _vH);
            else if (_vH * 2 < 1)
                return _v2;
            else if (_vH * 3 < 2)
                return (_v1 + (_v2 - _v1) * ((2f / 3) - _vH) * 6);
            else
                return _v1;
        }

        private Texture2D GetCopyTexture()
        {
            Texture2D originalTexture = m_sprite.texture;

            // Create a new texture from the original texture
            Texture2D copy = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, false);
            copy.LoadRawTextureData(originalTexture.GetRawTextureData());

            Color[] pixels = copy.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a == 0)
                    pixels[i] = new Color(1, 1, 1, 0); // To prevent Unity's wierd behaviour of changing white pixels with 0 alpha to non-white pixels with 0 alpha.
            }
            copy.filterMode = originalTexture.filterMode;

            SetTexture(copy, pixels);

            return copy;
        }

        private void SetTexture(Texture2D _targetTexture, Color[] _pixels)
        {
            _targetTexture.SetPixels(_pixels);

            //Apply all modifications to m_targetTexture
            _targetTexture.Apply();

            m_image.sprite = ImageConverter.TextureToSprite(_targetTexture, m_sprite);
        }
    }

    public enum eBlendingMethod
    {
        None,
        Normal,
        Multiply,
        Add,
        Subtract,
        Overlay,
        Screen,
        Lighten,
        Darken,
        Difference,
        Dodge,
        Burn,
        SoftLight,
        HardLight,
        Hue,
        Saturation,
        Color,
        Luminosity
    }
}