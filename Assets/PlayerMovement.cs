using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    #region Exposed

    [SerializeField]
    private float _movespeed = 5f;

    [SerializeField]
    private float _turnSpeed = 10f;

    [SerializeField]
    private float _yFloorOffset = 1f;

    #endregion

    #region Unity Lifecycle

    // Start is called before the first frame update

    private void Awake()
    {
        _rigidbody= GetComponent<Rigidbody>();

        _floorDetector = GetComponentInChildren<FloorDetector>();

        _groundChecker = GetComponent<GroundChecker>();
    }
    void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Déplacement           AVANT / ARRIERE                            // DROITE / GAUCHE
        _direction = _cameraTransform.forward * Input.GetAxis("Vertical") + _cameraTransform.right * Input.GetAxis("Horizontal");
        _direction *= _movespeed;

        //_floorDetector.AverageHeight();
    }

    private void FixedUpdate()
    {
        if(_direction.magnitude > 1) RotateTowardsCamera();
        // _direction.y = _rigidbody.velocity.y; //On applique la gravité
        // _rigidbody.velocity = _direction;

        // if (_groundChecker._isGrounded)
        // {
        //     StickToGround();
        // }

        if (_groundChecker._isGrounded)
        {
            StickToGround();
            _direction.y = 0; // Il n'y a pas de mv en y
        }
        else
        {
            _direction.y = _rigidbody.velocity.y; //On applique la gravité
        }
        _rigidbody.velocity = _direction; //La velocité est mise à jour par rapports aux inputs du joueur et par rapport a STG
    }

    #endregion 

    #region Main methods

    private void RotateTowardsCamera()
    {
        Vector3 cameraForward = _cameraTransform.forward;
        cameraForward.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(cameraForward);
        Quaternion rotation = Quaternion.RotateTowards(_rigidbody.rotation, lookRotation, _turnSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(rotation);
    }

    private void StickToGround()
    {
        Vector3 averagePosition = _floorDetector.AverageHeight();

        Vector3 newPosition = new Vector3(_rigidbody.position.x, averagePosition.y + _yFloorOffset, _rigidbody.position.z);
        //transform.position = newPosition;
        _rigidbody.MovePosition(newPosition);// Selon la doc il faut un rgbd en kinematic
        //Debug.Log(newPosition.y);
    }

    #endregion

    #region Privates and Protected

    public Vector3 _direction = new Vector3(); //Get / Set à faire

    private Rigidbody _rigidbody;

    private Transform _cameraTransform;

    private FloorDetector _floorDetector;

    private GroundChecker _groundChecker;
    #endregion

}
