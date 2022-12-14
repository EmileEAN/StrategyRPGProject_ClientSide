using EEANWorks.Games.TBSG._01.Data;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.Connection
{
    /// <summary>
    /// This checks whether an update is required on Awake(); and starts a coroutine to load the required data from the server if necessary.
    /// </summary>
    public class GameUpdateManager : MonoBehaviour
    {
        #region Serialized Fields
        public Text Text_GameLoading;
        #endregion

        #region Properties
        public bool IsWorking { get; private set; }
        #endregion

        #region Private Fields
        private static int m_latestUpdateNum = 0;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            IsWorking = true;
            Text_GameLoading.gameObject.SetActive(true);
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            this.StartCoroutineRepetitionUntilTrue(CheckForUpdate(looperAndCoroutineLinker), looperAndCoroutineLinker);
        }

        IEnumerator CheckForUpdate(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "CheckForUpdate"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
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
                else // response is the update number
                {
                    int updateNum = Convert.ToInt32(response);
                    if (m_latestUpdateNum != updateNum) // An update is required
                    {
                        GameDataLoader gameDataLoader = new GameDataLoader();
                        LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                        yield return this.StartCoroutineRepetitionUntilTrue(gameDataLoader.LoadGachaData(looperAndCoroutineLinker), looperAndCoroutineLinker);

                        m_latestUpdateNum = updateNum;
                    }

                    IsWorking = false;
                    Text_GameLoading.gameObject.SetActive(false);
                    _looperAndCoroutineLinker.SetTerminateLoopToTrue();
                }
            }
        }
    }
}
