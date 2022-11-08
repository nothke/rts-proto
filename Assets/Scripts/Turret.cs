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

    Entity _ownEntity;
    Entity ownEntity { get { if (!_ownEntity) _ownEntity = GetComponent<Entity>(); return _ownEntity; } }

    public Transform barrelPoint;

    public bool isLaser;

    public GameObject bulletPrefab;

    private void Update()
    {
        if (target)
        {
            Vector3 targetPos = target.position;
            targetPos.y = 0;

            Vector3 pos = transform.position;
            pos.y = 0;

            if (Vector3.Distance(pos, targetPos) > range)
            {
                target = null;
                return;
            }

            rotationPivot.rotation = Quaternion.LookRotation(targetPos - pos, Vector3.up);

            float time = Time.time;

            if (time - timeOfLastFire > rateOfFire)
            {
                //Debug.DrawLine(transform.position, target.position, Color.red, 0.2f);
                timeOfLastFire = time;

                Shoot();
            }


        }
        else
        {
            float closest = Mathf.Infinity;
            Entity closestEntity = null;

            float sqrRange = range * range;

            // Seek target
            foreach (var entity in Entity.all)
            {
                if (entity.faction != ownEntity.faction)
                {
                    float sqrd = Vector3.SqrMagnitude(transform.position - entity.transform.position);

                    if (sqrd < sqrRange &&
                        sqrd < closest)
                    {
                        closest = sqrd;
                        closestEntity = entity;
                    }
                }
            }

            if (closestEntity)
                target = closestEntity.transform;
        }
    }

    void Shoot()
    {
        var health = target.GetComponentInParent<Entity>();

        if (health)
            health.Damage(damage);

        if (isLaser)
            StartCoroutine(FireCo());
        else
        {
            target.GetComponent<Entity>();
            BulletManager.Shoot(barrelPoint.position, target.position + health.GetTargetOffset());
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
