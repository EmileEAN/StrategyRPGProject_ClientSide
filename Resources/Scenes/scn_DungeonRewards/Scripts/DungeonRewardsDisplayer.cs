using EEANWorks;
using EEANWorks.Games.TBSG._01;
using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.TBSG._01.Unity.UI;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using CoreValues = EEANWorks.Games.TBSG._01.CoreValues;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(InfoPanelManager_Unit))]
    [RequireComponent(typeof(InfoPanelManager_Item))]
    public class DungeonRewardsDisplayer : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Text m_rankExpText;
        [SerializeField]
        private Text m_goldText;
        [SerializeField]
        private Transform m_objectButtonsContainerTransform;
        #endregion

        #region Private Fields
        private GameObject m_objectButtonPrefab;

        private InfoPanelManager_Unit m_infoPanelManager_unit;
        private InfoPanelManager_Item m_infoPanelManager_item;

        private int m_rankExp;
        private int m_gold;
        private List<Unit> m_droppedUnits;
        private Dictionary<Item, int> m_droppedItems;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_objectButtonPrefab = SharedAssetContainer.Instance.ObjectButtonPrefab;

            m_infoPanelManager_unit = this.GetComponent<InfoPanelManager_Unit>();
            m_infoPanelManager_item = this.GetComponent<InfoPanelManager_Item>();

            m_droppedUnits = new List<Unit>();
            m_droppedItems = new Dictionary<Item, int>();

            StartCoroutine(Initialize());
        }

        IEnumerator Initialize()
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(LoadDungeonRewards(looperAndCoroutineLinker), looperAndCoroutineLinker);

            m_rankExpText.text = m_rankExp.ToString();
            yield return new WaitForSecondsRealtime(0.5f);

            m_goldText.text = m_gold.ToString();
            yield return new WaitForSecondsRealtime(0.5f);

            foreach (Unit droppedUnit in m_droppedUnits)
            {
                GameObject go_objectButton = Instantiate(m_objectButtonPrefab, m_objectButtonsContainerTransform);
                GameObjectFormatter_ObjectButton goFormatter_objectButton = go_objectButton.GetComponent<GameObjectFormatter_ObjectButton>();
                Unit tmp_unit = droppedUnit; // Variable to avoid reference error within lambda expression
                goFormatter_objectButton.Format(droppedUnit, Calculator.Level(droppedUnit).ToString(), () => { m_infoPanelManager_unit.InstantiateInfoPanel(tmp_unit, true); });

                yield return new WaitForSecondsRealtime(0.2f);
            }

            foreach (var entry in m_droppedItems)
            {
                GameObject go_objectButton = Instantiate(m_objectButtonPrefab, m_objectButtonsContainerTransform);
                GameObjectFormatter_ObjectButton goFormatter_objectButton = go_objectButton.GetComponent<GameObjectFormatter_ObjectButton>();
                Item tmp_item = entry.Key; // Variable to avoid reference error within lambda expression
                goFormatter_objectButton.Format(entry.Key, entry.Value.ToString(), () => { m_infoPanelManager_item.InstantiateInfoPanel(tmp_item); });

                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        IEnumerator LoadDungeonRewards(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "LoadDungeonRewards"},
                    {"sessionId", GameDataContainer.Instance.SessionId.ToString()}
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
                else if (response == "notExists")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "The dungeon does not exist!", "Return To Battle Mode Selection", () => SceneConnector.GoToScene("scn_BattleMode"));
                else
                {
                    try
                    {
                        string[] rewardsStrings = response.Split(';');

                        m_rankExp = Convert.ToInt32(rewardsStrings[0]);

                        m_gold = Convert.ToInt32(rewardsStrings[1]);

                        if (rewardsStrings[2] != "") // If unit section is not empty
                        {
                            IList<UnitData> unitEncyclopedia = GameDataContainer.Instance.UnitEncyclopedia;
                            string[] unitStrings = rewardsStrings[2].Split('/');
                            foreach (string unitString in unitStrings)
                            {
                                string[] valueStrings = unitString.Split(',');
                                int unitId = Convert.ToInt32(valueStrings[0]);
                                UnitData unitData = unitEncyclopedia.First(x => x.Id == unitId);
                                int unitUniqueId = Convert.ToInt32(valueStrings[1]);
                                int unitAccumulatedExp = Convert.ToInt32(valueStrings[2]);

                                Unit droppedUnit = new Unit(unitData, unitUniqueId, "", unitAccumulatedExp, false);
                                GameDataContainer.Instance.Player.UnitsOwned.Add(droppedUnit);
                                m_droppedUnits.Add(droppedUnit);
                            }
                        }

                        if (rewardsStrings[3] != "") // If item section is not empty
                        {
                            IList<Item> itemEncylopedia = GameDataContainer.Instance.ItemEncyclopedia;
                            string[] itemStrings = rewardsStrings[3].Split('/');
                            foreach (string itemString in itemStrings)
                            {
                                string[] valueStrings = itemString.Split(',');
                                int itemId = Convert.ToInt32(valueStrings[0]);
                                Item item = itemEncylopedia[itemId];
                                int quantity = Convert.ToInt32(valueStrings[1]);

                                GameDataContainer.Instance.Player.ItemsOwned.Sum(item, quantity);
                                m_droppedItems.Sum(item, quantity);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        response = "loadingError";
                    }
                    if (response == "loadingError")
                        PopUpWindowManager.Instance.CreateSimplePopUp("Loading Error!", "Failed to load newly obtained unit data!", "Return To Title", () => SceneConnector.GoToScene("scn_Title"));
                }

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }
    }
}
