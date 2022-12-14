using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class VersionValidator : MonoBehaviour
    {
        public static bool IsLatestGameVersion { get; private set; }

        #region Serialized Fields
        [SerializeField]
        private Text m_versionText;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_versionText.text = "Version: " + CoreValues.GAME_VERSION;

            IsLatestGameVersion = false;

            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            this.StartCoroutineRepetitionUntilTrue(CheckGameVersion(looperAndCoroutineLinker), looperAndCoroutineLinker);
        }

        IEnumerator CheckGameVersion(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "CheckGameVersion"},
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (!uwr.isNetworkError)
            {
                string response = uwr.downloadHandler.text;

                if (CoreValues.GAME_VERSION != response)
                    IsLatestGameVersion = true;
                else
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error", "The game has been updated. \nPlease download the latest version. ", "Open Download Page", () => Application.OpenURL("https://www.eeangames.com"));

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }
    }
}