using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody bulletRigidBody;
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
        Destroy(gameObject);
    }
}
