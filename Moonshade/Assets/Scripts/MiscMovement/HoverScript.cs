using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverScript : MonoBehaviour
{
    public bool active;

    public float distance;
    public int frames;
    public int framesPassed;

    Vector2 localPos;
    Transform thisTrans;

    bool up = true;

    bool prevActive;

    private void Awake()
    {
        thisTrans = transform;
        localPos = thisTrans.localPosition;
        framesPassed = Random.Range(0, frames);
    }
    
    void Update()
    {
        if(active)
        {
            framesPassed++;
            thisTrans.localPosition = thisTrans.up * Mathf.Lerp((up) ? -distance : distance,  (up) ? distance : -distance, (float)framesPassed / frames);
            if (framesPassed >= frames)
            {
                framesPassed = Random.Range(-2, 3);
                up = !up;
            }
            prevActive = true;
        }
        else if(prevActive)
        {
            prevActive = false;
            thisTrans.localPosition = localPos;
        }
    }
}
