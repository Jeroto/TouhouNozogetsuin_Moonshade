using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{

    Transform thisTrans;
    [SerializeField] Transform mainCam;

    [SerializeField] int frameDelay = 1;
    int framesWaited;

    private void Awake()
    {
        thisTrans = transform;
        if (mainCam == null)
            mainCam = Camera.main.transform;
    }
    
    void Update()
    {
        if (frameDelay > framesWaited)
            framesWaited++;
        else
        {
            thisTrans.LookAt(mainCam);
            thisTrans.forward = new Vector3(thisTrans.forward.x, 0, thisTrans.forward.z);
            framesWaited = 0;
        }
    }
}
