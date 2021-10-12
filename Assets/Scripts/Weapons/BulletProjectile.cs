using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody bulletRigidBody;
    [SerializeField] private Transform vfxHitEnemy;
    [SerializeField] private Transform vfxHitOther;
    [SerializeField] float speed = 5f;

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
        if(other.GetComponent<BulletTarget>() != null)
        {
            //Hit
            Instantiate(vfxHitEnemy, transform.position, Quaternion.identity);
        }
        else
        {
            //Hit something else
            Instantiate(vfxHitOther, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
