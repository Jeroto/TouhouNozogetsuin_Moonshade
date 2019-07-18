using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBattleSpawn : MonoBehaviour
{
    Transform thisTrans;
    GameMasterScript gameMaster;

    public int movePause;
    public int movementFrames;
    public int chasingFrames;
    int currentWaitFrames;
    int framesToWait;

    [SerializeField] bool chasing;

    Vector2 newPosition;
    Vector2 prevPosition;
    [SerializeField] LayerMask walls;

    private void Awake()
    {
        thisTrans = transform;
        prevPosition = thisTrans.position;
        newPosition = prevPosition;
    }

    void Start()
    {
        gameMaster = GameMasterScript.gameMaster;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentWaitFrames <= 0)
        {
            if (movePause <= 0)
            {
                chasing = Vector2.Distance(thisTrans.position, gameMaster.playerTrans.position) < 128;

                prevPosition = thisTrans.position;

                Vector2 directionInputs;

                if (chasing)
                {
                    directionInputs = gameMaster.playerTrans.position - thisTrans.position;
                    if (Mathf.Abs(directionInputs.x) > Mathf.Abs(directionInputs.y))
                        directionInputs = new Vector2(Mathf.Sign(directionInputs.x), 0);
                    else
                        directionInputs = new Vector2(0, Mathf.Sign(directionInputs.y));
                }
                else
                {
                    if (Random.Range(0, 2) == 0)
                        directionInputs = new Vector2(Random.Range(-1, 2), 0);
                    else
                        directionInputs = new Vector2(0, Random.Range(-1, 2));
                }

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

                if (Vector2.Distance(prevPosition, newPosition) > 1)
                {
                    if (DetectWall())
                    {
                        newPosition = prevPosition;
                        framesToWait = movementFrames;
                    }
                    else
                    {
                        if (chasing)
                        {
                            framesToWait = chasingFrames + Random.Range(-3, 3);
                            //movePause = Random.Range(0, 3);
                            movePause = 10 + Random.Range(-2, 11);
                        }
                        else
                        {
                            framesToWait = movementFrames ;
                            movePause = 10 + Random.Range(-2, 11);
                        }
                        currentWaitFrames = framesToWait;
                    }
                }
            }
            else
                movePause--;
        }
        else
        {
            currentWaitFrames--;
            thisTrans.position = Vector2.Lerp(newPosition, prevPosition, (float)currentWaitFrames / framesToWait);
        }

        if(!gameMaster.loading && Vector2.Distance(thisTrans.position, gameMaster.playerTrans.position) < 5)
        {
            gameMaster.loading = true;
            gameMaster.previousSceneNum = SceneManager.GetActiveScene().buildIndex;
            gameMaster.overworldPos = gameMaster.playerTrans.GetComponent<OverworldCharacter>().newPosition;
            SceneManager.LoadSceneAsync(1);
        }
    }

    bool DetectWall()
    {
        return Physics2D.Raycast(thisTrans.position, newPosition - prevPosition, 32, walls);
    }
}
