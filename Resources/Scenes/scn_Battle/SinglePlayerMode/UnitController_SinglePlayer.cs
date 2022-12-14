using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Graphics;
using System;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class UnitController_SinglePlayer : MonoBehaviour
    {
        #region Properties
        public int PublicIndex { get; private set; } // No unit can have same index
        public int PrivateIndex { get; private set; } // Units of same Player cannot have same id
        public int OwnerId { get; private set; }

        public UnitInstance UnitReference { get; private set; }

        public bool IsInitialized { get; private set; }
        #endregion

        #region Private Fields
        private UnityBattleSystem_SinglePlayer m_mainScript;

        private MeshRenderer m_meshRenderer_frame;
        private MeshRenderer m_meshRenderer_unit;
        private MeshRenderer m_meshRenderer_backGround;

        private MeshCollider m_collider;

        private bool m_areInitializationDataProvided;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            try
            {
                IsInitialized = false;
                m_areInitializationDataProvided = false;

                Transform transform_frame = this.transform.Find("Frame");
                m_meshRenderer_frame = transform_frame.GetComponent<MeshRenderer>();
                m_collider = transform_frame.GetComponent<MeshCollider>();
                //m_meshRenderer_class = this.transform.Find("SmallSquarePlane").GetComponent<MeshRenderer>();
                m_meshRenderer_unit = this.transform.Find("LargeSquarePlane").GetComponent<MeshRenderer>();
                m_meshRenderer_backGround = this.transform.Find("OctagonPlane").GetComponent<MeshRenderer>();

                //Required for Update() to avoid not being called
                if (!this.isActiveAndEnabled)
                    this.enabled = true;
            }
            catch (Exception ex)
            {
                Debug.Log("UnitController_SinglePlayer: at Awake()" + ex.Message);
            }
        }

        void Update()
        {
            if (!IsInitialized)
                Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_SinglePlayer>();

                if (m_mainScript.IsInitialized && m_areInitializationDataProvided)
                {
                    Debug.Log("UnitController_SinglePlayer: Start Initialization.");

                    for (int i = 0; i < m_mainScript.GOs_Player.Length; i++)
                    {
                        if (m_mainScript.PlayerControllers[i].PlayerId == OwnerId)
                        {
                            this.transform.parent = m_mainScript.GOs_Player[i].transform;
                            break;
                        }
                    }

                    if (this.transform.parent == null)
                    {
                        Debug.Log("UnitController_SinglePlayer: parent object not set");
                        return;
                    }

                    if (!SetImages())
                    {
                        Debug.Log("UnitController_SinglePlayer: error at SetImages()");
                        return;
                    }

                    IsInitialized = true;
                    Debug.Log("UnitController_SinglePlayer: End Initialization.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("UnitController_SinglePlayer: at Initialize() " + ex.Message);
            }
        }

        public bool SetInitializationData(int _publicIndex, int _privateIndex, int _ownerId, UnitInstance _unitReference)
        {
            if (m_areInitializationDataProvided)
                return false;

            PublicIndex = _publicIndex;
            PrivateIndex = _privateIndex;
            OwnerId = _ownerId;
            UnitReference = _unitReference;

            if (_unitReference != null)
                m_areInitializationDataProvided = true;

            return m_areInitializationDataProvided;
        }

        private bool SetImages()
        {
            try
            {
                int rarityIndex = (Convert.ToInt32(this.UnitReference.BaseInfo.Rarity) / CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES) - 1;
                m_meshRenderer_frame.material = MaterialContainer.Instance.RarityMaterials[rarityIndex];

                m_meshRenderer_unit.material.mainTexture = ImageConverter.ByteArrayToTexture(UnitReference.BaseInfo.IconAsBytes, FilterMode.Point);
                m_meshRenderer_unit.material.shader = BattleSceneAssetContainer.Instance.Shader_Default;

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("UnitController_SinglePlayer.SetImages() " + ex.Message);
                return false;
            }
        }
    }
}