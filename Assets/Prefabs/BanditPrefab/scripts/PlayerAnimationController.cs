using UnityEngine;

namespace StealthGame
{
    /// <summary>
    /// Ce composant gère les animations du personnage.
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        #region Show in inspector

        [SerializeField] private PlayerInputController _inputController;
        [SerializeField] private PlayerMovementController _movementController;
        [SerializeField] private float _speedSmoothTime;

        #endregion


        #region Init

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        #endregion


        #region Update

        private void Update()
        {
            // Met à jour les états de l'Animator en fonction des états du personnage.
            UpdateMovementStates();

            // Met à jour la vitesse de déplacement.
            UpdateMovementSpeed();

            // Met à jour la direction du mouvement dans le référentiel du personnage.
            UpdateMovementDirection();

            // Met à jour le vecteur velocity du Rigidbody dans le référentiel du personnage.
            UpdateVelocity();

            // Met à jour la direction de la caméra dans le référentiel du personnage.
            UpdateCameraDirection();

            // Synchronise les états taggés "Sync" (pour synchroniser le mouvement des pieds)
            SynchronizeStates();
        }

        /// <summary>
        /// Met à jour les états de déplacement du personnage dans l'Animator.
        /// Avec une State Machine, on utiliserait les méthodes OnStateXXXEnter pour modifier ces paramètres.
        /// </summary>
        private void UpdateMovementStates()
        {
            _animator.SetBool(_isGroundedId, _movementController.IsGrounded);
            _animator.SetBool(_isSneakingId, _inputController.SneakInput.IsActive);
            _animator.SetBool(_isJoggingId, _inputController.RunInput.IsActive);
            _animator.SetBool(_isMovingId, _inputController.HasMovementInput);
            _animator.SetBool(_isIdleId, !_inputController.HasMovementInput);
        }

        /// <summary>
        /// Met à jour la vitesse de déplacement du personnage dans l'Animator.
        /// </summary>
        private void UpdateMovementSpeed()
        {
            // La vitesse de déplacement. NB : cette valeur est déjà lissée dans le PlayerMovementController.
            _animator.SetFloat(_movementSpeedId, _movementController.MovementSpeed);
            // La vitesse de déplacement normalisée. On la lisse grâce aux deux paramètres supplémentaires.
            _animator.SetFloat(_movementSpeedNormalizedId, _movementController.MovementSpeedNormalized, _speedSmoothTime, Time.deltaTime);
        }

        /// <summary>
        /// Met à jour la direction du mouvement dans le référentiel du personnage.
        /// On se base sur la direction du Rigidbody. Sa longueur est [0, 1].
        /// C'est un choix, ça veut dire que si notre personnage se fait pousser, il fera une animation dans ce sens (sauf si
        /// on en fait un cas spécial dans l'Animator avec un paramètre "se fait pousser" par exemple).
        /// </summary>
        private void UpdateMovementDirection()
        {
            Vector3 direction = _movementController.Direction;
            _animator.SetFloat(_directionXId, direction.x);
            _animator.SetFloat(_directionZId, direction.z);
        }

        /// <summary>
        /// Met à jour le vecteur velocity du Rigidbody dans le référentiel du personnage.
        /// </summary>
        private void UpdateVelocity()
        {
            Vector3 velocity = _movementController.Velocity;
            _animator.SetFloat(_velocityXId, velocity.x);
            _animator.SetFloat(_velocityYId, velocity.y);
            _animator.SetFloat(_velocityZId, velocity.z);

            // Le vecteur velocity avec les vitesses normalisées.
            Vector3 direction = _movementController.Direction;
            _animator.SetFloat(_velocityXNormalizedId, direction.x * _animator.GetFloat(_movementSpeedNormalizedId));
            _animator.SetFloat(_velocityZNormalizedId, direction.z * _animator.GetFloat(_movementSpeedNormalizedId));
        }

        /// <summary>
        /// Met à jour la direction de la caméra dans le référentiel du personnage.
        /// </summary>
        private void UpdateCameraDirection()
        {
            Vector3 camera = _movementController.CameraDirection;
            _animator.SetFloat(_cameraXId, camera.x);
            _animator.SetFloat(_cameraYId, camera.y);
            _animator.SetFloat(_cameraZId, camera.z);
        }

        #endregion


        #region Animation Events

        /// <summary>
        /// Méthode appelée par l'Animation Event placé sur l'animation d'anticipation du saut, au moment où le personnage
        /// doit commencer à s'élever dans les airs.
        /// </summary>
        public void OnJumpStartAnimationEvent()
        {
            _movementController.DoJump();
        }

        /// <summary>
        /// Méthode appelée par un State Machine Behaviour au moment de sortir de l'état Land pour redonner le contrôle au joueur.
        /// </summary>
        public void OnLandAnimationEnded()
        {
            _movementController.DoLand();
        }

        #endregion


        #region Triggers

        /// <summary>
        /// Méthode appelée par le MouvementController quand l'input de saut est valide.
        /// </summary>
        public void StartJumpingAnimation()
        {
            _animator.SetTrigger(_jumpId);
        }

        #endregion


        #region States Synchronization

        /// <summary>
        /// Synchronise les animations entre les états qui portent le tag "Sync". Le but est de synchroniser les positions des pieds
        /// entre les animations de marche/course et les animations de déplacement silencieux pour que les pieds ne fassent pas 
        /// n'importe quoi durant les transitions
        /// </summary>
        private void SynchronizeStates()
        {
            // L'Animator est dans une transition
            if (_animator.IsInTransition(0))
            {
                // On ne veut effectuer ce code qu'une fois au début de la transition. On utilise un bool pour s'en assurer.
                if (!_isTransitioning)
                {
                    _isTransitioning = true;

                    // On récupère des infos sur l'état courant et l'état suivant pour connaître le moment dans l'animation où on a commencé la transition.
                    AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                    AnimatorStateInfo nextStateInfo = _animator.GetNextAnimatorStateInfo(0);

                    // On teste si les deux états portent bien le même tag "Sync" et qu'ils sont différents
                    if (stateInfo.fullPathHash != nextStateInfo.fullPathHash && stateInfo.tagHash == _syncTagId && nextStateInfo.tagHash == _syncTagId)
                    {
                        // Les infos de la transition en cours
                        AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);

                        // Le moment où on est dans l'animation. stateInfo.normalizedTime retourne un float normalisé, c'est-à-dire entre 0 et 1 avec
                        // 0 le début et 1 la fin. Si on est au-delà de 1, alors c'est la loop suivante (par exemple, 2.3 signifie qu'on est à 0.3 dans
                        // la 3e loop de l'animation). On retire donc toute la partie non décimale (par exemple, on fait 2.3 - 2 = 0.3)
                        float normalizedTime = stateInfo.normalizedTime - Mathf.Floor(stateInfo.normalizedTime);

                        // On force la transition vers le nouvel état, au même moment où on quitte notre état actuel. Les pieds seront donc normalement
                        // au même endroit
                        _animator.CrossFade(nextStateInfo.fullPathHash, transitionInfo.duration, 0, normalizedTime, transitionInfo.normalizedTime);
                    }
                }
            }
            // On n'est plus dans une transition, alors on remet le bool à faux pour la prochaine transition.
            else
            {
                _isTransitioning = false;
            }
        }

        #endregion


        #region Private

        private Animator _animator;
        private bool _isTransitioning;

        private int _movementSpeedId = Animator.StringToHash("movementSpeed");
        private int _movementSpeedNormalizedId = Animator.StringToHash("movementSpeedNormalized");
        private int _directionXId = Animator.StringToHash("directionX");
        private int _directionZId = Animator.StringToHash("directionZ");
        private int _velocityXId = Animator.StringToHash("velocityX");
        private int _velocityYId = Animator.StringToHash("velocityY");
        private int _velocityZId = Animator.StringToHash("velocityZ");
        private int _velocityXNormalizedId = Animator.StringToHash("velocityXNormalized");
        private int _velocityZNormalizedId = Animator.StringToHash("velocityZNormalized");

        private int _jumpId = Animator.StringToHash("jump");

        private int _cameraXId = Animator.StringToHash("cameraX");
        private int _cameraYId = Animator.StringToHash("cameraY");
        private int _cameraZId = Animator.StringToHash("cameraZ");

        private int _isIdleId = Animator.StringToHash("isIdle");
        private int _isMovingId = Animator.StringToHash("isMoving");
        private int _isJoggingId = Animator.StringToHash("isJogging");
        private int _isSneakingId = Animator.StringToHash("isSneaking");
        private int _isGroundedId = Animator.StringToHash("isGrounded");

        private int _syncTagId = Animator.StringToHash("Sync");
        private int _randomIdleId = Animator.StringToHash("randomIdle");

        #endregion
    }
}