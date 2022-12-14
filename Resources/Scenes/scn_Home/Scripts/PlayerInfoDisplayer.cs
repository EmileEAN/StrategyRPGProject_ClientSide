using EEANWorks.Games.TBSG._01.Data;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class PlayerInfoDisplayer : MonoBehaviour
    {
        // Awake is called before the first frame update
        void Awake()
        {
            GameObject go_canvas = GameObject.FindGameObjectWithTag("Canvas");
            Transform transform_playerInfo = go_canvas?.transform.Find("Panel@HomeMenu")?.Find("PlayerInfo");

            Player player = GameDataContainer.Instance.Player;
            if (player != null && transform_playerInfo != null)
            {
                Text text_playerName = transform_playerInfo.Find("Text@PlayerName")?.GetComponent<Text>();
                text_playerName.text = player.Name;

                Text text_goldAmount = transform_playerInfo.Find("Gold")?.Find("Panel@Amount")?.Find("Text@Amount")?.GetComponent<Text>();
                text_goldAmount.text = (player.GoldOwned < 1000000000) ? string.Format("{0:#,0}", player.GoldOwned) : "999,999,999+";

                Text text_gemsAmount = transform_playerInfo.Find("Gems")?.Find("Panel@Amount")?.Find("Text@Amount")?.GetComponent<Text>();
                text_gemsAmount.text = (player.GemsOwned < 1000000) ? string.Format("{0:#,0}", player.GemsOwned) : "999,999+";
            }
        }
    }
}