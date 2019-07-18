using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    GameMasterScript gameMaster;

    public string directionUpInput;
    public string directionDownInput;
    public string directionLeftInput;
    public string directionRightInput;
    public string shootInput;
    public string bombInput;
    public string focusInput;
    public string pause;

    public string directionUpAxis;
    public string directionDownAxis;
    public string directionLeftAxis;
    public string directionRightAxis;
    public string shootAxis;
    public string bombAxis;
    public string focusAxis;
    public string pauseAxis;

    public bool upIsAxis;
    public bool downIsAxis;
    public bool leftIsAxis;
    public bool rightIsAxis;
    public bool shootIsAxis;
    public bool bombIsAxis;
    public bool focusIsAxis;
    public bool pauseIsAxis;

    public Vector2 directionalInput;
    public Vector2 directionDown;
    public Vector2 directionUp;

    public bool focusPressed;
    public bool focusDown;
    public bool focusUp;

    public bool shootPressed;
    public bool shootDown;
    public bool shootUp;

    public bool bombPressed;
    public bool bombDown;
    public bool bombUp;

    public bool pausePressed;
    public bool pauseDown;
    public bool pauseUp;

    public Vector2 previousDirectionalInput;
    private bool previousFocusPressed;
    private bool previousShootPressed;
    private bool previousBombPressed;
    private bool previousPausePressed;

    public bool joystickPresent = false;
    public bool prevJoystickPresent;
    int joystickCheckTimer;

    void Awake()
    {
        if (GameMasterScript.inputScript == null)
        {
            GameMasterScript.inputScript = this;
            Debug.Log("Input script set.");
        }
    }

    private void Start()
    {
        gameMaster = GameMasterScript.gameMaster;
        AxisCheck();
    }

    void Update()
    {
        JoystickCheck();
        ResetUpDown();

        if (!gameMaster.loading && !gameMaster.lockInputs)
        {
            if (!gameMaster.lockInputs || gameMaster.pause)
            {
                SetPrevious();
                if (!joystickPresent)
                    GetInput();
                else
                    GetJoystickInput();
            }
            if (!joystickPresent)
                GetPause();
            else
                GetJoystickPause();
            GetUpDown();
            GetPauseUpDown();
        }
        else
        {
            ResetInput();
        }
    }

    public void AxisCheck()
    {
        if (directionLeftAxis.Contains("Axis"))
            leftIsAxis = true;

        if (directionRightAxis.Contains("Axis"))
            rightIsAxis = true;

        if (directionDownAxis.Contains("Axis"))
            downIsAxis = true;

        if (directionUpAxis.Contains("Axis"))
            upIsAxis = true;

        if (focusAxis.Contains("Axis"))
            focusIsAxis = true;

        if (shootAxis.Contains("Axis"))
            shootIsAxis = true;

        if (bombAxis.Contains("Axis"))
            bombIsAxis = true;

        if (pauseAxis.Contains("Axis"))
            pauseIsAxis = true;
    }

    public void JoystickCheck()
    {
        prevJoystickPresent = joystickPresent;
        if (joystickCheckTimer == 0)
        {
            joystickPresent = false;
            string[] joysticks = Input.GetJoystickNames();
            for (int i = 0; i < joysticks.Length; i++)
            {
                if (joysticks[i].Length > 0)
                {
                    joystickPresent = true;
                    Debug.Log(joysticks[i] + " is connected.");
                    break;
                }
            }
            joystickCheckTimer = 60;
        }
        else
        {
            joystickCheckTimer--;
        }
    }

    public void GetInputOld()
    {
        previousDirectionalInput = directionalInput;

        directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        directionDown = new Vector2((previousDirectionalInput.x == 0 && directionalInput.x != 0) ? Mathf.Sign(directionalInput.x) : directionDown.x, (previousDirectionalInput.y == 0 && directionalInput.y != 0) ? Mathf.Sign(directionalInput.y) : directionDown.y);
        directionUp = new Vector2((previousDirectionalInput.x != 0 && directionalInput.x == 0) ? Mathf.Sign(previousDirectionalInput.x) : directionUp.x, (previousDirectionalInput.y != 0 && directionalInput.y == 0) ? Mathf.Sign(previousDirectionalInput.y) : directionUp.y);


        previousFocusPressed = focusPressed;

        focusPressed = Input.GetButton("Focus");
        focusDown = (!previousFocusPressed && focusPressed) ? true : focusDown;
        focusUp = (previousFocusPressed && !focusPressed) ? true : (!focusDown) ? focusUp : false;


        previousShootPressed = shootPressed;

        shootPressed = Input.GetButton("Shoot");
        shootDown = (!previousShootPressed && shootPressed) ? true : shootDown;
        shootUp = (previousShootPressed && !shootPressed) ? true : (!shootDown) ? shootUp : false;


        previousBombPressed = bombPressed;

        bombPressed = Input.GetButton("Bomb");
        bombDown = (!previousBombPressed && bombPressed) ? true : bombDown;
        bombUp = (previousBombPressed && !bombPressed) ? true : (!bombDown) ? bombUp : false;
    }


    public void GetPause()
    {
        pausePressed = Input.GetKey(pause);
    }

    public void GetJoystickPause()
    {
        //Check if pause works off an axis
        pausePressed = ((!pauseIsAxis) ? Input.GetKey(pauseAxis) :
            //Check to see if focus works off positive or negative            if positive, find if value is greater than 0
            ((pauseAxis[pauseAxis.Length - 1] == '+') ? Input.GetAxisRaw(pauseAxis.Remove(pauseAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(pauseAxis.Remove(pauseAxis.Length - 1)) < 0)) ? true : false;
    }


    public void GetInput()
    {
        directionalInput = new Vector2((Input.GetKey(directionLeftInput)) ? -1 : (Input.GetKey(directionRightInput)) ? 1 : 0,
            (Input.GetKey(directionDownInput)) ? -1 : (Input.GetKey(directionUpInput)) ? 1 : 0);


        focusPressed = Input.GetKey(focusInput);


        shootPressed = Input.GetKey(shootInput);


        bombPressed = Input.GetKey(bombInput);


        pausePressed = Input.GetKey(pause);
    }

    public void GetJoystickInput()
    {
        //Check if the left direction works off an axis
        directionalInput = new Vector2(((!leftIsAxis) ? Input.GetKey(directionLeftAxis) :
            //Check to see if left direction works off positive or negative            if positive, find if value is greater than 0
            ((directionLeftAxis[directionLeftAxis.Length - 1] == '+') ? Input.GetAxisRaw(directionLeftAxis.Remove(directionLeftAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(directionLeftAxis.Remove(directionLeftAxis.Length - 1)) < 0)) ? -1 :

            //Check if the right direction works off an axis
            ((!rightIsAxis) ? Input.GetKey(directionRightAxis) :
            //Check to see if right direction works off positive or negative            if positive, find if value is greater than 0
            ((directionRightAxis[directionRightAxis.Length - 1] == '+') ? Input.GetAxisRaw(directionRightAxis.Remove(directionRightAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(directionRightAxis.Remove(directionRightAxis.Length - 1)) < 0)) ? 1 : 0,

            //Check if the down direction works off an axis
            ((!downIsAxis) ? Input.GetKey(directionDownAxis) :
            //Check to see if down direction works off positive or negative            if positive, find if value is greater than 0
            ((directionDownAxis[directionDownAxis.Length - 1] == '+') ? Input.GetAxisRaw(directionDownAxis.Remove(directionDownAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(directionDownAxis.Remove(directionDownAxis.Length - 1)) < 0)) ? -1 :

            //Check if the up direction works off an axis
            ((!upIsAxis) ? Input.GetKey(directionUpAxis) :
            //Check to see if up direction works off positive or negative            if positive, find if value is greater than 0
            ((directionUpAxis[directionUpAxis.Length - 1] == '+') ? Input.GetAxisRaw(directionUpAxis.Remove(directionUpAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(directionUpAxis.Remove(directionUpAxis.Length - 1)) < 0)) ? 1 : 0);


        //Check if focus works off an axis
        focusPressed = ((!focusIsAxis) ? Input.GetKey(focusAxis) :
            //Check to see if focus works off positive or negative            if positive, find if value is greater than 0
            ((focusAxis[focusAxis.Length - 1] == '+') ? Input.GetAxisRaw(focusAxis.Remove(focusAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(focusAxis.Remove(focusAxis.Length - 1)) < 0)) ? true : false;


        //Check if shoot works off an axis
        shootPressed = ((!shootIsAxis) ? Input.GetKey(shootAxis) :
            //Check to see if focus works off positive or negative            if positive, find if value is greater than 0
            ((shootAxis[shootAxis.Length - 1] == '+') ? Input.GetAxisRaw(shootAxis.Remove(shootAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(shootAxis.Remove(shootAxis.Length - 1)) < 0)) ? true : false;


        //Check if bomb works off an axis
        bombPressed = ((!bombIsAxis) ? Input.GetKey(bombAxis) :
            //Check to see if focus works off positive or negative            if positive, find if value is greater than 0
            ((bombAxis[bombAxis.Length - 1] == '+') ? Input.GetAxisRaw(bombAxis.Remove(bombAxis.Length - 1)) > 0 :
            //if negative, find if value is less than 0
            Input.GetAxisRaw(bombAxis.Remove(bombAxis.Length - 1)) < 0)) ? true : false;
    }

    public void GetPauseUpDown()
    {
        pauseDown = (!previousPausePressed && pausePressed) ? true : pauseDown;
        pauseUp = (previousPausePressed && !pausePressed) ? true : (!pauseDown) ? pauseUp : false;
    }

    public void GetUpDown()
    {
        directionDown = new Vector2((previousDirectionalInput.x == 0 && directionalInput.x != 0) ? Mathf.Sign(directionalInput.x) : directionDown.x, (previousDirectionalInput.y == 0 && directionalInput.y != 0) ? Mathf.Sign(directionalInput.y) : directionDown.y);
        directionUp = new Vector2((previousDirectionalInput.x != 0 && directionalInput.x == 0) ? Mathf.Sign(previousDirectionalInput.x) : directionUp.x, (previousDirectionalInput.y != 0 && directionalInput.y == 0) ? Mathf.Sign(previousDirectionalInput.y) : directionUp.y);

        focusDown = (!previousFocusPressed && focusPressed) ? true : focusDown;
        focusUp = (previousFocusPressed && !focusPressed) ? true : (!focusDown) ? focusUp : false;

        shootDown = (!previousShootPressed && shootPressed) ? true : shootDown;
        shootUp = (previousShootPressed && !shootPressed) ? true : (!shootDown) ? shootUp : false;

        bombDown = (!previousBombPressed && bombPressed) ? true : bombDown;
        bombUp = (previousBombPressed && !bombPressed) ? true : (!bombDown) ? bombUp : false;
    }

    public void SetPrevious()
    {
        previousDirectionalInput = directionalInput;

        previousFocusPressed = focusPressed;

        previousShootPressed = shootPressed;

        previousBombPressed = bombPressed;

        previousPausePressed = pausePressed;
    }

    public void ResetUpDown()
    {
        directionDown = Vector2.zero;
        directionUp = Vector2.zero;

        focusDown = false;
        focusUp = false;

        shootDown = false;
        shootUp = false;

        bombDown = false;
        bombUp = false;

        pauseUp = false;
        pauseDown = false;
    }

    public void ResetInput()
    {
        previousDirectionalInput = Vector2.zero;

        directionalInput = Vector2.zero;


        previousFocusPressed = false;

        focusPressed = false;


        previousShootPressed = false;

        shootPressed = false;


        previousBombPressed = false;

        bombPressed = false;


        previousPausePressed = false;

        pausePressed = false;
    }
}
