using EEANWorks.Games.TBSG._01.Unity.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TileMapManager_SinglePlayer : MonoBehaviour
    {
        #region Public Fields
        public const int SIZE_X = CoreValues.SIZE_OF_A_SIDE_OF_BOARD;
        public const int SIZE_Z = CoreValues.SIZE_OF_A_SIDE_OF_BOARD;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; }
        public List<MeshRenderer> MeshRenderers_Tile { get; private set; }
        #endregion

        #region Private Fields
        private UnityBattleSystem_SinglePlayer m_mainScript;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            try
            {
                IsInitialized = false;

                MeshRenderers_Tile = new List<MeshRenderer>();
            }
            catch (Exception ex)
            {
                Debug.Log("TileMap: at Awake() " + ex.Message);
            }
        }

        void Update()
        {
            if (!IsInitialized)
                Initialize();
        }

        public void Initialize()
        {
            try
            {
                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_SinglePlayer>();

                if (m_mainScript.IsInitialized)
                {
                    Debug.Log("TileMap (Script): Start Initialize.");

                    if (!GetTiles())
                        return;

                    if (m_mainScript.PlayerController.PlayerId != 1)
                    {
                        if (!RotateTiles())
                            return;
                    }

                    if (!ApplyMaterialToTiles())
                        return;

                    IsInitialized = true;
                    Debug.Log("TileMap (Script): End Initialize.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("TileMap: at Initialize() " + ex.Message);
            }
        }

        private bool GetTiles()
        {
            try
            {
                MeshRenderers_Tile.Clear();

                for (int z = 0; z < SIZE_Z; z++)
                {
                    for (int x = 0; x < SIZE_X; x++)
                    {
                        string tileObjectName = "Tile" + (SIZE_Z * z + x).ToString();

                        MeshRenderer meshRenderer = this.transform.Find(tileObjectName).GetComponent<MeshRenderer>();

                        if (meshRenderer != null)
                            MeshRenderers_Tile.Add(meshRenderer);
                        else
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("TileMap: at GetTiles() " + ex.Message);
                return false;
            }
        }

        private bool RotateTiles()
        {
            try
            {
                foreach (MeshRenderer meshRenderer in MeshRenderers_Tile)
                {
                    meshRenderer.transform.Rotate(0f, 180f, 0f);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("TileMapManager_SinglePlayer.RotateTiles() : " + ex.Message);
                return false;
            }

        }

        public bool ApplyMaterialToTiles()
        {
            try
            {
                for (int z = 0; z < SIZE_Z; z++)
                {
                    for (int x = 0; x < SIZE_X; x++)
                    {
                        eTileType tileType = m_mainScript.BattleSystemCore.Field.Board.Sockets[x, z].TileType;

                        int index = SIZE_Z * z + x;

                        MeshRenderers_Tile[index].material = BattleSceneAssetContainer.Instance.GetTileMaterialForType(tileType);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("TileMap: at ApplyMaterialToTiles(). " + ex.Message);
                return false;
            }
        }

        public void ChangeTile(int _boardCoordX, int _boardCoordY, eTileType _tileType)
        {
            if (_boardCoordX >= 0 && _boardCoordX < SIZE_X
                && _boardCoordY >= 0 && _boardCoordY < SIZE_Z)
            {
                int index = SIZE_X * _boardCoordY + _boardCoordX;
                MeshRenderers_Tile[index].material = BattleSceneAssetContainer.Instance.GetTileMaterialForType(_tileType);
            }
        }
    }
}