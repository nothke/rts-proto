using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UnitsDatabase", fileName = "UnitsDatabase")]
public class UnitsDatabase : ScriptableObject
{
    public Building[] buildings;
}