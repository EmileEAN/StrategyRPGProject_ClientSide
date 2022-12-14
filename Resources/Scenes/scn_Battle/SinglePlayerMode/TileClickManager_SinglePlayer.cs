using EEANWorks.Games.Unity.Engine.UI;
using System;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(MeshCollider))]
    public class TileClickManager_SinglePlayer : MonoBehaviour
    {

        #region Private Fields
        private TileMaskManager_SinglePlayer m_tileMaskManager;

        private InfoPanelManager m_mainInfoPanelManager;

        private bool m_isInitialized;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_isInitialized = false;

            m_tileMaskManager = GameObject.FindGameObjectWithTag("GameBoard").GetComponent<TileMaskManager_SinglePlayer>();

            m_mainInfoPanelManager = this.transform.root.GetComponent<InfoPanelManager>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();
        }

        private void Initialize()
        {
            if (m_tileMaskManager.IsInitialized)
                m_isInitialized = true;
        }

        private void OnMouseDown()
        {
            try
            {
                if (m_isInitialized)
                {
                    if (m_mainInfoPanelManager.InfoPanels.Count < 1) // If there is no info panel that must block the tile click functionality
                    {
                        string tileIdString = this.name.Remove(0, 4); //Remove "Mask" from the name of this gameobject.
                        int tileId = Convert.ToInt32(tileIdString);
                        Debug.Log("Tile" + tileIdString + " Clicked");
                        m_tileMaskManager.ChangeTileSelection(tileId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("TileClickManager.OnMouseDown() : " + ex.Message);
            }
        }
    }
}