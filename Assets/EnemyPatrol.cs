using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public enum PatrolType
{
    PINGPONG,
    CLOCKWISE,
    COUNTERCLOCK
    //CHASING
}

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField]
    private Transform[] _waypoints;

    [SerializeField]
    private int _startingID;

    [SerializeField]
    private PatrolType _patrolMode = PatrolType.CLOCKWISE;

    //[SerializeField]
    //public Transform _target;

    [Header("Gizmos")]
    public Color _gizmosColor;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _coneVision = GetComponentInChildren<ConeVision>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _agent.Warp(_waypoints[0].position);
        _agent.SetDestination(_waypoints[_startingID].position);
        _destinationWaypointID = _startingID;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(!_isChasing);
        //Debug.Log(_coneVision.m_target != null);
        if (_coneVision.m_target == null)
        {
            _isChasing = false;
        }
        if (_isChasing)
        {
            Debug.Log("Destination Le Bandit");
            _agent.SetDestination(_coneVision.m_target.transform.position);
            
        }
        if (_coneVision.m_target != null && !_isChasing) // La cible du cone de vision n'est pas null et _isChasing est false 
        {
            _isChasing = true;
        }
        else
        {
           // Debug.Log(_agent.remainingDistance);
            // Distance restante entre l'agent et ce qu'il lui rèste à parcourir 
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {                               // savoir si on est arrivé à déstination 
                //Debug.Log(_agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending);
                switch (_patrolMode)
                {
                    case PatrolType.PINGPONG:
                        PingPong();
                        break;
                    case PatrolType.CLOCKWISE:
                        ClockWise();
                        break;
                    case PatrolType.COUNTERCLOCK:
                        CounterClockwise();
                        break;
                    default:
                        break;

                        //_destinationWaypointID++;

                        //_agent.SetDestination(_waypoints[_destinationWaypointID].position);

                        //if (_destinationWaypointID >= _waypoints.Length -1)
                        //{
                        //    _destinationWaypointID = 0;
                        //}
                }
            }
        }

    }
    private void OnDrawGizmos()
    {
        if (_waypoints == null || _waypoints.Length == 0)
        {
            return;
        }
        Gizmos.color = _gizmosColor;
        for (int i = 0; i < _waypoints.Length; i++)
        {
            if (i ==0)
            {
                Gizmos.DrawSphere(_waypoints[i].position, 1f);
            }
            else if (i == _waypoints.Length - 1)//Si je suis au dernier élément
            {
                Gizmos.DrawSphere(_waypoints[i].position, 1f);

                if (_patrolMode != PatrolType.PINGPONG)
                {
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[0].position);

                }
            }
            else
            {
                Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
            }
        }
    }

    private void CounterClockwise()
    {
        _destinationWaypointID++;

        if (_destinationWaypointID >= _waypoints.Length - 1)
        {
            _destinationWaypointID = 0;
        }

        _agent.SetDestination(_waypoints[_destinationWaypointID].position);
    }

    private void PingPong()
    {
        if (_startToEnd)
        {
            _destinationWaypointID++;
        }
        else
        {
            _destinationWaypointID--;
        }

        if (_destinationWaypointID > _waypoints.Length - 1)
        {
            _startToEnd = false;
            _destinationWaypointID = _waypoints.Length - 1;
        }
        else if (_destinationWaypointID < 0)
        {
            _startToEnd = true;
            _destinationWaypointID = 0;

        }
        
        _agent.SetDestination(_waypoints[_destinationWaypointID].position);

    }

    private void ClockWise()
    {
        _destinationWaypointID--;

        if (_destinationWaypointID < 0)
        {
            _destinationWaypointID = _waypoints.Length - 1;
        }

        Debug.Log("Changement de déstination");
        _agent.SetDestination(_waypoints[_destinationWaypointID].position);
    }

    //private void Chasing()
    //{
    //    Debug.Log(_coneVision.m_target.transform.position);
    //    _agent.SetDestination(_coneVision.m_target.transform.position);
    //}

    private NavMeshAgent _agent;

    private ConeVision _coneVision;

    private int _destinationWaypointID = 0;

    private bool _startToEnd = true;

    private bool _isChasing = false;
}
