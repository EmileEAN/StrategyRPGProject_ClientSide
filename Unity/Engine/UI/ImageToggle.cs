using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [AddComponentMenu("UI/Image Toggle")]
    [RequireComponent(typeof(RectTransform))]
    public class ImageToggle : Selectable, IEventSystemHandler, IPointerClickHandler, ISubmitHandler, ICanvasElement
    {
        #if UNITY_EDITOR
        [MenuItem("GameObject/UI/Image Toggle", false, 2032)]
        private static void CreateImageToggle()
        {
            GameObject[] selectedGameObjects = UnityEditor.Selection.gameObjects;

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                Transform parent = (new GameObject("ImageToggle", new Type[] { typeof(ImageToggle) })).transform;
                parent.parent = selectedGameObject.transform;
                RectTransform parent_rt = parent.GetComponent<RectTransform>();
                parent_rt.localPosition = Vector3.zero;
                parent_rt.sizeDelta = Vector2.zero;
                parent_rt.anchorMin = Vector2.zero;
                parent_rt.anchorMax = Vector2.one;
                parent_rt.localScale = Vector3.one;

                GameObject onImage = new GameObject("Image@On", new Type[] { typeof(Image) });
                onImage.transform.parent = parent;
                Image onImage_image = onImage.GetComponent<Image>();
                RectTransform onImage_rt = onImage.GetComponent<RectTransform>();
                onImage_rt.localPosition = Vector3.zero;
                onImage_rt.sizeDelta = Vector2.zero;
                onImage_rt.anchorMin = Vector2.zero;
                onImage_rt.anchorMax = Vector2.one;
                onImage_rt.localScale = Vector3.one;

                GameObject offImage = new GameObject("Image@Off", new Type[] { typeof(Image) });
                offImage.transform.parent = parent;
                offImage.transform.localPosition = Vector3.zero;
                Image offImage_image = offImage.GetComponent<Image>();
                RectTransform offImage_rt = offImage.GetComponent<RectTransform>();
                offImage_rt.localPosition = Vector3.zero;
                offImage_rt.sizeDelta = Vector2.zero;
                offImage_rt.anchorMin = Vector2.zero;
                offImage_rt.anchorMax = Vector2.one;
                offImage_rt.localScale = Vector3.one;

                ImageToggle imageToggle = parent.GetComponent<ImageToggle>();
                imageToggle.OnGraphic = onImage_image;
                imageToggle.OffGraphic = offImage_image;
                imageToggle.m_isOn = true;
                imageToggle.targetGraphic = imageToggle.OnGraphic;
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Set(m_isOn, false);
            PlayEffect();

            var prefabAssetType = PrefabUtility.GetPrefabAssetType(this);
            if (prefabAssetType != PrefabAssetType.Regular && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }
        #endif

        public Graphic OnGraphic;
        public Graphic OffGraphic;

        // group that this toggle can belong to
        [SerializeField]
        private ImageToggleGroup m_group;

        public ImageToggleGroup Group
        {
            get { return m_group; }
            set
            {
                m_group = value;
                #if UNITY_EDITOR
                if (Application.isPlaying)
                #endif
                {
                    SetToggleGroup(m_group, true);
                    PlayEffect();
                }
            }
        }

        /// <summary>
        /// Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        /// </summary>
        public ToggleEvent onValueChanged = new ToggleEvent();

        // Whether the toggle is on
        [FormerlySerializedAs("m_IsActive")]
        [Tooltip("Is the toggle currently on or off?")]
        [SerializeField]
        private bool m_isOn;

        /// <summary>
        /// See ICanvasElement.LayoutComplete
        /// </summary>
        public virtual void LayoutComplete() { }

        /// <summary>
        /// See ICanvasElement.GraphicUpdateComplete
        /// </summary>
        public virtual void GraphicUpdateComplete() { }

        protected ImageToggle()
        { }

        /// <summary>
        /// Assume the correct visual state.
        /// </summary>
        protected override void Start()
        {
            PlayEffect();
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            #if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                onValueChanged.Invoke(m_isOn);
            #endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetToggleGroup(m_group, false);
            PlayEffect();
        }

        protected override void OnDisable()
        {
            SetToggleGroup(null, false);
            base.OnDisable();
        }

        private void SetToggleGroup(ImageToggleGroup _newGroup, bool _setMemberValue)
        {
            ImageToggleGroup oldGroup = m_group;

            // Sometimes IsActive returns false in OnDisable so don't check for it.
            // Rather remove the toggle too oftem than too little.
            if (m_group != null)
                m_group.UnregisterToggle(this);

            // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
            // That's why we use the setMemberValue parameter.
            if (_setMemberValue)
                m_group = _newGroup;

            // Only register to the new group if this Toggle is active.
            if (m_group != null && IsActive())
                m_group.RegisterToggle(this);

            // If we are in a new group, and this toggle is on, notify group.
            // Note: Don't refer to m_Group here as it's not guaranteed to have been set.
            if (_newGroup != null && _newGroup != oldGroup && IsOn && IsActive())
                m_group.NotifyToggleOn(this);
        }

        /// <summary>
        /// Whether the toggle is currently active.
        /// </summary>
        public bool IsOn
        {
            get { return m_isOn; }
            set
            {
                Set(value);
            }
        }

        void Set(bool _value)
        {
            Set(_value, true);
        }

        void Set(bool _value, bool _sendCallback)
        {
            if (m_isOn == _value)
                return;

            // if we are in a group and set to true, do group logic
            m_isOn = _value;
            if (m_group != null && IsActive())
            {
                if (m_isOn || (!m_group.AnyToggleOn() && !m_group.AllowSwitchOff))
                {
                    m_isOn = true;
                    m_group.NotifyToggleOn(this);
                }
            }

            // Always send event when toggle is clicked, even if value didn't change
            // due to already active toggle in a toggle group being clicked.
            // Controls like SelectionList rely on this.
            // It's up to the user to ignore a selection being set to the same value it already was, if desired.
            PlayEffect();
            if (_sendCallback)
                onValueChanged.Invoke(m_isOn);
        }

        /// <summary>
        /// Play the appropriate effect.
        /// </summary>
        private void PlayEffect()
        {
            if (OnGraphic == null || OffGraphic == null)
                return;

            if (m_isOn)
            {
                OnGraphic.enabled = true;
                OffGraphic.enabled = false;
                targetGraphic = OnGraphic;
            }
            else
            {
                OffGraphic.enabled = true;
                OnGraphic.enabled = false;
                targetGraphic = OffGraphic;
            }
        }

        private void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            IsOn = !IsOn;
        }

        /// <summary>
        /// React to clicks.
        /// </summary>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            InternalToggle();
        }
    }
}
