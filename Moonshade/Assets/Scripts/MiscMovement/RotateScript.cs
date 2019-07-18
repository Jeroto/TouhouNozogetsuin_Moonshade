using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour {

    [Range(0, 30)]public float rotatesPerSecond;
    public bool counterclockwise;
    public float timeRemaining = float.MaxValue;

    public Axis axis = Axis.Z;
    
    public enum Axis {X, Y, Z};

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(timeRemaining > 0)
        {
            RotateObject(transform, rotatesPerSecond, counterclockwise, axis);
            timeRemaining -= GameMasterScript.frameTime * GameMasterScript.gameMaster.timeScale;
        }
	}

    public static void RotateObject(Transform _object, float _rotatesPerSecond, bool _counterClockwise)
    {
        _object.Rotate(0, 0, (_rotatesPerSecond * 360 * GameMasterScript.frameTime * GameMasterScript.gameMaster.timeScale) * ((_counterClockwise) ? 1 : -1));
    }

    public static void RotateObject(Transform _object, float _rotatesPerSecond, bool _counterClockwise, Axis _axis)
    {
        switch(_axis)
        {
            case Axis.X:
                _object.Rotate((_rotatesPerSecond * 360 * GameMasterScript.frameTime * GameMasterScript.gameMaster.timeScale) * ((_counterClockwise) ? 1 : -1), 0, 0);
                break;

            case Axis.Y:
                _object.Rotate(0, (_rotatesPerSecond * 360 * GameMasterScript.frameTime * GameMasterScript.gameMaster.timeScale) * ((_counterClockwise) ? 1 : -1), 0);
                break;

            case Axis.Z:
                _object.Rotate(0, 0, (_rotatesPerSecond * 360 * GameMasterScript.frameTime * GameMasterScript.gameMaster.timeScale) * ((_counterClockwise) ? 1 : -1));
                break;
        }
    }
}
