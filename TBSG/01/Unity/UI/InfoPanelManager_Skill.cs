using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    [RequireComponent(typeof(InfoPanelManager))]
    [RequireComponent(typeof(InfoPanelManager_Item))]
    public class InfoPanelManager_Skill : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_skillInfoPanelPrefab;

        private InfoPanelManager m_mainInfoPanelManager;
        private InfoPanelManager_Item m_infoPanelManager_item;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;

        private GameObject m_objectButtonPrefab;
        #endregion

        //Use this for initialization

        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_skillInfoPanelPrefab = InfoPanelPrefabContainer.Instance.SkillInfoPanelPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
            m_infoPanelManager_item = this.GetComponent<InfoPanelManager_Item>();
            m_objectButtonPrefab = SharedAssetContainer.Instance.ObjectButtonPrefab;
        }


        public void InstantiateInfoPanel(Skill _skill)
        {
            GameObject go_infoPanel = Instantiate(m_skillInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _skill);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Skill _skill)
        {
            try
            {
                Transform transform_infoPanel = _go_infoPanel.transform;
                transform_infoPanel.Find("Image@SkillIcon").GetComponent<Image>().sprite = SpriteContainer.Instance.GetSkillIcon(_skill);
                transform_infoPanel.Find("Text@SkillName").GetComponent<Text>().text = _skill.BaseInfo.Name;
                transform_infoPanel.Find("Level").Find("Text@Value").GetComponent<Text>().text = (_skill.Level < CoreValues.MAX_SKILL_LEVEL) ? _skill.Level.ToString() : "MAX";

                Text text_classification = transform_infoPanel.Find("Classification").Find("Text@Value").GetComponent<Text>();
                if (_skill is OrdinarySkill)
                    text_classification.text = "<color=red>Ordinary</color>";
                else if (_skill is CounterSkill)
                    text_classification.text = "<color=yellow>Counter</color>";
                else if (_skill is UltimateSkill)
                    text_classification.text = "<color=purple>Ultimate</color>";
                else // if (_skill is PassiveSkill)
                    text_classification.text = "<color=blue>Passive</color>";


                Transform transform_spRequired = transform_infoPanel.Find("SPRequired");
                Transform transform_itemRequired = transform_infoPanel.Find("ItemsRequired");
                if (_skill is CostRequiringSkill)
                {
                    CostRequiringSkill crs = _skill as CostRequiringSkill;

                    transform_spRequired.Find("Text@Value").GetComponent<Text>().text = crs.BaseInfo.SPCost.ToString();

                    if (crs.BaseInfo.ItemCosts.Count == 0)
                        transform_itemRequired.Find("Text@None").gameObject.SetActive(true);
                    else
                    {
                        Transform transform_items = transform_itemRequired.Find("Items");
                        foreach (var itemCost in crs.BaseInfo.ItemCosts)
                        {
                            GameObject go_itemButton = Instantiate(m_objectButtonPrefab, transform_items);
                            GameObjectFormatter_ObjectButton goFormatter_itemButton = go_itemButton.GetComponent<GameObjectFormatter_ObjectButton>();
                            Item item = GameDataContainer.Instance.ItemEncyclopedia.First(x => x.Id == itemCost.Key.Id);
                            goFormatter_itemButton.Format(item, itemCost.Value.ToString(), null, () => m_infoPanelManager_item.InstantiateInfoPanel(item));
                        }
                    }

                    transform_spRequired.gameObject.SetActive(true);
                    transform_itemRequired.gameObject.SetActive(true);
                }
                else
                {
                    transform_spRequired.gameObject.SetActive(false);
                    transform_itemRequired.gameObject.SetActive(false);
                }

                //transform_contents.Find("ScrollMenu@Details").Find("Contents").Find("Text").GetComponent<Text>().text = _skill.BaseInfo.Explanation;
                transform_infoPanel.Find("ScrollMenu@Details").Find("Contents").Find("Text").GetComponent<Text>().text = "[Effects Explanation]\nTo be implemented...";
            }
            catch (Exception ex)
            {
                Debug.Log("InfoPanelManager_Skill_SinglePlayer.InitializeInfoPanel() : " + ex.Message);
            }
        }

        //Remove newest info panel
        public void RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            m_mainInfoPanelManager.Request_RemoveInfoPanel(_go_infoPanel);
        }
    }
}