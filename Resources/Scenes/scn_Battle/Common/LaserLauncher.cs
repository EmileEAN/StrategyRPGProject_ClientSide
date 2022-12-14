using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class LaserLauncher : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_laserEffectPrefab;
        #endregion

        #region Properties
        public GameObject LaserEffectPrefab { get { return m_laserEffectPrefab; } set { m_laserEffectPrefab = value; } }
        #endregion

        #region Private Fields
        private List<GameObject> m_gos_laserEffect;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_gos_laserEffect = new List<GameObject>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Public Methods
        public void GenerateLasers(int _numberOfLasers)
        {
            if (m_laserEffectPrefab == null)
            {
                Debug.Log(this.gameObject.name + "'s LaserLauncher: Laser Effect Prefab is not set!");
                return;
            }

            m_gos_laserEffect.Clear(); // Prevent laserEffects to be added to the previous amount
            for (int i = 0; i < _numberOfLasers; i++)
            {
                m_gos_laserEffect.Add(Instantiate(m_laserEffectPrefab, this.transform.position, this.transform.rotation, this.transform));
            }
        }

        public void LaunchLaser(Transform _transform_target) { LaunchLasers(new List<Transform> { _transform_target }); }
        public void LaunchLasers(List<Transform> _transforms_target)
        {
            if (m_gos_laserEffect.Count != _transforms_target.Count)
                return;

            for (int i = 0; i < m_gos_laserEffect.Count; i++)
            {
                GameObject m_go_laserEffect = m_gos_laserEffect[i];

                m_go_laserEffect.transform.parent = null;
                m_go_laserEffect.transform.LookAt(_transforms_target[i].transform);

                ParticleSystem[] particleSystems = m_go_laserEffect.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    particleSystem.Play();
                }
            }

            m_gos_laserEffect.Clear();
        }
        #endregion
    }
}