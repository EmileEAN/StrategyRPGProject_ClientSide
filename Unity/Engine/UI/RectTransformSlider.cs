using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine
{
    public class RectTransformSlider : MonoBehaviour
    {
        #region Serialized Fields
        public List<Vector2FloatPair> MovementAndSecondsToComplete;
        #endregion

        #region Private Fields
        private RectTransform m_rt;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_rt = this.GetComponent<RectTransform>();

            StartCoroutine(Move());
        }

        IEnumerator Move()
        {
            foreach (Vector2FloatPair animation in MovementAndSecondsToComplete)
            {
                yield return StartCoroutine(m_rt.MoveInSeconds(animation.Key, true, animation.Value));

                yield return null;
            }
        }
    }
}