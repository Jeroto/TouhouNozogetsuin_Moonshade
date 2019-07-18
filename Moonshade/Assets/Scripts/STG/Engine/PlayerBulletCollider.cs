using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletCollider : MonoBehaviour
{
    public Transform colliderTransform;

    public static LinkedList<PlayerBulletCollider> colliders = new LinkedList<PlayerBulletCollider>();

    public Vector2 hitboxScale;
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
        Gizmos.DrawWireCube(transform.position + (Vector3)hitboxOffset, hitboxScale * 2);
    }
}
