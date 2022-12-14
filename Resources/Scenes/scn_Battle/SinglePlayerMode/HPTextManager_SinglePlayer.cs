using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class HPTextManager_SinglePlayer : MonoBehaviour
    {

        public TextMesh MyTextMesh;
        public float Velocity;
        public float LifeSpan;
        private float m_startTime;

        // Awake is called before Update for the first frame
        void Awake()
        {
            MyTextMesh = this.GetComponent<TextMesh>();
            m_startTime = Time.realtimeSinceStartup;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.realtimeSinceStartup <= m_startTime + LifeSpan)
                this.transform.localPosition += new Vector3(0, Velocity);
            else
                Destroy(this.gameObject);
        }
    }
}