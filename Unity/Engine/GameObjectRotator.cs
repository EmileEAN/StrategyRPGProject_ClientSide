using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine
{
    public class GameObjectRotator : MonoBehaviour
    {
        #region Serialized Fields
        public List<Vector3FloatPair> RotationAndSecondsToComplete;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            StartCoroutine(Rotate());
        }

        IEnumerator Rotate()
        {
            foreach (Vector3FloatPair animation in RotationAndSecondsToComplete)
            {
                yield return StartCoroutine(this.transform.RotateInSeconds(animation.Key, animation.Value));

                yield return null;
            }
        }
    }
}