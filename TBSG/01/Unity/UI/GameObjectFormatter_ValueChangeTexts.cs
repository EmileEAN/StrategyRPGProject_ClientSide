using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    public class GameObjectFormatter_ValueChangeTexts : MonoBehaviour
    {
        #region Properties
        public Text Text_Label { get; private set; }
        public Text Text_InitialValue { get; private set; }
        public Text Text_EventualValue { get; private set; }
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            Text_Label = this.transform.Find("Text@Label").GetComponent<Text>();
            Text_InitialValue = this.transform.Find("Text@InitialValue").GetComponent<Text>();
            Text_EventualValue = this.transform.Find("Text@EventualValue").GetComponent<Text>();
        }

        public void Format(string _initialValue, string _eventualValue)
        {
            Text_InitialValue.text = _initialValue;
            Text_EventualValue.text = _eventualValue;
        }

        public void SetEventualToInitial()
        {
            Text_InitialValue.text = Text_EventualValue.text;
        }
    }
}
