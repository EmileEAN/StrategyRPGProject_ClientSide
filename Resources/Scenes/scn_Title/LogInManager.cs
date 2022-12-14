using EEANWorks.Games.TBSG._01.Unity.Connection;
using EEANWorks.Games.Unity.Engine;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    namespace EEANWorks.Games.TBSG._01.Unity
    {
        public class LogInManager : MonoBehaviour
        {
            #region Serialized Fields
            [SerializeField]
            private InputField m_userNameInputField;
            [SerializeField]
            private InputField m_passwordInputField;
            [SerializeField]
            private Button m_logInButton;
            #endregion

            #region Private Fields
            private Text m_text_logInButton;

            private readonly int m_minimum_userId_length = 8;
            private readonly int m_minimum_password_length = 8;
            #endregion

            void Awake()
            {
                m_text_logInButton = m_logInButton.GetComponentInChildren<Text>();

                SpriteContainer.Instance.ResetIconsDownloadFlag();
                GameDataContainer.Instance.ResetInitializationFlag();
            }

            public void Request_CheckCredentialsValidityAndAttemptLogIn()
            {
                m_text_logInButton.text = "Checking Credentials...";

                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                this.StartCoroutineRepetitionUntilTrue(CheckCredentialsValidityAndAttemptLogIn(looperAndCoroutineLinker), looperAndCoroutineLinker);
            }

            IEnumerator CheckCredentialsValidityAndAttemptLogIn(LooperAndCoroutineLinker _looperAndCoroutineLinker)
            {
                if (!m_logInButton.interactable) // If already being processed
                    yield break; // Disable double click

                m_logInButton.interactable = false;

                string userName = m_userNameInputField.text;
                string password = m_passwordInputField.text;

                string errorMsg = string.Empty;

                if (userName.Length < m_minimum_userId_length)
                    errorMsg = "A User Name contains at least " + m_minimum_userId_length.ToString() + " characters!\n";

                if (password.Length < m_minimum_password_length)
                    errorMsg += "A Password contains at least " + m_minimum_password_length.ToString() + " characters!\n";

                if (errorMsg == "")
                {
                    Dictionary<string, string> values = new Dictionary<string, string>
                        {
                            {"subject", "CheckPlayerCredentialsValidityAndAttemptToLogin"},
                            {"userName", userName},
                            {"password", password}
                        };

                    UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
                    yield return uwr.SendWebRequest();

                    if (uwr.isNetworkError)
                    {
                        if (_looperAndCoroutineLinker.IsEndOfLoop)
                        {
                            PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
                            m_text_logInButton.text = "Log In";
                        }
                    }
                    else
                    {
                        string response = uwr.downloadHandler.text;

                        switch (response)
                        {
                            case "currentlyInUse":
                                errorMsg = "A session for the account already exists!"
                                        + "\nSomeone else might be using your account!"
                                        + "\nIf you had just logged in, please try again after a few minutes.";
                                break;

                            case "wrongPassword":
                                errorMsg = "The password is wrong!";
                                break;

                            case "noAccount":
                                errorMsg = "There is no account associated to the given user name!";
                                break;

                            default: // case "success:..." ('...' is the unique sessionId)
                                {
                                    m_text_logInButton.text = "Loading Data...";
                                    yield return null;

                                    string sessionId = response.Remove("success:");

                                    GameDataLoader gameDataLoader = new GameDataLoader();

                                    LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                                    yield return this.StartCoroutineRepetitionUntilTrue(gameDataLoader.LoadCoreGameData(looperAndCoroutineLinker, userName, password, sessionId), looperAndCoroutineLinker);

                                    if (gameDataLoader.IsPlayerLoaded)
                                    {
                                        if (gameDataLoader.SaveLoadedData(sessionId))
                                        {
                                            _looperAndCoroutineLinker.SetTerminateLoopToTrue();

                                            SceneConnector.GoToScene("scn_Home");
                                        }
                                        else
                                            errorMsg = "Failed to save loaded data to application. \nPlease try again.";
                                    }
                                    else
                                        errorMsg = "Credentials are valid, but an unexpected issue has occurred while loading data! \nPlease try again.";
                                }
                                break;
                        }

                        _looperAndCoroutineLinker.SetTerminateLoopToTrue();
                    }
                }

                if (errorMsg != "")
                {
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error", errorMsg, "OK");
                    m_text_logInButton.text = "Log In";
                }

                m_logInButton.interactable = true;
            }
        }
    }
}