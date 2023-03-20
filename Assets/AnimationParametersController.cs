using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationParametersController : MonoBehaviour
{
    [SerializeField]
    private bool _isGrounded;

    private void Awake()
    {
        //_anim = GetComponentInChildren<Animator>();
        TryGetComponent<GroundChecker>(out _groundChecker);
        TryGetComponent<PlayerMovement>(out _playerMovement);
        TryGetComponent<Transform>(out _playerTransform);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _isGrounded = _groundChecker._isGrounded;
       // Debug.Log(_isGrounded);
        _anim.SetBool("isGrounded", _isGrounded);
        int _speedXID = Animator.StringToHash("speedX");
        //La direction est global mais speed X et speed Y doivent être local(relative au player Transform), origine du nouveau vecteur 3
        Vector3 relativeDirection = _playerTransform.InverseTransformVector(_playerMovement._direction);
        _anim.SetFloat(_speedXID, relativeDirection.x);
        int _SpeedYID = Animator.StringToHash("speedY");
        _anim.SetFloat(_SpeedYID, relativeDirection.z);// Il va devant/derrière

    }
    public Animator _anim;
    private GroundChecker _groundChecker;
    private PlayerMovement _playerMovement;
    private Transform _playerTransform;
}
