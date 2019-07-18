using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnToCameraOrientation : MonoBehaviour
{
    Transform thisTrans;
    [SerializeField] Transform mainCam;

    private void Awake()
    {
        thisTrans = transform;
        if(mainCam == null)
            mainCam = Camera.main.transform;
    }
    
    void Update()
    {
        thisTrans.eulerAngles = new Vector3(mainCam.eulerAngles.x, 0, 0);
    }
}
