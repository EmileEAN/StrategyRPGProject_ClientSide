using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    namespace EEANWorks.Games.TBSG._01.Unity
    {
        public class SceneLoader : MonoBehaviour
        {
            #region Serialized Fields
            public Text LoadingText;
            public string Text;
            #endregion

            #region Private Fields
            private bool m_isSceneLoaded;
            #endregion

            // Awake is called before Update for the first frame
            void Awake()
            {
                m_isSceneLoaded = false;
            }

            // Updates once per frame
            void FixedUpdate()
            {
                if (!m_isSceneLoaded)
                {
                    m_isSceneLoaded = true;

                    LoadingText.text = Text;

                    StartCoroutine(LoadNewScene());
                }
                else // If the new scene has started loading
                {
                    // Change the transparency of the loading text to let the player know that the application is still working.
                    LoadingText.color = new Color(LoadingText.color.r, LoadingText.color.g, LoadingText.color.b, Mathf.PingPong(Time.time, 1));
                }
            }


            #region Coroutines
            // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
            IEnumerator LoadNewScene()
            {
                //yield return new WaitForSeconds(2);

                // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
                AsyncOperation async = SceneManager.LoadSceneAsync(SceneConnector.m_nextScene);

                // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
                while (!async.isDone)
                {
                    yield return null;
                }
            }
            #endregion
        }
    }
}