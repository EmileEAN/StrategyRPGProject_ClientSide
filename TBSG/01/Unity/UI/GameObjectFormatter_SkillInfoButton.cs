using EEANWorks.Games.TBSG._01.Unity.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    public class GameObjectFormatter_SkillInfoButton : MonoBehaviour
    {
        #region Private Fields
        private Image m_image_buttonBackground;
        private Button m_button_skill;
        private Image m_image_icon;
        private Image m_image_iconFrame;
        private Text m_text_name;
        private Text m_text_level;

        private Color m_color_emptySkillButton = new Color32(200, 200, 200, 255);
        private Color m_color_ordinarySkillButton = Color.red;
        private Color m_color_counterSkillButton = new Color32(255, 220, 0, 255);
        private Color m_color_ultimateSkillButton = new Color32(220, 0, 255, 255);
        private Color m_color_passiveSkillButton = new Color32(0, 220, 225, 255);
        #endregion

        // Awake is called before the first Update
        void Awake()
        {
            m_image_buttonBackground = this.GetComponent<Image>();
            m_button_skill = this.GetComponent<Button>();
            Transform transform_iconImages = this.transform.Find("IconImages");
            m_image_icon = transform_iconImages.Find("Image@Icon").GetComponent<Image>();
            m_image_iconFrame = transform_iconImages.Find("Image@Frame").GetComponent<Image>();
            m_text_name = this.transform.Find("Text@Name").GetComponent<Text>();
            m_text_level = this.transform.Find("Text@Level").GetComponent<Text>();
            Debug.Log("Awaked");
        }

        public void Format(Skill _skill, UnityAction _pressAction)
        {
            Debug.Log("Formatting");
            if (_skill != null)
            {
                if (_skill is OrdinarySkill)
                {
                    m_image_buttonBackground.color = m_color_ordinarySkillButton;
                    m_image_iconFrame.sprite = SpriteContainer.Instance.SkillIconBaseSprites[1];
                }
                else if (_skill is CounterSkill)
                {
                    m_image_buttonBackground.color = m_color_counterSkillButton;
                    m_image_iconFrame.sprite = SpriteContainer.Instance.SkillIconBaseSprites[2];
                }
                else if (_skill is UltimateSkill)
                {
                    m_image_buttonBackground.color = m_color_ultimateSkillButton;
                    m_image_iconFrame.sprite = SpriteContainer.Instance.SkillIconBaseSprites[3];
                }
                else // if (Skill is PassiveSkill)
                {
                    m_image_buttonBackground.color = m_color_passiveSkillButton;
                    m_image_iconFrame.sprite = SpriteContainer.Instance.SkillIconBaseSprites[4];
                }

                m_image_icon.sprite = SpriteContainer.Instance.GetSkillIcon(_skill);

                m_text_name.text = _skill.BaseInfo.Name;
                m_text_level.text = _skill.Level.ToString();

                m_button_skill.onClick.AddListener(_pressAction);
                m_button_skill.interactable = true;
            }
            else
            {
                m_image_buttonBackground.color = m_color_emptySkillButton;
                m_image_icon.sprite = SpriteContainer.Instance.EmptySkillSlotIcon;
                m_image_iconFrame.sprite = SpriteContainer.Instance.SkillIconBaseSprites[0];
                m_text_name.text = "- - - -";
                m_text_level.text = "-";

                m_button_skill.interactable = false;
            }
        }
    }
}