using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBulletScript : MonoBehaviour
{
    public enum BulletColor { Red, White, Grey, Green, Teal, Blue, Pink, Orange, Yellow };

    public int damage;
    public GameMasterScript.DamageTypes damageType;

    [Space]

    static System.Random bulletRandom;

    public bool ignoreRenderer;

    public int framesBetweenBulletReorientation = 1;
    public int reorientationFramesWaited;

    public bool turnAtStart = true;
    public bool rotationBasedOnParent = false;

    public Vector2 movementDirection;
    public float currentSpeed;
    public float bulletSpeed;
    public bool turnBullet;
    public bool turnAfterWait;
    public bool gradualSpeedChange;
    public float timeToSpeedUp;
    public float timePassed;
    public float startingBulletSpeed;
    public float initialMoveDelay;
    public float spawnDelay = 15f;
    public float spawnDelayCurrent;

    public Sprite bulletSprite;
    public bool updateSprites = true;
    public bool regularKillzone = true;

    public SpriteRenderer spriteRenderer;
    [Space]
    protected Color spriteInitialColor;
    protected Vector2 spriteInitialScale;
    public float spawnEffectStartScale = 2;
    public float spawnEffectEndScale = 1;
    
    public Vector2 prevPosition;

    protected GameMasterScript gameMaster;
    protected ShmupManager shmupMaster;

    protected Transform thisTrans;
    
    protected int enemyBulletKillzoneLayer;

    private void Awake()
    {
        thisTrans = transform;
        gameMaster = GameMasterScript.gameMaster;
        shmupMaster = ShmupManager.shmupManager;

        if (regularKillzone)
        {
            DetectKillzone();
        }

        enemyBulletKillzoneLayer = LayerMask.NameToLayer("EnemyBulletKillZone");
    }

    void Start()
    {
        if (!gradualSpeedChange)
        {
            currentSpeed = bulletSpeed;
        }

        if (!ignoreRenderer)
        {
            spriteRenderer = thisTrans.GetComponentInChildren<SpriteRenderer>();

            spriteInitialScale = spriteRenderer.transform.localScale;
            spriteInitialColor = spriteRenderer.color;

            if (updateSprites)
            {
                spriteRenderer.sprite = bulletSprite;
            }


            spawnEffectStartScale = 3;
            spriteRenderer.color = new Color(1, 1, 1, 0);
            spriteRenderer.transform.localScale *= spawnEffectStartScale;
        }

        prevPosition = thisTrans.position;
    }


    void Update()
    {
        if (initialMoveDelay <= 0)
        {
            if (gradualSpeedChange)
            {
                timePassed += gameMaster.timeScale;

                currentSpeed = Mathf.Lerp(startingBulletSpeed, bulletSpeed, timePassed / timeToSpeedUp);
            }

            thisTrans.Translate(movementDirection * currentSpeed * gameMaster.timeScale);
        }
        else
        {
            initialMoveDelay -= gameMaster.timeScale;
        }

        if (spawnDelayCurrent < spawnDelay)
        {
            if (!ignoreRenderer)
            {
                spawnDelayCurrent += gameMaster.timeScale;

                spriteRenderer.transform.localScale = spriteInitialScale * Mathf.Lerp(spawnEffectStartScale, 1, spawnDelayCurrent / spawnDelay);
                spriteRenderer.color = Color.Lerp(new Color(spriteInitialColor.r, spriteInitialColor.g, spriteInitialColor.b, 0), spriteInitialColor, spawnDelayCurrent / (spawnDelay * 2.5f));

                if (spawnDelayCurrent >= spawnDelay)
                {
                    spriteRenderer.color = spriteInitialColor;
                    spriteRenderer.transform.localScale = spriteInitialScale;
                }
            }
        }

        AdditionalFunctions();

        if(reorientationFramesWaited >= framesBetweenBulletReorientation)
        {
            if(turnBullet)
            {
                if(!turnAfterWait || initialMoveDelay <= 0)
                {
                    if(Vector2.Distance(thisTrans.position, prevPosition) > 0.001f)
                    {
                        //thisTrans.GetChild(0).up = new Vector3(thisTrans.position.x - prevPosition.x, thisTrans.position.y - prevPosition.y, 0).normalized;
                        thisTrans.GetChild(0).eulerAngles = new Vector3(0, 0, MathFunctions.FindAngle(new Vector2(thisTrans.position.x - prevPosition.x, thisTrans.position.y - prevPosition.y)));
                    }
                    reorientationFramesWaited = 0;
                    prevPosition = thisTrans.position;
                }
            }
        }
        else
        {
            reorientationFramesWaited++;
        }

        if (regularKillzone)
        {
            DetectKillzone();
        }
    }

    public void DetectKillzone()
    {
        Vector2 localPosition = thisTrans.position - shmupMaster.bulletContainer.position;
        if (localPosition.x < shmupMaster.bulletDestroyX.x || localPosition.x > shmupMaster.bulletDestroyX.y || localPosition.y < shmupMaster.bulletDestroyY.x || localPosition.y > shmupMaster.bulletDestroyY.y)
        {
            //Destroy(gameObject);
            gameObject.SetActive(false);
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(thisTrans.position, new Vector3(0, 0, 1), 5, shmupMaster.bulletDestroyLayer);

            if (hit)
            {
                gameObject.SetActive(false);
            }
        }
    }


    protected virtual void AdditionalFunctions()
    {
        //To be overwritten
    }
}
