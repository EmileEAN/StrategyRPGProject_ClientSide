using EEANWorks.Games.TBSG._01.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class DungeonRewardsSceneExitManager : MonoBehaviour
    {
        public void ExitScene()
        {
            GameDataContainer.Instance.DungeonToPlay = null;
            GameDataContainer.Instance.FloorInstanceInfos = null;

            if (GameDataContainer.Instance.EpisodeToPlay.NovelSceneAfterBattle != null) // If current episode has a post-battle novel scene
            {
                GameDataContainer.Instance.EpisodePhase = eEpisodePhase.PostBattleScene;
                GameDataContainer.Instance.EpisodeToPlay = null;
                SceneConnector.GoToScene("scn_Novel");
            }
            else
                SceneConnector.GoToScene("scn_StorySelection");
        }
    }
}