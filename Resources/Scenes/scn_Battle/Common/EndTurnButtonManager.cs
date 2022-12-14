using EEANWorks.Games.Unity.Engine;
using System;
using System.Linq;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class EndTurnButtonManager : MonoBehaviour
    {

        #region Private Fields
        private bool m_isInitialized;

        private GameObject m_go_endTurnButton;


        private UnityBattleSystem_SinglePlayer m_mainScript_singlePlayer;
        private UnityBattleSystem_Multiplayer m_mainScript_multiplayer;

        private PlayerController_SinglePlayer m_playerController_singlePlayer;
        private PlayerController_Multiplayer m_playerController_multiplayer;

        private bool m_isSinglePlayerMode;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_isInitialized = false;
            m_go_endTurnButton = this.transform.Find("Button@EndTurn").gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();
            else
            {
                if (!m_go_endTurnButton.IsSettingActive())
                {
                    if (m_isSinglePlayerMode)
                    {
                        if (m_playerController_singlePlayer.IsMyTurn && !m_go_endTurnButton.activeSelf)
                            StartCoroutine(m_go_endTurnButton.SetActiveAfterOneFrame(true));
                        else if (!m_playerController_singlePlayer.IsMyTurn && m_go_endTurnButton.activeSelf)
                            StartCoroutine(m_go_endTurnButton.SetActiveAfterOneFrame(false));
                    }
                    else
                    {
                        if (m_playerController_multiplayer.IsMyTurn && !m_go_endTurnButton.activeSelf)
                            StartCoroutine(m_go_endTurnButton.SetActiveAfterOneFrame(true));
                        else if (!m_playerController_multiplayer.IsMyTurn && m_go_endTurnButton.activeSelf)
                            StartCoroutine(m_go_endTurnButton.SetActiveAfterOneFrame(false));
                    }
                }
            }
        }

        private void Initialize()
        {
            try
            {
                if (m_mainScript_singlePlayer == null)
                    m_mainScript_singlePlayer = this.transform.root.GetComponent<UnityBattleSystem_SinglePlayer>();

                if (m_mainScript_multiplayer == null)
                    m_mainScript_multiplayer = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();

                if (m_mainScript_singlePlayer == null && m_mainScript_multiplayer == null)
                    return;

                if (m_mainScript_singlePlayer != null && m_mainScript_singlePlayer.IsInitialized)
                {
                    m_playerController_singlePlayer = m_mainScript_singlePlayer.PlayerControllers.First(x => !x.IsCPU);
                    if (m_playerController_singlePlayer == null)
                        return;

                    m_isSinglePlayerMode = true;
                    m_isInitialized = true;
                }
                else if (m_playerController_multiplayer != null && m_mainScript_multiplayer.IsInitialized)
                {
                    m_playerController_multiplayer = m_mainScript_multiplayer.PlayerController;
                    if (m_playerController_multiplayer == null)
                        return;

                    m_isSinglePlayerMode = false;
                    m_isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ActionUIManager.Initialize() : " + ex.Message);
            }
        }
    }
}