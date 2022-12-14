using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine.UI
{
    public class InfoPanelManager : MonoBehaviour
    {
        #region Properties
        public List<GameObject> InfoPanels { get; private set; }
        #endregion

        #region Private Fields
        private Transform m_transform_canvas;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            InfoPanels = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
        }

        public void Request_AddInfoPanel(GameObject _go_infoPanel) { StartCoroutine(AddInfoPanel(_go_infoPanel)); }
        IEnumerator AddInfoPanel(GameObject _go_infoPanel)
        {
            if (_go_infoPanel != null)
            {
                if (InfoPanels.Count > 0)
                    yield return StartCoroutine(InfoPanels.Last().SetActiveAfterOneFrame(false)); // Deactivate newest info panel

                InfoPanels.Add(_go_infoPanel);
            }
        }

        public void Request_RemoveInfoPanel(GameObject _go_infoPanel) { StartCoroutine(RemoveInfoPanel(_go_infoPanel)); }
        public IEnumerator RemoveInfoPanel(GameObject _go_infoPanel)
        {
            if (InfoPanels.Exists(x => x == _go_infoPanel))
            {
                if (InfoPanels.Count > 0)
                    yield return StartCoroutine(InfoPanels.Last().SetActiveAfterOneFrame(true)); // Activate newest info panel

                InfoPanels.Remove(_go_infoPanel);
                if (InfoPanels.Count > 0)
                    yield return StartCoroutine(InfoPanels.Last().SetActiveAfterOneFrame(true));
                Destroy(_go_infoPanel);
            }
        }
    }
}