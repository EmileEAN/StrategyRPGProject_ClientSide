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
    public class InfoPanelManager_UnitActionRanges : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_unitActionRangeInfoPanelPrefab;

        private InfoPanelManager m_mainInfoPanelManager;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_unitActionRangeInfoPanelPrefab = InfoPanelPrefabContainer.Instance.UnitActionRangesInfoPanelPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
        }

        public void InstantiateInfoPanel(int _unitUniqueId)
        {
            Unit unit = GameDataContainer.Instance.Player.UnitsOwned.First(x => x.UniqueId == _unitUniqueId);

            InstantiateInfoPanel(unit);
        }
        public void InstantiateInfoPanel(Unit _unit)
        {
            GameObject go_infoPanel = Instantiate(m_unitActionRangeInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _unit);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Unit _unit)
        {
            _go_infoPanel.transform.Find("TargetAreaRepresentation@MovementRange").GetComponent<TargetAreaDisplayer>().DisplayTargetArea(_unit.BaseInfo.MovementRangeClassification);
            _go_infoPanel.transform.Find("TargetAreaRepresentation@NonMovementActionRange").GetComponent<TargetAreaDisplayer>().DisplayTargetArea(_unit.BaseInfo.NonMovementActionRangeClassification);
        }

        //Remove newest info panel
        public void RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            m_mainInfoPanelManager.Request_RemoveInfoPanel(_go_infoPanel);
        }
    }
}