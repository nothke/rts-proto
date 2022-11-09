using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public static List<Entity> all;

    public Faction faction;
    public float hp = 1;
    public float targetOffset;
    public float addedSelectionRadius = 0;

    static List<Renderer> tintRenderers;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reload()
    {
        all = null;
        tintRenderers = null;
    }

    private void Start()
    {
        if (all == null)
            all = new List<Entity>();

        all.Add(this);

        if (tintRenderers == null)
            tintRenderers = new List<Renderer>();

        tintRenderers.Clear();
        GetComponentsInChildren(tintRenderers);

        foreach (var renderer in tintRenderers)
        {
            if (renderer.sharedMaterial == Globals.instance.tintMaterial)
            {
                Debug.Assert(renderer);
                Debug.Assert(faction, "Faction is null");
                Debug.Assert(faction.factionMaterial);

                renderer.material = faction.factionMaterial;
            }
        }
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
