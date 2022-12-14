using UnityEngine.Events;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    public class GameObjectFormatter_ItemButton : GameObjectFormatter_ObjectButton
    {
        #region Properties
        public Button Button_Plus { get; private set; }
        public Button Button_Minus { get; private set; }
        #endregion

        // Awake is called before the first Update
        void Awake()
        {
            Button_Plus = this.transform.Find("Button@Plus").GetComponent<Button>();
            Button_Minus = this.transform.Find("Button@Minus").GetComponent<Button>();
        }

        public void Format(object _object, string _valueTextString = null, UnityAction _buttonClickAction = null, UnityAction _buttonLongPressAction = null, UnityAction _plusButtonPressAction = null, UnityAction _minusButtonPressAction = null)
        {
            if (_object != null)
            {
                base.Format(_object, _valueTextString, _buttonClickAction, _buttonLongPressAction);

                if (_plusButtonPressAction != null)
                {
                    Button_Plus.onClick.AddListener(_plusButtonPressAction);
                    Button_Plus.interactable = true;
                }
                else
                    Button_Plus.interactable = false;

                if (_minusButtonPressAction != null)
                {
                    Button_Minus.onClick.AddListener(_minusButtonPressAction);
                    Button_Minus.interactable = true;
                }
                else
                    Button_Minus.interactable = false;
            }
        }
    }
}
