using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [AddComponentMenu("UI/Range Slider")]
    [RequireComponent(typeof(RectTransform))]
    public class RangeSlider : Selectable, IEventSystemHandler, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        #if UNITY_EDITOR
        [MenuItem("GameObject/UI/Range Slider", false, 2034)]
        private static void CreateRangeSlider()
        {
            GameObject[] selectedGameObjects = UnityEditor.Selection.gameObjects;

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                Transform parent = (new GameObject("RangeSlider", new Type[] { typeof(RangeSlider) })).transform;
                parent.parent = selectedGameObject.transform;
                RectTransform parent_rt = parent.GetComponent<RectTransform>();
                parent_rt.SetPosition(0, 0, 0, 0);
                parent_rt.sizeDelta = Vector2.zero;
                parent_rt.anchorMin = Vector2.zero;
                parent_rt.anchorMax = Vector2.one;
                parent_rt.localScale = Vector3.one;

                Vector2 anchorMin = new Vector2(0.15f, 0.35f);
                Vector2 anchorMax = Vector2.one - anchorMin;

                GameObject background = new GameObject("Background", new Type[] { typeof(Image) });
                background.transform.parent = parent;
                Image background_image = background.GetComponent<Image>();
                background_image.color = new Color(0.7f, 0.7f, 0.7f, 1);
                RectTransform background_rt = background.GetComponent<RectTransform>();
                background_rt.SetPosition(0, 0, 0, 0);
                background_rt.sizeDelta = Vector2.zero;
                background_rt.anchorMin = anchorMin;
                background_rt.anchorMax = anchorMax;
                background_rt.localScale = Vector3.one;

                GameObject fillArea = new GameObject("Fill Area", new Type[] { typeof(RectTransform) });
                fillArea.transform.parent = parent;
                RectTransform fillArea_rt = fillArea.GetComponent<RectTransform>();
                fillArea_rt.SetPosition(0, 0, 0, 0);
                fillArea_rt.sizeDelta = Vector2.zero;
                fillArea_rt.anchorMin = anchorMin;
                fillArea_rt.anchorMax = anchorMax;
                fillArea_rt.localScale = Vector3.one;

                GameObject fill = new GameObject("Fill", new Type[] { typeof(Image) });
                fill.transform.parent = fillArea.transform;
                Image fill_image = fill.GetComponent<Image>();
                fill_image.color = new Color32(140, 255, 140, 255);
                RectTransform fill_rt = fill.GetComponent<RectTransform>();
                fill_rt.SetPosition(0, 0, 0, 0);
                fill_rt.sizeDelta = Vector2.zero;
                fill_rt.anchorMin = Vector2.zero;
                fill_rt.anchorMax = Vector2.one;
                fill_rt.localScale = Vector3.one;

                GameObject lowerHandleSlideArea = new GameObject("Lower Handle Slide Area", new Type[] { typeof(RectTransform) });
                lowerHandleSlideArea.transform.parent = parent;
                RectTransform lowerHandleSlideArea_rt = lowerHandleSlideArea.GetComponent<RectTransform>();
                lowerHandleSlideArea_rt.localPosition = Vector3.zero;
                lowerHandleSlideArea_rt.sizeDelta = Vector2.zero;
                lowerHandleSlideArea_rt.anchorMin = anchorMin;
                lowerHandleSlideArea_rt.anchorMax = anchorMax;
                lowerHandleSlideArea_rt.localScale = Vector3.one;

                GameObject lowerHandle = new GameObject("Handle", new Type[] { typeof(Image) });
                lowerHandle.transform.parent = lowerHandleSlideArea.transform;
                Image lowerHandle_image = lowerHandle.GetComponent<Image>();
                lowerHandle_image.color = new Color32(100, 170, 255, 255);
                RectTransform lowerHandle_rt = lowerHandle.GetComponent<RectTransform>();
                lowerHandle_rt.SetPosition(0, 0, 0, 0);
                lowerHandle_rt.sizeDelta = Vector2.zero;
                lowerHandle_rt.localScale = Vector3.one;

                GameObject higherHandleSlideArea = new GameObject("Higher Handle Slide Area", new Type[] { typeof(RectTransform) });
                higherHandleSlideArea.transform.parent = parent;
                RectTransform higherHandleSlideArea_rt = higherHandleSlideArea.GetComponent<RectTransform>();
                higherHandleSlideArea_rt.SetPosition(0, 0, 0, 0);
                higherHandleSlideArea_rt.sizeDelta = Vector2.zero;
                higherHandleSlideArea_rt.anchorMin = anchorMin;
                higherHandleSlideArea_rt.anchorMax = anchorMax;
                higherHandleSlideArea_rt.localScale = Vector3.one;

                GameObject higherHandle = new GameObject("Handle", new Type[] { typeof(Image) });
                higherHandle.transform.parent = higherHandleSlideArea.transform;
                Image higherHandle_image = higherHandle.GetComponent<Image>();
                higherHandle_image.color = new Color32(255, 140, 140, 255);
                RectTransform higherHandle_rt = higherHandle.GetComponent<RectTransform>();
                higherHandle_rt.SetPosition(0, 0, 0, 0);
                higherHandle_rt.sizeDelta = Vector2.zero;
                higherHandle_rt.localScale = Vector3.one;

                GameObject lowerValue_label = new GameObject("Label@LowerValue", new Type[] { typeof(Text) });
                lowerValue_label.transform.parent = parent;
                Text lowerValue_label_text = lowerValue_label.GetComponent<Text>();
                lowerValue_label_text.alignment = TextAnchor.MiddleCenter;
                RectTransform lowerValue_label_rt = lowerValue_label.GetComponent<RectTransform>();
                lowerValue_label_rt.SetPosition(0, 0, 0, 0);
                lowerValue_label_rt.sizeDelta = Vector2.zero;
                lowerValue_label_rt.anchorMin = Vector2.zero;
                lowerValue_label_rt.anchorMax = new Vector2(fillArea_rt.anchorMin.x, 1);
                lowerValue_label_rt.localScale = Vector3.one;

                GameObject higherValue_label = new GameObject("Label@HigherValue", new Type[] { typeof(Text) });
                higherValue_label.transform.parent = parent;
                Text higherValue_label_text = higherValue_label.GetComponent<Text>();
                higherValue_label_text.alignment = TextAnchor.MiddleCenter;
                RectTransform higherValue_label_rt = higherValue_label.GetComponent<RectTransform>();
                higherValue_label_rt.SetPosition(0, 0, 0, 0);
                higherValue_label_rt.sizeDelta = Vector2.zero;
                higherValue_label_rt.anchorMin = new Vector2(fillArea_rt.anchorMax.x, 0);
                higherValue_label_rt.anchorMax = Vector2.one;
                higherValue_label_rt.localScale = Vector3.one;

                RangeSlider rangeSlider = parent.GetComponent<RangeSlider>();
                rangeSlider.BackgroundRect = background_rt;
                rangeSlider.FillRect = fill_rt;
                rangeSlider.LowerValueHandleRect = lowerHandle_rt;
                rangeSlider.HigherValueHandleRect = higherHandle_rt;
                rangeSlider.m_lowerValueLabelRect = lowerValue_label_rt;
                rangeSlider.m_higherValueLabelRect = higherValue_label_rt;

                rangeSlider.UpdateCachedReferences();
                rangeSlider.UpdateVisuals();
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (WholeNumbers)
            {
                m_minValue = Mathf.Round(m_minValue);
                m_maxValue = Mathf.Round(m_maxValue);
            }
            UpdateCachedReferences();
            InitialSet(m_lowerValue, m_higherValue);
            // Update rects since other things might affect them even if value didn't change.
            UpdateVisuals();

            var prefabAssetType = PrefabUtility.GetPrefabAssetType(this);
            if (prefabAssetType != PrefabAssetType.Regular && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }
        #endif

        public enum eDirection
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        public enum ePivot
        {
            In,
            Half_In_Half_Out,
            Out
        }

        [Serializable]
        public class SliderEvent : UnityEvent<float> { }

        [SerializeField]
        private RectTransform m_backgroundRect;
        public RectTransform BackgroundRect { get { return m_backgroundRect; } set { if (SetPropertyUtility.SetClass(ref m_backgroundRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }
        
        [SerializeField]
        private RectTransform m_fillRect;
        public RectTransform FillRect { get { return m_fillRect; } set { if (SetPropertyUtility.SetClass(ref m_fillRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

        [SerializeField]
        private RectTransform m_lowerValueHandleRect;
        public RectTransform LowerValueHandleRect { get { return m_lowerValueHandleRect; } set { if (SetPropertyUtility.SetClass(ref m_lowerValueHandleRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

        [SerializeField]
        private RectTransform m_higherValueHandleRect;
        public RectTransform HigherValueHandleRect { get { return m_higherValueHandleRect; } set { if (SetPropertyUtility.SetClass(ref m_higherValueHandleRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

        [SerializeField]
        private RectTransform m_lowerValueLabelRect;
        public RectTransform LowerValueLabelRect { get { return m_lowerValueLabelRect; } set { if (SetPropertyUtility.SetClass(ref m_lowerValueLabelRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

        [SerializeField]
        private RectTransform m_higherValueLabelRect;
        public RectTransform HigherValueLabelRect { get { return m_higherValueLabelRect; } set { if (SetPropertyUtility.SetClass(ref m_higherValueLabelRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

        [Space(6)]

        [SerializeField]
        private eDirection m_direction = eDirection.LeftToRight;
        public eDirection Direction
        {
            get { return m_direction; }
            set
            {
                eAxis oldAxis = Axis;
                if (SetPropertyUtility.SetStruct(ref m_direction, value))
                {
                    bool axisChanged;

                    if (oldAxis == eAxis.Horizontal && (value == eDirection.BottomToTop || value == eDirection.TopToBottom))
                    {
                        axisChanged = true;
                    }
                    else if (oldAxis == eAxis.Vertical && (value == eDirection.LeftToRight || value == eDirection.RightToLeft))
                    {
                        axisChanged = true;
                    }
                    else
                        axisChanged = false;

                    UpdateVisuals(true, axisChanged);
                }
            }
        }

        [SerializeField]
        private float m_minValue = 0;
        public float MinValue { get { return m_minValue; } set { if (SetPropertyUtility.SetStruct(ref m_minValue, value)) { Set(m_lowerValue, true); UpdateVisuals(); } } }

        [SerializeField]
        private float m_maxValue = 1;
        public float MaxValue { get { return m_maxValue; } set { if (SetPropertyUtility.SetStruct(ref m_maxValue, value)) { Set(m_higherValue, false); UpdateVisuals(); } } }

        [SerializeField]
        private bool m_wholeNumbers = false;
        public bool WholeNumbers { get { return m_wholeNumbers; } }

        [SerializeField]
        private float m_lowerValue = 0f;
        public float LowerValue
        {
            get
            {
                if (WholeNumbers)
                    return Mathf.Round(m_lowerValue);
                return m_lowerValue;
            }
            set
            {
                Set(value, true);
            }
        }

        [SerializeField]
        private float m_higherValue = 1f;
        public float HigherValue
        {
            get
            {
                if (WholeNumbers)
                    return Mathf.Round(m_higherValue);
                return m_higherValue;
            }
            set
            {
                Set(value, false);
            }
        }

        public float NormalizedLowerValue
        {
            get
            {
                if (Mathf.Approximately(MinValue, MaxValue))
                    return 0;

                float baseValue = Mathf.InverseLerp(MinValue, MaxValue, LowerValue);
                float handleLength = m_sizeOfHandleRelativeToBackground[(int)Axis];
                switch (m_handlePivot)
                {
                    default: // case ePivot.In
                        return (1 - handleLength) * baseValue;
                    case ePivot.Half_In_Half_Out:
                    case ePivot.Out:
                        return baseValue;
                }
            }
            set
            {
                this.LowerValue = Mathf.Lerp(MinValue, MaxValue, value);
            }
        }

        public float NormalizedHigherValue
        {
            get
            {
                if (Mathf.Approximately(MinValue, MaxValue))
                    return 0;

                float baseValue = Mathf.InverseLerp(MinValue, MaxValue, HigherValue);
                float handleLength = m_sizeOfHandleRelativeToBackground[(int)Axis];
                switch (m_handlePivot)
                {
                    default: // case ePivot.In
                        return ((1 - handleLength) * baseValue) + handleLength;
                    case ePivot.Half_In_Half_Out:
                    case ePivot.Out:
                        return baseValue;
                }
            }
            set
            {
                this.HigherValue = Mathf.Lerp(MinValue, MaxValue, value);
            }
        }

        [SerializeField]
        private Vector2 m_sizeOfHandleRelativeToBackground = new Vector2(0.08f, 1);
        public Vector2 SizeOfHandleRelativeToBackground { get { return m_sizeOfHandleRelativeToBackground; } set { if (SetPropertyUtility.SetStruct(ref m_sizeOfHandleRelativeToBackground, value)) { UpdateVisuals(); } } }

        [SerializeField]
        private ePivot m_handlePivot = ePivot.In;
        public ePivot HandlePivot { get { return m_handlePivot; } set { if (SetPropertyUtility.SetStruct(ref m_handlePivot, value)) { UpdateVisuals(); } } }

        // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        [SerializeField]
        private SliderEvent m_onValueChanged = new SliderEvent();
        public SliderEvent OnValueChanged { get { return m_onValueChanged; } set { m_onValueChanged = value; } }

        // Private fields

        private Image m_fillImage;
        private Transform m_fillTransform;
        private RectTransform m_fillContainerRect;
        private Text m_label_lowerValue;
        private Text m_label_higherValue;
        private Transform m_handleTransform_lowerValue;
        private Transform m_handleTransform_higherValue;
        private RectTransform m_handleContainerRect_lowerValue;
        private RectTransform m_handleContainerRect_higherValue;

        private bool m_isLowerValueHandleSelected = false;
        private bool m_isHigherValueHandleSelected = false;

        // The offset from handle position to mouse down position
        private Vector2 m_offset = Vector2.zero;

        private DrivenRectTransformTracker m_tracker;

        // Size of each step.
        float StepSize { get { return WholeNumbers ? 1 : (MaxValue - MinValue) * 0.1f; } }

        /// <summary>
        /// See ICanvasElement.LayoutComplete
        /// </summary>
        public virtual void LayoutComplete() { }

        /// <summary>
        /// See ICanvasElement.GraphicUpdateComplete
        /// </summary>
        public virtual void GraphicUpdateComplete() { }

        protected RangeSlider()
        { }

        public virtual void Rebuild(CanvasUpdate _executing)
        {
#if UNITY_EDITOR
            if (_executing == CanvasUpdate.Prelayout)
                OnValueChanged.Invoke(m_isLowerValueHandleSelected ? LowerValue : HigherValue);
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCachedReferences();
            InitialSet(m_lowerValue, m_higherValue);
            // Update rects since they need to be initialized correctly.
            UpdateVisuals();
        }

        protected override void OnDisable()
        {
            m_tracker.Clear();
            base.OnDisable();
        }

        void UpdateCachedReferences()
        {
            if (m_fillRect)
            {
                m_fillTransform = m_fillRect.transform;
                m_fillImage = m_fillRect.GetComponent<Image>();
                if (m_fillTransform.parent != null)
                    m_fillContainerRect = m_fillTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                m_fillContainerRect = null;
                m_fillImage = null;
            }

            if (m_lowerValueHandleRect)
            {
                m_handleTransform_lowerValue = m_lowerValueHandleRect.transform;
                if (m_handleTransform_lowerValue.parent != null)
                    m_handleContainerRect_lowerValue = m_handleTransform_lowerValue.parent.GetComponent<RectTransform>();
            }
            else
            {
                m_handleContainerRect_lowerValue = null;
            }

            if (m_higherValueHandleRect)
            {
                m_handleTransform_higherValue = m_higherValueHandleRect.transform;
                if (m_handleTransform_higherValue.parent != null)
                    m_handleContainerRect_higherValue = m_handleTransform_higherValue.parent.GetComponent<RectTransform>();
            }
            else
            {
                m_handleContainerRect_higherValue = null;
            }

            m_label_lowerValue = m_lowerValueLabelRect?.GetComponent<Text>();
            m_label_higherValue = m_higherValueLabelRect?.GetComponent<Text>();
        }

        // Set the valueUpdate the visible Image. Called only for initialization.
        void InitialSet(float _lowerValue, float _higherValue)
        {
            // Clamp the input
            float newLowerValue = Mathf.Clamp(_lowerValue, MinValue, MaxValue);
            float newHigherValue = Mathf.Clamp(_higherValue, MinValue, MaxValue);
            if (WholeNumbers)
            {
                newLowerValue = Mathf.Round(newLowerValue);
                newHigherValue = Mathf.Round(newHigherValue);
            }

            // If the stepped value doesn't match the last one, it's time to update
            if (m_lowerValue != newLowerValue)
                m_lowerValue = newLowerValue;

            if (m_higherValue != newHigherValue)
                m_higherValue = newHigherValue;

            UpdateVisuals();
        }

        // Set the valueUpdate the visible Image.
        void Set(float _input, bool _isInputLowerValue)
        {
            Set(_input, _isInputLowerValue, true);
        }
        void Set(float _input, bool _isInputLowerValue, bool _sendCallback)
        {
            // Clamp the input
            float newValue = Mathf.Clamp(_input, MinValue, MaxValue);
            if (WholeNumbers)
                newValue = Mathf.Round(newValue);

            // If the stepped value doesn't match the last one, it's time to update
            if (_isInputLowerValue)
            {
                if (m_lowerValue == newValue)
                    return;

                // The lower value must not be greater than the higher value
                if (newValue > m_higherValue)
                    m_lowerValue = m_higherValue;
                else
                    m_lowerValue = newValue;
            }
            else
            {
                if (m_higherValue == newValue)
                    return;

                // The higher value must not be less than the lower value
                if (newValue < m_lowerValue)
                    m_higherValue = m_lowerValue;
                else
                    m_higherValue = newValue;
            }

            UpdateVisuals();
            if (_sendCallback)
                m_onValueChanged.Invoke(newValue);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            UpdateVisuals();
        }

        enum eAxis
        {
            Horizontal = 0,
            Vertical = 1
        }

        eAxis Axis { get { return (m_direction == eDirection.LeftToRight || m_direction == eDirection.RightToLeft) ? eAxis.Horizontal : eAxis.Vertical; } }
        bool ReverseValue { get { return m_direction == eDirection.RightToLeft || m_direction == eDirection.TopToBottom; } }

        // Force-update the slider. Useful if you've changed the properties and want it to update visually.
        private void UpdateVisuals(bool _directionChanged = false, bool _axisChanged = false)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif

            m_tracker.Clear();

            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;

            if (_directionChanged) // Update the objects affected by the change of direction
            {
                if (_axisChanged) // Update the objects affected by the change of axis
                {
                    if (!m_backgroundRect)
                        return;

                    SizeOfHandleRelativeToBackground = m_sizeOfHandleRelativeToBackground.SwapValues();

                    Debug.Log("Changed value to: " + m_sizeOfHandleRelativeToBackground.x.ToString() + "," + m_sizeOfHandleRelativeToBackground.y.ToString());

                    anchorMin = m_backgroundRect.anchorMin.SwapValues();
                    anchorMax = m_backgroundRect.anchorMax.SwapValues();

                    if (m_backgroundRect)
                    {
                        m_backgroundRect.anchorMin = anchorMin;
                        m_backgroundRect.anchorMax = anchorMax;
                    }

                    if (m_fillContainerRect)
                    {
                        m_fillContainerRect.anchorMin = anchorMin;
                        m_fillContainerRect.anchorMax = anchorMax;
                    }

                    if (m_handleContainerRect_lowerValue)
                    {
                        m_handleContainerRect_lowerValue.anchorMin = anchorMin;
                        m_handleContainerRect_lowerValue.anchorMax = anchorMax;
                    }

                    if (m_handleContainerRect_higherValue)
                    {
                        m_handleContainerRect_higherValue.anchorMin = anchorMin;
                        m_handleContainerRect_higherValue.anchorMax = anchorMax;
                    }
                }

                switch (Direction)
                {
                    default: // case eDirection.LeftToRight:
                        {
                            if (m_lowerValueLabelRect)
                            {
                                m_lowerValueLabelRect.anchorMin = Vector2.zero;
                                m_lowerValueLabelRect.anchorMax = new Vector2(m_fillContainerRect.anchorMin.x, 1);
                            }

                            if (m_higherValueLabelRect)
                            {
                                m_higherValueLabelRect.anchorMin = new Vector2(m_fillContainerRect.anchorMax.x, 0);
                                m_higherValueLabelRect.anchorMax = Vector2.one;
                            }
                        }
                        break;
                    case eDirection.RightToLeft:
                        {
                            if (m_lowerValueLabelRect)
                            {
                                m_lowerValueLabelRect.anchorMin = new Vector2(m_fillContainerRect.anchorMax.x, 0);
                                m_lowerValueLabelRect.anchorMax = Vector2.one;
                            }

                            if (m_higherValueLabelRect)
                            {
                                m_higherValueLabelRect.anchorMin = Vector2.zero;
                                m_higherValueLabelRect.anchorMax = new Vector2(m_fillContainerRect.anchorMin.x, 1);
                            }
                        }
                        break;
                    case eDirection.BottomToTop:
                        {
                            if (m_lowerValueLabelRect)
                            {
                                m_lowerValueLabelRect.anchorMin = Vector2.zero;
                                m_lowerValueLabelRect.anchorMax = new Vector2(1, m_fillContainerRect.anchorMin.y);
                            }

                            if (m_higherValueLabelRect)
                            {
                                m_higherValueLabelRect.anchorMin = new Vector2(0, m_fillContainerRect.anchorMax.y);
                                m_higherValueLabelRect.anchorMax = Vector2.one;
                            }
                        }
                        break;
                    case eDirection.TopToBottom:
                        {
                            if (m_lowerValueLabelRect)
                            {
                                m_lowerValueLabelRect.anchorMin = new Vector2(0, m_fillContainerRect.anchorMax.y);
                                m_lowerValueLabelRect.anchorMax = Vector2.one;
                            }

                            if (m_higherValueLabelRect)
                            {
                                m_higherValueLabelRect.anchorMin = Vector2.zero;
                                m_higherValueLabelRect.anchorMax = new Vector2(1, m_fillContainerRect.anchorMin.y);
                            }
                        }
                        break;
                }
            }

            if (m_fillContainerRect) // Update the fill container
            {
                m_tracker.Add(this, m_fillRect, DrivenTransformProperties.Anchors);

                if (m_fillImage != null && m_fillImage.type == Image.Type.Filled)
                {
                    m_fillImage.fillAmount = NormalizedHigherValue - NormalizedLowerValue;
                }
                else
                {
                    if (ReverseValue)
                    {
                        anchorMin[(int)Axis] = 1 - NormalizedHigherValue;
                        anchorMax[(int)Axis] = 1 - NormalizedLowerValue;
                    }
                    else
                    {
                        anchorMin[(int)Axis] = NormalizedLowerValue;
                        anchorMax[(int)Axis] = NormalizedHigherValue;
                    }
                }

                m_fillRect.anchorMin = anchorMin;
                m_fillRect.anchorMax = anchorMax;
            }

            if (m_handleContainerRect_lowerValue || m_handleContainerRect_higherValue) // Set the anchor values for the axis contrary to the one currently set
            {
                int contraryAxis = (Axis == eAxis.Horizontal) ? (int)eAxis.Vertical : (int)eAxis.Horizontal;
                float handleLength_contraryAxis = m_sizeOfHandleRelativeToBackground[contraryAxis];
                float lengthDifferenceToOne = 1 - handleLength_contraryAxis;
                float halfLenghtDifference = lengthDifferenceToOne / 2;
                anchorMin[contraryAxis] = halfLenghtDifference;
                anchorMax[contraryAxis] = 1 - halfLenghtDifference;
            }

            if (m_handleContainerRect_lowerValue) // Update the handle rect container for lower value
            {
                m_tracker.Add(this, m_lowerValueHandleRect, DrivenTransformProperties.Anchors);

                float normalizedValue = (ReverseValue ? (1 - NormalizedLowerValue) : NormalizedLowerValue);
                float handleLength = m_sizeOfHandleRelativeToBackground[(int)Axis];

                switch (m_handlePivot)
                {
                    default: // case ePivot.In
                        {
                            if (!ReverseValue)
                            {
                                anchorMin[(int)Axis] = normalizedValue;
                                anchorMax[(int)Axis] = normalizedValue + handleLength;
                            }
                            else
                            {
                                anchorMin[(int)Axis] = normalizedValue - handleLength;
                                anchorMax[(int)Axis] = normalizedValue;
                            }
                        }
                        break;
                    case ePivot.Half_In_Half_Out:
                        {
                            float halfLength = handleLength / 2;
                            anchorMin[(int)Axis] = normalizedValue - halfLength;
                            anchorMax[(int)Axis] = normalizedValue + halfLength;
                        }
                        break;
                    case ePivot.Out:
                        {
                            if (!ReverseValue)
                            {
                                anchorMin[(int)Axis] = normalizedValue - handleLength;
                                anchorMax[(int)Axis] = normalizedValue;
                            }
                            else
                            {
                                anchorMin[(int)Axis] = normalizedValue;
                                anchorMax[(int)Axis] = normalizedValue + handleLength;
                            }
                        }
                        break;
                }

                m_lowerValueHandleRect.anchorMin = anchorMin;
                m_lowerValueHandleRect.anchorMax = anchorMax;
            }

            if (m_handleContainerRect_higherValue) // Update the handle rect container for higher value
            {
                m_tracker.Add(this, m_higherValueHandleRect, DrivenTransformProperties.Anchors);

                float normalizedValue = (ReverseValue ? (1 - NormalizedHigherValue) : NormalizedHigherValue);
                float handleLength = m_sizeOfHandleRelativeToBackground[(int)Axis];

                switch (m_handlePivot)
                {
                    default: // case ePivot.In
                        {
                            if (!ReverseValue)
                            {
                                anchorMin[(int)Axis] = normalizedValue - handleLength;
                                anchorMax[(int)Axis] = normalizedValue;
                            }
                            else
                            {
                                anchorMin[(int)Axis] = normalizedValue;
                                anchorMax[(int)Axis] = normalizedValue + handleLength;
                            }
                        }
                        break;
                    case ePivot.Half_In_Half_Out:
                        {
                            float halfLength = handleLength / 2;
                            anchorMin[(int)Axis] = normalizedValue - halfLength;
                            anchorMax[(int)Axis] = normalizedValue + halfLength;
                        }
                        break;
                    case ePivot.Out:
                        {
                            if (!ReverseValue)
                            {
                                anchorMin[(int)Axis] = normalizedValue;
                                anchorMax[(int)Axis] = normalizedValue + handleLength;
                            }
                            else
                            {
                                anchorMin[(int)Axis] = normalizedValue - handleLength;
                                anchorMax[(int)Axis] = normalizedValue;
                            }
                        }
                        break;
                }

                m_higherValueHandleRect.anchorMin = anchorMin;
                m_higherValueHandleRect.anchorMax = anchorMax;
            }

            // Update the labels for the selected value range
            if (m_label_lowerValue != null)
                m_label_lowerValue.text = m_lowerValue.ToString();

            if (m_label_higherValue != null)
                m_label_higherValue.text = m_higherValue.ToString();
        }

        // Update the slider's position based on the mouse.
        void UpdateDrag(PointerEventData _eventData, Camera _cam)
        {
            RectTransform clickRect;
            Vector2 localCursor;

            if (m_isLowerValueHandleSelected)
            {
                clickRect = m_handleContainerRect_lowerValue ?? m_fillContainerRect;
                if (clickRect != null && clickRect.rect.size[(int)Axis] > 0)
                {
                    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, _eventData.position, _cam, out localCursor))
                        return;
                    localCursor -= clickRect.rect.position;

                    float val = Mathf.Clamp01((localCursor - m_offset)[(int)Axis] / clickRect.rect.size[(int)Axis]);
                    NormalizedLowerValue = (ReverseValue ? 1f - val : val);
                }
            }
            else if (m_isHigherValueHandleSelected)
            {
                clickRect = m_handleContainerRect_higherValue ?? m_fillContainerRect;
                if (clickRect != null && clickRect.rect.size[(int)Axis] > 0)
                {
                    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, _eventData.position, _cam, out localCursor))
                        return;
                    localCursor -= clickRect.rect.position;

                    float val = Mathf.Clamp01((localCursor - m_offset)[(int)Axis] / clickRect.rect.size[(int)Axis]);
                    NormalizedHigherValue = (ReverseValue ? 1f - val : val);
                }
            }
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public override void OnPointerDown(PointerEventData _eventData)
        {
            if (!MayDrag(_eventData))
                return;

            base.OnPointerDown(_eventData);

            m_offset = Vector2.zero;
            Vector2 localMousePos;

            if (m_handleContainerRect_lowerValue != null && RectTransformUtility.RectangleContainsScreenPoint(m_lowerValueHandleRect, _eventData.position, _eventData.enterEventCamera))
            {
                // Allow only one handle to be selected
                m_isLowerValueHandleSelected = true;
                m_isHigherValueHandleSelected = false;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_lowerValueHandleRect, _eventData.position, _eventData.pressEventCamera, out localMousePos))
                    m_offset = localMousePos;
            }
            else if (m_handleContainerRect_higherValue != null && RectTransformUtility.RectangleContainsScreenPoint(m_higherValueHandleRect, _eventData.position, _eventData.enterEventCamera))
            {
                // Allow only one handle to be selected
                m_isHigherValueHandleSelected = true;
                m_isLowerValueHandleSelected = false;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_higherValueHandleRect, _eventData.position, _eventData.pressEventCamera, out localMousePos))
                    m_offset = localMousePos;
            }
            else
                m_isLowerValueHandleSelected = m_isHigherValueHandleSelected = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public override void OnMove(AxisEventData _eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnMove(_eventData);
                return;
            }

            switch (_eventData.moveDir)
            {
                case MoveDirection.Left:
                    if (Axis == eAxis.Horizontal && FindSelectableOnLeft() == null)
                    {
                        Set(ReverseValue ? LowerValue + StepSize : LowerValue - StepSize, true);
                        Set(ReverseValue ? HigherValue + StepSize : HigherValue - StepSize, false);
                    }
                    else
                        base.OnMove(_eventData);
                    break;
                case MoveDirection.Right:
                    if (Axis == eAxis.Horizontal && FindSelectableOnRight() == null)
                    {
                        Set(ReverseValue ? LowerValue - StepSize : LowerValue + StepSize, true);
                        Set(ReverseValue ? HigherValue - StepSize : HigherValue + StepSize, false);
                    }
                    else
                        base.OnMove(_eventData);
                    break;
                case MoveDirection.Up:
                    if (Axis == eAxis.Vertical && FindSelectableOnUp() == null)
                    {
                        Set(ReverseValue ? LowerValue - StepSize : LowerValue + StepSize, true);
                        Set(ReverseValue ? HigherValue - StepSize : HigherValue + StepSize, false);
                    }
                    else
                        base.OnMove(_eventData);
                    break;
                case MoveDirection.Down:
                    if (Axis == eAxis.Vertical && FindSelectableOnDown() == null)
                    {
                        Set(ReverseValue ? LowerValue + StepSize : LowerValue - StepSize, true);
                        Set(ReverseValue ? HigherValue + StepSize : HigherValue - StepSize, false);
                    }
                    else
                        base.OnMove(_eventData);
                    break;
            }
        }

        public override Selectable FindSelectableOnLeft()
        {
            if (navigation.mode == Navigation.Mode.Automatic && Axis == eAxis.Horizontal)
                return null;
            return base.FindSelectableOnLeft();
        }

        public override Selectable FindSelectableOnRight()
        {
            if (navigation.mode == Navigation.Mode.Automatic && Axis == eAxis.Horizontal)
                return null;
            return base.FindSelectableOnRight();
        }

        public override Selectable FindSelectableOnUp()
        {
            if (navigation.mode == Navigation.Mode.Automatic && Axis == eAxis.Vertical)
                return null;
            return base.FindSelectableOnUp();
        }

        public override Selectable FindSelectableOnDown()
        {
            if (navigation.mode == Navigation.Mode.Automatic && Axis == eAxis.Vertical)
                return null;
            return base.FindSelectableOnDown();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData _eventData)
        {
            _eventData.useDragThreshold = false;
        }

        public void SetDirection(eDirection _direction)
        {
            eAxis oldAxis = Axis;
            bool oldReverse = ReverseValue;
            this.Direction = _direction;

            //if (!_includeRectLayouts)
            //    return;

            //if (Axis != oldAxis)
            //    RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);

            //if (ReverseValue != oldReverse)
            //    RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, (int)Axis, true, true);
        }
    }
}
