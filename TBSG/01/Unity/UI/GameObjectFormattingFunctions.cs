using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.SceneSpecific;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    public static class GameObjectFormattingFunctions
    {
        public static void FormatUnitEquipmentButtonsAsNonChangeable(Unit _unit, Transform _transform_mainWeaponButton, Transform _transform_subWeaponButton, Transform _transform_armourButton, Transform _transform_accessoryButton, InfoPanelManager_Weapon _infoPanelManager_weapon, InfoPanelManager_Armour _infoPanelManager_armour, InfoPanelManager_Accessory _infoPanelManager_accessory)
        {
            Sprite emptyEquipmentSlotSprite = SpriteContainer.Instance.EmptyObjectSprite_NotSet;

            // Main weapon button
            GameObjectFormatter_ObjectButton goFormatter_mainWeaponButton = _transform_mainWeaponButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_mainWeapon = (_unit?.MainWeapon != null) ? new UnityAction(() => _infoPanelManager_weapon.InstantiateInfoPanel(_unit?.MainWeapon)) : null;
            goFormatter_mainWeaponButton.Format(_unit?.MainWeapon, null, buttonClickAction_mainWeapon);
            if (_unit?.MainWeapon == null)
                goFormatter_mainWeaponButton.Image_Object.sprite = emptyEquipmentSlotSprite;

            // Sub weapon button
            GameObjectFormatter_ObjectButton goFormatter_subWeaponButton = _transform_subWeaponButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_subWeapon = (_unit?.SubWeapon != null) ? new UnityAction(() => _infoPanelManager_weapon.InstantiateInfoPanel(_unit?.SubWeapon)) : null;
            goFormatter_subWeaponButton.Format(_unit?.SubWeapon, null, buttonClickAction_subWeapon);
            if (_unit?.SubWeapon == null)
                goFormatter_subWeaponButton.Image_Object.sprite = emptyEquipmentSlotSprite;

            // Armour button
            GameObjectFormatter_ObjectButton goFormatter_armourButton = _transform_armourButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_armour = (_unit?.Armour != null) ? new UnityAction(() => _infoPanelManager_armour.InstantiateInfoPanel(_unit?.Armour)) : null;
            goFormatter_armourButton.Format(_unit?.Armour, null, buttonClickAction_armour);
            if (_unit?.Armour == null)
                goFormatter_armourButton.Image_Object.sprite = emptyEquipmentSlotSprite;

            // Accessory button
            GameObjectFormatter_ObjectButton goFormatter_accessoryButton = _transform_accessoryButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_accessory = (_unit?.Accessory != null) ? new UnityAction(() => _infoPanelManager_accessory.InstantiateInfoPanel(_unit?.Accessory)) : null;
            goFormatter_accessoryButton.Format(_unit?.Accessory, null, buttonClickAction_accessory);
            if (_unit?.Accessory == null)
                goFormatter_accessoryButton.Image_Object.sprite = emptyEquipmentSlotSprite;
        }

        public static void FormatUnitEquipmentButtonsAsChangeable(Unit _unit, Transform _transform_mainWeaponButton, Transform _transform_subWeaponButton, Transform _transform_armourButton, Transform _transform_accessoryButton, InfoPanelManager_Weapon _infoPanelManager_weapon, InfoPanelManager_Armour _infoPanelManager_armour, InfoPanelManager_Accessory _infoPanelManager_accessory, bool _disableEquipmentChange)
        {
            Sprite emptyEquipmentSlotSprite = _disableEquipmentChange ? SpriteContainer.Instance.EmptyObjectSprite_NotSet : SpriteContainer.Instance.EmptyObjectSprite_Set;

            // Main weapon button
            GameObjectFormatter_ObjectButton goFormatter_mainWeaponButton = _transform_mainWeaponButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_mainWeapon = _disableEquipmentChange ? null : new UnityAction(delegate
            {
                GameDataContainer.Instance.SelectedUnit = _unit;
                GameDataContainer.Instance.EquipmentSelectionMode = eEquipmentSelectionMode.UnitMainWeapon;
                SceneConnector.GoToScene("scn_EquipmentList", true);
            });
            UnityAction buttonLongPressAction_mainWeapon = (_unit?.MainWeapon != null) ? new UnityAction(() => _infoPanelManager_weapon.InstantiateInfoPanel(_unit?.MainWeapon)) : null;
            goFormatter_mainWeaponButton.Format(_unit?.MainWeapon, null, buttonClickAction_mainWeapon, buttonLongPressAction_mainWeapon);
            if (_unit?.MainWeapon == null)
                goFormatter_mainWeaponButton.Image_Object.sprite = emptyEquipmentSlotSprite;

            // Sub weapon button
            GameObjectFormatter_ObjectButton goFormatter_subWeaponButton = _transform_subWeaponButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_subWeapon = _disableEquipmentChange ? null : new UnityAction(delegate
            {
                GameDataContainer.Instance.SelectedUnit = _unit;
                GameDataContainer.Instance.EquipmentSelectionMode = eEquipmentSelectionMode.UnitSubWeapon;
                SceneConnector.GoToScene("scn_EquipmentList", true);
            });
            UnityAction buttonLongPressAction_subWeapon = (_unit?.SubWeapon != null) ? new UnityAction(() => _infoPanelManager_weapon.InstantiateInfoPanel(_unit?.SubWeapon)) : null;
            goFormatter_subWeaponButton.Format(_unit?.SubWeapon, null, buttonClickAction_subWeapon, buttonLongPressAction_subWeapon);
            if (_unit?.SubWeapon == null)
                goFormatter_subWeaponButton.Image_Object.sprite = emptyEquipmentSlotSprite;

            // Armour button
            GameObjectFormatter_ObjectButton goFormatter_armourButton = _transform_armourButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_armour = _disableEquipmentChange ? null : new UnityAction(delegate
            {
                GameDataContainer.Instance.SelectedUnit = _unit;
                GameDataContainer.Instance.EquipmentSelectionMode = eEquipmentSelectionMode.UnitArmour;
                SceneConnector.GoToScene("scn_EquipmentList", true);
            });
            UnityAction buttonLongPressAction_armour = (_unit?.Armour != null) ? new UnityAction(() => _infoPanelManager_armour.InstantiateInfoPanel(_unit?.Armour)) : null;
            goFormatter_armourButton.Format(_unit?.Armour, null, buttonClickAction_armour, buttonLongPressAction_armour);
            if (_unit?.Armour == null)
                goFormatter_armourButton.Image_Object.sprite = emptyEquipmentSlotSprite;

            // Accessory button
            GameObjectFormatter_ObjectButton goFormatter_accessoryButton = _transform_accessoryButton.GetComponent<GameObjectFormatter_ObjectButton>();
            UnityAction buttonClickAction_accessory = _disableEquipmentChange ? null : new UnityAction(delegate
            {
                GameDataContainer.Instance.SelectedUnit = _unit;
                GameDataContainer.Instance.EquipmentSelectionMode = eEquipmentSelectionMode.UnitAccessory;
                SceneConnector.GoToScene("scn_EquipmentList", true);
            });
            UnityAction buttonLongPressAction_accessory = (_unit?.Accessory != null) ? new UnityAction(() => _infoPanelManager_accessory.InstantiateInfoPanel(_unit?.Accessory)) : null;
            goFormatter_accessoryButton.Format(_unit?.Accessory, null, buttonClickAction_accessory, buttonLongPressAction_accessory);
            if (_unit?.Accessory == null)
                goFormatter_accessoryButton.Image_Object.sprite = emptyEquipmentSlotSprite;
        }
    }
}
