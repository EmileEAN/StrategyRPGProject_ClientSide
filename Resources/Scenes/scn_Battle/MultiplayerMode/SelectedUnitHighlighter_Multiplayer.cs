﻿using EEANWorks.Games.TBSG._01.Unity.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class SelectedUnitHighlighter_Multiplayer : MonoBehaviour
    {
        #region Private Fields
        private List<MeshRenderer> m_meshRenderers_unitChipFrame;
        private List<Material> m_materials_original;

        private UnityBattleSystem_Multiplayer m_mainScript;

        private PlayerOnBoard m_player;

        private bool m_isInitialized;

        private int m_currentSelectedUnitIndex;
        #endregion

        // Start is called before the first frame update
        void Awake()
        {
            m_isInitialized = false;

            m_meshRenderers_unitChipFrame = new List<MeshRenderer>();
            m_materials_original = new List<Material>();

            m_currentSelectedUnitIndex = -1;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();

            if (m_isInitialized)
            {
                PlayerController_Multiplayer playerController = m_mainScript.PlayerController;
                if (playerController.PlayerData.SelectedUnitIndex != m_currentSelectedUnitIndex)
                {
                    m_currentSelectedUnitIndex = playerController.PlayerData.SelectedUnitIndex;
                    UpdateHighlight();
                }
            }
        }

        private void Initialize()
        {
            try
            {
                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();
                if (m_mainScript == null)
                    return;

                if (m_mainScript.IsInitialized)
                {
                    List<GameObject> gos_unitChips = m_mainScript.GOs_PlayerUnit;

                    m_meshRenderers_unitChipFrame.Clear();
                    m_materials_original.Clear();
                    foreach (GameObject go_unitChip in gos_unitChips)
                    {
                        PlayerUnitController unitController = go_unitChip.GetComponent<PlayerUnitController>();
                        if (unitController == null || !unitController.IsInitialized)
                            return;

                        MeshRenderer meshRenderer = go_unitChip.transform.Find("Frame").GetComponent<MeshRenderer>();
                        m_meshRenderers_unitChipFrame.Add(meshRenderer);
                        m_materials_original.Add(meshRenderer.material);
                    }

                    int numOfUnits = gos_unitChips.Count;
                    if (m_meshRenderers_unitChipFrame.Count != numOfUnits || m_materials_original.Count != numOfUnits)
                        return;

                    Debug.Log("SelectedUnitHighlighter: Initialized successfully!");
                    m_isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("SelectedUnitHighlighter.Initialize() : " + ex.Message);
            }
        }

        private void UpdateHighlight()
        {
            if (m_currentSelectedUnitIndex > m_meshRenderers_unitChipFrame.Count)
                return;

            ResetHighlight();

            if (m_currentSelectedUnitIndex >= 0)
                m_meshRenderers_unitChipFrame[m_currentSelectedUnitIndex].material = BattleSceneAssetContainer.Instance.Material_SelectedUnitChipFrame;
        }

        private void ResetHighlight()
        {
            for (int i = 0; i < m_meshRenderers_unitChipFrame.Count; i++)
            {
                m_meshRenderers_unitChipFrame[i].material = m_materials_original[i];
            }
        }
    }
}