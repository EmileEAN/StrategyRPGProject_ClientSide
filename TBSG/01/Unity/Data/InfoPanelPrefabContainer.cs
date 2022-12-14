using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class InfoPanelPrefabContainer
    {
        private static InfoPanelPrefabContainer m_instance;

        public static InfoPanelPrefabContainer Instance { get { return m_instance ?? (m_instance = new InfoPanelPrefabContainer()); } }

        private InfoPanelPrefabContainer() { }

        #region Properties
        public GameObject UnitInfoPanelPrefab { get; private set; }
        public GameObject UnitActionRangesInfoPanelPrefab { get; private set; }
        public GameObject UnitEquipmentsInfoPanelPrefab { get; private set; }
        public GameObject UnitSkillsInfoPanelPrefab { get; private set; }
        public GameObject WeaponInfoPanelPrefab { get; private set; }
        public GameObject ArmourInfoPanelPrefab { get; private set; }
        public GameObject AccessoryInfoPanelPrefab { get; private set; }
        public GameObject ItemInfoPanelPrefab { get; private set; }
        public GameObject SkillInfoPanelPrefab { get; private set; }

        public GameObject InfoPanelUnitLabelPrefab { get; private set; }
        public GameObject InfoPanelSkillInfoButtonPrefab { get; private set; }
        #endregion

        #region Public Functions
        public void Initialize(GameObject _unitInfoPanelPrefab,
            GameObject _unitActionRangeInfoPanelPrefab, GameObject _unitEquipmentsInfoPanelPrefab, GameObject _unitSkillsInfoPanelPrefab, GameObject _weaponInfoPanelPrefab, GameObject _armourInfoPanelPrefab, GameObject _accessoryInfoPanelPrefab, GameObject _itemInfoPanelPrefab, GameObject _skillInfoPanelPrefab, GameObject _infoPanelUnitLabelPrefab, GameObject _infoPanelSkillInfoButtonPrefab)
        {
            UnitInfoPanelPrefab = _unitInfoPanelPrefab;
            UnitActionRangesInfoPanelPrefab = _unitActionRangeInfoPanelPrefab;
            UnitEquipmentsInfoPanelPrefab = _unitEquipmentsInfoPanelPrefab;
            UnitSkillsInfoPanelPrefab = _unitSkillsInfoPanelPrefab;
            WeaponInfoPanelPrefab = _weaponInfoPanelPrefab;
            ArmourInfoPanelPrefab = _armourInfoPanelPrefab;
            AccessoryInfoPanelPrefab = _accessoryInfoPanelPrefab;
            ItemInfoPanelPrefab = _itemInfoPanelPrefab;
            SkillInfoPanelPrefab = _skillInfoPanelPrefab;

            InfoPanelUnitLabelPrefab = _infoPanelUnitLabelPrefab;
            InfoPanelSkillInfoButtonPrefab = _infoPanelSkillInfoButtonPrefab;
        }
        #endregion
    }
}
