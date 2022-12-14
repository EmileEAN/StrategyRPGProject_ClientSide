using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class ParticleDisposer : MonoBehaviour
    {

        private float m_startTime;

        public float LifeSpan;

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_startTime = Time.realtimeSinceStartup;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.realtimeSinceStartup - m_startTime > LifeSpan)
                Destroy(this.gameObject);
        }
    }
}