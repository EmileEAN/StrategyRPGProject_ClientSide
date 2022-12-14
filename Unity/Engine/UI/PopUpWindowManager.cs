using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Engine.UI
{
    public sealed class PopUpWindowManager
    {
        private static PopUpWindowManager m_instance;

        public static PopUpWindowManager Instance { get { return m_instance ?? (m_instance = new PopUpWindowManager()); } }

        private PopUpWindowManager() { }

        #region Private Fields
        private GameObject m_popUpWindowPrefab;
        private GameObject m_buttonPrefab;

        private List<GameObject> m_gos_popUpWindow;
        #endregion


        #region Public Functions
        public void Initialize(GameObject _popUpWindowPrefab, GameObject _buttonPrefab)
        {
            m_popUpWindowPrefab = _popUpWindowPrefab;
            m_buttonPrefab = _buttonPrefab;

            m_gos_popUpWindow = new List<GameObject>();
        }

        public void CreateSimplePopUp(string _title, string _message, string _ok, UnityAction _method = null, bool _hideCloseButton = false)
        {
            GameObject go_popUpWindow = null;
            Transform transform_buttons = null;
            DynamicGridLayoutGroup gridLayoutGroup_buttons = null;

            CreatePopUp(_title, _message, ref go_popUpWindow, ref transform_buttons, ref gridLayoutGroup_buttons, _hideCloseButton);

            gridLayoutGroup_buttons.FixedNumOfElementsPerRow = true;
            gridLayoutGroup_buttons.FixedNumOfElementsPerColumn = true;
            gridLayoutGroup_buttons.ElementsPerRow = 1;
            gridLayoutGroup_buttons.ElementsPerColumn = 1;
            gridLayoutGroup_buttons.HorizontalAlignment = DynamicGridLayoutGroup.eHorizontalAlignment.Center;
            gridLayoutGroup_buttons.ElementSizeX = new ValueSet_Float_ElementSize { m_valueType = eValueTypeForElementSize.RelativeToParent, m_value = 0.5f };
            gridLayoutGroup_buttons.ElementSizeY = new ValueSet_Float_ElementSize { m_valueType = eValueTypeForElementSize.RelativeToParent, m_value = 1f };

            GameObject go_okButton = GameObject.Instantiate(m_buttonPrefab, transform_buttons);
            Button button_ok = go_okButton.GetComponent<Button>();
            if (_method != null)
                button_ok.onClick.AddListener(_method);
            button_ok.onClick.AddListener(() => ClosePopUp(go_popUpWindow));
            go_okButton.transform.Find("Text").GetComponent<Text>().text = _ok;
        }

        public void CreateYesNoPopUp(string _title, string _message, string _yes, string _no, UnityAction _yesMethod, UnityAction _noMethod = null, bool _hideCloseButton = false)
        {
            GameObject go_popUpWindow = null;
            Transform transform_buttons = null;
            DynamicGridLayoutGroup gridLayoutGroup_buttons = null;

            CreatePopUp(_title, _message, ref go_popUpWindow, ref transform_buttons, ref gridLayoutGroup_buttons, _hideCloseButton);

            gridLayoutGroup_buttons.FixedNumOfElementsPerRow = true;
            gridLayoutGroup_buttons.FixedNumOfElementsPerColumn = true;
            gridLayoutGroup_buttons.ElementsPerRow = 2;
            gridLayoutGroup_buttons.ElementsPerColumn = 1;
            gridLayoutGroup_buttons.HorizontalAlignment = DynamicGridLayoutGroup.eHorizontalAlignment.Center;
            gridLayoutGroup_buttons.AutomaticElementWidth = true;
            gridLayoutGroup_buttons.ElementSizeY = new ValueSet_Float_ElementSize { m_valueType = eValueTypeForElementSize.RelativeToParent, m_value = 1f };
            gridLayoutGroup_buttons.SpacingX = new ValueSet_Float_HorizontalSpacing { m_valueType = eValueTypeForHorizontalSpacing.RelativeToParent, m_value = 0.05f };

            GameObject go_yesButton = GameObject.Instantiate(m_buttonPrefab, transform_buttons);
            Button button_yes = go_yesButton.GetComponent<Button>();
            if (_yesMethod != null)
                button_yes.onClick.AddListener(_yesMethod);
            button_yes.onClick.AddListener(() => ClosePopUp(go_popUpWindow));
            go_yesButton.transform.Find("Text").GetComponent<Text>().text = _yes;

            GameObject go_noButton = GameObject.Instantiate(m_buttonPrefab, transform_buttons);
            Button button_no = go_noButton.GetComponent<Button>();
            if (_noMethod != null)
                button_no.onClick.AddListener(_noMethod);
            button_no.onClick.AddListener(() => ClosePopUp(go_popUpWindow));
            go_noButton.transform.Find("Text").GetComponent<Text>().text = _no;
        }
        #endregion

        #region Private Functions
        private void CreatePopUp(string _title, string _message, ref GameObject _go_popUpWindow, ref Transform _transform_buttons, ref DynamicGridLayoutGroup _buttonsPanelGridLayout, bool _hideCloseButton = false)
        {
            GameObject go_canvas = GameObject.FindGameObjectWithTag("Canvas");

            _go_popUpWindow = GameObject.Instantiate(m_popUpWindowPrefab, go_canvas.transform);

            Transform transform_window = _go_popUpWindow.transform.Find("Panel@Mask").Find("Panel@Window");

            Transform transform_closeButton = transform_window.Find("Close");
            transform_closeButton.GetComponent<DynamicGridLayoutGroup>().ScriptExecutionPriorityLevel = 1;
            Button button_close = transform_closeButton.Find("Button@Close").GetComponent<Button>();
            if (_hideCloseButton)
                button_close.gameObject.SetActive(false);
            else
            {
                GameObject go_popUpWindow = _go_popUpWindow; //Put the reference into another variable in order to use it within the lambda expression below
                button_close.onClick.AddListener(() => ClosePopUp(go_popUpWindow));
            }

            Text text_title = transform_window.Find("Panel@Title").Find("Text").GetComponent<Text>();
            text_title.text = _title;

            Text text_message = transform_window.Find("Message").FindChildWithTag("ScrollMenu").Find("Contents").Find("Text@Message").GetComponent<Text>();
            text_message.text = _message.Replace("\\n", "\n");

            _transform_buttons = transform_window.Find("Buttons");
            _buttonsPanelGridLayout = _transform_buttons.GetComponent<DynamicGridLayoutGroup>();
            _buttonsPanelGridLayout.ScriptExecutionPriorityLevel = 1; // It will always be instantiated as child of canvas

            m_gos_popUpWindow.Add(_go_popUpWindow);
        }

        private void ClosePopUp(GameObject _go_popUpWindow)
        {
            if (m_gos_popUpWindow.Contains(_go_popUpWindow))
                m_gos_popUpWindow.Remove(_go_popUpWindow);

            GameObject.Destroy(_go_popUpWindow);
        }
        #endregion
    }
}
