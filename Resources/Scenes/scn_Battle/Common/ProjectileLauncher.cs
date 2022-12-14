using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class ProjectileLauncher : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_projectilePrefab;

        [SerializeField]
        private float m_projectileForce;
        #endregion

        #region Properties
        public GameObject ProjectilePrefab { get { return m_projectilePrefab; } set { m_projectilePrefab = value; } }

        public float ProjectileForce { get { return m_projectileForce; } set { m_projectileForce = value; } }
        #endregion

        #region Private Fields
        private List<GameObject> m_gos_projectile;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_gos_projectile = new List<GameObject>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Public Methods
        public void GenerateProjectiles(int _numberOfProjectiles, GameObject _disposingEffectPrefab)
        {
            if (m_projectilePrefab == null)
            {
                Debug.Log(this.gameObject.name + "'s ProjectileLauncher: Projectile Prefab is not set!");
                return;
            }

            m_gos_projectile.Clear(); // Prevent projectiles to be added to the previous amount
            for (int i = 0; i < _numberOfProjectiles; i++)
            {
                m_gos_projectile.Add(Instantiate(m_projectilePrefab, this.transform.position, this.transform.rotation, this.transform));
            }

            foreach (GameObject go_projectile in m_gos_projectile)
            {
                go_projectile.GetComponent<ProjectileDisposer>().DisposingEffectPrefab = _disposingEffectPrefab;
            }
        }

        public void LaunchProjectile(Transform _transform_target) { LaunchProjectiles(new List<Transform> { _transform_target }); }
        public void LaunchProjectiles(List<Transform> _transforms_target)
        {
            if (m_gos_projectile.Count != _transforms_target.Count)
                return;

            for (int i = 0; i < m_gos_projectile.Count; i++)
            {
                GameObject m_go_projectile = m_gos_projectile[i];
                Rigidbody rigidbody = m_go_projectile.GetComponent<Rigidbody>();

                m_go_projectile.transform.parent = null;
                m_go_projectile.transform.LookAt(_transforms_target[i].transform);

                rigidbody.isKinematic = false;
                rigidbody.AddForce(m_go_projectile.transform.forward * m_projectileForce);
            }

            m_gos_projectile.Clear();
        }
        #endregion
    }
}