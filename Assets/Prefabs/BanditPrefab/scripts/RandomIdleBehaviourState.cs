using UnityEngine;

/// <summary>
/// Ce StateMachine Behaviour est à placer sur un Sub-StateMachine d'états Idle. Il permet d'en choisir aléatoirement un.
/// Il suffit d'avoir un paramètre "randomIdle" et  d'ajouter un poids par animation Idle différente dans le tableau sérialisé 
/// pour changer les probabilités de jouer telle animation.
/// </summary>
public class RandomIdleBehaviourState : StateMachineBehaviour
{
    #region Show in inspector

    [SerializeField] private int[] _weights;

    #endregion


    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (!_isInitialized)
        {
            _weightTotal = 0;
            foreach (int weight in _weights)
            {
                _weightTotal += weight;
            }
            _isInitialized = true;
        }

        int randomIndex;
        int total = 0;
        int randVal = Random.Range(0, _weightTotal + 1);

        for (randomIndex = 0; randomIndex < _weights.Length; randomIndex++)
        {
            total += _weights[randomIndex];
            if (total >= randVal)
            {
                break;
            }
        }

        animator.SetInteger(_idleRandomId, randomIndex);
    }

    #region Private

    private int _weightTotal;
    private bool _isInitialized;
    private int _idleRandomId = Animator.StringToHash("randomIdle");
    
    #endregion
}
