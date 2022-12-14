using EEANWorks.Games.TBSG._01.Unity.Data;
using System;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class SPDisplayer_SinglePlayer : MonoBehaviour
    {
        #region Private Fields
        private bool m_isInitialized;
        private UnityBattleSystem_SinglePlayer m_mainScript;

        private MeshRenderer[][] m_meshRenderers_spObject;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_isInitialized = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_SinglePlayer>();

                if (m_mainScript.IsInitialized
                    && !m_isInitialized)
                {
                    m_meshRenderers_spObject = new MeshRenderer[m_mainScript.PlayerControllers.Count][];

                    for (int i = 0; i < m_mainScript.PlayerControllers.Count; i++)
                    {
                        if (m_mainScript.PlayerControllers[i].IsInitialized)
                        {
                            m_meshRenderers_spObject[i] = new MeshRenderer[CoreValues.MAX_SP];
                            for (int j = 0; j < m_meshRenderers_spObject[i].Length; j++)
                            {
                                m_meshRenderers_spObject[i][j] = this.transform.Find("SkillStone" + m_mainScript.PlayerControllers[i].PlayerId.ToString() + "_" + (j + 1).ToString()).GetComponent<MeshRenderer>();
                            }

                            UpdateSPGraphic(i, (m_mainScript.PlayerControllers[i].PlayerId == 1) ? 1 : 0); // Each player starts with 1 SP at the beginning of her/his first turn. In other words, the Player 2 will have 0 SP during Player 1's first turn.

                            m_isInitialized = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("SPTextManager: at Initialize() " + ex.Message);
            }
        }

        public void UpdateSPGraphic(int _playerIndex, int _remainingSP)
        {
            for (int i = 0; i < _remainingSP; i++)
            {
                m_meshRenderers_spObject[_playerIndex][i].material = BattleSceneAssetContainer.Instance.Material_ActiveStone;
            }

            for (int i = _remainingSP; i < m_meshRenderers_spObject[_playerIndex].Length; i++)
            {
                m_meshRenderers_spObject[_playerIndex][i].material = BattleSceneAssetContainer.Instance.Material_InactiveStone;
            }
        }
    }
}