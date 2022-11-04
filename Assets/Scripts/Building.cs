using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(1, 1);

    Constructable _constructable;
    public Constructable constructable { get { if (!_constructable) _constructable = GetComponent<Constructable>(); return _constructable; } }
}
