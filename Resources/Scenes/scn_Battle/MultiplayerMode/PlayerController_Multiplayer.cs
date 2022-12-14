using System;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class PlayerController_Multiplayer : MonoBehaviour
    {
        #region Properties
        public PlayerOnBoard PlayerData { get { return m_mainScript?.PlayerData; } }

        public int PlayerId { get { return (PlayerData != null) ? (PlayerData.IsPlayer1 ? 1 : 2) : default; } }

        public bool IsMyTurn { get; set; }

        public bool IsInitialized { get; private set; }
        #endregion

        #region Private Fields
        private UnityBattleSystem_Multiplayer m_mainScript;

        private bool m_isCameraRotationAdjusted;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            try
            {
                Debug.Log("PlayerController: Awake");
                IsInitialized = false;
                m_isCameraRotationAdjusted = false;
            }
            catch (Exception ex)
            {
                Debug.Log("PlayerController: at Awake() " + ex.Message);
            }
        }

        private void Update()
        {
            if (!IsInitialized)
                Initialize();
            else if (!m_isCameraRotationAdjusted && PlayerData != null)
            {
                if (PlayerId == 2) // If Player 2
                    m_isCameraRotationAdjusted = SetPlayer2CameraPosition();
                else m_isCameraRotationAdjusted = true; // No need to adjust rotation if Player 1
            }
        }

        private void Initialize()
        {
            try
            {
                Debug.Log("PlayerController: Start Initialization.");

                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();
                if (m_mainScript == null)
                    return;

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.Log("PlayerController: at Initialize() " + ex.Message);
            }
        }

        private bool SetPlayer2CameraPosition()
        {
            try
            {
                Transform transform_gameBoard = GameObject.FindGameObjectWithTag("GameBoard").transform;

                Debug.Log("PlayerController: Set Camera Position for Player 2");

                GameObject go_mainCamera = GameObject.Find("Main Camera");
                Vector3 mainCameraPosition = go_mainCamera.transform.position;
                Vector3 mainCameraOriginalAngles = go_mainCamera.transform.rotation.eulerAngles;

                MeshRenderer meshRenderer_board = transform_gameBoard.GetComponent<MeshRenderer>();
                Vector3 boardCenter = meshRenderer_board.bounds.center;

                go_mainCamera.transform.RotateAround(boardCenter, new Vector3(0, 1f, 0), 180f); // Rotate camera based on board's y axis.

                Debug.Log("PlayerController: End InitializeExplicit (For Player2)");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("PlayerController: at SetPlayer2CameraPosition() " + ex.Message);
                return false;
            }
        }
    }
}