using EEANWorks.Games.TBSG._01.Data;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class MatchingManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Text m_loadingText;
        [SerializeField]
        private string m_text;
        #endregion

        #region Private Fields
        private bool m_startedMatching;
        private bool m_isMatching;

        private string m_errorMsg_cancelMatching;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_startedMatching = false;
            m_isMatching = false;

            StartCoroutine(TryStartMatching());
        }

        // Updates once per frame
        void FixedUpdate()
        {
            // If the new scene has started loading...
            if (m_isMatching)
            {
                if (m_loadingText.text != m_text)
                    m_loadingText.text = m_text;

                // ...then pulse the transparency of the loading text to let the player know that the application is still working.
                m_loadingText.color = new Color(m_loadingText.color.r, m_loadingText.color.g, m_loadingText.color.b, Mathf.PingPong(Time.time, 1));

                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                this.StartCoroutineRepetitionUntilTrue(CheckMatchingStatus(looperAndCoroutineLinker), looperAndCoroutineLinker);
            }

        }

        IEnumerator TryStartMatching()
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(StartMatching(looperAndCoroutineLinker), looperAndCoroutineLinker);
        }

        IEnumerator StartMatching(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "StartMatching"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
                    {"teamIndex", 0.ToString()}
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            m_errorMsg_cancelMatching = string.Empty;

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "Return To Selection Menu", () => SceneConnector.GoToPreviousScene(true));
            }
            else
            {
                string response = uwr.downloadHandler.text;

                switch (response)
                {
                    case "matching":
                        m_startedMatching = true;
                        m_isMatching = true;
                        break;

                    case "alreadyInMatch":
                        m_errorMsg_cancelMatching = "The player is already in match!";
                        break;

                    case "alreadyWaiting":
                        m_errorMsg_cancelMatching = "Matching request was already recived!";
                        break;

                    case "sessionExpired":
                        m_errorMsg_cancelMatching = CoreValues.SESSION_ERROR_MESSAGE;
                        break;

                    default:
                        m_errorMsg_cancelMatching = "Something went wrong!";
                        break;
                }

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }

            if (m_errorMsg_cancelMatching == CoreValues.SESSION_ERROR_MESSAGE)
                PopUpWindowManager.Instance.CreateSimplePopUp("Session Error!", m_errorMsg_cancelMatching, "Return To Title", () => SceneConnector.GoToScene("scn_Title"));
            else if (m_errorMsg_cancelMatching != "")
                PopUpWindowManager.Instance.CreateSimplePopUp("Error!", m_errorMsg_cancelMatching, "Return to Selection Menu", () => SceneConnector.GoToPreviousScene(), true);
        }

        IEnumerator CheckMatchingStatus(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "CheckMatchingStatus"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            string errorMsg = string.Empty;

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "Return To Selection Menu", () => SceneConnector.GoToPreviousScene(true));
            }
            else
            {
                string response = uwr.downloadHandler.text;

                if (response == "matched")
                {
                    _looperAndCoroutineLinker.SetTerminateLoopToTrue();

                    SceneConnector.GoToScene("scn_MultiplayerBattle");
                }
                else if (response == "sessionExpired")
                    errorMsg = CoreValues.SESSION_ERROR_MESSAGE;
                else
                    errorMsg = "Something went wrong!";

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }

            if (errorMsg == CoreValues.SESSION_ERROR_MESSAGE)
                PopUpWindowManager.Instance.CreateSimplePopUp("Session Error!", errorMsg, "Return To Title", () => SceneConnector.GoToScene("scn_Title"));
            else if (m_errorMsg_cancelMatching != "")
                PopUpWindowManager.Instance.CreateSimplePopUp("Error!", m_errorMsg_cancelMatching, "Return to Selection Menu", () => SceneConnector.GoToPreviousScene(), true);
        }

        public void Request_CancelMatching()
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            this.StartCoroutineRepetitionUntilTrue(CancelMatching(looperAndCoroutineLinker), looperAndCoroutineLinker);
        }
        IEnumerator CancelMatching(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "CancelMatching"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            m_errorMsg_cancelMatching = string.Empty;

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection and try again.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                switch (response)
                {
                    case "cancelled":
                        {
                            _looperAndCoroutineLinker.SetTerminateLoopToTrue();

                            SceneConnector.GoToPreviousScene();
                        }
                        break;

                    case "alreadyInMatch":
                        m_errorMsg_cancelMatching = "The player is already in match!";
                        break;

                    case "sessionExpired":
                        m_errorMsg_cancelMatching = CoreValues.SESSION_ERROR_MESSAGE;
                        break;

                    default:
                        m_errorMsg_cancelMatching = "Something went wrong!";
                        break;
                }

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }

            if (m_errorMsg_cancelMatching == CoreValues.SESSION_ERROR_MESSAGE)
                PopUpWindowManager.Instance.CreateSimplePopUp("Session Error!", m_errorMsg_cancelMatching, "Return To Title", () => SceneConnector.GoToScene("scn_Title"));
            else if (m_errorMsg_cancelMatching != "")
                PopUpWindowManager.Instance.CreateSimplePopUp("Error!", m_errorMsg_cancelMatching, "OK");
        }
    }
}