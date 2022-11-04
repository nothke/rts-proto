using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(1, 1);

    Constructable _constructable;
    public Constructable constructable { get { if (!_constructable) _constructable = GetComponent<Constructable>(); return _constructable; } }

    private void OnDestroy()
    {
        PlayerInput.instance.BuildingGotDestroyed(this);
    }

    private void OnDrawGizmos()
    {
        var matrix = Gizmos.matrix;
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, 0, size.y));

        Gizmos.matrix = matrix;
    }
}
