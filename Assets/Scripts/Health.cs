using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public static List<Health> all;

    public float hp = 1;
    public float targetOffset;

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

    public Vector3 GetTargetOffset()
    {
        return new Vector3(0, targetOffset, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + GetTargetOffset(), Vector3.one * 0.1f);
    }
}
