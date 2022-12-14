using UnityEngine;

namespace EEANWorks.Games.Unity.Engine.UI
{
    public class Anim_RotateContinuously : StateMachineBehaviour
    {

        public Vector3 Speed;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            _animator.transform.Rotate(new Vector3(Speed.x, Speed.y, Speed.z));
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