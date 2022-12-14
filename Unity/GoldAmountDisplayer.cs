using EEANWorks.Games.TBSG._01.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity
{
    [RequireComponent(typeof(Text))]
    public class GoldAmountDisplayer : MonoBehaviour
    {
        #region Private Fields
        private Text m_text_amount;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_text_amount = this.GetComponent<Text>();

            StartCoroutine(UpdateText());
        }

        IEnumerator UpdateText()
        {
            // Update text every second
            WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
            while (true)
            {
                m_text_amount.text = ToFormattedString(GameDataContainer.Instance.Player.GoldOwned);

                yield return wait;
            }
        }

        private string ToFormattedString(int _amount) { return (_amount < 1000000000) ? string.Format("{0:#,0}", _amount) : "999,999,999+"; }
    }
}
