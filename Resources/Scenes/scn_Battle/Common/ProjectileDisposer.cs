using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class ProjectileDisposer : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_disposingEffectPrefab;
        #endregion

        #region Properties
        public GameObject DisposingEffectPrefab { get { return m_disposingEffectPrefab; } set { m_disposingEffectPrefab = value; } }
        #endregion

        private void OnCollisionEnter(Collision _collision)
        {
            if (m_disposingEffectPrefab != null)
                Instantiate(m_disposingEffectPrefab, this.transform.position + new Vector3(0, 5, 0), _collision.transform.rotation);

            Destroy(this.gameObject);
        }
    }
}