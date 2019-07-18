using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldCharacter : MonoBehaviour
{
    Transform thisTrans;
    GameMasterScript gameMaster;
    InputScript input;
    StepSoundEffect stepSounds;

    public int movementFrames;
    public int runningFrames;
    int currentWaitFrames;
    int framesToWait;

    bool running;

    public Vector2 newPosition;
    Vector2 prevPosition;
    public Vector2 directionInputs;

    public bool lockUp;
    public bool lockDown;
    public bool lockLeft;
    public bool lockRight;

    [SerializeField] LayerMask walls;

    private void Awake()
    {
        thisTrans = transform;
        stepSounds = thisTrans.GetChild(1).GetComponent<StepSoundEffect>();
        prevPosition = thisTrans.position;
        newPosition = prevPosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameMaster = GameMasterScript.gameMaster;
        input = GameMasterScript.inputScript;
    }

    // Update is called once per frame
    void Update()
    {
        if (input.pauseDown)
            gameMaster.pause = !gameMaster.pause;

        bool anyDirectionLock = lockUp || lockDown || lockRight || lockLeft;

        if (input.directionalInput.x > 0 && !(input.directionalInput.y != 0 && lockRight))
        {
            if (!anyDirectionLock)
                lockRight = true;
            directionInputs = Vector2.right;
        }
        else if (input.directionalInput.x < 0 && !(input.directionalInput.y != 0 && lockLeft))
        {
            if (!anyDirectionLock)
                lockLeft = true;
            directionInputs = Vector2.left;
        }
        else if (input.directionalInput.y > 0 && !(input.directionalInput.x != 0 && lockUp))
        {
            if (!anyDirectionLock)
                lockUp = true;
            directionInputs = Vector2.up;
        }
        else if (input.directionalInput.y < 0 && !(input.directionalInput.x != 0 && lockDown))
        {
            if (!anyDirectionLock)
                lockDown = true;
            directionInputs = Vector2.down;
        }

        if (input.directionalInput.x < 0.01)
            lockRight = false;
        if (input.directionalInput.x > -0.01)
            lockLeft = false;
        if (input.directionalInput.y < 0.01)
            lockUp = false;
        if (input.directionalInput.y > -0.01)
            lockDown = false;

        if (input.directionUp.x != 0)
            directionInputs.x = 0;
        if (input.directionUp.y != 0)
            directionInputs.y = 0;

        if (currentWaitFrames <= 0)
        {
            running = input.focusPressed;

            prevPosition = thisTrans.position;

            if (directionInputs.y > 0.1f)
            {
                newPosition = prevPosition + Vector2.up * 32;
            }
            if (directionInputs.y < -0.1f)
            {
                newPosition = prevPosition + Vector2.down * 32;
            }
            if (directionInputs.x > 0.1f)
            {
                newPosition = prevPosition + Vector2.right * 32;
            }
            if (directionInputs.x < -0.1f)
            {
                newPosition = prevPosition + Vector2.left * 32;
            }
            
            if(Vector2.Distance(prevPosition, newPosition) > 1)
            {
                if(DetectWall())
                {
                    newPosition = prevPosition;
                    framesToWait = movementFrames;
                }
                else
                {
                    stepSounds.PlayStep(StepSoundEffect.TileType.Grass);
                    if (running)
                        framesToWait = runningFrames;
                    else
                        framesToWait = movementFrames;
                    currentWaitFrames = framesToWait;
                }
            }
        }
        else
        {
            currentWaitFrames--;
            thisTrans.position = Vector2.Lerp(newPosition, prevPosition, (float)currentWaitFrames / framesToWait);
        }
    }

    bool DetectWall()
    {
        return Physics2D.Raycast(thisTrans.position, newPosition - prevPosition, 32, walls);
    }
}
