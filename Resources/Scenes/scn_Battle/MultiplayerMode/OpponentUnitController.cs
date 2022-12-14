using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Graphics;
using System;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class OpponentUnitController : MonoBehaviour
    {
        #region Properties
        public int PublicIndex { get; private set; } // No unit can have same index
        public int OwnerId { get; private set; }

        public int MaxHP { get; set; }
        public int RemainingHP { get; set; }

        public UnitData UnitReference { get; set; }

        public bool IsInitialized { get; private set; }
        #endregion

        #region Private Fields
        private UnityBattleSystem_Multiplayer m_mainScript;

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
                Debug.Log("OpponentUnitController: at Awake()" + ex.Message);
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
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();

                if (m_mainScript.IsInitialized && m_areInitializationDataProvided)
                {
                    Debug.Log("UnitController_Multiplayer: Start Initialization.");

                    this.transform.parent = m_mainScript.GO_Opponent.transform;

                    if (this.transform.parent == null)
                    {
                        Debug.Log("UnitController_Multiplayer: parent object not set");
                        return;
                    }

                    if (!SetImages())
                    {
                        Debug.Log("UnitController_Multiplayer: error at SetImages()");
                        return;
                    }

                    IsInitialized = true;
                    Debug.Log("UnitController_Multiplayer: End Initialization.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("OpponentUnitController: at Initialize() " + ex.Message);
            }
        }

        public bool SetInitializationData(int _publicIndex, int _ownerId, UnitData _unitReference, int _maxHP, int _remainingHP)
        {
            PublicIndex = _publicIndex;
            OwnerId = _ownerId;

            UnitReference = _unitReference;

            MaxHP = _maxHP;
            RemainingHP = _remainingHP;

            if (_unitReference != null)
                m_areInitializationDataProvided = true;

            return m_areInitializationDataProvided;
        }

        private bool SetImages()
        {
            try
            {
                Shader shader_default = Shader.Find("Sprites/Default");

                int rarityIndex = (Convert.ToInt32(this.UnitReference.Rarity) / CoreValues.LEVEL_DIFFERENCE_BETWEEN_RARITIES) - 1;
                m_meshRenderer_frame.material = MaterialContainer.Instance.RarityMaterials[rarityIndex];

                m_meshRenderer_unit.material.mainTexture = ImageConverter.ByteArrayToTexture(UnitReference.IconAsBytes, FilterMode.Point);
                m_meshRenderer_unit.material.shader = shader_default;

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("OpponentUnitController.SetImages() " + ex.Message);
                return false;
            }
        }
    }
}