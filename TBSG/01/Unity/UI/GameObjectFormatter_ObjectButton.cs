using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    public class GameObjectFormatter_ObjectButton : MonoBehaviour
    {
        #region Serialized Fields
        public Sprite DefaultSprite;
        #endregion

        #region Properties
        public AdvancedButton Button_Object { get; private set; }
        public Image Image_Object { get; private set; }
        public Image Image_IconFrameLeft { get; private set; }
        public Image Image_IconFrameRight { get; private set; }
        public Image Image_IconLeftHalfMask { get; private set; }
        public Image Image_IconRightHalfMask { get; private set; }
        public Image Image_IconLeftHalf { get; private set; }
        public Image Image_IconRightHalf { get; private set; }
        public Image Image_RarityFrame { get; private set; }
        public Image Image_RarityTexture { get; private set; }
        public Text Text_Value { get; private set; }
        #endregion

        #region Private Fields
        private GameObject m_go_iconLeftHalfMask;
        private GameObject m_go_iconRightHalfMask;
        #endregion

        // Awake is called before the first Update
        void Awake()
        {
            Button_Object = this.GetComponent<AdvancedButton>();
            Image_Object = this.transform.Find("Image@Object").GetComponent<Image>();
            Image_IconFrameLeft = this.transform.Find("Image@IconFrameLeft").GetComponent<Image>();
            Image_IconFrameRight = this.transform.Find("Image@IconFrameRight").GetComponent<Image>();
            m_go_iconLeftHalfMask = this.transform.Find("Image@IconLeftHalf").gameObject;
            m_go_iconRightHalfMask = this.transform.Find("Image@IconRightHalf").gameObject;
            Image_IconLeftHalf = m_go_iconLeftHalfMask.transform.Find("Image@Icon").GetComponent<Image>();
            Image_IconRightHalf = m_go_iconRightHalfMask.transform.Find("Image@Icon").GetComponent<Image>();
            Transform transform_rarityFrameImage = this.transform.Find("Image@RarityFrame");
            Image_RarityFrame = transform_rarityFrameImage.GetComponent<Image>();
            Image_RarityTexture = transform_rarityFrameImage.Find("Image@Texture").GetComponent<Image>();
            Text_Value = this.transform.Find("Text@Value").GetComponent<Text>();
        }

        public void Format(object _object, string _valueTextString = null, UnityAction _buttonClickAction = null, UnityAction _buttonLongPressAction = null)
        {
            if (_object != null)
            {
                Image_RarityFrame.gameObject.SetActive(true);

                eRarity rarity;

                if (_object is UnitData || _object is Unit)
                {
                    Image_RarityFrame.sprite = SpriteContainer.Instance.ObjectIconFrameSprite_Unit;
                    m_go_iconLeftHalfMask.SetActive(true);
                    m_go_iconRightHalfMask.SetActive(true);
                    Image_IconFrameLeft.gameObject.SetActive(true);
                    Image_IconFrameRight.gameObject.SetActive(true);

                    UnitData unitData = (_object is UnitData) ? (_object as UnitData) : (_object as Unit).BaseInfo;

                    Image_Object.sprite = SpriteContainer.Instance.UnitIcons[unitData];
                    rarity = unitData.Rarity;

                    List<Sprite> sprites_elementIcon = SpriteContainer.Instance.ElementIconsForUnit(unitData);
                    Image_IconLeftHalf.sprite = sprites_elementIcon[0];
                    Image_IconRightHalf.sprite = sprites_elementIcon[1];

                    List<Sprite> sprites_elementFrame = SpriteContainer.Instance.ElementFrameSpritesForUnit(unitData);
                    Image_IconFrameLeft.sprite = sprites_elementFrame[0];
                    Image_IconFrameRight.sprite = sprites_elementFrame[1];
                }
                else
                {
                    Image_RarityFrame.sprite = SpriteContainer.Instance.ObjectIconFrameSprite_NonUnit;

                    m_go_iconLeftHalfMask.SetActive(false);
                    m_go_iconRightHalfMask.SetActive(false);
                    Image_IconFrameLeft.gameObject.SetActive(false);
                    Image_IconFrameRight.gameObject.SetActive(false);

                    if (_object is Weapon)
                    {
                        Weapon weapon = _object as Weapon;
                        Image_Object.sprite = SpriteContainer.Instance.WeaponIcons[weapon.BaseInfo];
                        rarity = weapon.BaseInfo.Rarity;
                    }
                    else if (_object is Armour)
                    {
                        Armour armour = _object as Armour;
                        Image_Object.sprite = SpriteContainer.Instance.ArmourIcons[armour.BaseInfo];
                        rarity = armour.BaseInfo.Rarity;
                    }
                    else if (_object is Accessory)
                    {
                        Accessory accessory = _object as Accessory;
                        Image_Object.sprite = SpriteContainer.Instance.AccessoryIcons[accessory.BaseInfo];
                        rarity = accessory.BaseInfo.Rarity;
                    }
                    else if (_object is Item)
                    {
                        Item item = _object as Item;
                        Image_Object.sprite = SpriteContainer.Instance.ItemIcons[item];
                        rarity = item.Rarity;
                    }
                    else
                        return;
                }

                Image_RarityTexture.sprite = SpriteContainer.Instance.GetRaritySprite(rarity);

                if (_valueTextString != null)
                    Text_Value.text = _valueTextString;
            }
            else
            {
                Image_RarityFrame.gameObject.SetActive(false);
                m_go_iconLeftHalfMask.SetActive(false);
                m_go_iconRightHalfMask.SetActive(false);
                Image_IconFrameLeft.gameObject.SetActive(false);
                Image_IconFrameRight.gameObject.SetActive(false);

                Image_Object.sprite = DefaultSprite;
            }

            if (_buttonClickAction != null)
                Button_Object.OnClick.AddListener(_buttonClickAction);

            if (_buttonLongPressAction != null)
            {
                Button_Object.EnableLongPress = true;
                Button_Object.OnLongPress.AddListener(_buttonLongPressAction);
            }

            Button_Object.interactable = (_buttonClickAction != null || _buttonLongPressAction != null);
        }
    }
}
