using UnityEngine;

namespace StealthGame
{
    /// <summary>
    /// Ce composant permet de recevoir les AnimationEvent lancés depuis les clips d'animation.
    /// On appelle ensuite les méthodes correspondantes dans l'AnimationController.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationEventReceiver : MonoBehaviour
    {
        private void Awake()
        {
            _animationController = GetComponentInParent<PlayerAnimationController>();
        }

        private void JumpStartAnimationEvent()
        {
            _animationController.OnJumpStartAnimationEvent();
        }

        private PlayerAnimationController _animationController;
    }
}