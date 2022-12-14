using EEANWorks.Games.TBSG._01.Unity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class TileMaskManager_Multiplayer : MonoBehaviour
    {
        #region Properties
        public bool IsInitialized { get; private set; }
        public int MaxNumOfTargets
        {
            get { return m_maxNumOfTargets; }
            set
            {
                if (value < 0)
                    m_maxNumOfTargets = 0;
                else
                    m_maxNumOfTargets = value;
            }
        }
        #endregion

        #region Private Fields
        private const int SIZE_X = CoreValues.SIZE_OF_A_SIDE_OF_BOARD;
        private const int SIZE_Z = CoreValues.SIZE_OF_A_SIDE_OF_BOARD;

        private UnityBattleSystem_Multiplayer m_mainScript;
        private ActionUIManager_Multiplayer m_actionUIManager;
        private AnimationController_Multiplayer m_animationController;

        private List<MeshRenderer> m_meshRenderers_lowerMask;
        private List<MeshRenderer> m_meshRenderers_upperMask;

        private Material m_material_targetingTile_default;
        private Material m_material_targetingTile_nonSelectable;
        private Material m_material_targetingTile_movement_selectable;
        private Material m_material_targetingTile_attack_selectable;
        private Material m_material_targetingTile_skill_selectable;
        private Material m_material_targetingTile_movement_selected;
        private Material m_material_targetingTile_attack_selected;
        private Material m_material_targetingTile_skill_selected;

        private Dictionary<_2DCoord, eMaskType> m_targetArea; //bool represents selection status
        private int m_maxNumOfTargets;

        private bool m_wasMouseButton1Down;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            try
            {
                IsInitialized = false;

                m_meshRenderers_lowerMask = new List<MeshRenderer>();
                m_meshRenderers_upperMask = new List<MeshRenderer>();

                m_material_targetingTile_default = BattleSceneAssetContainer.Instance.Material_TargetingTile_Default;
                m_material_targetingTile_nonSelectable = BattleSceneAssetContainer.Instance.Material_TargetingTile_NonSelectable;
                m_material_targetingTile_movement_selectable = BattleSceneAssetContainer.Instance.Material_TargetingTile_Movement_Selectable;
                m_material_targetingTile_attack_selectable = BattleSceneAssetContainer.Instance.Material_TargetingTile_Attack_Selectable;
                m_material_targetingTile_skill_selectable = BattleSceneAssetContainer.Instance.Material_TargetingTile_Skill_Selectable;
                m_material_targetingTile_movement_selected = BattleSceneAssetContainer.Instance.Material_TargetingTile_Movement_Selected;
                m_material_targetingTile_attack_selected = BattleSceneAssetContainer.Instance.Material_TargetingTile_Attack_Selected;
                m_material_targetingTile_skill_selected = BattleSceneAssetContainer.Instance.Material_TargetingTile_Skill_Selected;

                m_targetArea = new Dictionary<_2DCoord, eMaskType>();

                m_wasMouseButton1Down = false;
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

            if (IsInitialized)
                ManageMaskVisibility();
        }

        public void Initialize()
        {
            try
            {
                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();
                if (m_mainScript == null)
                    return;

                if (m_mainScript.IsInitialized)
                {
                    Debug.Log("TileMaskManager_Multiplayer: Start Initialize.");

                    if (m_actionUIManager == null)
                        m_actionUIManager = GameObject.Find("ActionUI").GetComponent<ActionUIManager_Multiplayer>();
                    if (m_actionUIManager == null)
                        return;

                    if (m_animationController == null)
                        m_animationController = this.transform.root.GetComponent<AnimationController_Multiplayer>();
                    if (m_animationController == null)
                        return;

                    if (!InitializeMaterial())
                        return;

                    IsInitialized = true;
                    Debug.Log("TileMaskManager_Multiplayer: End Initialize.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("TileMap: at Initialize() " + ex.Message);
            }
        }

        public void ChangeTileSelection(int _tileIndex) { ChangeTileSelection(_tileIndex.To2DCoord()); }

        public void ChangeTileSelection(_2DCoord _coord)
        {
            if (m_animationController.LockUI)
                return;

            if (_coord == null || !m_targetArea.ContainsKey(_coord))
                return;

            if (m_targetArea[_coord] == eMaskType.NonSelectable)
                return;

            if (m_targetArea[_coord] == eMaskType.Selected)
                m_targetArea[_coord] = eMaskType.Selectable;
            else if (m_targetArea.Values.Count(x => x == eMaskType.Selected) < m_maxNumOfTargets)
                m_targetArea[_coord] = eMaskType.Selected;

            SetMaterial(_coord);

            Debug.Log("SelectionStatusChanged: Tile " + _coord.ToIndex().ToString() + " -> " + m_targetArea[_coord].ToString());
            m_actionUIManager.UpdateConfirmationButton();
        }

        public int NumOfTilesSelected() { return m_targetArea.Count(x => x.Value == eMaskType.Selected); }

        public List<_2DCoord> SelectedCoords() { return m_targetArea.GetKeysWithValue(eMaskType.Selected); }

        public void DeselectAll()
        {
            foreach (_2DCoord coord in SelectedCoords())
            {
                ChangeTileSelection(coord);
            }
        }

        private void SetMaterial(_2DCoord _coord)
        {
            int tileIndex = _coord.ToIndex();

            if (tileIndex < 0 || tileIndex >= m_meshRenderers_lowerMask.Count || tileIndex >= m_meshRenderers_upperMask.Count) return;

            MeshRenderer meshRenderer_lowerMask = m_meshRenderers_lowerMask[tileIndex];
            MeshRenderer meshRenderer_upperMask = m_meshRenderers_upperMask[tileIndex];

            if (!m_targetArea.ContainsKey(_coord)) return;

            eMaskType maskType = m_targetArea[_coord];
            if (maskType == eMaskType.NonSelectable)
            {
                meshRenderer_lowerMask.material = m_material_targetingTile_nonSelectable;
                meshRenderer_upperMask.material = m_material_targetingTile_default;
            }
            else
            {
                switch (m_actionUIManager.CurrentActionSelected)
                {
                    case eActionType.Move:
                        switch (maskType)
                        {
                            default: //NonSelectable
                                break;
                            case eMaskType.Selectable:
                                meshRenderer_lowerMask.material = m_material_targetingTile_movement_selectable;
                                meshRenderer_upperMask.material = m_material_targetingTile_default;
                                break;
                            case eMaskType.Selected:
                                meshRenderer_lowerMask.material = m_material_targetingTile_default;
                                meshRenderer_upperMask.material = m_material_targetingTile_movement_selected;
                                break;
                        }
                        break;
                    case eActionType.Attack:
                        switch (maskType)
                        {
                            default: //NonSelectable
                                break;
                            case eMaskType.Selectable:
                                meshRenderer_lowerMask.material = m_material_targetingTile_attack_selectable;
                                meshRenderer_upperMask.material = m_material_targetingTile_default;
                                break;
                            case eMaskType.Selected:
                                meshRenderer_lowerMask.material = m_material_targetingTile_default;
                                meshRenderer_upperMask.material = m_material_targetingTile_attack_selected;
                                break;
                        }
                        break;
                    case eActionType.Skill:
                        switch (maskType)
                        {
                            default: //NonSelectable
                                break;
                            case eMaskType.Selectable:
                                meshRenderer_lowerMask.material = m_material_targetingTile_skill_selectable;
                                meshRenderer_upperMask.material = m_material_targetingTile_default;
                                break;
                            case eMaskType.Selected:
                                meshRenderer_lowerMask.material = m_material_targetingTile_default;
                                meshRenderer_upperMask.material = m_material_targetingTile_skill_selected;
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void UpdateTargetArea(Dictionary<_2DCoord, bool> _targetArea)
        {
            m_targetArea.Clear();

            if (_targetArea != null)
            {
                foreach (var tile in _targetArea)
                {
                    m_targetArea.Add(tile.Key, (tile.Value ? eMaskType.Selectable : eMaskType.NonSelectable));
                }
            }
        }

        public void DisplayTargetArea()
        {
            ResetMaterial();

            UpdateTargetingMaterial();
        }

        private bool InitializeMaterial()
        {
            try
            {
                if (!IsInitialized)
                {
                    m_meshRenderers_lowerMask.Clear();
                    m_meshRenderers_upperMask.Clear();

                    for (int z = 1; z <= SIZE_Z; z++)
                    {
                        for (int x = 1; x <= SIZE_X; x++)
                        {
                            int tileNum = SIZE_Z * (z - 1) + (x - 1);

                            string tileObjectName = "Tile" + tileNum.ToString();

                            Transform tile = this.transform.Find(tileObjectName);
                            MeshRenderer tileMaskMeshRenderer = tile.Find("Mask" + tileNum.ToString()).GetComponent<MeshRenderer>();
                            MeshRenderer tileUpperMaskMeshRenderer = tile.Find("UpperMask" + tileNum.ToString()).GetComponent<MeshRenderer>();

                            m_meshRenderers_lowerMask.Add(tileMaskMeshRenderer);
                            m_meshRenderers_upperMask.Add(tileUpperMaskMeshRenderer);
                        }
                    }
                    if (m_meshRenderers_lowerMask.Count != SIZE_X * SIZE_Z)
                        return false;
                }

                ResetMaterial();

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("TileMap: at ResetMaterial(). " + ex.Message);
                return false;
            }
        }

        private void ResetMaterial()
        {
            foreach (MeshRenderer meshRenderer in m_meshRenderers_lowerMask) { meshRenderer.material = m_material_targetingTile_default; }
            foreach (MeshRenderer meshRenderer in m_meshRenderers_upperMask) { meshRenderer.material = m_material_targetingTile_default; }
        }

        private bool UpdateTargetingMaterial()
        {
            try
            {
                if (m_targetArea == null)
                    return false;

                foreach (_2DCoord coord in m_targetArea.Keys)
                {
                    if (coord.X >= 0 && coord.Y < SIZE_X
                        && coord.Y >= 0 && coord.Y < SIZE_Z)
                    {
                        SetMaterial(coord);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("TileMap: at UpdateTargetingMaterial(). " + ex.Message);
                return false;
            }
        }

        private void ManageMaskVisibility()
        {
            try
            {
                if (Input.GetMouseButtonDown(1) && !m_wasMouseButton1Down)
                {
                    foreach (MeshRenderer tileMask in m_meshRenderers_lowerMask) { tileMask.enabled = false; }
                    foreach (MeshRenderer tileUpperMask in m_meshRenderers_upperMask) { tileUpperMask.enabled = false; }
                    m_wasMouseButton1Down = true;
                }
                else if (Input.GetMouseButtonUp(1) && m_wasMouseButton1Down)
                {
                    foreach (MeshRenderer tileMask in m_meshRenderers_lowerMask) { tileMask.enabled = true; }
                    foreach (MeshRenderer tileUpperMask in m_meshRenderers_upperMask) { tileUpperMask.enabled = true; }
                    m_wasMouseButton1Down = false;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("TileMask: at ManageMaskVisibility(). " + ex.Message);
            }
        }
    }
}