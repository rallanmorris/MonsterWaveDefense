using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody bulletRigidBody;
    [SerializeField] private Transform vfxHitEnemy;
    [SerializeField] private Transform vfxHitOther;
    [SerializeField] float speed = 5f;
    [SerializeField] int damage;
    private EnemyAI enemy;

    private void Awake()
    {
        bulletRigidBody = GetComponent<Rigidbody>();

    }

    private void Start()
    {
        bulletRigidBody.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BulletTarget>() != null)
        {
            //Hit
            enemy = other.gameObject.GetComponent<EnemyAI>();
            var hitEffect = Instantiate(vfxHitEnemy, transform.position, Quaternion.identity);
            hitEffect.transform.parent = enemy.transform;
            Destroy(hitEffect.gameObject, 0.2f);
            
            if (enemy != null)
                enemy.TakeDamage(damage);
        }
        else
        {
            //Hit something else
            var missEffect = Instantiate(vfxHitOther, transform.position, Quaternion.identity);
            Destroy(missEffect.gameObject, 0.5f);
        }
        Destroy(gameObject);
    }
}
