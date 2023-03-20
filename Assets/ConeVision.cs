using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeVision : MonoBehaviour
{
    [SerializeField]
    private LayerMask _playerLayer;

    [HideInInspector]
    public GameObject m_target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //GameObject.Find("Player").transform.position;
       // Debug.DrawLine(transform.position, GameObject.Find("Player").transform.position, Color.black, 10f, false);

        if (other.CompareTag("Player"))
        {
            //Debug.Log("Joueur détecté par le Trigger");

            Vector3 rayDirection = GameObject.Find("PointDeVision").transform.position - transform.position;

            //Debug.DrawLine(transform.position, GameObject.Find("PointDeVision").transform.position, Color.black, 10f, false);
            if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, Mathf.Infinity, _playerLayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    //Debug.DrawLine(transform.position, rayDirection, Color.black, 10f, false);
                    //Debug.Log("Joueur détecté par le Ray");
                    m_target = other.gameObject;
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_target = null;
        }
    }
}

