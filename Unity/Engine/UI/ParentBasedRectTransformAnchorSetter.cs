using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ParentBasedRectTransformAnchorSetter : MonoBehaviour
    {
        #if UNITY_EDITOR
        void OnValidate()
        {
            try
            {
                RectTransform rt_parent = this.transform.parent as RectTransform; 
                float parentAnchorWidth = rt_parent.anchorMax.x - rt_parent.anchorMin.x;
                float parentAnchorHeight = rt_parent.anchorMax.y - rt_parent.anchorMin.y;

                float myRelativeAnchorWidth = m_anchorMax.x - m_anchorMin.x;
                float myRelativeAnchorHeight = m_anchorMax.y - m_anchorMin.y;

                float myActualAnchorWidth = myRelativeAnchorWidth / parentAnchorWidth;
                float myActualAnchorHeight = myRelativeAnchorHeight / parentAnchorHeight;

                float relativeDistanceBetweenMyMinXAndSpecifiedX = (m_anchorMin.x - rt_parent.anchorMin.x);
                float relativeDistanceBetweenMyMinYAndSpecifiedY = (m_anchorMin.y - rt_parent.anchorMin.y);

                float myActualMinX = relativeDistanceBetweenMyMinXAndSpecifiedX / parentAnchorWidth;
                float myActualMinY = relativeDistanceBetweenMyMinYAndSpecifiedY / parentAnchorHeight;

                float myActualMaxX = myActualMinX + myActualAnchorWidth;
                float myActualMaxY = myActualMinY + myActualAnchorHeight;

                RectTransform rt = this.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(myActualMinX, myActualMinY);
                rt.anchorMax = new Vector2(myActualMaxX, myActualMaxY);
            }
            catch (Exception ex)
            {
                Debug.Log("Error while converting parent-based anchor to actual anchor!");
            }
        }
        #endif

        [SerializeField]
        private Vector2 m_anchorMin;
        public Vector2 AnchorMin { get { return m_anchorMin; } set { m_anchorMin = value; } }

        [SerializeField]
        private Vector2 m_anchorMax;
        public Vector2 AnchorMax { get { return m_anchorMax; } set { m_anchorMax = value; } }
    }
}