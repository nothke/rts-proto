using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NDraw;

public class Unit : MonoBehaviour
{
    public static List<Unit> all;

    NavMeshAgent _agent;
    public NavMeshAgent agent { get { if (!_agent) _agent = GetComponent<NavMeshAgent>(); return _agent; } }

    Constructable _constructable;
    public Constructable constructable { get { if (!_constructable) _constructable = GetComponent<Constructable>(); return _constructable; } }

    Entity _entity;
    public Entity entity { get { if (!_entity) _entity = GetComponent<Entity>(); return _entity; } }

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
        Vector3 off = Vector3.up * 0.2f;
        //Debug.DrawRay(agent.destination, Vector3.up * 0.2f);
        Draw.World.Line(transform.position + off, agent.destination + off);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }




}
