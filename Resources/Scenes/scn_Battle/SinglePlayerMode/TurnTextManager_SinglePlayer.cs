using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(Text))]
    public class TurnTextManager_SinglePlayer : MonoBehaviour
    {
        #region Private Fields
        private const string MY_TURN_TEXT = "Your Turn!";
        private const string OPPONENT_TURN_TEXT = "Opponent's Turn!";

        private Text m_text;

        private UnityBattleSystem_SinglePlayer m_mainScript;

        private bool m_isInitialized;
        #endregion

        // Awake is called before the Update for the first frame
        void Awake()
        {
            m_isInitialized = false;

            m_text = this.GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();

            if (m_isInitialized)
            {
                if (m_text.text != MY_TURN_TEXT && m_mainScript.PlayerController.IsMyTurn)
                    m_text.text = MY_TURN_TEXT;
                else if (m_text.text != OPPONENT_TURN_TEXT && !m_mainScript.PlayerController.IsMyTurn)
                    m_text.text = OPPONENT_TURN_TEXT;
            }
        }

        private void Initialize()
        {
            if (m_mainScript == null)
                m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_SinglePlayer>();
            if (m_mainScript == null)
                return;

            if (!m_mainScript.IsInitialized)
                return;

            m_isInitialized = true;
        }
    }
}