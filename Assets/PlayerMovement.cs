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


    #endregion

    #region Unity Lifecycle

    // Start is called before the first frame update

    private void Awake()
    {
        _rigidbody= GetComponent<Rigidbody>();
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
    }

    private void FixedUpdate()
    {
        _direction.y = _rigidbody.velocity.y;
        _rigidbody.velocity = _direction;

        RotateTowardsCamera();
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

    #endregion

    #region Privates and Protected

    private Vector3 _direction = new Vector3();

    private Rigidbody _rigidbody;

    private Transform _cameraTransform;
    #endregion

}
