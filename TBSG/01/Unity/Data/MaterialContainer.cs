using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class MaterialContainer
    {
        private static MaterialContainer m_instance;

        public static MaterialContainer Instance { get { return m_instance ?? (m_instance = new MaterialContainer()); } }

        private MaterialContainer() { }


        #region Properties
        public IList<Material> RarityMaterials { get { return m_rarityMaterials.AsReadOnly(); } }
        #endregion

        #region Private Fields
        private List<Material> m_rarityMaterials;
        #endregion

        #region Public Functions
        public void Initialize(List<Material> _rarityMaterials)
        {
            m_rarityMaterials = _rarityMaterials;
        }
        #endregion
    }
}
