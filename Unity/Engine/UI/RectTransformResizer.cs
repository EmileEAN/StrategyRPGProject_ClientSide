using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine
{
    public class RectTransformResizer : MonoBehaviour
    {
        #region Serialized Fields
        public List<Vector2FloatPair> RelativeSizeAndSecondsToComplete;
        #endregion

        #region Private Fields
        private RectTransform m_rt;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_rt = this.GetComponent<RectTransform>();

            StartCoroutine(Resize());
        }

        IEnumerator Resize()
        {
            foreach (Vector2FloatPair animation in RelativeSizeAndSecondsToComplete)
            {
                yield return StartCoroutine(m_rt.ResizingInSeconds(animation.Key, animation.Value));

                yield return null;
            }
        }
    }
}