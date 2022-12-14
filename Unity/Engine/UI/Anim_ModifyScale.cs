using UnityEngine;

namespace EEANWorks.Games.Unity.Engine.UI
{
    public class Anim_ModifyScale : StateMachineBehaviour
    {

        public Vector3 RelativeScale;
        public int FramesTillResizeEnd;

        private Vector3 m_originalScale;
        private Vector3 m_targetScale;
        private Vector3 m_scaleDifference;
        private Vector3 m_modificationPerFrame;
        private bool m_isInitialized = false;

        private bool m_modificationEnded;

        private void Initialize(Animator _animator)
        {
            if (!m_isInitialized)
            {
                m_originalScale = _animator.transform.localScale;

                m_targetScale = new Vector3(m_originalScale.x * RelativeScale.x, m_originalScale.y * RelativeScale.y, m_originalScale.z * RelativeScale.z);

                m_scaleDifference = m_targetScale - m_originalScale;

                if (FramesTillResizeEnd >= 1)
                    m_modificationPerFrame = m_scaleDifference / FramesTillResizeEnd;
                else
                    m_modificationPerFrame = m_scaleDifference;

                m_isInitialized = true;
            }
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            if (!m_isInitialized)
                Initialize(_animator);
            if (!m_isInitialized)
                return;

            m_modificationEnded = false;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            if (!m_isInitialized)
                Initialize(_animator);
            if (!m_isInitialized)
                return;

            if (!m_modificationEnded)
            {
                _animator.transform.localScale = _animator.transform.localScale + m_modificationPerFrame;

                Vector3 currentScale = _animator.transform.localScale;
                if (currentScale == m_targetScale)
                {
                    m_modificationEnded = true;
                }
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            if (!m_isInitialized)
                Initialize(_animator);
            if (!m_isInitialized)
                return;

            _animator.transform.localScale = m_originalScale;
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}