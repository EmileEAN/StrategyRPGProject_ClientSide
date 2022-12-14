using EEANWorks.Games.TBSG._01.Data;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EEANWorks.Games.TBSG._01.Unity
{
    public class GachaRollRequester : MonoBehaviour
    {
        #region Private Fields
        private List<object> m_gachaResultObjects;
        #endregion

        public void Request_RollGacha(Gacha _gacha, DispensationOption _dispensationOption)
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            this.StartCoroutineRepetitionUntilTrue(RollGacha(looperAndCoroutineLinker, _gacha, _dispensationOption), looperAndCoroutineLinker);
        }

        IEnumerator RollGacha(LooperAndCoroutineLinker _looperAndCoroutineLinker, Gacha _gacha, DispensationOption _dispensationOption)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "RollGacha"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
                    {"gachaId", _gacha.Id.ToString()},
                    {"dispensationOptionId", _dispensationOption.Id.ToString()}
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                if (response == "sessionExpired")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Session Error!", CoreValues.SESSION_ERROR_MESSAGE, "Return To Title", () => SceneConnector.GoToScene("scn_Title"));
                else if (response == "error")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error", "Someting went wrong!\nPlease try again.", "OK");
                else //Gacha succeeded!
                {
                    if (_dispensationOption.RemainingAttempts != -1) // If not -1, which means infinite
                        _dispensationOption.RemainingAttempts--; // Decrease number of remaining attempts

                    m_gachaResultObjects = ResponseStringToGachaResultObjects(response, _gacha.GachaClassification);

                    Player player = GameDataContainer.Instance.Player;
                    int cost = _dispensationOption.CostValue;
                    switch (_dispensationOption.CostType)
                    {
                        default: // case eCostType.Gem
                            player.GemsOwned -= cost;
                            break;
                        case eCostType.Gold:
                            player.GoldOwned -= cost;
                            break;
                        case eCostType.Item:
                            {
                                Item costItem = GameDataContainer.Instance.ItemEncyclopedia.First(x => x.Id == _dispensationOption.CostItemId);
                                player.ItemsOwned[costItem] -= cost;
                            }
                            break;
                    }

                    switch (_gacha.GachaClassification)
                    {
                        default: // case eGachaClassification.Unit
                            foreach (object gachaResultObject in m_gachaResultObjects) { player.UnitsOwned.Add(gachaResultObject as Unit); }
                            break;
                        case eGachaClassification.Weapon:
                            foreach (object gachaResultObject in m_gachaResultObjects) { player.WeaponsOwned.Add(gachaResultObject as Weapon); }
                            break;
                        case eGachaClassification.Armour:
                            foreach (object gachaResultObject in m_gachaResultObjects) { player.ArmoursOwned.Add(gachaResultObject as Armour); }
                            break;
                        case eGachaClassification.Accessory:
                            foreach (object gachaResultObject in m_gachaResultObjects) { player.AccessoriesOwned.Add(gachaResultObject as Accessory); }
                            break;
                        case eGachaClassification.SkillItem:
                        case eGachaClassification.SkillMaterial:
                        case eGachaClassification.ItemMaterial:
                        case eGachaClassification.EquipmentMaterial:
                        case eGachaClassification.EvolutionMaterial:
                        case eGachaClassification.WeaponEnhancementMaterial:
                        case eGachaClassification.UnitEnhancementMaterial:
                        case eGachaClassification.SkillEnhancementMaterial:
                            foreach (object gachaResultObject in m_gachaResultObjects)
                            {
                                Item item = gachaResultObject as Item;
                                if (player.ItemsOwned.ContainsKey(item))
                                    player.ItemsOwned[item] += 1;
                                else
                                    player.ItemsOwned.Add(item, 1);
                            }
                            break;
                    }

                    // Set the information that the GachaResult scene will need
                    GameDataContainer.Instance.GachaRolled = _gacha;
                    GameDataContainer.Instance.DispensationOptionSelected = _dispensationOption;
                    GameDataContainer.Instance.GachaResultObjects = m_gachaResultObjects;

                    _looperAndCoroutineLinker.SetTerminateLoopToTrue();

                    //Move to GachaResult scene
                    SceneConnector.GoToScene("scn_GachaResult");
                }
            }
        }

        private List<object> ResponseStringToGachaResultObjects(string _response, eGachaClassification _gachaClassification)
        {
            List<object> result = new List<object>();

            try
            {
                string sectionString = string.Empty;

                sectionString = _response.GetTagPortionWithoutOpeningAndClosingTags("Objects");
                while (sectionString != "")
                {
                    string objectString = string.Empty;
                    sectionString = sectionString.DetachTagPortion("Object", ref objectString);

                    object gachaResultObject = StringToGachaResultObject(objectString, _gachaClassification);
                    result.Add(gachaResultObject);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }

        private object StringToGachaResultObject(string _string, eGachaClassification _gachaClassification)
        {
            try
            {
                // Get Id of the object
                int objectId = _string.GetTagPortionValue<int>("Id");

                // Get uniqueId for applicable object
                int uniqueId = default;
                if (_gachaClassification == eGachaClassification.Unit
                    || _gachaClassification == eGachaClassification.Weapon
                    || _gachaClassification == eGachaClassification.Armour
                    || _gachaClassification == eGachaClassification.Accessory)
                {
                    uniqueId = _string.GetTagPortionValue<int>("UniqueId");
                }

                // Get objectBase of the object
                IRarityMeasurable objectBase = default;
                switch (_gachaClassification)
                {
                    default: // case eGachaClassification.Unit
                        objectBase = GameDataContainer.Instance.UnitEncyclopedia.ToList<UnitData>().First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.Weapon:
                        objectBase = GameDataContainer.Instance.WeaponEncyclopedia.ToList<WeaponData>().First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.Armour:
                        objectBase = GameDataContainer.Instance.WeaponEncyclopedia.ToList<ArmourData>().First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.Accessory:
                        objectBase = GameDataContainer.Instance.WeaponEncyclopedia.ToList<AccessoryData>().First(x => x.Id == objectId);
                        break;
                    case eGachaClassification.SkillItem:
                    case eGachaClassification.SkillMaterial:
                    case eGachaClassification.ItemMaterial:
                    case eGachaClassification.EquipmentMaterial:
                    case eGachaClassification.EvolutionMaterial:
                    case eGachaClassification.WeaponEnhancementMaterial:
                    case eGachaClassification.UnitEnhancementMaterial:
                    case eGachaClassification.SkillEnhancementMaterial:
                        objectBase = GameDataContainer.Instance.WeaponEncyclopedia.ToList<Item>().First(x => x.Id == objectId);
                        break;
                }

                // Get accumulatedExperience for applicable object
                int accumulatedExperience = default;
                {
                    bool isUnit = _gachaClassification == eGachaClassification.Unit;
                    bool isWeapon = _gachaClassification == eGachaClassification.Weapon;
                    bool isLevelableWeapon = false;
                    {
                        if (isWeapon)
                        {
                            if (!(objectBase is WeaponData weaponData))
                                return null;

                            if (weaponData.WeaponType == eWeaponType.Levelable || weaponData.WeaponType == eWeaponType.LevelableTransformable)
                            {
                                isLevelableWeapon = true;
                            }
                        }
                    }

                    if (isUnit || isLevelableWeapon)
                    {
                        accumulatedExperience = _string.GetTagPortionValue<int>("AccumulatedExperience");
                    }
                }

                // Return gacha result object
                switch (_gachaClassification)
                {
                    default: // case eGachaClassification.Unit
                        return new Unit((UnitData)objectBase, uniqueId, string.Empty, accumulatedExperience, false);
                    case eGachaClassification.Weapon:
                        {
                            if (!(objectBase is WeaponData weaponData))
                                return null;
                            else
                            {
                                switch (weaponData.WeaponType)
                                {
                                    default: // case eWeaponType.Ordinary
                                        return new OrdinaryWeapon(weaponData, uniqueId, false);
                                    case eWeaponType.Levelable:
                                        return new LevelableWeapon(weaponData, uniqueId, false, accumulatedExperience);
                                    case eWeaponType.Transformable:
                                        return new TransformableWeapon(weaponData, uniqueId, false);
                                    case eWeaponType.LevelableTransformable:
                                        return new LevelableTransformableWeapon(weaponData, uniqueId, false, accumulatedExperience);
                                }
                            }
                        }
                    case eGachaClassification.Armour:
                        return new Armour((ArmourData)objectBase, uniqueId, false);
                    case eGachaClassification.Accessory:
                        return new Accessory((AccessoryData)objectBase, uniqueId, false);
                    case eGachaClassification.SkillItem:
                    case eGachaClassification.SkillMaterial:
                    case eGachaClassification.ItemMaterial:
                    case eGachaClassification.EquipmentMaterial:
                    case eGachaClassification.EvolutionMaterial:
                        return objectBase;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }
    }
}
