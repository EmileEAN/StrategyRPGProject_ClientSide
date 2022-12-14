using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.SceneSpecific;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    [RequireComponent(typeof(InfoPanelManager))]
    [RequireComponent(typeof(InfoPanelManager_Weapon))]
    [RequireComponent(typeof(InfoPanelManager_Armour))]
    [RequireComponent(typeof(InfoPanelManager_Accessory))]
    public class InfoPanelManager_UnitEquipments : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_unitEquipmentsInfoPanelPrefab;

        private InfoPanelManager m_mainInfoPanelManager;
        private InfoPanelManager_Weapon m_infoPanelManager_weapon;
        private InfoPanelManager_Armour m_infoPanelManager_armour;
        private InfoPanelManager_Accessory m_infoPanelManager_accessory;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;

        private Color m_color_classificationIcon_bright = Color.white;
        private Color m_color_classificationIcon_dark = new Color32(100, 100, 100, 255);
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_unitEquipmentsInfoPanelPrefab = InfoPanelPrefabContainer.Instance.UnitEquipmentsInfoPanelPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
            m_infoPanelManager_weapon = this.GetComponent<InfoPanelManager_Weapon>();
            m_infoPanelManager_armour = this.GetComponent<InfoPanelManager_Armour>();
            m_infoPanelManager_accessory = this.GetComponent<InfoPanelManager_Accessory>();
        }

        public void InstantiateInfoPanel(int _unitUniqueId, bool _disableEquipmentChange)
        {
            Unit unit = GameDataContainer.Instance.Player.UnitsOwned.First(x => x.UniqueId == _unitUniqueId);

            InstantiateInfoPanel(unit, _disableEquipmentChange);
        }
        public void InstantiateInfoPanel(Unit _unit, bool _disableEquipmentChange)
        {
            GameObject go_infoPanel = Instantiate(m_unitEquipmentsInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _unit, _disableEquipmentChange);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Unit _unit, bool _disableEquipmentChange)
        {
            //-----------------------------------------------------------------------------
            // Initialize Section with equipped equipments
            //-----------------------------------------------------------------------------
            Transform transform_equipmentsContainer = _go_infoPanel.transform.Find("Equipments").Find("EquipmentsContainer");

            Sprite emptyEquipmentSlotSprite = _disableEquipmentChange ? SpriteContainer.Instance.EmptyObjectSprite_NotSet : SpriteContainer.Instance.EmptyObjectSprite_Set;

            Transform transform_mainWeaponButton = transform_equipmentsContainer.Find("MainWeapon").Find("ButtonContainer").Find("Button@Object");
            Transform transform_subWeaponButton = transform_equipmentsContainer.Find("SubWeapon").Find("ButtonContainer").Find("Button@Object");
            Transform transform_armourButton = transform_equipmentsContainer.Find("Armour").Find("ButtonContainer").Find("Button@Object");
            Transform transform_accessoryButton = transform_equipmentsContainer.Find("Accessory").Find("ButtonContainer").Find("Button@Object");

            GameObjectFormattingFunctions.FormatUnitEquipmentButtonsAsChangeable(_unit, transform_mainWeaponButton, transform_subWeaponButton, transform_armourButton, transform_accessoryButton, m_infoPanelManager_weapon, m_infoPanelManager_armour, m_infoPanelManager_accessory, _disableEquipmentChange);

            //-----------------------------------------------------------------------------
            // Initialize Section to display equipable equipment classifications
            //-----------------------------------------------------------------------------
            Transform transform_equipableEquipmentClassifications = _go_infoPanel.transform.Find("EquipableEquipmentClassifications");

            Transform transform_equipableWeaponClassificationIcons = transform_equipableEquipmentClassifications.Find("EquipableWeaponClassifications").Find("Panel@EquipableWeaponClassificationIcons");
            int index = 0;
            foreach (Transform child in transform_equipableWeaponClassificationIcons)
            {
                eWeaponClassification classification = (eWeaponClassification)index;

                child.GetComponent<DetailInfoPopUpController>().GO_detailInfoPopUp.transform.Find("Text").GetComponent<Text>().text = classification.ToString();

                Image image_classificationIcon = child.GetComponent<Image>();
                image_classificationIcon.sprite = SpriteContainer.Instance.GetWeaponClassificationIcon(classification);

                if (_unit.BaseInfo.EquipableWeaponClassifications.Contains(classification))
                    image_classificationIcon.color = m_color_classificationIcon_bright;
                else
                    image_classificationIcon.color = m_color_classificationIcon_dark;

                index++;
            }

            Transform transform_equipableArmourClassificationIcons = transform_equipableEquipmentClassifications.Find("EquipableArmourClassifications").Find("Panel@EquipableArmourClassificationIcons");
            index = 0;
            foreach (Transform child in transform_equipableArmourClassificationIcons)
            {
                eArmourClassification classification = (eArmourClassification)index;

                child.GetComponent<DetailInfoPopUpController>().GO_detailInfoPopUp.transform.Find("Text").GetComponent<Text>().text = classification.ToString();

                Image image_classificationIcon = child.GetComponent<Image>();
                image_classificationIcon.sprite = SpriteContainer.Instance.GetArmourClassificationIcon(classification);

                if (_unit.BaseInfo.EquipableArmourClassifications.Contains(classification))
                    image_classificationIcon.color = m_color_classificationIcon_bright;
                else
                    image_classificationIcon.color = m_color_classificationIcon_dark;

                index++;
            }

            Transform transform_equipableAccessoryClassificationIcons = transform_equipableEquipmentClassifications.Find("EquipableAccessoryClassifications").Find("Panel@EquipableAccessoryClassificationIcons");
            index = 0;
            foreach (Transform child in transform_equipableAccessoryClassificationIcons)
            {
                eAccessoryClassification classification = (eAccessoryClassification)index;

                child.GetComponent<DetailInfoPopUpController>().GO_detailInfoPopUp.transform.Find("Text").GetComponent<Text>().text = classification.ToString();

                Image image_classificationIcon = child.GetComponent<Image>();
                image_classificationIcon.sprite = SpriteContainer.Instance.GetAccessoryClassificationIcon(classification);

                if (_unit.BaseInfo.EquipableAccessoryClassifications.Contains(classification))
                    image_classificationIcon.color = m_color_classificationIcon_bright;
                else
                    image_classificationIcon.color = m_color_classificationIcon_dark;

                index++;
            }
        }

        //Remove newest info panel
        public void RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            m_mainInfoPanelManager.Request_RemoveInfoPanel(_go_infoPanel);
        }
    }
}