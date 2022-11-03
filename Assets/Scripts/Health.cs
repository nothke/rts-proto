using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public static List<Health> all;

    public float hp = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reload()
    {
        all = null;
    }

    private void Start()
    {
        if (all == null)
            all = new List<Health>();

        all.Add(this);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    public void Damage(float by)
    {
        hp -= by;

        if (hp < 0)
            Destroy(gameObject);
    }
}
