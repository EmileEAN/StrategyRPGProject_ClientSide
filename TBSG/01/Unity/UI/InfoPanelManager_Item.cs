using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    [RequireComponent(typeof(InfoPanelManager))]
    [RequireComponent(typeof(InfoPanelManager_Skill))]
    public class InfoPanelManager_Item : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_itemInfoPanelPrefab;
        private GameObject m_skillInfoButtonPrefab;

        private InfoPanelManager m_mainInfoPanelManager;
        private InfoPanelManager_Skill m_infoPanelManager_skill;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;

        private Image m_image_item;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_itemInfoPanelPrefab = InfoPanelPrefabContainer.Instance.ItemInfoPanelPrefab;
            m_skillInfoButtonPrefab = InfoPanelPrefabContainer.Instance.InfoPanelSkillInfoButtonPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
            m_infoPanelManager_skill = this.GetComponent<InfoPanelManager_Skill>();
        }

        public void InstantiateInfoPanel(int _itemId)
        {
            Item item = GameDataContainer.Instance.ItemEncyclopedia.First(x => x.Id == _itemId);

            InstantiateInfoPanel(item);
        }
        public void InstantiateInfoPanel(Item _item)
        {
            GameObject go_infoPanel = Instantiate(m_itemInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => Request_RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _item);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Item _item)
        {
            //-----------------------------------------------------------------------------
            // Initialize Left Section
            //-----------------------------------------------------------------------------
            Transform transform_leftSection = _go_infoPanel.transform.Find("LeftSection");

            transform_leftSection.Find("Panel@Name").Find("Text@Name").GetComponent<Text>().text = _item.Name;

            Transform transform_images_item = transform_leftSection.Find("ItemImagesContainer").Find("ItemImages");
            m_image_item = transform_images_item.Find("Image@Item").GetComponent<Image>();
            m_image_item.sprite = SpriteContainer.Instance.ItemIcons[_item];
            m_image_item.sprite.texture.filterMode = FilterMode.Point;
            transform_images_item.Find("Image@RarityFrame").Find("Image@Texture").GetComponent<Image>().sprite = SpriteContainer.Instance.GetRaritySprite(_item);

            //-----------------------------------------------------------------------------
            // Initialize Right Section
            //-----------------------------------------------------------------------------
            Transform transform_rightSection = _go_infoPanel.transform.Find("RightSection");

            string classificationText = string.Empty;
            {
                if (_item is SkillItem) classificationText = "Skill Item";
                else if (_item is SkillMaterial) classificationText = "Skill Material";
                else if (_item is ItemMaterial) classificationText = "Item Material";
                else if (_item is EquipmentMaterial) classificationText = "Equipment Material";
                else if (_item is EvolutionMaterial) classificationText = "Evolution Material";
                else if (_item is WeaponEnhancementMaterial) classificationText = "Weapon Enhancement Material";
                else if (_item is UnitEnhancementMaterial) classificationText = "Unit Enhancement Material";
                else if (_item is SkillEnhancementMaterial) classificationText = "Skill Enhancement Material";
                else if (_item is GachaCostItem) classificationText = "Gacha Cost Item";
                else if (_item is EquipmentTradingItem) classificationText = "Equipment Trading Item";
                else /*if (_item is UnitTradingItem)*/ classificationText = "Unit Trading Item";

                // Set color tags for rich text based on classification
                string colorString = string.Empty;
                if (_item is BattleItem) colorString = "cyan";
                else if (_item is EnhancementMaterial) colorString = "green";
                else if (_item is GachaCostItem || _item is EquipmentTradingItem || _item is UnitTradingItem) colorString = "yellow";
                else /*if _item is some other type of material*/ colorString = "brown";

                classificationText = "<color=" + colorString + ">" + classificationText + "</color>";
            }
            transform_rightSection.Find("Classification").Find("Text@Value").GetComponent<Text>().text = classificationText;

            transform_rightSection.Find("SellingPrice").Find("Text@Value").GetComponent<Text>().text = string.Format("{0:#,0}", _item.SellingPrice);

            if (_item is SkillItem)
            {
                ActiveSkill skill = (_item as SkillItem).Skill;
                Transform transform_skill = transform_rightSection.Find("Skill");
                transform_skill.gameObject.SetActive(true);
                transform_skill.Find("Button@SkillInfo").GetComponent<GameObjectFormatter_SkillInfoButton>().Format(skill, () => m_infoPanelManager_skill.InstantiateInfoPanel(skill));
            }
            else if (_item is EnhancementMaterial)
            {
                int enhancementValue = (_item as EnhancementMaterial).EnhancementValue;

                if (_item is WeaponEnhancementMaterial || _item is UnitEnhancementMaterial)
                {
                    Transform transform_obtainableEXP = transform_rightSection.Find("ObtainableEXP");
                    transform_obtainableEXP.Find("Text@Value").GetComponent<Text>().text = enhancementValue.ToString();
                    transform_obtainableEXP.gameObject.SetActive(true);

                    if (_item is WeaponEnhancementMaterial)
                    {
                        WeaponEnhancementMaterial weaponEnhancementMaterial = _item as WeaponEnhancementMaterial;

                        Transform transform_targetingWeaponClassification = transform_rightSection.Find("TargetingWeaponClassification");
                        Transform transform_weaponClassificationIcons = transform_targetingWeaponClassification.Find("Icons");
                        int numOfWeaponClassifications = weaponEnhancementMaterial.TargetingWeaponClassifications.Count;
                        if (numOfWeaponClassifications == 0)
                            transform_targetingWeaponClassification.Find("Text@All").gameObject.SetActive(true);
                        else
                        {
                            for (int i = 0; i < transform_weaponClassificationIcons.childCount; i++)
                            {
                                Transform transform_weaponClassificationIcon = transform_weaponClassificationIcons.GetChild(i);
                                if (i < numOfWeaponClassifications)
                                {
                                    eWeaponClassification classification = weaponEnhancementMaterial.TargetingWeaponClassifications[i];

                                    transform_weaponClassificationIcon.GetComponent<DetailInfoPopUpController>().GO_detailInfoPopUp.transform.Find("Text").GetComponent<Text>().text = classification.ToString();

                                    Image image_weaponClassification = transform_weaponClassificationIcon.GetComponent<Image>();
                                    image_weaponClassification.sprite = SpriteContainer.Instance.GetWeaponClassificationIcon(classification);

                                    transform_weaponClassificationIcon.gameObject.SetActive(true);
                                }
                            }
                        }
                        transform_targetingWeaponClassification.gameObject.SetActive(true);
                    }
                    else /*if (_item is UnitEnhancementMaterial)*/
                    {
                        UnitEnhancementMaterial unitEnhancementMaterial = _item as UnitEnhancementMaterial;

                        Transform transform_bonusElement = transform_rightSection.Find("BonusElement");
                        Transform transform_elementIcons = transform_bonusElement.Find("Icons");
                        for (int i = 0; i < transform_elementIcons.childCount; i++)
                        {
                            Transform transform_elementIcon = transform_elementIcons.GetChild(i);
                            if (unitEnhancementMaterial.BonusElements.Contains((eElement)(i + 1)))
                                transform_elementIcon.gameObject.SetActive(true);
                        }
                        transform_bonusElement.gameObject.SetActive(true);
                    }
                }
                else /*if (_item is SkillEnhancementMaterial)*/
                {
                    SkillEnhancementMaterial skillEnhancementMaterial = _item as SkillEnhancementMaterial;

                    Transform transform_increasingSkillLevel = transform_rightSection.Find("IncreasingSkillLevel");
                    transform_increasingSkillLevel.Find("Text@Value").GetComponent<Text>().text = enhancementValue.ToString();
                    transform_increasingSkillLevel.gameObject.SetActive(true);

                    Transform transform_target = transform_rightSection.Find("Target");
                    Transform transform_targetPanel = transform_target.Find("Panel@Target");
                    Transform transform_rarityIcons = transform_targetPanel.Find("TargetingRarities").Find("Icons");
                    for (int i = 0; i < transform_rarityIcons.childCount; i++)
                    {
                        Transform transform_rarityIcon = transform_rarityIcons.GetChild(i);
                        if (skillEnhancementMaterial.TargetingRarities.Contains((eRarity)i))
                            transform_rarityIcon.gameObject.SetActive(true);
                    }
                    Transform transform_targetingElements = transform_targetPanel.Find("TargetingElements");
                    Transform transform_elementIcons = transform_targetingElements.Find("Icons");
                    int numOfElements = skillEnhancementMaterial.TargetingElements.Count;
                    if (numOfElements == 0)
                        transform_targetingElements.Find("Text@All").gameObject.SetActive(true);
                    else
                    {
                        for (int i = 0; i < transform_elementIcons.childCount; i++)
                        {
                            Transform transform_elementIcon = transform_elementIcons.GetChild(i);
                            if (skillEnhancementMaterial.TargetingElements.Contains((eElement)(i + 1)))
                                transform_elementIcon.gameObject.SetActive(true);
                        }
                    }
                    Text text_targetingLabels = transform_targetPanel.Find("TargetingLabels").Find("Text@Value").GetComponent<Text>();
                    for (int i = 0; i < skillEnhancementMaterial.TargetingLabels.Count; i++)
                    {
                        if (i > 0)
                            text_targetingLabels.text += " ";

                        text_targetingLabels.text += "<" + skillEnhancementMaterial.TargetingLabels[i] + ">";
                    }
                }
            }
        }

        //Remove newest info panel
        private void Request_RemoveInfoPanel(GameObject _go_infoPanel) { StartCoroutine(RemoveInfoPanel(_go_infoPanel)); }
        IEnumerator RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            yield return StartCoroutine(m_mainInfoPanelManager.RemoveInfoPanel(_go_infoPanel));
            m_image_item.sprite.texture.filterMode = FilterMode.Bilinear;
        }
    }
}