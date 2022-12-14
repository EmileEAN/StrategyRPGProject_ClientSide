using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class EffectPrefabContainer
    {
        private static EffectPrefabContainer m_instance;

        public static EffectPrefabContainer Instance { get { return m_instance ?? (m_instance = new EffectPrefabContainer()); } }

        private EffectPrefabContainer() { }

        #region Properties
        public GameObject BasicAttackActivationEffectPrefab { get; private set; }
        public GameObject BasicAttackHitEffectPrefab { get; private set; }
        public List<GameObject> SkillActivationEffectPrefabs { get; private set; }
        public List<GameObject> EffectGenerationPointPrefabs { get; private set; } // Magic circle, light orb, etc...
        public List<GameObject> ProjectilePrefabs { get; private set; }
        public List<GameObject> LaserEffectPrefabs { get; private set; }
        public List<GameObject> HitEffectPrefabs { get; private set; }
        public List<GameObject> AttachmentEffectPrefabs { get; private set; }
        #endregion


        #region Public Functions
        public void Initialize(GameObject _basicAttackActivationEffectPrefab, GameObject _basicAttackHitEffectPrefab, List<GameObject> _skillActivationEffectPrefabs, List<GameObject> _effectGenerationPointPrefabs, List<GameObject> _projectilePrefabs, List<GameObject> _laserEffectPrefabs, List<GameObject> _hitEffectPrefabs, List<GameObject> _attachmentEffectPrefabs)
        {
            BasicAttackActivationEffectPrefab = _basicAttackActivationEffectPrefab;
            BasicAttackHitEffectPrefab = _basicAttackHitEffectPrefab;
            SkillActivationEffectPrefabs = _skillActivationEffectPrefabs;
            EffectGenerationPointPrefabs = _effectGenerationPointPrefabs;
            ProjectilePrefabs = _projectilePrefabs;
            LaserEffectPrefabs = _laserEffectPrefabs;
            HitEffectPrefabs = _hitEffectPrefabs;
            AttachmentEffectPrefabs = _attachmentEffectPrefabs;
        }
        #endregion
    }
}
