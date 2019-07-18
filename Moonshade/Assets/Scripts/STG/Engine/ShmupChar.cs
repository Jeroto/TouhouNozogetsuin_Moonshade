using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShmupChar : MonoBehaviour
{
    public int partyChar;
    public GameMasterScript.DamageTypes damageType;

    public GameObject lightningAreaSpawn;
    public GameObject percentTextSpawn;
    Transform lightningArea;
    TMPro.TextMeshPro percentText;
    float captureCharge;
    bool chargingCapPercent;
    bool takingCapture;
    public float captureRange;

    [Space]

    public float damageMult;
    public float defMult;
    public float mentalDefMult;

    static System.Random playerRandom;
    
    private SpriteRenderer m_SpriteRenderer;

    public SpriteRenderer hitboxSpriteRenderer;

    public float hitboxSize = 1;
    public float moveSpeed;
    public float focusSpeed;

    public int dizzyDelay;
    int dizzyInputLength;
    Vector2 dizzyInput;

    public float playerScale;
    bool focusing;

    [Space]
    public int damageEffect;
    public int defEffect;
    public int mentalDefEffect;
    public int sizeEffect;
    public int dizzyEffect;
    public int speedEffect;
    public bool focusLocked;
    public bool unfocusLocked;
    [Space]

    public Vector2 movementVelocity;
    private InputScript inputScript;
    private GameMasterScript gameMaster;
    private ShmupManager shmupManager;
    public bool hitByBullet;
    public bool dead;
    private CircleCollider2D playerHitbox;
    private BoxCollider2D spriteHitbox;
    private Animator m_Anim;

    private Transform extraBitsContainer;
    private Transform[] extraBits;
    private Animator[] extraBitAnims;
    private SpriteRenderer[] extraBitRenderers;

    [SerializeField] private float deadTimeMax;
    private float deadTimeCurrent;

    public float invincibilityTime;
    int pauseCooldown;

    public float bombTimeRemaining;

    public float bombWaitTime;
    public float mainFireWait;
    public float subFireWait;
    
    private float borderOfLife;

    public int burstsToFire;
    public int burstsFired;

    public int mainFired;
    public int subFired;

    public int mainFocusFired;
    public int subFocusFired;

    public bool shooting;

    public ShotTypeData shotType;

    public AudioClip mainShootSound;
    public AudioClip subShootSound;

    Transform thisTrans;

    private void Awake()
    {
        thisTrans = transform;

        gameMaster = GameMasterScript.gameMaster;
        shmupManager = ShmupManager.shmupManager;
        inputScript = GameMasterScript.inputScript;

        playerRandom = new System.Random();

        m_Anim = thisTrans.GetComponentInChildren<Animator>();
        hitboxSpriteRenderer = thisTrans.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
    }


    void Start()
    {
        if (shmupManager.captureMode)
        {
            lightningArea = Instantiate(lightningAreaSpawn, thisTrans.parent).transform;
            percentText = Instantiate(percentTextSpawn, thisTrans).GetComponent<TMPro.TextMeshPro>();
            percentText.text = "000%";
        }
        //SetCharacter(gameMaster.playerParty[0].shmupData, gameMaster.playerParty[0].shotTypeEquip);
    }


    void Update()
    {

        if (shmupManager.endSTG)
            if (lightningArea != null)
                Destroy(lightningArea.gameObject);

        focusing = ((inputScript.focusPressed && bombTimeRemaining <= 0 && !unfocusLocked) || focusLocked);

        if (focusing)
        {
            hitboxSpriteRenderer.color = new Color(hitboxSpriteRenderer.color.r, hitboxSpriteRenderer.color.g, hitboxSpriteRenderer.color.b, Mathf.Clamp01(hitboxSpriteRenderer.color.a + (GameMasterScript.frameTime * gameMaster.timeScale * 3)));
        }
        else
        {
            hitboxSpriteRenderer.color = new Color(hitboxSpriteRenderer.color.r, hitboxSpriteRenderer.color.g, hitboxSpriteRenderer.color.b, Mathf.Clamp01(hitboxSpriteRenderer.color.a - (GameMasterScript.frameTime * gameMaster.timeScale * 3)));
        }

        if (deadTimeCurrent > 0)
        {
            deadTimeCurrent -= gameMaster.timeScale;
            hitboxSpriteRenderer.color = new Color(hitboxSpriteRenderer.color.r, hitboxSpriteRenderer.color.g, hitboxSpriteRenderer.color.b, Mathf.Clamp01(hitboxSpriteRenderer.color.a - (0.075f * gameMaster.timeScale)));

            if (borderOfLife > 0)
            {
                borderOfLife--;
            }


            if (borderOfLife <= 0 && !dead)
            {
                //Death();
                shmupManager.spellCap = false;
                dead = true;
            }
        }
        else if (hitByBullet)
        {
            m_Anim.SetBool("Dead", false);
            hitByBullet = false;
            dead = false;
            deadTimeCurrent = 0;
            invincibilityTime = 3.5f;
            thisTrans.position = shmupManager.spawnPos;
        }

        Vector2 inputSigns = new Vector2((inputScript.directionalInput.x != 0) ? Mathf.Sign(inputScript.directionalInput.x) : 0, (inputScript.directionalInput.y != 0) ? Mathf.Sign(inputScript.directionalInput.y) : 0);

        if (!takingCapture)
        {
            if (dizzyEffect > 0)
            {
                if (dizzyDelay == 0)
                {
                    dizzyInput = new Vector2(playerRandom.Next(-1, 2), playerRandom.Next(-1, 2));
                }
                else if (dizzyDelay > -dizzyInputLength && dizzyDelay < 0)
                {
                    inputSigns = dizzyInput;
                    inputScript.directionalInput = dizzyInput;
                }
                else if (dizzyDelay < -dizzyInputLength)
                {
                    dizzyInputLength = playerRandom.Next(2, 6);
                    dizzyDelay = playerRandom.Next(240 / (dizzyEffect * 2), 240 / dizzyEffect);
                }

                dizzyDelay--;
            }


            float currentMoveSpeed = (focusing) ? (chargingCapPercent) ? focusSpeed * 0.5f : focusSpeed : moveSpeed;
            currentMoveSpeed *= 1 + (speedEffect * 0.1f);

            movementVelocity = new Vector2((inputScript.directionalInput.x != 0 && !hitByBullet) ? currentMoveSpeed * inputSigns.normalized.x : 0,
                (inputScript.directionalInput.y != 0 && !hitByBullet) ? currentMoveSpeed * inputSigns.normalized.y : 0);

            movementVelocity *= gameMaster.timeScale;
        }
        else
            movementVelocity = Vector2.zero;


        //RaycastCollisions();

        //thisTrans.Translate(movementVelocity);
        thisTrans.localPosition = new Vector3(Mathf.Clamp(thisTrans.localPosition.x + movementVelocity.x, shmupManager.playerXExtents.x, shmupManager.playerXExtents.y),
            Mathf.Clamp(thisTrans.localPosition.y + movementVelocity.y, shmupManager.playerYExtents.x, shmupManager.playerYExtents.y), thisTrans.localPosition.z);

        if(!shmupManager.endSTG)
            BulletCollisions();

        if (!shmupManager.captureMode)
        {
            shmupManager.UpdateGuages(gameMaster.playerParty[partyChar].maxHP, gameMaster.playerParty[partyChar].currentHP, gameMaster.playerParty[partyChar].maxMP,
                gameMaster.playerParty[partyChar].currentMP);
            Shoot();
        }
        else
            Capture();
    }

    private void LateUpdate()
    {
        UpdateManager();
    }

    void UpdateManager()
    {
        if(shmupManager.inSTG)
        {
            if (gameMaster.playerParty[partyChar].currentHP <= 0)
            {
                gameMaster.playerParty[partyChar].currentHP = 0;
                shmupManager.endSTG = true;
            }
        }
    }

    public void SetCharacter(ShmupPlayerData charData, ShotTypeData shotTypeData, int partyIndex, GameMasterScript.DamageTypes setDamageType, Character.StatusEffect[] effects)
    {
        partyChar = partyIndex;
        damageType = setDamageType;
        hitboxSize = charData.hitboxSize;
        hitboxSpriteRenderer.transform.localScale = new Vector3(0.25f, 0.25f, 1) * hitboxSize;
        borderOfLife = charData.borderOfLife;
        moveSpeed = charData.moveSpeed;
        focusSpeed = charData.focusSpeed;
        m_Anim.runtimeAnimatorController = charData.animations;
        extraBits = new Transform[charData.extraBits.Length];
        extraBitAnims = new Animator[extraBits.Length];
        extraBitRenderers = new SpriteRenderer[extraBits.Length];
        for (int i = 0; i < charData.extraBits.Length; i++)
        {
            extraBits[i] = Instantiate(charData.extraBits[i], thisTrans.position, new Quaternion(), extraBitsContainer).transform;
            extraBitAnims[i] = extraBits[i].GetComponent<Animator>();
            extraBitRenderers[i] = extraBits[i].GetComponent<SpriteRenderer>();
        }

        burstsFired = 0;
        mainFired = 0;
        subFired = 0;
        mainFocusFired = 0;
        subFocusFired = 0;
        mainFireWait = 0;
        subFireWait = 0;
        bombWaitTime = 0;
        bombTimeRemaining = 0;

        shotType = shotTypeData;

        int damageStrength = 0;
        int defStrength = 0;
        int mentalDefStrength = 0;
        int sizeStrength = 0;
        int dizzyStrenth = 0;
        int speedStrength= 0;

        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].effectType == Character.StatusEffectType.UnfocusLock)
                unfocusLocked = true;
            else if (effects[i].effectType == Character.StatusEffectType.FocusLock)
                focusLocked = true;
            else if (effects[i].effectType == Character.StatusEffectType.AttackUp || effects[i].effectType == Character.StatusEffectType.Weak)
            {
                if (Mathf.Abs(damageStrength) < Mathf.Abs(effects[i].strength))
                    damageStrength = effects[i].strength;
            }
            else if (effects[i].effectType == Character.StatusEffectType.DefenseUp || effects[i].effectType == Character.StatusEffectType.Frail)
            {
                if (Mathf.Abs(defStrength) < Mathf.Abs(effects[i].strength))
                    defStrength = effects[i].strength;
            }
            else if (effects[i].effectType == Character.StatusEffectType.MentalDefUp)
            {
                if (Mathf.Abs(mentalDefStrength) < Mathf.Abs(effects[i].strength))
                    mentalDefStrength = effects[i].strength;
            }
            else if (effects[i].effectType == Character.StatusEffectType.Graze || effects[i].effectType == Character.StatusEffectType.GrowBigger)
            {
                if (Mathf.Abs(sizeStrength) < Mathf.Abs(effects[i].strength))
                    sizeStrength = effects[i].strength;
            }
            else if (effects[i].effectType == Character.StatusEffectType.Dizzy)
            {
                if (Mathf.Abs(dizzyStrenth) < Mathf.Abs(effects[i].strength))
                    dizzyStrenth = effects[i].strength;
            }
            else if (effects[i].effectType == Character.StatusEffectType.MoveSpeedUp || effects[i].effectType == Character.StatusEffectType.MoveSpeedSlow)
            {
                if (Mathf.Abs(speedStrength) < Mathf.Abs(effects[i].strength))
                    speedStrength = effects[i].strength;
            }
        }

        damageEffect = damageStrength;
        defEffect = defStrength;
        mentalDefEffect = mentalDefStrength;
        sizeEffect = sizeStrength;
        dizzyEffect = dizzyStrenth;
        speedEffect = speedStrength;

        damageMult = 1 + (damageEffect * 0.1f);
        defMult = 1 + (defEffect * 0.1f);
        mentalDefMult = 1 + (mentalDefEffect * 0.1f);

        playerScale = 1f + ((sizeEffect > 0) ? sizeEffect * 0.1f : sizeEffect * .2f);
        hitboxSize *= playerScale;
        thisTrans.GetChild(0).localScale *= playerScale;
    }

    public void BulletCollisions()
    {
        if (!hitByBullet)
        {
            if (invincibilityTime <= 0)
            {
                LinkedListNode<EnemyBulletCollider> node = EnemyBulletCollider.colliders.First;
                EnemyBulletCollider colliderScript = null;
                float colliderSize;
                float distance;
                Vector2 lossyScale;
                for (int i = 0; i < EnemyBulletCollider.colliders.Count; i++)
                {
                    if (node == null)
                    {
                        continue;
                    }
                    else
                    {
                        colliderScript = node.Value;
                        if (colliderScript.gameObject.activeInHierarchy)
                        {

                            //colliderSize = colliderScript.hitboxScale * ((Vector2)colliderScript.colliderTransform.lossyScale).magnitude;
                            lossyScale = colliderScript.transform.lossyScale;
                            colliderSize = colliderScript.hitboxScale * (Mathf.Lerp(lossyScale.y, lossyScale.x, Mathf.Abs(MathFunctions.CalculateCircle(MathFunctions.FindAngleToSTGPlayer(colliderScript.transform.position) + (colliderScript.colliderTransform.eulerAngles.z)).x)));
                            distance = Vector2.Distance(colliderScript.colliderTransform.position, thisTrans.position) - colliderSize;
                            if (distance < Mathf.Abs(/*colliderSize + */(100 * hitboxSpriteRenderer.transform.lossyScale.x)))
                            {
                                /*if (!colliderScript.GetComponentInParent<GrazeScript>().grazed)
                                {
                                    gameMaster.grazePoints += 1;
                                    gameMaster.score += 500;
                                    AudioSource.PlayClipAtPoint(gameMaster.itemCollectSound, gameMaster.mainCamera.position, GameMasterScript.gameMaster.masterVolume);
                                    colliderScript.GetComponentInParent<GrazeScript>().grazed = true;
                                    ItemDropBase.SpawnItemDrop(ItemDropBase.ItemType.MFOrb, true, Vector2.zero, colliderScript.colliderTransform.position, false);
                                }*/

                                if (distance < Mathf.Abs(/*colliderSize + */colliderSize))
                                {
                                    //HitByBullet();
                                    Debug.Log("Hit by bullet");
                                    BaseBulletScript bulletScript = colliderScript.colliderTransform.GetComponentInParent<BaseBulletScript>();

                                    float multiplier = 1;
                                    for (int k = 0; k < gameMaster.playerParty[partyChar].weaknessesAndResistances.Length; k++)
                                    {
                                        if(gameMaster.playerParty[partyChar].weaknessesAndResistances[k].damageType == bulletScript.damageType)
                                        {
                                            multiplier = gameMaster.playerParty[partyChar].weaknessesAndResistances[k].damageMultiplier;
                                        }                                            
                                    }

                                    gameMaster.playerParty[partyChar].currentHP -= Mathf.CeilToInt(bulletScript.damage * multiplier * ((shmupManager.captureMode) ? 2 : 1));
                                    if (shmupManager.captureMode)
                                        shmupManager.endSTG = true;

                                    if (colliderScript.GetComponentInParent<BaseBulletScript>() != null)
                                        Destroy(colliderScript.GetComponentInParent<BaseBulletScript>().gameObject);
                                    break;
                                }
                            }

                        }
                    }
                    node = node.Next;
                }
            }
        }
    }

    public void Respawn()
    {
        thisTrans.localPosition = shmupManager.spawnPos;
    }

    void Capture()
    {
        if(!takingCapture)
        {
            chargingCapPercent = false;
            if (inputScript.shootPressed && focusing)
            {
                chargingCapPercent = true;
                captureCharge = Mathf.Clamp(captureCharge + (gameMaster.timeScale * ((captureCharge >= 100) ? 0.05f : 0.15f)), 0, 200);
            }
            else
                captureCharge += gameMaster.timeScale * ((captureCharge >= 100) ? 0 : 0.075f);


            if (inputScript.shootDown && !inputScript.focusPressed && captureCharge > 100)
            {
                takingCapture = true;
                lightningArea.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            }

            float nearestDist = float.MaxValue;
            float currentDist;
            Vector2 position = new Vector2(int.MaxValue, 0);
            for (int i = 0; i < shmupManager.enemyContainer.childCount; i++)
            {
                currentDist = Vector2.Distance(lightningArea.localPosition, shmupManager.enemyContainer.GetChild(i).localPosition);
                if (currentDist < nearestDist)
                {
                    nearestDist = currentDist;
                    position = shmupManager.enemyContainer.GetChild(i).localPosition;
                }
            }
            if (position.x != int.MaxValue)
                lightningArea.localPosition = MathFunctions.CalculateCircle(thisTrans.localPosition, Mathf.Clamp(nearestDist, 0, 40), MathFunctions.FindAngle(lightningArea.localPosition, position));
            else
                lightningArea.localPosition = thisTrans.localPosition;
        }
        else
        {
            captureCharge -= 1f;

            lightningArea.localPosition += (Vector3)(inputScript.directionalInput.normalized * 3);
            lightningArea.localScale = Vector3.one * (Mathf.Clamp(captureCharge, 30, 100) / 100);

            gameMaster.timeScale = 0.25f;
            Time.timeScale = 0.25f;

            if(!inputScript.shootPressed)
            {
                float nearestDist = float.MaxValue;
                float currentDist;
                for (int i = 0; i < shmupManager.enemyContainer.childCount; i++)
                {
                    currentDist = Vector2.Distance(lightningArea.position, shmupManager.enemyContainer.GetChild(i).position);
                    if (currentDist < nearestDist)
                        nearestDist = currentDist;
                }

                if (nearestDist < captureRange * (Mathf.Clamp(captureCharge, 30, 100) / 100))
                {
                    shmupManager.UpdateCaptures();
                    captureCharge = Mathf.Clamp(captureCharge - 100, 0, 100);
                }

                lightningArea.localScale = Vector3.one;
                lightningArea.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }

            if (captureCharge <= 30)
            {
                takingCapture = false;
                shmupManager.CaptureFreeze();
                gameMaster.timeScale = 1f;
                Time.timeScale = 1f;
                lightningArea.localScale = Vector3.one;
                lightningArea.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
        }

        percentText.text = Mathf.FloorToInt(captureCharge).ToString("000") + "%";
    }

    void Shoot()
    {
        if (inputScript.shootPressed && !hitByBullet && bombWaitTime <= 0)
        {
            shooting = true;
            burstsFired = 0;
        }

        if(shooting)
        {
            if (mainFireWait <= 0)
            {
                if (!focusing)
                    MainShot();
                else
                    MainFocusShot();
            }
            else
                mainFireWait -= gameMaster.timeScale;

            if (subFireWait <= 0)
            {
                if (!focusing)
                    SubShot();
                else
                    SubFocusShot();
            }
            else
                subFireWait -= gameMaster.timeScale;

            if (burstsFired >= burstsToFire || bombWaitTime > 0 || deadTimeCurrent > 0)
            {
                burstsFired = 0;
                shooting = false;
            }
        }
    }

    void MainShot()
    {
        if (mainShootSound != null)
            AudioSource.PlayClipAtPoint(mainShootSound, thisTrans.position);

        GameObject[] bulletsToFire = shotType.mainShots[mainFired].bullets;
        float[] bulletAngles = shotType.mainShots[mainFired].angles;
        Vector2[] firePosOffset = shotType.mainShots[mainFired].positionOffsets;

        Transform newBullet;
        BaseBulletScript topScript;
        for (int i = 0; i < bulletsToFire.Length; i++)
        {
            newBullet = Instantiate(bulletsToFire[i], thisTrans.position + (Vector3)firePosOffset[i], new Quaternion(), shmupManager.bulletContainer).transform;
            newBullet.eulerAngles = new Vector3(newBullet.eulerAngles.x, newBullet.eulerAngles.y, bulletAngles[i]);

            topScript = newBullet.GetComponent<BaseBulletScript>();
            if (topScript != null)
            {
                topScript.damageType = damageType;
                topScript.damage = Mathf.FloorToInt(topScript.damage * damageMult);
            }
        }
        mainFireWait += shotType.mainShots[mainFired].fireWait;
        mainFired++;
        if (mainFired == shotType.mainShots.Length)
            mainFired = 0;
        burstsFired++;
    }

    void MainFocusShot()
    {
        if (mainShootSound != null)
            AudioSource.PlayClipAtPoint(mainShootSound, thisTrans.position);

        GameObject[] bulletsToFire = shotType.mainFocusShots[mainFocusFired].bullets;
        float[] bulletAngles = shotType.mainFocusShots[mainFocusFired].angles;
        Vector2[] firePosOffset = shotType.mainFocusShots[mainFocusFired].positionOffsets;

        Transform newBullet;
        BaseBulletScript topScript;
        for (int i = 0; i < bulletsToFire.Length; i++)
        {
            newBullet = Instantiate(bulletsToFire[i], thisTrans.position + (Vector3)firePosOffset[i], new Quaternion(), shmupManager.bulletContainer).transform;
            newBullet.eulerAngles = new Vector3(newBullet.eulerAngles.x, newBullet.eulerAngles.y, bulletAngles[i]);

            topScript = newBullet.GetComponent<BaseBulletScript>();
            if(topScript != null)
            {
                topScript.damageType = damageType;
                topScript.damage = Mathf.FloorToInt(topScript.damage * damageMult);
            }
        }
        mainFireWait += shotType.mainFocusShots[mainFocusFired].fireWait;
        mainFocusFired++;
        if (mainFocusFired == shotType.mainFocusShots.Length)
            mainFocusFired = 0;
        burstsFired++;
    }

    void SubShot()
    {
        if(shotType.subShots.Length > 0)
        {
            if (subShootSound != null)
                AudioSource.PlayClipAtPoint(subShootSound, thisTrans.position);


            GameObject[] bulletsToFire = shotType.subShots[subFired].bullets;
            float[] bulletAngles = shotType.subShots[subFired].angles;
            Vector2[] firePosOffset = shotType.subShots[subFired].positionOffsets;

            Transform newBullet;
            BaseBulletScript topScript;
            for (int i = 0; i < bulletsToFire.Length; i++)
            {
                newBullet = Instantiate(bulletsToFire[i], thisTrans.position + (Vector3)firePosOffset[i], new Quaternion(), shmupManager.bulletContainer).transform;
                newBullet.eulerAngles = new Vector3(newBullet.eulerAngles.x, newBullet.eulerAngles.y, bulletAngles[i]);

                topScript = newBullet.GetComponent<BaseBulletScript>();
                if (topScript != null)
                {
                    topScript.damageType = damageType;
                    topScript.damage = Mathf.FloorToInt(topScript.damage * damageMult);
                }
            }
            subFireWait += shotType.subShots[subFired].fireWait;
            subFired++;
            if (subFired == shotType.subShots.Length)
                subFired = 0;
        }
    }

    void SubFocusShot()
    {
        if (shotType.subFocusShots.Length > 0)
        {
            if (subShootSound != null)
                AudioSource.PlayClipAtPoint(subShootSound, thisTrans.position);


            GameObject[] bulletsToFire = shotType.subFocusShots[subFocusFired].bullets;
            float[] bulletAngles = shotType.subFocusShots[subFocusFired].angles;
            Vector2[] firePosOffset = shotType.subFocusShots[subFocusFired].positionOffsets;

            Transform newBullet;
            BaseBulletScript topScript;
            for (int i = 0; i < bulletsToFire.Length; i++)
            {
                newBullet = Instantiate(bulletsToFire[i], thisTrans.position + (Vector3)firePosOffset[i], new Quaternion(), shmupManager.bulletContainer).transform;
                newBullet.eulerAngles = new Vector3(newBullet.eulerAngles.x, newBullet.eulerAngles.y, bulletAngles[i]);

                topScript = newBullet.GetComponent<BaseBulletScript>();
                if (topScript != null)
                {
                    topScript.damageType = damageType;
                    topScript.damage = Mathf.FloorToInt(topScript.damage * damageMult);
                }
            }
            subFireWait += shotType.subFocusShots[subFocusFired].fireWait;
            subFocusFired++;
            if (subFocusFired == shotType.subFocusShots.Length)
                subFocusFired = 0;
        }
    }


    void RaycastCollisions()
    {
        if (movementVelocity.x != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(thisTrans.position, Vector2.right * Mathf.Sign(movementVelocity.x), Mathf.Abs(movementVelocity.x) + shmupManager.skinWidth, shmupManager.collisionMask);
            Debug.DrawRay((Vector2)thisTrans.position + movementVelocity, Vector2.right * Mathf.Sign(movementVelocity.x) * (Mathf.Abs(movementVelocity.x) + shmupManager.skinWidth), Color.green);

            if (hit)
            {
                movementVelocity.x = (hit.distance - shmupManager.skinWidth) * Mathf.Sign(movementVelocity.x);
            }
        }

        if (movementVelocity.y != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(thisTrans.position, Vector2.up * Mathf.Sign(movementVelocity.y), Mathf.Abs(movementVelocity.y) + shmupManager.skinWidth, shmupManager.collisionMask);
            Debug.DrawRay((Vector2)thisTrans.position + movementVelocity, Vector2.up * Mathf.Sign(movementVelocity.y) * (Mathf.Abs(movementVelocity.y) + shmupManager.skinWidth), Color.green);

            if (hit)
            {
                movementVelocity.y = (hit.distance - shmupManager.skinWidth) * Mathf.Sign(movementVelocity.y);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(thisTrans.position, hitboxSize);
    }
}
