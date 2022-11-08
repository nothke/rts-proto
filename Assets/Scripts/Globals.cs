using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static Globals instance;
    void Awake() { instance = this; }

    public Material tintMaterial;
}
