using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class SharedAssetContainer
    {
        private static SharedAssetContainer m_instance;

        public static SharedAssetContainer Instance { get { return m_instance ?? (m_instance = new SharedAssetContainer()); } }

        private SharedAssetContainer() { }

        #region Properties
        public GameObject ImagePrefab { get; private set; }
        public GameObject ObjectButtonPrefab { get; private set; }
        public GameObject ItemButtonPrefab { get; private set; }
        #endregion

        #region Public Functions
        public void Initialize(GameObject _imagePrefab, GameObject _objectButtonPrefab, GameObject _itemButtonPrefab)
        {
            ImagePrefab = _imagePrefab;
            ObjectButtonPrefab = _objectButtonPrefab;
            ItemButtonPrefab = _itemButtonPrefab;
        }
        #endregion
    }
}
