using EEANWorks.Games.Unity.Engine.EventSystems;
using UnityEngine;
using static UnityEngine.EventSystems.PointerEventData;

namespace EEANWorks.Games.Unity.Engine.UI
{
    public class Anim_ButtonPressedState : StateMachineBehaviour
    {

        private bool m_isInitialized = false;

        private CustomStandaloneInputModule m_inputModule;

        private void Initialize(Animator _animator)
        {
            m_inputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<CustomStandaloneInputModule>();
            if (!m_inputModule)
                return;

            m_isInitialized = true;
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            if (!m_isInitialized)
                Initialize(_animator);
            if (!m_isInitialized)
                return;

            if (_animator.gameObject == m_inputModule.CurrentRaycastObject)
            {
                switch (m_inputModule.CurrentPointerState_Left)
                {
                    case FramePressState.PressedAndReleased:
                    case FramePressState.Released:
                        _animator.Play("Highlighted");
                        break;
                    default:
                        break;
                }
            }
            else
                _animator.Play("Normal");
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

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