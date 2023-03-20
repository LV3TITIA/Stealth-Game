using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDetector : MonoBehaviour
{
    [SerializeField]
    private Transform[] _rayOrigins;

    [SerializeField]
    private float _rayLength = 1.5f;

    [SerializeField]
    private LayerMask _groundMask;


    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        foreach(Transform o in _rayOrigins) 
        {
            Gizmos.DrawRay(o.position, Vector3.down * _rayLength);
        }
    }

    public Vector3 AverageHeight()
    {
        int hitCount = 0;
        Vector3 combinedPosition = Vector3.zero;

        RaycastHit hit;

        foreach (Transform o in _rayOrigins) 
        {
            if (Physics.Raycast(o.position, Vector3.down, out hit, _rayLength, _groundMask))
            {
                hitCount++;
                //hit.point : La position dans le World ou le Raycast à touché le collider

                combinedPosition += hit.point;
            
            }
        }

        Vector3 averagePosition = Vector3.zero;
        
        if(hitCount > 0) 
        {
            averagePosition = combinedPosition / hitCount;
        }

       return averagePosition;
    }
}

