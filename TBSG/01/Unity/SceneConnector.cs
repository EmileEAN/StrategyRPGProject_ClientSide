using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.SceneSpecific;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EEANWorks.Games.TBSG._01.Unity
{
    //Referenced by components in the editor
    public class SceneConnector : MonoBehaviour
    {
        #region Properties
        public static int SceneChangeCount { get; private set; } = 0;
        #endregion
        
        #region Public Fields
        public static string m_nextScene = DEFAULT_SCENE;
        #endregion

        #region Private Fields
        private const string DEFAULT_SCENE = "scn_Title";

        private static List<string> m_sceneLog = new List<string>();
        #endregion

        #region Public Methods
        public void ToStorySelectionScene(bool _showMainStories) { GameDataContainer.Instance.ShowMainStories = _showMainStories; GoToScene("scn_StorySelection"); }
        public void ToSceneViaLoaderScene(string _nameOfSceneToLoad) { GoToScene(_nameOfSceneToLoad); }
        public void ToSceneDirectly(string _nameOfSceneToLoad) { GoToScene(_nameOfSceneToLoad, true); }
        public static void GoToScene(string _nameOfSceneToLoad, bool _skipLoaderScene = false)
        {
            m_sceneLog.Add(_nameOfSceneToLoad);
            SceneChangeCount++;
            if (_skipLoaderScene)
                SceneManager.LoadScene(_nameOfSceneToLoad);
            else
            {
                m_nextScene = _nameOfSceneToLoad;
                SceneManager.LoadScene("scn_Loader");
            }
        }

        public void ToPreviousSceneViaLoaderScene() { GoToPreviousScene(); }
        public void ToPreviousSceneDirectly() { GoToPreviousScene(true); }
        public static void GoToPreviousScene(bool _skipLoaderScene = false)
        {
            m_sceneLog.RemoveLast();

            if (_skipLoaderScene)
                SceneManager.LoadScene(m_sceneLog.Last());
            else
            {
                m_nextScene = m_sceneLog.Last();
                SceneManager.LoadScene("scn_Loader");
            }
        }

        public void SetUnitSelectionModeToList() { GameDataContainer.Instance.UnitSelectionMode = eUnitSelectionMode.List; }
        public void SetEquipmentSelectionModeToList() { GameDataContainer.Instance.EquipmentSelectionMode = eEquipmentSelectionMode.List; }
        public void SetItemSelectionModeToList() { GameDataContainer.Instance.ItemSelectionMode = eItemSelectionMode.List; }
        #endregion
    }
}
