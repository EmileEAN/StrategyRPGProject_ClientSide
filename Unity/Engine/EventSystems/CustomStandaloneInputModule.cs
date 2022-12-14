using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Engine.EventSystems
{
    [AddComponentMenu("Event/Custom Standalone Input Module")]
    [RequireComponent(typeof(ScreenResizingDetector))]
    [RequireComponent(typeof(EventSystem))]
    public class CustomStandaloneInputModule : StandaloneInputModule
    {
        #if UNITY_EDITOR
        [MenuItem("GameObject/UI/Custom Event System Set", false, 2101)]
        private static void CreateSetOfCustomEventSystemComponents() { new GameObject("EventSystem", typeof(CustomStandaloneInputModule)); }
        #endif

        private readonly MouseState m_mouseState = new MouseState();

        private List<ButtonInfo> m_buttonInfos;

        public GameObject CurrentRaycastObject { get; private set; }
        public PointerEventData.FramePressState CurrentPointerState_Left { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            m_buttonInfos = new List<ButtonInfo>();

            UpdateReferenceTextures();
        }

        //public override void Process()
        //{
        //    bool usedEvent = SendUpdateEventToSelectedObject();

        //    if (eventSystem.sendNavigationEvents)
        //    {
        //        if (!usedEvent)
        //            usedEvent |= SendMoveEventToSelectedObject();

        //        if (!usedEvent)
        //            SendSubmitEventToSelectedObject();
        //    }

        //    ProcessMouseEvent();
        //}

        //private new void ProcessMouseEvent()
        //{
        //    var mouseData = GetMousePointerEventData();

        //    var pressed = mouseData.AnyPressesThisFrame();
        //    var released = mouseData.AnyReleasesThisFrame();

        //    var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        //    if (!UseMouse(pressed, released, leftButtonData.buttonData))
        //        return;

        //    // Process the first mouse button fully
        //    ProcessMousePress(leftButtonData);
        //    ProcessMove(leftButtonData.buttonData);
        //    ProcessDrag(leftButtonData.buttonData);

        //    // Now process right / middle clicks
        //    ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
        //    ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
        //    ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
        //    ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

        //    if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
        //    {
        //        var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
        //        ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
        //    }
        //}

        //private static bool UseMouse(bool _pressed, bool _released, PointerEventData _pointerData)
        //{
        //    if (_pressed || _released || _pointerData.IsPointerMoving() || _pointerData.IsScrolling())
        //        return true;

        //    return false;
        //}

        protected override MouseState GetMousePointerEventData(int _id)
        {
            // Populate the left button...
            var created = GetPointerData(kMouseLeftId, out PointerEventData leftData, true);

            leftData.Reset();

            if (created)
                leftData.position = input.mousePosition;

            Vector2 pos = input.mousePosition;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // We don't want to do ANY cursor-based interaction when the mouse is locked
                leftData.position = new Vector2(-1.0f, -1.0f);
                leftData.delta = Vector2.zero;
            }
            else
            {
                leftData.delta = pos - leftData.position;
                leftData.position = pos;
            }
            leftData.scrollDelta = input.mouseScrollDelta;
            leftData.button = PointerEventData.InputButton.Left;
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            var raycast = FindFirstAcceptableRaycast(m_RaycastResultCache); // Get the first RaycastResult that meets specific requirements.
            leftData.pointerCurrentRaycast = raycast;
            CurrentRaycastObject = leftData.pointerCurrentRaycast.gameObject ?? null; // Set current raycast game object for external reference
            m_RaycastResultCache.Clear();

            // copy the apropriate data into right and middle slots
            GetPointerData(kMouseRightId, out PointerEventData rightData, true);
            CopyFromTo(leftData, rightData);
            rightData.button = PointerEventData.InputButton.Right;

            GetPointerData(kMouseMiddleId, out PointerEventData middleData, true);
            CopyFromTo(leftData, middleData);
            middleData.button = PointerEventData.InputButton.Middle;

            m_mouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
            m_mouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
            m_mouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

            CurrentPointerState_Left = m_mouseState.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonState;

            return m_mouseState;
        }

        private RaycastResult FindFirstAcceptableRaycast(List<RaycastResult> _candidates)
        {
            List<RaycastResult> candidatesWithGameObject = _candidates.Where(x => x.gameObject != null).ToList();
            if (candidatesWithGameObject.Count > 0)
            {
                bool parentHasButton = false;
                RaycastResult targetRaycast = candidatesWithGameObject[0]; // Get the first RaycastResult
                {
                    Button button = targetRaycast.gameObject.GetComponent<Button>();
                    if (button == null) // If the gameObject does not contain a Button component
                    {
                        button = targetRaycast.gameObject.GetComponentInParent<Button>(); // Check whether its parent (or parent's parent, and so on...) contains a Button component
                        if (button == null) // If none of the parents contain a Button component
                            return targetRaycast;
                        else if (!candidatesWithGameObject.Any(x => x.gameObject == button.gameObject)) // If the parent containing a Button component is not within the raycast results
                            return targetRaycast;

                        parentHasButton = true;
                    }

                    if (parentHasButton) // If true, the parent containing a Button component is within the raycast results
                        targetRaycast = candidatesWithGameObject.First(x => x.gameObject == button.gameObject); // Hence, get the raycast result

                    // Register ButtonInfo if not registered yet.
                    if (!m_buttonInfos.Any(x => x.Button == button))
                    {
                        m_buttonInfos.Add(new ButtonInfo(button));
                        m_buttonInfos.Last().UpdateButtonInfo();
                    }

                    // Return RaycastResult if the pixel of the Button Image pointed by the pointer is not transparent
                    if (targetRaycast.gameObject.tag == "AlphaZeroIgnoredButton")
                    {
                        if (!IsPixelAlphaZeroAtPosition(input.mousePosition, m_buttonInfos.Find(x => x.Button == button)))
                            return targetRaycast;
                    }
                    else
                        return targetRaycast;
                }
            }

            // No RaycastResult had a gameObject. Hence return a new RaycastResult.
            return new RaycastResult();
        }

        public void UpdateReferenceTextures()
        {
            foreach (ButtonInfo imageInfo in m_buttonInfos)
            {
                imageInfo.UpdateButtonInfo();
            }
        }

        private bool IsPixelAlphaZeroAtPosition(Vector2 _pointerPosition, ButtonInfo _buttonInfo)
        {
            if (_buttonInfo.TextureCopy == null)
                return true;

            if (_buttonInfo.IsPositionWithinRectTransform(_pointerPosition))
            {
                float relativePositionX = (_pointerPosition.x - _buttonInfo.Left) / _buttonInfo.Width; // Between 0 and 1
                float relativePositionY = (_pointerPosition.y - _buttonInfo.Bottom) / _buttonInfo.Height; // Between 0 and 1

                int pixelX = Convert.ToInt32(relativePositionX * _buttonInfo.TextureCopy.width - 1);
                int pixelY = Convert.ToInt32(relativePositionY * _buttonInfo.TextureCopy.height - 1);

                Color colorAtMousePosition = _buttonInfo.TextureCopy.GetPixel(pixelX, pixelY);
                //Debug.Log("Color at Pixel(" + pixelX.ToString() + ", " + pixelY.ToString() + "): " + colorAtMousePosition.ToString());

                if (colorAtMousePosition.a == 0f)
                    return true;
            }

            return false;
        }

        private class ButtonInfo
        {
            public ButtonInfo(Button _button)
            {
                Button = _button;
            }

            public Button Button { get; }

            public float Left { get; private set; }
            public float Bottom { get; private set; }
            public float Right { get; private set; }
            public float Top { get; private set; }

            public float Width { get { return Right - Left; } }
            public float Height { get { return Top - Bottom; } }

            public Texture2D TextureCopy { get; private set; } // Perform tasks using a copy instead of original.

            public void UpdateButtonInfo()
            {
                try
                {
                    //Debug.Log("Button Name: " + Button.name);

                    Vector3[] sb = new Vector3[4];
                    Button.image?.rectTransform?.GetWorldCorners(sb);

                    Left = sb[0].x;
                    Bottom = sb[0].y;
                    Right = sb[2].x;
                    Top = sb[2].y;

                    Texture2D originalTexture = Button.image?.sprite?.texture;
                    if (originalTexture == null)
                    {
                        TextureCopy = null;
                        return;
                    }
                    TextureCopy = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, false);
                    TextureCopy.LoadRawTextureData(originalTexture.GetRawTextureData());
                    TextureCopy.Apply();
                    //Debug.Log("Reference texture has been updated.");
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }

            public bool IsPositionWithinRectTransform(Vector2 _position)
            {
                return _position.x >= Left && _position.x <= Right && _position.y >= Bottom && _position.y <= Top;
            }
        }
    }
}
