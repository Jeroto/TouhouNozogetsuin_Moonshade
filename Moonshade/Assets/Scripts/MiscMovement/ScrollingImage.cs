using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingImage : MonoBehaviour
{
    Transform thisTrans;
    List<Transform> spawned = new List<Transform>();
    [SerializeField] Sprite image;
    [SerializeField] int scrollSpeed;
    [SerializeField] int whenToSpawnNew;
    [SerializeField] int zOffset;
    [SerializeField] int layer;

    void Start()
    {
        thisTrans = transform;
        SpawnNew();
        spawned[spawned.Count - 1].localPosition = new Vector3(0, 0, zOffset);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            spawned[i].transform.localPosition += new Vector3(scrollSpeed, 0, 0);
        }

        if (Mathf.Abs(spawned[spawned.Count - 1].localPosition.x) > whenToSpawnNew)
        {
            SpawnNew();
            spawned[spawned.Count - 1].localPosition = new Vector3(whenToSpawnNew * -Mathf.Sign(scrollSpeed), 0, zOffset);
            if(spawned.Count > 2)
            {
                Destroy(spawned[spawned.Count - 3].gameObject);
                spawned.RemoveAt(spawned.Count - 3);
            }
        }
    }

    void SpawnNew()
    {
        GameObject newSpawn = new GameObject("Sky");
        newSpawn.AddComponent<SpriteRenderer>();
        newSpawn.GetComponent<SpriteRenderer>().sprite = image;
        newSpawn.GetComponent<SpriteRenderer>().flipX = spawned.Count % 2 == 1;
        newSpawn.layer = layer;
        spawned.Add(newSpawn.transform);
        spawned[spawned.Count - 1].parent = thisTrans;
        spawned[spawned.Count - 1].localEulerAngles = Vector3.zero;
        spawned[spawned.Count - 1].localScale = Vector3.one;
    }
}
