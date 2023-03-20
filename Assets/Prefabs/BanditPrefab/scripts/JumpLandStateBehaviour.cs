using UnityEngine;

namespace StealthGame
{
    /// <summary>
    /// Ce StateMachine Behaviour permet d'envoyer au Player un message comme quoi le saut est terminé à la fin
    /// de l'animation d'atterrissage.
    /// </summary>
    public class JumpLandStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_animationController == null)
            {
                _animationController = animator.GetComponentInParent<PlayerAnimationController>();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animationController?.OnLandAnimationEnded();
        }

        private PlayerAnimationController _animationController;
    }
}