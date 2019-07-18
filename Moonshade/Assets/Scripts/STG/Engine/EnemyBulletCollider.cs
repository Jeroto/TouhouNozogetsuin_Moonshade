using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletCollider : MonoBehaviour
{
    public Transform colliderTransform;

    public static LinkedList<EnemyBulletCollider> colliders = new LinkedList<EnemyBulletCollider>();

    public float hitboxScale;
    public Vector2 hitboxOffset;


    void Awake()
    {
        colliderTransform = transform;
    }

    private void OnEnable()
    {
        colliders.AddLast(this);
    }

    private void OnDestroy()
    {
        colliders.Remove(this);
    }

    private void OnDisable()
    {
        colliders.Remove(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3)hitboxOffset, hitboxScale);
    }
}
