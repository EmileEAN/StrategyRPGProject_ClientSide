using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    [RequireComponent(typeof(InfoPanelManager))]
    [RequireComponent(typeof(InfoPanelManager_Skill))]
    public class InfoPanelManager_UnitSkills : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_unitSkillsInfoPanelPrefab;

        private InfoPanelManager m_mainInfoPanelManager;
        private InfoPanelManager_Skill m_infoPanelManager_skill;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_unitSkillsInfoPanelPrefab = InfoPanelPrefabContainer.Instance.UnitSkillsInfoPanelPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
            m_infoPanelManager_skill = this.GetComponent<InfoPanelManager_Skill>();
        }

        public void InstantiateInfoPanel(int _unitUniqueId)
        {
            Unit unit = GameDataContainer.Instance.Player.UnitsOwned.First(x => x.UniqueId == _unitUniqueId);

            InstantiateInfoPanel(unit);
        }
        public void InstantiateInfoPanel(Unit _unit)
        {
            GameObject go_infoPanel = Instantiate(m_unitSkillsInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _unit);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Unit _unit)
        {
            Transform transform_skills = _go_infoPanel.transform.Find("Skills").Find("Panel@Skills");
            int numberOfSkills = _unit.Skills.Count;
            int skillIndex = 0;
            foreach (Transform child in transform_skills)
            {
                Skill skill = (skillIndex < numberOfSkills) ? _unit.Skills[skillIndex] : null;
                child.GetComponent<GameObjectFormatter_SkillInfoButton>().Format(skill, () => m_infoPanelManager_skill.InstantiateInfoPanel(skill));

                skillIndex++;
            }
            Skill mainWeaponSkill = _unit.MainWeapon?.BaseInfo.MainWeaponSkill;
            _go_infoPanel.transform.Find("MainWeaponSkill").Find("Button@SkillInfo").GetComponent<GameObjectFormatter_SkillInfoButton>().Format(mainWeaponSkill, () => m_infoPanelManager_skill.InstantiateInfoPanel(mainWeaponSkill));
            Skill inheritedSkill = _unit.SkillInheritor?.Skills.First(x => x.BaseInfo.Id == _unit.InheritingSkillId);
            _go_infoPanel.transform.Find("InheritedSkill").Find("Button@SkillInfo").GetComponent<GameObjectFormatter_SkillInfoButton>().Format(inheritedSkill, () => m_infoPanelManager_skill.InstantiateInfoPanel(inheritedSkill));
        }

        //Remove newest info panel
        public void RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            m_mainInfoPanelManager.Request_RemoveInfoPanel(_go_infoPanel);
        }
    }
}