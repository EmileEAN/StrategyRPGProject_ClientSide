using EEANWorks.Games.TBSG._01.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EEANWorks.Games.TBSG._01.Unity.Connection
{
    public class ConnectionRetainer : MonoBehaviour
    {
        #region Private Fields
        private int m_numOfFramesUntilNextUpdate;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_numOfFramesUntilNextUpdate = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (m_numOfFramesUntilNextUpdate == 0)
            {
                m_numOfFramesUntilNextUpdate = CoreValues.FRAMES_BEFORE_REUPDATING_CONNECTION;
                StartCoroutine(UpdateConnectionStatus());
            }

            m_numOfFramesUntilNextUpdate--;
        }

        IEnumerator UpdateConnectionStatus()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "UpdateConnectionStatus"},
                    {"sessionId", GameDataContainer.Instance.SessionId},
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();
        }
    }
}
