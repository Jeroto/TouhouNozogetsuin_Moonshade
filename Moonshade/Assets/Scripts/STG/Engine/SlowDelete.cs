using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDelete : MonoBehaviour {

    [SerializeField] int frameDelay = 0;
    int framesPassed;

    Transform thisTrans;

    private void Awake()
    {
        thisTrans = transform;   
    }
    
    void Update () {
        framesPassed++;
        if(framesPassed >= frameDelay)
        {
            GameObject child = null;
            for (int i = 0; i < thisTrans.childCount; i++)
            {
                child = thisTrans.GetChild(i).gameObject;
                if(!child.activeSelf)
                {
                    Destroy(child);
                    break;
                }
            }

            framesPassed = 0;
        }
	}
}
