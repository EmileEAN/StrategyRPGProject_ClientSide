using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine
{
    public class GameObjectResizer : MonoBehaviour
    {
        #region Serialized Fields
        public List<Vector3FloatPair> RelativeSizeAndSecondsToComplete;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            StartCoroutine(Resize());
        }

        IEnumerator Resize()
        {
            foreach (Vector3FloatPair animation in RelativeSizeAndSecondsToComplete)
            {
                yield return StartCoroutine(this.transform.ResizingInSeconds(animation.Key, animation.Value));

                yield return null;
            }
        }
    }
}