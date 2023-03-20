using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private void Awake()
    {
        TryGetComponent<GroundChecker>(out _groundChecker);
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerAnimator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        int _logicIndex = _playerAnimator.GetLayerIndex("Logic");
        AnimatorStateInfo _a = _playerAnimator.GetCurrentAnimatorStateInfo(_logicIndex);

        if (Input.GetButton("Jump") && _groundChecker._isGrounded && !_a.IsName("Jumping"))// Ne pas être dans jumping
        {

            _playerAnimator.SetTrigger("JumpTrigger");// a mettre dans l'animator parameter Controller
        }
        else
        {
            //_animator.GetBool("isGrounded");
        }
    }
    private GroundChecker _groundChecker;
    private Animator _playerAnimator;
}
