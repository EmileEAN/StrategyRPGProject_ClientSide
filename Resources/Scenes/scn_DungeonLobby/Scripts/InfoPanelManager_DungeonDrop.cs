using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Globalization;
using UnityEngine.Events;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    [RequireComponent(typeof(InfoPanelManager))]
    [RequireComponent(typeof(InfoPanelManager_Item))]
    public class InfoPanelManager_DungeonDrop : MonoBehaviour
    {
        #region Public Fields
        public GameObject DungeonDropInfoPanelPrefab;
        public GameObject DungeonDropInfoPrefab;
        #endregion

        #region Private Fields
        private InfoPanelManager m_mainInfoPanelManager;
        private InfoPanelManager_Item m_infoPanelManager_item;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
            m_infoPanelManager_item = this.GetComponent<InfoPanelManager_Item>();
        }

        public void InstantiateInfoPanel(Dungeon _dungeon)
        {
            GameObject go_infoPanel = Instantiate(DungeonDropInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => Request_RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _dungeon);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Dungeon _dungeon)
        {
            Transform transform_contents = _go_infoPanel.transform.Find("ScrollMenuContainer").Find("ScrollMenu@DungeonDropInfo").Find("Contents");

            for (int i = 0; i < _dungeon.Floors.Count; i++)
            {
                foreach (DungeonUnitInfo dungeonUnitInfo in _dungeon.Floors[i].DungeonUnitInfos)
                {
                    Transform transform_dungeonDropInfo = Instantiate(DungeonDropInfoPrefab, transform_contents).transform;

                    GameObjectFormatter_ObjectButton goFormatter_unitButton = transform_dungeonDropInfo.Find("UnitButtonContainer").Find("Button@Object").GetComponent<GameObjectFormatter_ObjectButton>();
                    goFormatter_unitButton.Format(dungeonUnitInfo.UnitData, dungeonUnitInfo.DropRate.ToString("P", CultureInfo.InvariantCulture));

                    transform_dungeonDropInfo.Find("Text@LevelRange").GetComponent<Text>().text = "Lv. " + dungeonUnitInfo.MinLevel.ToString() + " ~ " + dungeonUnitInfo.MaxLevel.ToString();

                    transform_dungeonDropInfo.Find("Text@Floor").GetComponent<Text>().text = (i + 1).ToString() + "F";

                    Transform transform_dropItemsContainer = transform_dungeonDropInfo.Find("DropItemsContainer");
                    foreach (DropItemInfo dropItemInfo in dungeonUnitInfo.DropItemInfos)
                    {
                        GameObject go_dropItemInfo = Instantiate(SharedAssetContainer.Instance.ObjectButtonPrefab, transform_dropItemsContainer);
                        GameObjectFormatter_ObjectButton goFormatter_itemButton = go_dropItemInfo.GetComponent<GameObjectFormatter_ObjectButton>();
                        UnityAction buttonClickAction = new UnityAction(() => m_infoPanelManager_item.InstantiateInfoPanel(dropItemInfo.Item));
                        goFormatter_itemButton.Format(dropItemInfo.Item, dropItemInfo.DropRate.ToString("P", CultureInfo.InvariantCulture), buttonClickAction);
                        goFormatter_itemButton.Text_Value.fontSize = 30;
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
        }
    }
}