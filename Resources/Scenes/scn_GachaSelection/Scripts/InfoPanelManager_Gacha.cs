using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(InfoPanelManager))]
    public class InfoPanelManager_Gacha : MonoBehaviour
    {

        [SerializeField]
        public GameObject GachaInfoPanelPrefab;

        private InfoPanelManager m_mainInformationPanelManager;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_mainInformationPanelManager = this.transform.GetComponent<InfoPanelManager>();
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
        }

        public void InstantiateInfoPanel(int _gachaPageNumber)
        {
            GameObject go_infoPanel = Instantiate(GachaInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("BackGround").Find("CloseButton").GetComponent<Button>().onClick.AddListener(() => RemoveInfoPanel());
            //RectTransform rt = go_infoPanel.GetComponent<RectTransform>();
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInformationPanelManager.Request_AddInfoPanel(go_infoPanel);

            Unit unitData = GameDataContainer.Instance.Player.UnitsOwned.First(x => x.UniqueId == _gachaPageNumber);

            UpdateInfoPanel(go_infoPanel, unitData);
        }

        private void UpdateInfoPanel(GameObject _go_infoPanel, Unit _unitData)
        {
            Transform infoPanelBG = _go_infoPanel.transform.Find("BackGround");

            //infoPanelBG.Find("UnitNickname").GetComponent<Text>().text = _unitData.Nickname;
        }

        //Remove newest info panel
        public void RemoveInfoPanel()
        {
            GameObject go_infoPanel = m_gos_infoPanel[m_gos_infoPanel.Count - 1];
            m_gos_infoPanel.Remove(go_infoPanel);
            m_mainInformationPanelManager.Request_RemoveInfoPanel(go_infoPanel);
        }
    }
}