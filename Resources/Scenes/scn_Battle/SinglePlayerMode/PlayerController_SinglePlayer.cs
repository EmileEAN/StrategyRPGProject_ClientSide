using EEANWorks.Games.TBSG._01.Data;
using System;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class PlayerController_SinglePlayer : MonoBehaviour
    {

        #region Serialized Fields
        public bool IsCPU;
        #endregion

        #region Properties
        public object PlayerData { get; set; } //Initially of type Player. Must be swapped with an instance of type PlayerOnBoard to properly use the SyncProperties() function.

        public int PlayerId { get; private set; }

        public bool IsMyTurn { get; private set; }

        public bool HasMoved { get; private set; }
        public bool HasAttacked { get; private set; }
        public bool HasUsedUltimateSkill { get; private set; }

        public int MaxSP { get; private set; }
        public int RemainingSP { get; private set; }
        public int SelectedAlliedUnitIndex { get; private set; } // Synced with Field.Players[].AlliedUnit[index]

        public bool IsInitialized { get; private set; }
        #endregion

        #region Private Fields
        private UnityBattleSystem_SinglePlayer m_mainScript;

        private bool m_isIdProvided;
        private bool m_isCameraRotationAdjusted;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            try
            {
                Debug.Log("PlayerController: Awake");
                IsInitialized = false;
                m_isIdProvided = false;
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

            if (!IsCPU
                && m_isIdProvided
                && !m_isCameraRotationAdjusted)
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
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_SinglePlayer>();
                if (m_mainScript == null)
                    return;

                PlayerData = IsCPU ? GameDataContainer.Instance.CPU : GameDataContainer.Instance.Player;
                if (PlayerData == null)
                    return;

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.Log("PlayerController: at Initialize() " + ex.Message);
            }
        }

        public void SyncProperties(int _initializingId = -1)
        {
            if (!m_isIdProvided)
            {
                if (_initializingId != -1)
                {
                    PlayerId = _initializingId;
                    m_isIdProvided = true;
                }
                else
                    return;
            }

            if (m_mainScript.BattleSystemCore != null)
            {
                PlayerOnBoard pob = PlayerData as PlayerOnBoard;

                if (pob != null)
                {
                    IsMyTurn = (pob == m_mainScript.BattleSystemCore.CurrentTurnPlayer) ? true : false;

                    HasMoved = pob.Moved;
                    HasAttacked = pob.Attacked;
                    HasUsedUltimateSkill = pob.UsedUltimateSkill;

                    MaxSP = pob.MaxSP;
                    RemainingSP = pob.RemainingSP;
                    SelectedAlliedUnitIndex = pob.SelectedUnitIndex;
                }
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