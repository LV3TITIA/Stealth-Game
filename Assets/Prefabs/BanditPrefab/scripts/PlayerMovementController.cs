using UnityEngine;

namespace StealthGame
{
    [SelectionBase]
    public class PlayerMovementController : MonoBehaviour
    {
        #region Show in inspector

        [Header("Controllers")]

        [SerializeField] private PlayerInputController _inputController;
        [SerializeField] private PlayerAnimationController _animationController;

        [Header("Movement parameters")]

        [Tooltip("La vitesse de déplacement en marche en m/s")]
        [SerializeField] private float _walkSpeed = 1.5f;

        [Tooltip("La vitesse de déplacement en jogging en m/s")]
        [SerializeField] private float _jogSpeed = 4.708f;

        [Tooltip("La vitesse de déplacement silencieux en m/s")]
        [SerializeField] private float _sneakSpeed;

        [Tooltip("La vitesse de rotation en °/s")]
        [SerializeField] private float _turnSpeed;

        [Space]

        [Tooltip("En combien de temps se font les changements de vitesse")]
        [SerializeField] private float _speedSmoothTime;

        [Tooltip("En combien de temps se font les changements de direction")]
        [SerializeField] private float _directionSmoothTime;

        [Header("Jump parameters")]

        [Tooltip("La force du saut en m/s²")]
        [SerializeField] private float _jumpForce;

        [Header("Gravity parameters")]

        [Tooltip("La gravité")]
        [SerializeField] private float _gravity;

        [Tooltip("Le facteur de gravité en saut")]
        [SerializeField] private float _gravityJumpMultiplier;

        [Tooltip("Le facteur de gravité en chute")]
        [SerializeField] private float _gravityFallMultiplier;

        [Header("Ground checker")]

        [SerializeField] private PlayerGroundChecker _groundChecker;

        #endregion


        #region Public

        /// <summary>
        /// La vitesse actuelle du Rigidbody sur le plan XZ.
        /// </summary>
        public float MovementSpeed
        {
            get => new Vector2(_rigidbody.velocity.x, _rigidbody.velocity.z).magnitude;
        }

        /// <summary>
        /// La vitesse normalisée sur le plan XZ en fonction de l'état du personnage.
        /// Idle => 0
        /// Sneak => 1
        /// Walk => 2
        /// Jog => 3
        /// </summary>
        public int MovementSpeedNormalized { get; private set; }

        /// <summary>
        /// Le vecteur Velocity du Rigidbody dans le référentiel du personnage.
        /// </summary>
        public Vector3 Velocity
        {
            get => _transform.InverseTransformVector(_rigidbody.velocity);
        }

        /// <summary>
        /// La direction actuelle du mouvement dans le référentiel du personnage.
        /// Sa taille est [0, 1] pour prendre en compte la quantité d'input et le lissage de changement de direction.
        /// </summary>
        public Vector3 Direction
        {
            //get => Vector3.ClampMagnitude(Velocity, 1);
            get => _transform.InverseTransformDirection(_movementDirection);
        }

        /// <summary>
        /// La direction actuelle de la caméra dans le référentiel du personnage.
        /// </summary>
        public Vector3 CameraDirection
        {
            get => _transform.InverseTransformDirection(_cameraTransform.forward);
        }

        /// <summary>
        /// Retourne true ssi le joueur touche le sol.
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        /// Retourne true ssi le joueur est en train de sauter (mouvement ascendant)
        /// </summary>
        public bool IsJumping { get; private set; }

        /// <summary>
        /// Retourne true ssi le joueur est en train de tomber (mouvement descendant)
        /// </summary>
        public bool IsFalling { get; private set; }

        #endregion


        #region Init

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _cameraTransform = Camera.main.transform;
        }


        #endregion


        #region Update

        private void Update()
        {
            // On commence par détecter le sol et calculer sa position.
            CheckGround();

            // Calcule la direction du mouvement en fonction des inputs.
            CalculateMovementDirection();

            // Met à jour la vitesse de déplacement en fonction des inputs
            SetSpeed();

            // Chute 
            if (_rigidbody.velocity.y < -0.01f)
            {
                IsJumping = false; // IsJumping passe à false signifie qu'on recommence à faire CheckGround
                IsFalling = true;
            }

            // Jumping
            if (!IsJumping && IsGrounded && !_inputController.SneakInput.IsDown && _inputController.JumpInput.IsDown)
            {
                _animationController.StartJumpingAnimation();

                // On met IsJumping à true ici pour indiquer qu'on est dans la phase montante du saut et arrêter 
                // l'appel à CheckGround. On met donc aussi IsGrounded à false.
                IsJumping = true;
            }
        }

        /// <summary>
        /// Détecte le sol et calcule sa position.
        /// </summary>
        private void CheckGround()
        {
            // Si le personnage est en phase montante d'un saut, alors on ne détecte pas le sol.
            // Si on le faisait, alors au début de chaque saut, on détecterait le sol ce qui metterait 
            // immédiatement fin au saut.
            if (IsJumping)
            {
                return;
            }

            // Détecte le sol et stocke sa position dans la variable _groundPosition
            IsGrounded = _groundChecker.CheckGround(out _groundPosition);
        }

        /// <summary>
        /// Calcule la direction du mouvement en fonction des inputs. Le changement de direction est lissé.
        /// </summary>
        private void CalculateMovementDirection()
        {
            if (_inputController.HasMovementInput)
            {
                //_targetDirection = _cameraTransform.TransformDirection(_inputController.MovementInput);
                // Equivalent à 
                _targetDirection = _cameraTransform.right * _inputController.HorizontalInput.Value +
                                   _cameraTransform.forward * _inputController.VerticalInput.Value;
                _targetDirection.y = 0;
                // On re-normalize la direction parce qu'on a enlevé la composante y (on peut donc être en-dessous d'une taille de 1)
                _targetDirection.Normalize();
                // On reporte la quantité d'input
                _targetDirection *= _inputController.MovementInput.magnitude;
            }

            // On lisse le changement de la direction grâce à un SmoothDamp.
            _movementDirection = Vector3.SmoothDamp(_movementDirection, _targetDirection, ref _directionSmoothDampVelocity, _directionSmoothTime);
        }

        /// <summary>
        /// Change la vitesse du personnage en fonction de son état actuel. Le changement de vitesse est lissé.
        /// </summary>
        private void SetSpeed()
        {
            // Si on saute, on ne fait pas de changement de vitesse (par exemple si on appuie sur le bouton Jog durant le saut)
            if (IsGrounded)
            {
                // On teste d'abord si on a un input de mouvement
                if (_inputController.HasMovementInput)
                {
                    // Puis on teste les différents inputs de changement de vitesse :

                    // Déplacement silencieux
                    if (_inputController.SneakInput.IsActive)
                    {
                        _targetSpeed = _sneakSpeed;
                        MovementSpeedNormalized = 1;
                    }

                    // Course
                    else if (_inputController.RunInput.IsActive)
                    {
                        _targetSpeed = _jogSpeed;
                        MovementSpeedNormalized = 3;
                    }

                    // Marche normale
                    else
                    {
                        _targetSpeed = _walkSpeed;
                        MovementSpeedNormalized = 2;
                    }
                }
                // S'il n'y a pas d'input de mouvement, alors on met la vitesse à 0
                else
                {
                    _targetSpeed = 0;
                    MovementSpeedNormalized = 0;
                }
            }

            // Lisse le changement de vitesse par un SmoothDamp
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, _targetSpeed, ref _speedSmoothDampVelocity, _speedSmoothTime);
        }

        #endregion


        #region Public

        /// <summary>
        /// Méthode appelée par l'Animation Controller, durant l'animation d'anticipation de saut
        /// </summary>
        public void DoJump()
        {
            _jumpTrigger = true;
        }

        /// <summary>
        /// Méthode appelée par l'Animation Controller, à la fin de l'animation d'atterrissage
        /// </summary>
        public void DoLand()
        {
            IsFalling = false;
        }

        #endregion


        #region Fixed Update

        private void FixedUpdate()
        {
            // Déplace le personnage sur le plan XZ
            MoveXZ();

            // Tourne le joueur vers l'orientation de la caméra
            RotateTowardsCameraForward();

            // Gravité
            ApplyGravity();

            // Saut
            ApplyJump();

            // Colle le personnage au sol
            StickToGround();
        }

        /// <summary>
        /// Déplace le Rigidbody sur le plan XZ.
        /// </summary>
        private void MoveXZ()
        {
            // Calcule le vecteur velocity en fonction des inputs
            Vector3 velocity = _movementDirection * _currentSpeed;

            // On rapporte le Y que le moteur physique a calculé
            velocity.y = _rigidbody.velocity.y;

            // On remplace le vecteur velocity du Rigidbody par ce qu'on a calculé.
            _rigidbody.velocity = velocity;
        }

        /// <summary>
        /// Fait tourner le Rigidbody dans la direction de la caméra quand le personnage est en mouvement.
        /// </summary>
        private void RotateTowardsCameraForward()
        {
            // S'il n'y a aucun mouvement, alors on sort immédiatement de la méthode.
            if (!_inputController.HasMovementInput)
            {
                return;
            }

            // La direction dans laquelle on veut tourner le personnage est la direction forward de la caméra.
            Vector3 lookDirection = _cameraTransform.forward;
            // La caméra peut regarder vers le haut ou le bas. On ne veut pas que le personnage s'oriente ni
            // vers le haut, ni vers le bas, alors on met la composante y à 0.
            lookDirection.y = 0;
            // On calcule la rotation vers cette direction
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            // Puis on calcule la rotation à appliquer à cette frame en fonction de la vitesse de rotation
            rotation = Quaternion.RotateTowards(_rigidbody.rotation, rotation, _turnSpeed * Time.fixedDeltaTime);
            // Enfin, on applique la rotation résultante au Rigidbody
            _rigidbody.MoveRotation(rotation);
        }

        /// <summary>
        /// Calcule et applique la gravité au Rigidbody
        /// </summary>
        private void ApplyGravity()
        {
            // Si le personnage est au sol, alors on n'applique aucune gravité, et on met la vitesse verticale à 0
            if (IsGrounded)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                return;
            }

            // Calcule le vecteur de gravité
            Vector3 gravity = Vector3.down * _gravity;

            // En cas de chute, on applique le multiplicateur de chute
            if (_rigidbody.velocity.y < -0.01f)
            {

                gravity *= _gravityFallMultiplier;
            }
            // Sinon, on est en phase montante d'un saut, on applique l'autre multiplicateur
            else
            {
                gravity *= _gravityJumpMultiplier;
            }

            // Applique le vecteur gravité au Rigidbody en ignorant la masse.
            _rigidbody.AddForce(gravity, ForceMode.Acceleration);
        }

        /// <summary>
        /// Applique le saut au Rigidbody quand il est déclenché.
        /// </summary>
        private void ApplyJump()
        {
            // Si le saut n'est pas déclenché, on sort tout de suite de la méthode.
            if (!_jumpTrigger)
            {
                return;
            }

            // Le trigger est consommé.
            _jumpTrigger = false;

            // On met IsGrounded à false ici
            IsGrounded = false;

            // Applique le vecteur du saut au Rigidbody comme une impulsion et en ignorant la masse.
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Colle le personnage au sol. On utilise cette méthode pour pouvoir monter les pentes et escaliers.
        /// </summary>
        private void StickToGround()
        {
            // Si le personnage n'est pas au sol, on sort tout de suite de la méthode.
            if (!IsGrounded)
            {
                return;
            }

            // On utilise la position y du sol calculée par la méthode CheckGround pour calculer la nouvelle
            // position du personnage.
            Vector3 position = _transform.position;
            position.y = _groundPosition.y;
            // On applique cette position en téléportant le Rigidbody.
            _rigidbody.MovePosition(position);
        }

        #endregion


        #region Private

        private Transform _transform;
        private Transform _cameraTransform;
        private Rigidbody _rigidbody;

        private Vector3 _targetDirection;
        private Vector3 _movementDirection;
        private Vector3 _directionSmoothDampVelocity;


        private float _currentSpeed;
        private float _targetSpeed;
        private float _speedSmoothDampVelocity;

        private bool _jumpTrigger;

        private Vector3 _groundPosition;
        private Vector3 _targetGroundPosition;
        private Vector3 _targetGroundPositionSmoothDampVelocity;

        #endregion
    }
}