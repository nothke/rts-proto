using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager instance;
    void Awake() { instance = this; }

    class Bullet
    {
        public Vector3 from;
        public Vector3 to;
        public float distFactor;
        public float time;
        public Transform t;
    }

    public GameObject bulletPrefab;

    Queue<Bullet> inactiveBullets = new Queue<Bullet>();
    List<Bullet> activeBullets = new List<Bullet>();

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            var bulletT = Instantiate(bulletPrefab).transform;
            bulletT.gameObject.SetActive(false);
            inactiveBullets.Enqueue(new Bullet() { t = bulletT });
        }
    }

    public static void Shoot(Vector3 from, Vector3 to)
    {
        instance.ShootLocal(from, to);
    }

    void ShootLocal(Vector3 from, Vector3 to)
    {
        if (inactiveBullets.Count > 0)
        {
            var bullet = inactiveBullets.Dequeue();
            bullet.from = from;
            bullet.to = to;
            bullet.distFactor = 1.0f / (to - from).magnitude;
            bullet.time = 0;
            activeBullets.Add(bullet);

            bullet.t.gameObject.SetActive(true);
            bullet.t.position = from;
            bullet.t.rotation = Quaternion.LookRotation((to - from).normalized);
        }
    }

    void Update()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            var bullet = activeBullets[i];
            bullet.time += Time.deltaTime * 40.0f * bullet.distFactor;

            bullet.t.position = Vector3.Lerp(bullet.from, bullet.to, bullet.time);

            if (bullet.time > 1.0f)
            {
                bullet.t.gameObject.SetActive(false);
                activeBullets.Remove(bullet);
                inactiveBullets.Enqueue(bullet);
            }
        }
    }
}
