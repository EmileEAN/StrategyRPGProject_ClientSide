using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Engine.UI
{
    // Button that's meant to work with mouse or touch-based devices.
    [AddComponentMenu("UI/Advanced Button")]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(AdvancedButtonClickTypeDetector))]
    [RequireComponent(typeof(Image))]
    public class AdvancedButton : Selectable, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ISubmitHandler
    {
        #if UNITY_EDITOR
        [MenuItem("GameObject/UI/Advanced Button", false, 2031)]
        private static void CreateRangeSlider()
        {
            GameObject[] selectedGameObjects = UnityEditor.Selection.gameObjects;

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                Transform parent = (new GameObject("AdvancedButton", new Type[] { typeof(AdvancedButton) })).transform;
                parent.parent = selectedGameObject.transform;
                RectTransform parent_rt = parent.GetComponent<RectTransform>();
                parent_rt.SetPosition(0, 0, 0, 0);
                parent_rt.sizeDelta = Vector2.zero;
                parent_rt.anchorMin = Vector2.zero;
                parent_rt.anchorMax = Vector2.one;
                parent_rt.localScale = Vector3.one;

                AdvancedButton advancedButton = parent.GetComponent<AdvancedButton>();
                advancedButton.m_maxSecondsForDoubleClick = 0.5f;
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!m_enableDoubleClick || !m_enableLongPress) // If either event is disabled
                return; // Do nothing

            if (m_maxSecondsForDoubleClick != m_previousValidatedMaxSecondsForDoubleClick)
            {
                if (m_maxSecondsForDoubleClick < 0f)
                    m_maxSecondsForDoubleClick = 0f;
                if (m_maxSecondsForDoubleClick > m_secondsForLongPress)
                    m_maxSecondsForDoubleClick = m_secondsForLongPress;

                m_previousValidatedMaxSecondsForDoubleClick = m_maxSecondsForDoubleClick;
            }

            if (m_secondsForLongPress != m_previousValidatedSecondsForLongPress)
            {
                if (m_secondsForLongPress < 0f)
                    m_secondsForLongPress = 0f;
                if (m_secondsForLongPress < m_maxSecondsForDoubleClick)
                    m_secondsForLongPress = m_maxSecondsForDoubleClick;

                m_previousValidatedSecondsForLongPress = m_secondsForLongPress;
            }
        }
        #endif

        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }

        // Event delegates triggered on click.
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private ButtonClickedEvent m_onClick = new ButtonClickedEvent();

        [SerializeField]
        private bool m_enableDoubleClick = false;
        public bool EnableDoubleClick { get { return m_enableDoubleClick; } set { m_enableDoubleClick = value; } }

        private float m_previousValidatedMaxSecondsForDoubleClick = TBSG._01.CoreValues.BUTTON_MAX_SECONDS_FOR_DOUBLE_CLICK;
        [SerializeField]
        private float m_maxSecondsForDoubleClick = TBSG._01.CoreValues.BUTTON_MAX_SECONDS_FOR_DOUBLE_CLICK;
        public float MaxSecondsForDoubleClick { get { return m_maxSecondsForDoubleClick; } set { if (SetPropertyUtility.SetStruct(ref m_maxSecondsForDoubleClick, value)) { m_maxSecondsForDoubleClick = Mathf.Clamp(m_maxSecondsForDoubleClick, 0f, float.MaxValue); } } }

        // Event delegates triggered on double click.
        [FormerlySerializedAs("onDoubleClick")]
        [SerializeField]
        private ButtonClickedEvent m_onDoubleClick = new ButtonClickedEvent();

        [SerializeField]
        private bool m_enableLongPress = false;
        public bool EnableLongPress { get { return m_enableLongPress; } set { m_enableLongPress = value; } }

        private float m_previousValidatedSecondsForLongPress = TBSG._01.CoreValues.BUTTON_SECONDS_FOR_LONG_PRESS;
        [SerializeField]
        private float m_secondsForLongPress = TBSG._01.CoreValues.BUTTON_SECONDS_FOR_LONG_PRESS;
        public float SecondsForLongPress { get { return m_secondsForLongPress; } set { if (SetPropertyUtility.SetStruct(ref m_secondsForLongPress, value)) { m_secondsForLongPress = Mathf.Clamp(m_secondsForLongPress, 0f, float.MaxValue); } } }

        // Event delegates triggered on long press.
        [FormerlySerializedAs("onLongPress")]
        [SerializeField]
        private ButtonClickedEvent m_onLongPress = new ButtonClickedEvent();

        protected AdvancedButton()
        { }

        public ButtonClickedEvent OnClick
        {
            get { return m_onClick; }
            set { m_onClick = value; }
        }

        public ButtonClickedEvent OnDoubleClick
        {
            get { return m_onDoubleClick; }
            set { m_onDoubleClick = value; }
        }

        public ButtonClickedEvent OnLongPress
        {
            get { return m_onLongPress; }
            set { m_onLongPress = value; }
        }
        public float TimePressStarted { get; private set; } = -1f;
        public float TimePressEnded { get; private set; } = -1f;
        public override void OnPointerDown(PointerEventData _eventData)
        {
            base.OnPointerDown(_eventData);

            if (!IsActive() || !IsInteractable()) // If the component is not active or it is not interactable
                return; // Do nothing

            if (_eventData.button != PointerEventData.InputButton.Left) // If it is not left click
                return; // Do nothing

            TimePressStarted = Time.realtimeSinceStartup;
        }

        public override void OnPointerUp(PointerEventData _eventData)
        {
            base.OnPointerUp(_eventData);

            if (!IsActive() || !IsInteractable()) // If the component is not active or it is not interactable
                return; // Do nothing

            if (_eventData.button != PointerEventData.InputButton.Left) // If it is not left click
                return; // Do nothing

            if (TimePressStarted == -1f) // If pressing has been canceled
                return; //Do nothing

            if (TimePressEnded == -1f) // If this is the first click
                TimePressEnded = Time.realtimeSinceStartup;
            else if (m_enableDoubleClick) // If double click is enabled
            {
                if (Time.realtimeSinceStartup - TimePressEnded <= MaxSecondsForDoubleClick) // If it is not the first click and not too much time has passed since last click
                Execute(eClickType.Double); // Invoke double click event
            }
        }

        public override void OnPointerExit(PointerEventData _eventData)
        {
            base.OnPointerExit(_eventData);

            if (!IsActive() || !IsInteractable()) // If the component is not active or it is not interactable
                return; // Do nothing

            if (_eventData.button != PointerEventData.InputButton.Left) // If it is not left click
                return; // Do nothing

            if (TimePressStarted != -1f && TimePressEnded == -1f) // If button is pressed but has not been clicked
                CancelPressing();
        }

        public virtual void OnSubmit(BaseEventData _eventData)
        {
            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        private void CancelPressing()
        {
            TimePressStarted = -1f;
            TimePressEnded = -1f;
        }

        public bool IsExecuting { get; private set; } = false;
        public void Execute(eClickType _clickType)
        {
            //Only allow one event to be invoked at a time
            if (IsExecuting)
                return;

            IsExecuting = true;

            switch (_clickType)
            {
                default: // case eClickType.Single
                    m_onClick.Invoke();
                    break;
                case eClickType.Double:
                    m_onDoubleClick.Invoke();
                    break;
                case eClickType.LongPress:
                    m_onLongPress.Invoke();
                    break;
            }

            CancelPressing(); // Already processed pressing

            IsExecuting = false;
        }

        public void DisableDoubleClick()
        {
            EnableDoubleClick = false;
        }
        public void DisableLongPress()
        {
            EnableLongPress = false;
        }
        public void EnableDoubleClickAsDefault()
        {
            EnableDoubleClick = true;
            MaxSecondsForDoubleClick = TBSG._01.CoreValues.BUTTON_MAX_SECONDS_FOR_DOUBLE_CLICK;
        }
        public void EnableLongPressAsDefault()
        {
            EnableLongPress = true;
            SecondsForLongPress = TBSG._01.CoreValues.BUTTON_SECONDS_FOR_LONG_PRESS;
        }
        public void SetSecondsForDoubleClickAndLongPress(float _maxSecondsForDoubleClick, float _secondsForLongPress)
        {
            MaxSecondsForDoubleClick = _maxSecondsForDoubleClick;
            SecondsForLongPress = _secondsForLongPress;
        }
    }
}
