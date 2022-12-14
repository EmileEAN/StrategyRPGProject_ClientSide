namespace EEANWorks.Games.TBSG._01
{
    public abstract class AnimationInfo
    {
    }

    public class SimpleAnimationInfo : AnimationInfo
    {
        public SimpleAnimationInfo(int _hitEffectId)
        {
            HitEffectId = _hitEffectId;
        }

        #region Properties
        public int HitEffectId { get; }
        #endregion
    }

    public class ProjectileAnimationInfo : AnimationInfo
    {
        public ProjectileAnimationInfo(int _projectileGenerationPointId, int _projectileId, int _hitEffectId)
        {
            ProjectileGenerationPointId = _projectileGenerationPointId;
            ProjectileId = _projectileId;
            HitEffectId = _hitEffectId;
        }

        #region Properties
        public int ProjectileGenerationPointId { get; }
        public int ProjectileId { get; }
        public int HitEffectId { get; }
        #endregion
    }

    public class LaserAnimationInfo : AnimationInfo
    {
        public LaserAnimationInfo(int _laserGenerationPointId, int _laserEffectId, int _hitEffectId)
        {
            LaserGenerationPointId = _laserGenerationPointId;
            LaserEffectId = _laserEffectId;
            HitEffectId = _hitEffectId;
        }

        #region Properties
        public int LaserGenerationPointId { get; }
        public int LaserEffectId { get; }
        public int HitEffectId { get; }
        #endregion
    }

    public class MovementAnimationInfo : AnimationInfo
    {
        public MovementAnimationInfo(int _attachmentEffectId)
        {
            AttachmentEffectId = _attachmentEffectId;
        }

        #region Properties
        public int AttachmentEffectId { get; }
        #endregion
    }
}
