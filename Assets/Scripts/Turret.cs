using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform target;
    public float range = 10;
    public float damage = 0.21f;

    public Transform rotationPivot;

    public float rateOfFire = 1;
    float timeOfLastFire = Mathf.NegativeInfinity;

    LineRenderer _line;
    LineRenderer line { get { if (!_line) _line = GetComponent<LineRenderer>(); return _line; } }

    Health _ownHealth;
    Health ownHealth { get { if (!_ownHealth) _ownHealth = GetComponent<Health>(); return _ownHealth; } }

    public Transform barrelPoint;

    private void Update()
    {
        if (target)
        {
            Vector3 targetPos = target.position;
            targetPos.y = 0;

            Vector3 pos = transform.position;
            pos.y = 0;

            rotationPivot.rotation = Quaternion.LookRotation(targetPos - pos, Vector3.up);

            float time = Time.time;

            if (time - timeOfLastFire > rateOfFire)
            {
                //Debug.DrawLine(transform.position, target.position, Color.red, 0.2f);
                timeOfLastFire = time;

                StartCoroutine(FireCo());

                var health = target.GetComponentInParent<Health>();

                if (health)
                    health.Damage(damage);
            }
        }
        else
        {
            float closest = Mathf.Infinity;
            Health closestHealth = null;

            // Seek target
            foreach (var health in Health.all)
            {
                float d = Vector3.Distance(health.transform.position, transform.position);
                if (d < closest && health != ownHealth && d < range)
                {
                    closest = d;
                    closestHealth = health;
                }
            }

            if (closestHealth)
                target = closestHealth.transform;
        }
    }

    IEnumerator FireCo()
    {
        line.enabled = true;
        line.SetPosition(0, barrelPoint.position);
        line.SetPosition(1, target.position);

        for (int i = 0; i < 10; i++)
            yield return null;

        line.enabled = false;
    }
}
