using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine
{
    public class GameObjectSlider : MonoBehaviour
    {
        #region Serialized Fields
        public List<Vector3FloatPair> MovementAndSecondsToComplete;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            StartCoroutine(Move());
        }

        IEnumerator Move()
        {
            foreach (Vector3FloatPair animation in MovementAndSecondsToComplete)
            {
                yield return StartCoroutine(this.transform.MoveInSeconds(animation.Key, true, animation.Value));

                yield return null;
            }
        }
    }
}