using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPosition : MonoBehaviour
{
    Transform thisTrans;

    [SerializeField] bool lockXPos;
    [SerializeField] bool lockYPos;
    [SerializeField] bool lockZPos;

    Vector3 startingPos;

    private void Awake()
    {
        thisTrans = transform;
        startingPos = thisTrans.position;
    }

    void Update()
    {
        thisTrans.position = new Vector3((lockXPos) ? startingPos.x : thisTrans.position.x, (lockYPos) ? startingPos.y : thisTrans.position.y, (lockZPos) ? startingPos.z : thisTrans.position.z);
    }
}
