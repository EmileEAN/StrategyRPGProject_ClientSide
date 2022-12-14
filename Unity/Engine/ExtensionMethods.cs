using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Engine
{
    public static class CharExtension
    {
        public static string IntoColorTag(this char _char, Color32 _color) { return "<color=" + _color.ToHexString() + ">" + _char + "</color>"; }
    }

    public static class StringExtension
    {
        public static string IntoColorTag(this string _string, Color32 _color) { return "<color=" + _color.ToHexString() + ">" + _string + "</color>"; }
    }

    public static class GameObjectExtension
    {
        public static bool IsPointerOver(this GameObject _gameObject)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            return raycastResults.Any(x => x.gameObject == _gameObject);
        }

        public static bool IsPointOverAnyChild(this GameObject _gameObject) { return _gameObject.transform.IsPointerOverAnyChild(); }

        // The coroutine is required for OnStateExit, of Buttons with animation, to be called before SetActive() is applyied to its gameobject or the gameobject containing it.
        private static List<GameObject> m_gos_settingActive = new List<GameObject>();
        private static Dictionary<GameObject, List<Button>> m_gameObject_enabledAndInteractableButtonsInChildren = new Dictionary<GameObject, List<Button>>();
        private static Dictionary<GameObject, List<Image>> m_gameObject_enabledImagesInChildren = new Dictionary<GameObject, List<Image>>();
        private static Dictionary<GameObject, List<Text>> m_gameObject_enabledTextsInChildren = new Dictionary<GameObject, List<Text>>();
        public static bool IsSettingActive(this GameObject _gameObject) { return m_gos_settingActive.Contains(_gameObject); }
        public static IEnumerator SetActiveAfterOneFrame(this GameObject _gameObject, bool _value)
        {
            if (m_gos_settingActive.Contains(_gameObject))
                yield break;

            m_gos_settingActive.Add(_gameObject);

            List<Button> buttons_enabledAndInteractable = new List<Button>();
            List<Image> images_enabled = new List<Image>();
            List<Text> texts_enabled = new List<Text>();
            if (_value == false) // Save what visual components were enabled/interactable
            {
                buttons_enabledAndInteractable = _gameObject.GetComponentsInChildren<Button>()?.Where(x => x.enabled && x.interactable).ToList();
                images_enabled = _gameObject.GetComponentsInChildren<Image>()?.Where(x => x.enabled).ToList();
                texts_enabled = _gameObject.GetComponentsInChildren<Text>()?.Where(x => x.enabled).ToList();

                m_gameObject_enabledAndInteractableButtonsInChildren.Add(_gameObject, buttons_enabledAndInteractable);
                m_gameObject_enabledImagesInChildren.Add(_gameObject, images_enabled);
                m_gameObject_enabledTextsInChildren.Add(_gameObject, texts_enabled);
            }
            else // Load the visual components if available
            {
                if (m_gameObject_enabledAndInteractableButtonsInChildren.ContainsKey(_gameObject))
                {
                    buttons_enabledAndInteractable = m_gameObject_enabledAndInteractableButtonsInChildren[_gameObject];
                    m_gameObject_enabledAndInteractableButtonsInChildren.Remove(_gameObject);
                }

                if (m_gameObject_enabledImagesInChildren.ContainsKey(_gameObject))
                {
                    images_enabled = m_gameObject_enabledImagesInChildren[_gameObject];
                    m_gameObject_enabledImagesInChildren.Remove(_gameObject);
                }

                if (m_gameObject_enabledTextsInChildren.ContainsKey(_gameObject))
                {
                    texts_enabled = m_gameObject_enabledTextsInChildren[_gameObject];
                    m_gameObject_enabledTextsInChildren.Remove(_gameObject);
                }
            }

            foreach (Button button in buttons_enabledAndInteractable) { button.interactable = _value; }
            foreach (Image image in images_enabled) { image.enabled = _value; }
            foreach (Text text in texts_enabled) { text.enabled = _value; }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            _gameObject.SetActive(_value);

            m_gos_settingActive.Remove(_gameObject);
        }
    }

    public static class TransformExtension
    {
        public static void ClearChildren(this Transform _transform)
        {
            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in _transform)
            {
                children.Add(child.gameObject);
            }

            _transform.DetachChildren();

            foreach (GameObject go in children)
            {
                GameObject.Destroy(go);
            }
        }

        public static List<Transform> FindChildrenWithTag(this Transform _transform, string _tag)
        {
            List<Transform> result = new List<Transform>();
            foreach (Transform child in _transform)
            {
                if (child.tag == _tag)
                    result.Add(child);
            }

            return result;
        }

        public static Transform FindChildWithTag(this Transform _transform, string _tag)
        {
            foreach (Transform child in _transform)
            {
                if (child.tag == _tag)
                    return child;
            }

            return null;
        }

        public static IEnumerator MoveInSeconds(this Transform _transform, Vector3 _destination, bool _isPositionRelative, float _secondsToComplete)
        {
            float initialTime = Time.time;
            Vector3 initialPosition = _transform.position;
            Vector3 movement = _isPositionRelative ? _destination : _destination - initialPosition;

            float timeElapsed = 0;
            while (timeElapsed < _secondsToComplete)
            {
                float movementRatio = timeElapsed / _secondsToComplete;
                Vector3 partialMovement = movement * movementRatio;

                _transform.position = initialPosition + partialMovement;

                timeElapsed += Time.deltaTime;

                yield return null;
            }
            _transform.position = initialPosition + movement;
        }

        public static IEnumerator RotateInSeconds(this Transform _transform, Vector3 _rotation, float _secondsToComplete)
        {
            float initialTime = Time.time;
            Vector3 initialRotation = _transform.eulerAngles;
            Vector3 rotation = _rotation;

            float timeElapsed = 0;
            while (timeElapsed < _secondsToComplete)
            {
                float rotationRatio = timeElapsed / _secondsToComplete;
                Vector3 partialRotation = rotation * rotationRatio;

                _transform.eulerAngles = initialRotation + partialRotation;

                timeElapsed += Time.deltaTime;

                yield return null;
            }
            _transform.eulerAngles = initialRotation + rotation;
        }

        public static IEnumerator ResizingInSeconds(this Transform _transform, Vector3 _relativeSize, float _secondsToComplete)
        {
            float initialTime = Time.time;
            Vector3 initialScale = _transform.localScale;
            Vector3 scale = _relativeSize;

            float timeElapsed = 0;
            while (timeElapsed < _secondsToComplete)
            {
                float resizeRatio = timeElapsed / _secondsToComplete;
                Vector3 partialScale = new Vector3();

                if (scale.x == 1 || scale.x < 0)
                    partialScale.x = 1;
                else if (scale.x > 1)
                    partialScale.x = scale.x * resizeRatio;
                else if (scale.x > 0 && scale.x < 1)
                    partialScale.x = 1 - scale.x * resizeRatio;
                else // scale.x == 0
                    partialScale.x = 1 - resizeRatio;

                if (scale.y == 1 || scale.y < 0)
                    partialScale.y = 1;
                else if (scale.y > 1)
                    partialScale.y = scale.y * resizeRatio;
                else if (scale.y > 0 && scale.y < 1)
                    partialScale.y = 1 - scale.y * resizeRatio;
                else // scale.y == 0
                    partialScale.y = 1 - resizeRatio;

                if (scale.z == 1 || scale.z < 0)
                    partialScale.z = 1;
                else if (scale.z > 1)
                    partialScale.z = scale.z * resizeRatio;
                else if (scale.z > 0 && scale.z < 1)
                    partialScale.z = 1 - scale.z * resizeRatio;
                else // scale.z == 0
                    partialScale.z = 1 - resizeRatio;

                _transform.localScale = initialScale.MultiplyBy(partialScale);

                timeElapsed += Time.deltaTime;

                yield return null;
            }
            _transform.localScale = initialScale.MultiplyBy(scale);
        }

        /// <summary>
        /// _destinationPointOnReferenceAxis: 
        /// Less than 0 => moving backwards. 
        /// 0 => no movement. 
        /// Between 0 and 1 => move to some point in between the starting point and vertex.
        /// 1 => move to vertex. 
        /// Greater than 1 => passing vertex.
        /// </summary>
        public static IEnumerator ParabolicMotionInSeconds(this Transform _transform, eAxis _referenceAxis, Vector3 _vertex, bool _isPositionsRelative, float _relativeDestinationPoint, float _secondsToComplete)
        {
            float initialTime = Time.time;
            Vector3 initialPosition_3D = _transform.position;

            Vector3 vertex_3D = _isPositionsRelative ? _vertex + initialPosition_3D : _vertex;

            switch (_referenceAxis)
            {
                default: // case eAxis.Y
                    break;
                case eAxis.X:
                    {
                        initialPosition_3D = initialPosition_3D.SwapValues(e3DCoordValueSwapMethod.RotateToRight);
                        vertex_3D = vertex_3D.SwapValues(e3DCoordValueSwapMethod.RotateToRight);
                    }
                    break;
                case eAxis.Z:
                    {
                        initialPosition_3D = initialPosition_3D.SwapValues(e3DCoordValueSwapMethod.RotateToLeft);
                        vertex_3D = vertex_3D.SwapValues(e3DCoordValueSwapMethod.RotateToLeft);
                    }
                    break;
            }

            Vector3 initialPosition_withoutY = new Vector3(initialPosition_3D.x, 0, initialPosition_3D.z);


            // Convert 3D coordinates into 2D representation for parabolic motion calculation
            // Note that the 2D representation uses relative positions rather than actual Unity positions
            // Using formula y = a(x - p) ^ 2 + q
            Vector2 initialPosition_2D = Vector3.zero;
            Vector2 vertex_2D = new Vector2
            {
                x = Vector3.Distance(initialPosition_withoutY, new Vector3(vertex_3D.x, 0, vertex_3D.z)),
                y = vertex_3D.y - initialPosition_3D.y
            };

            float a = (initialPosition_2D.y - vertex_2D.y) / (float)Math.Pow(initialPosition_2D.x - vertex_2D.x, 2); // y = a(x - p) ^ 2 + q => a = (y - q) / ((x - p) ^ 2)


            Vector3 xzDistanceToVertex = vertex_3D - initialPosition_3D;
            xzDistanceToVertex.y = 0; // Remove for correct calculation of destination

            Vector3 destination_witoutY = initialPosition_3D + xzDistanceToVertex * _relativeDestinationPoint; // Set two properties correctly
            destination_witoutY.y = 0; // Remove for correct calculation of destination

            float destination_2D_x = Vector3.Distance(initialPosition_withoutY, destination_witoutY);

            float destination_2D_y = a * (float)Math.Pow(destination_2D_x - vertex_2D.x, 2); // y = a(x - p) ^ 2 + q
            destination_2D_y += vertex_2D.y; // For some reason writing the assignment statement in a single line causes the value to be incorrect

            Vector3 xzDistanceToDestination = destination_witoutY - initialPosition_3D;
            xzDistanceToDestination.y = 0; // Remove for correct calculation

            Vector3 destination_3D = destination_witoutY + new Vector3(0, initialPosition_3D.y + destination_2D_y, 0);


            float timeElapsed = 0;
            while (timeElapsed < _secondsToComplete)
            {
                float movementRatio = timeElapsed / _secondsToComplete;
                Vector3 newPosition_withoutY = initialPosition_3D + xzDistanceToDestination * movementRatio;
                newPosition_withoutY.y = 0; // Remove for correct calculation

                float newPosition_2D_x = Vector3.Distance(initialPosition_withoutY, newPosition_withoutY);
                float newPosition_2D_y = a * (float)Math.Pow(newPosition_2D_x - vertex_2D.x, 2) + vertex_2D.y;
                Vector3 newPosition = newPosition_withoutY + new Vector3(0, initialPosition_3D.y + newPosition_2D_y , 0);

                switch (_referenceAxis)
                {
                    default: // case eAxis.Y
                        break;
                    case eAxis.X:
                        newPosition = newPosition.SwapValues(e3DCoordValueSwapMethod.RotateToLeft);
                        break;
                    case eAxis.Z:
                        newPosition = newPosition.SwapValues(e3DCoordValueSwapMethod.RotateToRight);
                        break;
                }

                _transform.position = newPosition;

                timeElapsed += Time.deltaTime;

                yield return null;
            }

            switch (_referenceAxis)
            {
                default: // case eAxis.Y
                    break;
                case eAxis.X:
                    destination_3D = destination_3D.SwapValues(e3DCoordValueSwapMethod.RotateToLeft);
                    break;
                case eAxis.Z:
                    destination_3D = destination_3D.SwapValues(e3DCoordValueSwapMethod.RotateToRight);
                    break;
            }
            _transform.position = destination_3D;
        }

        public static bool IsPointerOver(this Transform _transform) { return _transform.gameObject.IsPointerOver(); }

        public static bool IsPointerOverAnyChild(this Transform _transform)
        {
            bool result = false;
            foreach (Transform child in _transform)
            {
                if (child.IsPointerOver())
                {
                    result = true;
                    break;
                }
                else if (child.childCount > 0)
                {
                    if (child.IsPointerOverAnyChild())
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }
    }

    public enum eAxis
    {
        X,
        Y,
        Z
    }

    public static class RectTransformExtension
    {
        public static void SetLeft(this RectTransform _rectTransform, float _left) { _rectTransform.offsetMin = new Vector2(_left, _rectTransform.offsetMin.y); }

        public static void SetRight(this RectTransform _rectTransform, float _right) { _rectTransform.offsetMax = new Vector2(-_right, _rectTransform.offsetMax.y); }

        public static void SetTop(this RectTransform _rectTransform, float _top) { _rectTransform.offsetMax = new Vector2(_rectTransform.offsetMax.x, -_top); }

        public static void SetBottom(this RectTransform _rectTransform, float _bottom) { _rectTransform.offsetMin = new Vector2(_rectTransform.offsetMin.x, _bottom); }

        public static void SetPosition(this RectTransform _rectTransform, float _left, float _right, float _top, float _bottom)
        {
            _rectTransform.SetLeft(_left);
            _rectTransform.SetRight(_right);
            _rectTransform.SetTop(_top);
            _rectTransform.SetBottom(_bottom);
        }

        public static IEnumerator MoveInSeconds(this RectTransform _rt, Vector2 _targetAnchorMin, bool _isPositionRelative, float _secondsToComplete)
        {
            float initialTime = Time.time;
            Vector2 initialAnchorMin = _rt.anchorMin;
            Vector2 initialAnchorMax = _rt.anchorMax;
            Vector2 movement = _isPositionRelative ? _targetAnchorMin : _targetAnchorMin - initialAnchorMin;

            float timeElapsed = 0;
            while (timeElapsed < _secondsToComplete)
            {
                float movementRatio = timeElapsed / _secondsToComplete;
                Vector2 partialMovement = movement * movementRatio;

                _rt.anchorMin = initialAnchorMin + partialMovement;
                _rt.anchorMax = initialAnchorMax + partialMovement;

                timeElapsed += Time.deltaTime;

                yield return null;
            }
            _rt.anchorMin = initialAnchorMin + movement;
            _rt.anchorMax = initialAnchorMax + movement;
        }

        public static IEnumerator ResizingInSeconds(this RectTransform _rt, Vector2 _relativeSize, float _secondsToComplete)
        {
            float initialTime = Time.time;
            Vector2 initialAnchorMin = _rt.anchorMin;
            Vector2 initialAnchorMax = _rt.anchorMax;
            Vector2 initialSize = _rt.sizeDelta;
            Vector2 scale = _relativeSize;

            float timeElapsed = 0;
            while (timeElapsed < _secondsToComplete)
            {
                float resizeRatio = timeElapsed / _secondsToComplete;
                Vector2 partialScale = new Vector2();

                if (scale.x == 1 || scale.x < 0)
                    partialScale.x = 1;
                else if (scale.x > 1)
                    partialScale.x = scale.x * resizeRatio;
                else if (scale.x > 0 && scale.x < 1)
                    partialScale.x = 1 - scale.x * resizeRatio;
                else // scale.x == 0
                    partialScale.x = 1 - resizeRatio;

                if (scale.y == 1 || scale.y < 0)
                    partialScale.y = 1;
                else if (scale.y > 1)
                    partialScale.y = scale.y * resizeRatio;
                else if (scale.y > 0 && scale.y < 1)
                    partialScale.y = 1 - scale.y * resizeRatio;
                else // scale.y == 0
                    partialScale.y = 1 - resizeRatio;

                Vector2 newSize = initialSize.MultiplyBy(partialScale);
                Vector2 halfOfNewSize = newSize / 2;
                _rt.anchorMin = initialAnchorMin - halfOfNewSize;
                _rt.anchorMax = initialAnchorMax + halfOfNewSize;

                timeElapsed += Time.deltaTime;

                yield return null;
            }
            Vector2 eventualSize = initialSize.MultiplyBy(scale);
            Vector2 halfOfEventualSize = eventualSize / 2;
            _rt.anchorMin = initialAnchorMin - halfOfEventualSize;
            _rt.anchorMax = initialAnchorMax + halfOfEventualSize;
        }
    }

    public static class Vector2Extension
    {
        public static Vector2 MultiplyBy(this Vector2 _vector, Vector2 _multiplier) { return Vector2.Scale(_vector, _multiplier); }

        public static Vector2 SwapValues(this Vector2 _vector) { return new Vector2(_vector.y, _vector.x); }
    }

    public static class Vector3Extension
    {
        public static Vector3 Pow(this Vector3 _vector, double _power)
        {
            float x = (float)Math.Pow(_vector.x, _power);
            float y = (float)Math.Pow(_vector.y, _power);
            float z = (float)Math.Pow(_vector.z, _power);

            return new Vector3(x, y, z);
        }

        public static Vector3 MultiplyBy(this Vector3 _vector, Vector3 _multiplier) { return Vector3.Scale(_vector, _multiplier); }

        public static Vector3 DivideBy(this Vector3 _vector, Vector3 _divisor)
        {
            if ((_vector.x != 0 && _divisor.x == 0)
                ||(_vector.y != 0 && _divisor.y == 0)
                ||(_vector.z != 0 && _divisor.z == 0))
            {
                return Vector3.negativeInfinity; // Return this as an error value.
            }

            // Do not consider x, y, and/or z for division if the correspoinding value of _vector and _divisor are both 0
            float x = _divisor.x == 0 ? 0 : _vector.x / _divisor.x;
            float y = _divisor.y == 0 ? 0 : _vector.y / _divisor.y;
            float z = _divisor.z == 0 ? 0 : _vector.z / _divisor.z;

            return new Vector3(x, y, z);
        }

        public static Vector3 SwapValues(this Vector3 _vector, e3DCoordValueSwapMethod _swapMethod)
        {
            switch (_swapMethod)
            {
                default: // case eVector3ValueSwap.RotateToRight
                    return new Vector3(_vector.z, _vector.x, _vector.y);
                case e3DCoordValueSwapMethod.RotateToRight:
                    return new Vector3(_vector.y, _vector.z, _vector.x);
                case e3DCoordValueSwapMethod.XAndY:
                    return new Vector3(_vector.y, _vector.x, _vector.z);
                case e3DCoordValueSwapMethod.YAndZ:
                    return new Vector3(_vector.x, _vector.z, _vector.y);
                case e3DCoordValueSwapMethod.ZAndX:
                    return new Vector3(_vector.z, _vector.y, _vector.x);
            }
        }
    }

    public static class TextExtension
    {
        public static IEnumerator CountNumber(this Text _text, decimal _startNumber, decimal _endNumber, float _timeToComplete, int _numberOfDecimalPoints)
        {
            if (_numberOfDecimalPoints < 0)
                _numberOfDecimalPoints = 0;

            float startTime = Time.realtimeSinceStartup;

            decimal difference = _endNumber - _startNumber;

            string numberStringFormat = "N" + _numberOfDecimalPoints.ToString();

            decimal currentValue = _startNumber;

            _text.text = currentValue.ToString(numberStringFormat);

            yield return null; // Make text change visible

            while (Time.realtimeSinceStartup - startTime < _timeToComplete) // While the total elapsed time is less than _timeToComplete
            {
                float elapsedTimePercentage = Time.deltaTime / _timeToComplete; // Get the percentage of 'time elpased since last frame' relative to '_timeToComplete'

                decimal modifyingValue = difference * Convert.ToDecimal(elapsedTimePercentage);

                currentValue += modifyingValue;
                if ((modifyingValue >= 0) ? (currentValue > _endNumber) : (currentValue < _endNumber)) // If the value exceeds the limit
                    currentValue = _endNumber; // Adjust it

                //Debug.Log(currentValue.ToString());

                _text.text = currentValue.ToString(numberStringFormat);

                yield return null;
            }

            _text.text = _endNumber.ToString(numberStringFormat);
        }
    }

    public static class ColorExtension
    {
        public static string ToHexString(this Color _color)
        {
            Color32 color = _color;
            return color.ToHexString();
        }

        public static Color RGBShift(this Color _color, eFixedGradient _colorShiftType, int _timesToShift, bool _fromRedToYellow = true)
        {
            Color32 color = _color;
            return color.RGBShift(_colorShiftType, _timesToShift, _fromRedToYellow);
        }
        public static Color RGBShift(this Color _color, ePointToPointGradient _gradientType, Color _initColor, Color _endColor, int _numOfColorsInBetween, int _timesToShift)
        {
            Color32 color = _color;
            return color.RGBShift(_gradientType, _initColor, _endColor, _numOfColorsInBetween, _timesToShift);
        }
    }

    public static class Color32Extension
    {
        public static string ToHexString(this Color32 _color) { return "#" + _color.r.ToString("X2") + _color.g.ToString("X2") + _color.b.ToString("X2") + _color.a.ToString("X2"); }

        public static Color32 RGBShift(this Color32 _color, eFixedGradient _gradientType, int _timesToShift, bool _fromRedToYellow = true)
        {
            switch (_gradientType)
            {
                case eFixedGradient._1536Colors:
                    return RGBShift_FixedGradient_1536Colors(_color, _timesToShift, _fromRedToYellow);
                default:
                    return _color;
            }
        }
        public static Color32 RGBShift(this Color32 _color, ePointToPointGradient _gradientType, Color32 _initColor, Color32 _endColor, int _numOfColorsInBetween, int _timesToShift)
        {
            switch (_gradientType)
            {
                case ePointToPointGradient.Loop:
                    return RGBShift_PointToPointGradient_Loop(_color, _initColor, _endColor, _numOfColorsInBetween, _timesToShift);
                default:
                    return _color;
            }
        }

        private static Color32 RGBShift_FixedGradient_1536Colors(Color32 _color, int _timesToShift, bool _fromRedToYellow)
        {
            if (_color.r != 255 && _color.g != 255 && _color.b != 255) //At least one property must be 255
                return _color;
            else if (_color.r > 0 && _color.g > 0 && _color.b > 0) //Only two properties can be greater than 0 at the same time
                return _color;

            if (_fromRedToYellow) // (r, g, b) => (255, 0, 0) -> (255, 255, 0)
            {
                for (int i = 0; i < _timesToShift; i++)
                {
                    if (_color.r == 255)
                    {
                        if (_color.b > 0)
                            _color.b--;
                        else if (_color.b == 0 && _color.g != 255)
                            _color.g++;
                        else if (_color.g == 255)
                            _color.r--;
                    }
                    else if (_color.g == 255)
                    {
                        if (_color.r > 0)
                            _color.r--;
                        else if (_color.r == 0 && _color.b != 255)
                            _color.b++;
                        else if (_color.b == 255)
                            _color.g--;
                    }
                    else // if (_color.b == 255)
                    {
                        if (_color.g > 0)
                            _color.g--;
                        else if (_color.g == 0 && _color.r != 255)
                            _color.r++;
                        else if (_color.r == 255)
                            _color.b--;
                    }
                }
            }
            else // from Red to Magenta (r, g, b) => (255, 0, 0) -> (255, 0, 255)
            {
                for (int i = 0; i < _timesToShift; i++)
                {
                    if (_color.r == 255)
                    {
                        if (_color.g > 0)
                            _color.g--;
                        else if (_color.g == 0 && _color.b != 255)
                            _color.b++;
                        else if (_color.b == 255)
                            _color.r--;
                    }
                    else if (_color.g == 255)
                    {
                        if (_color.b > 0)
                            _color.b--;
                        else if (_color.b == 0 && _color.r != 255)
                            _color.r++;
                        else if (_color.r == 255)
                            _color.g--;
                    }
                    else // if (_color.b == 255)
                    {
                        if (_color.r > 0)
                            _color.r--;
                        else if (_color.r == 0 && _color.g != 255)
                            _color.g++;
                        else if (_color.g == 255)
                            _color.b--;
                    }
                }
            }

            return _color;
        }

        private static Color32 RGBShift_PointToPointGradient_Loop(Color32 _color, Color32 _initColor, Color32 _endColor, int _numOfColorsInBetween, int _timesToShift)
        {
            if (_numOfColorsInBetween < 0)
                return _color;

            bool isInitRLess = _endColor.r >= _initColor.r;
            bool isInitGLess = _endColor.g >= _initColor.g;
            bool isInitBLess = _endColor.b >= _initColor.b;

            byte difference_r = Convert.ToByte(isInitRLess ? (_endColor.r - _initColor.r) : (_initColor.r - _endColor.r));
            byte diffegence_g = Convert.ToByte(isInitGLess ? (_endColor.g - _initColor.g) : (_initColor.g - _endColor.g));
            byte difference_b = Convert.ToByte(isInitBLess ? (_endColor.b - _initColor.b) : (_initColor.b - _endColor.b));

            int denominator = _numOfColorsInBetween + 1;
            byte differenceForMidpoints_r = Convert.ToByte(difference_r / denominator);
            byte differenceForMidpoints_g = Convert.ToByte(diffegence_g / denominator);
            byte differenceForMidpoints_b = Convert.ToByte(difference_b / denominator);

            List<Color32> colors = new List<Color32> { _initColor };
            for (int i = 1; i < _numOfColorsInBetween; i++)
            {
                byte r = Convert.ToByte(isInitRLess ? (_initColor.r + differenceForMidpoints_r * i) : (_endColor.r + differenceForMidpoints_r * i));
                byte g = Convert.ToByte(isInitGLess ? (_initColor.g + differenceForMidpoints_g * i) : (_endColor.g + differenceForMidpoints_g * i));
                byte b = Convert.ToByte(isInitBLess ? (_initColor.b + differenceForMidpoints_b * i) : (_endColor.b + differenceForMidpoints_b * i));

                Color32 midPoint = new Color32(r, g, b, _color.a);
                colors.Add(midPoint);
            }
            colors.Add(_endColor);

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].r == _color.r && colors[i].g == _color.g && colors[i].b == _color.b)
                {
                    int targetIndex = i + _timesToShift;
                    if (_timesToShift > colors.Count - 1)
                        targetIndex %= colors.Count;

                    _color = colors[targetIndex];

                    break;
                }
            }

            return _color;
        }
    }

    public enum eFixedGradient
    {
        _1536Colors,
    }

    public enum ePointToPointGradient
    {
        Loop,
        PingPong,
    }

    public static class MonoBehaviourExtension
    {
        public static Coroutine StartCoroutineRepetitionUntilTrue(this MonoBehaviour _monoBehaviour, IEnumerator _coroutine, LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            return _monoBehaviour.StartCoroutine(StartLoopingCoroutine(_monoBehaviour, _coroutine,  _looperAndCoroutineLinker));
        }

        static IEnumerator StartLoopingCoroutine(MonoBehaviour _monoBehaviour, IEnumerator _coroutine, LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            //float startTime = Time.realtimeSinceStartup;
            int maxNumOfRepetitions = 30;

            for (int i = 0; i < maxNumOfRepetitions; i++)
            {
                if (i == maxNumOfRepetitions - 1)
                    _looperAndCoroutineLinker.SetIsLoopEndToTrue();

                yield return _monoBehaviour.StartCoroutine(_coroutine);

                if (_looperAndCoroutineLinker.TerminateLoop)
                    break;
            }

            //Debug.Log((Time.realtimeSinceStartup - startTime).ToString() + "seconds <<<<<<<<<<<<<<<<<<<<<<<");
        }

        public sealed class LooperAndCoroutineLinker
        {
            public LooperAndCoroutineLinker()
            {
                IsEndOfLoop = false;
                TerminateLoop = false;
            }

            public bool IsEndOfLoop { get; private set; } // Set within the StartLoopingCoroutine() method and used within the target enumerator method
            public bool TerminateLoop { get; private set; } // Set within the target enumerator method

            public void SetIsLoopEndToTrue() { IsEndOfLoop = true; }
            public void SetTerminateLoopToTrue() { TerminateLoop = true; }
        }
    }
}
