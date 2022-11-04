using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public static List<Unit> all;

    NavMeshAgent _agent;
    public NavMeshAgent agent { get { if (!_agent) _agent = GetComponent<NavMeshAgent>(); return _agent; } }

    public float timeToBuild = 1.0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reload()
    {
        all = null;
    }

    private void Start()
    {
        agent.SetDestination(transform.position + Vector3.forward);

        if (all == null)
            all = new List<Unit>();

        all.Add(this);
    }

    private void Update()
    {
        Debug.DrawRay(agent.destination, Vector3.up * 0.2f);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }




}
