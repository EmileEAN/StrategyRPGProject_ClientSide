using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class ConnectionStatusIndicator : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Text m_connectionText;

        [SerializeField]
        private int m_framesBetweenUpdate;
        #endregion

        #region Private Constant Fields
        private const string CONNECTED_MESSAGE = "Server is On";
        #endregion

        #region Private Fields
        private int m_frameCount;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_frameCount = 0;
        }

        // FixedUpdate is called once every frame
        void FixedUpdate()
        {
            if (m_frameCount == m_framesBetweenUpdate)
                m_frameCount = 0;

            if (m_frameCount == 0)
                StartCoroutine(CheckConnectionAndSetText());

            m_frameCount++;
        }

        public void ToAccountCreationSceneIfConnected() { if (m_connectionText.text == CONNECTED_MESSAGE) SceneConnector.GoToScene("scn_AccountCreation"); }

        IEnumerator CheckConnectionAndSetText()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "CheckConnection"},
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
                m_connectionText.text = "Please check your internet connection. If you are connected, server may be currently off/down.";
            else
                m_connectionText.text = CONNECTED_MESSAGE;
        }
    }
}