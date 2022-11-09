using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDraw;

public class UnitProducer : MonoBehaviour
{
    public Unit[] canProduce;
    public Vector3 rallyPoint;

    public Vector3 GetRallyPoint() => rallyPoint == Vector3.zero ? transform.position : rallyPoint;

    private void Update()
    {
        Draw.World.Line(transform.position, GetRallyPoint());
    }
}
