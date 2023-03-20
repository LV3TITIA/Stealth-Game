using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Floor Detection")]
    [SerializeField]
    private LayerMask _groundMask;

    [SerializeField]
    private Vector3 _boxDimension;

    [SerializeField]
    private Transform _groundChecker;

    // Start is called before the first frame update
    void Start()
    {
        _groundChecker = transform.Find("GroundChecker");
        _playerAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame 
    void FixedUpdate()
    {
        // Si le state du layer Logic n'est pas Jumping
        int _logicIndex = _playerAnimator.GetLayerIndex("Logic");
        AnimatorStateInfo _a = _playerAnimator.GetCurrentAnimatorStateInfo(_logicIndex);

        if (!_a.IsName("Jumping"))
        {
            Collider[] groundColliders = Physics.OverlapBox(_groundChecker.position, _boxDimension, Quaternion.identity, _groundMask);
            //Debug.Log(groundColliders.Length);
            if (groundColliders.Length > 0)
            {
                //Debug.Log("Je touche le sol");
                _isGrounded = true;
            }
            else _isGrounded = false;
        }

        if (_a.IsName("Jumping"))
        {
            _isGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(_groundChecker.position, _boxDimension * 2f);
    }

    public bool _isGrounded {get; private set;}

    private Animator _playerAnimator;
}
